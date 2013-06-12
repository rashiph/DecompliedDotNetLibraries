namespace System.Web
{
    using System;

    internal enum EtwWorkerRequestType
    {
        IIS7Integrated = 3,
        InProc = 0,
        OutOfProc = 1,
        Undefined = -1,
        Unknown = 0x3e7
    }
}

