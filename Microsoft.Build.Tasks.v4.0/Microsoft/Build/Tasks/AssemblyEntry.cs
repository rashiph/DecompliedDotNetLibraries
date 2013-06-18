namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Globalization;

    internal class AssemblyEntry
    {
        private Microsoft.Build.Shared.AssemblyNameExtension assemblyName;
        private readonly string frameworkDirectory;
        private readonly string fullName;
        private readonly bool inGAC;
        private readonly bool? isRedistRoot;
        private readonly string redistName;
        private readonly string simpleName;

        public AssemblyEntry(string name, string version, string publicKeyToken, string culture, bool inGAC, bool? isRedistRoot, string redistName, string frameworkDirectory)
        {
            this.simpleName = name;
            this.fullName = string.Format(CultureInfo.InvariantCulture, "{0}, Version={1}, Culture={2}, PublicKeyToken={3}", new object[] { name, version, culture, publicKeyToken });
            this.inGAC = inGAC;
            this.isRedistRoot = isRedistRoot;
            this.redistName = redistName;
            this.frameworkDirectory = frameworkDirectory;
        }

        public Microsoft.Build.Shared.AssemblyNameExtension AssemblyNameExtension
        {
            get
            {
                if (this.assemblyName == null)
                {
                    this.assemblyName = new Microsoft.Build.Shared.AssemblyNameExtension(this.fullName);
                }
                return this.assemblyName;
            }
        }

        public string FrameworkDirectory
        {
            get
            {
                return this.frameworkDirectory;
            }
        }

        public string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        public bool InGAC
        {
            get
            {
                return this.inGAC;
            }
        }

        public bool? IsRedistRoot
        {
            get
            {
                return this.isRedistRoot;
            }
        }

        public string RedistName
        {
            get
            {
                return this.redistName;
            }
        }

        public string SimpleName
        {
            get
            {
                return this.simpleName;
            }
        }
    }
}

