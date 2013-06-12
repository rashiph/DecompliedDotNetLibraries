namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.Remoting;
    using System.Security;

    internal interface IInternalMessage
    {
        [SecurityCritical]
        bool HasProperties();
        [SecurityCritical]
        void SetCallContext(LogicalCallContext callContext);
        [SecurityCritical]
        void SetURI(string uri);

        Identity IdentityObject { [SecurityCritical] get; [SecurityCritical] set; }

        ServerIdentity ServerIdentityObject { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

