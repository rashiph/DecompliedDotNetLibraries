namespace System.ComponentModel
{
    using System;

    public interface IRevertibleChangeTracking : IChangeTracking
    {
        void RejectChanges();
    }
}

