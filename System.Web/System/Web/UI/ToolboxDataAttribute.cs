namespace System.Web.UI
{
    using System;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ToolboxDataAttribute : Attribute
    {
        private string data = string.Empty;
        public static readonly ToolboxDataAttribute Default = new ToolboxDataAttribute(string.Empty);

        public ToolboxDataAttribute(string data)
        {
            this.data = data;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is ToolboxDataAttribute)) && StringUtil.EqualsIgnoreCase(((ToolboxDataAttribute) obj).Data, this.data)));
        }

        public override int GetHashCode()
        {
            if (this.Data == null)
            {
                return 0;
            }
            return this.Data.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public string Data
        {
            get
            {
                return this.data;
            }
        }
    }
}

