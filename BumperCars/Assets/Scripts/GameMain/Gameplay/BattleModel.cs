using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    //pvp,pve通用数据库
    public class BattleModel
    {
        public Dictionary<string, AttackInfo> lastAttackInfoDict; //key：被攻击的carName，value：最近一次攻击信息
        public Dictionary< CarController,CarCamera> controllerCameraDict;
        public List<CarController> carControllers;
        public List<SkillPickUpItem> skillPickUpItems;
        public Dictionary<CarController, Transform> spawnPosDict;
        private Dictionary<string,CarController> carDict;
        public Dictionary<string,int> killOtherPlayerNumDict;
        public int deadCnt = 0;
        public CarCamera player1Camera;
        public CarCamera player2Camera;
        public SkillEnum p1Ultimate;
        public SkillEnum p2Ultimate;
        public DungeonMode currentDungeonMode;
        BattleModel()
        {
            lastAttackInfoDict = new();
            controllerCameraDict = new();
            carControllers = new();
            skillPickUpItems = new();
            carDict = new();
            spawnPosDict = new();
            killOtherPlayerNumDict = new();
            currentDungeonMode = DungeonMode.Single;//默认是单人训练
            p1Ultimate = SkillEnum.TownPortal; //默认都是传送
            p2Ultimate = SkillEnum.TownPortal;
        }

        public static BattleModel Instance { get; } = new BattleModel();

        public void ClearModel()
        {
            lastAttackInfoDict.Clear();
            controllerCameraDict.Clear();
            carControllers.Clear();
            skillPickUpItems.Clear();
            carDict.Clear();
            spawnPosDict.Clear();
            killOtherPlayerNumDict.Clear();
        }
        
        public void SetBattleCamera(CarController p1Controller, CarController p2Controller)
        {
            player1Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player1Camera, Vector3.zero, Quaternion.identity, p1Controller.carComponent);
            player2Camera = GameEntry.EntityManager.CreateEntity<CarCamera>(EntityEnum.Player2Camera, Vector3.zero, Quaternion.identity, p2Controller.carComponent);
            controllerCameraDict.Add(p1Controller,player1Camera);
            controllerCameraDict.Add(p2Controller,player2Camera);
        }
        
        public CarPlayer AddPlayer(CarController.CarControllerData playerData)
        {
            var carPlayer1 = GameEntry.EntityManager.CreateEntity<CarPlayer>(EntityEnum.CarPlayer, Vector3.zero, Quaternion.identity, playerData);
            carControllers.Add(carPlayer1);
            if(!carDict.ContainsKey(carPlayer1.controllerName))
            {
                carDict.Add(carPlayer1.controllerName, carPlayer1);
                killOtherPlayerNumDict.Add(carPlayer1.controllerName, 0);
            }
            return carPlayer1;
        }
        
        public CarAI AddAI(CarController.CarControllerData aiData)
        {
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI, Vector3.zero, Quaternion.identity, aiData);
            carControllers.Add(carAi);
            
            if(!carDict.ContainsKey(carAi.controllerName))
            {
                carDict.Add(carAi.controllerName, carAi);
            }
            return carAi;
        }
        
        public CarController GetControllerByName(string name)
        {
            if(carDict.TryGetValue(name, out var controller))
            {
                return controller;
            }
            return null;
        }
    }
}

