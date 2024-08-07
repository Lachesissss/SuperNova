using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using ProcedureOwner = FSM<Lachesis.Core.ProcedureManager>;


namespace Lachesis.GamePlay
{
    public class ProcedureBattleNetwork : ProcedureBase
    {
        private ProcedureOwner procedureOwner;
        private BattleField m_battleField;
        private static Dictionary<string, int> m_playerScoreDict;
        private BattleModel battleModel;
        private int carAiIndex;
        private int maxSkillPickUpItemsCount = 8;
        private BattleUI m_battleUI;
        private bool isGoMenu;
        private bool isGoSettlement;
        private SettlementData m_settlementData;
        private GlobalConfig m_globalConfig;
        private BumperCarNetworkNetworkManager m_networkManager;

        protected internal override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            battleModel = BattleModel.Instance;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            m_playerScoreDict = new Dictionary<string, int>();
            m_playerScoreDict.Add(m_globalConfig.p1Name, 0);
            m_playerScoreDict.Add(m_globalConfig.p2Name, 0);
        }

        protected internal override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            this.procedureOwner = procedureOwner;
            Debug.Log("进入主战斗流程");


            //注册事件
            // GameEntry.EventManager.AddListener(AttackHitArgs.EventId, OnAttackHappened);
            // GameEntry.EventManager.AddListener(ProcedureChangeEventArgs.EventId, OnProcedureChange);
            // GameEntry.EventManager.AddListener(SwitchCarEventArgs.EventId, OnSwitchCar);
            // GameEntry.EventManager.AddListener(GetSkillEventArgs.EventId, OnSkillItemPicked);
            isGoMenu = false;
            isGoSettlement = false;
            m_settlementData = null;

            //关卡相关，生成实例
            m_networkManager = GameEntry.EntityManager
                .CreateEntity<NetworkManagerEntity>(EntityEnum.BumperCarNetworkManagers, Vector3.zero, Quaternion.identity).manager;
            m_battleField = GameEntry.EntityManager.CreateEntity<BattleField>(EntityEnum.BattleField, Vector3.zero, Quaternion.identity);
            // var car1 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans1.position,
            //     m_battleField.spawnTrans1.rotation, CarComponent.ClothColor.Yellow);
            // var car2 = GameEntry.EntityManager.CreateEntity<CarComponent>(EntityEnum.Car, m_battleField.spawnTrans2.position,
            //     m_battleField.spawnTrans2.rotation, CarComponent.ClothColor.Black);
            // var controllerData1 = new CarController.CarControllerData
            //     { carComponent = car1, controllerName = m_globalConfig.p1Name, userData = CarPlayer.PlayerType.P1 };
            // var aiName = $"人机{carAiIndex++}";
            // var controllerData2 = new CarController.CarControllerData { carComponent = car2, controllerName = aiName };
            // var carPlayer = battleModel.AddPlayer(controllerData1);
            // var carAi = battleModel.AddAI(controllerData2);

            // var carPlayer = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, controllerData1);
            // var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI,Vector3.zero, Quaternion.identity,controllerData2);
            // carControllers.Add(carPlayer);
            // carControllers.Add(carAi);

            // var battleUIData = new BattleUI.BattleUIData
            // {
            //     p1Name = m_globalConfig.p1Name, p2Name = m_globalConfig.p2Name,
            //     targetScore = m_globalConfig.targetScore, carController1 = carPlayer, carController2 = carAi
            // };
            // m_battleUI = GameEntry.EntityManager.CreateEntity<BattleUI>(EntityEnum.BattleUI, GameEntry.instance.canvasRoot.transform, battleUIData);
            // battleModel.SetBattleCamera(carPlayer, carAi);
        }

        protected internal override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
        }

        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }
    }
}