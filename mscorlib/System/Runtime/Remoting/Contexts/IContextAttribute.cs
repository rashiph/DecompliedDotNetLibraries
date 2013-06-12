namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Security;

    [ComVisible(true)]
    public interface IContextAttribute
    {
        [SecurityCritical]
        void GetPropertiesForNewContext(IConstructionCallMessage msg);
        [SecurityCritical]
        bool IsContextOK(Context ctx, IConstructionCallMessage msg);
    }
}

