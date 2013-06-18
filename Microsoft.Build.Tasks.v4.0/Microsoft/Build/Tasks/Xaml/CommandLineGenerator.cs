namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Framework.XamlTypes;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class CommandLineGenerator
    {
        private Dictionary<string, CommandLineToolSwitch> activeCommandLineToolSwitches;
        private string additionalOptions;
        private IEnumerable<string> switchOrderList;

        public CommandLineGenerator(Rule rule, Dictionary<string, object> parameterValues)
        {
            this.activeCommandLineToolSwitches = new Dictionary<string, CommandLineToolSwitch>(StringComparer.OrdinalIgnoreCase);
            this.additionalOptions = string.Empty;
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(rule, "rule");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(parameterValues, "parameterValues");
            TaskParser parser = new TaskParser();
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(parser.ParseXamlDocument(rule), "Unable to parse specified file or contents.");
            this.switchOrderList = parser.SwitchOrderList;
            this.activeCommandLineToolSwitches = new Dictionary<string, CommandLineToolSwitch>(StringComparer.OrdinalIgnoreCase);
            foreach (Property property in parser.Properties)
            {
                object obj2 = null;
                if (parameterValues.TryGetValue(property.Name, out obj2))
                {
                    CommandLineToolSwitch switch2 = new CommandLineToolSwitch();
                    if (!string.IsNullOrEmpty(property.Reversible) && string.Equals(property.Reversible, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        switch2.Reversible = true;
                    }
                    switch2.Separator = property.Separator;
                    switch2.DisplayName = property.DisplayName;
                    switch2.Description = property.Description;
                    if (!string.IsNullOrEmpty(property.Required) && string.Equals(property.Required, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        switch2.Required = true;
                    }
                    switch2.FallbackArgumentParameter = property.Fallback;
                    switch2.FalseSuffix = property.FalseSuffix;
                    switch2.TrueSuffix = property.TrueSuffix;
                    if (!string.IsNullOrEmpty(property.SwitchName))
                    {
                        switch2.SwitchValue = property.Prefix + property.SwitchName;
                    }
                    switch2.IsValid = true;
                    switch (property.Type)
                    {
                        case PropertyType.Boolean:
                            switch2.Type = CommandLineToolSwitchType.Boolean;
                            switch2.BooleanValue = (bool) obj2;
                            if (!string.IsNullOrEmpty(property.ReverseSwitchName))
                            {
                                switch2.ReverseSwitchValue = property.Prefix + property.ReverseSwitchName;
                            }
                            break;

                        case PropertyType.String:
                            switch2.Type = CommandLineToolSwitchType.String;
                            switch2.ReverseSwitchValue = property.Prefix + property.ReverseSwitchName;
                            if (property.Values.Count > 0)
                            {
                                string b = (string) obj2;
                                switch2.Value = (string) obj2;
                                switch2.AllowMultipleValues = true;
                                foreach (Value value2 in property.Values)
                                {
                                    if (string.Equals(value2.Name, b, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (!string.IsNullOrEmpty(value2.SwitchName))
                                        {
                                            switch2.SwitchValue = value2.Prefix + value2.SwitchName;
                                        }
                                        else
                                        {
                                            switch2 = null;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                switch2.Value = (string) obj2;
                            }
                            break;

                        case PropertyType.Integer:
                            switch2.Type = CommandLineToolSwitchType.Integer;
                            switch2.Number = (int) obj2;
                            if (!string.IsNullOrEmpty(property.Min) && (switch2.Number < Convert.ToInt32(property.Min, Thread.CurrentThread.CurrentCulture)))
                            {
                                switch2.IsValid = false;
                            }
                            if (!string.IsNullOrEmpty(property.Max) && (switch2.Number > Convert.ToInt32(property.Max, Thread.CurrentThread.CurrentCulture)))
                            {
                                switch2.IsValid = false;
                            }
                            break;

                        case PropertyType.StringArray:
                            switch2.Type = CommandLineToolSwitchType.StringArray;
                            switch2.StringList = (string[]) obj2;
                            break;

                        case PropertyType.ItemArray:
                            switch2.Type = CommandLineToolSwitchType.ITaskItemArray;
                            switch2.TaskItemArray = (ITaskItem[]) obj2;
                            break;
                    }
                    if (switch2 != null)
                    {
                        this.activeCommandLineToolSwitches[property.Name] = switch2;
                    }
                }
            }
        }

        internal CommandLineGenerator(Dictionary<string, CommandLineToolSwitch> activeCommandLineToolSwitches, IEnumerable<string> switchOrderList)
        {
            this.activeCommandLineToolSwitches = new Dictionary<string, CommandLineToolSwitch>(StringComparer.OrdinalIgnoreCase);
            this.additionalOptions = string.Empty;
            this.activeCommandLineToolSwitches = activeCommandLineToolSwitches;
            this.switchOrderList = switchOrderList;
        }

        internal void BuildAdditionalArgs(CommandLineBuilder cmdLine)
        {
            if ((cmdLine != null) && !string.IsNullOrEmpty(this.additionalOptions))
            {
                cmdLine.AppendSwitch(this.additionalOptions);
            }
        }

        private void EmitBooleanSwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            if (commandLineToolSwitch.BooleanValue)
            {
                if (!string.IsNullOrEmpty(commandLineToolSwitch.SwitchValue))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Insert(0, commandLineToolSwitch.Separator);
                    builder.Insert(0, commandLineToolSwitch.TrueSuffix);
                    builder.Insert(0, commandLineToolSwitch.SwitchValue);
                    clb.AppendSwitch(builder.ToString());
                }
            }
            else
            {
                this.EmitReversibleBooleanSwitch(clb, commandLineToolSwitch);
            }
        }

        private void EmitIntegerSwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            if (commandLineToolSwitch.IsValid)
            {
                string switchValue = commandLineToolSwitch.Number.ToString(Thread.CurrentThread.CurrentCulture);
                if (!this.PerformSwitchValueSubstition(clb, commandLineToolSwitch, switchValue))
                {
                    if (!string.IsNullOrEmpty(commandLineToolSwitch.Separator))
                    {
                        clb.AppendSwitch(commandLineToolSwitch.SwitchValue + commandLineToolSwitch.Separator + switchValue);
                    }
                    else
                    {
                        clb.AppendSwitch(commandLineToolSwitch.SwitchValue + switchValue);
                    }
                }
            }
        }

        private void EmitReversibleBooleanSwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            if (!string.IsNullOrEmpty(commandLineToolSwitch.ReverseSwitchValue))
            {
                string str = commandLineToolSwitch.BooleanValue ? commandLineToolSwitch.TrueSuffix : commandLineToolSwitch.FalseSuffix;
                StringBuilder builder = new StringBuilder();
                builder.Insert(0, str);
                builder.Insert(0, commandLineToolSwitch.Separator);
                builder.Insert(0, commandLineToolSwitch.TrueSuffix);
                builder.Insert(0, commandLineToolSwitch.ReverseSwitchValue);
                clb.AppendSwitch(builder.ToString());
            }
        }

        private void EmitStringArraySwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            List<string> list = new List<string>(commandLineToolSwitch.StringList.Length);
            for (int i = 0; i < commandLineToolSwitch.StringList.Length; i++)
            {
                string str;
                if (commandLineToolSwitch.StringList[i].StartsWith("\"", StringComparison.OrdinalIgnoreCase) && commandLineToolSwitch.StringList[i].EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                {
                    str = commandLineToolSwitch.StringList[i].Substring(1, commandLineToolSwitch.StringList[i].Length - 2).Trim();
                }
                else
                {
                    str = commandLineToolSwitch.StringList[i].Trim();
                }
                if (!string.IsNullOrEmpty(str))
                {
                    list.Add(str);
                }
            }
            string[] strArray = list.ToArray();
            if (string.IsNullOrEmpty(commandLineToolSwitch.Separator))
            {
                foreach (string str2 in strArray)
                {
                    if (!this.PerformSwitchValueSubstition(clb, commandLineToolSwitch, str2))
                    {
                        clb.AppendSwitchIfNotNull(commandLineToolSwitch.SwitchValue, str2);
                    }
                }
            }
            else if (!this.PerformSwitchValueSubstition(clb, commandLineToolSwitch, string.Join(commandLineToolSwitch.Separator, strArray)))
            {
                clb.AppendSwitchIfNotNull(commandLineToolSwitch.SwitchValue, strArray, commandLineToolSwitch.Separator);
            }
        }

        private void EmitStringSwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            if (!this.PerformSwitchValueSubstition(clb, commandLineToolSwitch, commandLineToolSwitch.Value))
            {
                string switchName = string.Empty + commandLineToolSwitch.SwitchValue + commandLineToolSwitch.Separator;
                string source = commandLineToolSwitch.Value;
                if (!commandLineToolSwitch.AllowMultipleValues)
                {
                    source = source.Trim();
                    if (source.Contains<char>(' ') && !source.StartsWith("\"", StringComparison.OrdinalIgnoreCase))
                    {
                        source = "\"" + source;
                        if (source.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) && !source.EndsWith(@"\\", StringComparison.OrdinalIgnoreCase))
                        {
                            source = source + "\\\"";
                        }
                        else
                        {
                            source = source + "\"";
                        }
                    }
                }
                else
                {
                    switchName = string.Empty;
                    source = commandLineToolSwitch.SwitchValue;
                }
                clb.AppendSwitchUnquotedIfNotNull(switchName, source);
            }
        }

        private static void EmitTaskItemArraySwitch(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch)
        {
            if (string.IsNullOrEmpty(commandLineToolSwitch.Separator))
            {
                foreach (ITaskItem item in commandLineToolSwitch.TaskItemArray)
                {
                    clb.AppendSwitchIfNotNull(commandLineToolSwitch.SwitchValue, item.ItemSpec);
                }
            }
            else
            {
                clb.AppendSwitchIfNotNull(commandLineToolSwitch.SwitchValue, commandLineToolSwitch.TaskItemArray, commandLineToolSwitch.Separator);
            }
        }

        public string GenerateCommandLine()
        {
            CommandLineBuilder builder = new CommandLineBuilder(true);
            if (!string.IsNullOrEmpty(this.CommandLineTemplate))
            {
                this.GenerateTemplatedCommandLine(builder);
            }
            else
            {
                this.GenerateStandardCommandLine(builder, false);
            }
            return builder.ToString();
        }

        internal void GenerateCommandsAccordingToType(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch, bool recursive)
        {
            if ((commandLineToolSwitch.Parents.Count <= 0) || recursive)
            {
                switch (commandLineToolSwitch.Type)
                {
                    case CommandLineToolSwitchType.Boolean:
                        this.EmitBooleanSwitch(clb, commandLineToolSwitch);
                        return;

                    case CommandLineToolSwitchType.Integer:
                        this.EmitIntegerSwitch(clb, commandLineToolSwitch);
                        return;

                    case CommandLineToolSwitchType.String:
                        this.EmitStringSwitch(clb, commandLineToolSwitch);
                        return;

                    case CommandLineToolSwitchType.StringArray:
                        this.EmitStringArraySwitch(clb, commandLineToolSwitch);
                        return;

                    case CommandLineToolSwitchType.ITaskItemArray:
                        EmitTaskItemArraySwitch(clb, commandLineToolSwitch);
                        return;
                }
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "InternalError");
            }
        }

        private void GenerateStandardCommandLine(CommandLineBuilder builder, bool allOptionsMode)
        {
            foreach (string str in this.switchOrderList)
            {
                if (this.IsPropertySet(str))
                {
                    CommandLineToolSwitch property = this.activeCommandLineToolSwitches[str];
                    if ((!allOptionsMode || ((property.Type != CommandLineToolSwitchType.ITaskItemArray) && !string.Equals(str, "AdditionalOptions", StringComparison.OrdinalIgnoreCase))) && (this.VerifyDependenciesArePresent(property) && this.VerifyRequiredArgumentsArePresent(property, false)))
                    {
                        this.GenerateCommandsAccordingToType(builder, property, false);
                    }
                }
                else if (string.Equals(str, "AlwaysAppend", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendSwitch(this.AlwaysAppend);
                }
            }
            if (!allOptionsMode)
            {
                this.BuildAdditionalArgs(builder);
            }
        }

        private void GenerateTemplatedCommandLine(CommandLineBuilder builder)
        {
            string pattern = @"\[[^\[\]]+\]";
            MatchCollection matchs = new Regex(pattern, RegexOptions.ECMAScript).Matches(this.CommandLineTemplate);
            int startIndex = 0;
            foreach (Match match in matchs)
            {
                if (match.Length == 0)
                {
                    continue;
                }
                int index = match.Index;
                goto Label_0059;
            Label_0053:
                index--;
            Label_0059:
                if (index > startIndex)
                {
                    char ch = this.CommandLineTemplate[index - 1];
                    if (ch.Equals('['))
                    {
                        goto Label_0053;
                    }
                }
                if (index != startIndex)
                {
                    builder.AppendTextUnquoted(this.CommandLineTemplate.Substring(startIndex, index - startIndex));
                }
                int num3 = (match.Index - index) + 1;
                if ((num3 % 2) == 0)
                {
                    for (int i = 0; i < (num3 / 2); i++)
                    {
                        builder.AppendTextUnquoted("[");
                    }
                    builder.AppendTextUnquoted(match.Value.Substring(1, match.Value.Length - 1));
                }
                else
                {
                    for (int j = 0; j < ((num3 - 1) / 2); j++)
                    {
                        builder.AppendTextUnquoted("[");
                    }
                    string a = match.Value.Substring(1, match.Value.Length - 2);
                    if (string.Equals(a, "AllOptions", StringComparison.OrdinalIgnoreCase))
                    {
                        CommandLineBuilder builder2 = new CommandLineBuilder(true);
                        this.GenerateStandardCommandLine(builder2, true);
                        builder.AppendTextUnquoted(builder2.ToString());
                    }
                    else if (string.Equals(a, "AdditionalOptions", StringComparison.OrdinalIgnoreCase))
                    {
                        this.BuildAdditionalArgs(builder);
                    }
                    else if (this.IsPropertySet(a))
                    {
                        CommandLineToolSwitch property = this.activeCommandLineToolSwitches[a];
                        if (this.VerifyDependenciesArePresent(property) && this.VerifyRequiredArgumentsArePresent(property, false))
                        {
                            CommandLineBuilder clb = new CommandLineBuilder(true);
                            this.GenerateCommandsAccordingToType(clb, property, false);
                            builder.AppendTextUnquoted(clb.ToString());
                        }
                    }
                    else if (!this.PropertyExists(a))
                    {
                        builder.AppendTextUnquoted('[' + a + ']');
                    }
                }
                startIndex = match.Index + match.Length;
            }
            builder.AppendTextUnquoted(this.CommandLineTemplate.Substring(startIndex, this.CommandLineTemplate.Length - startIndex));
        }

        internal bool HasSwitch(string propertyName)
        {
            return (this.IsPropertySet(propertyName) && !string.IsNullOrEmpty(this.activeCommandLineToolSwitches[propertyName].Name));
        }

        internal bool IsPropertySet(string propertyName)
        {
            return (!string.IsNullOrEmpty(propertyName) && this.activeCommandLineToolSwitches.ContainsKey(propertyName));
        }

        private bool PerformSwitchValueSubstition(CommandLineBuilder clb, CommandLineToolSwitch commandLineToolSwitch, string switchValue)
        {
            string str2;
            Match match = new Regex(@"\[value]", RegexOptions.IgnoreCase).Match(commandLineToolSwitch.SwitchValue);
            if (!match.Success)
            {
                return false;
            }
            string str = commandLineToolSwitch.SwitchValue.Substring(match.Index + match.Length, commandLineToolSwitch.SwitchValue.Length - (match.Index + match.Length));
            if ((!switchValue.EndsWith(@"\\", StringComparison.OrdinalIgnoreCase) && switchValue.EndsWith(@"\", StringComparison.OrdinalIgnoreCase)) && (str[0] == '"'))
            {
                str2 = commandLineToolSwitch.SwitchValue.Substring(0, match.Index) + switchValue + @"\" + str;
            }
            else
            {
                str2 = commandLineToolSwitch.SwitchValue.Substring(0, match.Index) + switchValue + str;
            }
            clb.AppendSwitch(str2);
            return true;
        }

        internal bool PropertyExists(string propertyName)
        {
            return this.switchOrderList.Contains<string>(propertyName, StringComparer.OrdinalIgnoreCase);
        }

        internal bool VerifyDependenciesArePresent(CommandLineToolSwitch property)
        {
            if (property.Parents.Count <= 0)
            {
                return true;
            }
            bool flag = false;
            foreach (string str in property.Parents)
            {
                flag = flag || this.HasSwitch(str);
            }
            return flag;
        }

        internal bool VerifyRequiredArgumentsArePresent(CommandLineToolSwitch property, bool throwOnError)
        {
            return true;
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

        public string AlwaysAppend { get; set; }

        public string CommandLineTemplate { get; set; }
    }
}

