namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Application.Manifest;
    using System.Security.Policy;

    internal class CommitApplicationParams
    {
        public string AppGroup;
        public DefinitionAppId AppId;
        public AssemblyManifest AppManifest;
        public string AppManifestPath;
        public string AppPayloadPath;
        public Uri AppSourceUri;
        public AppType appType;
        public bool CommitApp;
        public bool CommitDeploy;
        public AssemblyManifest DeployManifest;
        public string DeployManifestPath;
        public Uri DeploySourceUri;
        public bool IsConfirmed;
        public bool IsFullTrustRequested;
        public bool IsRequiredUpdate;
        public bool IsUpdate;
        public bool IsUpdateInPKTGroup;
        public DateTime TimeStamp;
        public System.Security.Policy.ApplicationTrust Trust;

        public CommitApplicationParams()
        {
            this.TimeStamp = DateTime.MinValue;
        }

        public CommitApplicationParams(CommitApplicationParams src)
        {
            this.TimeStamp = DateTime.MinValue;
            this.AppId = src.AppId;
            this.CommitApp = src.CommitApp;
            this.AppManifest = src.AppManifest;
            this.AppSourceUri = src.AppSourceUri;
            this.AppManifestPath = src.AppManifestPath;
            this.AppPayloadPath = src.AppPayloadPath;
            this.AppGroup = src.AppGroup;
            this.CommitDeploy = src.CommitDeploy;
            this.DeployManifest = src.DeployManifest;
            this.DeploySourceUri = src.DeploySourceUri;
            this.DeployManifestPath = src.DeployManifestPath;
            this.TimeStamp = src.TimeStamp;
            this.IsConfirmed = src.IsConfirmed;
            this.IsUpdate = src.IsUpdate;
            this.IsRequiredUpdate = src.IsRequiredUpdate;
            this.IsUpdateInPKTGroup = src.IsUpdateInPKTGroup;
            this.IsFullTrustRequested = src.IsFullTrustRequested;
            this.appType = src.appType;
            this.Trust = src.Trust;
        }

        public string EffectiveCertificatePublicKeyToken
        {
            get
            {
                if ((this.AppManifest != null) && this.AppManifest.UseManifestForTrust)
                {
                    return this.AppManifest.Identity.PublicKeyToken;
                }
                if (this.DeployManifest == null)
                {
                    return null;
                }
                return this.DeployManifest.Identity.PublicKeyToken;
            }
        }

        public Description EffectiveDescription
        {
            get
            {
                if ((this.AppManifest != null) && this.AppManifest.UseManifestForTrust)
                {
                    return this.AppManifest.Description;
                }
                if (this.DeployManifest == null)
                {
                    return null;
                }
                return this.DeployManifest.Description;
            }
        }
    }
}

