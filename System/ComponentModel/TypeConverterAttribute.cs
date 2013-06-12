namespace System.ComponentModel
{
    using System;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class TypeConverterAttribute : Attribute
    {
        public static readonly TypeConverterAttribute Default = new TypeConverterAttribute();
        private string typeName;

        public TypeConverterAttribute()
        {
            this.typeName = string.Empty;
        }

        public TypeConverterAttribute(string typeName)
        {
            typeName.ToUpper(CultureInfo.InvariantCulture);
            this.typeName = typeName;
        }

        public TypeConverterAttribute(Type type)
        {
            this.typeName = type.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            TypeConverterAttribute attribute = obj as TypeConverterAttribute;
            return ((attribute != null) && (attribute.ConverterTypeName == this.typeName));
        }

        public override int GetHashCode()
        {
            return this.typeName.GetHashCode();
        }

        public string ConverterTypeName
        {
            get
            {
                return this.typeName;
            }
        }
    }
}

