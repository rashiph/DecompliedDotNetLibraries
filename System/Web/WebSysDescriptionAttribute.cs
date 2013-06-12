namespace System.Web
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All)]
    internal class WebSysDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        internal WebSysDescriptionAttribute(string description) : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DescriptionValue = System.Web.SR.GetString(base.Description);
                }
                return base.Description;
            }
        }

        public override object TypeId
        {
            get
            {
                return typeof(DescriptionAttribute);
            }
        }
    }
}

