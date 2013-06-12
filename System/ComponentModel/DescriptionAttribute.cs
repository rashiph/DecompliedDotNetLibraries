namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute
    {
        public static readonly DescriptionAttribute Default = new DescriptionAttribute();
        private string description;

        public DescriptionAttribute() : this(string.Empty)
        {
        }

        public DescriptionAttribute(string description)
        {
            this.description = description;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DescriptionAttribute attribute = obj as DescriptionAttribute;
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
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }
    }
}

