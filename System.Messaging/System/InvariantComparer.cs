namespace System
{
    using System.Collections;
    using System.Globalization;

    [Serializable]
    internal class InvariantComparer : IComparer
    {
        internal static readonly System.InvariantComparer Default = new System.InvariantComparer();
        private CompareInfo m_compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        internal InvariantComparer()
        {
        }

        public int Compare(object a, object b)
        {
            string str = a as string;
            string str2 = b as string;
            if ((str != null) && (str2 != null))
            {
                return this.m_compareInfo.Compare(str, str2);
            }
            return Comparer.Default.Compare(a, b);
        }
    }
}

