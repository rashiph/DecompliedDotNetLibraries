namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using System;

    public sealed class FormatUrl : TaskExtension
    {
        private string inputUrl;
        private string outputUrl;

        public override bool Execute()
        {
            if (this.inputUrl != null)
            {
                this.outputUrl = PathUtil.Format(this.inputUrl);
            }
            else
            {
                this.outputUrl = string.Empty;
            }
            return true;
        }

        public string InputUrl
        {
            get
            {
                return this.inputUrl;
            }
            set
            {
                this.inputUrl = value;
            }
        }

        [Output]
        public string OutputUrl
        {
            get
            {
                return this.outputUrl;
            }
            set
            {
                this.outputUrl = value;
            }
        }
    }
}

