namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("afef65ad-4577-447a-a148-83acadd3d4b9"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class SQLDebugging : ISQLDebug
    {
        private const int ACL_REVISION = 2;
        private const int DELETE = 0x10000;
        private const int DOMAIN_ALIAS_RID_ADMINS = 0x220;
        private const int DOMAIN_GROUP_RID_ADMINS = 0x200;
        private const int FILE_ALL_ACCESS = 0x1f01ff;
        private const uint GENERIC_ALL = 0x10000000;
        private const uint GENERIC_EXECUTE = 0x20000000;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const int READ_CONTROL = 0x20000;
        private const int SECURITY_AUTHENTICATED_USER_RID = 11;
        private const int SECURITY_BUILTIN_DOMAIN_RID = 0x20;
        private const int SECURITY_DESCRIPTOR_REVISION = 1;
        private const int SECURITY_LOCAL_SYSTEM_RID = 0x12;
        private const byte SECURITY_NT_AUTHORITY = 5;
        private const int SECURITY_WORLD_RID = 0;
        private const int sizeofACCESS_ALLOWED_ACE = 12;
        private const int sizeofACCESS_DENIED_ACE = 12;
        private const int sizeofACL = 8;
        private const int sizeofSECURITY_ATTRIBUTES = 12;
        private const int sizeofSECURITY_DESCRIPTOR = 20;
        private const int sizeofSID_IDENTIFIER_AUTHORITY = 6;
        private const int STANDARD_RIGHTS_REQUIRED = 0xf0000;
        private const int SYNCHRONIZE = 0x100000;
        private const int WRITE_DAC = 0x40000;
        private const int WRITE_OWNER = 0x80000;

        private IntPtr CreateSD(ref IntPtr pDacl)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr pSid = IntPtr.Zero;
            IntPtr ptr4 = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            int cb = 0;
            bool flag = false;
            ptr = Marshal.AllocHGlobal(6);
            if (ptr != IntPtr.Zero)
            {
                Marshal.WriteInt32(ptr, 0, 0);
                Marshal.WriteByte(ptr, 4, 0);
                Marshal.WriteByte(ptr, 5, 5);
                flag = System.Data.Common.NativeMethods.AllocateAndInitializeSid(ptr, 1, 11, 0, 0, 0, 0, 0, 0, 0, ref pSid);
                if (flag && (pSid != IntPtr.Zero))
                {
                    flag = System.Data.Common.NativeMethods.AllocateAndInitializeSid(ptr, 2, 0x20, 0x220, 0, 0, 0, 0, 0, 0, ref ptr4);
                    if (flag && (ptr4 != IntPtr.Zero))
                    {
                        flag = false;
                        zero = Marshal.AllocHGlobal(20);
                        if (zero != IntPtr.Zero)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                Marshal.WriteByte(zero, i, 0);
                            }
                            cb = (0x2c + System.Data.Common.NativeMethods.GetLengthSid(pSid)) + System.Data.Common.NativeMethods.GetLengthSid(ptr4);
                            pDacl = Marshal.AllocHGlobal(cb);
                            if ((((pDacl != IntPtr.Zero) && System.Data.Common.NativeMethods.InitializeAcl(pDacl, cb, 2)) && (System.Data.Common.NativeMethods.AddAccessDeniedAce(pDacl, 2, 0x40000, pSid) && System.Data.Common.NativeMethods.AddAccessAllowedAce(pDacl, 2, 0x80000000, pSid))) && ((System.Data.Common.NativeMethods.AddAccessAllowedAce(pDacl, 2, 0x10000000, ptr4) && System.Data.Common.NativeMethods.InitializeSecurityDescriptor(zero, 1)) && System.Data.Common.NativeMethods.SetSecurityDescriptorDacl(zero, true, pDacl, false)))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }
            if (ptr4 != IntPtr.Zero)
            {
                System.Data.Common.NativeMethods.FreeSid(ptr4);
            }
            if (pSid != IntPtr.Zero)
            {
                System.Data.Common.NativeMethods.FreeSid(pSid);
            }
            if (flag)
            {
                return zero;
            }
            if (zero != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(zero);
            }
            return IntPtr.Zero;
        }

        bool ISQLDebug.SQLDebug(int dwpidDebugger, int dwpidDebuggee, [MarshalAs(UnmanagedType.LPStr)] string pszMachineName, [MarshalAs(UnmanagedType.LPStr)] string pszSDIDLLName, int dwOption, int cbData, byte[] rgbData)
        {
            string str;
            bool flag = false;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr val = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            IntPtr pDacl = IntPtr.Zero;
            if ((pszMachineName == null) || (pszSDIDLLName == null))
            {
                return false;
            }
            if ((pszMachineName.Length > 0x20) || (pszSDIDLLName.Length > 0x10))
            {
                return false;
            }
            Encoding encoding = Encoding.GetEncoding(0x4e4);
            byte[] bytes = encoding.GetBytes(pszMachineName);
            byte[] source = encoding.GetBytes(pszSDIDLLName);
            if ((rgbData != null) && (cbData > 0xff))
            {
                return false;
            }
            if (ADP.IsPlatformNT5)
            {
                str = @"Global\SqlClientSSDebug";
            }
            else
            {
                str = "SqlClientSSDebug";
            }
            str = str + dwpidDebuggee.ToString(CultureInfo.InvariantCulture);
            val = this.CreateSD(ref pDacl);
            ptr2 = Marshal.AllocHGlobal(12);
            if ((val == IntPtr.Zero) || (ptr2 == IntPtr.Zero))
            {
                return false;
            }
            Marshal.WriteInt32(ptr2, 0, 12);
            Marshal.WriteIntPtr(ptr2, 4, val);
            Marshal.WriteInt32(ptr2, 8, 0);
            zero = System.Data.Common.NativeMethods.CreateFileMappingA(ADP.InvalidPtr, ptr2, 4, 0, Marshal.SizeOf(typeof(MEMMAP)), str);
            if (IntPtr.Zero != zero)
            {
                ptr = System.Data.Common.NativeMethods.MapViewOfFile(zero, 6, 0, 0, IntPtr.Zero);
                if (IntPtr.Zero != ptr)
                {
                    int ofs = 0;
                    Marshal.WriteInt32(ptr, ofs, dwpidDebugger);
                    ofs += 4;
                    Marshal.WriteInt32(ptr, ofs, dwOption);
                    ofs += 4;
                    Marshal.Copy(bytes, 0, ADP.IntPtrOffset(ptr, ofs), bytes.Length);
                    ofs += 0x20;
                    Marshal.Copy(source, 0, ADP.IntPtrOffset(ptr, ofs), source.Length);
                    ofs += 0x10;
                    Marshal.WriteInt32(ptr, ofs, cbData);
                    ofs += 4;
                    if (rgbData != null)
                    {
                        Marshal.Copy(rgbData, 0, ADP.IntPtrOffset(ptr, ofs), cbData);
                    }
                    System.Data.Common.NativeMethods.UnmapViewOfFile(ptr);
                    flag = true;
                }
            }
            if (!flag && (zero != IntPtr.Zero))
            {
                System.Data.Common.NativeMethods.CloseHandle(zero);
            }
            if (ptr2 != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr2);
            }
            if (val != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(val);
            }
            if (pDacl != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pDacl);
            }
            return flag;
        }
    }
}

