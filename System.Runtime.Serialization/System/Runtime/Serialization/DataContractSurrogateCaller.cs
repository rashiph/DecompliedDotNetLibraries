namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal static class DataContractSurrogateCaller
    {
        internal static object GetCustomDataToExport(IDataContractSurrogate surrogate, MemberInfo memberInfo, Type dataContractType)
        {
            return surrogate.GetCustomDataToExport(memberInfo, dataContractType);
        }

        internal static object GetCustomDataToExport(IDataContractSurrogate surrogate, Type clrType, Type dataContractType)
        {
            if (DataContract.GetBuiltInDataContract(clrType) != null)
            {
                return null;
            }
            return surrogate.GetCustomDataToExport(clrType, dataContractType);
        }

        internal static Type GetDataContractType(IDataContractSurrogate surrogate, Type type)
        {
            if (DataContract.GetBuiltInDataContract(type) != null)
            {
                return type;
            }
            Type dataContractType = surrogate.GetDataContractType(type);
            if (dataContractType == null)
            {
                return type;
            }
            return dataContractType;
        }

        internal static object GetDeserializedObject(IDataContractSurrogate surrogate, object obj, Type objType, Type memberType)
        {
            if (obj == null)
            {
                return null;
            }
            if (DataContract.GetBuiltInDataContract(objType) != null)
            {
                return obj;
            }
            return surrogate.GetDeserializedObject(obj, memberType);
        }

        internal static void GetKnownCustomDataTypes(IDataContractSurrogate surrogate, Collection<Type> customDataTypes)
        {
            surrogate.GetKnownCustomDataTypes(customDataTypes);
        }

        internal static object GetObjectToSerialize(IDataContractSurrogate surrogate, object obj, Type objType, Type membertype)
        {
            if (obj == null)
            {
                return null;
            }
            if (DataContract.GetBuiltInDataContract(objType) != null)
            {
                return obj;
            }
            return surrogate.GetObjectToSerialize(obj, membertype);
        }

        internal static Type GetReferencedTypeOnImport(IDataContractSurrogate surrogate, string typeName, string typeNamespace, object customData)
        {
            if (DataContract.GetBuiltInDataContract(typeName, typeNamespace) != null)
            {
                return null;
            }
            return surrogate.GetReferencedTypeOnImport(typeName, typeNamespace, customData);
        }

        internal static CodeTypeDeclaration ProcessImportedType(IDataContractSurrogate surrogate, CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit)
        {
            return surrogate.ProcessImportedType(typeDeclaration, compileUnit);
        }
    }
}

