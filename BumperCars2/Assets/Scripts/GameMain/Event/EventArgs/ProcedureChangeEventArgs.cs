using System;

namespace Lachesis.Core
{
    public class ProcedureChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ProcedureChangeEventArgs).GetHashCode();

        public override int Id => EventId;

        public object userData { get; private set; }

        public Type TargetProcedureType { get; private set; }

        public static ProcedureChangeEventArgs Create(Type target, object userData = null)
        {
            var args = new ProcedureChangeEventArgs();
            args.TargetProcedureType = target;
            args.userData = userData;
            return args;
        }
    }
}