namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;
    using System.Runtime;

    [Serializable]
    internal sealed class DirectoryRedirect : IDirectoryOperation
    {
        private string m_getPropertyName;
        private bool m_recursive;
        private string m_searchPropertyName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DirectoryRedirect(string getPropertyName, string searchPropertyName) : this(getPropertyName, searchPropertyName, false)
        {
        }

        public DirectoryRedirect(string getPropertyName, string searchPropertyName, bool recursive)
        {
            if (getPropertyName == null)
            {
                throw new ArgumentNullException("getPropertyName");
            }
            if (searchPropertyName == null)
            {
                throw new ArgumentNullException("searchPropertyName");
            }
            this.m_getPropertyName = getPropertyName;
            this.m_searchPropertyName = searchPropertyName;
            this.m_recursive = recursive;
        }

        private DirectorySearcher CreateSearcher(DirectoryEntry rootEntry, DirectoryEntry currentEntry)
        {
            DirectorySearcher searcher = new DirectorySearcher(rootEntry);
            PropertyValueCollection values = currentEntry.Properties[this.m_getPropertyName];
            searcher.Filter = string.Concat(new object[] { "(", this.m_searchPropertyName, "=", values[0], ")" });
            return searcher;
        }

        public void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response)
        {
            if (rootEntry == null)
            {
                throw new ArgumentNullException("rootEntry");
            }
            if (currentEntry == null)
            {
                throw new ArgumentNullException("currentEntry");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            if (!this.m_recursive)
            {
                using (DirectorySearcher searcher = this.CreateSearcher(rootEntry, currentEntry))
                {
                    foreach (SearchResult result in searcher.FindAll())
                    {
                        response.Add(result.GetDirectoryEntry());
                    }
                    return;
                }
            }
            Dictionary<Guid, DirectoryEntry> dictionary = new Dictionary<Guid, DirectoryEntry>();
            Stack<DirectoryEntry> stack = new Stack<DirectoryEntry>();
            stack.Push(currentEntry);
            while (stack.Count != 0)
            {
                DirectoryEntry entry = stack.Pop();
                using (DirectorySearcher searcher2 = this.CreateSearcher(rootEntry, entry))
                {
                    foreach (SearchResult result2 in searcher2.FindAll())
                    {
                        DirectoryEntry directoryEntry = result2.GetDirectoryEntry();
                        if (!dictionary.ContainsKey(directoryEntry.Guid))
                        {
                            dictionary.Add(directoryEntry.Guid, directoryEntry);
                        }
                        stack.Push(directoryEntry);
                    }
                    continue;
                }
            }
            response.AddRange(dictionary.Values);
        }
    }
}

