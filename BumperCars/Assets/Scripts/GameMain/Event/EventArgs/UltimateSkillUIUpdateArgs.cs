using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class UltimateSkillUIUpdateArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UltimateSkillUIUpdateArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }
        

        public static UltimateSkillUIUpdateArgs Create(object userData = null)
        {
            var args = new UltimateSkillUIUpdateArgs();
            args.UserData = userData;
            return args;
        }
    }
}
