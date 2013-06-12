namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;

    internal class StructModel : TypeModel
    {
        internal StructModel(Type type, TypeDesc typeDesc, ModelScope scope) : base(type, typeDesc, scope)
        {
        }

        internal static bool CheckPropertyRead(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return false;
            }
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod.IsStatic)
            {
                return false;
            }
            if (getMethod.GetParameters().Length > 0)
            {
                return false;
            }
            return true;
        }

        private void CheckSupportedMember(TypeDesc typeDesc, MemberInfo member, Type type)
        {
            if (typeDesc != null)
            {
                if (typeDesc.IsUnsupported)
                {
                    if (typeDesc.Exception == null)
                    {
                        typeDesc.Exception = new NotSupportedException(Res.GetString("XmlSerializerUnsupportedType", new object[] { typeDesc.FullName }));
                    }
                    throw new InvalidOperationException(Res.GetString("XmlSerializerUnsupportedMember", new object[] { member.DeclaringType.FullName + "." + member.Name, type.FullName }), typeDesc.Exception);
                }
                this.CheckSupportedMember(typeDesc.BaseTypeDesc, member, type);
                this.CheckSupportedMember(typeDesc.ArrayElementTypeDesc, member, type);
            }
        }

        private FieldModel GetFieldModel(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                return null;
            }
            if (fieldInfo.DeclaringType != base.Type)
            {
                return null;
            }
            TypeDesc typeDesc = base.ModelScope.TypeScope.GetTypeDesc(fieldInfo.FieldType, fieldInfo, true, false);
            if ((fieldInfo.IsInitOnly && (typeDesc.Kind != TypeKind.Collection)) && (typeDesc.Kind != TypeKind.Enumerable))
            {
                return null;
            }
            this.CheckSupportedMember(typeDesc, fieldInfo, fieldInfo.FieldType);
            return new FieldModel(fieldInfo, fieldInfo.FieldType, typeDesc);
        }

        internal FieldModel GetFieldModel(MemberInfo memberInfo)
        {
            FieldModel fieldModel = null;
            if (memberInfo is FieldInfo)
            {
                fieldModel = this.GetFieldModel((FieldInfo) memberInfo);
            }
            else if (memberInfo is PropertyInfo)
            {
                fieldModel = this.GetPropertyModel((PropertyInfo) memberInfo);
            }
            if (((fieldModel != null) && fieldModel.ReadOnly) && ((fieldModel.FieldTypeDesc.Kind != TypeKind.Collection) && (fieldModel.FieldTypeDesc.Kind != TypeKind.Enumerable)))
            {
                return null;
            }
            return fieldModel;
        }

        internal MemberInfo[] GetMemberInfos()
        {
            MemberInfo[] members = base.Type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MemberInfo[] infoArray2 = new MemberInfo[members.Length];
            int num = 0;
            for (int i = 0; i < members.Length; i++)
            {
                if ((members[i].MemberType & MemberTypes.Property) == 0)
                {
                    infoArray2[num++] = members[i];
                }
            }
            for (int j = 0; j < members.Length; j++)
            {
                if ((members[j].MemberType & MemberTypes.Property) != 0)
                {
                    infoArray2[num++] = members[j];
                }
            }
            return infoArray2;
        }

        private FieldModel GetPropertyModel(PropertyInfo propertyInfo)
        {
            if (propertyInfo.DeclaringType != base.Type)
            {
                return null;
            }
            if (!CheckPropertyRead(propertyInfo))
            {
                return null;
            }
            TypeDesc typeDesc = base.ModelScope.TypeScope.GetTypeDesc(propertyInfo.PropertyType, propertyInfo, true, false);
            if ((!propertyInfo.CanWrite && (typeDesc.Kind != TypeKind.Collection)) && (typeDesc.Kind != TypeKind.Enumerable))
            {
                return null;
            }
            this.CheckSupportedMember(typeDesc, propertyInfo, propertyInfo.PropertyType);
            return new FieldModel(propertyInfo, propertyInfo.PropertyType, typeDesc);
        }
    }
}

