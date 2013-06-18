namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;

    internal class ComInfo
    {
        private readonly string clsid;
        private readonly string componentFileName;
        private readonly string manifestFileName;
        private readonly string tlbid;

        public ComInfo(string manifestFileName, string componentFileName, string clsid, string tlbid)
        {
            this.componentFileName = componentFileName;
            this.clsid = clsid;
            this.manifestFileName = manifestFileName;
            this.tlbid = tlbid;
        }

        public string ClsId
        {
            get
            {
                return this.clsid;
            }
        }

        public string ComponentFileName
        {
            get
            {
                return this.componentFileName;
            }
        }

        public string ManifestFileName
        {
            get
            {
                return this.manifestFileName;
            }
        }

        public string TlbId
        {
            get
            {
                return this.tlbid;
            }
        }
    }
}

