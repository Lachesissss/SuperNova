using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class CarriedScoreUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CarriedScoreUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public string playerName { get; private set; }
        public int getPointNum { get; private set; }

        public static CarriedScoreUpdateEventArgs Create(string playerName, int getPointNum, object userData = null)
        {
            var args = new CarriedScoreUpdateEventArgs();
            args.playerName = playerName;
            args.getPointNum = getPointNum;
            args.UserData = userData;
            return args;
        }
    }
}