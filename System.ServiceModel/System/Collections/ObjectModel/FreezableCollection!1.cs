namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class FreezableCollection<T> : Collection<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private bool frozen;

        public FreezableCollection()
        {
        }

        public FreezableCollection(IList<T> list) : base(list)
        {
        }

        protected override void ClearItems()
        {
            this.ThrowIfFrozen();
            base.ClearItems();
        }

        public void Freeze()
        {
            this.frozen = true;
        }

        protected override void InsertItem(int index, T item)
        {
            this.ThrowIfFrozen();
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.ThrowIfFrozen();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            this.ThrowIfFrozen();
            base.SetItem(index, item);
        }

        private void ThrowIfFrozen()
        {
            if (this.frozen)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException("ObjectIsReadOnly"));
            }
        }

        public bool IsFrozen
        {
            get
            {
                return this.frozen;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return this.frozen;
            }
        }
    }
}

