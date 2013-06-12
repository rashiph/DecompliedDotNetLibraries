namespace System.Collections
{
    using System;

    [Serializable]
    internal class CompatibleComparer : IEqualityComparer
    {
        private IComparer _comparer;
        private IHashCodeProvider _hcp;

        internal CompatibleComparer(IComparer comparer, IHashCodeProvider hashCodeProvider)
        {
            this._comparer = comparer;
            this._hcp = hashCodeProvider;
        }

        public int Compare(object a, object b)
        {
            if (a == b)
            {
                return 0;
            }
            if (a == null)
            {
                return -1;
            }
            if (b == null)
            {
                return 1;
            }
            if (this._comparer != null)
            {
                return this._comparer.Compare(a, b);
            }
            IComparable comparable = a as IComparable;
            if (comparable == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_ImplementIComparable"));
            }
            return comparable.CompareTo(b);
        }

        public bool Equals(object a, object b)
        {
            return (this.Compare(a, b) == 0);
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (this._hcp != null)
            {
                return this._hcp.GetHashCode(obj);
            }
            return obj.GetHashCode();
        }

        internal IComparer Comparer
        {
            get
            {
                return this._comparer;
            }
        }

        internal IHashCodeProvider HashCodeProvider
        {
            get
            {
                return this._hcp;
            }
        }
    }
}

