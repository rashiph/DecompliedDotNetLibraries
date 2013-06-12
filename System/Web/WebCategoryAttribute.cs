namespace System.Web
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class WebCategoryAttribute : CategoryAttribute
    {
        internal WebCategoryAttribute(string category) : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            string localizedString = base.GetLocalizedString(value);
            if (localizedString == null)
            {
                localizedString = System.Web.SR.GetString("Category_" + value);
            }
            return localizedString;
        }

        public override object TypeId
        {
            get
            {
                return typeof(CategoryAttribute);
            }
        }
    }
}

