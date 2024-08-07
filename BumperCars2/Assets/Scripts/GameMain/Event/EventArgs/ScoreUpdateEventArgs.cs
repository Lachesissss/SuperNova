using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class ScoreUpdateEventArgs: GameEventArgs
    {
        public static readonly int EventId = typeof(ScoreUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public int p1NewScore { get; private set; }
        public int p2NewScore { get; private set; }

        public static ScoreUpdateEventArgs Create( int p1,int p2 , object userData = null)
        {
            var args = new ScoreUpdateEventArgs();
            args.p1NewScore = p1;
            args.p2NewScore = p2;
            args.UserData = userData;
            return args;
        }
    }
}

