using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lachesis.Core;
namespace Lachesis.GamePlay
{
    public class CoinPickedUpEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(CoinPickedUpEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }
        
        public int pickedNum { get; private set; }

        public static CoinPickedUpEventArgs Create(int pickedNum, object userData = null)
        {
            var args = new CoinPickedUpEventArgs();
            args.pickedNum = pickedNum;
            args.UserData = userData;
            return args;
        }
    }
}