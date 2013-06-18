namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
    internal interface IMetaDataImport
    {
        [PreserveSig]
        void CloseEnum();
        void CountEnum(IntPtr iRef, ref uint ulCount);
        void ResetEnum();
        void EnumTypeDefs();
        void EnumInterfaceImpls();
        void EnumTypeRefs();
        void FindTypeDefByName();
        void GetScopeProps();
        void GetModuleFromScope();
        void GetTypeDefProps();
        void GetInterfaceImplProps();
        void GetTypeRefProps();
        void ResolveTypeRef();
        void EnumMembers();
        void EnumMembersWithName();
        void EnumMethods();
        void EnumMethodsWithName();
        void EnumFields();
        void EnumFieldsWithName();
        void EnumParams();
        void EnumMemberRefs();
        void EnumMethodImpls();
        void EnumPermissionSets();
        void FindMember();
        void FindMethod();
        void FindField();
        void FindMemberRef();
        void GetMethodProps();
        void GetMemberRefProps();
        void EnumProperties();
        void EnumEvents();
        void GetEventProps();
        void EnumMethodSemantics();
        void GetMethodSemantics();
        void GetClassLayout();
        void GetFieldMarshal();
        void GetRVA();
        void GetPermissionSetProps();
        void GetSigFromToken();
        void GetModuleRefProps();
        void EnumModuleRefs();
        void GetTypeSpecFromToken();
        void GetNameFromToken();
        void EnumUnresolvedMethods();
        void GetUserString();
        void GetPinvokeMap();
        void EnumSignatures();
        void EnumTypeSpecs();
        void EnumUserStrings();
        void GetParamForMethodIndex();
        void EnumCustomAttributes();
        void GetCustomAttributeProps();
        void FindTypeRef();
        void GetMemberProps();
        void GetFieldProps();
        void GetPropertyProps();
        void GetParamProps();
        void GetCustomAttributeByName();
        void IsValidToken();
        void GetNestedClassProps();
        void GetNativeCallConvFromSig();
        void IsGlobal();
    }
}

