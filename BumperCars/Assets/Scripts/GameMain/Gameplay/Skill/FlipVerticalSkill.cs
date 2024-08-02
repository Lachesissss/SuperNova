using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class FlipVerticalSkill : Skill
    {
        private GlobalConfig m_globalConfig;

        public override void Init(object userData = null)
        {
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
                if (target == ProcedureBattle.player1Camera.car)
                {
                    ProcedureBattle.player1Camera.SetFlipVertical(true);
                    GameEntry.instance.StartCoroutine(DelayToRecoverFlip(ProcedureBattle.player1Camera));
                    return true;
                }

                if (target == ProcedureBattle.player2Camera.car)
                {
                    ProcedureBattle.player2Camera.SetFlipVertical(true);
                    GameEntry.instance.StartCoroutine(DelayToRecoverFlip(ProcedureBattle.player2Camera));
                    return true;
                }
            }

            return false;
        }

        //找拥有摄像机且离自己最近的实体
        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            //if(source.carControllerName == ProcedureBattle.player1Camera.)
            var minDis = float.PositiveInfinity;
            target = null;
            foreach (var controller in ProcedureBattle.carControllers)
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
            return car.carComponent == ProcedureBattle.player1Camera.car || car.carComponent == ProcedureBattle.player2Camera.car;
        }

        private IEnumerator DelayToRecoverFlip(CarCamera camera)
        {
            yield return new WaitForSeconds(m_globalConfig.flipRecoverTime);
            camera.SetFlipVertical(false);
        }
    }
}