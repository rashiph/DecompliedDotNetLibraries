namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ComplexBindingPropertiesAttribute : Attribute
    {
        private readonly string dataMember;
        private readonly string dataSource;
        public static readonly ComplexBindingPropertiesAttribute Default = new ComplexBindingPropertiesAttribute();

        public ComplexBindingPropertiesAttribute()
        {
            this.dataSource = null;
            this.dataMember = null;
        }

        public ComplexBindingPropertiesAttribute(string dataSource)
        {
            this.dataSource = dataSource;
            this.dataMember = null;
        }

        public ComplexBindingPropertiesAttribute(string dataSource, string dataMember)
        {
            this.dataSource = dataSource;
            this.dataMember = dataMember;
        }

        public override bool Equals(object obj)
        {
            ComplexBindingPropertiesAttribute attribute = obj as ComplexBindingPropertiesAttribute;
            return (((attribute != null) && (attribute.DataSource == this.dataSource)) && (attribute.DataMember == this.dataMember));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string DataMember
        {
            get
            {
                return this.dataMember;
            }
        }

        public string DataSource
        {
            get
            {
                return this.dataSource;
            }
        }
    }
}

