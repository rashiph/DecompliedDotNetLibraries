namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [ComVisible(true)]
    public interface IConstructionCallMessage : IMethodCallMessage, IMethodMessage, IMessage
    {
        Type ActivationType { [SecurityCritical] get; }

        string ActivationTypeName { [SecurityCritical] get; }

        IActivator Activator { [SecurityCritical] get; [SecurityCritical] set; }

        object[] CallSiteActivationAttributes { [SecurityCritical] get; }

        IList ContextProperties { [SecurityCritical] get; }
    }
}

