using System;

namespace Lachesis.Core
{
    public class ProcedureChangeEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(ProcedureChangeEventArgs).GetHashCode();

        public override int Id => EventId;

        public object UserData { get; private set; }

        public Type TargetProcedureType { get; private set; }

        public static ProcedureChangeEventArgs Create(Type target, object userData = null)
        {
            var buildTowerEventArgs = new ProcedureChangeEventArgs();
            buildTowerEventArgs.TargetProcedureType = target;
            buildTowerEventArgs.UserData = userData;
            return buildTowerEventArgs;
        }
    }
}