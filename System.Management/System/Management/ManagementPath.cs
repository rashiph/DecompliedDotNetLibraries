namespace System.Management
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [TypeConverter(typeof(ManagementPathConverter))]
    public class ManagementPath : ICloneable
    {
        private static ManagementPath defaultPath = new ManagementPath("//./root/cimv2");
        private bool isWbemPathShared;
        private IWbemPath wmiPath;

        internal event IdentifierChangedEventHandler IdentifierChanged;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ManagementPath() : this(null)
        {
        }

        public ManagementPath(string path)
        {
            if ((path != null) && (0 < path.Length))
            {
                this.wmiPath = this.CreateWbemPath(path);
            }
        }

        internal static ManagementPath _Clone(ManagementPath path)
        {
            return _Clone(path, null);
        }

        internal static ManagementPath _Clone(ManagementPath path, IdentifierChangedEventHandler handler)
        {
            ManagementPath path2 = new ManagementPath();
            if (handler != null)
            {
                path2.IdentifierChanged = handler;
            }
            if ((path != null) && (path.wmiPath != null))
            {
                path2.wmiPath = path.wmiPath;
                path2.isWbemPathShared = path.isWbemPathShared = true;
            }
            return path2;
        }

        private void ClearKeys(bool setAsSingleton)
        {
            int errorCode = 0;
            try
            {
                if (this.wmiPath != null)
                {
                    IWbemPathKeyList pOut = null;
                    errorCode = this.wmiPath.GetKeyList_(out pOut);
                    if (pOut != null)
                    {
                        errorCode = pOut.RemoveAllKeys_(0);
                        if ((errorCode & 0x80000000L) == 0L)
                        {
                            sbyte bSet = setAsSingleton ? ((sbyte) (-1)) : ((sbyte) 0);
                            errorCode = pOut.MakeSingleton_(bSet);
                            this.FireIdentifierChanged();
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                ManagementException.ThrowWithExtendedInfo(exception);
            }
            if ((errorCode & 0xfffff000L) == 0x80041000L)
            {
                ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
            }
            else if ((errorCode & 0x80000000L) != 0L)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public ManagementPath Clone()
        {
            return new ManagementPath(this.Path);
        }

        private IWbemPath CreateWbemPath(string path)
        {
            IWbemPath wbemPath = (IWbemPath) MTAHelper.CreateInMTA(typeof(WbemDefPath));
            SetWbemPath(wbemPath, path);
            return wbemPath;
        }

        private void FireIdentifierChanged()
        {
            if (this.IdentifierChanged != null)
            {
                this.IdentifierChanged(this, null);
            }
        }

        internal static string GetManagementPath(IWbemClassObjectFreeThreaded wbemObject)
        {
            string str = null;
            int errorCode = -2147217407;
            if (wbemObject == null)
            {
                return str;
            }
            int pType = 0;
            int plFlavor = 0;
            object pVal = null;
            if ((wbemObject.Get_("__PATH", 0, ref pVal, ref pType, ref plFlavor) < 0) || (pVal == DBNull.Value))
            {
                errorCode = wbemObject.Get_("__RELPATH", 0, ref pVal, ref pType, ref plFlavor);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            if (DBNull.Value == pVal)
            {
                return null;
            }
            return (string) pVal;
        }

        internal string GetNamespacePath(int flags)
        {
            return GetNamespacePath(this.wmiPath, flags);
        }

        internal static string GetNamespacePath(IWbemPath wbemPath, int flags)
        {
            string pszText = string.Empty;
            if (wbemPath != null)
            {
                uint puCount = 0;
                int errorCode = 0;
                errorCode = wbemPath.GetNamespaceCount_(out puCount);
                if ((errorCode >= 0) && (puCount > 0))
                {
                    uint puBuffLength = 0;
                    errorCode = wbemPath.GetText_(flags, ref puBuffLength, null);
                    if ((errorCode >= 0) && (puBuffLength > 0))
                    {
                        pszText = new string('0', ((int) puBuffLength) - 1);
                        errorCode = wbemPath.GetText_(flags, ref puBuffLength, pszText);
                    }
                }
                if ((errorCode >= 0) || (errorCode == -2147217400))
                {
                    return pszText;
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return pszText;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return pszText;
        }

        private string GetWbemPath()
        {
            return GetWbemPath(this.wmiPath);
        }

        private static string GetWbemPath(IWbemPath wbemPath)
        {
            string pszText = string.Empty;
            if (wbemPath != null)
            {
                int lFlags = 4;
                uint puCount = 0;
                int errorCode = 0;
                errorCode = wbemPath.GetNamespaceCount_(out puCount);
                if (errorCode >= 0)
                {
                    if (puCount == 0)
                    {
                        lFlags = 2;
                    }
                    uint puBuffLength = 0;
                    errorCode = wbemPath.GetText_(lFlags, ref puBuffLength, null);
                    if ((errorCode >= 0) && (0 < puBuffLength))
                    {
                        pszText = new string('0', ((int) puBuffLength) - 1);
                        errorCode = wbemPath.GetText_(lFlags, ref puBuffLength, pszText);
                    }
                }
                if ((errorCode >= 0) || (errorCode == -2147217400))
                {
                    return pszText;
                }
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return pszText;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return pszText;
        }

        internal static bool IsValidNamespaceSyntax(string nsPath)
        {
            if (nsPath.Length != 0)
            {
                char[] anyOf = new char[] { '\\', '/' };
                if ((nsPath.IndexOfAny(anyOf) == -1) && (string.Compare("root", nsPath, StringComparison.OrdinalIgnoreCase) != 0))
                {
                    return false;
                }
            }
            return true;
        }

        public void SetAsClass()
        {
            if (!this.IsClass && !this.IsInstance)
            {
                throw new ManagementException(ManagementStatus.InvalidOperation, null, null);
            }
            if (this.isWbemPathShared)
            {
                this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                this.isWbemPathShared = false;
            }
            this.ClearKeys(false);
        }

        public void SetAsSingleton()
        {
            if (!this.IsClass && !this.IsInstance)
            {
                throw new ManagementException(ManagementStatus.InvalidOperation, null, null);
            }
            if (this.isWbemPathShared)
            {
                this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                this.isWbemPathShared = false;
            }
            this.ClearKeys(true);
        }

        internal string SetNamespacePath(string nsPath, out bool bChange)
        {
            int errorCode = 0;
            string strA = null;
            string strB = null;
            IWbemPath wbemPath = null;
            bChange = false;
            if (!IsValidNamespaceSyntax(nsPath))
            {
                ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
            }
            wbemPath = this.CreateWbemPath(nsPath);
            if (this.wmiPath == null)
            {
                this.wmiPath = this.CreateWbemPath("");
            }
            else if (this.isWbemPathShared)
            {
                this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                this.isWbemPathShared = false;
            }
            strA = GetNamespacePath(this.wmiPath, 0x10);
            strB = GetNamespacePath(wbemPath, 0x10);
            if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) != 0)
            {
                this.wmiPath.RemoveAllNamespaces_();
                bChange = true;
                uint puCount = 0;
                errorCode = wbemPath.GetNamespaceCount_(out puCount);
                if (errorCode >= 0)
                {
                    for (uint i = 0; i < puCount; i++)
                    {
                        uint puNameBufLength = 0;
                        errorCode = wbemPath.GetNamespaceAt_(i, ref puNameBufLength, null);
                        if (errorCode < 0)
                        {
                            break;
                        }
                        string pName = new string('0', ((int) puNameBufLength) - 1);
                        errorCode = wbemPath.GetNamespaceAt_(i, ref puNameBufLength, pName);
                        if (errorCode < 0)
                        {
                            break;
                        }
                        errorCode = this.wmiPath.SetNamespaceAt_(i, pName);
                        if (errorCode < 0)
                        {
                            break;
                        }
                    }
                }
            }
            if (((errorCode >= 0) && (nsPath.Length > 1)) && (((nsPath[0] == '\\') && (nsPath[1] == '\\')) || ((nsPath[0] == '/') && (nsPath[1] == '/'))))
            {
                uint num5 = 0;
                errorCode = wbemPath.GetServer_(ref num5, null);
                if ((errorCode >= 0) && (num5 > 0))
                {
                    string str4 = new string('0', ((int) num5) - 1);
                    errorCode = wbemPath.GetServer_(ref num5, str4);
                    if (errorCode >= 0)
                    {
                        num5 = 0;
                        errorCode = this.wmiPath.GetServer_(ref num5, null);
                        if (errorCode >= 0)
                        {
                            string str5 = new string('0', ((int) num5) - 1);
                            errorCode = this.wmiPath.GetServer_(ref num5, str5);
                            if ((errorCode >= 0) && (string.Compare(str5, str4, StringComparison.OrdinalIgnoreCase) != 0))
                            {
                                errorCode = this.wmiPath.SetServer_(str4);
                            }
                        }
                        else if (errorCode == -2147217399)
                        {
                            errorCode = this.wmiPath.SetServer_(str4);
                            if (errorCode >= 0)
                            {
                                bChange = true;
                            }
                        }
                    }
                }
                else if (errorCode == -2147217399)
                {
                    errorCode = 0;
                }
            }
            if (errorCode < 0)
            {
                if ((errorCode & 0xfffff000L) == 0x80041000L)
                {
                    ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    return strB;
                }
                Marshal.ThrowExceptionForHR(errorCode);
            }
            return strB;
        }

        internal void SetRelativePath(string relPath)
        {
            ManagementPath path = new ManagementPath(relPath) {
                NamespacePath = this.GetNamespacePath(8),
                Server = this.Server
            };
            this.wmiPath = path.wmiPath;
        }

        private void SetWbemPath(string path)
        {
            if (this.wmiPath == null)
            {
                this.wmiPath = this.CreateWbemPath(path);
            }
            else
            {
                SetWbemPath(this.wmiPath, path);
            }
        }

        private static void SetWbemPath(IWbemPath wbemPath, string path)
        {
            if (wbemPath != null)
            {
                uint uMode = 4;
                if (string.Compare(path, "root", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    uMode |= 8;
                }
                int errorCode = wbemPath.SetText_(uMode, path);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.Path;
        }

        internal void UpdateRelativePath(string relPath)
        {
            if (relPath != null)
            {
                string path = string.Empty;
                string namespacePath = this.GetNamespacePath(8);
                if (namespacePath.Length > 0)
                {
                    path = namespacePath + ":" + relPath;
                }
                else
                {
                    path = relPath;
                }
                if (this.isWbemPathShared)
                {
                    this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                    this.isWbemPathShared = false;
                }
                this.SetWbemPath(path);
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public string ClassName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.internalClassName;
            }
            set
            {
                if (string.Compare(this.ClassName, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.internalClassName = value;
                    this.FireIdentifierChanged();
                }
            }
        }

        public static ManagementPath DefaultPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return defaultPath;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                defaultPath = value;
            }
        }

        internal string internalClassName
        {
            get
            {
                string pszName = string.Empty;
                if (this.wmiPath != null)
                {
                    uint puBuffLength = 0;
                    if ((this.wmiPath.GetClassName_(ref puBuffLength, null) >= 0) && (0 < puBuffLength))
                    {
                        pszName = new string('0', ((int) puBuffLength) - 1);
                        if (this.wmiPath.GetClassName_(ref puBuffLength, pszName) < 0)
                        {
                            pszName = string.Empty;
                        }
                    }
                }
                return pszName;
            }
            set
            {
                int errorCode = 0;
                if (this.wmiPath == null)
                {
                    this.wmiPath = (IWbemPath) MTAHelper.CreateInMTA(typeof(WbemDefPath));
                }
                else if (this.isWbemPathShared)
                {
                    this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                    this.isWbemPathShared = false;
                }
                try
                {
                    errorCode = this.wmiPath.SetClassName_(value);
                }
                catch (COMException)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
        }

        public bool IsClass
        {
            get
            {
                if (this.wmiPath == null)
                {
                    return false;
                }
                ulong puResponse = 0L;
                int errorCode = this.wmiPath.GetInfo_(0, out puResponse);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                return (0L != (puResponse & ((ulong) 4L)));
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (this.Path.Length == 0);
            }
        }

        public bool IsInstance
        {
            get
            {
                if (this.wmiPath == null)
                {
                    return false;
                }
                ulong puResponse = 0L;
                int errorCode = this.wmiPath.GetInfo_(0, out puResponse);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                return (0L != (puResponse & ((ulong) 8L)));
            }
        }

        public bool IsSingleton
        {
            get
            {
                if (this.wmiPath == null)
                {
                    return false;
                }
                ulong puResponse = 0L;
                int errorCode = this.wmiPath.GetInfo_(0, out puResponse);
                if (errorCode < 0)
                {
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                return (0L != (puResponse & ((ulong) 0x1000L)));
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public string NamespacePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetNamespacePath(0x10);
            }
            set
            {
                bool bChange = false;
                try
                {
                    this.SetNamespacePath(value, out bChange);
                }
                catch (COMException)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (bChange)
                {
                    this.FireIdentifierChanged();
                }
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetWbemPath();
            }
            set
            {
                try
                {
                    if (this.isWbemPathShared)
                    {
                        this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                        this.isWbemPathShared = false;
                    }
                    this.SetWbemPath(value);
                }
                catch
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.FireIdentifierChanged();
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public string RelativePath
        {
            get
            {
                string pszText = string.Empty;
                if (this.wmiPath != null)
                {
                    uint puBuffLength = 0;
                    int errorCode = this.wmiPath.GetText_(2, ref puBuffLength, null);
                    if ((errorCode >= 0) && (0 < puBuffLength))
                    {
                        pszText = new string('0', ((int) puBuffLength) - 1);
                        errorCode = this.wmiPath.GetText_(2, ref puBuffLength, pszText);
                    }
                    if ((errorCode >= 0) || (errorCode == -2147217400))
                    {
                        return pszText;
                    }
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        return pszText;
                    }
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return pszText;
            }
            set
            {
                try
                {
                    this.SetRelativePath(value);
                }
                catch (COMException)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.FireIdentifierChanged();
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public string Server
        {
            get
            {
                string pName = string.Empty;
                if (this.wmiPath != null)
                {
                    uint puNameBufLength = 0;
                    int errorCode = this.wmiPath.GetServer_(ref puNameBufLength, null);
                    if ((errorCode >= 0) && (0 < puNameBufLength))
                    {
                        pName = new string('0', ((int) puNameBufLength) - 1);
                        errorCode = this.wmiPath.GetServer_(ref puNameBufLength, pName);
                    }
                    if ((errorCode >= 0) || (errorCode == -2147217399))
                    {
                        return pName;
                    }
                    if ((errorCode & 0xfffff000L) == 0x80041000L)
                    {
                        ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        return pName;
                    }
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return pName;
            }
            set
            {
                if (string.Compare(this.Server, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (this.wmiPath == null)
                    {
                        this.wmiPath = (IWbemPath) MTAHelper.CreateInMTA(typeof(WbemDefPath));
                    }
                    else if (this.isWbemPathShared)
                    {
                        this.wmiPath = this.CreateWbemPath(this.GetWbemPath());
                        this.isWbemPathShared = false;
                    }
                    int errorCode = this.wmiPath.SetServer_(value);
                    if (errorCode < 0)
                    {
                        if ((errorCode & 0xfffff000L) == 0x80041000L)
                        {
                            ManagementException.ThrowWithExtendedInfo((ManagementStatus) errorCode);
                        }
                        else
                        {
                            Marshal.ThrowExceptionForHR(errorCode);
                        }
                    }
                    this.FireIdentifierChanged();
                }
            }
        }
    }
}

