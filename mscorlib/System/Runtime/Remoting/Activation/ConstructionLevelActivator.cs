namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable]
    internal class ConstructionLevelActivator : IActivator
    {
        internal ConstructionLevelActivator()
        {
        }

        [ComVisible(true), SecurityCritical]
        public virtual IConstructionReturnMessage Activate(IConstructionCallMessage ctorMsg)
        {
            ctorMsg.Activator = ctorMsg.Activator.NextActivator;
            return ActivationServices.DoServerContextActivation(ctorMsg);
        }

        public virtual ActivatorLevel Level
        {
            [SecurityCritical]
            get
            {
                return ActivatorLevel.Construction;
            }
        }

        public virtual IActivator NextActivator
        {
            [SecurityCritical]
            get
            {
                return null;
            }
            [SecurityCritical]
            set
            {
                throw new InvalidOperationException();
            }
        }
    }
}

