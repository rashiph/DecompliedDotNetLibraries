namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;

    public class AssignCulture : TaskExtension
    {
        private ITaskItem[] assignedFiles;
        private ITaskItem[] assignedFilesWithCulture;
        private ITaskItem[] assignedFilesWithNoCulture;
        private ITaskItem[] cultureNeutralAssignedFiles;
        private ITaskItem[] files = new TaskItem[0];

        public override bool Execute()
        {
            this.assignedFiles = new TaskItem[this.Files.Length];
            this.cultureNeutralAssignedFiles = new TaskItem[this.Files.Length];
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            bool flag = true;
            for (int i = 0; i < this.Files.Length; i++)
            {
                try
                {
                    this.AssignedFiles[i] = new TaskItem(this.Files[i]);
                    string metadata = this.AssignedFiles[i].GetMetadata("DependentUpon");
                    Culture.ItemCultureInfo itemCultureInfo = Culture.GetItemCultureInfo(this.AssignedFiles[i].ItemSpec, metadata);
                    if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                    {
                        this.AssignedFiles[i].SetMetadata("Culture", itemCultureInfo.culture);
                        this.AssignedFiles[i].SetMetadata("WithCulture", "true");
                        list.Add(this.AssignedFiles[i]);
                    }
                    else
                    {
                        list2.Add(this.AssignedFiles[i]);
                        this.AssignedFiles[i].SetMetadata("WithCulture", "false");
                    }
                    this.CultureNeutralAssignedFiles[i] = new TaskItem(this.AssignedFiles[i]);
                    this.CultureNeutralAssignedFiles[i].ItemSpec = itemCultureInfo.cultureNeutralFilename;
                    base.Log.LogMessageFromResources(MessageImportance.Low, "AssignCulture.Comment", new object[] { this.AssignedFiles[i].GetMetadata("Culture"), this.AssignedFiles[i].ItemSpec });
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("AssignCulture.CannotExtractCulture", new object[] { this.Files[i].ItemSpec, exception.Message });
                    flag = false;
                }
            }
            this.assignedFilesWithCulture = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            this.assignedFilesWithNoCulture = (ITaskItem[]) list2.ToArray(typeof(ITaskItem));
            return flag;
        }

        [Output]
        public ITaskItem[] AssignedFiles
        {
            get
            {
                return this.assignedFiles;
            }
        }

        [Output]
        public ITaskItem[] AssignedFilesWithCulture
        {
            get
            {
                return this.assignedFilesWithCulture;
            }
        }

        [Output]
        public ITaskItem[] AssignedFilesWithNoCulture
        {
            get
            {
                return this.assignedFilesWithNoCulture;
            }
        }

        [Output]
        public ITaskItem[] CultureNeutralAssignedFiles
        {
            get
            {
                return this.cultureNeutralAssignedFiles;
            }
        }

        [Required]
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
    }
}

