namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class WinCategoryAttribute : CategoryAttribute
    {
        public WinCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            string localizedString = base.GetLocalizedString(value);
            if (localizedString == null)
            {
                localizedString = (string) System.Windows.Forms.SR.GetObject("WinFormsCategory" + value);
            }
            return localizedString;
        }
    }
}

