namespace System.ComponentModel
{
    using System;
    using System.Globalization;

    [AttributeUsage(AttributeTargets.All, AllowMultiple=true, Inherited=true)]
    public sealed class EditorAttribute : Attribute
    {
        private string baseTypeName;
        private string typeId;
        private string typeName;

        public EditorAttribute()
        {
            this.typeName = string.Empty;
            this.baseTypeName = string.Empty;
        }

        public EditorAttribute(string typeName, string baseTypeName)
        {
            typeName.ToUpper(CultureInfo.InvariantCulture);
            this.typeName = typeName;
            this.baseTypeName = baseTypeName;
        }

        public EditorAttribute(string typeName, Type baseType)
        {
            typeName.ToUpper(CultureInfo.InvariantCulture);
            this.typeName = typeName;
            this.baseTypeName = baseType.AssemblyQualifiedName;
        }

        public EditorAttribute(Type type, Type baseType)
        {
            this.typeName = type.AssemblyQualifiedName;
            this.baseTypeName = baseType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            EditorAttribute attribute = obj as EditorAttribute;
            return (((attribute != null) && (attribute.typeName == this.typeName)) && (attribute.baseTypeName == this.baseTypeName));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string EditorBaseTypeName
        {
            get
            {
                return this.baseTypeName;
            }
        }

        public string EditorTypeName
        {
            get
            {
                return this.typeName;
            }
        }

        public override object TypeId
        {
            get
            {
                if (this.typeId == null)
                {
                    string baseTypeName = this.baseTypeName;
                    int index = baseTypeName.IndexOf(',');
                    if (index != -1)
                    {
                        baseTypeName = baseTypeName.Substring(0, index);
                    }
                    this.typeId = base.GetType().FullName + baseTypeName;
                }
                return this.typeId;
            }
        }
    }
}

