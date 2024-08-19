using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lachesis.Core;

namespace Lachesis.GamePlay
{
    public class VolumeChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(VolumeChangeEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }
        

        public static VolumeChangeEventArgs Create(object userData = null)
        {
            var args = new VolumeChangeEventArgs();
            args.UserData = userData;
            return args;
        }
    }
}
