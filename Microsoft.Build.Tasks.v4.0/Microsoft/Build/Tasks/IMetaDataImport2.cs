namespace Microsoft.Build.Tasks
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("FCE5EFA0-8BBA-4f8e-A036-8F2022B08466")]
    internal interface IMetaDataImport2
    {
        void CloseEnum();
        void CountEnum();
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
        void GetCustomAttributeByName([In] uint tkObj, [MarshalAs(UnmanagedType.LPArray)] char[] szName, out IntPtr ppData, out uint pcbData);
        void IsValidToken();
        void GetNestedClassProps();
        void GetNativeCallConvFromSig();
        void IsGlobal();
        void EnumGenericParams();
        void GetGenericParamProps();
        void GetMethodSpecProps();
        void EnumGenericParamConstraints();
        void GetGenericParamConstraintProps();
        void GetPEKind(out uint pdwPEKind, out uint pdwMachine);
        void GetVersionString([MarshalAs(UnmanagedType.LPArray)] char[] pwzBuf, uint ccBufSize, out uint pccBufSize);
    }
}

