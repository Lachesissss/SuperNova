using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
namespace Lachesis.GamePlay
{
    public class BossInfoUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(BossInfoUpdateEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public int  bossHealth { get; private set; }
        public string bossName { get; private set; }

        public static BossInfoUpdateEventArgs Create( int bossHealth, string bossName , object userData = null)
        {
            var args = new BossInfoUpdateEventArgs
            {
                bossHealth = bossHealth,
                bossName = bossName
            };
            args.UserData = userData;
            return args;
        }
    }
}