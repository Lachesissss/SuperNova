using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;


namespace Lachesis.GamePlay
{
    public class SwitchCarEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(SwitchCarEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }
        

        public static SwitchCarEventArgs Create(object userData = null)
        {
            var args = new SwitchCarEventArgs();
            args.UserData = userData;
            return args;
        }
    }
}

