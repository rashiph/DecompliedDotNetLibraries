namespace System.DirectoryServices.Interop
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal class UnsafeNativeMethods
    {
        internal const int INVALID_FILTER = -2147016642;
        internal const int S_ADS_NOMORE_ROWS = 0x5012;
        internal const int SIZE_LIMIT_EXCEEDED = -2147016669;

        public static int ADsOpenObject(string path, string userName, string password, int flags, [In, Out] ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object ppObject)
        {
            int num;
            try
            {
                num = IntADsOpenObject(path, userName, password, flags, ref iid, out ppObject);
            }
            catch (EntryPointNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("DSAdsiNotInstalled"));
            }
            return num;
        }

        [DllImport("activeds.dll", EntryPoint="ADsOpenObject", CharSet=CharSet.Unicode, ExactSpelling=true)]
        private static extern int IntADsOpenObject(string path, string userName, string password, int flags, [In, Out] ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object ppObject);

        [ComImport, Guid("FD8256D0-FD15-11CE-ABC4-02608C9E7553"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IAds
        {
            string Name { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            string Class { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            string GUID { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            string ADsPath { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            string Parent { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            string Schema { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; }
            [SuppressUnmanagedCodeSecurity]
            void GetInfo();
            [SuppressUnmanagedCodeSecurity]
            void SetInfo();
            [return: MarshalAs(UnmanagedType.Struct)]
            [SuppressUnmanagedCodeSecurity]
            object Get([In, MarshalAs(UnmanagedType.BStr)] string bstrName);
            [SuppressUnmanagedCodeSecurity]
            void Put([In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetEx([In, MarshalAs(UnmanagedType.BStr)] string bstrName, [MarshalAs(UnmanagedType.Struct)] out object value);
            [SuppressUnmanagedCodeSecurity]
            void PutEx([In, MarshalAs(UnmanagedType.U4)] int lnControlCode, [In, MarshalAs(UnmanagedType.BStr)] string bstrName, [In, MarshalAs(UnmanagedType.Struct)] object vProp);
            [SuppressUnmanagedCodeSecurity]
            void GetInfoEx([In, MarshalAs(UnmanagedType.Struct)] object vProperties, [In, MarshalAs(UnmanagedType.U4)] int lnReserved);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("001677D0-FD16-11CE-ABC4-02608C9E7553")]
        public interface IAdsContainer
        {
            int Count { [return: MarshalAs(UnmanagedType.U4)] [SuppressUnmanagedCodeSecurity] get; }
            object _NewEnum { [return: MarshalAs(UnmanagedType.Interface)] [SuppressUnmanagedCodeSecurity] get; }
            object Filter { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] set; }
            object Hints { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] set; }
            [return: MarshalAs(UnmanagedType.Interface)]
            [SuppressUnmanagedCodeSecurity]
            object GetObject([In, MarshalAs(UnmanagedType.BStr)] string className, [In, MarshalAs(UnmanagedType.BStr)] string relativeName);
            [return: MarshalAs(UnmanagedType.Interface)]
            [SuppressUnmanagedCodeSecurity]
            object Create([In, MarshalAs(UnmanagedType.BStr)] string className, [In, MarshalAs(UnmanagedType.BStr)] string relativeName);
            [SuppressUnmanagedCodeSecurity]
            void Delete([In, MarshalAs(UnmanagedType.BStr)] string className, [In, MarshalAs(UnmanagedType.BStr)] string relativeName);
            [return: MarshalAs(UnmanagedType.Interface)]
            [SuppressUnmanagedCodeSecurity]
            object CopyHere([In, MarshalAs(UnmanagedType.BStr)] string sourceName, [In, MarshalAs(UnmanagedType.BStr)] string newName);
            [return: MarshalAs(UnmanagedType.Interface)]
            [SuppressUnmanagedCodeSecurity]
            object MoveHere([In, MarshalAs(UnmanagedType.BStr)] string sourceName, [In, MarshalAs(UnmanagedType.BStr)] string newName);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("B2BD0902-8878-11D1-8C21-00C04FD8D503")]
        public interface IAdsDeleteOps
        {
            [SuppressUnmanagedCodeSecurity]
            void DeleteObject(int flags);
        }

        [ComImport, Guid("46F14FDA-232B-11D1-A808-00C04FD8D5A8"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IAdsObjectOptions
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [SuppressUnmanagedCodeSecurity]
            object GetOption(int flag);
            [SuppressUnmanagedCodeSecurity]
            void SetOption(int flag, [In, MarshalAs(UnmanagedType.Struct)] object varValue);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("46f14fda-232b-11d1-a808-00c04fd8d5a8")]
        public interface IAdsObjectOptions2
        {
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetOption(int flag, [MarshalAs(UnmanagedType.Struct)] out object value);
            [SuppressUnmanagedCodeSecurity]
            void SetOption(int option, System.DirectoryServices.Interop.Variant value);
        }

        [ComImport, Guid("05792C8E-941F-11D0-8529-00C04FD8D503"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IAdsPropertyEntry
        {
            [SuppressUnmanagedCodeSecurity]
            void Clear();
            string Name { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] set; }
            int ADsType { [SuppressUnmanagedCodeSecurity] get; [SuppressUnmanagedCodeSecurity] set; }
            int ControlCode { [SuppressUnmanagedCodeSecurity] get; [SuppressUnmanagedCodeSecurity] set; }
            object Values { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] set; }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("C6F602B6-8F69-11D0-8528-00C04FD8D503")]
        public interface IAdsPropertyList
        {
            int PropertyCount { [return: MarshalAs(UnmanagedType.U4)] [SuppressUnmanagedCodeSecurity] get; }
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int Next([MarshalAs(UnmanagedType.Struct)] out object nextProp);
            void Skip([In] int cElements);
            [SuppressUnmanagedCodeSecurity]
            void Reset();
            [return: MarshalAs(UnmanagedType.Struct)]
            [SuppressUnmanagedCodeSecurity]
            object Item([In, MarshalAs(UnmanagedType.Struct)] object varIndex);
            [return: MarshalAs(UnmanagedType.Struct)]
            [SuppressUnmanagedCodeSecurity]
            object GetPropertyItem([In, MarshalAs(UnmanagedType.BStr)] string bstrName, int ADsType);
            [SuppressUnmanagedCodeSecurity]
            void PutPropertyItem([In, MarshalAs(UnmanagedType.Struct)] object varData);
            void ResetPropertyItem([In, MarshalAs(UnmanagedType.Struct)] object varEntry);
            void PurgePropertyList();
        }

        [ComImport, Guid("79FA9AD0-A97C-11D0-8534-00C04FD8D503"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IAdsPropertyValue
        {
            [SuppressUnmanagedCodeSecurity]
            void Clear();
            int ADsType { [SuppressUnmanagedCodeSecurity] get; [SuppressUnmanagedCodeSecurity] set; }
            string DNString { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string CaseExactString { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string CaseIgnoreString { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string PrintableString { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            string NumericString { [return: MarshalAs(UnmanagedType.BStr)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.BStr)] set; }
            bool Boolean { get; set; }
            int Integer { get; set; }
            object OctetString { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] set; }
            object SecurityDescriptor { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object LargeInteger { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
            object UTCTime { [return: MarshalAs(UnmanagedType.Struct)] [SuppressUnmanagedCodeSecurity] get; [param: MarshalAs(UnmanagedType.Struct)] set; }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("109BA8EC-92F0-11D0-A790-00C04FD8D5A8")]
        public interface IDirectorySearch
        {
            [SuppressUnmanagedCodeSecurity]
            void SetSearchPreference([In] IntPtr pSearchPrefs, int dwNumPrefs);
            [SuppressUnmanagedCodeSecurity]
            void ExecuteSearch([In, MarshalAs(UnmanagedType.LPWStr)] string pszSearchFilter, [In, MarshalAs(UnmanagedType.LPArray)] string[] pAttributeNames, [In] int dwNumberAttributes, out IntPtr hSearchResult);
            [SuppressUnmanagedCodeSecurity]
            void AbandonSearch([In] IntPtr hSearchResult);
            [return: MarshalAs(UnmanagedType.U4)]
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetFirstRow([In] IntPtr hSearchResult);
            [return: MarshalAs(UnmanagedType.U4)]
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetNextRow([In] IntPtr hSearchResult);
            [return: MarshalAs(UnmanagedType.U4)]
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetPreviousRow([In] IntPtr hSearchResult);
            [return: MarshalAs(UnmanagedType.U4)]
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetNextColumnName([In] IntPtr hSearchResult, [Out] IntPtr ppszColumnName);
            [SuppressUnmanagedCodeSecurity]
            void GetColumn([In] IntPtr hSearchResult, [In] IntPtr szColumnName, [In] IntPtr pSearchColumn);
            [SuppressUnmanagedCodeSecurity]
            void FreeColumn([In] IntPtr pSearchColumn);
            [SuppressUnmanagedCodeSecurity]
            void CloseSearchHandle([In] IntPtr hSearchResult);
        }

        [ComImport, Guid("72d3edc2-a4c4-11d0-8533-00c04fd8d503")]
        public class PropertyEntry
        {
        }

        [ComImport, Guid("7b9e38b0-a97c-11d0-8534-00c04fd8d503")]
        public class PropertyValue
        {
        }
    }
}

