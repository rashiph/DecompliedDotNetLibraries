namespace System.ServiceModel.Channels
{
    using System;

    public interface ITransactedBindingElement
    {
        bool TransactedReceiveEnabled { get; }
    }
}

