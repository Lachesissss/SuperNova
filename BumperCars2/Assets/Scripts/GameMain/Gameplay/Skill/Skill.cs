namespace Lachesis.GamePlay
{
    public abstract class Skill
    {
        public string skillName;
        public SkillEnum skillEnum;
        public abstract void Init(object userData = null);
        public abstract void Update(float elapseSeconds, float realElapseSeconds);
        public abstract bool TryActivate(CarComponent source, object userData = null);

        public abstract bool TryGetSkillTarget(CarComponent source, out CarComponent target);
    }
}

