using System;

namespace Lachesis.Core
{
    public abstract class GameEventArgs : EventArgs
    {
        public abstract int Id { get; }
    }
}