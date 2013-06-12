namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Security;

    public interface ISecurableChannel
    {
        bool IsSecured { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

