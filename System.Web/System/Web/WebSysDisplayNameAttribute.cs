namespace System.Web
{
    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Class)]
    internal sealed class WebSysDisplayNameAttribute : DisplayNameAttribute
    {
        private bool replaced;

        internal WebSysDisplayNameAttribute(string DisplayName) : base(DisplayName)
        {
        }

        public override string DisplayName
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DisplayNameValue = System.Web.SR.GetString(base.DisplayName);
                }
                return base.DisplayName;
            }
        }

        public override object TypeId
        {
            get
            {
                return typeof(DisplayNameAttribute);
            }
        }
    }
}

