namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Application.Manifest;
    using System.Text;

    internal class SubscriptionStateInternal
    {
        public AppType appType;
        public DefinitionIdentity CurrentApplication;
        public AssemblyManifest CurrentApplicationManifest;
        public Uri CurrentApplicationSourceUri;
        public DefinitionAppId CurrentBind;
        public DefinitionIdentity CurrentDeployment;
        public AssemblyManifest CurrentDeploymentManifest;
        public Uri CurrentDeploymentSourceUri;
        public Uri DeploymentProviderUri;
        public DefinitionIdentity ExcludedDeployment;
        public bool IsInstalled;
        public bool IsShellVisible;
        public DateTime LastCheckTime;
        public Version MinimumRequiredVersion;
        public DefinitionAppId PendingBind;
        public DefinitionIdentity PendingDeployment;
        public DefinitionIdentity PreviousApplication;
        public AssemblyManifest PreviousApplicationManifest;
        public DefinitionAppId PreviousBind;
        public DefinitionIdentity RollbackDeployment;
        public DefinitionIdentity UpdateSkippedDeployment;
        public DateTime UpdateSkipTime;

        public SubscriptionStateInternal()
        {
            this.Reset();
        }

        public SubscriptionStateInternal(SubscriptionState subState)
        {
            this.IsInstalled = subState.IsInstalled;
            this.IsShellVisible = subState.IsShellVisible;
            this.CurrentBind = subState.CurrentBind;
            this.PreviousBind = subState.PreviousBind;
            this.PendingBind = subState.PreviousBind;
            this.PendingDeployment = subState.PendingDeployment;
            this.ExcludedDeployment = subState.ExcludedDeployment;
            this.DeploymentProviderUri = subState.DeploymentProviderUri;
            this.MinimumRequiredVersion = subState.MinimumRequiredVersion;
            this.LastCheckTime = subState.LastCheckTime;
            this.UpdateSkippedDeployment = subState.UpdateSkippedDeployment;
            this.UpdateSkipTime = subState.UpdateSkipTime;
            this.appType = subState.appType;
        }

        public void Reset()
        {
            this.IsInstalled = this.IsShellVisible = false;
            this.CurrentBind = this.PreviousBind = (DefinitionAppId) (this.PendingBind = null);
            this.ExcludedDeployment = (DefinitionIdentity) (this.PendingDeployment = null);
            this.DeploymentProviderUri = null;
            this.MinimumRequiredVersion = null;
            this.LastCheckTime = DateTime.MinValue;
            this.UpdateSkippedDeployment = null;
            this.UpdateSkipTime = DateTime.MinValue;
            this.CurrentDeployment = null;
            this.RollbackDeployment = null;
            this.CurrentDeploymentManifest = null;
            this.CurrentDeploymentSourceUri = null;
            this.CurrentApplication = null;
            this.CurrentApplicationManifest = null;
            this.CurrentApplicationSourceUri = null;
            this.PreviousApplication = null;
            this.PreviousApplicationManifest = null;
            this.appType = AppType.None;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("IsInstalled=" + this.IsInstalled.ToString() + "\r\n");
            builder.Append("IsShellVisible=" + this.IsShellVisible.ToString() + "\r\n");
            builder.Append("CurrentBind=" + ((this.CurrentBind != null) ? this.CurrentBind.ToString() : "null") + "\r\n");
            builder.Append("PreviousBind=" + ((this.PreviousBind != null) ? this.PreviousBind.ToString() : "null") + "\r\n");
            builder.Append("PendingBind=" + ((this.PendingBind != null) ? this.PendingBind.ToString() : "null") + "\r\n");
            builder.Append("PendingDeployment=" + ((this.PendingDeployment != null) ? this.PendingDeployment.ToString() : "null") + "\r\n");
            builder.Append("ExcludedDeployment=" + ((this.ExcludedDeployment != null) ? this.ExcludedDeployment.ToString() : "null") + "\r\n");
            builder.Append("DeploymentProviderUri=" + ((this.DeploymentProviderUri != null) ? this.DeploymentProviderUri.ToString() : "null") + "\r\n");
            builder.Append("MinimumRequiredVersion=" + ((this.MinimumRequiredVersion != null) ? this.MinimumRequiredVersion.ToString() : "null") + "\r\n");
            builder.Append("LastCheckTime=" + this.LastCheckTime.ToString() + "\r\n");
            builder.Append("UpdateSkipTime=" + this.UpdateSkipTime.ToString() + "\r\n");
            builder.Append("UpdateSkippedDeployment=" + ((this.UpdateSkippedDeployment != null) ? this.UpdateSkippedDeployment.ToString() : "null") + "\r\n");
            builder.Append("appType=" + ((ushort) this.appType) + "\r\n");
            return builder.ToString();
        }
    }
}

