namespace System.Web.DataAccess
{
    using System;
    using System.DirectoryServices;
    using System.Web;

    internal sealed class DirectoryEntryHolder
    {
        private ImpersonationContext ctx;
        private System.DirectoryServices.DirectoryEntry entry;
        private bool opened;

        internal DirectoryEntryHolder(System.DirectoryServices.DirectoryEntry entry)
        {
            this.entry = entry;
        }

        internal void Close()
        {
            if (this.opened)
            {
                this.entry.Dispose();
                this.RestoreImpersonation();
                this.opened = false;
            }
        }

        internal void Open(HttpContext context, bool revertImpersonate)
        {
            if (!this.opened)
            {
                if (revertImpersonate)
                {
                    this.ctx = new ApplicationImpersonationContext();
                }
                else
                {
                    this.ctx = null;
                }
                this.opened = true;
            }
        }

        internal void RestoreImpersonation()
        {
            if (this.ctx != null)
            {
                this.ctx.Undo();
                this.ctx = null;
            }
        }

        internal System.DirectoryServices.DirectoryEntry DirectoryEntry
        {
            get
            {
                return this.entry;
            }
        }
    }
}

