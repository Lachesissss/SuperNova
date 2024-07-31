using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class GetSkillEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(GetSkillEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public SkillEnum skillEnum { get; private set; }
        public string userName { get; private set; }

        public static GetSkillEventArgs Create( SkillEnum skillEnum, string userName , object userData = null)
        {
            var args = new GetSkillEventArgs
            {
                skillEnum = skillEnum,
                userName = userName
            };
            args.UserData = userData;
            return args;
        }
    }
}

