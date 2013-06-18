namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Application.Manifest;

    internal class SubscriptionState
    {
        private bool _stateIsValid;
        private DefinitionIdentity _subId;
        private System.Deployment.Application.SubscriptionStore _subStore;
        private SubscriptionStateInternal state;

        public SubscriptionState(System.Deployment.Application.SubscriptionStore subStore, DefinitionIdentity subId)
        {
            this.Initialize(subStore, subId);
        }

        public SubscriptionState(System.Deployment.Application.SubscriptionStore subStore, AssemblyManifest deployment)
        {
            this.Initialize(subStore, deployment.Identity.ToSubscriptionId());
        }

        private void Initialize(System.Deployment.Application.SubscriptionStore subStore, DefinitionIdentity subId)
        {
            this._subStore = subStore;
            this._subId = subId;
            this.Invalidate();
        }

        public void Invalidate()
        {
            this._stateIsValid = false;
        }

        public override string ToString()
        {
            this.Validate();
            return this.state.ToString();
        }

        private void Validate()
        {
            if (!this._stateIsValid)
            {
                this.state = this._subStore.GetSubscriptionStateInternal(this);
                this._stateIsValid = true;
            }
        }

        public AppType appType
        {
            get
            {
                this.Validate();
                return this.state.appType;
            }
        }

        public AssemblyManifest CurrentApplicationManifest
        {
            get
            {
                this.Validate();
                return this.state.CurrentApplicationManifest;
            }
        }

        public Uri CurrentApplicationSourceUri
        {
            get
            {
                this.Validate();
                return this.state.CurrentApplicationSourceUri;
            }
        }

        public DefinitionAppId CurrentBind
        {
            get
            {
                this.Validate();
                return this.state.CurrentBind;
            }
        }

        public DefinitionIdentity CurrentDeployment
        {
            get
            {
                this.Validate();
                return this.state.CurrentDeployment;
            }
        }

        public AssemblyManifest CurrentDeploymentManifest
        {
            get
            {
                this.Validate();
                return this.state.CurrentDeploymentManifest;
            }
        }

        public Uri CurrentDeploymentSourceUri
        {
            get
            {
                this.Validate();
                return this.state.CurrentDeploymentSourceUri;
            }
        }

        public Uri DeploymentProviderUri
        {
            get
            {
                this.Validate();
                return this.state.DeploymentProviderUri;
            }
        }

        public string EffectiveCertificatePublicKeyToken
        {
            get
            {
                if ((this.CurrentApplicationManifest != null) && this.CurrentApplicationManifest.UseManifestForTrust)
                {
                    return this.CurrentApplicationManifest.Identity.PublicKeyToken;
                }
                if (this.CurrentDeploymentManifest == null)
                {
                    return null;
                }
                return this.CurrentDeploymentManifest.Identity.PublicKeyToken;
            }
        }

        public Description EffectiveDescription
        {
            get
            {
                if ((this.CurrentApplicationManifest != null) && this.CurrentApplicationManifest.UseManifestForTrust)
                {
                    return this.CurrentApplicationManifest.Description;
                }
                if (this.CurrentDeploymentManifest == null)
                {
                    return null;
                }
                return this.CurrentDeploymentManifest.Description;
            }
        }

        public DefinitionIdentity ExcludedDeployment
        {
            get
            {
                this.Validate();
                return this.state.ExcludedDeployment;
            }
        }

        public bool IsInstalled
        {
            get
            {
                this.Validate();
                return this.state.IsInstalled;
            }
        }

        public bool IsShellVisible
        {
            get
            {
                this.Validate();
                return this.state.IsShellVisible;
            }
        }

        public DateTime LastCheckTime
        {
            get
            {
                this.Validate();
                return this.state.LastCheckTime;
            }
        }

        public Version MinimumRequiredVersion
        {
            get
            {
                this.Validate();
                return this.state.MinimumRequiredVersion;
            }
        }

        public DefinitionAppId PendingBind
        {
            get
            {
                this.Validate();
                return this.state.PendingBind;
            }
        }

        public DefinitionIdentity PendingDeployment
        {
            get
            {
                this.Validate();
                return this.state.PendingDeployment;
            }
        }

        public DefinitionIdentity PKTGroupId
        {
            get
            {
                DefinitionIdentity identity = (DefinitionIdentity) this._subId.Clone();
                identity["publicKeyToken"] = null;
                return identity;
            }
        }

        public AssemblyManifest PreviousApplicationManifest
        {
            get
            {
                this.Validate();
                return this.state.PreviousApplicationManifest;
            }
        }

        public DefinitionAppId PreviousBind
        {
            get
            {
                this.Validate();
                return this.state.PreviousBind;
            }
        }

        public DefinitionIdentity RollbackDeployment
        {
            get
            {
                this.Validate();
                return this.state.RollbackDeployment;
            }
        }

        public DefinitionIdentity SubscriptionId
        {
            get
            {
                return this._subId;
            }
        }

        public System.Deployment.Application.SubscriptionStore SubscriptionStore
        {
            get
            {
                return this._subStore;
            }
        }

        public DefinitionIdentity UpdateSkippedDeployment
        {
            get
            {
                this.Validate();
                return this.state.UpdateSkippedDeployment;
            }
        }

        public DateTime UpdateSkipTime
        {
            get
            {
                this.Validate();
                return this.state.UpdateSkipTime;
            }
        }
    }
}

