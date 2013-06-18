namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Framework.XamlTypes;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xaml;

    internal class TaskParser
    {
        private string baseClass = "DataDrivenToolTask";
        private string defaultPrefix = string.Empty;
        private LinkedList<Property> defaultSet = new LinkedList<Property>();
        private LinkedList<string> errorLog = new LinkedList<string>();
        private Dictionary<string, string> fallbackSet = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private string name;
        private string namespaceValue = "XamlTaskNamespace";
        private LinkedList<Property> properties = new LinkedList<Property>();
        private string resourceNamespaceValue;
        private System.Collections.Generic.HashSet<string> switchesAdded = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private List<string> switchOrderList = new List<string>();
        private string toolName;

        private Property ObtainAttributes(BaseProperty baseProperty, Property parameterGroup)
        {
            Property property;
            if (parameterGroup != null)
            {
                property = parameterGroup.Clone();
            }
            else
            {
                property = new Property();
            }
            BoolProperty property2 = baseProperty as BoolProperty;
            DynamicEnumProperty property3 = baseProperty as DynamicEnumProperty;
            EnumProperty property4 = baseProperty as EnumProperty;
            IntProperty property5 = baseProperty as IntProperty;
            StringProperty property6 = baseProperty as StringProperty;
            StringListProperty property7 = baseProperty as StringListProperty;
            if (baseProperty.Name != null)
            {
                property.Name = baseProperty.Name;
            }
            if ((property2 != null) && !string.IsNullOrEmpty(property2.ReverseSwitch))
            {
                property.Reversible = "true";
            }
            if (property2 != null)
            {
                property.Type = PropertyType.Boolean;
            }
            else if (property4 != null)
            {
                property.Type = PropertyType.String;
            }
            else if (property3 != null)
            {
                property.Type = PropertyType.String;
            }
            else if (property5 != null)
            {
                property.Type = PropertyType.Integer;
            }
            else if (property6 != null)
            {
                property.Type = PropertyType.String;
            }
            else if (property7 != null)
            {
                property.Type = PropertyType.StringArray;
            }
            if (((baseProperty.DataSource != null) && !string.IsNullOrEmpty(baseProperty.DataSource.SourceType)) && baseProperty.DataSource.SourceType.Equals("Item", StringComparison.OrdinalIgnoreCase))
            {
                property.Type = PropertyType.ItemArray;
            }
            if (property5 != null)
            {
                property.Max = property5.MaxValue.HasValue ? property5.MaxValue.ToString() : null;
                property.Min = property5.MinValue.HasValue ? property5.MinValue.ToString() : null;
            }
            if (property2 != null)
            {
                property.ReverseSwitchName = property2.ReverseSwitch;
            }
            if (baseProperty.Switch != null)
            {
                property.SwitchName = baseProperty.Switch;
            }
            if (property7 != null)
            {
                property.Separator = property7.Separator;
            }
            if (baseProperty.Default != null)
            {
                property.DefaultValue = baseProperty.Default;
            }
            property.Required = baseProperty.IsRequired.ToString().ToLower(CultureInfo.InvariantCulture);
            if (baseProperty.Category != null)
            {
                property.Category = baseProperty.Category;
            }
            if (baseProperty.DisplayName != null)
            {
                property.DisplayName = baseProperty.DisplayName;
            }
            if (baseProperty.Description != null)
            {
                property.Description = baseProperty.Description;
            }
            if (baseProperty.SwitchPrefix != null)
            {
                property.Prefix = baseProperty.SwitchPrefix;
            }
            return property;
        }

        public bool Parse(string contentOrFile, string desiredRule)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentLength(contentOrFile, "contentOrFile");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentLength(desiredRule, "desiredRule");
            string path = null;
            bool flag = false;
            try
            {
                path = Path.GetFullPath(contentOrFile);
            }
            catch (Exception exception)
            {
                if (Microsoft.Build.Shared.ExceptionHandling.IsCriticalException(exception))
                {
                    throw;
                }
            }
            if (path != null)
            {
                if (!File.Exists(path))
                {
                    throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Xaml.RuleFileNotFound", new object[] { path }));
                }
                flag = this.ParseXamlDocument(new StreamReader(path), desiredRule);
            }
            else
            {
                flag = this.ParseXamlDocument(new StringReader(contentOrFile), desiredRule);
            }
            if (flag)
            {
                return flag;
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            foreach (string str2 in this.ErrorLog)
            {
                builder.AppendLine(str2);
            }
            throw new ArgumentException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Xaml.RuleParseFailed", new object[] { builder.ToString() }));
        }

        private bool ParseParameter(BaseProperty baseProperty, LinkedList<Property> propertyList, Property property)
        {
            Property property2 = this.ObtainAttributes(baseProperty, property);
            if (string.IsNullOrEmpty(property2.Name))
            {
                property2.Name = "AlwaysAppend";
            }
            if (!this.switchesAdded.Contains(property2.Name))
            {
                this.switchOrderList.Add(property2.Name);
            }
            if (string.IsNullOrEmpty(property2.Prefix))
            {
                property2.Prefix = this.DefaultPrefix;
            }
            EnumProperty property3 = baseProperty as EnumProperty;
            if (property3 != null)
            {
                foreach (EnumValue value2 in property3.AdmissibleValues)
                {
                    Value value3 = new Value {
                        Name = value2.Name,
                        SwitchName = value2.Switch
                    };
                    if (value3.SwitchName == null)
                    {
                        value3.SwitchName = string.Empty;
                    }
                    value3.DisplayName = value2.DisplayName;
                    value3.Description = value2.Description;
                    value3.Prefix = value2.SwitchPrefix;
                    if (string.IsNullOrEmpty(value3.Prefix))
                    {
                        value3.Prefix = property3.SwitchPrefix;
                    }
                    if (string.IsNullOrEmpty(value3.Prefix))
                    {
                        value3.Prefix = this.DefaultPrefix;
                    }
                    if (value2.Arguments.Count > 0)
                    {
                        value3.Arguments = new ArrayList();
                        foreach (Microsoft.Build.Framework.XamlTypes.Argument argument in value2.Arguments)
                        {
                            Microsoft.Build.Tasks.Xaml.Argument argument2 = new Microsoft.Build.Tasks.Xaml.Argument {
                                Parameter = argument.Property,
                                Separator = argument.Separator,
                                Required = argument.IsRequired
                            };
                            value3.Arguments.Add(argument2);
                        }
                    }
                    if (value3.Prefix == null)
                    {
                        value3.Prefix = property2.Prefix;
                    }
                    property2.Values.Add(value3);
                }
            }
            foreach (Microsoft.Build.Framework.XamlTypes.Argument argument3 in baseProperty.Arguments)
            {
                if (property2.Arguments == null)
                {
                    property2.Arguments = new ArrayList();
                }
                Microsoft.Build.Tasks.Xaml.Argument argument4 = new Microsoft.Build.Tasks.Xaml.Argument {
                    Parameter = argument3.Property,
                    Separator = argument3.Separator,
                    Required = argument3.IsRequired
                };
                property2.Arguments.Add(argument4);
            }
            propertyList.AddLast(property2);
            return true;
        }

        private bool ParseParameterGroupOrParameter(BaseProperty baseProperty, LinkedList<Property> propertyList, Property property)
        {
            if (!this.ParseParameter(baseProperty, propertyList, property))
            {
                return false;
            }
            return true;
        }

        internal bool ParseXamlDocument(Rule rule)
        {
            if (rule == null)
            {
                return false;
            }
            this.defaultPrefix = rule.SwitchPrefix;
            this.toolName = string.IsNullOrEmpty(rule.ToolName) ? rule.ToolName : rule.ToolName.Replace(@"\", @"\\");
            this.name = rule.Name;
            foreach (BaseProperty property in rule.Properties)
            {
                if (property.IncludeInCommandLine && !this.ParseParameterGroupOrParameter(property, this.properties, null))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool ParseXamlDocument(TextReader reader, string desiredRule)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(reader, "reader");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentLength(desiredRule, "desiredRule");
            object obj2 = XamlServices.Load(reader);
            if (obj2 == null)
            {
                return false;
            }
            ProjectSchemaDefinitions definitions = obj2 as ProjectSchemaDefinitions;
            if (definitions == null)
            {
                throw new XamlParseException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Xaml.InvalidRootObject", new object[0]));
            }
            foreach (IProjectSchemaNode node in definitions.Nodes)
            {
                Rule rule = node as Rule;
                if ((rule != null) && string.Equals(rule.Name, desiredRule, StringComparison.OrdinalIgnoreCase))
                {
                    return this.ParseXamlDocument(rule);
                }
            }
            throw new XamlParseException(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("Xaml.RuleNotFound", new object[] { desiredRule }));
        }

        public string BaseClass
        {
            get
            {
                return this.baseClass;
            }
        }

        public string DefaultPrefix
        {
            get
            {
                return this.defaultPrefix;
            }
        }

        public LinkedList<Property> DefaultSet
        {
            get
            {
                return this.defaultSet;
            }
        }

        public LinkedList<string> ErrorLog
        {
            get
            {
                return this.errorLog;
            }
        }

        public Dictionary<string, string> FallbackSet
        {
            get
            {
                return this.fallbackSet;
            }
        }

        public string GeneratedTaskName
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

        public string Namespace
        {
            get
            {
                return this.namespaceValue;
            }
        }

        public LinkedList<Property> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public string ResourceNamespace
        {
            get
            {
                return this.resourceNamespaceValue;
            }
        }

        public IEnumerable<string> SwitchOrderList
        {
            get
            {
                return this.switchOrderList;
            }
        }

        public string ToolName
        {
            get
            {
                return this.toolName;
            }
        }
    }
}

