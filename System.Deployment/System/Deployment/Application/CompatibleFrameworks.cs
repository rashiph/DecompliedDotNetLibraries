namespace System.Deployment.Application
{
    using System;
    using System.Collections.Generic;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal.Isolation.Manifest;

    [Serializable]
    public class CompatibleFrameworks
    {
        private readonly CompatibleFramework[] _frameworks;
        private readonly Uri _supportUrl;

        internal CompatibleFrameworks(System.Deployment.Internal.Isolation.Manifest.CompatibleFrameworksMetadataEntry compatibleFrameworksMetadataEntry, CompatibleFramework[] frameworks)
        {
            this._supportUrl = AssemblyManifest.UriFromMetadataEntry(compatibleFrameworksMetadataEntry.SupportUrl, "Ex_CompatibleFrameworksSupportUrlNotValid");
            this._frameworks = frameworks;
        }

        public IList<CompatibleFramework> Frameworks
        {
            get
            {
                return Array.AsReadOnly<CompatibleFramework>(this._frameworks);
            }
        }

        public Uri SupportUrl
        {
            get
            {
                return this._supportUrl;
            }
        }
    }
}

