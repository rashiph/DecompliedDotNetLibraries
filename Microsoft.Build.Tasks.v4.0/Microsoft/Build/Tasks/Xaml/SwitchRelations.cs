namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;

    internal class SwitchRelations
    {
        private List<string> conflicts = new List<string>();
        private List<string> excludedPlatforms = new List<string>();
        private Dictionary<string, List<string>> externalConflicts = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<string>> externalOverrides = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<string>> externalRequires = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private List<string> includedPlatforms = new List<string>();
        private List<string> overrides = new List<string>();
        private List<string> requires = new List<string>();
        private string status = string.Empty;
        private string switchValue = string.Empty;

        public SwitchRelations Clone()
        {
            return new SwitchRelations { switchValue = this.switchValue, status = this.status, conflicts = new List<string>(this.conflicts), overrides = new List<string>(this.overrides), requires = new List<string>(this.requires), excludedPlatforms = new List<string>(this.excludedPlatforms), includedPlatforms = new List<string>(this.includedPlatforms), externalConflicts = new Dictionary<string, List<string>>(this.externalConflicts, StringComparer.OrdinalIgnoreCase), externalOverrides = new Dictionary<string, List<string>>(this.externalOverrides, StringComparer.OrdinalIgnoreCase), externalRequires = new Dictionary<string, List<string>>(this.externalRequires, StringComparer.OrdinalIgnoreCase) };
        }

        public List<string> Conflicts
        {
            get
            {
                return this.conflicts;
            }
            set
            {
                this.conflicts = value;
            }
        }

        public List<string> ExcludedPlatforms
        {
            get
            {
                return this.excludedPlatforms;
            }
            set
            {
                this.excludedPlatforms = value;
            }
        }

        public Dictionary<string, List<string>> ExternalConflicts
        {
            get
            {
                return this.externalConflicts;
            }
            set
            {
                this.externalConflicts = value;
            }
        }

        public Dictionary<string, List<string>> ExternalOverrides
        {
            get
            {
                return this.externalOverrides;
            }
            set
            {
                this.externalOverrides = value;
            }
        }

        public Dictionary<string, List<string>> ExternalRequires
        {
            get
            {
                return this.externalRequires;
            }
            set
            {
                this.externalRequires = value;
            }
        }

        public List<string> IncludedPlatforms
        {
            get
            {
                return this.includedPlatforms;
            }
            set
            {
                this.includedPlatforms = value;
            }
        }

        public List<string> Overrides
        {
            get
            {
                return this.overrides;
            }
            set
            {
                this.overrides = value;
            }
        }

        public List<string> Requires
        {
            get
            {
                return this.requires;
            }
            set
            {
                this.requires = value;
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }

        public string SwitchValue
        {
            get
            {
                return this.switchValue;
            }
            set
            {
                this.switchValue = value;
            }
        }
    }
}

