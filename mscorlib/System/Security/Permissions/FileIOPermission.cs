namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class FileIOPermission : CodeAccessPermission, IUnrestrictedPermission, IBuiltInPermission
    {
        private FileIOAccess m_append;
        [OptionalField(VersionAdded=2)]
        private FileIOAccess m_changeAcl;
        private static readonly char[] m_illegalCharacters = new char[] { '?', '*' };
        private FileIOAccess m_pathDiscovery;
        private FileIOAccess m_read;
        private bool m_unrestricted;
        [OptionalField(VersionAdded=2)]
        private FileIOAccess m_viewAcl;
        private FileIOAccess m_write;

        public FileIOPermission(PermissionState state)
        {
            if (state == PermissionState.Unrestricted)
            {
                this.m_unrestricted = true;
            }
            else
            {
                if (state != PermissionState.None)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPermissionState"));
                }
                this.m_unrestricted = false;
            }
        }

        [SecuritySafeCritical]
        public FileIOPermission(FileIOPermissionAccess access, string path)
        {
            VerifyAccess(access);
            string[] pathListOrig = new string[] { path };
            this.AddPathList(access, pathListOrig, false, true, false);
        }

        [SecuritySafeCritical]
        public FileIOPermission(FileIOPermissionAccess access, string[] pathList)
        {
            VerifyAccess(access);
            this.AddPathList(access, pathList, false, true, false);
        }

        [SecuritySafeCritical]
        public FileIOPermission(FileIOPermissionAccess access, AccessControlActions control, string path)
        {
            VerifyAccess(access);
            string[] pathListOrig = new string[] { path };
            this.AddPathList(access, control, pathListOrig, false, true, false);
        }

        [SecuritySafeCritical]
        public FileIOPermission(FileIOPermissionAccess access, AccessControlActions control, string[] pathList) : this(access, control, pathList, true, true)
        {
        }

        [SecurityCritical]
        internal FileIOPermission(FileIOPermissionAccess access, string[] pathList, bool checkForDuplicates, bool needFullPath)
        {
            VerifyAccess(access);
            this.AddPathList(access, pathList, checkForDuplicates, needFullPath, true);
        }

        [SecurityCritical]
        internal FileIOPermission(FileIOPermissionAccess access, AccessControlActions control, string[] pathList, bool checkForDuplicates, bool needFullPath)
        {
            VerifyAccess(access);
            this.AddPathList(access, control, pathList, checkForDuplicates, needFullPath, true);
        }

        private static bool AccessIsSet(FileIOPermissionAccess access, FileIOPermissionAccess question)
        {
            return ((access & question) != FileIOPermissionAccess.NoAccess);
        }

        [SecuritySafeCritical]
        public void AddPathList(FileIOPermissionAccess access, string path)
        {
            string[] strArray;
            if (path == null)
            {
                strArray = new string[0];
            }
            else
            {
                strArray = new string[] { path };
            }
            this.AddPathList(access, strArray, false, true, false);
        }

        [SecuritySafeCritical]
        public void AddPathList(FileIOPermissionAccess access, string[] pathList)
        {
            this.AddPathList(access, pathList, true, true, true);
        }

        [SecurityCritical]
        internal void AddPathList(FileIOPermissionAccess access, string[] pathListOrig, bool checkForDuplicates, bool needFullPath, bool copyPathList)
        {
            this.AddPathList(access, AccessControlActions.None, pathListOrig, checkForDuplicates, needFullPath, copyPathList);
        }

        [SecurityCritical]
        internal void AddPathList(FileIOPermissionAccess access, AccessControlActions control, string[] pathListOrig, bool checkForDuplicates, bool needFullPath, bool copyPathList)
        {
            if (pathListOrig == null)
            {
                throw new ArgumentNullException("pathList");
            }
            if (pathListOrig.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            VerifyAccess(access);
            if (!this.m_unrestricted)
            {
                string[] destinationArray = pathListOrig;
                if (copyPathList)
                {
                    destinationArray = new string[pathListOrig.Length];
                    Array.Copy(pathListOrig, destinationArray, pathListOrig.Length);
                }
                HasIllegalCharacters(destinationArray);
                ArrayList values = StringExpressionSet.CreateListFromExpressions(destinationArray, needFullPath);
                if ((access & FileIOPermissionAccess.Read) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_read == null)
                    {
                        this.m_read = new FileIOAccess();
                    }
                    this.m_read.AddExpressions(values, checkForDuplicates);
                }
                if ((access & FileIOPermissionAccess.Write) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_write == null)
                    {
                        this.m_write = new FileIOAccess();
                    }
                    this.m_write.AddExpressions(values, checkForDuplicates);
                }
                if ((access & FileIOPermissionAccess.Append) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_append == null)
                    {
                        this.m_append = new FileIOAccess();
                    }
                    this.m_append.AddExpressions(values, checkForDuplicates);
                }
                if ((access & FileIOPermissionAccess.PathDiscovery) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_pathDiscovery == null)
                    {
                        this.m_pathDiscovery = new FileIOAccess(true);
                    }
                    this.m_pathDiscovery.AddExpressions(values, checkForDuplicates);
                }
                if ((control & AccessControlActions.View) != AccessControlActions.None)
                {
                    if (this.m_viewAcl == null)
                    {
                        this.m_viewAcl = new FileIOAccess();
                    }
                    this.m_viewAcl.AddExpressions(values, checkForDuplicates);
                }
                if ((control & AccessControlActions.Change) != AccessControlActions.None)
                {
                    if (this.m_changeAcl == null)
                    {
                        this.m_changeAcl = new FileIOAccess();
                    }
                    this.m_changeAcl.AddExpressions(values, checkForDuplicates);
                }
            }
        }

        public override IPermission Copy()
        {
            FileIOPermission permission = new FileIOPermission(PermissionState.None);
            if (this.m_unrestricted)
            {
                permission.m_unrestricted = true;
                return permission;
            }
            permission.m_unrestricted = false;
            if (this.m_read != null)
            {
                permission.m_read = this.m_read.Copy();
            }
            if (this.m_write != null)
            {
                permission.m_write = this.m_write.Copy();
            }
            if (this.m_append != null)
            {
                permission.m_append = this.m_append.Copy();
            }
            if (this.m_pathDiscovery != null)
            {
                permission.m_pathDiscovery = this.m_pathDiscovery.Copy();
            }
            if (this.m_viewAcl != null)
            {
                permission.m_viewAcl = this.m_viewAcl.Copy();
            }
            if (this.m_changeAcl != null)
            {
                permission.m_changeAcl = this.m_changeAcl.Copy();
            }
            return permission;
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            FileIOPermission permission = obj as FileIOPermission;
            if (permission == null)
            {
                return false;
            }
            if (!this.m_unrestricted || !permission.m_unrestricted)
            {
                if (this.m_unrestricted != permission.m_unrestricted)
                {
                    return false;
                }
                if (this.m_read == null)
                {
                    if ((permission.m_read != null) && !permission.m_read.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_read.Equals(permission.m_read))
                {
                    return false;
                }
                if (this.m_write == null)
                {
                    if ((permission.m_write != null) && !permission.m_write.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_write.Equals(permission.m_write))
                {
                    return false;
                }
                if (this.m_append == null)
                {
                    if ((permission.m_append != null) && !permission.m_append.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_append.Equals(permission.m_append))
                {
                    return false;
                }
                if (this.m_pathDiscovery == null)
                {
                    if ((permission.m_pathDiscovery != null) && !permission.m_pathDiscovery.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_pathDiscovery.Equals(permission.m_pathDiscovery))
                {
                    return false;
                }
                if (this.m_viewAcl == null)
                {
                    if ((permission.m_viewAcl != null) && !permission.m_viewAcl.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_viewAcl.Equals(permission.m_viewAcl))
                {
                    return false;
                }
                if (this.m_changeAcl == null)
                {
                    if ((permission.m_changeAcl != null) && !permission.m_changeAcl.IsEmpty())
                    {
                        return false;
                    }
                }
                else if (!this.m_changeAcl.Equals(permission.m_changeAcl))
                {
                    return false;
                }
            }
            return true;
        }

        private static void ExclusiveAccess(FileIOPermissionAccess access)
        {
            if (access == FileIOPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
            if ((access & (access - 1)) != FileIOPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumNotSingleFlag"));
            }
        }

        [SecuritySafeCritical]
        public override void FromXml(SecurityElement esd)
        {
            CodeAccessPermission.ValidateElement(esd, this);
            if (XMLUtil.IsUnrestricted(esd))
            {
                this.m_unrestricted = true;
            }
            else
            {
                this.m_unrestricted = false;
                string str = esd.Attribute("Read");
                if (str != null)
                {
                    this.m_read = new FileIOAccess(str);
                }
                else
                {
                    this.m_read = null;
                }
                str = esd.Attribute("Write");
                if (str != null)
                {
                    this.m_write = new FileIOAccess(str);
                }
                else
                {
                    this.m_write = null;
                }
                str = esd.Attribute("Append");
                if (str != null)
                {
                    this.m_append = new FileIOAccess(str);
                }
                else
                {
                    this.m_append = null;
                }
                str = esd.Attribute("PathDiscovery");
                if (str != null)
                {
                    this.m_pathDiscovery = new FileIOAccess(str);
                    this.m_pathDiscovery.PathDiscovery = true;
                }
                else
                {
                    this.m_pathDiscovery = null;
                }
                str = esd.Attribute("ViewAcl");
                if (str != null)
                {
                    this.m_viewAcl = new FileIOAccess(str);
                }
                else
                {
                    this.m_viewAcl = null;
                }
                str = esd.Attribute("ChangeAcl");
                if (str != null)
                {
                    this.m_changeAcl = new FileIOAccess(str);
                }
                else
                {
                    this.m_changeAcl = null;
                }
            }
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string[] GetPathList(FileIOPermissionAccess access)
        {
            VerifyAccess(access);
            ExclusiveAccess(access);
            if (AccessIsSet(access, FileIOPermissionAccess.Read))
            {
                if (this.m_read == null)
                {
                    return null;
                }
                return this.m_read.ToStringArray();
            }
            if (AccessIsSet(access, FileIOPermissionAccess.Write))
            {
                if (this.m_write == null)
                {
                    return null;
                }
                return this.m_write.ToStringArray();
            }
            if (AccessIsSet(access, FileIOPermissionAccess.Append))
            {
                if (this.m_append == null)
                {
                    return null;
                }
                return this.m_append.ToStringArray();
            }
            if (!AccessIsSet(access, FileIOPermissionAccess.PathDiscovery))
            {
                return null;
            }
            if (this.m_pathDiscovery == null)
            {
                return null;
            }
            return this.m_pathDiscovery.ToStringArray();
        }

        internal static int GetTokenIndex()
        {
            return 2;
        }

        private static void HasIllegalCharacters(string[] str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == null)
                {
                    throw new ArgumentNullException("str");
                }
                Path.CheckInvalidPathChars(str[i]);
                if (str[i].IndexOfAny(m_illegalCharacters) != -1)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidPathChars"));
                }
            }
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            FileIOPermission permission = target as FileIOPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.IsUnrestricted())
            {
                return target.Copy();
            }
            if (permission.IsUnrestricted())
            {
                return this.Copy();
            }
            FileIOAccess access = (this.m_read == null) ? null : this.m_read.Intersect(permission.m_read);
            FileIOAccess access2 = (this.m_write == null) ? null : this.m_write.Intersect(permission.m_write);
            FileIOAccess access3 = (this.m_append == null) ? null : this.m_append.Intersect(permission.m_append);
            FileIOAccess access4 = (this.m_pathDiscovery == null) ? null : this.m_pathDiscovery.Intersect(permission.m_pathDiscovery);
            FileIOAccess access5 = (this.m_viewAcl == null) ? null : this.m_viewAcl.Intersect(permission.m_viewAcl);
            FileIOAccess access6 = (this.m_changeAcl == null) ? null : this.m_changeAcl.Intersect(permission.m_changeAcl);
            if (((((access == null) || access.IsEmpty()) && ((access2 == null) || access2.IsEmpty())) && (((access3 == null) || access3.IsEmpty()) && ((access4 == null) || access4.IsEmpty()))) && (((access5 == null) || access5.IsEmpty()) && ((access6 == null) || access6.IsEmpty())))
            {
                return null;
            }
            return new FileIOPermission(PermissionState.None) { m_unrestricted = false, m_read = access, m_write = access2, m_append = access3, m_pathDiscovery = access4, m_viewAcl = access5, m_changeAcl = access6 };
        }

        private bool IsEmpty()
        {
            if ((((!this.m_unrestricted && ((this.m_read == null) || this.m_read.IsEmpty())) && ((this.m_write == null) || this.m_write.IsEmpty())) && (((this.m_append == null) || this.m_append.IsEmpty()) && ((this.m_pathDiscovery == null) || this.m_pathDiscovery.IsEmpty()))) && ((this.m_viewAcl == null) || this.m_viewAcl.IsEmpty()))
            {
                if (this.m_changeAcl != null)
                {
                    return this.m_changeAcl.IsEmpty();
                }
                return true;
            }
            return false;
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return this.IsEmpty();
            }
            FileIOPermission permission = target as FileIOPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (permission.IsUnrestricted())
            {
                return true;
            }
            if (!this.IsUnrestricted() && (((((this.m_read == null) || this.m_read.IsSubsetOf(permission.m_read)) && ((this.m_write == null) || this.m_write.IsSubsetOf(permission.m_write))) && (((this.m_append == null) || this.m_append.IsSubsetOf(permission.m_append)) && ((this.m_pathDiscovery == null) || this.m_pathDiscovery.IsSubsetOf(permission.m_pathDiscovery)))) && ((this.m_viewAcl == null) || this.m_viewAcl.IsSubsetOf(permission.m_viewAcl))))
            {
                if (this.m_changeAcl != null)
                {
                    return this.m_changeAcl.IsSubsetOf(permission.m_changeAcl);
                }
                return true;
            }
            return false;
        }

        public bool IsUnrestricted()
        {
            return this.m_unrestricted;
        }

        public void SetPathList(FileIOPermissionAccess access, string path)
        {
            string[] strArray;
            if (path == null)
            {
                strArray = new string[0];
            }
            else
            {
                strArray = new string[] { path };
            }
            this.SetPathList(access, strArray, false);
        }

        public void SetPathList(FileIOPermissionAccess access, string[] pathList)
        {
            this.SetPathList(access, pathList, true);
        }

        internal void SetPathList(FileIOPermissionAccess access, string[] pathList, bool checkForDuplicates)
        {
            this.SetPathList(access, AccessControlActions.None, pathList, checkForDuplicates);
        }

        [SecuritySafeCritical]
        internal void SetPathList(FileIOPermissionAccess access, AccessControlActions control, string[] pathList, bool checkForDuplicates)
        {
            VerifyAccess(access);
            if ((access & FileIOPermissionAccess.Read) != FileIOPermissionAccess.NoAccess)
            {
                this.m_read = null;
            }
            if ((access & FileIOPermissionAccess.Write) != FileIOPermissionAccess.NoAccess)
            {
                this.m_write = null;
            }
            if ((access & FileIOPermissionAccess.Append) != FileIOPermissionAccess.NoAccess)
            {
                this.m_append = null;
            }
            if ((access & FileIOPermissionAccess.PathDiscovery) != FileIOPermissionAccess.NoAccess)
            {
                this.m_pathDiscovery = null;
            }
            if ((control & AccessControlActions.View) != AccessControlActions.None)
            {
                this.m_viewAcl = null;
            }
            if ((control & AccessControlActions.Change) != AccessControlActions.None)
            {
                this.m_changeAcl = null;
            }
            this.m_unrestricted = false;
            this.AddPathList(access, control, pathList, checkForDuplicates, true, true);
        }

        int IBuiltInPermission.GetTokenIndex()
        {
            return GetTokenIndex();
        }

        public override SecurityElement ToXml()
        {
            SecurityElement element = CodeAccessPermission.CreatePermissionElement(this, "System.Security.Permissions.FileIOPermission");
            if (!this.IsUnrestricted())
            {
                if ((this.m_read != null) && !this.m_read.IsEmpty())
                {
                    element.AddAttribute("Read", SecurityElement.Escape(this.m_read.ToString()));
                }
                if ((this.m_write != null) && !this.m_write.IsEmpty())
                {
                    element.AddAttribute("Write", SecurityElement.Escape(this.m_write.ToString()));
                }
                if ((this.m_append != null) && !this.m_append.IsEmpty())
                {
                    element.AddAttribute("Append", SecurityElement.Escape(this.m_append.ToString()));
                }
                if ((this.m_pathDiscovery != null) && !this.m_pathDiscovery.IsEmpty())
                {
                    element.AddAttribute("PathDiscovery", SecurityElement.Escape(this.m_pathDiscovery.ToString()));
                }
                if ((this.m_viewAcl != null) && !this.m_viewAcl.IsEmpty())
                {
                    element.AddAttribute("ViewAcl", SecurityElement.Escape(this.m_viewAcl.ToString()));
                }
                if ((this.m_changeAcl != null) && !this.m_changeAcl.IsEmpty())
                {
                    element.AddAttribute("ChangeAcl", SecurityElement.Escape(this.m_changeAcl.ToString()));
                }
                return element;
            }
            element.AddAttribute("Unrestricted", "true");
            return element;
        }

        public override IPermission Union(IPermission other)
        {
            if (other == null)
            {
                return this.Copy();
            }
            FileIOPermission permission = other as FileIOPermission;
            if (permission == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_WrongType", new object[] { base.GetType().FullName }));
            }
            if (this.IsUnrestricted() || permission.IsUnrestricted())
            {
                return new FileIOPermission(PermissionState.Unrestricted);
            }
            FileIOAccess access = (this.m_read == null) ? permission.m_read : this.m_read.Union(permission.m_read);
            FileIOAccess access2 = (this.m_write == null) ? permission.m_write : this.m_write.Union(permission.m_write);
            FileIOAccess access3 = (this.m_append == null) ? permission.m_append : this.m_append.Union(permission.m_append);
            FileIOAccess access4 = (this.m_pathDiscovery == null) ? permission.m_pathDiscovery : this.m_pathDiscovery.Union(permission.m_pathDiscovery);
            FileIOAccess access5 = (this.m_viewAcl == null) ? permission.m_viewAcl : this.m_viewAcl.Union(permission.m_viewAcl);
            FileIOAccess access6 = (this.m_changeAcl == null) ? permission.m_changeAcl : this.m_changeAcl.Union(permission.m_changeAcl);
            if (((((access == null) || access.IsEmpty()) && ((access2 == null) || access2.IsEmpty())) && (((access3 == null) || access3.IsEmpty()) && ((access4 == null) || access4.IsEmpty()))) && (((access5 == null) || access5.IsEmpty()) && ((access6 == null) || access6.IsEmpty())))
            {
                return null;
            }
            return new FileIOPermission(PermissionState.None) { m_unrestricted = false, m_read = access, m_write = access2, m_append = access3, m_pathDiscovery = access4, m_viewAcl = access5, m_changeAcl = access6 };
        }

        private static void VerifyAccess(FileIOPermissionAccess access)
        {
            if ((access & ~FileIOPermissionAccess.AllAccess) != FileIOPermissionAccess.NoAccess)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) access }));
            }
        }

        public FileIOPermissionAccess AllFiles
        {
            get
            {
                if (this.m_unrestricted)
                {
                    return FileIOPermissionAccess.AllAccess;
                }
                FileIOPermissionAccess noAccess = FileIOPermissionAccess.NoAccess;
                if ((this.m_read != null) && this.m_read.AllFiles)
                {
                    noAccess |= FileIOPermissionAccess.Read;
                }
                if ((this.m_write != null) && this.m_write.AllFiles)
                {
                    noAccess |= FileIOPermissionAccess.Write;
                }
                if ((this.m_append != null) && this.m_append.AllFiles)
                {
                    noAccess |= FileIOPermissionAccess.Append;
                }
                if ((this.m_pathDiscovery != null) && this.m_pathDiscovery.AllFiles)
                {
                    noAccess |= FileIOPermissionAccess.PathDiscovery;
                }
                return noAccess;
            }
            set
            {
                if (value == FileIOPermissionAccess.AllAccess)
                {
                    this.m_unrestricted = true;
                }
                else
                {
                    if ((value & FileIOPermissionAccess.Read) != FileIOPermissionAccess.NoAccess)
                    {
                        if (this.m_read == null)
                        {
                            this.m_read = new FileIOAccess();
                        }
                        this.m_read.AllFiles = true;
                    }
                    else if (this.m_read != null)
                    {
                        this.m_read.AllFiles = false;
                    }
                    if ((value & FileIOPermissionAccess.Write) != FileIOPermissionAccess.NoAccess)
                    {
                        if (this.m_write == null)
                        {
                            this.m_write = new FileIOAccess();
                        }
                        this.m_write.AllFiles = true;
                    }
                    else if (this.m_write != null)
                    {
                        this.m_write.AllFiles = false;
                    }
                    if ((value & FileIOPermissionAccess.Append) != FileIOPermissionAccess.NoAccess)
                    {
                        if (this.m_append == null)
                        {
                            this.m_append = new FileIOAccess();
                        }
                        this.m_append.AllFiles = true;
                    }
                    else if (this.m_append != null)
                    {
                        this.m_append.AllFiles = false;
                    }
                    if ((value & FileIOPermissionAccess.PathDiscovery) != FileIOPermissionAccess.NoAccess)
                    {
                        if (this.m_pathDiscovery == null)
                        {
                            this.m_pathDiscovery = new FileIOAccess(true);
                        }
                        this.m_pathDiscovery.AllFiles = true;
                    }
                    else if (this.m_pathDiscovery != null)
                    {
                        this.m_pathDiscovery.AllFiles = false;
                    }
                }
            }
        }

        public FileIOPermissionAccess AllLocalFiles
        {
            get
            {
                if (this.m_unrestricted)
                {
                    return FileIOPermissionAccess.AllAccess;
                }
                FileIOPermissionAccess noAccess = FileIOPermissionAccess.NoAccess;
                if ((this.m_read != null) && this.m_read.AllLocalFiles)
                {
                    noAccess |= FileIOPermissionAccess.Read;
                }
                if ((this.m_write != null) && this.m_write.AllLocalFiles)
                {
                    noAccess |= FileIOPermissionAccess.Write;
                }
                if ((this.m_append != null) && this.m_append.AllLocalFiles)
                {
                    noAccess |= FileIOPermissionAccess.Append;
                }
                if ((this.m_pathDiscovery != null) && this.m_pathDiscovery.AllLocalFiles)
                {
                    noAccess |= FileIOPermissionAccess.PathDiscovery;
                }
                return noAccess;
            }
            set
            {
                if ((value & FileIOPermissionAccess.Read) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_read == null)
                    {
                        this.m_read = new FileIOAccess();
                    }
                    this.m_read.AllLocalFiles = true;
                }
                else if (this.m_read != null)
                {
                    this.m_read.AllLocalFiles = false;
                }
                if ((value & FileIOPermissionAccess.Write) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_write == null)
                    {
                        this.m_write = new FileIOAccess();
                    }
                    this.m_write.AllLocalFiles = true;
                }
                else if (this.m_write != null)
                {
                    this.m_write.AllLocalFiles = false;
                }
                if ((value & FileIOPermissionAccess.Append) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_append == null)
                    {
                        this.m_append = new FileIOAccess();
                    }
                    this.m_append.AllLocalFiles = true;
                }
                else if (this.m_append != null)
                {
                    this.m_append.AllLocalFiles = false;
                }
                if ((value & FileIOPermissionAccess.PathDiscovery) != FileIOPermissionAccess.NoAccess)
                {
                    if (this.m_pathDiscovery == null)
                    {
                        this.m_pathDiscovery = new FileIOAccess(true);
                    }
                    this.m_pathDiscovery.AllLocalFiles = true;
                }
                else if (this.m_pathDiscovery != null)
                {
                    this.m_pathDiscovery.AllLocalFiles = false;
                }
            }
        }
    }
}

