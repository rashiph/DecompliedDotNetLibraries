namespace System.Collections
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public class CaseInsensitiveComparer : IComparer
    {
        private CompareInfo m_compareInfo;
        private static CaseInsensitiveComparer m_InvariantCaseInsensitiveComparer;

        public CaseInsensitiveComparer()
        {
            this.m_compareInfo = CultureInfo.CurrentCulture.CompareInfo;
        }

        public CaseInsensitiveComparer(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            this.m_compareInfo = culture.CompareInfo;
        }

        public int Compare(object a, object b)
        {
            string str = a as string;
            string str2 = b as string;
            if ((str != null) && (str2 != null))
            {
                return this.m_compareInfo.Compare(str, str2, CompareOptions.IgnoreCase);
            }
            return Comparer.Default.Compare(a, b);
        }

        public static CaseInsensitiveComparer Default
        {
            get
            {
                return new CaseInsensitiveComparer(CultureInfo.CurrentCulture);
            }
        }

        public static CaseInsensitiveComparer DefaultInvariant
        {
            get
            {
                if (m_InvariantCaseInsensitiveComparer == null)
                {
                    m_InvariantCaseInsensitiveComparer = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);
                }
                return m_InvariantCaseInsensitiveComparer;
            }
        }
    }
}

