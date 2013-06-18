namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;

    internal class DependentOS
    {
        private readonly ushort _buildNumber;
        private readonly ushort _majorVersion;
        private readonly ushort _minorVersion;
        private readonly byte _servicePackMajor;
        private readonly byte _servicePackMinor;
        private readonly Uri _supportUrl;

        public DependentOS(System.Deployment.Internal.Isolation.Manifest.DependentOSMetadataEntry dependentOSMetadataEntry)
        {
            this._majorVersion = dependentOSMetadataEntry.MajorVersion;
            this._minorVersion = dependentOSMetadataEntry.MinorVersion;
            this._buildNumber = dependentOSMetadataEntry.BuildNumber;
            this._servicePackMajor = dependentOSMetadataEntry.ServicePackMajor;
            this._servicePackMinor = dependentOSMetadataEntry.ServicePackMinor;
            this._supportUrl = AssemblyManifest.UriFromMetadataEntry(dependentOSMetadataEntry.SupportUrl, "Ex_DependentOSSupportUrlNotValid");
        }

        public ushort BuildNumber
        {
            get
            {
                return this._buildNumber;
            }
        }

        public ushort MajorVersion
        {
            get
            {
                return this._majorVersion;
            }
        }

        public ushort MinorVersion
        {
            get
            {
                return this._minorVersion;
            }
        }

        public byte ServicePackMajor
        {
            get
            {
                return this._servicePackMajor;
            }
        }

        public byte ServicePackMinor
        {
            get
            {
                return this._servicePackMinor;
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

