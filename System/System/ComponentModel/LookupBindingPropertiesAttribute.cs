namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LookupBindingPropertiesAttribute : Attribute
    {
        private readonly string dataSource;
        public static readonly LookupBindingPropertiesAttribute Default = new LookupBindingPropertiesAttribute();
        private readonly string displayMember;
        private readonly string lookupMember;
        private readonly string valueMember;

        public LookupBindingPropertiesAttribute()
        {
            this.dataSource = null;
            this.displayMember = null;
            this.valueMember = null;
            this.lookupMember = null;
        }

        public LookupBindingPropertiesAttribute(string dataSource, string displayMember, string valueMember, string lookupMember)
        {
            this.dataSource = dataSource;
            this.displayMember = displayMember;
            this.valueMember = valueMember;
            this.lookupMember = lookupMember;
        }

        public override bool Equals(object obj)
        {
            LookupBindingPropertiesAttribute attribute = obj as LookupBindingPropertiesAttribute;
            return ((((attribute != null) && (attribute.DataSource == this.dataSource)) && ((attribute.displayMember == this.displayMember) && (attribute.valueMember == this.valueMember))) && (attribute.lookupMember == this.lookupMember));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string DataSource
        {
            get
            {
                return this.dataSource;
            }
        }

        public string DisplayMember
        {
            get
            {
                return this.displayMember;
            }
        }

        public string LookupMember
        {
            get
            {
                return this.lookupMember;
            }
        }

        public string ValueMember
        {
            get
            {
                return this.valueMember;
            }
        }
    }
}

