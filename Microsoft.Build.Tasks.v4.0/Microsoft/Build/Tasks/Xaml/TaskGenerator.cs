namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Shared;
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class TaskGenerator
    {
        private const string AddActiveSwitchToolValueMethod = "AddActiveSwitchToolValue";
        private const string AddDefaultsToActiveSwitchList = "AddDefaultsToActiveSwitchList";
        private const string AddFallbacksToActiveSwitchList = "AddFallbacksToActiveSwitchList";
        private const string AddLastMethod = "AddLast";
        private const string AddMethod = "Add";
        private const string AlwaysType = "always";
        private const string AppendAlwaysMethod = "AlwaysAppend";
        private const string ArgumentProperty = "ArgumentParameter";
        private const string ArgumentRelationList = "ArgumentRelationList";
        private const string ArgumentRequiredProperty = "ArgumentRequired";
        private const string BaseClass = "DataDrivenToolTask";
        private const string BooleanValueProperty = "BooleanValue";
        private Dictionary<string, Property> dependencyList;
        private const string DescriptionProperty = "Description";
        private const string DictionaryOfSwitches = "ActiveToolSwitches";
        private const string DictionaryOfSwitchesValues = "ActiveToolSwitchesValues";
        private const string DisplayNameProperty = "DisplayName";
        private const string EnsureTrailingSlashMethod = "EnsureTrailingSlash";
        private int errorCount;
        private LinkedList<string> errorLog;
        private const string FallbackProperty = "FallbackArgumentParameter";
        private const string FalseSuffixProperty = "FalseSuffix";
        private const string FileNameProperty = "Value";
        private const string ImportType = "import";
        private const string IsOff = "false";
        private const string IsOn = "true";
        private const string IsPropertySetMethod = "IsPropertySet";
        private const string IsSwitchValueSetMethod = "IsSwitchValueSet";
        private const string IsValidProperty = "IsValid";
        private const string MultiValues = "AllowMultipleValues";
        private const string NameProperty = "Name";
        private const string NamespaceOfGeneratedTask = "MyDataDrivenTasks";
        private const string NumberProperty = "Number";
        private const string OutputProperty = "Output";
        private const string Overrides = "Overrides";
        private const string ParentProperty = "Parents";
        private string platform;
        private string[] propertiesTypesToIgnore;
        private const string PropertyRequiredProperty = "Required";
        private const string ReadSwitchMapMethod = "ReadSwitchMap";
        private const string Relation = "relation";
        private RelationsParser relationsParser;
        private const string RemoveMethod = "Remove";
        private const string ReplaceToolSwitchMethod = "ReplaceToolSwitch";
        private const string RequiredProperty = "Required";
        private const string ReverseSwitchValueProperty = "ReverseSwitchValue";
        private const string ReversibleProperty = "Reversible";
        private const string SeparatorProperty = "Separator";
        private const string StringListProperty = "StringList";
        private const string SwitchMap = "switchMap";
        private const string SwitchToAdd = "switchToAdd";
        private const string SwitchValueProperty = "SwitchValue";
        private const string TaskItemArrayProperty = "TaskItemArray";
        private const string TaskItemProperty = "TaskItem";
        private TaskParser taskParser;
        private const string ToolExeFieldName = "toolExe";
        private const string ToolExePropertyName = "ToolExe";
        private const string ToolNamePropertyName = "ToolName";
        private const string TrueSuffixProperty = "TrueSuffix";
        private const string TypeAlways = "always";
        private const string TypeAlwaysAppend = "AlwaysAppend";
        private const string TypeArgumentRelation = "CommandLineArgumentRelation";
        private const string TypeBoolean = "Boolean";
        private const string TypeDirectory = "Directory";
        private const string TypeFile = "File";
        private const string TypeInteger = "Integer";
        private const string TypeITaskItem = "ITaskItem";
        private const string TypeITaskItemArray = "ITaskItemArray";
        private const string TypeKeyValuePairStrings = "KeyValuePair<string,string>";
        private const string TypeProperty = "Type";
        private const string TypeString = "String";
        private const string TypeStringArray = "StringArray";
        private const string TypeToolSwitch = "CommandLineToolSwitch";
        private const string TypeToolSwitchType = "CommandLineToolSwitchType";
        private const string ValidateIntegerMethod = "ValidateInteger";
        private const string ValidateRelationsMethod = "ValidateRelations";
        private const string ValueAttribute = "value";
        private const string ValueProperty = "Value";

        public TaskGenerator()
        {
            this.propertiesTypesToIgnore = new string[] { "AdditionalOptions" };
            this.platform = string.Empty;
            this.dependencyList = new Dictionary<string, Property>(StringComparer.OrdinalIgnoreCase);
            this.errorLog = new LinkedList<string>();
            this.taskParser = new TaskParser();
            this.relationsParser = new RelationsParser();
        }

        internal TaskGenerator(TaskParser parser)
        {
            this.propertiesTypesToIgnore = new string[] { "AdditionalOptions" };
            this.platform = string.Empty;
            this.dependencyList = new Dictionary<string, Property>(StringComparer.OrdinalIgnoreCase);
            this.errorLog = new LinkedList<string>();
            this.taskParser = new TaskParser();
            this.relationsParser = new RelationsParser();
            this.taskParser = parser;
        }

        private bool ContainsCurrentPlatform(Property property)
        {
            if (this.Platform == null)
            {
                return true;
            }
            if (property.Values.Count <= 0)
            {
                return this.ContainsCurrentPlatform(property.SwitchName);
            }
            bool flag = false;
            foreach (Value value2 in property.Values)
            {
                flag = this.ContainsCurrentPlatform(value2.SwitchName) || flag;
            }
            return flag;
        }

        private bool ContainsCurrentPlatform(string SwitchValue)
        {
            if ((this.Platform != null) && this.relationsParser.SwitchRelationsList.ContainsKey(SwitchValue))
            {
                SwitchRelations relations = this.relationsParser.SwitchRelationsList[SwitchValue];
                if (relations.ExcludedPlatforms.Count > 0)
                {
                    foreach (string str in relations.ExcludedPlatforms)
                    {
                        if (this.Platform == str)
                        {
                            return false;
                        }
                    }
                }
                if (relations.IncludedPlatforms.Count > 0)
                {
                    bool flag = false;
                    foreach (string str2 in relations.IncludedPlatforms)
                    {
                        if (this.Platform == str2)
                        {
                            flag = true;
                        }
                    }
                    return flag;
                }
            }
            return true;
        }

        private string EnsureSwitchIsPrefixed(string prefix, string toolSwitch)
        {
            if (!string.IsNullOrEmpty(toolSwitch))
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    if (toolSwitch[0] != this.taskParser.DefaultPrefix[0])
                    {
                        return (this.taskParser.DefaultPrefix + toolSwitch);
                    }
                    return toolSwitch;
                }
                if (toolSwitch[0] != prefix[0])
                {
                    return (prefix + toolSwitch);
                }
            }
            return toolSwitch;
        }

        private void GenerateAssignPropertyToString(CodeMemberProperty propertyName, string property, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), property), new CodeSnippetExpression(this.SurroundWithQuotes(value)));
                propertyName.SetStatements.Add(statement);
            }
        }

        private void GenerateAssignPropertyToValue(CodeMemberProperty propertyName, string property, CodeExpression value)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(value != null, "NullValue", property);
            CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), property), value);
            propertyName.SetStatements.Add(statement);
        }

        private void GenerateAssignToolSwitch(CodeMemberProperty propertyName, string property, string prefix, string toolSwitchName)
        {
            if (!string.IsNullOrEmpty(toolSwitchName))
            {
                this.GenerateAssignPropertyToString(propertyName, property, prefix + toolSwitchName);
            }
        }

        private void GenerateBooleans(Property property, CodeMemberProperty propertyName)
        {
            this.GenerateCommon(property, propertyName, "Boolean", typeof(bool), "BooleanValue");
            this.GenerateAssignToolSwitch(propertyName, "SwitchValue", property.Prefix, property.SwitchName);
            this.GenerateAssignToolSwitch(propertyName, "ReverseSwitchValue", property.Prefix, property.ReverseSwitchName);
            CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
            propertyName.SetStatements.Add(statement);
            this.GenerateCommonSetStatements(property, propertyName, "BooleanValue");
        }

        internal CodeCompileUnit GenerateCode()
        {
            try
            {
                CodeCompileUnit unit = new CodeCompileUnit();
                CodeNamespace namespace2 = new CodeNamespace(this.taskParser.Namespace);
                CodeTypeDeclaration declaration = new CodeTypeDeclaration(this.taskParser.GeneratedTaskName);
                if (this.GenerateComments)
                {
                    declaration.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                    string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ClassDescription", new object[] { this.taskParser.GeneratedTaskName });
                    declaration.Comments.Add(new CodeCommentStatement(text, true));
                    declaration.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
                }
                declaration.IsClass = true;
                declaration.IsPartial = true;
                declaration.BaseTypes.Add(new CodeTypeReference("XamlDataDrivenToolTask"));
                namespace2.Types.Add(declaration);
                unit.Namespaces.Add(namespace2);
                this.RemovePropertiesWithIgnoredTypes(this.taskParser.Properties);
                this.GenerateImports(namespace2);
                this.GenerateConstructor(declaration);
                this.GenerateToolNameProperty(declaration);
                this.GenerateProperties(declaration, this.taskParser.Properties);
                while (this.dependencyList.Count > 0)
                {
                    LinkedList<Property> propertyList = new LinkedList<Property>();
                    foreach (KeyValuePair<string, Property> pair in this.dependencyList)
                    {
                        propertyList.AddLast(pair.Value);
                    }
                    this.dependencyList.Clear();
                    this.GenerateProperties(declaration, propertyList);
                }
                this.GenerateDefaultSetProperties(declaration);
                this.GenerateFallbacks(declaration);
                this.GenerateRelations(declaration);
                return unit;
            }
            catch (ConfigurationException exception)
            {
                this.LogError("InvalidLanguage", new object[] { exception.Message });
            }
            return null;
        }

        private void GenerateCommon(Property property, CodeMemberProperty propertyName, string type, Type returnType, string valueName)
        {
            propertyName.Type = new CodeTypeReference(returnType);
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsPropertySet", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)) })
            };
            statement.TrueStatements.Add(new CodeMethodReturnStatement(new CodePropertyReferenceExpression(new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("ActiveToolSwitches"), new CodeExpression[] { new CodeVariableReferenceExpression(this.SurroundWithQuotes(property.Name)) }), valueName)));
            if (property.Type == PropertyType.Boolean)
            {
                statement.FalseStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("false")));
            }
            else if (property.Type == PropertyType.Integer)
            {
                statement.FalseStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(0)));
            }
            else
            {
                statement.FalseStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression("null")));
            }
            propertyName.GetStatements.Add(statement);
            CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement(new CodeTypeReference("CommandLineToolSwitch"), "switchToAdd", new CodeObjectCreateExpression("CommandLineToolSwitch", new CodeExpression[] { new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("CommandLineToolSwitchType"), type) }));
            propertyName.SetStatements.Add(statement2);
            if (!string.IsNullOrEmpty(property.Reversible) && string.Equals(property.Reversible, "true", StringComparison.OrdinalIgnoreCase))
            {
                this.GenerateAssignPropertyToValue(propertyName, "Reversible", new CodeSnippetExpression(property.Reversible));
            }
            this.GenerateAssignPropertyToString(propertyName, "ArgumentParameter", property.Argument);
            this.GenerateAssignPropertyToString(propertyName, "Separator", property.Separator);
            this.GenerateAssignPropertyToString(propertyName, "DisplayName", property.DisplayName);
            this.GenerateAssignPropertyToString(propertyName, "Description", property.Description);
            if (!string.IsNullOrEmpty(property.Required) && string.Equals(property.Required, "true", StringComparison.OrdinalIgnoreCase))
            {
                this.GenerateAssignPropertyToValue(propertyName, "Required", new CodeSnippetExpression(property.Required));
            }
            this.GenerateAssignPropertyToString(propertyName, "FallbackArgumentParameter", property.Fallback);
            this.GenerateAssignPropertyToString(propertyName, "FalseSuffix", property.FalseSuffix);
            this.GenerateAssignPropertyToString(propertyName, "TrueSuffix", property.TrueSuffix);
            if (property.Parents.Count > 0)
            {
                foreach (string str in property.Parents)
                {
                    CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Parents"), "AddLast", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(str)) });
                    propertyName.SetStatements.Add(expression);
                }
            }
            if (this.GenerateComments)
            {
                propertyName.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("PropertyNameDescription", new object[] { property.Name });
                propertyName.Comments.Add(new CodeCommentStatement(text, true));
                text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("PropertyTypeDescription", new object[] { type });
                propertyName.Comments.Add(new CodeCommentStatement(text, true));
                text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("PropertySwitchDescription", new object[] { property.SwitchName });
                propertyName.Comments.Add(new CodeCommentStatement(text, true));
                propertyName.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
            }
        }

        private void GenerateCommonSetStatements(Property property, CodeMemberProperty propertyName, string referencedProperty)
        {
            if (referencedProperty != null)
            {
                CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), referencedProperty), new CodePropertySetValueReferenceExpression());
                propertyName.SetStatements.Add(statement);
            }
            propertyName.SetStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "ReplaceToolSwitch", new CodeExpression[] { new CodeSnippetExpression("switchToAdd") }));
        }

        private void GenerateConstructor(CodeTypeDeclaration taskClass)
        {
            CodeConstructor constructor = new CodeConstructor {
                Attributes = MemberAttributes.Public
            };
            CodeTypeReference createType = new CodeTypeReference("System.Resources.ResourceManager");
            CodeSnippetExpression expression = new CodeSnippetExpression(this.SurroundWithQuotes(this.taskParser.ResourceNamespace));
            CodeTypeReferenceExpression targetObject = new CodeTypeReferenceExpression("System.Reflection.Assembly");
            CodeMethodReferenceExpression method = new CodeMethodReferenceExpression(targetObject, "GetExecutingAssembly");
            CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression(method, new CodeExpression[0]);
            CodeObjectCreateExpression expression5 = new CodeObjectCreateExpression(createType, new CodeExpression[] { expression, expression4 });
            CodeTypeReference reference2 = new CodeTypeReference(new CodeTypeReference("System.String"), 1);
            List<CodeExpression> list = new List<CodeExpression>();
            foreach (string str in this.taskParser.SwitchOrderList)
            {
                list.Add(new CodeSnippetExpression(this.SurroundWithQuotes(str)));
            }
            CodeArrayCreateExpression expression6 = new CodeArrayCreateExpression(reference2, list.ToArray());
            constructor.BaseConstructorArgs.Add(expression6);
            constructor.BaseConstructorArgs.Add(expression5);
            taskClass.Members.Add(constructor);
            if (this.GenerateComments)
            {
                constructor.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ConstructorDescription", new object[0]);
                constructor.Comments.Add(new CodeCommentStatement(text, true));
                constructor.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
            }
        }

        private void GenerateDefaultSetProperties(CodeTypeDeclaration taskClass)
        {
            if (this.taskParser.DefaultSet.Count > 0)
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "AddDefaultsToActiveSwitchList",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                foreach (Property property in this.taskParser.DefaultSet)
                {
                    CodeConditionStatement statement = new CodeConditionStatement {
                        Condition = new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "IsPropertySet"), new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)) }), CodeBinaryOperatorType.IdentityEquality, new CodeSnippetExpression("false"))
                    };
                    if (property.Type == PropertyType.Boolean)
                    {
                        statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property.Name), new CodeSnippetExpression(property.DefaultValue)));
                    }
                    else
                    {
                        statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(property.Name), new CodeSnippetExpression(this.SurroundWithQuotes(property.DefaultValue))));
                    }
                    method.Statements.Add(statement);
                }
                taskClass.Members.Add(method);
                if (this.GenerateComments)
                {
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                    string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("AddDefaultsToActiveSwitchListDescription", new object[0]);
                    method.Comments.Add(new CodeCommentStatement(text, true));
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
                }
            }
        }

        private void GenerateFallbacks(CodeTypeDeclaration taskClass)
        {
            if (this.taskParser.FallbackSet.Count > 0)
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "AddFallbacksToActiveSwitchList",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                foreach (KeyValuePair<string, string> pair in this.taskParser.FallbackSet)
                {
                    CodeConditionStatement statement = new CodeConditionStatement();
                    CodeMethodInvokeExpression right = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsPropertySet", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(pair.Value)) });
                    CodeBinaryOperatorExpression left = new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "IsPropertySet", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(pair.Key)) }), CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression("false"));
                    statement.Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right);
                    statement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(pair.Key), new CodeVariableReferenceExpression(pair.Value)));
                    method.Statements.Add(statement);
                }
                taskClass.Members.Add(method);
                if (this.GenerateComments)
                {
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                    string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("AddFallbacksToActiveSwitchListDescription", new object[0]);
                    method.Comments.Add(new CodeCommentStatement(text, true));
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
                }
            }
        }

        private void GenerateImports(CodeNamespace codeNamespace)
        {
            string[] strArray = new string[] { "System", "System.Globalization", "System.Collections", "System.Collections.Generic", "System.Diagnostics", "System.IO", "Microsoft.Build.Utilities", "Microsoft.Build.Framework", "Microsoft.Build.Tasks.Xaml" };
            foreach (string str in strArray)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport(str));
            }
        }

        private void GenerateIntegers(Property property, CodeMemberProperty propertyName)
        {
            CodeExpression[] expressionArray;
            this.GenerateCommon(property, propertyName, "Integer", typeof(int), "Number");
            string unformattedText = (property.SwitchName != string.Empty) ? (property.Prefix + property.SwitchName) : property.Name;
            if (!string.IsNullOrEmpty(property.Min) && !string.IsNullOrEmpty(property.Max))
            {
                expressionArray = new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(unformattedText)), new CodePrimitiveExpression(int.Parse(property.Min, CultureInfo.CurrentCulture)), new CodePrimitiveExpression(int.Parse(property.Max, CultureInfo.CurrentCulture)), new CodePropertySetValueReferenceExpression() };
            }
            else if (!string.IsNullOrEmpty(property.Min))
            {
                expressionArray = new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(unformattedText)), new CodePrimitiveExpression(int.Parse(property.Min, CultureInfo.CurrentCulture)), new CodeSnippetExpression("Int32.MaxValue"), new CodePropertySetValueReferenceExpression() };
            }
            else if (!string.IsNullOrEmpty(property.Max))
            {
                expressionArray = new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(unformattedText)), new CodeSnippetExpression("Int32.MinValue"), new CodePrimitiveExpression(int.Parse(property.Max, CultureInfo.CurrentCulture)), new CodePropertySetValueReferenceExpression() };
            }
            else
            {
                expressionArray = new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(unformattedText)), new CodeSnippetExpression("Int32.MinValue"), new CodeSnippetExpression("Int32.MaxValue"), new CodePropertySetValueReferenceExpression() };
            }
            CodeMethodReferenceExpression method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "ValidateInteger");
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = new CodeMethodInvokeExpression(method, expressionArray)
            };
            statement.TrueStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "IsValid"), new CodeSnippetExpression("true")));
            statement.FalseStatements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "IsValid"), new CodeSnippetExpression("false")));
            propertyName.SetStatements.Add(statement);
            CodeAssignStatement statement2 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
            propertyName.SetStatements.Add(statement2);
            this.GenerateAssignToolSwitch(propertyName, "SwitchValue", property.Prefix, property.SwitchName);
            this.GenerateCommonSetStatements(property, propertyName, "Number");
        }

        private void GenerateITaskItemArray(Property property, CodeMemberProperty propertyName)
        {
            CodeTypeReference reference = new CodeTypeReference {
                BaseType = "ITaskItem",
                ArrayRank = 1
            };
            this.GenerateCommon(property, propertyName, "ITaskItemArray", typeof(Array), "TaskItemArray");
            propertyName.Type = reference;
            CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
            propertyName.SetStatements.Add(statement);
            this.GenerateAssignToolSwitch(propertyName, "SwitchValue", property.Prefix, property.SwitchName);
            this.GenerateCommonSetStatements(property, propertyName, "TaskItemArray");
        }

        private void GenerateOverrides(Property property, CodeMemberProperty propertyName)
        {
            if (this.relationsParser.SwitchRelationsList.ContainsKey(property.SwitchName))
            {
                SwitchRelations relations = this.relationsParser.SwitchRelationsList[property.SwitchName];
                if (relations.Overrides.Count > 0)
                {
                    foreach (string str in relations.Overrides)
                    {
                        propertyName.SetStatements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Overrides"), "AddLast", new CodeExpression[] { new CodeObjectCreateExpression(new CodeTypeReference("KeyValuePair<string,string>"), new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(relations.SwitchValue)), new CodeSnippetExpression(this.SurroundWithQuotes(str)) }) }));
                    }
                }
                if (property.ReverseSwitchName != "")
                {
                    relations = this.relationsParser.SwitchRelationsList[property.ReverseSwitchName];
                    if (relations.Overrides.Count > 0)
                    {
                        foreach (string str2 in relations.Overrides)
                        {
                            propertyName.SetStatements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Overrides"), "AddLast", new CodeExpression[] { new CodeObjectCreateExpression(new CodeTypeReference("KeyValuePair<string,string>"), new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(relations.SwitchValue)), new CodeSnippetExpression(this.SurroundWithQuotes(str2)) }) }));
                        }
                    }
                }
            }
        }

        private void GenerateProperties(CodeTypeDeclaration taskClass, LinkedList<Property> propertyList)
        {
            foreach (Property property in propertyList)
            {
                if (!string.Equals(property.Name, "import", StringComparison.OrdinalIgnoreCase) && this.ContainsCurrentPlatform(property))
                {
                    CodeAttributeDeclarationCollection declarations = new CodeAttributeDeclarationCollection();
                    CodeMemberProperty propertyName = new CodeMemberProperty {
                        Name = property.Name,
                        HasGet = true,
                        HasSet = true,
                        Attributes = MemberAttributes.Public
                    };
                    if (!string.IsNullOrEmpty(property.DefaultValue))
                    {
                        this.taskParser.DefaultSet.AddLast(property);
                    }
                    if (!string.IsNullOrEmpty(property.Required) && (property.Required == "true"))
                    {
                        declarations.Add(new CodeAttributeDeclaration("Required"));
                    }
                    if (property.Output)
                    {
                        declarations.Add(new CodeAttributeDeclaration("Output"));
                    }
                    if (string.IsNullOrEmpty(property.Argument) && !string.IsNullOrEmpty(property.Fallback))
                    {
                        this.taskParser.FallbackSet.Add(property.Name, property.Fallback);
                    }
                    if (property.Type == PropertyType.StringArray)
                    {
                        this.GenerateStringArrays(property, propertyName);
                    }
                    else if (property.Type == PropertyType.String)
                    {
                        this.GenerateStrings(property, propertyName);
                    }
                    else if (property.Type == PropertyType.Boolean)
                    {
                        this.GenerateBooleans(property, propertyName);
                    }
                    else if (property.Type == PropertyType.Integer)
                    {
                        this.GenerateIntegers(property, propertyName);
                    }
                    else if (property.Type == PropertyType.ItemArray)
                    {
                        this.GenerateITaskItemArray(property, propertyName);
                    }
                    else
                    {
                        this.LogError("ImproperType", new object[] { property.Name, property.Type });
                    }
                    foreach (Property property3 in property.Dependencies)
                    {
                        if (!this.dependencyList.ContainsKey(property3.Name))
                        {
                            this.dependencyList.Add(property3.Name, property3);
                            property3.Parents.AddLast(property.Name);
                        }
                        else if (!this.dependencyList[property3.Name].Parents.Contains(property.Name))
                        {
                            this.dependencyList[property3.Name].Parents.AddLast(property.Name);
                        }
                    }
                    this.GenerateOverrides(property, propertyName);
                    propertyName.CustomAttributes = declarations;
                    taskClass.Members.Add(propertyName);
                }
            }
        }

        private void GenerateRelations(CodeTypeDeclaration taskClass)
        {
            if (this.relationsParser.SwitchRelationsList.Count > 0)
            {
                CodeMemberMethod method = new CodeMemberMethod {
                    Name = "ValidateRelations",
                    Attributes = MemberAttributes.Family | MemberAttributes.Override
                };
                foreach (KeyValuePair<string, SwitchRelations> pair in this.relationsParser.SwitchRelationsList)
                {
                    if (pair.Value.Requires.Count > 0)
                    {
                        CodeConditionStatement statement = new CodeConditionStatement {
                            Condition = null
                        };
                        foreach (string str in pair.Value.Requires)
                        {
                            if (statement.Condition != null)
                            {
                                statement.Condition = new CodeBinaryOperatorExpression(statement.Condition, CodeBinaryOperatorType.BooleanAnd, new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "IsSwitchValueSet"), new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(str)) }), CodeBinaryOperatorType.IdentityEquality, new CodeSnippetExpression("false")));
                            }
                            else
                            {
                                statement.Condition = new CodeBinaryOperatorExpression(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "IsSwitchValueSet"), new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(str)) }), CodeBinaryOperatorType.IdentityEquality, new CodeSnippetExpression("false"));
                            }
                        }
                        statement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "RemoveSwitchToolBasedOnValue", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(pair.Key)) }));
                        method.Statements.Add(statement);
                    }
                }
                taskClass.Members.Add(method);
                if (this.GenerateComments)
                {
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                    string text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("AddValidateRelationsMethod", new object[0]);
                    method.Comments.Add(new CodeCommentStatement(text, true));
                    method.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
                }
            }
        }

        private void GenerateStringArrays(Property property, CodeMemberProperty propertyName)
        {
            CodeTypeReference reference = new CodeTypeReference {
                BaseType = "System.String",
                ArrayRank = 1
            };
            this.GenerateCommon(property, propertyName, "StringArray", typeof(Array), "StringList");
            propertyName.Type = reference;
            this.GenerateAssignToolSwitch(propertyName, "SwitchValue", property.Prefix, property.SwitchName);
            CodeAssignStatement statement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
            propertyName.SetStatements.Add(statement);
            this.GenerateCommonSetStatements(property, propertyName, "StringList");
        }

        private void GenerateStrings(Property property, CodeMemberProperty propertyName)
        {
            this.GenerateCommon(property, propertyName, "String", typeof(string), "Value");
            string referencedProperty = null;
            if (property.Values.Count > 0)
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement("System.String[][]", "switchMap");
                List<CodeExpression> list = new List<CodeExpression>();
                int num = 0;
                CodeTypeReference createType = new CodeTypeReference(typeof(string));
                foreach (Value value2 in property.Values)
                {
                    if (this.ContainsCurrentPlatform(value2.SwitchName))
                    {
                        CodeSnippetExpression[] initializers = new CodeSnippetExpression[2];
                        initializers[0] = new CodeSnippetExpression(this.SurroundWithQuotes(value2.Name));
                        if (value2.SwitchName != string.Empty)
                        {
                            initializers[1] = new CodeSnippetExpression(this.SurroundWithQuotes(value2.Prefix + value2.SwitchName));
                        }
                        else
                        {
                            initializers[1] = new CodeSnippetExpression(this.SurroundWithQuotes(""));
                        }
                        CodeArrayCreateExpression item = new CodeArrayCreateExpression(createType, initializers);
                        list.Add(item);
                        num++;
                    }
                }
                CodeArrayCreateExpression expression2 = new CodeArrayCreateExpression("System.String[][]", list.ToArray());
                statement.InitExpression = expression2;
                propertyName.SetStatements.Add(statement);
                CodeAssignStatement statement2 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "SwitchValue"), new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "ReadSwitchMap", new CodeExpression[] { new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)), new CodeVariableReferenceExpression("switchMap"), new CodeVariableReferenceExpression("value") }));
                propertyName.SetStatements.Add(statement2);
                CodeAssignStatement statement3 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
                propertyName.SetStatements.Add(statement3);
                referencedProperty = "Value";
                this.GenerateAssignPropertyToValue(propertyName, "AllowMultipleValues", new CodeSnippetExpression("true"));
            }
            else
            {
                CodeAssignStatement statement4 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "Name"), new CodeSnippetExpression(this.SurroundWithQuotes(property.Name)));
                propertyName.SetStatements.Add(statement4);
                referencedProperty = "Value";
                CodeAssignStatement statement5 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("switchToAdd"), "SwitchValue"), (property.SwitchName != string.Empty) ? new CodeSnippetExpression(this.SurroundWithQuotes(property.Prefix + property.SwitchName)) : new CodeSnippetExpression(this.SurroundWithQuotes("")));
                propertyName.SetStatements.Add(statement5);
                this.GenerateAssignToolSwitch(propertyName, "ReverseSwitchValue", property.Prefix, property.ReverseSwitchName);
            }
            this.GenerateCommonSetStatements(property, propertyName, referencedProperty);
        }

        private void GenerateToolNameProperty(CodeTypeDeclaration taskClass)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Name = "ToolName",
                HasGet = true,
                HasSet = false,
                Attributes = MemberAttributes.Family | MemberAttributes.Override,
                Type = new CodeTypeReference(typeof(string))
            };
            string text = null;
            if (this.GenerateComments)
            {
                text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ToolExeFieldDescription", new object[0]);
                property.GetStatements.Add(new CodeCommentStatement(text, false));
            }
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(this.SurroundWithQuotes(this.taskParser.ToolName))));
            taskClass.Members.Add(property);
            if (this.GenerateComments)
            {
                property.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("StartSummary", new object[0]), true));
                text = Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("ToolNameDescription", new object[0]);
                property.Comments.Add(new CodeCommentStatement(text, true));
                property.Comments.Add(new CodeCommentStatement(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("EndSummary", new object[0]), true));
            }
        }

        private void LogError(string messageResourceName, params object[] messageArgs)
        {
            this.errorLog.AddLast(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(messageResourceName, messageArgs));
            this.errorCount++;
        }

        internal void RemovePropertiesWithIgnoredTypes(LinkedList<Property> propertyList)
        {
            LinkedList<Property> list = new LinkedList<Property>();
            foreach (Property property in propertyList)
            {
                foreach (string str in this.propertiesTypesToIgnore)
                {
                    if (string.Equals(property.Name, str, StringComparison.OrdinalIgnoreCase))
                    {
                        list.AddFirst(property);
                    }
                }
            }
            foreach (Property property2 in list)
            {
                propertyList.Remove(property2);
            }
        }

        private string SurroundWithQuotes(string unformattedText)
        {
            if (string.IsNullOrEmpty(unformattedText))
            {
                return "@\"\"";
            }
            return ("@\"" + unformattedText.Replace("\"", "\"\"") + "\"");
        }

        internal int ErrorCount
        {
            get
            {
                return this.errorCount;
            }
        }

        internal LinkedList<string> ErrorLog
        {
            get
            {
                return this.errorLog;
            }
        }

        public bool GenerateComments { get; set; }

        private string Platform
        {
            get
            {
                return this.platform;
            }
        }
    }
}

