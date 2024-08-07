using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class GetNetworkSkillEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(GetNetworkSkillEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public NetworkSkillEnum skillEnum { get; private set; }
        public string userName { get; private set; }

        public static GetNetworkSkillEventArgs Create(NetworkSkillEnum skillEnum, string userName, object userData = null)
        {
            var args = new GetNetworkSkillEventArgs
            {
                skillEnum = skillEnum,
                userName = userName
            };
            args.UserData = userData;
            return args;
        }
    }
}