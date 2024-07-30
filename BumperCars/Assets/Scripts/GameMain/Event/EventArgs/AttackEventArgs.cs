using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class AttackEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(AttackEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public AttackInfo attackInfo { get; private set; }

        public static AttackEventArgs Create(AttackInfo info, object userData = null)
        {
            var args = new AttackEventArgs();
            args.attackInfo = info;
            args.UserData = userData;
            return args;
        }
    }
}