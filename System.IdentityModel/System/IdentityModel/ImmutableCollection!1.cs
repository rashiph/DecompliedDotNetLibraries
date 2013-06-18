namespace System.IdentityModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class ImmutableCollection<T> : Collection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private bool isReadOnly;

        protected override void ClearItems()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
            }
            base.InsertItem(index, item);
        }

        public void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        protected override void RemoveItem(int index)
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("ObjectIsReadOnly")));
            }
            base.SetItem(index, item);
        }

        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }
    }
}

