namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.IO;

    public class FindAppConfigFile : TaskExtension
    {
        private const string appConfigFile = "app.config";
        private ITaskItem appConfigFileFound;
        private ITaskItem[] primaryList;
        private ITaskItem[] secondaryList;
        private string targetPath;

        private bool ConsultLists(bool matchWholeItemSpec)
        {
            for (int i = this.PrimaryList.Length - 1; i >= 0; i--)
            {
                if (this.IsMatchingItem(this.PrimaryList[i], matchWholeItemSpec))
                {
                    return true;
                }
            }
            for (int j = this.SecondaryList.Length - 1; j >= 0; j--)
            {
                if (this.IsMatchingItem(this.SecondaryList[j], matchWholeItemSpec))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Execute()
        {
            return (this.ConsultLists(true) || (this.ConsultLists(false) || true));
        }

        private bool IsMatchingItem(ITaskItem item, bool matchWholeItemSpec)
        {
            try
            {
                string a = matchWholeItemSpec ? item.ItemSpec : Path.GetFileName(item.ItemSpec);
                if (string.Equals(a, "app.config", StringComparison.OrdinalIgnoreCase))
                {
                    this.appConfigFileFound = item;
                    this.appConfigFileFound.SetMetadata("OriginalItemSpec", item.ItemSpec);
                    this.appConfigFileFound.SetMetadata("TargetPath", this.TargetPath);
                    base.Log.LogMessageFromResources(MessageImportance.Low, "FindInList.Found", new object[] { this.appConfigFileFound.ItemSpec });
                    return true;
                }
            }
            catch (ArgumentException exception)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "FindInList.InvalidPath", new object[] { item.ItemSpec, exception.Message });
            }
            return false;
        }

        [Output]
        public ITaskItem AppConfigFile
        {
            get
            {
                return this.appConfigFileFound;
            }
            set
            {
                this.appConfigFileFound = value;
            }
        }

        [Required]
        public ITaskItem[] PrimaryList
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.primaryList, "primaryList");
                return this.primaryList;
            }
            set
            {
                this.primaryList = value;
            }
        }

        [Required]
        public ITaskItem[] SecondaryList
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.secondaryList, "secondaryList");
                return this.secondaryList;
            }
            set
            {
                this.secondaryList = value;
            }
        }

        [Required]
        public string TargetPath
        {
            get
            {
                return this.targetPath;
            }
            set
            {
                this.targetPath = value;
            }
        }
    }
}

