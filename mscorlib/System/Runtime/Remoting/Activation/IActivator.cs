namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IActivator
    {
        [SecurityCritical]
        IConstructionReturnMessage Activate(IConstructionCallMessage msg);

        ActivatorLevel Level { [SecurityCritical] get; }

        IActivator NextActivator { [SecurityCritical] get; [SecurityCritical] set; }
    }
}

