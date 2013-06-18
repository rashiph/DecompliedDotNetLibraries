namespace System.Deployment.Application
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;

    [Serializable]
    public class CompatibleFramework
    {
        private readonly string _profile;
        private readonly string _supportedRuntime;
        private readonly string _targetVersion;

        internal CompatibleFramework(System.Deployment.Internal.Isolation.Manifest.CompatibleFrameworkEntry compatibleFrameworkEntry)
        {
            this._supportedRuntime = compatibleFrameworkEntry.SupportedRuntime;
            this._profile = compatibleFrameworkEntry.Profile;
            this._targetVersion = compatibleFrameworkEntry.TargetVersion;
        }

        public string Profile
        {
            get
            {
                return this._profile;
            }
        }

        public string SupportedRuntime
        {
            get
            {
                return this._supportedRuntime;
            }
        }

        public string TargetVersion
        {
            get
            {
                return this._targetVersion;
            }
        }
    }
}

