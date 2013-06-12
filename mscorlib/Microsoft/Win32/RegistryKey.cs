namespace Microsoft.Win32
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;

    [ComVisible(true)]
    public sealed class RegistryKey : MarshalByRefObject, IDisposable
    {
        private RegistryKeyPermissionCheck checkMode;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        [SecurityCritical]
        private SafeRegistryHandle hkey;
        internal static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(-2147483648);
        internal static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(-2147483643);
        internal static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(-2147483647);
        internal static readonly IntPtr HKEY_DYN_DATA = new IntPtr(-2147483642);
        internal static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(-2147483646);
        internal static readonly IntPtr HKEY_PERFORMANCE_DATA = new IntPtr(-2147483644);
        internal static readonly IntPtr HKEY_USERS = new IntPtr(-2147483645);
        private static readonly string[] hkeyNames = new string[] { "HKEY_CLASSES_ROOT", "HKEY_CURRENT_USER", "HKEY_LOCAL_MACHINE", "HKEY_USERS", "HKEY_PERFORMANCE_DATA", "HKEY_CURRENT_CONFIG", "HKEY_DYN_DATA" };
        private string keyName;
        private const int MaxKeyLength = 0xff;
        private const int MaxValueLength = 0x3fff;
        private RegistryView regView;
        private bool remoteKey;
        private int state;
        private const int STATE_DIRTY = 1;
        private const int STATE_PERF_DATA = 8;
        private const int STATE_SYSTEMKEY = 2;
        private const int STATE_WRITEACCESS = 4;

        [SecurityCritical]
        private RegistryKey(SafeRegistryHandle hkey, bool writable, RegistryView view) : this(hkey, writable, false, false, false, view)
        {
        }

        [SecurityCritical]
        private RegistryKey(SafeRegistryHandle hkey, bool writable, bool systemkey, bool remoteKey, bool isPerfData, RegistryView view)
        {
            this.hkey = hkey;
            this.keyName = "";
            this.remoteKey = remoteKey;
            this.regView = view;
            if (systemkey)
            {
                this.state |= 2;
            }
            if (writable)
            {
                this.state |= 4;
            }
            if (isPerfData)
            {
                this.state |= 8;
            }
            ValidateKeyView(view);
        }

        private RegistryValueKind CalculateValueKind(object value)
        {
            if (value is int)
            {
                return RegistryValueKind.DWord;
            }
            if (!(value is Array))
            {
                return RegistryValueKind.String;
            }
            if (value is byte[])
            {
                return RegistryValueKind.Binary;
            }
            if (!(value is string[]))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RegSetBadArrType", new object[] { value.GetType().Name }));
            }
            return RegistryValueKind.MultiString;
        }

        [SecurityCritical]
        private void CheckPermission(RegistryInternalCheck check, string item, bool subKeyWritable, RegistryKeyPermissionCheck subKeyCheck)
        {
            bool flag = false;
            RegistryPermissionAccess noAccess = RegistryPermissionAccess.NoAccess;
            string path = null;
            if (!CodeAccessSecurityEngine.QuickCheckForAllDemands())
            {
                switch (check)
                {
                    case RegistryInternalCheck.CheckSubKeyWritePermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetSubKeyWritePermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckSubKeyReadPermission:
                        if (!this.remoteKey)
                        {
                            flag = true;
                            this.GetSubKeyReadPermission(item, out noAccess, out path);
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckSubKeyCreatePermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetSubKeyCreatePermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckSubTreeReadPermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetSubTreeReadPermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckSubTreeWritePermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetSubTreeWritePermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckSubTreeReadWritePermission:
                        if (!this.remoteKey)
                        {
                            flag = true;
                            this.GetSubTreeReadWritePermission(item, out noAccess, out path);
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckValueWritePermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetValueWritePermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckValueCreatePermission:
                        if (!this.remoteKey)
                        {
                            if (this.checkMode == RegistryKeyPermissionCheck.Default)
                            {
                                flag = true;
                                this.GetValueCreatePermission(item, out noAccess, out path);
                            }
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckValueReadPermission:
                        if (this.checkMode == RegistryKeyPermissionCheck.Default)
                        {
                            flag = true;
                            this.GetValueReadPermission(item, out noAccess, out path);
                        }
                        break;

                    case RegistryInternalCheck.CheckKeyReadPermission:
                        if (this.checkMode == RegistryKeyPermissionCheck.Default)
                        {
                            flag = true;
                            this.GetKeyReadPermission(out noAccess, out path);
                        }
                        break;

                    case RegistryInternalCheck.CheckSubTreePermission:
                        if (subKeyCheck != RegistryKeyPermissionCheck.ReadSubTree)
                        {
                            if ((subKeyCheck == RegistryKeyPermissionCheck.ReadWriteSubTree) && (this.checkMode != RegistryKeyPermissionCheck.ReadWriteSubTree))
                            {
                                if (this.remoteKey)
                                {
                                    CheckUnmanagedCodePermission();
                                }
                                else
                                {
                                    flag = true;
                                    this.GetSubTreeReadWritePermission(item, out noAccess, out path);
                                }
                            }
                            break;
                        }
                        if (this.checkMode == RegistryKeyPermissionCheck.Default)
                        {
                            if (!this.remoteKey)
                            {
                                flag = true;
                                this.GetSubTreeReadPermission(item, out noAccess, out path);
                                break;
                            }
                            CheckUnmanagedCodePermission();
                        }
                        break;

                    case RegistryInternalCheck.CheckOpenSubKeyWithWritablePermission:
                        if (this.checkMode != RegistryKeyPermissionCheck.Default)
                        {
                            if (subKeyWritable && (this.checkMode == RegistryKeyPermissionCheck.ReadSubTree))
                            {
                                if (this.remoteKey)
                                {
                                    CheckUnmanagedCodePermission();
                                }
                                else
                                {
                                    flag = true;
                                    this.GetSubTreeReadWritePermission(item, out noAccess, out path);
                                }
                            }
                            break;
                        }
                        if (!this.remoteKey)
                        {
                            flag = true;
                            this.GetSubKeyReadPermission(item, out noAccess, out path);
                            break;
                        }
                        CheckUnmanagedCodePermission();
                        break;

                    case RegistryInternalCheck.CheckOpenSubKeyPermission:
                        if ((subKeyCheck == RegistryKeyPermissionCheck.Default) && (this.checkMode == RegistryKeyPermissionCheck.Default))
                        {
                            if (!this.remoteKey)
                            {
                                flag = true;
                                this.GetSubKeyReadPermission(item, out noAccess, out path);
                                break;
                            }
                            CheckUnmanagedCodePermission();
                        }
                        break;
                }
                if (flag)
                {
                    new RegistryPermission(noAccess, path).Demand();
                }
            }
        }

        [SecurityCritical]
        private static void CheckUnmanagedCodePermission()
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
        }

        public void Close()
        {
            this.Dispose(true);
        }

        [SecurityCritical]
        private bool ContainsRegistryValue(string name)
        {
            int lpType = 0;
            int lpcbData = 0;
            return (Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData) == 0);
        }

        public RegistryKey CreateSubKey(string subkey)
        {
            return this.CreateSubKey(subkey, this.checkMode);
        }

        [ComVisible(false)]
        public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, null, RegistryOptions.None);
        }

        [ComVisible(false)]
        public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions options)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, null, options);
        }

        [ComVisible(false)]
        public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistrySecurity registrySecurity)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, registrySecurity, RegistryOptions.None);
        }

        [ComVisible(false)]
        public RegistryKey CreateSubKey(string subkey, RegistryKeyPermissionCheck permissionCheck, RegistryOptions registryOptions, RegistrySecurity registrySecurity)
        {
            return this.CreateSubKeyInternal(subkey, permissionCheck, registrySecurity, registryOptions);
        }

        [ComVisible(false), SecuritySafeCritical]
        private unsafe RegistryKey CreateSubKeyInternal(string subkey, RegistryKeyPermissionCheck permissionCheck, object registrySecurityObj, RegistryOptions registryOptions)
        {
            ValidateKeyOptions(registryOptions);
            ValidateKeyName(subkey);
            ValidateKeyMode(permissionCheck);
            this.EnsureWriteable();
            subkey = FixupName(subkey);
            if (!this.remoteKey)
            {
                RegistryKey key = this.InternalOpenSubKey(subkey, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree);
                if (key != null)
                {
                    this.CheckPermission(RegistryInternalCheck.CheckSubKeyWritePermission, subkey, false, RegistryKeyPermissionCheck.Default);
                    this.CheckPermission(RegistryInternalCheck.CheckSubTreePermission, subkey, false, permissionCheck);
                    key.checkMode = permissionCheck;
                    return key;
                }
            }
            this.CheckPermission(RegistryInternalCheck.CheckSubKeyCreatePermission, subkey, false, RegistryKeyPermissionCheck.Default);
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            RegistrySecurity security = (RegistrySecurity) registrySecurityObj;
            if (security != null)
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                byte[] securityDescriptorBinaryForm = security.GetSecurityDescriptorBinaryForm();
                byte* pDest = stackalloc byte[(IntPtr) securityDescriptorBinaryForm.Length];
                Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                structure.pSecurityDescriptor = pDest;
            }
            int lpdwDisposition = 0;
            SafeRegistryHandle hkResult = null;
            int errorCode = Win32Native.RegCreateKeyEx(this.hkey, subkey, 0, null, (int) registryOptions, GetRegistryKeyAccess(permissionCheck != RegistryKeyPermissionCheck.ReadSubTree, this.regView), structure, out hkResult, out lpdwDisposition);
            if ((errorCode == 0) && !hkResult.IsInvalid)
            {
                RegistryKey key2 = new RegistryKey(hkResult, permissionCheck != RegistryKeyPermissionCheck.ReadSubTree, false, this.remoteKey, false, this.regView);
                this.CheckPermission(RegistryInternalCheck.CheckSubTreePermission, subkey, false, permissionCheck);
                key2.checkMode = permissionCheck;
                if (subkey.Length == 0)
                {
                    key2.keyName = this.keyName;
                    return key2;
                }
                key2.keyName = this.keyName + @"\" + subkey;
                return key2;
            }
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, this.keyName + @"\" + subkey);
            }
            return null;
        }

        public void DeleteSubKey(string subkey)
        {
            this.DeleteSubKey(subkey, true);
        }

        [SecuritySafeCritical]
        public void DeleteSubKey(string subkey, bool throwOnMissingSubKey)
        {
            ValidateKeyName(subkey);
            this.EnsureWriteable();
            subkey = FixupName(subkey);
            this.CheckPermission(RegistryInternalCheck.CheckSubKeyWritePermission, subkey, false, RegistryKeyPermissionCheck.Default);
            RegistryKey key = this.InternalOpenSubKey(subkey, false);
            if (key != null)
            {
                int num;
                try
                {
                    if (key.InternalSubKeyCount() > 0)
                    {
                        ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_RegRemoveSubKey);
                    }
                }
                finally
                {
                    key.Close();
                }
                try
                {
                    num = Win32Native.RegDeleteKeyEx(this.hkey, subkey, (int) this.regView, 0);
                }
                catch (EntryPointNotFoundException)
                {
                    num = Win32Native.RegDeleteKey(this.hkey, subkey);
                }
                switch (num)
                {
                    case 0:
                        return;

                    case 2:
                        if (throwOnMissingSubKey)
                        {
                            ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
                            return;
                        }
                        return;
                }
                this.Win32Error(num, null);
            }
            else if (throwOnMissingSubKey)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
            }
        }

        [SecuritySafeCritical]
        public void DeleteSubKeyTree(string subkey)
        {
            this.DeleteSubKeyTree(subkey, true);
        }

        [ComVisible(false), SecuritySafeCritical]
        public void DeleteSubKeyTree(string subkey, bool throwOnMissingSubKey)
        {
            ValidateKeyName(subkey);
            if ((subkey.Length == 0) && this.IsSystemKey())
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyDelHive);
            }
            this.EnsureWriteable();
            subkey = FixupName(subkey);
            this.CheckPermission(RegistryInternalCheck.CheckSubTreeWritePermission, subkey, false, RegistryKeyPermissionCheck.Default);
            RegistryKey key = this.InternalOpenSubKey(subkey, true);
            if (key != null)
            {
                int num2;
                try
                {
                    if (key.InternalSubKeyCount() > 0)
                    {
                        string[] subKeyNames = key.InternalGetSubKeyNames();
                        for (int i = 0; i < subKeyNames.Length; i++)
                        {
                            key.DeleteSubKeyTreeInternal(subKeyNames[i]);
                        }
                    }
                }
                finally
                {
                    key.Close();
                }
                try
                {
                    num2 = Win32Native.RegDeleteKeyEx(this.hkey, subkey, (int) this.regView, 0);
                }
                catch (EntryPointNotFoundException)
                {
                    num2 = Win32Native.RegDeleteKey(this.hkey, subkey);
                }
                if (num2 != 0)
                {
                    this.Win32Error(num2, null);
                }
            }
            else if (throwOnMissingSubKey)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
            }
        }

        [SecurityCritical]
        private void DeleteSubKeyTreeInternal(string subkey)
        {
            RegistryKey key = this.InternalOpenSubKey(subkey, true);
            if (key != null)
            {
                int num2;
                try
                {
                    if (key.InternalSubKeyCount() > 0)
                    {
                        string[] subKeyNames = key.InternalGetSubKeyNames();
                        for (int i = 0; i < subKeyNames.Length; i++)
                        {
                            key.DeleteSubKeyTreeInternal(subKeyNames[i]);
                        }
                    }
                }
                finally
                {
                    key.Close();
                }
                try
                {
                    num2 = Win32Native.RegDeleteKeyEx(this.hkey, subkey, (int) this.regView, 0);
                }
                catch (EntryPointNotFoundException)
                {
                    num2 = Win32Native.RegDeleteKey(this.hkey, subkey);
                }
                if (num2 != 0)
                {
                    this.Win32Error(num2, null);
                }
            }
            else
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyAbsent);
            }
        }

        public void DeleteValue(string name)
        {
            this.DeleteValue(name, true);
        }

        [SecuritySafeCritical]
        public void DeleteValue(string name, bool throwOnMissingValue)
        {
            this.EnsureWriteable();
            this.CheckPermission(RegistryInternalCheck.CheckValueWritePermission, name, false, RegistryKeyPermissionCheck.Default);
            int num = Win32Native.RegDeleteValue(this.hkey, name);
            if (((num == 2) || (num == 0xce)) && throwOnMissingValue)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSubKeyValueAbsent);
            }
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        private void Dispose(bool disposing)
        {
            if (this.hkey != null)
            {
                if (!this.IsSystemKey())
                {
                    try
                    {
                        try
                        {
                            this.hkey.Dispose();
                        }
                        catch (IOException)
                        {
                        }
                        return;
                    }
                    finally
                    {
                        this.hkey = null;
                    }
                }
                if (disposing && this.IsPerfDataKey())
                {
                    SafeRegistryHandle.RegCloseKey(HKEY_PERFORMANCE_DATA);
                }
            }
        }

        [SecurityCritical]
        private void EnsureNotDisposed()
        {
            if (this.hkey == null)
            {
                ThrowHelper.ThrowObjectDisposedException(this.keyName, ExceptionResource.ObjectDisposed_RegKeyClosed);
            }
        }

        [SecurityCritical]
        private void EnsureWriteable()
        {
            this.EnsureNotDisposed();
            if (!this.IsWritable())
            {
                ThrowHelper.ThrowUnauthorizedAccessException(ExceptionResource.UnauthorizedAccess_RegistryNoWrite);
            }
        }

        internal static string FixupName(string name)
        {
            if (name.IndexOf('\\') == -1)
            {
                return name;
            }
            StringBuilder path = new StringBuilder(name);
            FixupPath(path);
            int num = path.Length - 1;
            if ((num >= 0) && (path[num] == '\\'))
            {
                path.Length = num;
            }
            return path.ToString();
        }

        private static void FixupPath(StringBuilder path)
        {
            int num2;
            int length = path.Length;
            bool flag = false;
            char ch = 0xffff;
            for (num2 = 1; num2 < (length - 1); num2++)
            {
                if (path[num2] == '\\')
                {
                    num2++;
                    while (num2 < length)
                    {
                        if (path[num2] != '\\')
                        {
                            break;
                        }
                        path[num2] = ch;
                        num2++;
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                num2 = 0;
                int num3 = 0;
                while (num2 < length)
                {
                    if (path[num2] == ch)
                    {
                        num2++;
                    }
                    else
                    {
                        path[num3] = path[num2];
                        num2++;
                        num3++;
                    }
                }
                path.Length += num3 - num2;
            }
        }

        [SecuritySafeCritical]
        public void Flush()
        {
            if ((this.hkey != null) && this.IsDirty())
            {
                Win32Native.RegFlushKey(this.hkey);
            }
        }

        [SecurityCritical, ComVisible(false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static RegistryKey FromHandle(SafeRegistryHandle handle)
        {
            return FromHandle(handle, RegistryView.Default);
        }

        [SecurityCritical, ComVisible(false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static RegistryKey FromHandle(SafeRegistryHandle handle, RegistryView view)
        {
            if (handle == null)
            {
                throw new ArgumentNullException("handle");
            }
            ValidateKeyView(view);
            return new RegistryKey(handle, true, view);
        }

        [SecuritySafeCritical]
        public RegistrySecurity GetAccessControl()
        {
            return this.GetAccessControl(AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecuritySafeCritical]
        public RegistrySecurity GetAccessControl(AccessControlSections includeSections)
        {
            this.EnsureNotDisposed();
            return new RegistrySecurity(this.hkey, this.keyName, includeSections);
        }

        [SecurityCritical]
        internal static RegistryKey GetBaseKey(IntPtr hKey)
        {
            return GetBaseKey(hKey, RegistryView.Default);
        }

        [SecurityCritical]
        internal static RegistryKey GetBaseKey(IntPtr hKey, RegistryView view)
        {
            int index = ((int) hKey) & 0xfffffff;
            bool isPerfData = hKey == HKEY_PERFORMANCE_DATA;
            return new RegistryKey(new SafeRegistryHandle(hKey, isPerfData), true, true, false, isPerfData, view) { checkMode = RegistryKeyPermissionCheck.Default, keyName = hkeyNames[index] };
        }

        private void GetKeyReadPermission(out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Read;
            path = this.keyName + @"\.";
        }

        private static int GetRegistryKeyAccess(RegistryKeyPermissionCheck mode, RegistryView view)
        {
            int num = 0;
            switch (mode)
            {
                case RegistryKeyPermissionCheck.Default:
                case RegistryKeyPermissionCheck.ReadSubTree:
                    num = 0x20019;
                    break;

                case RegistryKeyPermissionCheck.ReadWriteSubTree:
                    num = 0x2001f;
                    break;
            }
            return (num | view);
        }

        private static int GetRegistryKeyAccess(bool isWritable, RegistryView view)
        {
            int num;
            if (!isWritable)
            {
                num = 0x20019;
            }
            else
            {
                num = 0x2001f;
            }
            return (num | view);
        }

        private void GetSubKeyCreatePermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Create;
            path = this.keyName + @"\" + subkeyName + @"\.";
        }

        [SecuritySafeCritical]
        public string[] GetSubKeyNames()
        {
            this.CheckPermission(RegistryInternalCheck.CheckKeyReadPermission, null, false, RegistryKeyPermissionCheck.Default);
            return this.InternalGetSubKeyNames();
        }

        private RegistryKeyPermissionCheck GetSubKeyPermissonCheck(bool subkeyWritable)
        {
            if (this.checkMode == RegistryKeyPermissionCheck.Default)
            {
                return this.checkMode;
            }
            if (subkeyWritable)
            {
                return RegistryKeyPermissionCheck.ReadWriteSubTree;
            }
            return RegistryKeyPermissionCheck.ReadSubTree;
        }

        private void GetSubKeyReadPermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Read;
            path = this.keyName + @"\" + subkeyName + @"\.";
        }

        private void GetSubKeyWritePermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Write;
            path = this.keyName + @"\" + subkeyName + @"\.";
        }

        private void GetSubTreeReadPermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Read;
            path = this.keyName + @"\" + subkeyName + @"\";
        }

        private void GetSubTreeReadWritePermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Write | RegistryPermissionAccess.Read;
            path = this.keyName + @"\" + subkeyName;
        }

        private void GetSubTreeWritePermission(string subkeyName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Write;
            path = this.keyName + @"\" + subkeyName + @"\";
        }

        [SecuritySafeCritical]
        public object GetValue(string name)
        {
            this.CheckPermission(RegistryInternalCheck.CheckValueReadPermission, name, false, RegistryKeyPermissionCheck.Default);
            return this.InternalGetValue(name, null, false, true);
        }

        [SecuritySafeCritical]
        public object GetValue(string name, object defaultValue)
        {
            this.CheckPermission(RegistryInternalCheck.CheckValueReadPermission, name, false, RegistryKeyPermissionCheck.Default);
            return this.InternalGetValue(name, defaultValue, false, true);
        }

        [ComVisible(false), SecuritySafeCritical]
        public object GetValue(string name, object defaultValue, RegistryValueOptions options)
        {
            if ((options < RegistryValueOptions.None) || (options > RegistryValueOptions.DoNotExpandEnvironmentNames))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) options }), "options");
            }
            bool doNotExpand = options == RegistryValueOptions.DoNotExpandEnvironmentNames;
            this.CheckPermission(RegistryInternalCheck.CheckValueReadPermission, name, false, RegistryKeyPermissionCheck.Default);
            return this.InternalGetValue(name, defaultValue, doNotExpand, true);
        }

        private void GetValueCreatePermission(string valueName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Create;
            path = this.keyName + @"\" + valueName;
        }

        [ComVisible(false), SecuritySafeCritical]
        public RegistryValueKind GetValueKind(string name)
        {
            this.CheckPermission(RegistryInternalCheck.CheckValueReadPermission, name, false, RegistryKeyPermissionCheck.Default);
            this.EnsureNotDisposed();
            int lpType = 0;
            int lpcbData = 0;
            int errorCode = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            if (lpType == 0)
            {
                return RegistryValueKind.None;
            }
            if (!Enum.IsDefined(typeof(RegistryValueKind), lpType))
            {
                return RegistryValueKind.Unknown;
            }
            return (RegistryValueKind) lpType;
        }

        [SecuritySafeCritical]
        public string[] GetValueNames()
        {
            this.CheckPermission(RegistryInternalCheck.CheckKeyReadPermission, null, false, RegistryKeyPermissionCheck.Default);
            this.EnsureNotDisposed();
            int num = this.InternalValueCount();
            string[] strArray = new string[num];
            if (num > 0)
            {
                StringBuilder lpValueName = new StringBuilder(0x4000);
                for (int i = 0; i < num; i++)
                {
                    int capacity = lpValueName.Capacity;
                    int errorCode = Win32Native.RegEnumValue(this.hkey, i, lpValueName, ref capacity, Win32Native.NULL, null, null, null);
                    if ((errorCode != 0) && (!this.IsPerfDataKey() || (errorCode != 0xea)))
                    {
                        this.Win32Error(errorCode, null);
                    }
                    strArray[i] = lpValueName.ToString();
                }
            }
            return strArray;
        }

        private void GetValueReadPermission(string valueName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Read;
            path = this.keyName + @"\" + valueName;
        }

        private void GetValueWritePermission(string valueName, out RegistryPermissionAccess access, out string path)
        {
            access = RegistryPermissionAccess.Write;
            path = this.keyName + @"\" + valueName;
        }

        [SecurityCritical]
        internal string[] InternalGetSubKeyNames()
        {
            this.EnsureNotDisposed();
            int num = this.InternalSubKeyCount();
            string[] strArray = new string[num];
            if (num > 0)
            {
                StringBuilder lpName = new StringBuilder(0x100);
                for (int i = 0; i < num; i++)
                {
                    int capacity = lpName.Capacity;
                    int errorCode = Win32Native.RegEnumKeyEx(this.hkey, i, lpName, out capacity, null, null, null, null);
                    if (errorCode != 0)
                    {
                        this.Win32Error(errorCode, null);
                    }
                    strArray[i] = lpName.ToString();
                }
            }
            return strArray;
        }

        [SecurityCritical]
        internal object InternalGetValue(string name, object defaultValue, bool doNotExpand, bool checkSecurity)
        {
            byte[] buffer2;
            if (checkSecurity)
            {
                this.EnsureNotDisposed();
            }
            object obj2 = defaultValue;
            int lpType = 0;
            int lpcbData = 0;
            int num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, (byte[]) null, ref lpcbData);
            if (num3 != 0)
            {
                if (this.IsPerfDataKey())
                {
                    int num6;
                    int num4 = 0xfde8;
                    int num5 = num4;
                    byte[] buffer = new byte[num4];
                    while (0xea == (num6 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, buffer, ref num5)))
                    {
                        if (num4 == 0x7fffffff)
                        {
                            this.Win32Error(num6, name);
                        }
                        else if (num4 > 0x3fffffff)
                        {
                            num4 = 0x7fffffff;
                        }
                        else
                        {
                            num4 *= 2;
                        }
                        num5 = num4;
                        buffer = new byte[num4];
                    }
                    if (num6 != 0)
                    {
                        this.Win32Error(num6, name);
                    }
                    return buffer;
                }
                if (num3 != 0xea)
                {
                    return obj2;
                }
            }
            if (lpcbData < 0)
            {
                lpcbData = 0;
            }
            switch (lpType)
            {
                case 0:
                case 3:
                case 5:
                    break;

                case 1:
                {
                    if ((lpcbData % 2) == 1)
                    {
                        try
                        {
                            lpcbData++;
                        }
                        catch (OverflowException exception)
                        {
                            throw new IOException(Environment.GetResourceString("Arg_RegGetOverflowBug"), exception);
                        }
                    }
                    char[] chArray = new char[lpcbData / 2];
                    num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, chArray, ref lpcbData);
                    if ((chArray.Length > 0) && (chArray[chArray.Length - 1] == '\0'))
                    {
                        return new string(chArray, 0, chArray.Length - 1);
                    }
                    return new string(chArray);
                }
                case 2:
                {
                    if ((lpcbData % 2) == 1)
                    {
                        try
                        {
                            lpcbData++;
                        }
                        catch (OverflowException exception2)
                        {
                            throw new IOException(Environment.GetResourceString("Arg_RegGetOverflowBug"), exception2);
                        }
                    }
                    char[] chArray2 = new char[lpcbData / 2];
                    num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, chArray2, ref lpcbData);
                    if ((chArray2.Length > 0) && (chArray2[chArray2.Length - 1] == '\0'))
                    {
                        obj2 = new string(chArray2, 0, chArray2.Length - 1);
                    }
                    else
                    {
                        obj2 = new string(chArray2);
                    }
                    if (!doNotExpand)
                    {
                        obj2 = Environment.ExpandEnvironmentVariables((string) obj2);
                    }
                    return obj2;
                }
                case 4:
                {
                    if (lpcbData > 4)
                    {
                        goto Label_0120;
                    }
                    int num8 = 0;
                    num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, ref num8, ref lpcbData);
                    return num8;
                }
                case 6:
                case 8:
                case 9:
                case 10:
                    return obj2;

                case 7:
                {
                    if ((lpcbData % 2) == 1)
                    {
                        try
                        {
                            lpcbData++;
                        }
                        catch (OverflowException exception3)
                        {
                            throw new IOException(Environment.GetResourceString("Arg_RegGetOverflowBug"), exception3);
                        }
                    }
                    char[] chArray3 = new char[lpcbData / 2];
                    num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, chArray3, ref lpcbData);
                    if ((chArray3.Length > 0) && (chArray3[chArray3.Length - 1] != '\0'))
                    {
                        try
                        {
                            char[] chArray4 = new char[chArray3.Length + 1];
                            for (int i = 0; i < chArray3.Length; i++)
                            {
                                chArray4[i] = chArray3[i];
                            }
                            chArray4[chArray4.Length - 1] = '\0';
                            chArray3 = chArray4;
                        }
                        catch (OverflowException exception4)
                        {
                            throw new IOException(Environment.GetResourceString("Arg_RegGetOverflowBug"), exception4);
                        }
                        chArray3[chArray3.Length - 1] = '\0';
                    }
                    IList<string> list = new List<string>();
                    int startIndex = 0;
                    int length = chArray3.Length;
                    while ((num3 == 0) && (startIndex < length))
                    {
                        int index = startIndex;
                        while ((index < length) && (chArray3[index] != '\0'))
                        {
                            index++;
                        }
                        if (index < length)
                        {
                            if ((index - startIndex) > 0)
                            {
                                list.Add(new string(chArray3, startIndex, index - startIndex));
                            }
                            else if (index != (length - 1))
                            {
                                list.Add(string.Empty);
                            }
                        }
                        else
                        {
                            list.Add(new string(chArray3, startIndex, length - startIndex));
                        }
                        startIndex = index + 1;
                    }
                    obj2 = new string[list.Count];
                    list.CopyTo((string[]) obj2, 0);
                    return obj2;
                }
                case 11:
                    goto Label_0120;

                default:
                    return obj2;
            }
        Label_00FC:
            buffer2 = new byte[lpcbData];
            num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, buffer2, ref lpcbData);
            return buffer2;
        Label_0120:
            if (lpcbData > 8)
            {
                goto Label_00FC;
            }
            long lpData = 0L;
            num3 = Win32Native.RegQueryValueEx(this.hkey, name, null, ref lpType, ref lpData, ref lpcbData);
            return lpData;
        }

        [SecurityCritical]
        internal RegistryKey InternalOpenSubKey(string name, bool writable)
        {
            ValidateKeyName(name);
            this.EnsureNotDisposed();
            SafeRegistryHandle hkResult = null;
            if ((Win32Native.RegOpenKeyEx(this.hkey, name, 0, GetRegistryKeyAccess(writable, this.regView), out hkResult) == 0) && !hkResult.IsInvalid)
            {
                return new RegistryKey(hkResult, writable, false, this.remoteKey, false, this.regView) { keyName = this.keyName + @"\" + name };
            }
            return null;
        }

        [SecurityCritical]
        private RegistryKey InternalOpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, int rights)
        {
            ValidateKeyName(name);
            ValidateKeyMode(permissionCheck);
            ValidateKeyRights(rights);
            this.EnsureNotDisposed();
            name = FixupName(name);
            this.CheckPermission(RegistryInternalCheck.CheckOpenSubKeyPermission, name, false, permissionCheck);
            this.CheckPermission(RegistryInternalCheck.CheckSubTreePermission, name, false, permissionCheck);
            SafeRegistryHandle hkResult = null;
            int num = Win32Native.RegOpenKeyEx(this.hkey, name, 0, rights | this.regView, out hkResult);
            if ((num == 0) && !hkResult.IsInvalid)
            {
                return new RegistryKey(hkResult, permissionCheck == RegistryKeyPermissionCheck.ReadWriteSubTree, false, this.remoteKey, false, this.regView) { keyName = this.keyName + @"\" + name, checkMode = permissionCheck };
            }
            if ((num == 5) || (num == 0x542))
            {
                ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
            }
            return null;
        }

        [SecurityCritical]
        internal int InternalSubKeyCount()
        {
            this.EnsureNotDisposed();
            int lpcSubKeys = 0;
            int lpcValues = 0;
            int errorCode = Win32Native.RegQueryInfoKey(this.hkey, null, null, Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            return lpcSubKeys;
        }

        [SecurityCritical]
        internal int InternalValueCount()
        {
            this.EnsureNotDisposed();
            int lpcValues = 0;
            int lpcSubKeys = 0;
            int errorCode = Win32Native.RegQueryInfoKey(this.hkey, null, null, Win32Native.NULL, ref lpcSubKeys, null, null, ref lpcValues, null, null, null, null);
            if (errorCode != 0)
            {
                this.Win32Error(errorCode, null);
            }
            return lpcValues;
        }

        private bool IsDirty()
        {
            return ((this.state & 1) != 0);
        }

        private bool IsPerfDataKey()
        {
            return ((this.state & 8) != 0);
        }

        private bool IsSystemKey()
        {
            return ((this.state & 2) != 0);
        }

        private bool IsWritable()
        {
            return ((this.state & 4) != 0);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public static RegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
        {
            ValidateKeyView(view);
            CheckUnmanagedCodePermission();
            return GetBaseKey((IntPtr) hKey, view);
        }

        [SecuritySafeCritical]
        public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey, string machineName)
        {
            return OpenRemoteBaseKey(hKey, machineName, RegistryView.Default);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public static RegistryKey OpenRemoteBaseKey(RegistryHive hKey, string machineName, RegistryView view)
        {
            if (machineName == null)
            {
                throw new ArgumentNullException("machineName");
            }
            int index = ((int) hKey) & 0xfffffff;
            if (((index < 0) || (index >= hkeyNames.Length)) || ((((long) hKey) & 0xfffffff0L) != 0x80000000L))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RegKeyOutOfRange"));
            }
            ValidateKeyView(view);
            CheckUnmanagedCodePermission();
            SafeRegistryHandle result = null;
            int errorCode = Win32Native.RegConnectRegistry(machineName, new SafeRegistryHandle(new IntPtr((int) hKey), false), out result);
            if (errorCode == 0x45a)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DllInitFailure"));
            }
            if (errorCode != 0)
            {
                Win32ErrorStatic(errorCode, null);
            }
            if (result.IsInvalid)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RegKeyNoRemoteConnect", new object[] { machineName }));
            }
            return new RegistryKey(result, true, false, true, ((IntPtr) ((long) hKey)) == HKEY_PERFORMANCE_DATA, view) { checkMode = RegistryKeyPermissionCheck.Default, keyName = hkeyNames[index] };
        }

        public RegistryKey OpenSubKey(string name)
        {
            return this.OpenSubKey(name, false);
        }

        [ComVisible(false), SecuritySafeCritical]
        public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck)
        {
            ValidateKeyMode(permissionCheck);
            return this.InternalOpenSubKey(name, permissionCheck, GetRegistryKeyAccess(permissionCheck, this.regView));
        }

        [SecuritySafeCritical]
        public RegistryKey OpenSubKey(string name, bool writable)
        {
            ValidateKeyName(name);
            this.EnsureNotDisposed();
            name = FixupName(name);
            this.CheckPermission(RegistryInternalCheck.CheckOpenSubKeyWithWritablePermission, name, writable, RegistryKeyPermissionCheck.Default);
            SafeRegistryHandle hkResult = null;
            int num = Win32Native.RegOpenKeyEx(this.hkey, name, 0, GetRegistryKeyAccess(writable, this.regView), out hkResult);
            if ((num == 0) && !hkResult.IsInvalid)
            {
                return new RegistryKey(hkResult, writable, false, this.remoteKey, false, this.regView) { checkMode = this.GetSubKeyPermissonCheck(writable), keyName = this.keyName + @"\" + name };
            }
            if ((num == 5) || (num == 0x542))
            {
                ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
            }
            return null;
        }

        [ComVisible(false), SecuritySafeCritical]
        public RegistryKey OpenSubKey(string name, RegistryKeyPermissionCheck permissionCheck, RegistryRights rights)
        {
            return this.InternalOpenSubKey(name, permissionCheck, (int) rights);
        }

        [SecuritySafeCritical]
        public void SetAccessControl(RegistrySecurity registrySecurity)
        {
            this.EnsureWriteable();
            if (registrySecurity == null)
            {
                throw new ArgumentNullException("registrySecurity");
            }
            registrySecurity.Persist(this.hkey, this.keyName);
        }

        private void SetDirty()
        {
            this.state |= 1;
        }

        public void SetValue(string name, object value)
        {
            this.SetValue(name, value, RegistryValueKind.Unknown);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public unsafe void SetValue(string name, object value, RegistryValueKind valueKind)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }
            if ((name != null) && (name.Length > 0x3fff))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RegValStrLenBug"));
            }
            if (!Enum.IsDefined(typeof(RegistryValueKind), valueKind))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_RegBadKeyKind"), "valueKind");
            }
            this.EnsureWriteable();
            if (!this.remoteKey && this.ContainsRegistryValue(name))
            {
                this.CheckPermission(RegistryInternalCheck.CheckValueWritePermission, name, false, RegistryKeyPermissionCheck.Default);
            }
            else
            {
                this.CheckPermission(RegistryInternalCheck.CheckValueCreatePermission, name, false, RegistryKeyPermissionCheck.Default);
            }
            if (valueKind == RegistryValueKind.Unknown)
            {
                valueKind = this.CalculateValueKind(value);
            }
            int errorCode = 0;
            try
            {
                string[] strArray;
                int num2;
                int num3;
                byte[] buffer2;
                switch (valueKind)
                {
                    case RegistryValueKind.None:
                    case RegistryValueKind.Binary:
                        goto Label_0201;

                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                    {
                        string str = value.ToString();
                        errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, valueKind, str, (str.Length * 2) + 2);
                        goto Label_0292;
                    }
                    case RegistryValueKind.DWord:
                    {
                        int num5 = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                        errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.DWord, ref num5, 4);
                        goto Label_0292;
                    }
                    case RegistryValueKind.MultiString:
                        strArray = (string[]) ((string[]) value).Clone();
                        num2 = 0;
                        num3 = 0;
                        goto Label_0137;

                    case RegistryValueKind.QWord:
                    {
                        long num6 = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                        errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.QWord, ref num6, 8);
                        goto Label_0292;
                    }
                    default:
                        goto Label_0292;
                }
            Label_0114:
                if (strArray[num3] == null)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetStrArrNull);
                }
                num2 += (strArray[num3].Length + 1) * 2;
                num3++;
            Label_0137:
                if (num3 < strArray.Length)
                {
                    goto Label_0114;
                }
                num2 += 2;
                byte[] lpData = new byte[num2];
                try
                {
                    fixed (byte* numRef = lpData)
                    {
                        IntPtr dest = new IntPtr((void*) numRef);
                        for (int i = 0; i < strArray.Length; i++)
                        {
                            string.InternalCopy(strArray[i], dest, strArray[i].Length * 2);
                            dest = new IntPtr(((long) dest) + (strArray[i].Length * 2));
                            *((short*) dest.ToPointer()) = 0;
                            dest = new IntPtr(((long) dest) + 2L);
                        }
                        *((short*) dest.ToPointer()) = 0;
                        dest = new IntPtr(((long) dest) + 2L);
                        errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, RegistryValueKind.MultiString, lpData, num2);
                        goto Label_0292;
                    }
                }
                finally
                {
                    numRef = null;
                }
            Label_0201:
                buffer2 = (byte[]) value;
                errorCode = Win32Native.RegSetValueEx(this.hkey, name, 0, (valueKind == RegistryValueKind.None) ? RegistryValueKind.Unknown : RegistryValueKind.Binary, buffer2, buffer2.Length);
            }
            catch (OverflowException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
            }
            catch (InvalidOperationException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
            }
            catch (FormatException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
            }
            catch (InvalidCastException)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegSetMismatchedKind);
            }
        Label_0292:
            if (errorCode == 0)
            {
                this.SetDirty();
            }
            else
            {
                this.Win32Error(errorCode, null);
            }
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            this.EnsureNotDisposed();
            return this.keyName;
        }

        private static void ValidateKeyMode(RegistryKeyPermissionCheck mode)
        {
            if ((mode < RegistryKeyPermissionCheck.Default) || (mode > RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidRegistryKeyPermissionCheck, ExceptionArgument.mode);
            }
        }

        private static void ValidateKeyName(string name)
        {
            if (name == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name);
            }
            int index = name.IndexOf(@"\", StringComparison.OrdinalIgnoreCase);
            int startIndex = 0;
            while (index != -1)
            {
                if ((index - startIndex) > 0xff)
                {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyStrLenBug);
                }
                startIndex = index + 1;
                index = name.IndexOf(@"\", startIndex, StringComparison.OrdinalIgnoreCase);
            }
            if ((name.Length - startIndex) > 0xff)
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RegKeyStrLenBug);
            }
        }

        private static void ValidateKeyOptions(RegistryOptions options)
        {
            if ((options < RegistryOptions.None) || (options > RegistryOptions.Volatile))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidRegistryOptionsCheck, ExceptionArgument.options);
            }
        }

        private static void ValidateKeyRights(int rights)
        {
            if ((rights & -983104) != 0)
            {
                ThrowHelper.ThrowSecurityException(ExceptionResource.Security_RegistryPermission);
            }
        }

        private static void ValidateKeyView(RegistryView view)
        {
            if (((view != RegistryView.Default) && (view != RegistryView.Registry32)) && (view != RegistryView.Registry64))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidRegistryViewCheck, ExceptionArgument.view);
            }
        }

        [SecuritySafeCritical]
        internal void Win32Error(int errorCode, string str)
        {
            switch (errorCode)
            {
                case 2:
                    throw new IOException(Environment.GetResourceString("Arg_RegKeyNotFound"), errorCode);

                case 5:
                    if (str != null)
                    {
                        throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_RegistryKeyGeneric_Key", new object[] { str }));
                    }
                    throw new UnauthorizedAccessException();

                case 6:
                    if (!this.IsPerfDataKey())
                    {
                        this.hkey.SetHandleAsInvalid();
                        this.hkey = null;
                    }
                    break;
            }
            throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
        }

        [SecuritySafeCritical]
        internal static void Win32ErrorStatic(int errorCode, string str)
        {
            if (errorCode != 5)
            {
                throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
            }
            if (str != null)
            {
                throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_RegistryKeyGeneric_Key", new object[] { str }));
            }
            throw new UnauthorizedAccessException();
        }

        [ComVisible(false)]
        public SafeRegistryHandle Handle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                SafeRegistryHandle handle;
                this.EnsureNotDisposed();
                int errorCode = 6;
                if (!this.IsSystemKey())
                {
                    return this.hkey;
                }
                IntPtr zero = IntPtr.Zero;
                switch (this.keyName)
                {
                    case "HKEY_CLASSES_ROOT":
                        zero = HKEY_CLASSES_ROOT;
                        break;

                    case "HKEY_CURRENT_USER":
                        zero = HKEY_CURRENT_USER;
                        break;

                    case "HKEY_LOCAL_MACHINE":
                        zero = HKEY_LOCAL_MACHINE;
                        break;

                    case "HKEY_USERS":
                        zero = HKEY_USERS;
                        break;

                    case "HKEY_PERFORMANCE_DATA":
                        zero = HKEY_PERFORMANCE_DATA;
                        break;

                    case "HKEY_CURRENT_CONFIG":
                        zero = HKEY_CURRENT_CONFIG;
                        break;

                    case "HKEY_DYN_DATA":
                        zero = HKEY_DYN_DATA;
                        break;

                    default:
                        this.Win32Error(errorCode, null);
                        break;
                }
                errorCode = Win32Native.RegOpenKeyEx(zero, null, 0, GetRegistryKeyAccess(this.IsWritable(), this.regView), out handle);
                if ((errorCode == 0) && !handle.IsInvalid)
                {
                    return handle;
                }
                this.Win32Error(errorCode, null);
                throw new IOException(Win32Native.GetMessage(errorCode), errorCode);
            }
        }

        public string Name
        {
            [SecuritySafeCritical]
            get
            {
                this.EnsureNotDisposed();
                return this.keyName;
            }
        }

        public int SubKeyCount
        {
            [SecuritySafeCritical]
            get
            {
                this.CheckPermission(RegistryInternalCheck.CheckKeyReadPermission, null, false, RegistryKeyPermissionCheck.Default);
                return this.InternalSubKeyCount();
            }
        }

        public int ValueCount
        {
            [SecuritySafeCritical]
            get
            {
                this.CheckPermission(RegistryInternalCheck.CheckKeyReadPermission, null, false, RegistryKeyPermissionCheck.Default);
                return this.InternalValueCount();
            }
        }

        [ComVisible(false)]
        public RegistryView View
        {
            [SecuritySafeCritical]
            get
            {
                this.EnsureNotDisposed();
                return this.regView;
            }
        }

        private enum RegistryInternalCheck
        {
            CheckSubKeyWritePermission,
            CheckSubKeyReadPermission,
            CheckSubKeyCreatePermission,
            CheckSubTreeReadPermission,
            CheckSubTreeWritePermission,
            CheckSubTreeReadWritePermission,
            CheckValueWritePermission,
            CheckValueCreatePermission,
            CheckValueReadPermission,
            CheckKeyReadPermission,
            CheckSubTreePermission,
            CheckOpenSubKeyWithWritablePermission,
            CheckOpenSubKeyPermission
        }
    }
}

