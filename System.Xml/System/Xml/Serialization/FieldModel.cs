namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;

    internal class FieldModel
    {
        private bool checkShouldPersist;
        private SpecifiedAccessor checkSpecified;
        private Type fieldType;
        private TypeDesc fieldTypeDesc;
        private bool isProperty;
        private string name;
        private bool readOnly;

        internal FieldModel(MemberInfo memberInfo, Type fieldType, TypeDesc fieldTypeDesc)
        {
            this.name = memberInfo.Name;
            this.fieldType = fieldType;
            this.fieldTypeDesc = fieldTypeDesc;
            this.checkShouldPersist = memberInfo.DeclaringType.GetMethod("ShouldSerialize" + memberInfo.Name, new Type[0]) != null;
            FieldInfo field = memberInfo.DeclaringType.GetField(memberInfo.Name + "Specified");
            if (field != null)
            {
                if (field.FieldType != typeof(bool))
                {
                    throw new InvalidOperationException(Res.GetString("XmlInvalidSpecifiedType", new object[] { field.Name, field.FieldType.FullName, typeof(bool).FullName }));
                }
                this.checkSpecified = field.IsInitOnly ? SpecifiedAccessor.ReadOnly : SpecifiedAccessor.ReadWrite;
            }
            else
            {
                PropertyInfo property = memberInfo.DeclaringType.GetProperty(memberInfo.Name + "Specified");
                if (property != null)
                {
                    if (StructModel.CheckPropertyRead(property))
                    {
                        this.checkSpecified = property.CanWrite ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.ReadOnly;
                    }
                    if ((this.checkSpecified != SpecifiedAccessor.None) && (property.PropertyType != typeof(bool)))
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidSpecifiedType", new object[] { property.Name, property.PropertyType.FullName, typeof(bool).FullName }));
                    }
                }
            }
            if (memberInfo is PropertyInfo)
            {
                this.readOnly = !((PropertyInfo) memberInfo).CanWrite;
                this.isProperty = true;
            }
            else if (memberInfo is FieldInfo)
            {
                this.readOnly = ((FieldInfo) memberInfo).IsInitOnly;
            }
        }

        internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist) : this(name, fieldType, fieldTypeDesc, checkSpecified, checkShouldPersist, false)
        {
        }

        internal FieldModel(string name, Type fieldType, TypeDesc fieldTypeDesc, bool checkSpecified, bool checkShouldPersist, bool readOnly)
        {
            this.fieldTypeDesc = fieldTypeDesc;
            this.name = name;
            this.fieldType = fieldType;
            this.checkSpecified = checkSpecified ? SpecifiedAccessor.ReadWrite : SpecifiedAccessor.None;
            this.checkShouldPersist = checkShouldPersist;
            this.readOnly = readOnly;
        }

        internal bool CheckShouldPersist
        {
            get
            {
                return this.checkShouldPersist;
            }
        }

        internal SpecifiedAccessor CheckSpecified
        {
            get
            {
                return this.checkSpecified;
            }
        }

        internal Type FieldType
        {
            get
            {
                return this.fieldType;
            }
        }

        internal TypeDesc FieldTypeDesc
        {
            get
            {
                return this.fieldTypeDesc;
            }
        }

        internal bool IsProperty
        {
            get
            {
                return this.isProperty;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }
    }
}

