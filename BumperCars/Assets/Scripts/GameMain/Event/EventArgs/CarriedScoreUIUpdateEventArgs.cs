using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class CarriedScoreUIUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CarriedScoreUIUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public int p1NewCarriedScore { get; private set; }
        public int p2NewCarriedScore { get; private set; }

        public static CarriedScoreUIUpdateEventArgs Create(int p1, int p2, object userData = null)
        {
            var args = new CarriedScoreUIUpdateEventArgs();
            args.p1NewCarriedScore = p1;
            args.p2NewCarriedScore = p2;
            args.UserData = userData;
            return args;
        }
    }
}