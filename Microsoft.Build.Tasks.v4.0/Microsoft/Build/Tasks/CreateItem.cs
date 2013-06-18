namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;

    public class CreateItem : TaskExtension
    {
        private string[] additionalMetadata;
        private ITaskItem[] exclude;
        private ITaskItem[] include;
        private bool preserveExistingMetadata;

        private ArrayList CreateOutputItems(Hashtable metadataTable, Hashtable excludeItems)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < this.Include.Length; i++)
            {
                if ((excludeItems.Count != 0) && excludeItems.ContainsKey(this.Include[i].ItemSpec))
                {
                    continue;
                }
                ITaskItem item = this.include[i];
                if (metadataTable != null)
                {
                    foreach (DictionaryEntry entry in metadataTable)
                    {
                        if (!this.preserveExistingMetadata || string.IsNullOrEmpty(item.GetMetadata((string) entry.Key)))
                        {
                            if (Microsoft.Build.Shared.FileUtilities.ItemSpecModifiers.IsItemSpecModifier((string) entry.Key))
                            {
                                base.Log.LogErrorWithCodeFromResources("CreateItem.AdditionalMetadataError", new object[] { (string) entry.Key });
                                break;
                            }
                            item.SetMetadata((string) entry.Key, (string) entry.Value);
                        }
                    }
                }
                list.Add(item);
            }
            return list;
        }

        public override bool Execute()
        {
            Hashtable hashtable;
            if (this.Include == null)
            {
                this.include = new TaskItem[0];
                return true;
            }
            this.Include = ExpandWildcards(this.Include);
            this.Exclude = ExpandWildcards(this.Exclude);
            if ((this.AdditionalMetadata == null) && (this.Exclude == null))
            {
                return true;
            }
            if (!PropertyParser.GetTable(base.Log, "AdditionalMetadata", this.AdditionalMetadata, out hashtable))
            {
                return false;
            }
            Hashtable uniqueItems = GetUniqueItems(this.Exclude);
            ArrayList list = this.CreateOutputItems(hashtable, uniqueItems);
            this.include = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
            return !base.Log.HasLoggedErrors;
        }

        private static ITaskItem[] ExpandWildcards(ITaskItem[] expand)
        {
            if (expand == null)
            {
                return null;
            }
            ArrayList list = new ArrayList();
            foreach (ITaskItem item in expand)
            {
                if (Microsoft.Build.Shared.FileMatcher.HasWildcards(item.ItemSpec))
                {
                    foreach (string str in Microsoft.Build.Shared.FileMatcher.GetFiles(null, item.ItemSpec))
                    {
                        TaskItem item2 = new TaskItem(item) {
                            ItemSpec = str
                        };
                        Microsoft.Build.Shared.FileMatcher.Result result = Microsoft.Build.Shared.FileMatcher.FileMatch(item.ItemSpec, str);
                        if ((result.isLegalFileSpec && result.isMatch) && ((result.wildcardDirectoryPart != null) && (result.wildcardDirectoryPart.Length > 0)))
                        {
                            item2.SetMetadata("RecursiveDir", result.wildcardDirectoryPart);
                        }
                        list.Add(item2);
                    }
                }
                else
                {
                    list.Add(item);
                }
            }
            return (ITaskItem[]) list.ToArray(typeof(ITaskItem));
        }

        private static Hashtable GetUniqueItems(ITaskItem[] items)
        {
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (items != null)
            {
                foreach (ITaskItem item in items)
                {
                    hashtable[item.ItemSpec] = string.Empty;
                }
            }
            return hashtable;
        }

        public string[] AdditionalMetadata
        {
            get
            {
                return this.additionalMetadata;
            }
            set
            {
                this.additionalMetadata = value;
            }
        }

        public ITaskItem[] Exclude
        {
            get
            {
                return this.exclude;
            }
            set
            {
                this.exclude = value;
            }
        }

        [Output]
        public ITaskItem[] Include
        {
            get
            {
                return this.include;
            }
            set
            {
                this.include = value;
            }
        }

        public bool PreserveExistingMetadata
        {
            get
            {
                return this.preserveExistingMetadata;
            }
            set
            {
                this.preserveExistingMetadata = value;
            }
        }
    }
}

