namespace Lachesis.GamePlay
{
    public abstract class NetworkSkill
    {
        public string skillName;
        public NetworkSkillEnum skillEnum;
        public abstract void Init(object userData = null);
        public abstract void Update(float elapseSeconds, float realElapseSeconds);
        public abstract bool TryActivate(NetworkCarComponent source, object userData = null);

        public abstract bool TryGetSkillTarget(NetworkCarComponent source, out NetworkCarComponent target);
    }
}