namespace System.Runtime.Hosting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Policy;

    [Serializable, ComVisible(true)]
    public sealed class ActivationArguments : EvidenceBase
    {
        private bool m_activateInstance;
        private string[] m_activationData;
        private string m_appFullName;
        private string[] m_appManifestPaths;
        private bool m_useFusionActivationContext;

        private ActivationArguments()
        {
        }

        public ActivationArguments(System.ActivationContext activationData) : this(activationData, null)
        {
        }

        public ActivationArguments(System.ApplicationIdentity applicationIdentity) : this(applicationIdentity, null)
        {
        }

        public ActivationArguments(System.ActivationContext activationContext, string[] activationData)
        {
            if (activationContext == null)
            {
                throw new ArgumentNullException("activationContext");
            }
            this.m_appFullName = activationContext.Identity.FullName;
            this.m_appManifestPaths = activationContext.ManifestPaths;
            this.m_activationData = activationData;
            this.m_useFusionActivationContext = true;
        }

        public ActivationArguments(System.ApplicationIdentity applicationIdentity, string[] activationData)
        {
            if (applicationIdentity == null)
            {
                throw new ArgumentNullException("applicationIdentity");
            }
            this.m_appFullName = applicationIdentity.FullName;
            this.m_activationData = activationData;
        }

        internal ActivationArguments(string appFullName, string[] appManifestPaths, string[] activationData)
        {
            if (appFullName == null)
            {
                throw new ArgumentNullException("appFullName");
            }
            this.m_appFullName = appFullName;
            this.m_appManifestPaths = appManifestPaths;
            this.m_activationData = activationData;
            this.m_useFusionActivationContext = true;
        }

        public override EvidenceBase Clone()
        {
            ActivationArguments arguments = new ActivationArguments {
                m_useFusionActivationContext = this.m_useFusionActivationContext,
                m_activateInstance = this.m_activateInstance,
                m_appFullName = this.m_appFullName
            };
            if (this.m_appManifestPaths != null)
            {
                arguments.m_appManifestPaths = new string[this.m_appManifestPaths.Length];
                Array.Copy(this.m_appManifestPaths, arguments.m_appManifestPaths, arguments.m_appManifestPaths.Length);
            }
            if (this.m_activationData != null)
            {
                arguments.m_activationData = new string[this.m_activationData.Length];
                Array.Copy(this.m_activationData, arguments.m_activationData, arguments.m_activationData.Length);
            }
            arguments.m_activateInstance = this.m_activateInstance;
            arguments.m_appFullName = this.m_appFullName;
            arguments.m_useFusionActivationContext = this.m_useFusionActivationContext;
            return arguments;
        }

        internal bool ActivateInstance
        {
            get
            {
                return this.m_activateInstance;
            }
            set
            {
                this.m_activateInstance = value;
            }
        }

        public System.ActivationContext ActivationContext
        {
            get
            {
                if (!this.UseFusionActivationContext)
                {
                    return null;
                }
                if (this.m_appManifestPaths == null)
                {
                    return new System.ActivationContext(new System.ApplicationIdentity(this.m_appFullName));
                }
                return new System.ActivationContext(new System.ApplicationIdentity(this.m_appFullName), this.m_appManifestPaths);
            }
        }

        public string[] ActivationData
        {
            get
            {
                return this.m_activationData;
            }
        }

        internal string ApplicationFullName
        {
            get
            {
                return this.m_appFullName;
            }
        }

        public System.ApplicationIdentity ApplicationIdentity
        {
            get
            {
                return new System.ApplicationIdentity(this.m_appFullName);
            }
        }

        internal string[] ApplicationManifestPaths
        {
            get
            {
                return this.m_appManifestPaths;
            }
        }

        internal bool UseFusionActivationContext
        {
            get
            {
                return this.m_useFusionActivationContext;
            }
        }
    }
}

