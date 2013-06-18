namespace System.ServiceModel.ComIntegration
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Text;

    internal class RegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal static readonly RegistryHandle HKEY_CLASSES_ROOT = new RegistryHandle(new IntPtr(-2147483648), false);
        internal static readonly RegistryHandle HKEY_CURRENT_CONFIG = new RegistryHandle(new IntPtr(-2147483643), false);
        internal static readonly RegistryHandle HKEY_CURRENT_USER = new RegistryHandle(new IntPtr(-2147483647), false);
        internal static readonly RegistryHandle HKEY_DYN_DATA = new RegistryHandle(new IntPtr(-2147483642), false);
        internal static readonly RegistryHandle HKEY_LOCAL_MACHINE = new RegistryHandle(new IntPtr(-2147483646), false);
        internal static readonly RegistryHandle HKEY_PERFORMANCE_DATA = new RegistryHandle(new IntPtr(-2147483644), false);
        internal static readonly RegistryHandle HKEY_USERS = new RegistryHandle(new IntPtr(-2147483645), false);

        public RegistryHandle() : base(true)
        {
        }

        public RegistryHandle(IntPtr hKey, bool ownHandle) : base(ownHandle)
        {
            base.handle = hKey;
        }

        public bool DeleteKey(string key)
        {
            return (SafeNativeMethods.RegDeleteKey(this, key) == 0);
        }

        private static RegistryHandle Get32bitHKCR()
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, 0x20219, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        private static RegistryHandle Get32bitHKLMSubkey(string key)
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, 0x20219, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        private static RegistryHandle Get64bitHKCR()
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, 0x20119, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        private static RegistryHandle Get64bitHKLMSubkey(string key)
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, 0x20119, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        public static RegistryHandle GetBitnessHKCR(bool is64bit)
        {
            return GetCorrectBitnessHive(is64bit);
        }

        private static RegistryHandle GetCorrectBitnessHive(bool is64bit)
        {
            if (!is64bit || (IntPtr.Size != 8))
            {
                if (is64bit && (IntPtr.Size == 4))
                {
                    return Get64bitHKCR();
                }
                if (!is64bit && (IntPtr.Size == 8))
                {
                    return Get32bitHKCR();
                }
                if (is64bit || (IntPtr.Size != 4))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(50));
                }
            }
            return GetHKCR();
        }

        public static RegistryHandle GetCorrectBitnessHKLMSubkey(bool is64bit, string key)
        {
            if (!is64bit || (IntPtr.Size != 8))
            {
                if (is64bit && (IntPtr.Size == 4))
                {
                    return Get64bitHKLMSubkey(key);
                }
                if (!is64bit && (IntPtr.Size == 8))
                {
                    return Get32bitHKLMSubkey(key);
                }
                if (is64bit || (IntPtr.Size != 4))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(50));
                }
            }
            return GetHKLMSubkey(key);
        }

        private static RegistryHandle GetHKCR()
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, @"Software\Classes", 0, 0x20019, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        private static RegistryHandle GetHKLMSubkey(string key)
        {
            RegistryHandle hkResult = null;
            int error = SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, key, 0, 0x20019, out hkResult);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(hkResult);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            if ((hkResult != null) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(6));
        }

        internal static RegistryHandle GetNativeHKLMSubkey(string subKey, bool writeable)
        {
            RegistryHandle hkResult = null;
            int samDesired = 0x20119;
            if (writeable)
            {
                samDesired |= 0x20006;
            }
            if (((SafeNativeMethods.RegOpenKeyEx(HKEY_LOCAL_MACHINE, subKey, 0, samDesired, out hkResult) == 0) && (hkResult != null)) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            return null;
        }

        public string GetStringValue(string valName)
        {
            int lpType = 0;
            int lpcbData = 0;
            if ((SafeNativeMethods.RegQueryValueEx(this, valName, null, ref lpType, null, ref lpcbData) == 0) && (lpType == 1))
            {
                byte[] lpData = new byte[lpcbData];
                int num3 = SafeNativeMethods.RegQueryValueEx(this, valName, null, ref lpType, lpData, ref lpcbData);
                UnicodeEncoding encoding = new UnicodeEncoding();
                return encoding.GetString(lpData);
            }
            return null;
        }

        public StringCollection GetSubKeyNames()
        {
            int num = 0;
            int index = 0;
            StringCollection strings = new StringCollection();
            do
            {
                int len = 0;
                num = SafeNativeMethods.RegEnumKey(this, index, null, ref len);
                if (num == 0xea)
                {
                    StringBuilder lpName = new StringBuilder(len + 1);
                    num = SafeNativeMethods.RegEnumKey(this, index, lpName, ref len);
                    if (num == 0)
                    {
                        strings.Add(lpName.ToString());
                    }
                }
                index++;
            }
            while (num == 0);
            return strings;
        }

        internal unsafe object GetValue(string valName)
        {
            object obj2 = null;
            int lpType = 0;
            int lpcbData = 0;
            if (SafeNativeMethods.RegQueryValueEx(this, valName, null, ref lpType, null, ref lpcbData) != 0)
            {
                return obj2;
            }
            byte[] lpData = new byte[lpcbData];
            if (SafeNativeMethods.RegQueryValueEx(this, valName, null, ref lpType, lpData, ref lpcbData) != 0)
            {
                return obj2;
            }
            string str = new UnicodeEncoding().GetString(lpData);
            switch (lpType)
            {
                case 1:
                case 2:
                    return str.Trim(new char[1]);

                case 3:
                    return lpData;

                case 4:
                    fixed (byte* numRef = lpData)
                    {
                        obj2 = Marshal.ReadInt32((IntPtr) numRef);
                    }
                    return obj2;

                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                    return lpData;

                case 7:
                {
                    char[] separator = new char[1];
                    return str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                }
                case 11:
                    byte[] buffer3;
                    if (((buffer3 = lpData) != null) && (buffer3.Length != 0))
                    {
                        fixed (byte* numRef2 = buffer3)
                        {
                        Label_00F6:
                            obj2 = Marshal.ReadInt64((IntPtr) numRef2);
                        }
                        return obj2;
                    }
                    numRef2 = null;
                    goto Label_00F6;
            }
            return lpData;
        }

        public RegistryHandle OpenSubKey(string subkey)
        {
            RegistryHandle hkResult = null;
            if (((SafeNativeMethods.RegOpenKeyEx(this, subkey, 0, 0x20019, out hkResult) == 0) && (hkResult != null)) && !hkResult.IsInvalid)
            {
                return hkResult;
            }
            Utility.CloseInvalidOutSafeHandle(hkResult);
            return null;
        }

        protected override bool ReleaseHandle()
        {
            return (SafeNativeMethods.RegCloseKey(base.handle) == 0);
        }

        public void SetValue(string valName, string value)
        {
            int error = SafeNativeMethods.RegSetValueEx(this, valName, 0, 1, value, (value.Length * 2) + 2);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }
    }
}

