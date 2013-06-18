namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.DirectoryServices;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Security.Principal;

    [Serializable]
    public sealed class ActiveDirectoryRole : WorkflowRole, ISerializable, IDisposable
    {
        private string m_name;
        private List<IDirectoryOperation> m_operations;
        private DirectoryEntry m_root;

        internal ActiveDirectoryRole(DirectoryEntry rootEntry, IDirectoryOperation operation)
        {
            if (rootEntry == null)
            {
                throw new ArgumentNullException("rootEntry");
            }
            this.m_root = rootEntry;
            this.m_operations = new List<IDirectoryOperation>();
            if (operation != null)
            {
                this.m_operations.Add(operation);
            }
        }

        internal ActiveDirectoryRole(DirectoryEntry rootEntry, ICollection<IDirectoryOperation> operations)
        {
            if (rootEntry == null)
            {
                throw new ArgumentNullException("rootEntry");
            }
            this.m_root = rootEntry;
            if (operations == null)
            {
                this.m_operations = new List<IDirectoryOperation>();
            }
            else
            {
                this.m_operations = new List<IDirectoryOperation>(operations);
            }
        }

        private ActiveDirectoryRole(SerializationInfo info, StreamingContext context)
        {
            this.m_name = info.GetString("m_name");
            this.m_operations = (List<IDirectoryOperation>) info.GetValue("m_operations", typeof(List<IDirectoryOperation>));
            string path = info.GetString(@"m_root\path");
            this.m_root = new DirectoryEntry(path);
        }

        public ActiveDirectoryRole GetAllReports()
        {
            return new ActiveDirectoryRole(this.RootEntry, new List<IDirectoryOperation>(this.Operations) { new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager, 1) });
        }

        public ActiveDirectoryRole GetDirectReports()
        {
            return new ActiveDirectoryRole(this.RootEntry, new List<IDirectoryOperation>(this.Operations) { new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager) });
        }

        public ICollection<DirectoryEntry> GetEntries()
        {
            List<DirectoryEntry> list = new List<DirectoryEntry> {
                this.m_root
            };
            List<DirectoryEntry> response = new List<DirectoryEntry>();
            for (int i = 0; i < this.m_operations.Count; i++)
            {
                for (int k = 0; k < list.Count; k++)
                {
                    this.m_operations[i].GetResult(this.m_root, list[k], response);
                }
                List<DirectoryEntry> list3 = list;
                list = response;
                list3.Clear();
            }
            Dictionary<Guid, DirectoryEntry> dictionary = new Dictionary<Guid, DirectoryEntry>();
            for (int j = 0; j < list.Count; j++)
            {
                if (!dictionary.ContainsKey(list[j].Guid))
                {
                    dictionary.Add(list[j].Guid, list[j]);
                }
            }
            return dictionary.Values;
        }

        public override IList<string> GetIdentities()
        {
            List<string> list = new List<string>();
            foreach (SecurityIdentifier identifier in this.GetSecurityIdentifiers())
            {
                list.Add(identifier.Translate(typeof(NTAccount)).ToString());
            }
            return list;
        }

        public ActiveDirectoryRole GetManager()
        {
            return new ActiveDirectoryRole(this.RootEntry, new List<IDirectoryOperation>(this.Operations) { new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports) });
        }

        public ActiveDirectoryRole GetManagerialChain()
        {
            return new ActiveDirectoryRole(this.RootEntry, new List<IDirectoryOperation>(this.Operations) { new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports, 1) });
        }

        public ActiveDirectoryRole GetPeers()
        {
            ICollection<DirectoryEntry> entries = this.GetEntries();
            List<IDirectoryOperation> operations = new List<IDirectoryOperation>(this.Operations) {
                new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.DirectReports),
                new DirectoryRedirect(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, ActiveDirectoryRoleFactory.Configuration.Manager)
            };
            foreach (DirectoryEntry entry in entries)
            {
                operations.Add(new DirectoryLocalQuery(ActiveDirectoryRoleFactory.Configuration.DistinguishedName, (string) entry.Properties[ActiveDirectoryRoleFactory.Configuration.DistinguishedName][0], DirectoryQueryOperation.NotEqual));
            }
            return new ActiveDirectoryRole(this.RootEntry, operations);
        }

        public IList<SecurityIdentifier> GetSecurityIdentifiers()
        {
            List<SecurityIdentifier> list = new List<SecurityIdentifier>();
            foreach (DirectoryEntry entry in this.GetEntries())
            {
                if ((entry.Properties["objectSid"] != null) && (entry.Properties["objectSid"].Count != 0))
                {
                    list.Add(new SecurityIdentifier((byte[]) entry.Properties["objectSid"][0], 0));
                }
                else
                {
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "Unable to find 'objectSid' property for directory entry = {0}.", new object[] { entry.Path });
                }
            }
            return list;
        }

        public override bool IncludesIdentity(string identity)
        {
            if (identity != null)
            {
                foreach (string str in this.GetIdentities())
                {
                    if (string.Compare(identity, str, StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void IDisposable.Dispose()
        {
            this.m_root.Dispose();
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("m_name", this.m_name);
            info.AddValue("m_operations", this.m_operations);
            info.AddValue(@"m_root\path", this.m_root.Path);
        }

        public override string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_name = value;
            }
        }

        internal ICollection<IDirectoryOperation> Operations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_operations;
            }
        }

        public DirectoryEntry RootEntry
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_root;
            }
        }
    }
}

