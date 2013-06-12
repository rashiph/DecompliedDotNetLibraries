namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class SerializationStore : IDisposable
    {
        protected SerializationStore()
        {
        }

        public abstract void Close();
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
        }

        public abstract void Save(Stream stream);
        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        public abstract ICollection Errors { get; }
    }
}

