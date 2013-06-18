namespace System.ServiceModel.Channels
{
    using System;

    internal interface IPostRollbackErrorStrategy
    {
        bool AnotherTryNeeded();
    }
}

