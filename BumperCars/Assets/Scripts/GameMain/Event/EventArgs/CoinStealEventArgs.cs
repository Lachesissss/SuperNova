using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class CoinStealEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CoinStealEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public string stealerName { get; private set; }
        public string underStealerName { get; private set; }

        public static CoinStealEventArgs Create(string stealerName, string underStealerName, object userData = null)
        {
            var args = new CoinStealEventArgs();
            args.stealerName = stealerName;
            args.underStealerName = underStealerName;
            args.UserData = userData;
            return args;
        }
    }
}