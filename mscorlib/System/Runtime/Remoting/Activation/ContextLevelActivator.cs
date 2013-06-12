namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class ContextLevelActivator : IActivator
    {
        private IActivator m_NextActivator;

        internal ContextLevelActivator()
        {
            this.m_NextActivator = null;
        }

        internal ContextLevelActivator(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_NextActivator = (IActivator) info.GetValue("m_NextActivator", typeof(IActivator));
        }

        [ComVisible(true), SecurityCritical]
        public virtual IConstructionReturnMessage Activate(IConstructionCallMessage ctorMsg)
        {
            ctorMsg.Activator = ctorMsg.Activator.NextActivator;
            return ActivationServices.DoCrossContextActivation(ctorMsg);
        }

        public virtual ActivatorLevel Level
        {
            [SecurityCritical]
            get
            {
                return ActivatorLevel.Context;
            }
        }

        public virtual IActivator NextActivator
        {
            [SecurityCritical]
            get
            {
                return this.m_NextActivator;
            }
            [SecurityCritical]
            set
            {
                this.m_NextActivator = value;
            }
        }
    }
}

