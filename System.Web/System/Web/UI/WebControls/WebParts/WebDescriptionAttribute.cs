namespace System.Web.UI.WebControls.WebParts
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class WebDescriptionAttribute : Attribute
    {
        private string _description;
        public static readonly WebDescriptionAttribute Default = new WebDescriptionAttribute();

        public WebDescriptionAttribute() : this(string.Empty)
        {
        }

        public WebDescriptionAttribute(string description)
        {
            this._description = description;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            WebDescriptionAttribute attribute = obj as WebDescriptionAttribute;
            return ((attribute != null) && (attribute.Description == this.Description));
        }

        public override int GetHashCode()
        {
            return this.Description.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public virtual string Description
        {
            get
            {
                return this.DescriptionValue;
            }
        }

        protected string DescriptionValue
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }
    }
}

