namespace System.ComponentModel
{
    using System;

    public interface ISupportInitializeNotification : ISupportInitialize
    {
        event EventHandler Initialized;

        bool IsInitialized { get; }
    }
}

