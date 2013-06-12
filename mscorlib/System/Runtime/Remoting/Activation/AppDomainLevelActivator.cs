namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class AppDomainLevelActivator : IActivator
    {
        private IActivator m_NextActivator;
        private string m_RemActivatorURL;

        internal AppDomainLevelActivator(string remActivatorURL)
        {
            this.m_RemActivatorURL = remActivatorURL;
        }

        internal AppDomainLevelActivator(SerializationInfo info, StreamingContext context)
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
            ctorMsg.Activator = this.m_NextActivator;
            return ActivationServices.GetActivator().Activate(ctorMsg);
        }

        public virtual ActivatorLevel Level
        {
            [SecurityCritical]
            get
            {
                return ActivatorLevel.AppDomain;
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

