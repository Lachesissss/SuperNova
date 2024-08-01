using System;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Player : Entity
    {
        public CarController carController;

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            carController.ChangeCarTurnState(GameEntry.PlayerInputManager.steeringDeltaP1);
            carController.ChangeCarForwardState(GameEntry.PlayerInputManager.motorDeltaP1);
            if(GameEntry.PlayerInputManager.boostP1)
            {
                carController.DoBoost();
            }
            if(GameEntry.PlayerInputManager.skill1P1)
            {
                carController.ActivateSkill(0, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill2P1)
            {
                carController.ActivateSkill(1, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill3P1)
            {
                carController.ActivateSkill(2, GetSkillTarget());
            }
            carController.ChangeCarHandBrakeState(GameEntry.PlayerInputManager.handbrakeP1);
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            if(userData is string str)
            {
                carController.carName = str;
            }
        }

        public override void OnReturnToPool(bool isShowDown = false)
        {
            base.OnReturnToPool(isShowDown);
        }
        
        private CarController GetSkillTarget()
        {//先统一获取最近的实体
            float minDis = Single.PositiveInfinity;
            CarController closestCar = null;
            foreach (var ai in ProcedureBattle.carEnemies)
            {
                var dis =Vector3.Distance(ai.transform.position, transform.position);
                if(minDis>dis)
                {
                    minDis = dis;
                    closestCar = ai.carController;
                }
            }
            foreach (var player in ProcedureBattle.carPlayers)
            {
                var dis =Vector3.Distance(player.transform.position, transform.position);
                if(minDis>dis&&player.carController.name!=carController.name)
                {
                    minDis = dis;
                    closestCar = player.carController;
                }
            }
            return closestCar;
        }
    }
}