namespace System.ComponentModel
{
    using System;

    public interface IChangeTracking
    {
        void AcceptChanges();

        bool IsChanged { get; }
    }
}

