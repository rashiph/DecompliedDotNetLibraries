namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
    using Microsoft.Build.Utilities;
    using System;

    public class UpdateManifest : Task
    {
        private ITaskItem applicationManifest;
        private string applicationPath;
        private ITaskItem inputManifest;
        private ITaskItem outputManifest;

        public override bool Execute()
        {
            Manifest.UpdateEntryPoint(this.InputManifest.ItemSpec, this.OutputManifest.ItemSpec, this.ApplicationPath, this.ApplicationManifest.ItemSpec);
            return true;
        }

        [Required]
        public ITaskItem ApplicationManifest
        {
            get
            {
                return this.applicationManifest;
            }
            set
            {
                this.applicationManifest = value;
            }
        }

        [Required]
        public string ApplicationPath
        {
            get
            {
                return this.applicationPath;
            }
            set
            {
                this.applicationPath = value;
            }
        }

        [Required]
        public ITaskItem InputManifest
        {
            get
            {
                return this.inputManifest;
            }
            set
            {
                this.inputManifest = value;
            }
        }

        [Output]
        public ITaskItem OutputManifest
        {
            get
            {
                return this.outputManifest;
            }
            set
            {
                this.outputManifest = value;
            }
        }
    }
}

