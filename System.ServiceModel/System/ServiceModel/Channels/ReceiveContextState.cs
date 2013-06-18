namespace System.ServiceModel.Channels
{
    using System;

    public enum ReceiveContextState
    {
        Received,
        Completing,
        Completed,
        Abandoning,
        Abandoned,
        Faulted
    }
}

