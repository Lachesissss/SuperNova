using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CompressSkill : Skill
    {
        private GlobalConfig m_globalConfig;
        private BattleModel m_battleModel;
        public override void Init(object userData = null)
        {
            m_battleModel = BattleModel.Instance;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        //特攻类技能
        public override bool TryActivate(CarComponent source, object userData = null)
        {
            if (TryGetSkillTarget(source, out var target))
            {
                if (target == m_battleModel.player1Camera.car)
                {
                    CarCamera.Compress(m_battleModel.player2Camera, m_battleModel.player1Camera);
                    return true;
                }

                if (target == m_battleModel.player2Camera.car)
                {
                    CarCamera.Compress(m_battleModel.player1Camera, m_battleModel.player2Camera);
                    return true;
                }
            }

            return false;
        }

        //找拥有摄像机且离自己最近的实体
        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            if(CarCamera.isCompressing)
            {
                target = null;
                return false;
            } 
            var minDis = float.PositiveInfinity;
            target = null;
            foreach (var controller in m_battleModel.carControllers)
                if (controller.IsHasCar && controller != source.controller && HasCamera(controller))
                {
                    var dis = Vector3.Distance(controller.carComponent.transform.position, source.transform.position);
                    if (minDis > dis)
                    {
                        minDis = dis;
                        target = controller.carComponent;
                    }
                }

            return target != null;
        }

        private bool HasCamera(CarController car)
        {
            return car.carComponent == m_battleModel.player1Camera.car || car.carComponent == m_battleModel.player2Camera.car;
        }
    }
}
