namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;

    public class GetManifestCompletedEventArgs : AsyncCompletedEventArgs
    {
        private System.ActivationContext _actContext;
        private ActivationDescription _activationDescription;
        private System.ApplicationIdentity _applicationIdentity;
        private bool _isCached;
        private string _logFilePath;
        private string _name;
        private byte[] _rawApplicationManifest;
        private byte[] _rawDeploymentManifest;
        private DefinitionIdentity _subId;
        private Uri _support;
        private System.Version _version;

        internal GetManifestCompletedEventArgs(BindCompletedEventArgs e, string logFilePath) : base(e.Error, e.Cancelled, e.UserState)
        {
            this._logFilePath = logFilePath;
        }

        internal GetManifestCompletedEventArgs(BindCompletedEventArgs e, Exception error, string logFilePath) : base(error, e.Cancelled, e.UserState)
        {
            this._logFilePath = logFilePath;
        }

        internal GetManifestCompletedEventArgs(BindCompletedEventArgs e, ActivationDescription activationDescription, string logFilePath, Logger.LogIdentity log) : base(e.Error, e.Cancelled, e.UserState)
        {
            this._applicationIdentity = (e.ActivationContext != null) ? e.ActivationContext.Identity : null;
            Logger.AddInternalState(log, "Creating GetManifestCompletedEventArgs.");
            string text = this._applicationIdentity.ToString();
            DefinitionAppId id = new DefinitionAppId(text);
            this._subId = id.DeploymentIdentity.ToSubscriptionId();
            this._logFilePath = logFilePath;
            this._isCached = e.IsCached;
            this._name = e.FriendlyName;
            this._actContext = e.ActivationContext;
            Logger.AddInternalState(log, "Application identity=" + text);
            Logger.AddInternalState(log, "Subscription identity=" + ((this._subId != null) ? this._subId.ToString() : "null"));
            Logger.AddInternalState(log, "IsCached=" + this._isCached.ToString());
            if (this._isCached)
            {
                this._rawDeploymentManifest = e.ActivationContext.DeploymentManifestBytes;
                this._rawApplicationManifest = e.ActivationContext.ApplicationManifestBytes;
            }
            this._activationDescription = activationDescription;
            this._version = this._activationDescription.AppId.DeploymentIdentity.Version;
            this._support = this._activationDescription.DeployManifest.Description.SupportUri;
        }

        private static XmlReader ManifestToXml(byte[] rawManifest)
        {
            if (rawManifest == null)
            {
                return null;
            }
            return new XmlTextReader(new MemoryStream(rawManifest));
        }

        public System.ActivationContext ActivationContext
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._actContext;
            }
        }

        public System.ApplicationIdentity ApplicationIdentity
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._applicationIdentity;
            }
        }

        public XmlReader ApplicationManifest
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return ManifestToXml(this.RawApplicationManifest);
            }
        }

        public XmlReader DeploymentManifest
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return ManifestToXml(this.RawDeploymentManifest);
            }
        }

        public bool IsCached
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._isCached;
            }
        }

        public string LogFilePath
        {
            get
            {
                return this._logFilePath;
            }
        }

        public string ProductName
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._name;
            }
        }

        private byte[] RawApplicationManifest
        {
            get
            {
                if (this._rawApplicationManifest == null)
                {
                    this._rawApplicationManifest = this._activationDescription.AppManifest.RawXmlBytes;
                }
                return this._rawApplicationManifest;
            }
        }

        private byte[] RawDeploymentManifest
        {
            get
            {
                if (this._rawDeploymentManifest == null)
                {
                    this._rawDeploymentManifest = this._activationDescription.DeployManifest.RawXmlBytes;
                }
                return this._rawDeploymentManifest;
            }
        }

        public string SubscriptionIdentity
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._subId.ToString();
            }
        }

        public Uri SupportUri
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._support;
            }
        }

        public System.Version Version
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return this._version;
            }
        }
    }
}

