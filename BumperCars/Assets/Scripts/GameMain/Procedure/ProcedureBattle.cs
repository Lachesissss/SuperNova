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
        public static List<CarController> carControllers;
        public static List<Entity> userEntities; //其他实体再创建的实体先放在这里，后面考虑把ProcedureBattle里的数据移到一个BattleManager里
        private BattleUI m_battleUI;
        private string p1Name;
        private string p2Name;
        private int carAiIndex = 0;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementData m_settlementData;
        private CarCamera player1Camera;
        private CarCamera player2Camera;
        private GlobalConfig m_globalConfig;
        
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            lastAttackInfoDict = new Dictionary<string, AttackInfo>();
            carControllers = new List<CarController>();
            m_playerScoreDict = new Dictionary<string, int>();
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            p1Name = m_globalConfig.p1Name;
            p2Name = m_globalConfig.p2Name;
            m_playerScoreDict.Add(p1Name, 0);
            m_playerScoreDict.Add(p2Name, 0);
            GameEntry.EventManager.Subscribe(AttackEventArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.Subscribe(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.Subscribe(SwitchCarEventArgs.EventId, OnSwitchCar);
        }


        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementData = null;
            Debug.Log("进入主战斗流程");
            //这里后面的逻辑最后要封装成关卡，兼容单人多人关卡，最好可以配置
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation);
            var controllerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = p1Name};
            var controllerData2 = new CarController.CarControllerData(){carComponent = car2, controllerName = $"人机{carAiIndex++}"};
            var carPlayer = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, controllerData1);
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI,Vector3.zero, Quaternion.identity,controllerData2);
            carControllers.Add(carPlayer);
            carControllers.Add(carAi);
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, 
                targetScore = m_globalConfig.targetScore, carPlayer = carPlayer};
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            player1Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player1Camera, Vector3.zero, Quaternion.identity, carPlayer.carComponent);
            player1Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player2Camera, Vector3.zero, Quaternion.identity, carAi.carComponent);
            //测试,创建技能卡
            GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, Vector3.zero, Quaternion.identity, SkillEnum.Lighting);
            GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, new Vector3(-2, 0, 2), Quaternion.identity, SkillEnum.Stronger);
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
            //SpawnAI();
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
        // private void SpawnAI()
        // { //暂时是，始终保持场上有1个AI
        //     int aiNum = 0;
        //     foreach (var controller in carControllers)
        //     {
        //         if(controller.GetType()==typeof(CarAI))
        //             aiNum++;
        //     }
        //     
        //     if(aiNum==0)
        //     {
        //         var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, $"人机{carAiIndex++}");
        //         carControllers.Add(carAi);
        //     }
        // }

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
        
        
        private void OnSwitchCar(object sender, GameEventArgs e)
        {   //这里就先简单交换玩家和人机
            if(e is SwitchCarEventArgs args)
            {
                if(carControllers.Count>=2)
                {
                    CarController.SwitchCar(carControllers[0],carControllers[1]);
                    CarCamera.SwapController(player1Camera, player2Camera);
                }
            }
        }
        
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.instance.StopAllCoroutines();
            foreach (var controller in carControllers)
            {
                if(controller is CarPlayer carPlayer)
                {
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.CarPlayer, carPlayer);
                }
                else if(controller is CarAI carAI)
                {
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.CarAI, carAI);
                }
                
            }
            // foreach (var player in carPlayers)
            // {
            //     GameEntry.EntityManager.ReturnEntity(EntityEnum.CarPlayer, player);
            // }
            //
            // GameEntry.instance.StopAllCoroutines();
            // GameEntry.EntityManager.ReturnEntity(EntityEnum.BattleUI, m_battleUI);
            // GameEntry.EntityManager.ReturnEntity(EntityEnum.BattleField, m_battleField);
            m_battleField = null;
            m_battleUI = null;
            lastAttackInfoDict.Clear();
            carControllers.Clear();
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
            for(int i=0;i<carControllers.Count;i++)
            {
                if(i>=carControllers.Count) break;
                if(!carControllers[i].IsHasCar) break;
                if(carControllers[i].carComponent.transform.position.y<m_battleField.dieOutTrans.position.y)
                {
                    DeathProcess(carControllers[i]);
                    var controlCar = carControllers[i].carComponent;
                    if(controlCar!=null)
                    {
                        carControllers[i].ClearCar();
                        if( carControllers[i] is CarPlayer carPlayer)
                        {
                            GameEntry.instance.GameStartCoroutine(DelayToRevive(carPlayer));
                        }
                        else if(carControllers[i] is CarAI carAI)
                        {//目前人机立即复活
                            Revive(carAI);
                        }
                    }
                }
            }
        }
        
        private void DeathProcess(CarController carController)
        {
            var carComponent =carController.carComponent;
            var carName = carComponent.carControllerName;
            if (lastAttackInfoDict.TryGetValue(carName, out var attackInfo))
            {
                var now = DateTime.Now;
                var span = now - attackInfo.attackTime;
                if (span.TotalSeconds < 3f)
                {
                    //这些后面都要变成tips
                    var killer = attackInfo.attacker;

                    if(attackInfo.attackType==AttackType.Collide)
                    {
                        Debug.Log($"{carComponent.carControllerName} 在与{killer}的激烈碰撞中牺牲了！");
                    }
                    else if(attackInfo.attackType==AttackType.Skill)
                    {
                        if(attackInfo.userData is SkillEnum skillEnum)
                        {
                            var skillCfg = GameEntry.SkillManager.GetSkillConfigItem(skillEnum);
                            Debug.Log($"{carName} 被{killer}使用[{skillCfg.skillName}]{skillCfg.killText}！");
                        }
                    }
                            
                    if (m_playerScoreDict.ContainsKey(killer))
                    {
                        m_playerScoreDict[killer]+=5;
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
        }
        
        private void WinSettlement()
        {
            foreach (var kv in m_playerScoreDict)
            {
                if(kv.Value >= m_globalConfig.targetScore)
                {
                    var data =  new SettlementData();
                    data.winner = kv.Key;
                    isGoSettlement = true;
                    m_settlementData = data;
                    return;
                }
            }
        }
        
        private IEnumerator DelayToRevive(CarPlayer carPlayer)
        {
            yield return new WaitForSeconds(m_globalConfig.playerReviveTime);
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation);
            carPlayer.carComponent = car;
            carPlayer.ClearSkills();
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, 
                targetScore = m_globalConfig.targetScore, carPlayer = carPlayer};
            m_battleUI.RefreshAll(battleUIData);
        }
        
        private void Revive(CarAI carAI)
        {
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, $"人机{carAiIndex++}");
            carAI.carComponent = car;
        }
        
        
    }
}