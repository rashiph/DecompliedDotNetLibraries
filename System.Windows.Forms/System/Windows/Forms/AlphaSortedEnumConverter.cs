namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal class AlphaSortedEnumConverter : EnumConverter
    {
        public AlphaSortedEnumConverter(System.Type type) : base(type)
        {
        }

        protected override IComparer Comparer
        {
            get
            {
                return EnumValAlphaComparer.Default;
            }
        }
    }
}

