namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections;

    internal class Value
    {
        private ArrayList arguments = new ArrayList();
        private string description = string.Empty;
        private string displayName = string.Empty;
        private string name = string.Empty;
        private string prefix;
        private string reverseSwitchName = string.Empty;
        private string switchName = string.Empty;

        public ArrayList Arguments
        {
            get
            {
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
            set
            {
                this.prefix = value;
            }
        }

        public string ReverseSwitchName
        {
            get
            {
                return this.reverseSwitchName;
            }
            set
            {
                this.reverseSwitchName = value;
            }
        }

        public string SwitchName
        {
            get
            {
                return this.switchName;
            }
            set
            {
                this.switchName = value;
            }
        }
    }
}

