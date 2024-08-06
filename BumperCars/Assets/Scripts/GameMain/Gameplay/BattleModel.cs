using System.Collections;
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
        
        public CarCamera player1Camera;
        public CarCamera player2Camera;
        
        BattleModel()
        {
            lastAttackInfoDict = new();
            controllerCameraDict = new();
            carControllers = new();
            skillPickUpItems = new();
        }

        public static BattleModel Instance { get; } = new BattleModel();

        public void ClearModel()
        {
            lastAttackInfoDict.Clear();
            controllerCameraDict.Clear();
            carControllers.Clear();
            skillPickUpItems.Clear();
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
            return carPlayer1;
        }
        
        public CarAI AddAI(CarController.CarControllerData aiData)
        {
            var carAi = GameEntry.EntityManager.CreateEntity<CarAI>(EntityEnum.CarAI, Vector3.zero, Quaternion.identity, aiData);
            carControllers.Add(carAi);
            return carAi;
        }
    }
}

