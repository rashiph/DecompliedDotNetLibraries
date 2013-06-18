namespace System.ServiceModel.Channels
{
    using System;

    internal interface IRequestBase
    {
        void Abort(RequestChannel requestChannel);
        void Fault(RequestChannel requestChannel);
    }
}

