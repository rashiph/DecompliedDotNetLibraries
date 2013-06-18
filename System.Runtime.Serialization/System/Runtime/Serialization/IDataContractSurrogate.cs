namespace System.Runtime.Serialization
{
    using System;
    using System.CodeDom;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public interface IDataContractSurrogate
    {
        object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType);
        object GetCustomDataToExport(Type clrType, Type dataContractType);
        Type GetDataContractType(Type type);
        object GetDeserializedObject(object obj, Type targetType);
        void GetKnownCustomDataTypes(Collection<Type> customDataTypes);
        object GetObjectToSerialize(object obj, Type targetType);
        Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData);
        CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit);
    }
}

