namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Application.Manifest;

    internal class ActivationDescription : CommitApplicationParams
    {
        private ActivationType activationType;

        public void SetApplicationManifest(AssemblyManifest manifest, Uri manifestUri, string manifestPath)
        {
            base.AppManifest = manifest;
            base.AppSourceUri = manifestUri;
            base.AppManifestPath = manifestPath;
            if (base.AppManifest.EntryPoints[0].CustomHostSpecified)
            {
                base.appType = AppType.CustomHostSpecified;
            }
            if (base.AppManifest.EntryPoints[0].CustomUX)
            {
                base.appType = AppType.CustomUX;
            }
        }

        public void SetDeploymentManifest(AssemblyManifest manifest, Uri manifestUri, string manifestPath)
        {
            base.DeploySourceUri = manifestUri;
            base.DeployManifest = manifest;
            base.DeployManifestPath = manifestPath;
        }

        public ActivationContext ToActivationContext()
        {
            ApplicationIdentity identity = base.AppId.ToApplicationIdentity();
            string[] manifestPaths = new string[] { base.DeployManifestPath, base.AppManifestPath };
            return ActivationContext.CreatePartialActivationContext(identity, manifestPaths);
        }

        public string ToAppCodebase()
        {
            Uri uri = ((base.DeploySourceUri.Query != null) && (base.DeploySourceUri.Query.Length > 0)) ? new Uri(base.DeploySourceUri.GetLeftPart(UriPartial.Path)) : base.DeploySourceUri;
            return uri.AbsoluteUri;
        }

        public ActivationType ActType
        {
            get
            {
                return this.activationType;
            }
            set
            {
                this.activationType = value;
            }
        }
    }
}

