namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;

    public class CommandLineToolSwitch
    {
        private bool allowMultipleValues;
        private string argumentParameter;
        private bool argumentRequired;
        private bool booleanValue;
        private string description;
        private string displayName;
        private string fallback;
        private string falseSuffix;
        private bool isValid;
        private string name;
        private int number;
        private LinkedList<KeyValuePair<string, string>> overrides;
        private LinkedList<string> parents;
        private bool required;
        private string reverseSwitchValue;
        private bool reversible;
        private string separator;
        private string[] stringList;
        private string switchValue;
        private ITaskItem[] taskItemArray;
        private string trueSuffix;
        private CommandLineToolSwitchType type;
        private const string TypeBoolean = "CommandLineToolSwitchType.Boolean";
        private const string TypeInteger = "CommandLineToolSwitchType.Integer";
        private const string TypeITaskItem = "CommandLineToolSwitchType.ITaskItem";
        private const string TypeITaskItemArray = "CommandLineToolSwitchType.ITaskItemArray";
        private const string TypeStringArray = "CommandLineToolSwitchType.StringArray";
        private string value;

        public CommandLineToolSwitch()
        {
            this.name = string.Empty;
            this.falseSuffix = string.Empty;
            this.trueSuffix = string.Empty;
            this.separator = string.Empty;
            this.argumentParameter = string.Empty;
            this.fallback = string.Empty;
            this.parents = new LinkedList<string>();
            this.overrides = new LinkedList<KeyValuePair<string, string>>();
            this.booleanValue = true;
            this.value = string.Empty;
            this.switchValue = string.Empty;
            this.reverseSwitchValue = string.Empty;
            this.description = string.Empty;
            this.displayName = string.Empty;
        }

        public CommandLineToolSwitch(CommandLineToolSwitchType toolType)
        {
            this.name = string.Empty;
            this.falseSuffix = string.Empty;
            this.trueSuffix = string.Empty;
            this.separator = string.Empty;
            this.argumentParameter = string.Empty;
            this.fallback = string.Empty;
            this.parents = new LinkedList<string>();
            this.overrides = new LinkedList<KeyValuePair<string, string>>();
            this.booleanValue = true;
            this.value = string.Empty;
            this.switchValue = string.Empty;
            this.reverseSwitchValue = string.Empty;
            this.description = string.Empty;
            this.displayName = string.Empty;
            this.type = toolType;
        }

        public bool AllowMultipleValues
        {
            get
            {
                return this.allowMultipleValues;
            }
            set
            {
                this.allowMultipleValues = value;
            }
        }

        public bool ArgumentRequired
        {
            get
            {
                return this.argumentRequired;
            }
            set
            {
                this.argumentRequired = value;
            }
        }

        public bool BooleanValue
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.Boolean, "InvalidType", "CommandLineToolSwitchType.Boolean");
                return this.booleanValue;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.Boolean, "InvalidType", "CommandLineToolSwitchType.Boolean");
                this.booleanValue = value;
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

        public string FallbackArgumentParameter
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

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
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

        public int Number
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.Integer, "InvalidType", "CommandLineToolSwitchType.Integer");
                return this.number;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.Integer, "InvalidType", "CommandLineToolSwitchType.Integer");
                this.number = value;
            }
        }

        public LinkedList<KeyValuePair<string, string>> Overrides
        {
            get
            {
                return this.overrides;
            }
        }

        public LinkedList<string> Parents
        {
            get
            {
                return this.parents;
            }
        }

        public bool Required
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

        public string ReverseSwitchValue
        {
            get
            {
                return this.reverseSwitchValue;
            }
            set
            {
                this.reverseSwitchValue = value;
            }
        }

        public bool Reversible
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

        public string[] StringList
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.StringArray, "InvalidType", "CommandLineToolSwitchType.StringArray");
                return this.stringList;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.StringArray, "InvalidType", "CommandLineToolSwitchType.StringArray");
                this.stringList = value;
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

        public ITaskItem[] TaskItemArray
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.ITaskItemArray, "InvalidType", "CommandLineToolSwitchType.ITaskItemArray");
                return this.taskItemArray;
            }
            set
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.type == CommandLineToolSwitchType.ITaskItemArray, "InvalidType", "CommandLineToolSwitchType.ITaskItemArray");
                this.taskItemArray = value;
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

        public CommandLineToolSwitchType Type
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

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

