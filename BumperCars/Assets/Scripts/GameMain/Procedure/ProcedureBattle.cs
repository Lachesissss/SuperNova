using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class ProcedureBattle : ProcedureBase
    {
        //private bool changeScene;
        private ProcedureOwner procedureOwner;
        private BattleField m_battleField = null;

        private static Dictionary<string, AttackInfo> lastAttackInfoDict; //key：被攻击的carName，value：最近一次攻击信息
        private static Dictionary<string, int> m_playerScoreDict;
        private static Dictionary< CarController,CarCamera> m_ControllerCameraDict;
        public static List<CarController> carControllers;
        private BattleUI m_battleUI;
        private string p1Name;
        private string p2Name;
        private int carAiIndex = 0;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementData m_settlementData;
        public static CarCamera player1Camera;
        public static CarCamera player2Camera;
        private GlobalConfig m_globalConfig;
        private List<SkillPickUpItem> m_skillPickUpItems;
        private int maxSkillPickUpItemsCount = 8;
        private DungeonMode dungeonMode;
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            m_playerScoreDict = new Dictionary<string, int>();
            m_ControllerCameraDict = new ();
            p1Name = m_globalConfig.p1Name;
            p2Name = m_globalConfig.p2Name;
            m_playerScoreDict.Add(p1Name, 0);
            m_playerScoreDict.Add(p2Name, 0);
        }


        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            Debug.Log("进入主战斗流程");
            
            //初始化容器
            lastAttackInfoDict = new Dictionary<string, AttackInfo>();
            carControllers = new List<CarController>();
            m_skillPickUpItems = new List<SkillPickUpItem>();
            
            //注册事件
            GameEntry.EventManager.AddListener(AttackHitArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.AddListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.AddListener(SwitchCarEventArgs.EventId, OnSwitchCar);
            GameEntry.EventManager.AddListener(GetSkillEventArgs.EventId, OnSkillItemPicked);
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementData = null;
            
            //关卡相关，生成实例
            if(this.userData is DungeonMode mode)
            {
                dungeonMode = mode;
                if(dungeonMode == DungeonMode.Single)
                {
                    SingleModeEnter();
                }
                else if(dungeonMode == DungeonMode.Double)
                {
                    DoubleModeEnter();
                }
            }
            //测试,创建技能卡
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, Vector3.zero, Quaternion.identity, SkillEnum.Lighting);
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, new Vector3(-2, 0, 2), Quaternion.identity, SkillEnum.Stronger);
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, new Vector3(2, 0, -2), Quaternion.identity, SkillEnum.FlipVertical);
        }
        
        private void SingleModeEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation,CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, CarComponent.ClothColor.Black);
            var controllerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = p1Name, userData = CarPlayer.PlayerType.P1};
            var aiName = $"人机{carAiIndex++}";
            var controllerData2 = new CarController.CarControllerData { carComponent = car2, controllerName = aiName };
            var carPlayer = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, controllerData1);
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI,Vector3.zero, Quaternion.identity,controllerData2);
            carControllers.Add(carPlayer);
            carControllers.Add(carAi);
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name,
                targetScore = m_globalConfig.targetScore, carController1 = carPlayer, carController2 = carAi
            };
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            player1Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player1Camera, Vector3.zero, Quaternion.identity, carPlayer.carComponent);
            player2Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player2Camera, Vector3.zero, Quaternion.identity, carAi.carComponent);
            m_ControllerCameraDict.Add(carPlayer,player1Camera);
            m_ControllerCameraDict.Add(carAi,player2Camera);
        }
        
        private void DoubleModeEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation, CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, CarComponent.ClothColor.Black);
            var car3 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,Vector3.zero,Quaternion.identity,CarComponent.ClothColor.Red);
            var carPlayerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = p1Name, userData = CarPlayer.PlayerType.P1};
            var carPlayerData2 = new CarController.CarControllerData(){carComponent = car2, controllerName = p2Name, userData = CarPlayer.PlayerType.P2};
            var carAiData = new CarController.CarControllerData(){carComponent = car3, controllerName = $"人机{carAiIndex++}"};
            var carPlayer1 = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, carPlayerData1);
            var carPlayer2 = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, carPlayerData2);
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI,Vector3.zero, Quaternion.identity,carAiData);
            carControllers.Add(carPlayer1);
            carControllers.Add(carPlayer2);
            carControllers.Add(carAi);
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name,
                targetScore = m_globalConfig.targetScore, carController1 = carPlayer1, carController2 = carPlayer2
            };
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            player1Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player1Camera, Vector3.zero, Quaternion.identity, carPlayer1.carComponent);
            player2Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player2Camera, Vector3.zero, Quaternion.identity, carPlayer2.carComponent);
            m_ControllerCameraDict.Add(carPlayer1,player1Camera);
            m_ControllerCameraDict.Add(carPlayer2,player2Camera);
            
        }
        
        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if(dungeonMode==DungeonMode.Single)
            {
                SingleModeUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            }
            else if(dungeonMode == DungeonMode.Double)
            {
                DoubleModeUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            }
            
        }
        
        private void SingleModeUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if(CheckStateChange())
            {
                return;
            }
            DieOutSettlement();
            WinSettlement();
            SpawnSkillPickUpItem();
        }
        
        private void DoubleModeUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if(CheckStateChange())
            {
                return;
            }
            DieOutSettlement();
            WinSettlement();
            SpawnSkillPickUpItem();
        }
        
        /// <summary>
        /// 随机生成可拾取技能
        /// </summary>
        private void SpawnSkillPickUpItem()
        {
            if(m_skillPickUpItems.Count<maxSkillPickUpItemsCount)
            {
                SkillEnum skillEnum = (SkillEnum)Random.Range(0, Enum.GetValues(typeof(SkillEnum)).Length);
                Vector3 spawnPosition = GetRandomPositionInBattleField();
                m_skillPickUpItems.Add(GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, spawnPosition, Quaternion.identity, skillEnum));
            }
        }
        
        Vector3 GetRandomPositionInBattleField()
        {
            float angle = Random.Range(0f, Mathf.PI * 2);
            float distance = Random.Range(0f, m_battleField.Radius);
            Vector3 position = new Vector3(
                m_battleField.fieldCenter.position.x + distance * Mathf.Cos(angle),
                0,
                m_battleField.fieldCenter.position.z + distance * Mathf.Sin(angle)
            );
            return position;
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
            if (e is AttackHitArgs args)
            {
                lastAttackInfoDict[args.attackInfo.underAttacker] = args.attackInfo;
            } 
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
                if(TryGetSwitchControllers(out var car1, out var car2))
                {
                    CarController.SwitchCar(car1,car2);
                    CarCamera.SwapController(player1Camera, player2Camera);
                }
                else
                {
                    Debug.Log("没有找到可切换的目标，无法切换");
                }
            }
        }
        
        //技能被捡，生成新的技能
        private void OnSkillItemPicked(object sender, GameEventArgs e)
        {   
            if(sender is SkillPickUpItem item)
            {
                m_skillPickUpItems.Remove(item);
            }
        }
        
        //由于流程退出的自动回收机制，这里不用根据DungeonMode区分，都回收了
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
            
            m_battleField = null;
            m_battleUI = null;
            lastAttackInfoDict.Clear();
            carControllers.Clear();
            m_skillPickUpItems.Clear();
            m_ControllerCameraDict.Clear();
            m_playerScoreDict[p1Name] = 0;
            m_playerScoreDict[p2Name] = 0;
            GameEntry.EventManager.RemoveListener(AttackHitArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.RemoveListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.RemoveListener(SwitchCarEventArgs.EventId, OnSwitchCar);
            GameEntry.EventManager.RemoveListener(GetSkillEventArgs.EventId, OnSkillItemPicked);
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
                    var lastColor = controlCar.clothColor;
                    if(controlCar!=null)
                    {
                        carControllers[i].ClearCar();
                        if(m_ControllerCameraDict.TryGetValue(carControllers[i], out var camera))
                        {
                            camera.StopToTracked();
                        }
                        if( carControllers[i] is CarPlayer carPlayer)
                        {
                            if(dungeonMode == DungeonMode.Single)
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToRevivePlayerSingleMode(carPlayer, lastColor));
                            }
                            else
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToRevivePlayerDoubleMode(carPlayer,lastColor));
                            }
                        }
                        else if(carControllers[i] is CarAI carAI)
                        {//目前人机立即复活
                            if(dungeonMode == DungeonMode.Single)
                            {
                                ReviveAISingleMode(carAI,lastColor);
                            }
                            else
                            {
                                ReviveAIDoubleMode(carAI,lastColor);
                            }
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
                        BattleUI.ShowPopupTips($"{carComponent.carControllerName} 在与{killer}的激烈碰撞中牺牲了！");
                    }
                    else if(attackInfo.attackType==AttackType.Skill)
                    {
                        if(attackInfo.userData is SkillEnum skillEnum)
                        {
                            var skillCfg = GameEntry.SkillManager.GetSkillConfigItem(skillEnum);
                            Debug.Log($"[{carName}]被{killer}使用[{skillCfg.skillName}]{skillCfg.killText}！");
                            BattleUI.ShowPopupTips($"[{carName}]被{killer}使用[{skillCfg.skillName}]{skillCfg.killText}!");
                        }
                    }
                            
                    if (m_playerScoreDict.ContainsKey(killer))
                    {
                        m_playerScoreDict[killer] += 1;
                        GameEntry.EventManager.Invoke(ScoreUpdateEventArgs.EventId,
                            ScoreUpdateEventArgs.Create(m_playerScoreDict[p1Name], m_playerScoreDict[p2Name]));
                    }
                }
                else
                {
                    Debug.Log($"[{carName}]Ta自杀了...");
                    BattleUI.ShowPopupTips($"[{carName}]Ta自杀了...");
                }
            }
            else
            {
                Debug.Log($"[{carName}]Ta自杀了...");
                BattleUI.ShowPopupTips($"[{carName}]Ta自杀了...");
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
        
        private IEnumerator DelayToRevivePlayerSingleMode(CarPlayer carPlayer,CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.playerReviveTime);
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation,lastColor);
            player1Camera.ReSetTrackedTarget(car);
            carPlayer.SetCar(car);
            carPlayer.ClearSkills();
            // var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, 
            //     targetScore = m_globalConfig.targetScore, carPlayer = carPlayer};
            // m_battleUI.RefreshAll(battleUIData);
            m_battleUI.RefreshSkillSlotsUI();
        }
        
        private IEnumerator DelayToRevivePlayerDoubleMode(CarPlayer carPlayer,CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.playerReviveTime);
            if(carPlayer.playerType == CarPlayer.PlayerType.P1)
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation,lastColor);
                player1Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
                
                //目前只有P1有UI
                // var battleUIData =new BattleUI.BattleUIData(){p1Name = this.p1Name,p2Name = this.p2Name, 
                //     targetScore = m_globalConfig.targetScore, carPlayer = carPlayer};
                // m_battleUI.RefreshAll(battleUIData);
                m_battleUI.RefreshSkillSlotsUI();
            }
            else
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation,lastColor);
                player2Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
            }
        }
        
        private void ReviveAISingleMode(CarAI carAI,CarComponent.ClothColor lastColor)
        {
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, lastColor);
            carAI.SetCar(car);
            carAI.ClearSkills();
            player2Camera.ReSetTrackedTarget(car);
        }
        
        private void ReviveAIDoubleMode(CarAI carAI, CarComponent.ClothColor lastColor)
        {
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,Vector3.zero,Quaternion.identity, lastColor);
            carAI.SetCar(car);
            carAI.ClearSkills();
        }
        
        private bool TryGetSwitchControllers(out CarController car1, out CarController car2)
        {
            car1 = null;
            car2 = null;
            bool get1 = false;
            bool get2 = false;
            foreach (var controller in carControllers)
            {
                if(controller.carComponent==null) continue;
                if(controller.carComponent==player1Camera.car)
                {
                    car1 = controller;
                    get1 = true;
                }
                else if(controller.carComponent==player2Camera.car)
                {
                    car2 = controller;
                    get2 = true;
                }
            }
            
            return get1&&get2;
        }
    }
}