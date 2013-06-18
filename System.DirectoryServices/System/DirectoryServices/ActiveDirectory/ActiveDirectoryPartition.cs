namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Security.Permissions;

    public abstract class ActiveDirectoryPartition : IDisposable
    {
        internal DirectoryContext context;
        internal DirectoryEntryManager directoryEntryMgr;
        private bool disposed;
        internal string partitionName;

        protected ActiveDirectoryPartition()
        {
        }

        internal ActiveDirectoryPartition(DirectoryContext context, string name)
        {
            this.context = context;
            this.partitionName = name;
        }

        internal void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (DirectoryEntry entry in this.directoryEntryMgr.GetCachedDirectoryEntries())
                    {
                        entry.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public abstract DirectoryEntry GetDirectoryEntry();
        public override string ToString()
        {
            return this.Name;
        }

        public string Name
        {
            get
            {
                this.CheckIfDisposed();
                return this.partitionName;
            }
        }
    }
}

