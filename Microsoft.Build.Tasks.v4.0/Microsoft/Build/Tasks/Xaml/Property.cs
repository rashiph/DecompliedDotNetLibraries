namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class Property
    {
        private string argument = string.Empty;
        private ArrayList arguments = new ArrayList();
        private string category = string.Empty;
        private LinkedList<Property> conflicts = new LinkedList<Property>();
        private string defaultValue = string.Empty;
        private LinkedList<Property> dependencies = new LinkedList<Property>();
        private string description = string.Empty;
        private string displayName = string.Empty;
        private string fallback = string.Empty;
        private string falseSuffix = string.Empty;
        private string max = string.Empty;
        private string min = string.Empty;
        private string name = string.Empty;
        private bool output;
        private LinkedList<string> parents = new LinkedList<string>();
        private string prefix;
        private string required = string.Empty;
        private string reverseSwitchName = string.Empty;
        private string reversible = string.Empty;
        private string separator = string.Empty;
        private string switchName = string.Empty;
        private string trueSuffix = string.Empty;
        private PropertyType type;
        private ArrayList values = new ArrayList();

        public Property Clone()
        {
            return new Property { Type = this.type, SwitchName = this.switchName, ReverseSwitchName = this.reverseSwitchName, FalseSuffix = this.falseSuffix, TrueSuffix = this.trueSuffix, Max = this.max, Min = this.min, Separator = this.separator, DefaultValue = this.defaultValue, Argument = this.argument, Fallback = this.fallback, Required = this.required, Output = this.output, Reversible = this.reversible, Name = this.name, Prefix = this.prefix };
        }

        public string Argument
        {
            get
            {
                return this.argument;
            }
            set
            {
                this.argument = value;
            }
        }

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

        public string Category
        {
            get
            {
                return this.category;
            }
            set
            {
                this.category = value;
            }
        }

        public LinkedList<Property> Conflicts
        {
            get
            {
                return this.dependencies;
            }
        }

        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        public LinkedList<Property> Dependencies
        {
            get
            {
                return this.dependencies;
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

        public string Fallback
        {
            get
            {
                return this.fallback;
            }
            set
            {
                this.fallback = value;
            }
        }

        public string FalseSuffix
        {
            get
            {
                return this.falseSuffix;
            }
            set
            {
                this.falseSuffix = value;
            }
        }

        public string Max
        {
            get
            {
                return this.max;
            }
            set
            {
                this.max = value;
            }
        }

        public string Min
        {
            get
            {
                return this.min;
            }
            set
            {
                this.min = value;
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

        public bool Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
            }
        }

        public LinkedList<string> Parents
        {
            get
            {
                return this.parents;
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

        public string Required
        {
            get
            {
                return this.required;
            }
            set
            {
                this.required = value;
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

        public string Reversible
        {
            get
            {
                return this.reversible;
            }
            set
            {
                this.reversible = value;
            }
        }

        public string Separator
        {
            get
            {
                return this.separator;
            }
            set
            {
                this.separator = value;
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

        public string TrueSuffix
        {
            get
            {
                return this.trueSuffix;
            }
            set
            {
                this.trueSuffix = value;
            }
        }

        public PropertyType Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        public ArrayList Values
        {
            get
            {
                return this.values;
            }
        }
    }
}

