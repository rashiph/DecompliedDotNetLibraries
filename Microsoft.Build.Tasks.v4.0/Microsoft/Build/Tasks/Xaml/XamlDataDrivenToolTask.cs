namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Text;

    public abstract class XamlDataDrivenToolTask : ToolTask
    {
        private string[] acceptableNonZeroExitCodes;
        private Dictionary<string, CommandLineToolSwitch> activeToolSwitches;
        private Dictionary<string, CommandLineToolSwitch> activeToolSwitchesValues;
        private string additionalOptions;
        private string commandLine;
        private TaskLoggingHelper logPrivate;
        private bool skipResponseFileCommandGeneration;
        private IEnumerable<string> switchOrderList;
        private Dictionary<string, Dictionary<string, string>> values;

        protected XamlDataDrivenToolTask(string[] switchOrderList, ResourceManager taskResources) : base(taskResources)
        {
            this.activeToolSwitches = new Dictionary<string, CommandLineToolSwitch>(StringComparer.OrdinalIgnoreCase);
            this.values = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            this.additionalOptions = string.Empty;
            this.activeToolSwitchesValues = new Dictionary<string, CommandLineToolSwitch>(StringComparer.OrdinalIgnoreCase);
            this.InitializeLogger(taskResources);
            this.switchOrderList = switchOrderList;
            this.logPrivate = new TaskLoggingHelper(this);
            this.logPrivate.TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources;
            this.logPrivate.HelpKeywordPrefix = "MSBuild.";
        }

        public void AddActiveSwitchToolValue(CommandLineToolSwitch switchToAdd)
        {
            if ((switchToAdd.Type != CommandLineToolSwitchType.Boolean) || switchToAdd.BooleanValue)
            {
                if (switchToAdd.SwitchValue != string.Empty)
                {
                    this.ActiveToolSwitchesValues.Add(switchToAdd.SwitchValue, switchToAdd);
                }
            }
            else if (switchToAdd.ReverseSwitchValue != string.Empty)
            {
                this.ActiveToolSwitchesValues.Add(switchToAdd.ReverseSwitchValue, switchToAdd);
            }
        }

        public override bool Execute()
        {
            if (!string.IsNullOrEmpty(this.CommandLineTemplate))
            {
                base.UseCommandProcessor = true;
            }
            else if (string.IsNullOrEmpty(this.ToolExe))
            {
                base.Log.LogError(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Xaml.RuleMissingToolName", new object[0]), new object[0]);
                return false;
            }
            return base.Execute();
        }

        protected override string GenerateCommandLineCommands()
        {
            if (this.CommandLine.Length < 0x7d00)
            {
                this.skipResponseFileCommandGeneration = true;
                return this.CommandLine;
            }
            this.skipResponseFileCommandGeneration = false;
            return null;
        }

        private string GenerateCommands()
        {
            this.PostProcessSwitchList();
            this.CommandLine = new CommandLineGenerator(this.activeToolSwitches, this.SwitchOrderList) { CommandLineTemplate = this.CommandLineTemplate, AdditionalOptions = this.additionalOptions }.GenerateCommandLine();
            return this.CommandLine;
        }

        protected override string GenerateFullPathToTool()
        {
            return this.ToolName;
        }

        protected override string GenerateResponseFileCommands()
        {
            if (this.skipResponseFileCommandGeneration)
            {
                this.skipResponseFileCommandGeneration = false;
                return null;
            }
            return this.CommandLine;
        }

        internal string GetCommandLine_ForUnitTestsOnly()
        {
            return this.GenerateResponseFileCommands();
        }

        protected override bool HandleTaskExecutionErrors()
        {
            if (this.IsAcceptableReturnValue())
            {
                return true;
            }
            if (base.ExitCode == 5)
            {
                this.logPrivate.LogErrorWithCodeFromResources("Xaml.CommandFailedAccessDenied", new object[] { this.CommandLine, base.ExitCode });
            }
            else
            {
                this.logPrivate.LogErrorWithCodeFromResources("Xaml.CommandFailed", new object[] { this.CommandLine, base.ExitCode });
            }
            return false;
        }

        internal bool HasSwitch(string propertyName)
        {
            return (this.IsPropertySet(propertyName) && !string.IsNullOrEmpty(this.activeToolSwitches[propertyName].Name));
        }

        internal void InitializeLogger(ResourceManager taskResources)
        {
            this.logPrivate = new TaskLoggingHelper(this);
            this.logPrivate.TaskResources = Microsoft.Build.Shared.AssemblyResources.PrimaryResources;
            this.logPrivate.HelpKeywordPrefix = "MSBuild.";
        }

        internal bool IsAcceptableReturnValue()
        {
            if (this.AcceptableNonZeroExitCodes != null)
            {
                foreach (string str in this.AcceptableNonZeroExitCodes)
                {
                    if (base.ExitCode == Convert.ToInt32(str, CultureInfo.InvariantCulture))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsPropertySet(string propertyName)
        {
            return (!string.IsNullOrEmpty(propertyName) && this.activeToolSwitches.ContainsKey(propertyName));
        }

        internal void PostProcessSwitchList()
        {
            this.ValidateRelations();
            this.ValidateOverrides();
        }

        public string ReadSwitchMap(string propertyName, string[][] switchMap, string value)
        {
            if (switchMap != null)
            {
                for (int i = 0; i < switchMap.Length; i++)
                {
                    if (string.Equals(switchMap[i][0], value, StringComparison.OrdinalIgnoreCase))
                    {
                        return switchMap[i][1];
                    }
                }
                this.logPrivate.LogErrorWithCodeFromResources("Xaml.ArgumentOutOfRange", new object[] { propertyName, value });
            }
            return string.Empty;
        }

        public void ReplaceToolSwitch(CommandLineToolSwitch switchToAdd)
        {
            this.activeToolSwitches[switchToAdd.Name] = switchToAdd;
        }

        public bool ValidateInteger(string switchName, int min, int max, int value)
        {
            if ((value >= min) && (value <= max))
            {
                return true;
            }
            this.logPrivate.LogErrorWithCodeFromResources("Xaml.ArgumentOutOfRange", new object[] { switchName, value });
            return false;
        }

        internal void ValidateOverrides()
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, CommandLineToolSwitch> pair in this.ActiveToolSwitches)
            {
                foreach (KeyValuePair<string, string> pair2 in pair.Value.Overrides)
                {
                    if (string.Equals(pair2.Key, ((pair.Value.Type == CommandLineToolSwitchType.Boolean) && !pair.Value.BooleanValue) ? pair.Value.ReverseSwitchValue.TrimStart(new char[] { '/' }) : pair.Value.SwitchValue.TrimStart(new char[] { '/' }), StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (KeyValuePair<string, CommandLineToolSwitch> pair3 in this.ActiveToolSwitches)
                        {
                            if (!string.Equals(pair3.Key, pair.Key, StringComparison.OrdinalIgnoreCase))
                            {
                                if (string.Equals(pair3.Value.SwitchValue.TrimStart(new char[] { '/' }), pair2.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    list.Add(pair3.Key);
                                    break;
                                }
                                if (((pair3.Value.Type == CommandLineToolSwitchType.Boolean) && !pair3.Value.BooleanValue) && string.Equals(pair3.Value.ReverseSwitchValue.TrimStart(new char[] { '/' }), pair2.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    list.Add(pair3.Key);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            foreach (string str in list)
            {
                this.ActiveToolSwitches.Remove(str);
            }
        }

        protected override bool ValidateParameters()
        {
            return (!this.logPrivate.HasLoggedErrors && !base.Log.HasLoggedErrors);
        }

        internal void ValidateRelations()
        {
        }

        public virtual string[] AcceptableNonZeroExitCodes
        {
            get
            {
                return this.acceptableNonZeroExitCodes;
            }
            set
            {
                this.acceptableNonZeroExitCodes = value;
            }
        }

        protected internal Dictionary<string, CommandLineToolSwitch> ActiveToolSwitches
        {
            get
            {
                return this.activeToolSwitches;
            }
        }

        public Dictionary<string, CommandLineToolSwitch> ActiveToolSwitchesValues
        {
            get
            {
                return this.activeToolSwitchesValues;
            }
            set
            {
                this.activeToolSwitchesValues = value;
            }
        }

        public string AdditionalOptions
        {
            get
            {
                return this.additionalOptions;
            }
            set
            {
                this.additionalOptions = value;
            }
        }

        private string CommandLine
        {
            get
            {
                if (this.commandLine == null)
                {
                    this.commandLine = this.GenerateCommands();
                }
                return this.commandLine;
            }
            set
            {
                this.commandLine = value;
            }
        }

        public string CommandLineTemplate { get; set; }

        protected override Encoding ResponseFileEncoding
        {
            get
            {
                return Encoding.Unicode;
            }
        }

        internal virtual IEnumerable<string> SwitchOrderList
        {
            get
            {
                return this.switchOrderList;
            }
        }
    }
}

