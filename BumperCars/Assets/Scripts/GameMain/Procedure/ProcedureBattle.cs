using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using Lachesis.Gameplay;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;

namespace Lachesis.GamePlay
{
    public class ProcedureBattle : ProcedureBase
    {
        //private bool changeScene;
        private ProcedureOwner procedureOwner;
        private GameObject canvarRoot;
        private BattleField battleField = null;
        private List<CarAI> carEnemies = new();
        private List<Player> carPlayers = new();
        private string p1Name = "玩家1";
        private string p2Name = "玩家2";
        Dictionary<string, int> m_playerScoreDict = new();
        private int carAiIndex = 0;
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            canvarRoot = GameEntry.instance.canvasRoot;
            carEnemies.Clear();
            carPlayers.Clear();
            m_playerScoreDict.Clear();
            m_playerScoreDict.Add(p1Name, 0);
            m_playerScoreDict.Add(p2Name, 0);
            
            Debug.Log("进入主战斗流程");
            battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, targetScore = GameEntry.ConfigManager.GetConfig<GlobalConfig>().targetScore};
            GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            var carPlayer = GameEntry.EntityManager.CreateEntity<Player>(EntityEnum.CarPlayer, battleField.spawnTrans1.position,battleField.spawnTrans1.rotation, p1Name);
            carPlayers.Add(carPlayer);
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarEnemy,battleField.spawnTrans2.position,battleField.spawnTrans2.rotation, $"人机{carAiIndex++}");
            carEnemies.Add(carAi);
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            for(int i=0;i<carEnemies.Count;i++)
            {
                if(i>=carEnemies.Count) break;
                if(carEnemies[i].transform.position.y<battleField.dieOutTrans.position.y)
                {
                    var now = DateTime.Now;
                    var span = now - carEnemies[i].carController.lastAttackedTime;
                    if(span.TotalSeconds<3f)
                    {
                        var killer = carEnemies[i].carController.lastAttackerName;
                        Debug.Log($"{carEnemies[i].carController.name} 在与{killer}的激烈碰撞中牺牲了！");
                        if(m_playerScoreDict.ContainsKey(killer))
                        {
                            m_playerScoreDict[killer]++;
                            GameEntry.EventManager.Fire(ScoreUpdateEventArgs.EventId, ScoreUpdateEventArgs.Create( m_playerScoreDict[p1Name], m_playerScoreDict[p2Name]));
                        }
                    }
                    else
                    {
                        Debug.Log($"{carEnemies[i].carController.name} 自杀了！");
                    }
                    GameEntry.EntityManager.ReturnEntity<CarAI>(EntityEnum.CarEnemy, carEnemies[i].gameObject);
                    carEnemies.RemoveAt(i);
                    i--;
                }
            }

            for(int i=0;i<carPlayers.Count;i++)
            {
                if(i>=carPlayers.Count) break;
                if(carPlayers[i].transform.position.y<battleField.dieOutTrans.position.y)
                {
                    var now = DateTime.Now;
                    var span = now - carPlayers[i].carController.lastAttackedTime;
                    if(span.TotalSeconds<3f)
                    {
                        //这些后面都要变成tips
                        var killer = carPlayers[i].carController.lastAttackerName;
                        
                        Debug.Log($"{carPlayers[i].carController.name} 在与{killer}的激烈碰撞中牺牲了！");
                        if(m_playerScoreDict.ContainsKey(killer))
                        {
                            m_playerScoreDict[killer]++;
                            GameEntry.EventManager.Fire(ScoreUpdateEventArgs.EventId, ScoreUpdateEventArgs.Create( m_playerScoreDict[p1Name], m_playerScoreDict[p2Name]));
                        }
                        
                    }
                    else
                    {
                        Debug.Log($"{carPlayers[i].carController.name} 自杀了！");
                    }
                    
                    GameEntry.EntityManager.ReturnEntity<Player>(EntityEnum.CarPlayer, carPlayers[i].gameObject);
                    GameEntry.instance.GameStartCoroutine(DelayToRevive());
                    carPlayers.RemoveAt(i);
                    i--;
                }
            }
        }

        private IEnumerator DelayToRevive()
        {
            yield return new WaitForSeconds(GameEntry.ConfigManager.GetConfig<GlobalConfig>().playerReviveTime);
            var carPlayer = GameEntry.EntityManager.CreateEntity<Player>(EntityEnum.CarPlayer, battleField.spawnTrans1.position,battleField.spawnTrans1.rotation);
            carPlayers.Add(carPlayer);
        }
        
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }
    }
}