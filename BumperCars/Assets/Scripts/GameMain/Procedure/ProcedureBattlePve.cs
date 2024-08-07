using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Lachesis.Core;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class ProcedureBattlePve : ProcedureBase
    {
        private ProcedureOwner procedureOwner;
        private BattleField m_battleField = null;
        
        private BattleModel battleModel;
        private int maxSkillPickUpItemsCount = 8;
        
        
        private BattlePveUI m_battlePveUI;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementPveData m_settlementPveData;
        private GlobalConfig m_globalConfig;
        private int bossCurHealth;
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
            
            
            PveEnter();
        }
        
        
        private void PveEnter()
        {
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation, CarComponent.ClothColor.Yellow);
            var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car,m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation, CarComponent.ClothColor.Black);
            var car3 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.BossCar,Vector3.zero,Quaternion.identity);
            var carPlayerData1 = new CarController.CarControllerData(){carComponent = car1, controllerName = m_globalConfig.p1Name, userData = CarPlayer.PlayerType.P1};
            var carPlayerData2 = new CarController.CarControllerData(){carComponent = car2, controllerName = m_globalConfig.p2Name, userData = CarPlayer.PlayerType.P2};
            var carAiData = new CarController.CarControllerData(){carComponent = car3, controllerName = m_globalConfig.pveBossName};
            
            var carPlayer1 = battleModel.AddPlayer(carPlayerData1);
            var carPlayer2 = battleModel.AddPlayer(carPlayerData2);
            var carAi = battleModel.AddAI(carAiData);
            
            
            var battleUIPveData =new BattlePveUI.BattlePveUIData(){p1Name = m_globalConfig.p1Name,p2Name = m_globalConfig.p2Name,
                bossName = m_globalConfig.pveBossName, maxBossHealth = m_globalConfig.pveBossMaxHealth, carController1 = carPlayer1, carController2 = carPlayer2
            };
            m_battlePveUI = GameEntry.EntityManager.CreateEntity<BattlePveUI>(EntityEnum.BattlePveUI, GameEntry.instance.canvasRoot.transform, battleUIPveData);
            battleModel.SetBattleCamera(carPlayer1, carPlayer2);
            bossCurHealth = m_globalConfig.pveBossMaxHealth;
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
                SkillEnum skillEnum = GameEntry.SkillManager.GetRandomSkillEnumPve();
                Vector3 spawnPosition = GetRandomPositionInBattleField();
                battleModel.skillPickUpItems.Add(GameEntry.EntityManager.CreateEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, spawnPosition, Quaternion.identity, skillEnum));
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
                ChangeState<ProcedureWinSettlementPve>(procedureOwner, m_settlementPveData);
                return true;
            }
            return false;
        }

        private void OnAttackHappened(object sender, GameEventArgs e)
        {
            if (e is AttackHitArgs args)
            {
                battleModel.lastAttackInfoDict[args.attackInfo.underAttacker] = args.attackInfo;
                if(args.attackInfo.underAttacker == m_globalConfig.pveBossName)
                {
                    bossCurHealth-=args.attackInfo.attackDamge;
                    GameEntry.EventManager.Invoke(this, BossInfoUpdateEventArgs.Create(bossCurHealth, m_globalConfig.pveBossName));
                }
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
                            //TODO:Boss立即复活，血量掉很大一段
                            ReviveBossAI(carAI,lastColor);
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
            if(bossCurHealth<=0)
            {
                var data =  new SettlementPveData();
                data.isWin = true;
                isGoSettlement = true;
                m_settlementPveData = data;
                return;
            }
            
        }
        
        private IEnumerator DelayToRevivePlayerPve(CarPlayer carPlayer,CarComponent.ClothColor lastColor)
        {
            yield return new WaitForSeconds(m_globalConfig.playerReviveTime);
            if(carPlayer.playerType == CarPlayer.PlayerType.P1)
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,m_battleField.spawnTrans1.rotation,lastColor);
                battleModel.player1Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
                m_battlePveUI.RefreshSkillSlotsUI();
            }
            else
            {
                var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans2.position,m_battleField.spawnTrans2.rotation,lastColor);
                battleModel.player2Camera.ReSetTrackedTarget(car);
                carPlayer.SetCar(car);
                carPlayer.ClearSkills();
            }
        }
        
        private void ReviveBossAI(CarAI carAI, CarComponent.ClothColor lastColor)
        {
            var car = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.BossCar,Vector3.zero,Quaternion.identity);
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
