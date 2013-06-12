namespace System.Security.Permissions
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Util;

    [Serializable]
    internal sealed class FileIOAccess
    {
        private bool m_allFiles;
        private bool m_allLocalFiles;
        private bool m_ignoreCase;
        private bool m_pathDiscovery;
        private StringExpressionSet m_set;
        private const string m_strAllFiles = "*AllFiles*";
        private const string m_strAllLocalFiles = "*AllLocalFiles*";

        [SecuritySafeCritical]
        public FileIOAccess()
        {
            this.m_ignoreCase = true;
            this.m_set = new StringExpressionSet(this.m_ignoreCase, true);
            this.m_allFiles = false;
            this.m_allLocalFiles = false;
            this.m_pathDiscovery = false;
        }

        [SecuritySafeCritical]
        public FileIOAccess(bool pathDiscovery)
        {
            this.m_ignoreCase = true;
            this.m_set = new StringExpressionSet(this.m_ignoreCase, true);
            this.m_allFiles = false;
            this.m_allLocalFiles = false;
            this.m_pathDiscovery = pathDiscovery;
        }

        private FileIOAccess(FileIOAccess operand)
        {
            this.m_ignoreCase = true;
            this.m_set = operand.m_set.Copy();
            this.m_allFiles = operand.m_allFiles;
            this.m_allLocalFiles = operand.m_allLocalFiles;
            this.m_pathDiscovery = operand.m_pathDiscovery;
        }

        [SecurityCritical]
        public FileIOAccess(string value)
        {
            this.m_ignoreCase = true;
            if (value == null)
            {
                this.m_set = new StringExpressionSet(this.m_ignoreCase, true);
                this.m_allFiles = false;
                this.m_allLocalFiles = false;
            }
            else if ((value.Length >= "*AllFiles*".Length) && (string.Compare("*AllFiles*", value, StringComparison.Ordinal) == 0))
            {
                this.m_set = new StringExpressionSet(this.m_ignoreCase, true);
                this.m_allFiles = true;
                this.m_allLocalFiles = false;
            }
            else if ((value.Length >= "*AllLocalFiles*".Length) && (string.Compare("*AllLocalFiles*", 0, value, 0, "*AllLocalFiles*".Length, StringComparison.Ordinal) == 0))
            {
                this.m_set = new StringExpressionSet(this.m_ignoreCase, value.Substring("*AllLocalFiles*".Length), true);
                this.m_allFiles = false;
                this.m_allLocalFiles = true;
            }
            else
            {
                this.m_set = new StringExpressionSet(this.m_ignoreCase, value, true);
                this.m_allFiles = false;
                this.m_allLocalFiles = false;
            }
            this.m_pathDiscovery = false;
        }

        [SecuritySafeCritical]
        public FileIOAccess(bool allFiles, bool allLocalFiles, bool pathDiscovery)
        {
            this.m_ignoreCase = true;
            this.m_set = new StringExpressionSet(this.m_ignoreCase, true);
            this.m_allFiles = allFiles;
            this.m_allLocalFiles = allLocalFiles;
            this.m_pathDiscovery = pathDiscovery;
        }

        public FileIOAccess(StringExpressionSet set, bool allFiles, bool allLocalFiles, bool pathDiscovery)
        {
            this.m_ignoreCase = true;
            this.m_set = set;
            this.m_set.SetThrowOnRelative(true);
            this.m_allFiles = allFiles;
            this.m_allLocalFiles = allLocalFiles;
            this.m_pathDiscovery = pathDiscovery;
        }

        [SecurityCritical]
        public void AddExpressions(ArrayList values, bool checkForDuplicates)
        {
            this.m_allFiles = false;
            this.m_set.AddExpressions(values, checkForDuplicates);
        }

        public FileIOAccess Copy()
        {
            return new FileIOAccess(this);
        }

        [SecuritySafeCritical]
        public override bool Equals(object obj)
        {
            FileIOAccess operand = obj as FileIOAccess;
            if (operand == null)
            {
                return (this.IsEmpty() && (obj == null));
            }
            if (this.m_pathDiscovery)
            {
                return ((this.m_allFiles && operand.m_allFiles) || (((this.m_allLocalFiles == operand.m_allLocalFiles) && this.m_set.IsSubsetOf(operand.m_set)) && operand.m_set.IsSubsetOf(this.m_set)));
            }
            if (!this.IsSubsetOf(operand))
            {
                return false;
            }
            if (!operand.IsSubsetOf(this))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static string GetRoot(string path)
        {
            string str = path.Substring(0, 3);
            if (str.EndsWith(@":\", StringComparison.Ordinal))
            {
                return str;
            }
            return null;
        }

        [SecuritySafeCritical]
        public FileIOAccess Intersect(FileIOAccess operand)
        {
            if (operand == null)
            {
                return null;
            }
            if (this.m_allFiles)
            {
                if (operand.m_allFiles)
                {
                    return new FileIOAccess(true, false, this.m_pathDiscovery);
                }
                return new FileIOAccess(operand.m_set.Copy(), false, operand.m_allLocalFiles, this.m_pathDiscovery);
            }
            if (operand.m_allFiles)
            {
                return new FileIOAccess(this.m_set.Copy(), false, this.m_allLocalFiles, this.m_pathDiscovery);
            }
            StringExpressionSet set = new StringExpressionSet(this.m_ignoreCase, true);
            if (this.m_allLocalFiles)
            {
                string[] strArray = operand.m_set.ToStringArray();
                if (strArray != null)
                {
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        string root = GetRoot(strArray[i]);
                        if ((root != null) && IsLocalDrive(GetRoot(root)))
                        {
                            set.AddExpressions(new string[] { strArray[i] }, true, false);
                        }
                    }
                }
            }
            if (operand.m_allLocalFiles)
            {
                string[] strArray2 = this.m_set.ToStringArray();
                if (strArray2 != null)
                {
                    for (int j = 0; j < strArray2.Length; j++)
                    {
                        string path = GetRoot(strArray2[j]);
                        if ((path != null) && IsLocalDrive(GetRoot(path)))
                        {
                            set.AddExpressions(new string[] { strArray2[j] }, true, false);
                        }
                    }
                }
            }
            string[] strArray3 = this.m_set.Intersect(operand.m_set).ToStringArray();
            if (strArray3 != null)
            {
                set.AddExpressions(strArray3, !set.IsEmpty(), false);
            }
            return new FileIOAccess(set, false, this.m_allLocalFiles && operand.m_allLocalFiles, this.m_pathDiscovery);
        }

        public bool IsEmpty()
        {
            if (this.m_allFiles || this.m_allLocalFiles)
            {
                return false;
            }
            if (this.m_set != null)
            {
                return this.m_set.IsEmpty();
            }
            return true;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool IsLocalDrive(string path);
        [SecuritySafeCritical]
        public bool IsSubsetOf(FileIOAccess operand)
        {
            if (operand == null)
            {
                return this.IsEmpty();
            }
            if (!operand.m_allFiles && ((!this.m_pathDiscovery || !this.m_set.IsSubsetOfPathDiscovery(operand.m_set)) && !this.m_set.IsSubsetOf(operand.m_set)))
            {
                if (!operand.m_allLocalFiles)
                {
                    return false;
                }
                string[] strArray = this.m_set.ToStringArray();
                for (int i = 0; i < strArray.Length; i++)
                {
                    string root = GetRoot(strArray[i]);
                    if ((root == null) || !IsLocalDrive(GetRoot(root)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            if (this.m_allFiles)
            {
                return "*AllFiles*";
            }
            if (!this.m_allLocalFiles)
            {
                return this.m_set.ToString();
            }
            string str = "*AllLocalFiles*";
            string str2 = this.m_set.ToString();
            if ((str2 != null) && (str2.Length > 0))
            {
                str = str + ";" + str2;
            }
            return str;
        }

        public string[] ToStringArray()
        {
            return this.m_set.ToStringArray();
        }

        [SecuritySafeCritical]
        public FileIOAccess Union(FileIOAccess operand)
        {
            if (operand == null)
            {
                if (!this.IsEmpty())
                {
                    return this.Copy();
                }
                return null;
            }
            if (this.m_allFiles || operand.m_allFiles)
            {
                return new FileIOAccess(true, false, this.m_pathDiscovery);
            }
            return new FileIOAccess(this.m_set.Union(operand.m_set), false, this.m_allLocalFiles || operand.m_allLocalFiles, this.m_pathDiscovery);
        }

        public bool AllFiles
        {
            get
            {
                return this.m_allFiles;
            }
            set
            {
                this.m_allFiles = value;
            }
        }

        public bool AllLocalFiles
        {
            get
            {
                return this.m_allLocalFiles;
            }
            set
            {
                this.m_allLocalFiles = value;
            }
        }

        public bool PathDiscovery
        {
            set
            {
                this.m_pathDiscovery = value;
            }
        }
    }
}

