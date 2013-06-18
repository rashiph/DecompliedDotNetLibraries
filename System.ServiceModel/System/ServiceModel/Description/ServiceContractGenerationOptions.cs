namespace System.ServiceModel.Description
{
    using System;

    [Flags]
    public enum ServiceContractGenerationOptions
    {
        AsynchronousMethods = 1,
        ChannelInterface = 2,
        ClientClass = 8,
        EventBasedAsynchronousMethods = 0x20,
        InternalTypes = 4,
        None = 0,
        TypedMessages = 0x10
    }
}

