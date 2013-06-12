namespace System.Net.Mail
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class LinkedResourceCollection : Collection<LinkedResource>, IDisposable
    {
        private bool disposed;

        internal LinkedResourceCollection()
        {
        }

        protected override void ClearItems()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            base.ClearItems();
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                foreach (LinkedResource resource in this)
                {
                    resource.Dispose();
                }
                base.Clear();
                this.disposed = true;
            }
        }

        protected override void InsertItem(int index, LinkedResource item)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, LinkedResource item)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            base.SetItem(index, item);
        }
    }
}

