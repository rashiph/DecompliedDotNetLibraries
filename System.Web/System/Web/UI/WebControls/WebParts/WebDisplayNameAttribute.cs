namespace System.Web.UI.WebControls.WebParts
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class WebDisplayNameAttribute : Attribute
    {
        private string _displayName;
        public static readonly WebDisplayNameAttribute Default = new WebDisplayNameAttribute();

        public WebDisplayNameAttribute() : this(string.Empty)
        {
        }

        public WebDisplayNameAttribute(string displayName)
        {
            this._displayName = displayName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            WebDisplayNameAttribute attribute = obj as WebDisplayNameAttribute;
            return ((attribute != null) && (attribute.DisplayName == this.DisplayName));
        }

        public override int GetHashCode()
        {
            return this.DisplayName.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public virtual string DisplayName
        {
            get
            {
                return this.DisplayNameValue;
            }
        }

        protected string DisplayNameValue
        {
            get
            {
                return this._displayName;
            }
            set
            {
                this._displayName = value;
            }
        }
    }
}

