namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;

    internal class ResolutionSearchLocation
    {
        private AssemblyNameExtension assemblyName;
        private string fileNameAttempted;
        private NoMatchReason reason;
        private string searchPath;

        internal AssemblyNameExtension AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
            }
        }

        internal string FileNameAttempted
        {
            get
            {
                return this.fileNameAttempted;
            }
            set
            {
                this.fileNameAttempted = value;
            }
        }

        internal NoMatchReason Reason
        {
            get
            {
                return this.reason;
            }
            set
            {
                this.reason = value;
            }
        }

        internal string SearchPath
        {
            get
            {
                return this.searchPath;
            }
            set
            {
                this.searchPath = value;
            }
        }
    }
}

