namespace System.ServiceModel.Channels
{
    using System;

    public interface IBindingRuntimePreferences
    {
        bool ReceiveSynchronously { get; }
    }
}

