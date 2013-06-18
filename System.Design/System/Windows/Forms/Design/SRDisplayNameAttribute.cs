namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    internal sealed class SRDisplayNameAttribute : DisplayNameAttribute
    {
        private bool replaced;

        public SRDisplayNameAttribute(string displayName) : base(displayName)
        {
        }

        public override string DisplayName
        {
            get
            {
                if (!this.replaced)
                {
                    this.replaced = true;
                    base.DisplayNameValue = System.Design.SR.GetString(base.DisplayName);
                }
                return base.DisplayName;
            }
        }
    }
}

