namespace System.Web.Hosting
{
    using System;
    using System.Web.Compilation;
    using System.Web.Util;

    [Serializable]
    internal class HostingEnvironmentParameters
    {
        private System.Web.Compilation.ClientBuildManagerParameter _clientBuildManagerParameter;
        private HostingEnvironmentFlags _hostingFlags;
        private string _iisExpressVersion;
        private string _precompTargetPhysicalDir;

        public System.Web.Compilation.ClientBuildManagerParameter ClientBuildManagerParameter
        {
            get
            {
                return this._clientBuildManagerParameter;
            }
            set
            {
                this._clientBuildManagerParameter = value;
            }
        }

        public HostingEnvironmentFlags HostingFlags
        {
            get
            {
                return this._hostingFlags;
            }
            set
            {
                this._hostingFlags = value;
            }
        }

        public string IISExpressVersion
        {
            get
            {
                return this._iisExpressVersion;
            }
            set
            {
                this._iisExpressVersion = value;
            }
        }

        public string PrecompilationTargetPhysicalDirectory
        {
            get
            {
                return this._precompTargetPhysicalDir;
            }
            set
            {
                this._precompTargetPhysicalDir = FileUtil.FixUpPhysicalDirectory(value);
            }
        }
    }
}

