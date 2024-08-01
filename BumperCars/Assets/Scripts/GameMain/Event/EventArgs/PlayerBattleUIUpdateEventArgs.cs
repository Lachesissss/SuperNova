using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerBattleUIUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerBattleUIUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public static PlayerBattleUIUpdateEventArgs Create(object userData = null)
        {
            var args = new PlayerBattleUIUpdateEventArgs();
            args.UserData = userData;
            return args;
        }
    }
}

