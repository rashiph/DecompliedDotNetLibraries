namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Globalization;
    using System.IO;

    public class AssignTargetPath : TaskExtension
    {
        private ITaskItem[] assignedFiles;
        private ITaskItem[] files = new ITaskItem[0];
        private string rootFolder;

        public override bool Execute()
        {
            this.assignedFiles = new TaskItem[this.Files.Length];
            if (this.Files.Length > 0)
            {
                string strA = Microsoft.Build.Shared.FileUtilities.EnsureTrailingSlash(Path.GetFullPath(this.RootFolder));
                string currentDirectory = Directory.GetCurrentDirectory();
                bool flag = ((strA.Length - 1) == currentDirectory.Length) && (string.Compare(strA, 0, currentDirectory, 0, strA.Length - 1, StringComparison.OrdinalIgnoreCase) == 0);
                for (int i = 0; i < this.Files.Length; i++)
                {
                    string metadata = this.Files[i].GetMetadata("Link");
                    this.AssignedFiles[i] = new TaskItem(this.Files[i]);
                    string metadataValue = metadata;
                    if ((metadata == null) || (metadata.Length == 0))
                    {
                        if ((!Path.IsPathRooted(this.Files[i].ItemSpec) && !this.Files[i].ItemSpec.Contains(@".\")) && flag)
                        {
                            metadataValue = this.Files[i].ItemSpec;
                        }
                        else
                        {
                            string fullPath = Path.GetFullPath(this.Files[i].ItemSpec);
                            if (string.Compare(strA, 0, fullPath, 0, strA.Length, true, CultureInfo.CurrentCulture) == 0)
                            {
                                metadataValue = fullPath.Substring(strA.Length);
                            }
                            else
                            {
                                metadataValue = Path.GetFileName(this.Files[i].ItemSpec);
                            }
                        }
                    }
                    this.AssignedFiles[i].SetMetadata("TargetPath", metadataValue);
                }
            }
            return true;
        }

        [Output]
        public ITaskItem[] AssignedFiles
        {
            get
            {
                return this.assignedFiles;
            }
        }

        public ITaskItem[] Files
        {
            get
            {
                return this.files;
            }
            set
            {
                this.files = value;
            }
        }

        [Required]
        public string RootFolder
        {
            get
            {
                return this.rootFolder;
            }
            set
            {
                this.rootFolder = value;
            }
        }
    }
}

