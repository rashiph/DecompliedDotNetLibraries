namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class ApplicationIdentity
    {
        private readonly AssemblyIdentity applicationManifestIdentity;
        private readonly AssemblyIdentity deployManifestIdentity;
        private readonly string url;

        public ApplicationIdentity(string url, AssemblyIdentity deployManifestIdentity, AssemblyIdentity applicationManifestIdentity)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (deployManifestIdentity == null)
            {
                throw new ArgumentNullException("deployManifestIdentity");
            }
            if (applicationManifestIdentity == null)
            {
                throw new ArgumentNullException("applicationManifestIdentity");
            }
            this.url = url;
            this.deployManifestIdentity = deployManifestIdentity;
            this.applicationManifestIdentity = applicationManifestIdentity;
        }

        public ApplicationIdentity(string url, string deployManifestPath, string applicationManifestPath)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            if (string.IsNullOrEmpty(deployManifestPath))
            {
                throw new ArgumentNullException("deployManifestPath");
            }
            if (string.IsNullOrEmpty(applicationManifestPath))
            {
                throw new ArgumentNullException("applicationManifestPath");
            }
            this.url = url;
            this.deployManifestIdentity = AssemblyIdentity.FromManifest(deployManifestPath);
            this.applicationManifestIdentity = AssemblyIdentity.FromManifest(applicationManifestPath);
        }

        public override string ToString()
        {
            string fullName = string.Empty;
            if (this.deployManifestIdentity != null)
            {
                fullName = this.deployManifestIdentity.GetFullName(AssemblyIdentity.FullNameFlags.ProcessorArchitecture);
            }
            string str2 = string.Empty;
            if (this.applicationManifestIdentity != null)
            {
                str2 = this.applicationManifestIdentity.GetFullName(AssemblyIdentity.FullNameFlags.All);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}#{1}/{2}", new object[] { this.url, fullName, str2 });
        }
    }
}

