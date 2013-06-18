namespace Microsoft.VisualBasic.Activities
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xml.Linq;

    public class VisualBasicImportReference : IEquatable<VisualBasicImportReference>
    {
        private System.Reflection.AssemblyName assemblyName;
        private string assemblyNameString;
        private static AssemblyNameEqualityComparer equalityComparer = new AssemblyNameEqualityComparer();
        private int hashCode;
        private string import;

        internal VisualBasicImportReference Clone()
        {
            VisualBasicImportReference reference = (VisualBasicImportReference) base.MemberwiseClone();
            reference.EarlyBoundAssembly = null;
            return reference;
        }

        public bool Equals(VisualBasicImportReference other)
        {
            if (other == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }
            if (this.EarlyBoundAssembly != other.EarlyBoundAssembly)
            {
                return false;
            }
            if (string.Compare(this.Import, other.Import, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            if ((this.AssemblyName == null) && (other.AssemblyName == null))
            {
                return true;
            }
            if ((this.AssemblyName == null) && (other.AssemblyName != null))
            {
                return false;
            }
            if ((this.AssemblyName != null) && (other.AssemblyName == null))
            {
                return false;
            }
            return equalityComparer.Equals(this.AssemblyName, other.AssemblyName);
        }

        internal void GenerateXamlNamespace(INamespacePrefixLookup namespaceLookup)
        {
            string ns = null;
            if ((this.Xmlns != null) && !string.IsNullOrEmpty(this.Xmlns.NamespaceName))
            {
                ns = this.Xmlns.NamespaceName;
            }
            else
            {
                ns = string.Format(CultureInfo.InvariantCulture, "clr-namespace:{0};assembly={1}", new object[] { this.Import, this.Assembly });
            }
            namespaceLookup.LookupPrefix(ns);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public string Assembly
        {
            get
            {
                return this.assemblyNameString;
            }
            set
            {
                if (value == null)
                {
                    this.assemblyName = null;
                    this.assemblyNameString = null;
                }
                else
                {
                    this.assemblyName = new System.Reflection.AssemblyName(value);
                    this.assemblyNameString = this.assemblyName.FullName;
                }
                this.EarlyBoundAssembly = null;
            }
        }

        internal System.Reflection.AssemblyName AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
        }

        internal System.Reflection.Assembly EarlyBoundAssembly { get; set; }

        public string Import
        {
            get
            {
                return this.import;
            }
            set
            {
                if (value != null)
                {
                    this.import = value.Trim();
                    this.hashCode = this.import.ToUpperInvariant().GetHashCode();
                }
                else
                {
                    this.import = null;
                    this.hashCode = 0;
                }
                this.EarlyBoundAssembly = null;
            }
        }

        internal XNamespace Xmlns { get; set; }
    }
}

