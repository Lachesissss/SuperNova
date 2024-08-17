using System;
using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class FlipVerticalSkill : Skill
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
                void OnHit()
                {
                    if (target == m_battleModel.player1Camera.car)
                    {
                        m_battleModel.player1Camera.SetFlipVertical(true);
                        source.StartCoroutine(DelayToRecoverFlip(m_battleModel.player1Camera));
                    }

                    if (target == m_battleModel.player2Camera.car)
                    {
                        m_battleModel.player2Camera.SetFlipVertical(true);
                        GameEntry.instance.StartCoroutine(DelayToRecoverFlip(m_battleModel.player2Camera));
                    }
                }

                var attackInfo = new AttackInfo();
                attackInfo.attacker = source.carControllerName;
                attackInfo.underAttacker = target.carControllerName;
                attackInfo.attackTime = DateTime.Now;
                attackInfo.attackType = AttackType.Skill;
                attackInfo.userData = skillEnum;
                attackInfo.attackDamge = 0;
                attackInfo.canDodge = false;
                GameEntry.EventManager.Invoke(this, AttackEventArgs.Create(attackInfo, OnHit));
                return true;
            }

            return false;
        }

        //找拥有摄像机且离自己最近的实体
        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            //if(source.carControllerName == ProcedureBattle.player1Camera.)
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

        private IEnumerator DelayToRecoverFlip(CarCamera camera)
        {
            yield return new WaitForSeconds(m_globalConfig.flipRecoverTime);
            camera.SetFlipVertical(false);
        }
    }
}