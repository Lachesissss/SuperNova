using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class PlayerArriveHomeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerArriveHomeEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public string playerName { get; private set; }

        public static PlayerArriveHomeEventArgs Create(string playerName, object userData = null)
        {
            var args = new PlayerArriveHomeEventArgs();
            args.playerName = playerName;
            args.UserData = userData;
            return args;
        }
    }
}