namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class TrustManagerContext
    {
        private ApplicationIdentity m_appId;
        private bool m_ignorePersistedDecision;
        private bool m_keepAlive;
        private bool m_noPrompt;
        private bool m_persist;
        private TrustManagerUIContext m_uiContext;

        public TrustManagerContext() : this(TrustManagerUIContext.Run)
        {
        }

        public TrustManagerContext(TrustManagerUIContext uiContext)
        {
            this.m_ignorePersistedDecision = false;
            this.m_uiContext = uiContext;
            this.m_keepAlive = false;
            this.m_persist = true;
        }

        public virtual bool IgnorePersistedDecision
        {
            get
            {
                return this.m_ignorePersistedDecision;
            }
            set
            {
                this.m_ignorePersistedDecision = value;
            }
        }

        public virtual bool KeepAlive
        {
            get
            {
                return this.m_keepAlive;
            }
            set
            {
                this.m_keepAlive = value;
            }
        }

        public virtual bool NoPrompt
        {
            get
            {
                return this.m_noPrompt;
            }
            set
            {
                this.m_noPrompt = value;
            }
        }

        public virtual bool Persist
        {
            get
            {
                return this.m_persist;
            }
            set
            {
                this.m_persist = value;
            }
        }

        public virtual ApplicationIdentity PreviousApplicationIdentity
        {
            get
            {
                return this.m_appId;
            }
            set
            {
                this.m_appId = value;
            }
        }

        public virtual TrustManagerUIContext UIContext
        {
            get
            {
                return this.m_uiContext;
            }
            set
            {
                this.m_uiContext = value;
            }
        }
    }
}

