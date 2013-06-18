namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Globalization;

    internal class EnumValAlphaComparer : IComparer
    {
        internal static readonly EnumValAlphaComparer Default = new EnumValAlphaComparer();
        private CompareInfo m_compareInfo = CultureInfo.InvariantCulture.CompareInfo;

        internal EnumValAlphaComparer()
        {
        }

        public int Compare(object a, object b)
        {
            return this.m_compareInfo.Compare(a.ToString(), b.ToString());
        }
    }
}

