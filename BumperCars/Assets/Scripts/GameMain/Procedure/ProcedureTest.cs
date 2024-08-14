using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Lachesis.Core;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class ProcedureTest : ProcedureBase
    {
        private ProcedureOwner procedureOwner;
        private TestField m_battleField = null;
        
        private BattleModel battleModel;
        private int maxSkillPickUpItemsCount = 8;
        private int carAiIndex = 0;
        
        private BattleUI m_battlePveUI;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementPveData m_settlementPveData;
        private GlobalConfig m_globalConfig;

        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            battleModel = BattleModel.Instance;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
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
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementPveData = null;
            
            
            TestEnter();
        }
        
        
        private void TestEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<TestField>(EntityEnum.TestField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,  new Vector3(5,0,10), Quaternion.identity, CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, new Vector3(5,0,-10), Quaternion.identity, CarComponent.ClothColor.Black);
            var car3 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, new Vector3(-5,0,10), Quaternion.identity, CarComponent.ClothColor.Red);
            var car4 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, new Vector3(-5,0,-10), Quaternion.identity, CarComponent.ClothColor.Green);
            var carAiData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = $"人机{carAiIndex++}"};
            var carAiData2 = new CarController.CarControllerData(){carComponent = car2, controllerName = $"人机{carAiIndex++}"};
            var carAiData3 = new CarController.CarControllerData(){carComponent = car3, controllerName = $"人机{carAiIndex++}"};
            var carAiData4 = new CarController.CarControllerData(){carComponent = car4, controllerName = $"人机{carAiIndex++}"};
            
            var carAi1 = battleModel.AddAI(carAiData1);
            var carAi2 = battleModel.AddAI(carAiData2);
            var carAi3 = battleModel.AddAI(carAiData3);
            var carAi4 = battleModel.AddAI(carAiData4);
            
            
            var battleUIData =new BattleUI.BattleUIData(){p1Name = m_globalConfig.p1Name,p2Name = m_globalConfig.p2Name,
               carController1 = carAi1, carController2 = carAi2
            };
            m_battlePveUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            battleModel.SetBattleCamera(carAi1, carAi2);
        }
        
        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            PveUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }
        
        private void PveUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            if(CheckStateChange())
            {
                return;
            }
            DieOutSettlement();
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
                ChangeState<ProcedureWinSettlementPve>(procedureOwner, m_settlementPveData);
                return true;
            }
            return false;
        }

        private void OnAttackHappened(object sender, GameEventArgs e)
        {
            if (e is AttackHitArgs args)
            {

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
            m_battlePveUI = null;
            //清理容器
            battleModel.ClearModel();
            
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
            for(int i=0;i<battleModel.carControllers.Count;i++)
            {
                if(i>=battleModel.carControllers.Count) break;
                if(!battleModel.carControllers[i].IsHasCar) break;
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
                            GameEntry.instance.GameStartCoroutine(DelayToRevivePlayerPve(carPlayer,lastColor));
                        }
                        else if(battleModel.carControllers[i] is CarAI carAI)
                        {   
                            ReviveAI(carAI,lastColor);
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
                        var showMsg = $"{carComponent.carControllerName} 在与{killer}的激烈碰撞中牺牲了！";
                        Debug.Log(showMsg);
                        GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                    }
                    else if(attackInfo.attackType==AttackType.Skill)
                    {
                        if(attackInfo.userData is SkillEnum skillEnum)
                        {
                            var skillCfg = GameEntry.SkillManager.GetSkillConfigItem(skillEnum);
                            var showMsg =$"[{carName}]被{killer}使用[{skillCfg.skillName}]{skillCfg.killText}！";
                            Debug.Log(showMsg);
                            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                        }
                    }
                    battleModel.lastAttackInfoDict.Remove(carName);
                }
                else
                {
                    var showMsg = $"[{carName}]Ta自杀了...";
                    Debug.Log(showMsg);
                    GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
                }
            }
            else
            {
                var showMsg = $"[{carName}]Ta自杀了...";
                Debug.Log(showMsg);
                GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create(showMsg));
            }
        }
        
        private void WinSettlement()
        {

            
        }
        
        private IEnumerator DelayToRevivePlayerPve(CarPlayer carPlayer,CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.playerReviveTime);
            if(carPlayer.playerType == CarPlayer.PlayerType.P1)
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,Vector3.zero,Quaternion.identity,lastColor);
                battleModel.player1Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
                m_battlePveUI.RefreshSkillSlotsUI();
            }
            else
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,Vector3.zero,Quaternion.identity,lastColor);
                battleModel.player2Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
            }
        }
        
        private void ReviveAI(CarAI carAI, CarComponent.ClothColor lastColor)
        {
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,Vector3.zero,Quaternion.identity,lastColor);
            carAI.SetCar(car);
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
            
            return get1&&get2;
        }
    }
}
