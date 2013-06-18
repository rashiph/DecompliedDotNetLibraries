namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.IO;

    public class FindInList : TaskExtension
    {
        private bool caseSensitive;
        private bool findLastMatch;
        private ITaskItem itemFound;
        private string itemSpecToFind;
        private ITaskItem[] list;
        private bool matchFileNameOnly;

        public override bool Execute()
        {
            StringComparison ordinal;
            if (this.caseSensitive)
            {
                ordinal = StringComparison.Ordinal;
            }
            else
            {
                ordinal = StringComparison.OrdinalIgnoreCase;
            }
            if (!this.FindLastMatch)
            {
                foreach (ITaskItem item in this.List)
                {
                    if (this.IsMatchingItem(ordinal, item))
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int i = this.List.Length - 1; i >= 0; i--)
                {
                    if (this.IsMatchingItem(ordinal, this.List[i]))
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        private bool IsMatchingItem(StringComparison comparison, ITaskItem item)
        {
            try
            {
                string a = this.MatchFileNameOnly ? Path.GetFileName(item.ItemSpec) : item.ItemSpec;
                if (string.Equals(a, this.itemSpecToFind, comparison))
                {
                    this.ItemFound = item;
                    base.Log.LogMessageFromResources(MessageImportance.Low, "FindInList.Found", new object[] { item.ItemSpec });
                    return true;
                }
            }
            catch (ArgumentException exception)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "FindInList.InvalidPath", new object[] { item.ItemSpec, exception.Message });
            }
            return false;
        }

        public bool CaseSensitive
        {
            get
            {
                return this.caseSensitive;
            }
            set
            {
                this.caseSensitive = value;
            }
        }

        public bool FindLastMatch
        {
            get
            {
                return this.findLastMatch;
            }
            set
            {
                this.findLastMatch = value;
            }
        }

        [Output]
        public ITaskItem ItemFound
        {
            get
            {
                return this.itemFound;
            }
            set
            {
                this.itemFound = value;
            }
        }

        [Required]
        public string ItemSpecToFind
        {
            get
            {
                return this.itemSpecToFind;
            }
            set
            {
                this.itemSpecToFind = value;
            }
        }

        [Required]
        public ITaskItem[] List
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.list, "list");
                return this.list;
            }
            set
            {
                this.list = value;
            }
        }

        public bool MatchFileNameOnly
        {
            get
            {
                return this.matchFileNameOnly;
            }
            set
            {
                this.matchFileNameOnly = value;
            }
        }
    }
}

