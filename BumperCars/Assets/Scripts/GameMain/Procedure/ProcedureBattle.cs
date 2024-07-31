using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class ProcedureBattle : ProcedureBase
    {
        //private bool changeScene;
        private ProcedureOwner procedureOwner;
        private BattleField m_battleField = null;

        private static Dictionary<string, AttackInfo> lastAttackInfoDict; //key：被攻击的carName，value：最近一次攻击信息
        private static Dictionary<string, int> m_playerScoreDict;
        private static List<CarAI> carEnemies;
        private static List<Player> carPlayers;
        private BattleUI m_battleUI;
        private string p1Name;
        private string p2Name;
        private int carAiIndex = 0;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementData m_settlementData;
        
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            lastAttackInfoDict = new Dictionary<string, AttackInfo>();
            carEnemies = new List<CarAI>();
            carPlayers = new List<Player>();
            m_playerScoreDict = new Dictionary<string, int>();
            p1Name = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p1Name;
            p2Name = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2Name;
            m_playerScoreDict.Add(p1Name, 0);
            m_playerScoreDict.Add(p2Name, 0);
            GameEntry.EventManager.Subscribe(AttackEventArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.Subscribe(ProcedureChangeEventArgs.EventId, OnProcedureChange);
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementData = null;
            Debug.Log("进入主战斗流程");
            //创建实体
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, targetScore = GameEntry.ConfigManager.GetConfig<GlobalConfig>().targetScore};
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            var carPlayer = GameEntry.EntityManager.CreateEntity<Player>(EntityEnum.CarPlayer, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation, p1Name);
            carPlayers.Add(carPlayer);
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarEnemy,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, $"人机{carAiIndex++}");
            carEnemies.Add(carAi);
            
            //测试
            GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, Vector3.zero, Quaternion.identity, SkillEnum.Smash);
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if(CheckStateChange())
            {
                return;
            }
            DieOutSettlement();
            WinSettlement();
            SpawnAI();
        }
        
        private bool CheckStateChange()
        {
            if(isGoMenu)
            {
                ChangeState<ProcedureMenu>(procedureOwner);
                return true;
            }
            if(isGoSettlement)
            {
                ChangeState<ProcedureWinSettlement>(procedureOwner, m_settlementData);
                return true;
            }
            return false;
        }
        private void SpawnAI()
        { //暂时是，始终保持场上有1个AI
            if(carEnemies.Count==0)
            {
                var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarEnemy,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, $"人机{carAiIndex++}");
                carEnemies.Add(carAi);
            }
        }

        private void OnAttackHappened(object sender, GameEventArgs e)
        {
            if (e is AttackEventArgs args) lastAttackInfoDict[args.attackInfo.underAttacker] = args.attackInfo;
        }

        private void OnProcedureChange(object sender, GameEventArgs e)
        {
            var ne = (ProcedureChangeEventArgs)e;
            if (e is ProcedureChangeEventArgs args)
            {
                var targetProcedure = GameEntry.ProcedureManager.GetProcedure(args.TargetProcedureType);
                if (targetProcedure is ProcedureMenu) isGoMenu = true;
            }
        }
        
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            foreach (var ai in carEnemies)
            {
                GameEntry.EntityManager.ReturnEntity<CarAI>(EntityEnum.CarEnemy, ai.gameObject);
            }
            foreach (var player in carPlayers)
            {
                GameEntry.EntityManager.ReturnEntity<Player>(EntityEnum.CarPlayer, player.gameObject);
            }
            GameEntry.EntityManager.ReturnEntity<BattleUI>(EntityEnum.BattleUI, m_battleUI.gameObject);
            GameEntry.EntityManager.ReturnEntity<BattleField>(EntityEnum.BattleField, m_battleField.gameObject);
            m_battleField = null;
            m_battleUI = null;
            lastAttackInfoDict.Clear();
            carEnemies.Clear();
            carPlayers.Clear();
            m_playerScoreDict[p1Name] = 0;
            m_playerScoreDict[p2Name] = 0;
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        //死亡结算
        private void DieOutSettlement()
        {
            for(int i=0;i<carEnemies.Count;i++)
            {
                if(i>=carEnemies.Count) break;
                if(carEnemies[i].transform.position.y<m_battleField.dieOutTrans.position.y)
                {
                    var carName = carEnemies[i].carController.carName;
                    if (lastAttackInfoDict.TryGetValue(carName, out var attackInfo))
                    {
                        //没有被攻击记录或已有记录在3秒之前，则判定为自杀
                        var now = DateTime.Now;
                        var span = now - attackInfo.attackTime;
                        if (span.TotalSeconds < 3f)
                        {
                            var killer = attackInfo.attacker;
                            Debug.Log($"{carName} 在与{killer}的激烈碰撞中牺牲了！");
                            if (m_playerScoreDict.ContainsKey(killer))
                            {
                                m_playerScoreDict[killer]+=5;
                                GameEntry.EventManager.Fire(this, ScoreUpdateEventArgs.Create(m_playerScoreDict[p1Name], m_playerScoreDict[p2Name]));
                            }
                        }
                        else
                        {
                            Debug.Log($"{carName} Ta自杀了...");
                        }
                    }
                    else
                    {
                        Debug.Log($"{carName} Ta自杀了...");
                    }
                    GameEntry.EntityManager.ReturnEntity<CarAI>(EntityEnum.CarEnemy, carEnemies[i].gameObject);
                    carEnemies.RemoveAt(i);
                    i--;
                }
            }

            for(int i=0;i<carPlayers.Count;i++)
            {
                if(i>=carPlayers.Count) break;
                if(carPlayers[i].transform.position.y<m_battleField.dieOutTrans.position.y)
                {
                    var carName = carPlayers[i].carController.carName;
                    if (lastAttackInfoDict.TryGetValue(carName, out var attackInfo))
                    {
                        var now = DateTime.Now;
                        var span = now - attackInfo.attackTime;
                        if (span.TotalSeconds < 3f)
                        {
                            //这些后面都要变成tips
                            var killer = attackInfo.attacker;

                            Debug.Log($"{carPlayers[i].carController.carName} 在与{killer}的激烈碰撞中牺牲了！");
                            if (m_playerScoreDict.ContainsKey(killer))
                            {
                                m_playerScoreDict[killer]++;
                                GameEntry.EventManager.Fire(ScoreUpdateEventArgs.EventId,
                                    ScoreUpdateEventArgs.Create(m_playerScoreDict[p1Name], m_playerScoreDict[p2Name]));
                            }
                        }
                        else
                        {
                            Debug.Log($"{carName} Ta自杀了...");
                        }
                    }
                    else
                    {
                        Debug.Log($"{carName} Ta自杀了...");
                    }
                    
                    
                    GameEntry.EntityManager.ReturnEntity<Player>(EntityEnum.CarPlayer, carPlayers[i].gameObject);
                    GameEntry.instance.GameStartCoroutine(DelayToRevive());
                    carPlayers.RemoveAt(i);
                    i--;
                }
            }
        }
        
        private void WinSettlement()
        {
            foreach (var kv in m_playerScoreDict)
            {
                if(kv.Value >= GameEntry.ConfigManager.GetConfig<GlobalConfig>().targetScore)
                {
                    var data =  new SettlementData();
                    data.winner = kv.Key;
                    isGoSettlement = true;
                    m_settlementData = data;
                    return;
                }
            }
        }
        
        private IEnumerator DelayToRevive()
        {
            yield return new WaitForSeconds(GameEntry.ConfigManager.GetConfig<GlobalConfig>().playerReviveTime);
            var carPlayer = GameEntry.EntityManager.CreateEntity<Player>(EntityEnum.CarPlayer, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation);
            carPlayers.Add(carPlayer);
        }
        
        
    }
}