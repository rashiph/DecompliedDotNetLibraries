namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal sealed class NativeComInterfaces
    {
        internal const int ADS_ESCAPEDMODE_OFF_EX = 4;
        internal const int ADS_ESCAPEDMODE_ON = 2;
        internal const int ADS_FORMAT_LEAF = 11;
        internal const int ADS_FORMAT_X500_DN = 7;
        internal const int ADS_SETTYPE_DN = 4;

        [ComImport, Guid("C8F93DD0-4AE0-11CF-9E73-00AA004A5691"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IAdsClass
        {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Class { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string GUID { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string ADsPath { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Parent { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Schema { [return: MarshalAs(UnmanagedType.BStr)] get; }
            void GetInfo();
            void SetInfo();
            [return: MarshalAs(UnmanagedType.Struct)]
            object Get([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            void Put([In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetEx([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            void PutEx([In, MarshalAs(UnmanagedType.U4)] int lnControlCode, [In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            void GetInfoEx([In, MarshalAs(UnmanagedType.Struct)] object vProperties, [In, MarshalAs(UnmanagedType.U4)] int lnReserved);
            string PrimaryInterface { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string CLSID { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string OID { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            bool Abstract { [return: MarshalAs(UnmanagedType.VariantBool)] get; [param: MarshalAs(UnmanagedType.VariantBool)] set; }
            bool Auxiliary { [return: MarshalAs(UnmanagedType.VariantBool)] get; [param: MarshalAs(UnmanagedType.VariantBool)] set; }
            object MandatoryProperties { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object OptionalProperties { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object NamingProperties { [return: MarshalAs(UnmanagedType.Struct)] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object DerivedFrom { [return: MarshalAs(UnmanagedType.Struct)] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object AuxDerivedFrom { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object PossibleSuperiors { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object Containment { [return: MarshalAs(UnmanagedType.Struct)] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            bool Container { [return: MarshalAs(UnmanagedType.VariantBool)] get; [param: MarshalAs(UnmanagedType.VariantBool)] set; }
            string HelpFileName { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            int HelpFileContext { [return: MarshalAs(UnmanagedType.U4)] get; [param: MarshalAs(UnmanagedType.U4)] set; }
            [return: MarshalAs(UnmanagedType.Interface)]
            object Qualifiers();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("D592AED4-F420-11D0-A36E-00C04FB950DC")]
        internal interface IAdsPathname
        {
            [SuppressUnmanagedCodeSecurity]
            int Set([In, MarshalAs(UnmanagedType.BStr)] string bstrADsPath, [In, MarshalAs(UnmanagedType.U4)] int lnSetType);
            int SetDisplayType([In, MarshalAs(UnmanagedType.U4)] int lnDisplayType);
            [return: MarshalAs(UnmanagedType.BStr)]
            [SuppressUnmanagedCodeSecurity]
            string Retrieve([In, MarshalAs(UnmanagedType.U4)] int lnFormatType);
            [return: MarshalAs(UnmanagedType.U4)]
            int GetNumElements();
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetElement([In, MarshalAs(UnmanagedType.U4)] int lnElementIndex);
            void AddLeafElement([In, MarshalAs(UnmanagedType.BStr)] string bstrLeafElement);
            void RemoveLeafElement();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CopyPath();
            [return: MarshalAs(UnmanagedType.BStr)]
            [SuppressUnmanagedCodeSecurity]
            string GetEscapedElement([In, MarshalAs(UnmanagedType.U4)] int lnReserved, [In, MarshalAs(UnmanagedType.BStr)] string bstrInStr);
            int EscapedMode { get; [SuppressUnmanagedCodeSecurity] set; }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("C8F93DD3-4AE0-11CF-9E73-00AA004A5691")]
        internal interface IAdsProperty
        {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Class { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string GUID { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string ADsPath { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Parent { [return: MarshalAs(UnmanagedType.BStr)] get; }
            string Schema { [return: MarshalAs(UnmanagedType.BStr)] get; }
            void GetInfo();
            void SetInfo();
            [return: MarshalAs(UnmanagedType.Struct)]
            object Get([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            void Put([In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            [return: MarshalAs(UnmanagedType.Struct)]
            object GetEx([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            void PutEx([In, MarshalAs(UnmanagedType.U4)] int lnControlCode, [In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            void GetInfoEx([In, MarshalAs(UnmanagedType.Struct)] object vProperties, [In, MarshalAs(UnmanagedType.U4)] int lnReserved);
            string OID { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string Syntax { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            int MaxRange { [return: MarshalAs(UnmanagedType.U4)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.U4)] set; }
            int MinRange { [return: MarshalAs(UnmanagedType.U4)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.U4)] set; }
            bool MultiValued { [return: MarshalAs(UnmanagedType.VariantBool)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.VariantBool)] set; }
            object Qualifiers();
        }

        [ComImport, Guid("080d0d78-f421-11d0-a36e-00c04fb950dc")]
        internal class Pathname
        {
        }
    }
}

