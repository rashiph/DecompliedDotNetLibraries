namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;

    [Serializable]
    internal sealed class AssemblyNameExtension
    {
        private System.Reflection.AssemblyName asAssemblyName;
        private string asString;
        private bool hasProcessorArchitectureInFusionName;
        private bool isSimpleName;
        private static AssemblyNameExtension unnamedAssembly = new AssemblyNameExtension();

        private AssemblyNameExtension()
        {
        }

        internal AssemblyNameExtension(System.Reflection.AssemblyName assemblyName)
        {
            this.asAssemblyName = assemblyName;
        }

        internal AssemblyNameExtension(string assemblyName)
        {
            this.asString = assemblyName;
        }

        internal AssemblyNameExtension(string assemblyName, bool validate)
        {
            this.asString = assemblyName;
            if (validate)
            {
                this.CreateAssemblyName();
            }
        }

        private static int CompareBaseNamesStringWise(string asString1, string asString2)
        {
            if (asString1 == asString2)
            {
                return 0;
            }
            int index = asString1.IndexOf(',');
            int length = asString2.IndexOf(',');
            if (index == -1)
            {
                index = asString1.Length;
            }
            if (length == -1)
            {
                length = asString2.Length;
            }
            if (index == length)
            {
                return string.Compare(asString1, 0, asString2, 0, index, StringComparison.OrdinalIgnoreCase);
            }
            string strA = asString1.Substring(0, index);
            string strB = asString2.Substring(0, length);
            return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
        }

        internal int CompareBaseNameTo(AssemblyNameExtension that)
        {
            return this.CompareBaseNameToImpl(that);
        }

        private int CompareBaseNameToImpl(AssemblyNameExtension that)
        {
            if (this == that)
            {
                return 0;
            }
            if ((this.asAssemblyName != null) && (that.asAssemblyName != null))
            {
                if (this.asAssemblyName == that.asAssemblyName)
                {
                    return 0;
                }
                return string.Compare(this.asAssemblyName.Name, that.asAssemblyName.Name, StringComparison.OrdinalIgnoreCase);
            }
            if ((this.asString != null) && (that.asString != null))
            {
                return CompareBaseNamesStringWise(this.asString, that.asString);
            }
            return string.Compare(this.Name, that.Name, StringComparison.OrdinalIgnoreCase);
        }

        internal bool CompareCulture(AssemblyNameExtension that)
        {
            System.Globalization.CultureInfo cultureInfo = this.CultureInfo;
            System.Globalization.CultureInfo invariantCulture = that.CultureInfo;
            if (cultureInfo == null)
            {
                cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
            }
            if (invariantCulture == null)
            {
                invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            }
            if (cultureInfo.LCID != invariantCulture.LCID)
            {
                return false;
            }
            return true;
        }

        internal bool ComparePublicKeyToken(AssemblyNameExtension that)
        {
            byte[] publicKeyToken = this.GetPublicKeyToken();
            byte[] bPKT = that.GetPublicKeyToken();
            return ComparePublicKeyTokens(publicKeyToken, bPKT);
        }

        private static bool ComparePublicKeyTokens(byte[] aPKT, byte[] bPKT)
        {
            if (aPKT == null)
            {
                aPKT = new byte[0];
            }
            if (bPKT == null)
            {
                bPKT = new byte[0];
            }
            if (aPKT.Length != bPKT.Length)
            {
                return false;
            }
            for (int i = 0; i < aPKT.Length; i++)
            {
                if (aPKT[i] != bPKT[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal int CompareTo(AssemblyNameExtension that)
        {
            if (this.Equals(that))
            {
                return 0;
            }
            int num = this.CompareBaseNameTo(that);
            if (num != 0)
            {
                return num;
            }
            if (!(this.Version != that.Version))
            {
                return string.Compare(this.FullName, that.FullName, StringComparison.OrdinalIgnoreCase);
            }
            if (this.Version == null)
            {
                return -1;
            }
            return this.Version.CompareTo(that.Version);
        }

        private void CreateAssemblyName()
        {
            if (this.asAssemblyName == null)
            {
                this.asAssemblyName = GetAssemblyNameFromDisplayName(this.asString);
                if (this.asAssemblyName != null)
                {
                    this.hasProcessorArchitectureInFusionName = this.asString.IndexOf("ProcessorArchitecture", StringComparison.OrdinalIgnoreCase) != -1;
                    this.isSimpleName = (((this.Version == null) && (this.CultureInfo == null)) && (this.GetPublicKeyToken() == null)) && !this.hasProcessorArchitectureInFusionName;
                }
            }
        }

        private void CreateFullName()
        {
            if (this.asString == null)
            {
                this.asString = this.asAssemblyName.FullName;
            }
        }

        internal bool Equals(AssemblyNameExtension that)
        {
            return this.EqualsImpl(that, false);
        }

        internal bool EqualsIgnoreVersion(AssemblyNameExtension that)
        {
            return this.EqualsImpl(that, true);
        }

        private bool EqualsImpl(AssemblyNameExtension that, bool ignoreVersion)
        {
            if (!object.ReferenceEquals(this, that))
            {
                if (object.ReferenceEquals(that, null))
                {
                    return false;
                }
                if (((this.asAssemblyName != null) && (that.asAssemblyName != null)) && object.ReferenceEquals(this.asAssemblyName, that.asAssemblyName))
                {
                    return true;
                }
                if (((this.asString != null) && (that.asString != null)) && (this.asString == that.asString))
                {
                    return true;
                }
                if (string.Compare(this.Name, that.Name, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                if (!ignoreVersion && (this.Version != that.Version))
                {
                    return false;
                }
                if (!this.CompareCulture(that))
                {
                    return false;
                }
                if (!this.ComparePublicKeyToken(that))
                {
                    return false;
                }
            }
            return true;
        }

        internal static string EscapeDisplayNameCharacters(string displayName)
        {
            StringBuilder builder = new StringBuilder(displayName);
            return builder.Replace(@"\", @"\\").Replace("=", @"\=").Replace(",", @"\,").Replace("\"", "\\\"").Replace("'", @"\'").ToString();
        }

        internal static AssemblyNameExtension GetAssemblyNameEx(string path)
        {
            System.Reflection.AssemblyName assemblyName = null;
            try
            {
                assemblyName = System.Reflection.AssemblyName.GetAssemblyName(path);
            }
            catch (FileLoadException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            if (assemblyName == null)
            {
                return null;
            }
            return new AssemblyNameExtension(assemblyName);
        }

        private static System.Reflection.AssemblyName GetAssemblyNameFromDisplayName(string displayName)
        {
            return new System.Reflection.AssemblyName(displayName);
        }

        internal int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name);
        }

        internal byte[] GetPublicKeyToken()
        {
            this.CreateAssemblyName();
            return this.asAssemblyName.GetPublicKeyToken();
        }

        internal bool PartialNameCompare(AssemblyNameExtension that)
        {
            return this.PartialNameCompare(that, PartialComparisonFlags.Default);
        }

        internal bool PartialNameCompare(AssemblyNameExtension that, PartialComparisonFlags comparisonFlags)
        {
            if (!object.ReferenceEquals(this, that))
            {
                if (object.ReferenceEquals(that, null))
                {
                    return false;
                }
                if (((this.asAssemblyName != null) && (that.asAssemblyName != null)) && object.ReferenceEquals(this.asAssemblyName, that.asAssemblyName))
                {
                    return true;
                }
                if ((((comparisonFlags & PartialComparisonFlags.SimpleName) != 0) && (this.Name != null)) && !string.Equals(this.Name, that.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                if ((((comparisonFlags & PartialComparisonFlags.Version) != 0) && (this.Version != null)) && (this.Version != that.Version))
                {
                    return false;
                }
                if ((((comparisonFlags & PartialComparisonFlags.Culture) != 0) && (this.CultureInfo != null)) && ((that.CultureInfo == null) || !this.CompareCulture(that)))
                {
                    return false;
                }
                if ((((comparisonFlags & PartialComparisonFlags.PublicKeyToken) != 0) && (this.GetPublicKeyToken() != null)) && !this.ComparePublicKeyToken(that))
                {
                    return false;
                }
            }
            return true;
        }

        internal void ReplaceVersion(System.Version version)
        {
            this.CreateAssemblyName();
            if (this.asAssemblyName.Version != version)
            {
                this.asAssemblyName.Version = version;
                this.asString = null;
            }
        }

        public override string ToString()
        {
            this.CreateFullName();
            return this.asString;
        }

        internal System.Reflection.AssemblyName AssemblyName
        {
            get
            {
                this.CreateAssemblyName();
                return this.asAssemblyName;
            }
        }

        internal System.Globalization.CultureInfo CultureInfo
        {
            get
            {
                this.CreateAssemblyName();
                return this.asAssemblyName.CultureInfo;
            }
        }

        internal string FullName
        {
            get
            {
                this.CreateFullName();
                return this.asString;
            }
        }

        internal bool HasProcessorArchitectureInFusionName
        {
            get
            {
                this.CreateAssemblyName();
                return this.hasProcessorArchitectureInFusionName;
            }
        }

        internal bool IsSimpleName
        {
            get
            {
                this.CreateAssemblyName();
                return this.isSimpleName;
            }
        }

        internal bool IsUnnamedAssembly
        {
            get
            {
                return ((this.asAssemblyName == null) && (this.asString == null));
            }
        }

        internal string Name
        {
            get
            {
                this.CreateAssemblyName();
                return this.asAssemblyName.Name;
            }
        }

        internal static AssemblyNameExtension UnnamedAssembly
        {
            get
            {
                return unnamedAssembly;
            }
        }

        internal System.Version Version
        {
            get
            {
                this.CreateAssemblyName();
                return this.asAssemblyName.Version;
            }
        }
    }
}

