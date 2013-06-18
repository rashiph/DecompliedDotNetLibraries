namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;

    [Serializable]
    internal sealed class DirectoryLocalQuery : IDirectoryOperation
    {
        internal string m_name;
        internal DirectoryQueryOperation m_operation;
        internal string m_value;

        public DirectoryLocalQuery(string name, string value, DirectoryQueryOperation operation)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.m_name = name;
            this.m_value = value;
            this.m_operation = operation;
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
            using (DirectorySearcher searcher = new DirectorySearcher(currentEntry))
            {
                string str = "(";
                string str2 = "";
                string str3 = ")";
                switch (this.m_operation)
                {
                    case DirectoryQueryOperation.Equal:
                        str2 = "=";
                        break;

                    case DirectoryQueryOperation.NotEqual:
                        str = "(!(";
                        str2 = "=";
                        str3 = "))";
                        break;
                }
                searcher.Filter = str + this.m_name + str2 + this.m_value + str3;
                foreach (SearchResult result in searcher.FindAll())
                {
                    response.Add(result.GetDirectoryEntry());
                }
            }
        }
    }
}

