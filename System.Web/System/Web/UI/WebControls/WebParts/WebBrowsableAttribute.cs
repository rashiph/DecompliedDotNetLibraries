namespace System.Web.UI.WebControls.WebParts
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class WebBrowsableAttribute : Attribute
    {
        private bool _browsable;
        public static readonly WebBrowsableAttribute Default = No;
        public static readonly WebBrowsableAttribute No = new WebBrowsableAttribute(false);
        public static readonly WebBrowsableAttribute Yes = new WebBrowsableAttribute(true);

        public WebBrowsableAttribute() : this(true)
        {
        }

        public WebBrowsableAttribute(bool browsable)
        {
            this._browsable = browsable;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            WebBrowsableAttribute attribute = obj as WebBrowsableAttribute;
            return ((attribute != null) && (attribute.Browsable == this.Browsable));
        }

        public override int GetHashCode()
        {
            return this._browsable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool Browsable
        {
            get
            {
                return this._browsable;
            }
        }
    }
}

