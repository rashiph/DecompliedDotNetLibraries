namespace System.Data
{
    using System;
    using System.Collections;
    using System.Configuration;

    internal sealed class LocalDBInstancesCollection : ConfigurationElementCollection
    {
        private static readonly TrimOrdinalIgnoreCaseStringComparer s_comparer = new TrimOrdinalIgnoreCaseStringComparer();

        internal LocalDBInstancesCollection() : base(s_comparer)
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new LocalDBInstanceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LocalDBInstanceElement) element).Name;
        }

        private class TrimOrdinalIgnoreCaseStringComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string str2 = x as string;
                if (str2 != null)
                {
                    x = str2.Trim();
                }
                string str = y as string;
                if (str != null)
                {
                    y = str.Trim();
                }
                return StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }
        }
    }
}

