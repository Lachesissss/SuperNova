using System;
using Lachesis.Core;
using Unity.VisualScripting;

namespace Lachesis.GamePlay
{
    public class AttackEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(AttackEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public AttackInfo attackInfo { get; private set; }
        
        public Action OnHit;

        public static AttackEventArgs Create(AttackInfo info,Action onHit, object userData = null)
        {
            var args = new AttackEventArgs();
            args.attackInfo = info;
            args.OnHit = onHit;
            args.UserData = userData;
            return args;
        }
    }
}