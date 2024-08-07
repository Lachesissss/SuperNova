using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class ShowUITipsEventArgs: GameEventArgs
    {
        public static readonly int EventId = typeof(ShowUITipsEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public string content;

        public static ShowUITipsEventArgs Create( string content, object userData = null)
        {
            var args = new ShowUITipsEventArgs();
            args.content = content;
            args.UserData = userData;
            return args;
        }
    }
}