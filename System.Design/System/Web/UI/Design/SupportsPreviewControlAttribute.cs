namespace System.Web.UI.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SupportsPreviewControlAttribute : Attribute
    {
        private bool _supportsPreviewControl;
        public static readonly SupportsPreviewControlAttribute Default = new SupportsPreviewControlAttribute(false);

        public SupportsPreviewControlAttribute(bool supportsPreviewControl)
        {
            this._supportsPreviewControl = supportsPreviewControl;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            SupportsPreviewControlAttribute attribute = obj as SupportsPreviewControlAttribute;
            return ((attribute != null) && (attribute.SupportsPreviewControl == this._supportsPreviewControl));
        }

        public override int GetHashCode()
        {
            return this._supportsPreviewControl.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool SupportsPreviewControl
        {
            get
            {
                return this._supportsPreviewControl;
            }
        }
    }
}

