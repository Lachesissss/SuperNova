using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerSkillSlotsUIUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerSkillSlotsUIUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public static PlayerSkillSlotsUIUpdateEventArgs Create(object userData = null)
        {
            var args = new PlayerSkillSlotsUIUpdateEventArgs();
            args.UserData = userData;
            return args;
        }
    }
}

