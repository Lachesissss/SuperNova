using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class AttackHitArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(AttackHitArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public AttackInfo attackInfo { get; private set; }

        public static AttackHitArgs Create(AttackInfo attackInfo, object userData = null)
        {
            var args = new AttackHitArgs();
            args.attackInfo = attackInfo;
            args.UserData = userData;
            return args;
        }
    }
}

