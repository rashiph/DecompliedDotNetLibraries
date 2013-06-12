namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Globalization;

    [Serializable]
    internal class CompatibleComparer : IEqualityComparer
    {
        private IComparer _comparer;
        private IHashCodeProvider _hcp;
        private static IComparer defaultComparer;
        private static IHashCodeProvider defaultHashProvider;

        internal CompatibleComparer(IComparer comparer, IHashCodeProvider hashCodeProvider)
        {
            this._comparer = comparer;
            this._hcp = hashCodeProvider;
        }

        public bool Equals(object a, object b)
        {
            if (a == b)
            {
                return true;
            }
            if ((a == null) || (b == null))
            {
                return false;
            }
            try
            {
                if (this._comparer != null)
                {
                    return (this._comparer.Compare(a, b) == 0);
                }
                IComparable comparable = a as IComparable;
                if (comparable != null)
                {
                    return (comparable.CompareTo(b) == 0);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
            return a.Equals(b);
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

        public IComparer Comparer
        {
            get
            {
                return this._comparer;
            }
        }

        public static IComparer DefaultComparer
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);
                }
                return defaultComparer;
            }
        }

        public static IHashCodeProvider DefaultHashCodeProvider
        {
            get
            {
                if (defaultHashProvider == null)
                {
                    defaultHashProvider = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
                }
                return defaultHashProvider;
            }
        }

        public IHashCodeProvider HashCodeProvider
        {
            get
            {
                return this._hcp;
            }
        }
    }
}

