using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerCoolingUIUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(PlayerCoolingUIUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object userData { get; private set; }

        public BattleUI.CoolingInfo coolingInfo { get; private set; }

        public static PlayerCoolingUIUpdateEventArgs Create(BattleUI.CoolingInfo info, object userData = null)
        {
            var args = new PlayerCoolingUIUpdateEventArgs();
            args.coolingInfo = info;
            args.userData = userData;
            return args;
        }
    }
}

