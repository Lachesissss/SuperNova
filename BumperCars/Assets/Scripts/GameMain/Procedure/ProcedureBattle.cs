using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.AI;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class ProcedureBattle : ProcedureBase
    {
        private ProcedureOwner procedureOwner;
        private BattleField m_battleField = null;
        
        private BattleModel battleModel;
        private int carAiIndex = 0;
        private int maxSkillPickUpItemsCount = 8;
        private Vector3 skillItemSpawnOffset = new Vector3(0,0.5f,0);
        
        private static Dictionary<string, int> m_playerScoreDict;
        private static Dictionary<string, int> m_playerCarriedScoreDict;
        private BattleUI m_battleUI;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementData m_settlementData;
        private GlobalConfig m_globalConfig;
        
        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            battleModel = BattleModel.Instance;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            m_playerScoreDict = new Dictionary<string, int>();
            m_playerCarriedScoreDict = new Dictionary<string, int>();
            m_playerScoreDict.Add(m_globalConfig.p1Name, 0);
            m_playerScoreDict.Add(m_globalConfig.p2Name, 0);
            m_playerCarriedScoreDict.Add(m_globalConfig.p1Name, 0);
            m_playerCarriedScoreDict.Add(m_globalConfig.p2Name, 0);
        }


        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            Debug.Log("进入主战斗流程");
            
            
            
            //注册事件
            GameEntry.EventManager.AddListener(AttackHitArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.AddListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.AddListener(SwitchCarEventArgs.EventId, OnSwitchCar);
            GameEntry.EventManager.AddListener(GetSkillEventArgs.EventId, OnSkillItemPicked);
            GameEntry.EventManager.AddListener(CarriedScoreUpdateEventArgs.EventId, OnCoinPicked);
            GameEntry.EventManager.AddListener(PlayerArriveHomeEventArgs.EventId, OnPlayerArriveHome);
            GameEntry.EventManager.AddListener(CoinStealEventArgs.EventId, OnCoinStolen);
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementData = null;
            battleModel.spawnPosDict.Clear();
            //关卡相关，生成实例
            if(BattleModel.Instance.currentDungeonMode == DungeonMode.Single)
            {
                SingleModeEnter();
            }
            else
            {
                DoubleModeEnter();
            }

            //测试,创建技能卡
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, Vector3.zero, Quaternion.identity, SkillEnum.Lighting);
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, new Vector3(-2, 0, 2), Quaternion.identity, SkillEnum.Stronger);
            // GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, new Vector3(2, 0, -2), Quaternion.identity, SkillEnum.FlipVertical);
        }
        
        private void SingleModeEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans3.position,m_battleField.spawnTrans3.rotation,CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans5.position,m_battleField.spawnTrans5.rotation, CarComponent.ClothColor.Black);
            var controllerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = m_globalConfig.p1Name, userData = CarPlayer.PlayerType.P1};
            var aiName = $"人机{carAiIndex++}";
            var controllerData2 = new CarController.CarControllerData { carComponent = car2, controllerName = aiName };
            var carPlayer = battleModel.AddPlayer(controllerData1);
            var carAi = battleModel.AddAI(controllerData2);
            
            battleModel.spawnPosDict.Add(carPlayer,  m_battleField.spawnTrans3);
            battleModel.spawnPosDict.Add(carAi,  m_battleField.spawnTrans5);
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = m_globalConfig.p1Name,p2Name = m_globalConfig.p2Name,
                targetScore = m_globalConfig.targetScore, carController1 = carPlayer, carController2 = carAi
            };
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            battleModel.SetBattleCamera(carPlayer, carAi);
        }
        
        private void DoubleModeEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation, CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, CarComponent.ClothColor.Black);
            var car3 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans5.position,m_battleField.spawnTrans5.rotation,CarComponent.ClothColor.Red);
            var car4 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans6.position,m_battleField.spawnTrans6.rotation,CarComponent.ClothColor.Green);
            var carPlayerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = m_globalConfig.p1Name, userData = CarPlayer.PlayerType.P1};
            var carPlayerData2 = new CarController.CarControllerData(){carComponent = car2, controllerName = m_globalConfig.p2Name, userData = CarPlayer.PlayerType.P2};
            var carAiData1 = new CarController.CarControllerData(){carComponent = car3, controllerName = $"人机{carAiIndex++}"};
            var carAiData2 = new CarController.CarControllerData(){carComponent = car4, controllerName = $"人机{carAiIndex++}"};
            
            var carPlayer1 = battleModel.AddPlayer(carPlayerData1);
            var carPlayer2 = battleModel.AddPlayer(carPlayerData2);
            var carAi1 = battleModel.AddAI(carAiData1);
            var carAi2 = battleModel.AddAI(carAiData2);
            
            
            battleModel.spawnPosDict.Add(carPlayer1,  m_battleField.spawnTrans1);
            battleModel.spawnPosDict.Add(carPlayer2,  m_battleField.spawnTrans2);
            battleModel.spawnPosDict.Add(carAi1,  m_battleField.spawnTrans5);
            battleModel.spawnPosDict.Add(carAi2,  m_battleField.spawnTrans6);
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = m_globalConfig.p1Name,p2Name = m_globalConfig.p2Name,
                targetScore = m_globalConfig.targetScore, carController1 = carPlayer1, carController2 = carPlayer2
            };
            m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            battleModel.SetBattleCamera(carPlayer1, carPlayer2);
            
        }
        
        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if(BattleModel.Instance.currentDungeonMode==DungeonMode.Single)
            {
                SingleModeUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            }
            else
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
            if(battleModel.skillPickUpItems.Count<maxSkillPickUpItemsCount)
            {
                SkillEnum skillEnum = GameEntry.SkillManager.GetRandomSkillEnum();
                Vector3 spawnPosition = GetRandomPositionInBattleField();
                battleModel.skillPickUpItems.Add(GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, spawnPosition, Quaternion.identity, skillEnum));
            }
        }
        
        Vector3 RandomNavMeshLocation(float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
        
        Vector3 GetRandomPositionInBattleField()
        {
            int NavMeshLayerBite = 0;
            
            NavMeshLayerBite+= 1 << NavMesh.GetAreaFromName("Walkable");
            Vector3 randomDirection = Random.insideUnitSphere * m_battleField.Radius;
            randomDirection+=m_battleField.fieldCenter.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, m_battleField.Radius, NavMeshLayerBite))
            {
                finalPosition = hit.position;
            }
            return finalPosition+skillItemSpawnOffset;
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
                battleModel.lastAttackInfoDict[args.attackInfo.underAttacker] = args.attackInfo;
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
                    CarCamera.SwapController(battleModel.player1Camera, battleModel.player2Camera);
                    GameEntry.SoundManager.PlayerSound(car1, SoundEnum.SpecialAudio, false);
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
                battleModel.skillPickUpItems.Remove(item);
            }
        }

        private void OnCoinPicked(object sender, GameEventArgs e)
        {
            if (e is CarriedScoreUpdateEventArgs args)
                if (m_playerCarriedScoreDict.ContainsKey(args.playerName))
                {
                    m_playerCarriedScoreDict[args.playerName] += args.getPointNum;
                    GameEntry.EventManager.Invoke(this,
                        CarriedScoreUIUpdateEventArgs.Create(m_playerCarriedScoreDict[m_globalConfig.p1Name], m_playerCarriedScoreDict[m_globalConfig.p2Name]));
                }
        }

        private void OnCoinStolen(object sender, GameEventArgs e)
        {
            if (e is CoinStealEventArgs args)
                if (m_playerCarriedScoreDict.ContainsKey(args.stealerName) && m_playerCarriedScoreDict.ContainsKey(args.underStealerName))
                {
                    var stealNum = m_playerCarriedScoreDict[args.underStealerName] - m_playerCarriedScoreDict[args.underStealerName] / 2;
                    m_playerCarriedScoreDict[args.underStealerName] /= 2;
                    m_playerCarriedScoreDict[args.stealerName] += stealNum;
                    GameEntry.EventManager.Invoke(this,
                        CarriedScoreUIUpdateEventArgs.Create(m_playerCarriedScoreDict[m_globalConfig.p1Name], m_playerCarriedScoreDict[m_globalConfig.p2Name]));
                }
        }

        private void OnPlayerArriveHome(object sender, GameEventArgs e)
        {
            if (e is PlayerArriveHomeEventArgs args)
                if (m_playerScoreDict.ContainsKey(args.playerName))
                {
                    m_playerScoreDict[args.playerName] += m_playerCarriedScoreDict[args.playerName];
                    m_playerCarriedScoreDict[args.playerName] = 0;
                    GameEntry.EventManager.Invoke(this,
                        ScoreUIUpdateEventArgs.Create(m_playerScoreDict[m_globalConfig.p1Name], m_playerScoreDict[m_globalConfig.p2Name]));
                    GameEntry.EventManager.Invoke(this,
                        CarriedScoreUIUpdateEventArgs.Create(m_playerCarriedScoreDict[m_globalConfig.p1Name], m_playerCarriedScoreDict[m_globalConfig.p2Name]));
                }
        }
        
        //由于流程退出的自动回收机制，这里不用根据DungeonMode区分，都回收了
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.instance.StopAllCoroutines();
            foreach (var controller in battleModel.carControllers)
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
            //清理容器
            battleModel.ClearModel();
            
            m_playerScoreDict[m_globalConfig.p1Name] = 0;
            m_playerScoreDict[m_globalConfig.p2Name] = 0;
            m_playerCarriedScoreDict[m_globalConfig.p1Name] = 0;
            m_playerCarriedScoreDict[m_globalConfig.p2Name] = 0;
            GameEntry.EventManager.RemoveListener(AttackHitArgs.EventId, OnAttackHappened);
            GameEntry.EventManager.RemoveListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            GameEntry.EventManager.RemoveListener(SwitchCarEventArgs.EventId, OnSwitchCar);
            GameEntry.EventManager.RemoveListener(GetSkillEventArgs.EventId, OnSkillItemPicked);
            GameEntry.EventManager.RemoveListener(CarriedScoreUpdateEventArgs.EventId, OnCoinPicked);
            GameEntry.EventManager.RemoveListener(PlayerArriveHomeEventArgs.EventId, OnPlayerArriveHome);
            GameEntry.EventManager.RemoveListener(CoinStealEventArgs.EventId, OnCoinStolen);
        }

        protected internal override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        //死亡结算
        private void DieOutSettlement()
        {
            for(int i=0;i<battleModel.carControllers.Count;i++)
            {
                if (i >= battleModel.carControllers.Count) continue;
                if (!battleModel.carControllers[i].IsHasCar) continue;
                if(battleModel.carControllers[i].carComponent.transform.position.y<m_battleField.dieOutTrans.position.y)
                {
                    DeathProcess(battleModel.carControllers[i]);
                    var controlCar = battleModel.carControllers[i].carComponent;
                    var lastColor = controlCar.clothColor;
                    if(controlCar!=null)
                    {
                        battleModel.carControllers[i].ClearCar();
                        if(battleModel.controllerCameraDict.TryGetValue(battleModel.carControllers[i], out var camera))
                        {
                            camera.StopToTracked();
                        }
                        if( battleModel.carControllers[i] is CarPlayer carPlayer)
                        {
                            GameEntry.SoundManager.PlayerSound(battleModel.carControllers[i], SoundEnum.PlayerDead, false, 1, false);
                            if(BattleModel.Instance.currentDungeonMode == DungeonMode.Single)
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToRevivePlayerSingleMode(carPlayer, lastColor));
                            }
                            else
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToRevivePlayerDoubleMode(carPlayer,lastColor));
                            }
                        }
                        else if(battleModel.carControllers[i] is CarAI carAI)
                        {//目前人机立即复活
                            if(BattleModel.Instance.currentDungeonMode == DungeonMode.Single)
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToReviveAISingleMode(carAI, lastColor));
                                //ReviveAISingleMode(carAI,lastColor);
                            }
                            else
                            {
                                GameEntry.instance.GameStartCoroutine(DelayToReviveAIDoubleMode(carAI, lastColor));
                                //ReviveAIDoubleMode(carAI,lastColor);
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
            if (battleModel.lastAttackInfoDict.TryGetValue(carName, out var attackInfo))
            {
                var now = DateTime.Now;
                var span = now - attackInfo.attackTime;
                if (span.TotalSeconds < 3f)
                {
                    //这些后面都要变成tips
                    var killer = attackInfo.attacker;

                    if(attackInfo.attackType==AttackType.Collide)
                    {
                        var showMsg = $"[{carComponent.carControllerName}]在与[{killer}]的激烈碰撞中牺牲了!";
                        Debug.Log(showMsg);
                        GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                    }
                    else if(attackInfo.attackType==AttackType.Skill)
                    {
                        if(attackInfo.userData is SkillEnum skillEnum)
                        {
                            var skillCfg = GameEntry.SkillManager.GetSkillConfigItem(skillEnum);
                            var showMsg = $"[{carName}]被[{killer}]使用[{skillCfg.skillName}]{skillCfg.killText}!";
                            Debug.Log(showMsg);
                            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                        }
                    }
                            
                    if (m_playerScoreDict.ContainsKey(killer))
                    {
                        m_playerScoreDict[killer] += 1; //击杀分数直接加1，并夺取被杀者分数
                        if (m_playerCarriedScoreDict.ContainsKey(attackInfo.underAttacker))
                            m_playerCarriedScoreDict[killer] += m_playerCarriedScoreDict[carController.controllerName];

                        GameEntry.EventManager.Invoke(ScoreUIUpdateEventArgs.EventId,
                            ScoreUIUpdateEventArgs.Create(m_playerScoreDict[m_globalConfig.p1Name], m_playerScoreDict[m_globalConfig.p2Name]));
                    }
                    
                    if(battleModel.killOtherPlayerNumDict.ContainsKey(killer)&&battleModel.killOtherPlayerNumDict.ContainsKey(attackInfo.underAttacker))
                    {
                        if(battleModel.killOtherPlayerNumDict[killer]<3)
                        {
                            battleModel.killOtherPlayerNumDict[killer]++;
                            GameEntry.EventManager.Invoke(UltimateSkillUIUpdateArgs.EventId, UltimateSkillUIUpdateArgs.Create());
                        }
                    }
                }
                else
                {
                    var showMsg = $"[{carName}]Ta自杀了...";
                    Debug.Log(showMsg);
                    GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                }
                battleModel.lastAttackInfoDict.Remove(carName);
            }
            else
            {
                var showMsg = $"[{carName}]Ta自杀了...";
                Debug.Log(showMsg);
                GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
            }

            if (m_playerCarriedScoreDict.ContainsKey(carController.controllerName))
            {
                m_playerCarriedScoreDict[carController.controllerName] = 0;
                GameEntry.EventManager.Invoke(this,
                    CarriedScoreUIUpdateEventArgs.Create(m_playerCarriedScoreDict[m_globalConfig.p1Name], m_playerCarriedScoreDict[m_globalConfig.p2Name]));
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
            var spawnTrans = battleModel.spawnPosDict[carPlayer];
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, spawnTrans.position,spawnTrans.rotation,lastColor);
            battleModel.player1Camera.ReSetTrackedTarget(car);
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
                var spawnTrans = battleModel.spawnPosDict[carPlayer];
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, spawnTrans.position,spawnTrans.rotation,lastColor);
                battleModel.player1Camera.ReSetTrackedTarget(car);
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
                battleModel.player2Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
            }
        }

        private IEnumerator DelayToReviveAIDoubleMode(CarAI carAi, CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.aiReviveTime);
            ReviveAIDoubleMode(carAi, lastColor);
        }
        
        private IEnumerator DelayToReviveAISingleMode(CarAI carAi, CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.aiReviveTime);
            ReviveAISingleMode(carAi, lastColor);
        }
        
        private void ReviveAISingleMode(CarAI carAI,CarComponent.ClothColor lastColor)
        {
            var spawnTrans = battleModel.spawnPosDict[carAI];
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,spawnTrans.position,spawnTrans.rotation, lastColor);
            carAI.SetCar(car);
            carAI.ResetCarAIState();
            carAI.ClearSkills();
            battleModel.player2Camera.ReSetTrackedTarget(car);
        }
        
        private void ReviveAIDoubleMode(CarAI carAI, CarComponent.ClothColor lastColor)
        {
            var spawnTrans = battleModel.spawnPosDict[carAI];
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,spawnTrans.position,spawnTrans.rotation, lastColor);
            carAI.SetCar(car);
            carAI.ResetCarAIState();
            carAI.ClearSkills();
        }
        
        private bool TryGetSwitchControllers(out CarController car1, out CarController car2)
        {
            car1 = null;
            car2 = null;
            bool get1 = false;
            bool get2 = false;
            foreach (var controller in battleModel.carControllers)
            {
                if(controller.carComponent==null) continue;
                if(controller.carComponent==battleModel.player1Camera.car)
                {
                    car1 = controller;
                    get1 = true;
                }
                else if(controller.carComponent==battleModel.player2Camera.car)
                {
                    car2 = controller;
                    get2 = true;
                }
            }

            // if (car2 != null && car1 != null && Vector3.Distance(car1.carComponent.transform.position, car2.carComponent.transform.position) >
            //     m_globalConfig.switchMinDistance)
            // {
            //     var showMsg = $"无法切换, 需要与对方距离小于{m_globalConfig.switchMinDistance}米";
            //     GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
            //     Debug.Log(showMsg);
            //     return false;
            // }
            
            return get1&&get2;
        }
    }
}