namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;

    internal class UnifiedAssemblyName
    {
        private bool isPrerequisite;
        private bool? isRedistRoot;
        private bool isUnified;
        private AssemblyNameExtension postUnified;
        private AssemblyNameExtension preUnified;
        private string redistName;
        private Microsoft.Build.Tasks.UnificationReason unificationReason;

        public UnifiedAssemblyName(AssemblyNameExtension preUnified, AssemblyNameExtension postUnified, bool isUnified, Microsoft.Build.Tasks.UnificationReason unificationReason, bool isPrerequisite, bool? isRedistRoot, string redistName)
        {
            this.preUnified = preUnified;
            this.postUnified = postUnified;
            this.isUnified = isUnified;
            this.isPrerequisite = isPrerequisite;
            this.isRedistRoot = isRedistRoot;
            this.redistName = redistName;
            this.unificationReason = unificationReason;
        }

        public bool IsPrerequisite
        {
            get
            {
                return this.isPrerequisite;
            }
        }

        public bool? IsRedistRoot
        {
            get
            {
                return this.isRedistRoot;
            }
        }

        public bool IsUnified
        {
            get
            {
                return this.isUnified;
            }
        }

        public AssemblyNameExtension PostUnified
        {
            get
            {
                return this.postUnified;
            }
        }

        public AssemblyNameExtension PreUnified
        {
            get
            {
                return this.preUnified;
            }
        }

        public string RedistName
        {
            get
            {
                return this.redistName;
            }
        }

        public Microsoft.Build.Tasks.UnificationReason UnificationReason
        {
            get
            {
                return this.unificationReason;
            }
        }
    }
}

