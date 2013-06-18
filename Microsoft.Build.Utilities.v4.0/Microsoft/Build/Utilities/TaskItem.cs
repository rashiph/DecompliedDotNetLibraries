namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Collections;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Security.Permissions;

    public sealed class TaskItem : MarshalByRefObject, ITaskItem2, ITaskItem
    {
        private string itemSpec;
        private CopyOnWriteDictionary<string, string> itemSpecModifiers;
        private CopyOnWriteDictionary<string, string> metadata;

        public TaskItem()
        {
            this.itemSpec = string.Empty;
        }

        public TaskItem(ITaskItem sourceItem)
        {
            ErrorUtilities.VerifyThrowArgumentNull(sourceItem, "sourceItem");
            ITaskItem2 item = sourceItem as ITaskItem2;
            if (item == null)
            {
                this.itemSpec = sourceItem.ItemSpec;
            }
            else
            {
                this.itemSpec = item.EvaluatedIncludeEscaped;
            }
            sourceItem.CopyMetadataTo(this);
        }

        public TaskItem(string itemSpec)
        {
            ErrorUtilities.VerifyThrowArgumentNull(itemSpec, "itemSpec");
            this.itemSpec = itemSpec;
        }

        public TaskItem(string itemSpec, IDictionary itemMetadata) : this(itemSpec)
        {
            ErrorUtilities.VerifyThrowArgumentNull(itemMetadata, "itemMetadata");
            if (itemMetadata.Count > 0)
            {
                this.metadata = new CopyOnWriteDictionary<string, string>(MSBuildNameIgnoreCaseComparer.Default);
                foreach (DictionaryEntry entry in itemMetadata)
                {
                    string key = (string) entry.Key;
                    if (!FileUtilities.ItemSpecModifiers.IsDerivableItemSpecModifier(key))
                    {
                        this.metadata[key] = (string) entry.Value;
                    }
                }
            }
        }

        public IDictionary CloneCustomMetadata()
        {
            CopyOnWriteDictionary<string, string> dictionary = new CopyOnWriteDictionary<string, string>(MSBuildNameIgnoreCaseComparer.Default);
            if (this.metadata != null)
            {
                foreach (KeyValuePair<string, string> pair in this.metadata)
                {
                    dictionary.Add(pair.Key, EscapingUtilities.UnescapeAll(pair.Value));
                }
            }
            return dictionary;
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            ErrorUtilities.VerifyThrowArgumentNull(destinationItem, "destinationItem");
            string metadata = destinationItem.GetMetadata("OriginalItemSpec");
            ITaskItem2 item = destinationItem as ITaskItem2;
            if (this.metadata != null)
            {
                TaskItem item2 = destinationItem as TaskItem;
                if ((item2 != null) && (item2.metadata == null))
                {
                    item2.metadata = this.metadata.Clone();
                }
                else
                {
                    foreach (KeyValuePair<string, string> pair in this.metadata)
                    {
                        if (item != null)
                        {
                            if (string.IsNullOrEmpty(item.GetMetadataValueEscaped(pair.Key)))
                            {
                                item.SetMetadata(pair.Key, pair.Value);
                            }
                        }
                        else if (string.IsNullOrEmpty(destinationItem.GetMetadata(pair.Key)))
                        {
                            destinationItem.SetMetadata(pair.Key, EscapingUtilities.Escape(pair.Value));
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(metadata))
            {
                if (item != null)
                {
                    item.SetMetadata("OriginalItemSpec", ((ITaskItem2) this).EvaluatedIncludeEscaped);
                }
                else
                {
                    destinationItem.SetMetadata("OriginalItemSpec", EscapingUtilities.Escape(this.ItemSpec));
                }
            }
        }

        public string GetMetadata(string metadataName)
        {
            return EscapingUtilities.UnescapeAll(((ITaskItem2) this).GetMetadataValueEscaped(metadataName));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        IDictionary ITaskItem2.CloneCustomMetadataEscaped()
        {
            if (this.metadata == null)
            {
                return new CopyOnWriteDictionary<string, string>(MSBuildNameIgnoreCaseComparer.Default);
            }
            return this.metadata.Clone();
        }

        string ITaskItem2.GetMetadataValueEscaped(string metadataName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(metadataName, "metadataName");
            string str = null;
            if (FileUtilities.ItemSpecModifiers.IsDerivableItemSpecModifier(metadataName))
            {
                str = FileUtilities.ItemSpecModifiers.GetItemSpecModifier(null, this.itemSpec, metadataName, ref this.itemSpecModifiers);
            }
            else if (this.metadata != null)
            {
                this.metadata.TryGetValue(metadataName, out str);
            }
            if (str != null)
            {
                return str;
            }
            return string.Empty;
        }

        void ITaskItem2.SetMetadataValueLiteral(string metadataName, string metadataValue)
        {
            this.SetMetadata(metadataName, EscapingUtilities.Escape(metadataValue));
        }

        public static explicit operator string(TaskItem taskItemToCast)
        {
            ErrorUtilities.VerifyThrowArgumentNull(taskItemToCast, "taskItemToCast");
            return taskItemToCast.ItemSpec;
        }

        public void RemoveMetadata(string metadataName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(metadataName, "metadataName");
            ErrorUtilities.VerifyThrowArgument(!FileUtilities.ItemSpecModifiers.IsItemSpecModifier(metadataName), "Shared.CannotChangeItemSpecModifiers", metadataName);
            if (this.metadata != null)
            {
                this.metadata.Remove(metadataName);
            }
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            ErrorUtilities.VerifyThrowArgumentLength(metadataName, "metadataName");
            ErrorUtilities.VerifyThrowArgumentNull(metadataValue, "metadataValue");
            ErrorUtilities.VerifyThrowArgument(!FileUtilities.ItemSpecModifiers.IsDerivableItemSpecModifier(metadataName), "Shared.CannotChangeItemSpecModifiers", metadataName);
            this.metadata = this.metadata ?? new CopyOnWriteDictionary<string, string>(MSBuildNameIgnoreCaseComparer.Default);
            this.metadata[metadataName] = metadataValue;
        }

        public override string ToString()
        {
            return this.itemSpec;
        }

        public string ItemSpec
        {
            get
            {
                if (this.itemSpec != null)
                {
                    return EscapingUtilities.UnescapeAll(this.itemSpec);
                }
                return string.Empty;
            }
            set
            {
                ErrorUtilities.VerifyThrowArgumentNull(value, "ItemSpec");
                this.itemSpec = value;
                this.itemSpecModifiers = null;
            }
        }

        public int MetadataCount
        {
            get
            {
                int num = (this.metadata == null) ? 0 : this.metadata.Count;
                return (num + FileUtilities.ItemSpecModifiers.All.Length);
            }
        }

        public ICollection MetadataNames
        {
            get
            {
                List<string> list = new List<string>((this.metadata == null) ? ReadOnlyEmptyList<string>.Instance : this.metadata.Keys);
                list.AddRange(FileUtilities.ItemSpecModifiers.All);
                return list;
            }
        }

        string ITaskItem2.EvaluatedIncludeEscaped
        {
            get
            {
                return this.itemSpec;
            }
            set
            {
                this.itemSpec = value;
            }
        }
    }
}

