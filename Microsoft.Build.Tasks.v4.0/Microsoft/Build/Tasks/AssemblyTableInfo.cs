namespace Microsoft.Build.Tasks
{
    using System;

    internal class AssemblyTableInfo : IComparable
    {
        private string descriptor;
        private readonly string frameworkDirectory;
        private readonly string path;

        internal AssemblyTableInfo(string path, string frameworkDirectory)
        {
            this.path = path;
            this.frameworkDirectory = frameworkDirectory;
        }

        public int CompareTo(object obj)
        {
            AssemblyTableInfo info = (AssemblyTableInfo) obj;
            return string.Compare(this.Descriptor, info.Descriptor, StringComparison.OrdinalIgnoreCase);
        }

        internal string Descriptor
        {
            get
            {
                if (this.descriptor == null)
                {
                    this.descriptor = this.path + this.frameworkDirectory;
                }
                return this.descriptor;
            }
        }

        internal string FrameworkDirectory
        {
            get
            {
                return this.frameworkDirectory;
            }
        }

        internal string Path
        {
            get
            {
                return this.path;
            }
        }
    }
}

