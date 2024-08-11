using System;

namespace Lachesis.GamePlay
{
    public class MagicShieldSkill : Skill
    {
        public override void Init(object userData = null)
        {
            
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public override bool TryActivate(CarComponent source, object userData = null)
        {
            if (source == null) return false;
            source.GetMagicShield();
            return true;
        }

        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            target = source;
            return true;
        }
    }
}