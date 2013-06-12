namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    internal abstract class BaseTemplateCodeDomTreeGenerator : BaseCodeDomTreeGenerator
    {
        private int _controlCount;
        private const string _localVariableRef = "__ctrl";
        private TemplateParser _parser;
        protected static readonly string applyStyleSheetMethodName = "ApplyStyleSheetSkin";
        protected static readonly string buildMethodPrefix = "__BuildControl";
        protected static readonly string extractTemplateValuesMethodPrefix = "__ExtractValues";
        private const int minLongLiteralStringLength = 0x100;
        protected static readonly string pagePropertyName = "Page";
        private const string renderMethodParameterName = "__w";
        internal const string skinIDPropertyName = "SkinID";
        protected static readonly string templateSourceDirectoryName = "AppRelativeTemplateSourceDirectory";
        internal const string tempObjectVariable = "__o";

        internal BaseTemplateCodeDomTreeGenerator(TemplateParser parser) : base(parser)
        {
            this._parser = parser;
        }

        private void AddEntryBuildersToList(ICollection entries, ArrayList list)
        {
            if ((entries != null) && (list != null))
            {
                foreach (BuilderPropertyEntry entry in entries)
                {
                    if (entry.Builder != null)
                    {
                        TemplatePropertyEntry entry2 = entry as TemplatePropertyEntry;
                        if ((entry2 == null) || !entry2.IsMultiple)
                        {
                            list.Add(entry.Builder);
                        }
                    }
                }
            }
        }

        private void AddOutputWriteStatement(CodeStatementCollection methodStatements, CodeExpression expr, CodeLinePragma linePragma)
        {
            CodeStatement outputWriteStatement = this.GetOutputWriteStatement(expr, false);
            if (linePragma != null)
            {
                outputWriteStatement.LinePragma = linePragma;
            }
            methodStatements.Add(outputWriteStatement);
        }

        private void AddOutputWriteStringStatement(CodeStatementCollection methodStatements, string s)
        {
            if (!this.UseResourceLiteralString(s))
            {
                this.AddOutputWriteStatement(methodStatements, new CodePrimitiveExpression(s), null);
            }
            else
            {
                int num;
                int num2;
                bool flag;
                base._stringResourceBuilder.AddString(s, out num, out num2, out flag);
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression();
                CodeExpressionStatement statement = new CodeExpressionStatement(expression);
                expression.Method.TargetObject = new CodeThisReferenceExpression();
                expression.Method.MethodName = "WriteUTF8ResourceString";
                expression.Parameters.Add(new CodeArgumentReferenceExpression("__w"));
                expression.Parameters.Add(new CodePrimitiveExpression(num));
                expression.Parameters.Add(new CodePrimitiveExpression(num2));
                expression.Parameters.Add(new CodePrimitiveExpression(flag));
                methodStatements.Add(statement);
            }
        }

        private string BindingMethodName(ControlBuilder builder)
        {
            return ("__DataBind" + builder.ID);
        }

        private static void BuildAddParsedSubObjectStatement(CodeStatementCollection statements, CodeExpression ctrlToAdd, CodeLinePragma linePragma, CodeExpression ctrlRefExpr, ref bool gotParserVariable)
        {
            if (!gotParserVariable)
            {
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement {
                    Name = "__parser",
                    Type = new CodeTypeReference(typeof(IParserAccessor)),
                    InitExpression = new CodeCastExpression(typeof(IParserAccessor), ctrlRefExpr)
                };
                statements.Add(statement);
                gotParserVariable = true;
            }
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("__parser"), "AddParsedSubObject", new CodeExpression[0]);
            expression.Parameters.Add(ctrlToAdd);
            CodeExpressionStatement statement2 = new CodeExpressionStatement(expression) {
                LinePragma = linePragma
            };
            statements.Add(statement2);
        }

        protected CodeMemberMethod BuildBuildMethod(ControlBuilder builder, bool fTemplate, bool fInTemplate, bool topLevelControlInTemplate, PropertyEntry pse, bool fControlSkin)
        {
            ServiceContainer serviceProvider = new ServiceContainer();
            serviceProvider.AddService(typeof(IFilterResolutionService), HttpCapabilitiesBase.EmptyHttpCapabilitiesBase);
            try
            {
                builder.SetServiceProvider(serviceProvider);
                builder.EnsureEntriesSorted();
            }
            finally
            {
                builder.SetServiceProvider(null);
            }
            string methodNameForBuilder = this.GetMethodNameForBuilder(buildMethodPrefix, builder);
            Type ctrlTypeForBuilder = this.GetCtrlTypeForBuilder(builder, fTemplate);
            bool fStandardControl = false;
            bool fControlFieldDeclared = false;
            CodeMemberMethod method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = methodNameForBuilder;
            method.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            base._sourceDataClass.Members.Add(method);
            ComplexPropertyEntry entry = pse as ComplexPropertyEntry;
            if (fTemplate || ((entry != null) && entry.ReadOnly))
            {
                if (builder is RootBuilder)
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(base._sourceDataClass.Name, "__ctrl"));
                }
                else
                {
                    method.Parameters.Add(new CodeParameterDeclarationExpression(ctrlTypeForBuilder, "__ctrl"));
                }
            }
            else
            {
                if (typeof(Control).IsAssignableFrom(builder.ControlType))
                {
                    fStandardControl = true;
                }
                if (builder.ControlType != null)
                {
                    if (fControlSkin)
                    {
                        if (fStandardControl)
                        {
                            method.ReturnType = new CodeTypeReference(typeof(Control));
                        }
                    }
                    else if (((PartialCachingAttribute) TypeDescriptor.GetAttributes(builder.ControlType)[typeof(PartialCachingAttribute)]) != null)
                    {
                        method.ReturnType = new CodeTypeReference(typeof(Control));
                    }
                    else
                    {
                        method.ReturnType = CodeDomUtility.BuildGlobalCodeTypeReference(builder.ControlType);
                    }
                }
                fControlFieldDeclared = true;
            }
            if (fControlSkin)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control).FullName, "ctrl"));
            }
            this.BuildBuildMethodInternal(builder, builder.ControlType, fInTemplate, topLevelControlInTemplate, pse, method.Statements, fStandardControl, fControlFieldDeclared, null, fControlSkin);
            return method;
        }

        private void BuildBuildMethodInternal(ControlBuilder builder, Type ctrlType, bool fInTemplate, bool topLevelControlInTemplate, PropertyEntry pse, CodeStatementCollection statements, bool fStandardControl, bool fControlFieldDeclared, string deviceFilter, bool fControlSkin)
        {
            CodeObjectCreateExpression expression;
            CodeExpressionStatement statement;
            CodeMethodInvokeExpression expression2;
            CodeExpression expression3;
            CodeLinePragma linePragma = base.CreateCodeLinePragma(builder);
            if (fControlSkin)
            {
                CodeCastExpression initExpression = new CodeCastExpression(builder.ControlType.FullName, new CodeArgumentReferenceExpression("ctrl"));
                statements.Add(new CodeVariableDeclarationStatement(builder.ControlType.FullName, "__ctrl", initExpression));
                expression3 = new CodeVariableReferenceExpression("__ctrl");
            }
            else if (!fControlFieldDeclared)
            {
                expression3 = new CodeArgumentReferenceExpression("__ctrl");
            }
            else
            {
                CodeTypeReference createType = CodeDomUtility.BuildGlobalCodeTypeReference(ctrlType);
                expression = new CodeObjectCreateExpression(createType, new CodeExpression[0]);
                ConstructorNeedsTagAttribute attribute = (ConstructorNeedsTagAttribute) TypeDescriptor.GetAttributes(ctrlType)[typeof(ConstructorNeedsTagAttribute)];
                if ((attribute != null) && attribute.NeedsTag)
                {
                    expression.Parameters.Add(new CodePrimitiveExpression(builder.TagName));
                }
                DataBoundLiteralControlBuilder builder2 = builder as DataBoundLiteralControlBuilder;
                if (builder2 != null)
                {
                    expression.Parameters.Add(new CodePrimitiveExpression(builder2.GetStaticLiteralsCount()));
                    expression.Parameters.Add(new CodePrimitiveExpression(builder2.GetDataBoundLiteralCount()));
                }
                statements.Add(new CodeVariableDeclarationStatement(createType, "__ctrl"));
                expression3 = new CodeVariableReferenceExpression("__ctrl");
                CodeAssignStatement statement2 = new CodeAssignStatement(expression3, expression) {
                    LinePragma = linePragma
                };
                statements.Add(statement2);
                if (!builder.IsGeneratedID)
                {
                    CodeFieldReferenceExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), builder.ID);
                    CodeAssignStatement statement3 = new CodeAssignStatement(left, expression3);
                    statements.Add(statement3);
                }
                if (topLevelControlInTemplate && !typeof(TemplateControl).IsAssignableFrom(ctrlType))
                {
                    statements.Add(this.BuildTemplatePropertyStatement(expression3));
                }
                if (fStandardControl)
                {
                    if (builder.SkinID != null)
                    {
                        CodeAssignStatement statement4 = new CodeAssignStatement {
                            Left = new CodePropertyReferenceExpression(expression3, "SkinID"),
                            Right = new CodePrimitiveExpression(builder.SkinID)
                        };
                        statements.Add(statement4);
                    }
                    if (ThemeableAttribute.IsTypeThemeable(ctrlType))
                    {
                        CodeMethodInvokeExpression expression6 = new CodeMethodInvokeExpression(expression3, applyStyleSheetMethodName, new CodeExpression[0]);
                        expression6.Parameters.Add(this.BuildPagePropertyReferenceExpression());
                        statements.Add(expression6);
                    }
                }
            }
            if (builder.TemplatePropertyEntries.Count > 0)
            {
                CodeStatementCollection nextStmts = statements;
                PropertyEntry previous = null;
                foreach (TemplatePropertyEntry entry2 in builder.TemplatePropertyEntries)
                {
                    CodeStatementCollection currentStmts = nextStmts;
                    this.HandleDeviceFilterConditional(ref previous, entry2, statements, ref currentStmts, out nextStmts);
                    string iD = entry2.Builder.ID;
                    CodeDelegateCreateExpression expression7 = new CodeDelegateCreateExpression {
                        DelegateType = new CodeTypeReference(typeof(BuildTemplateMethod)),
                        TargetObject = new CodeThisReferenceExpression(),
                        MethodName = buildMethodPrefix + iD
                    };
                    CodeAssignStatement statement5 = new CodeAssignStatement();
                    if (entry2.PropertyInfo != null)
                    {
                        statement5.Left = new CodePropertyReferenceExpression(expression3, entry2.Name);
                    }
                    else
                    {
                        statement5.Left = new CodeFieldReferenceExpression(expression3, entry2.Name);
                    }
                    if (entry2.BindableTemplate)
                    {
                        CodeExpression expression8;
                        if (entry2.Builder.HasTwoWayBoundProperties)
                        {
                            expression8 = new CodeDelegateCreateExpression();
                            ((CodeDelegateCreateExpression) expression8).DelegateType = new CodeTypeReference(typeof(ExtractTemplateValuesMethod));
                            ((CodeDelegateCreateExpression) expression8).TargetObject = new CodeThisReferenceExpression();
                            ((CodeDelegateCreateExpression) expression8).MethodName = extractTemplateValuesMethodPrefix + iD;
                        }
                        else
                        {
                            expression8 = new CodePrimitiveExpression(null);
                        }
                        expression = new CodeObjectCreateExpression(typeof(CompiledBindableTemplateBuilder), new CodeExpression[0]);
                        expression.Parameters.Add(expression7);
                        expression.Parameters.Add(expression8);
                    }
                    else
                    {
                        expression = new CodeObjectCreateExpression(typeof(CompiledTemplateBuilder), new CodeExpression[0]);
                        expression.Parameters.Add(expression7);
                    }
                    statement5.Right = expression;
                    statement5.LinePragma = base.CreateCodeLinePragma(entry2.Builder);
                    currentStmts.Add(statement5);
                }
            }
            if ((typeof(UserControl).IsAssignableFrom(ctrlType) && fControlFieldDeclared) && !fControlSkin)
            {
                expression2 = new CodeMethodInvokeExpression(expression3, "InitializeAsUserControl", new CodeExpression[0]);
                expression2.Parameters.Add(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pagePropertyName));
                statement = new CodeExpressionStatement(expression2) {
                    LinePragma = linePragma
                };
                statements.Add(statement);
            }
            if (builder.SimplePropertyEntries.Count > 0)
            {
                CodeStatementCollection statements5 = statements;
                PropertyEntry entry3 = null;
                foreach (SimplePropertyEntry entry4 in builder.SimplePropertyEntries)
                {
                    CodeStatementCollection statements4 = statements5;
                    this.HandleDeviceFilterConditional(ref entry3, entry4, statements, ref statements4, out statements5);
                    CodeStatement codeStatement = entry4.GetCodeStatement(this, expression3);
                    codeStatement.LinePragma = linePragma;
                    statements4.Add(codeStatement);
                }
            }
            if (typeof(Page).IsAssignableFrom(ctrlType) && !fControlSkin)
            {
                expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitializeCulture", new CodeExpression[0]);
                statement = new CodeExpressionStatement(expression2) {
                    LinePragma = linePragma
                };
                statements.Add(statement);
            }
            CodeMethodInvokeExpression expression9 = null;
            CodeConditionStatement statement7 = null;
            CodeStatementCollection falseStatements = statements;
            string propName = null;
            if (builder is ContentPlaceHolderBuilder)
            {
                string name = ((ContentPlaceHolderBuilder) builder).Name;
                propName = MasterPageControlBuilder.AutoTemplatePrefix + name;
                string fieldName = "__" + propName;
                Type bindingContainerType = builder.BindingContainerType;
                if (!typeof(INamingContainer).IsAssignableFrom(bindingContainerType))
                {
                    if (typeof(INamingContainer).IsAssignableFrom(this.Parser.BaseType))
                    {
                        bindingContainerType = this.Parser.BaseType;
                    }
                    else
                    {
                        bindingContainerType = typeof(Control);
                    }
                }
                CodeAttributeDeclarationCollection attrDeclarations = new CodeAttributeDeclarationCollection();
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("TemplateContainer", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeTypeOfExpression(bindingContainerType)) });
                attrDeclarations.Add(declaration);
                if (!fInTemplate)
                {
                    CodeAttributeDeclaration declaration2 = new CodeAttributeDeclaration("TemplateInstanceAttribute", new CodeAttributeArgument[] { new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(TemplateInstance)), "Single")) });
                    attrDeclarations.Add(declaration2);
                }
                base.BuildFieldAndAccessorProperty(propName, fieldName, typeof(ITemplate), false, attrDeclarations);
                CodeExpression expression10 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
                if (builder is ContentPlaceHolderBuilder)
                {
                    CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ContentTemplates");
                    CodeAssignStatement statement8 = new CodeAssignStatement {
                        Left = expression10,
                        Right = new CodeCastExpression(typeof(ITemplate), new CodeIndexerExpression(targetObject, new CodeExpression[] { new CodePrimitiveExpression(name) }))
                    };
                    CodeConditionStatement statement9 = new CodeConditionStatement();
                    CodeBinaryOperatorExpression expression12 = new CodeBinaryOperatorExpression(targetObject, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                    CodeMethodInvokeExpression expression13 = new CodeMethodInvokeExpression(targetObject, "Remove", new CodeExpression[0]);
                    expression13.Parameters.Add(new CodePrimitiveExpression(name));
                    statement9.Condition = expression12;
                    statement9.TrueStatements.Add(statement8);
                    statements.Add(statement9);
                }
                if (MultiTargetingUtil.IsTargetFramework40OrAbove)
                {
                    expression9 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateInContentPlaceHolder", new CodeExpression[0]);
                    expression9.Parameters.Add(expression3);
                    expression9.Parameters.Add(expression10);
                }
                else
                {
                    expression9 = new CodeMethodInvokeExpression(expression10, "InstantiateIn", new CodeExpression[0]);
                    expression9.Parameters.Add(expression3);
                }
                statement7 = new CodeConditionStatement {
                    Condition = new CodeBinaryOperatorExpression(expression10, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null))
                };
                statement7.TrueStatements.Add(new CodeExpressionStatement(expression9));
                falseStatements = statement7.FalseStatements;
                statements.Add(statement7);
            }
            ICollection contentBuilderEntries = null;
            if (builder is FileLevelPageControlBuilder)
            {
                contentBuilderEntries = ((FileLevelPageControlBuilder) builder).ContentBuilderEntries;
                if (contentBuilderEntries != null)
                {
                    CodeStatementCollection statements8 = statements;
                    PropertyEntry entry5 = null;
                    foreach (TemplatePropertyEntry entry6 in contentBuilderEntries)
                    {
                        ContentBuilderInternal internal2 = (ContentBuilderInternal) entry6.Builder;
                        CodeStatementCollection statements7 = statements8;
                        this.HandleDeviceFilterConditional(ref entry5, entry6, statements, ref statements7, out statements8);
                        string str5 = internal2.ID;
                        string contentPlaceHolder = internal2.ContentPlaceHolder;
                        CodeDelegateCreateExpression expression14 = new CodeDelegateCreateExpression {
                            DelegateType = new CodeTypeReference(typeof(BuildTemplateMethod)),
                            TargetObject = new CodeThisReferenceExpression(),
                            MethodName = buildMethodPrefix + str5
                        };
                        CodeObjectCreateExpression expression15 = new CodeObjectCreateExpression(typeof(CompiledTemplateBuilder), new CodeExpression[0]);
                        expression15.Parameters.Add(expression14);
                        CodeMethodInvokeExpression expression16 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "AddContentTemplate", new CodeExpression[0]);
                        expression16.Parameters.Add(new CodePrimitiveExpression(contentPlaceHolder));
                        expression16.Parameters.Add(expression15);
                        CodeExpressionStatement statement10 = new CodeExpressionStatement(expression16) {
                            LinePragma = base.CreateCodeLinePragma(internal2)
                        };
                        statements7.Add(statement10);
                    }
                }
            }
            if (builder is DataBoundLiteralControlBuilder)
            {
                int num = -1;
                foreach (object obj2 in builder.SubBuilders)
                {
                    num++;
                    if ((obj2 != null) && ((num % 2) != 1))
                    {
                        string str7 = (string) obj2;
                        expression2 = new CodeMethodInvokeExpression(expression3, "SetStaticString", new CodeExpression[0]);
                        expression2.Parameters.Add(new CodePrimitiveExpression(num / 2));
                        expression2.Parameters.Add(new CodePrimitiveExpression(str7));
                        statements.Add(new CodeExpressionStatement(expression2));
                    }
                }
            }
            else if (builder.SubBuilders != null)
            {
                bool gotParserVariable = false;
                int num2 = 1;
                foreach (object obj3 in builder.SubBuilders)
                {
                    if (((obj3 is ControlBuilder) && !(obj3 is CodeBlockBuilder)) && !(obj3 is ContentBuilderInternal))
                    {
                        ControlBuilder builder3 = (ControlBuilder) obj3;
                        if (fControlSkin)
                        {
                            throw new HttpParseException(System.Web.SR.GetString("ControlSkin_cannot_contain_controls"), null, builder.VirtualPath, null, builder.Line);
                        }
                        PartialCachingAttribute attribute2 = (PartialCachingAttribute) TypeDescriptor.GetAttributes(builder3.ControlType)[typeof(PartialCachingAttribute)];
                        expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), buildMethodPrefix + builder3.ID, new CodeExpression[0]);
                        statement = new CodeExpressionStatement(expression2);
                        if (attribute2 == null)
                        {
                            int num6 = num2++;
                            string variableName = "__ctrl" + num6.ToString(CultureInfo.InvariantCulture);
                            CodeVariableReferenceExpression expression17 = new CodeVariableReferenceExpression(variableName);
                            CodeTypeReference type = CodeDomUtility.BuildGlobalCodeTypeReference(builder3.ControlType);
                            falseStatements.Add(new CodeVariableDeclarationStatement(type, variableName));
                            CodeAssignStatement statement11 = new CodeAssignStatement(expression17, expression2) {
                                LinePragma = linePragma
                            };
                            falseStatements.Add(statement11);
                            BuildAddParsedSubObjectStatement(falseStatements, expression17, linePragma, expression3, ref gotParserVariable);
                        }
                        else
                        {
                            string providerName = null;
                            bool flag2 = MultiTargetingUtil.IsTargetFramework40OrAbove;
                            if (flag2)
                            {
                                providerName = attribute2.ProviderName;
                                if (providerName == "AspNetInternalProvider")
                                {
                                    providerName = null;
                                }
                            }
                            CodeMethodInvokeExpression expression18 = new CodeMethodInvokeExpression {
                                Method = { TargetObject = new CodeTypeReferenceExpression(typeof(StaticPartialCachingControl)), MethodName = "BuildCachedControl" }
                            };
                            expression18.Parameters.Add(expression3);
                            expression18.Parameters.Add(new CodePrimitiveExpression(builder3.ID));
                            if (attribute2.Shared)
                            {
                                expression18.Parameters.Add(new CodePrimitiveExpression(builder3.ControlType.GetHashCode().ToString(CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                expression18.Parameters.Add(new CodePrimitiveExpression(Guid.NewGuid().ToString()));
                            }
                            expression18.Parameters.Add(new CodePrimitiveExpression(attribute2.Duration));
                            expression18.Parameters.Add(new CodePrimitiveExpression(attribute2.VaryByParams));
                            expression18.Parameters.Add(new CodePrimitiveExpression(attribute2.VaryByControls));
                            expression18.Parameters.Add(new CodePrimitiveExpression(attribute2.VaryByCustom));
                            expression18.Parameters.Add(new CodePrimitiveExpression(attribute2.SqlDependency));
                            CodeDelegateCreateExpression expression19 = new CodeDelegateCreateExpression {
                                DelegateType = new CodeTypeReference(typeof(BuildMethod)),
                                TargetObject = new CodeThisReferenceExpression(),
                                MethodName = buildMethodPrefix + builder3.ID
                            };
                            expression18.Parameters.Add(expression19);
                            if (flag2)
                            {
                                expression18.Parameters.Add(new CodePrimitiveExpression(providerName));
                            }
                            falseStatements.Add(new CodeExpressionStatement(expression18));
                        }
                    }
                    else if (((obj3 is string) && !builder.HasAspCode) && (!fControlSkin || !builder.AllowWhitespaceLiterals()))
                    {
                        CodeExpression expression20;
                        string s = (string) obj3;
                        if (!this.UseResourceLiteralString(s))
                        {
                            expression = new CodeObjectCreateExpression(typeof(LiteralControl), new CodeExpression[0]);
                            expression.Parameters.Add(new CodePrimitiveExpression(s));
                            expression20 = expression;
                        }
                        else
                        {
                            int num3;
                            int num4;
                            bool flag3;
                            base._stringResourceBuilder.AddString(s, out num3, out num4, out flag3);
                            expression2 = new CodeMethodInvokeExpression {
                                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "CreateResourceBasedLiteralControl" }
                            };
                            expression2.Parameters.Add(new CodePrimitiveExpression(num3));
                            expression2.Parameters.Add(new CodePrimitiveExpression(num4));
                            expression2.Parameters.Add(new CodePrimitiveExpression(flag3));
                            expression20 = expression2;
                        }
                        BuildAddParsedSubObjectStatement(falseStatements, expression20, linePragma, expression3, ref gotParserVariable);
                    }
                }
            }
            if (builder.ComplexPropertyEntries.Count > 0)
            {
                CodeStatementCollection statements10 = statements;
                PropertyEntry entry7 = null;
                int num5 = 1;
                string str11 = null;
                foreach (ComplexPropertyEntry entry8 in builder.ComplexPropertyEntries)
                {
                    CodeStatementCollection statements9 = statements10;
                    this.HandleDeviceFilterConditional(ref entry7, entry8, statements, ref statements9, out statements10);
                    if (entry8.Builder is StringPropertyBuilder)
                    {
                        CodeExpression right = null;
                        CodeExpression expression21 = new CodePropertyReferenceExpression(expression3, entry8.Name);
                        right = this.BuildStringPropertyExpression(entry8);
                        CodeAssignStatement statement12 = new CodeAssignStatement(expression21, right) {
                            LinePragma = linePragma
                        };
                        statements9.Add(statement12);
                    }
                    else if (entry8.ReadOnly)
                    {
                        if ((fControlSkin && (entry8.Builder != null)) && ((entry8.Builder is CollectionBuilder) && (entry8.Builder.ComplexPropertyEntries.Count > 0)))
                        {
                            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
                            if (entry8.Type.GetMethod("Clear", bindingAttr) != null)
                            {
                                CodeMethodReferenceExpression expression23 = new CodeMethodReferenceExpression {
                                    MethodName = "Clear",
                                    TargetObject = new CodePropertyReferenceExpression(expression3, entry8.Name)
                                };
                                CodeMethodInvokeExpression expression24 = new CodeMethodInvokeExpression {
                                    Method = expression23
                                };
                                statements9.Add(expression24);
                            }
                        }
                        expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), buildMethodPrefix + entry8.Builder.ID, new CodeExpression[0]);
                        expression2.Parameters.Add(new CodePropertyReferenceExpression(expression3, entry8.Name));
                        statement = new CodeExpressionStatement(expression2) {
                            LinePragma = linePragma
                        };
                        statements9.Add(statement);
                    }
                    else
                    {
                        str11 = "__ctrl" + num5++.ToString(CultureInfo.InvariantCulture);
                        CodeTypeReference reference3 = CodeDomUtility.BuildGlobalCodeTypeReference(entry8.Builder.ControlType);
                        statements9.Add(new CodeVariableDeclarationStatement(reference3, str11));
                        CodeVariableReferenceExpression expression25 = new CodeVariableReferenceExpression(str11);
                        expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), buildMethodPrefix + entry8.Builder.ID, new CodeExpression[0]);
                        statement = new CodeExpressionStatement(expression2);
                        CodeAssignStatement statement13 = new CodeAssignStatement(expression25, expression2) {
                            LinePragma = linePragma
                        };
                        statements9.Add(statement13);
                        if (entry8.IsCollectionItem)
                        {
                            expression2 = new CodeMethodInvokeExpression(expression3, "Add", new CodeExpression[0]);
                            statement = new CodeExpressionStatement(expression2) {
                                LinePragma = linePragma
                            };
                            statements9.Add(statement);
                            expression2.Parameters.Add(expression25);
                        }
                        else
                        {
                            CodeAssignStatement statement14 = new CodeAssignStatement {
                                Left = new CodePropertyReferenceExpression(expression3, entry8.Name),
                                Right = expression25,
                                LinePragma = linePragma
                            };
                            statements9.Add(statement14);
                        }
                    }
                }
            }
            if (builder.BoundPropertyEntries.Count > 0)
            {
                bool flag4 = builder is BindableTemplateBuilder;
                bool flag5 = false;
                CodeStatementCollection methodStatements = statements;
                CodeStatementCollection statements13 = statements;
                PropertyEntry entry9 = null;
                bool hasTempObject = false;
                foreach (BoundPropertyEntry entry10 in builder.BoundPropertyEntries)
                {
                    if (!entry10.TwoWayBound || (!flag4 && !entry10.ReadOnlyProperty))
                    {
                        if (entry10.IsDataBindingEntry)
                        {
                            flag5 = true;
                        }
                        else
                        {
                            CodeStatementCollection statements11 = statements13;
                            this.HandleDeviceFilterConditional(ref entry9, entry10, statements, ref statements11, out statements13);
                            entry10.ExpressionBuilder.BuildExpression(entry10, builder, expression3, methodStatements, statements11, null, ref hasTempObject);
                        }
                    }
                }
                if (flag5)
                {
                    EventInfo info = DataBindingExpressionBuilder.Event;
                    CodeDelegateCreateExpression listener = new CodeDelegateCreateExpression();
                    CodeAttachEventStatement statement15 = new CodeAttachEventStatement(expression3, info.Name, listener) {
                        LinePragma = linePragma
                    };
                    listener.DelegateType = new CodeTypeReference(typeof(EventHandler));
                    listener.TargetObject = new CodeThisReferenceExpression();
                    listener.MethodName = this.GetExpressionBuilderMethodName(info.Name, builder);
                    statements.Add(statement15);
                }
            }
            if (builder is DataBoundLiteralControlBuilder)
            {
                CodeDelegateCreateExpression expression27 = new CodeDelegateCreateExpression();
                CodeAttachEventStatement statement16 = new CodeAttachEventStatement(expression3, "DataBinding", expression27) {
                    LinePragma = linePragma
                };
                expression27.DelegateType = new CodeTypeReference(typeof(EventHandler));
                expression27.TargetObject = new CodeThisReferenceExpression();
                expression27.MethodName = this.BindingMethodName(builder);
                statements.Add(statement16);
            }
            if (builder.HasAspCode && !fControlSkin)
            {
                CodeDelegateCreateExpression expression28 = new CodeDelegateCreateExpression {
                    DelegateType = new CodeTypeReference(typeof(RenderMethod)),
                    TargetObject = new CodeThisReferenceExpression(),
                    MethodName = "__Render" + builder.ID
                };
                expression2 = new CodeMethodInvokeExpression(expression3, "SetRenderMethodDelegate", new CodeExpression[0]);
                expression2.Parameters.Add(expression28);
                statement = new CodeExpressionStatement(expression2);
                if (builder is ContentPlaceHolderBuilder)
                {
                    string str12 = ((ContentPlaceHolderBuilder) builder).Name;
                    propName = MasterPageControlBuilder.AutoTemplatePrefix + str12;
                    string str13 = "__" + propName;
                    CodeExpression expression29 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), str13);
                    statement7 = new CodeConditionStatement {
                        Condition = new CodeBinaryOperatorExpression(expression29, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
                    };
                    statement7.TrueStatements.Add(statement);
                    statements.Add(statement7);
                }
                else
                {
                    statements.Add(statement);
                }
            }
            if (builder.EventEntries.Count > 0)
            {
                foreach (EventEntry entry11 in builder.EventEntries)
                {
                    CodeDelegateCreateExpression expression30 = new CodeDelegateCreateExpression {
                        DelegateType = new CodeTypeReference(entry11.HandlerType),
                        TargetObject = new CodeThisReferenceExpression(),
                        MethodName = entry11.HandlerMethodName
                    };
                    if (this.Parser.HasCodeBehind)
                    {
                        CodeRemoveEventStatement statement17 = new CodeRemoveEventStatement(expression3, entry11.Name, expression30) {
                            LinePragma = linePragma
                        };
                        statements.Add(statement17);
                    }
                    CodeAttachEventStatement statement18 = new CodeAttachEventStatement(expression3, entry11.Name, expression30) {
                        LinePragma = linePragma
                    };
                    statements.Add(statement18);
                }
            }
            if (fControlFieldDeclared)
            {
                statements.Add(new CodeMethodReturnStatement(expression3));
            }
        }

        protected void BuildExtractMethod(ControlBuilder builder)
        {
            BindableTemplateBuilder builder2 = builder as BindableTemplateBuilder;
            if ((builder2 != null) && builder2.HasTwoWayBoundProperties)
            {
                string str = this.ExtractMethodName(builder);
                CodeLinePragma linePragma = base.CreateCodeLinePragma(builder);
                CodeMemberMethod method = new CodeMemberMethod();
                base.AddDebuggerNonUserCodeAttribute(method);
                method.Name = str;
                method.Attributes &= ~MemberAttributes.AccessMask;
                method.Attributes |= MemberAttributes.Public;
                method.ReturnType = new CodeTypeReference(typeof(IOrderedDictionary));
                base._sourceDataClass.Members.Add(method);
                CodeStatementCollection topLevelStatements = method.Statements;
                CodeStatementCollection statements = new CodeStatementCollection();
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control), "__container"));
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(typeof(OrderedDictionary), "__table");
                topLevelStatements.Add(statement);
                CodeObjectCreateExpression right = new CodeObjectCreateExpression(typeof(OrderedDictionary), new CodeExpression[0]);
                CodeAssignStatement statement2 = new CodeAssignStatement(new CodeVariableReferenceExpression("__table"), right) {
                    LinePragma = linePragma
                };
                statements.Add(statement2);
                this.BuildExtractStatementsRecursive(builder2.SubBuilders, statements, topLevelStatements, linePragma, "__table", "__container");
                CodeMethodReturnStatement statement3 = new CodeMethodReturnStatement(new CodeVariableReferenceExpression("__table"));
                statements.Add(statement3);
                method.Statements.AddRange(statements);
            }
        }

        private void BuildExtractStatementsRecursive(ArrayList subBuilders, CodeStatementCollection statements, CodeStatementCollection topLevelStatements, CodeLinePragma linePragma, string tableVarName, string containerVarName)
        {
            foreach (object obj2 in subBuilders)
            {
                ControlBuilder builder = obj2 as ControlBuilder;
                if (builder != null)
                {
                    CodeStatementCollection currentStmts = null;
                    CodeStatementCollection nextStmts = statements;
                    PropertyEntry previous = null;
                    string strA = null;
                    bool flag = true;
                    foreach (BoundPropertyEntry entry2 in builder.BoundPropertyEntries)
                    {
                        if (entry2.TwoWayBound)
                        {
                            if (string.Compare(strA, entry2.ControlID, StringComparison.Ordinal) != 0)
                            {
                                previous = null;
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                            strA = entry2.ControlID;
                            currentStmts = nextStmts;
                            this.HandleDeviceFilterConditional(ref previous, entry2, statements, ref currentStmts, out nextStmts);
                            if (flag)
                            {
                                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(entry2.ControlType, entry2.ControlID);
                                topLevelStatements.Add(statement);
                                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression(containerVarName), "FindControl", new CodeExpression[0]);
                                string controlID = entry2.ControlID;
                                expression.Parameters.Add(new CodePrimitiveExpression(controlID));
                                CodeCastExpression right = new CodeCastExpression(entry2.ControlType, expression);
                                CodeAssignStatement statement2 = new CodeAssignStatement(new CodeVariableReferenceExpression(entry2.ControlID), right) {
                                    LinePragma = linePragma
                                };
                                topLevelStatements.Add(statement2);
                            }
                            CodeConditionStatement statement3 = new CodeConditionStatement();
                            CodeBinaryOperatorExpression expression3 = new CodeBinaryOperatorExpression {
                                Operator = CodeBinaryOperatorType.IdentityInequality,
                                Left = new CodeVariableReferenceExpression(entry2.ControlID),
                                Right = new CodePrimitiveExpression(null)
                            };
                            statement3.Condition = expression3;
                            string fieldName = entry2.FieldName;
                            CodeIndexerExpression left = new CodeIndexerExpression(new CodeVariableReferenceExpression(tableVarName), new CodeExpression[] { new CodePrimitiveExpression(fieldName) });
                            CodeExpression expression5 = CodeDomUtility.BuildPropertyReferenceExpression(new CodeVariableReferenceExpression(entry2.ControlID), entry2.Name);
                            if (base._usingVJSCompiler)
                            {
                                expression5 = CodeDomUtility.BuildJSharpCastExpression(entry2.Type, expression5);
                            }
                            CodeAssignStatement statement4 = new CodeAssignStatement(left, expression5);
                            statement3.TrueStatements.Add(statement4);
                            statement3.LinePragma = linePragma;
                            currentStmts.Add(statement3);
                        }
                    }
                    if (builder.SubBuilders.Count > 0)
                    {
                        this.BuildExtractStatementsRecursive(builder.SubBuilders, statements, topLevelStatements, linePragma, tableVarName, containerVarName);
                    }
                    ArrayList list = new ArrayList();
                    this.AddEntryBuildersToList(builder.ComplexPropertyEntries, list);
                    this.AddEntryBuildersToList(builder.TemplatePropertyEntries, list);
                    if (list.Count > 0)
                    {
                        this.BuildExtractStatementsRecursive(list, statements, topLevelStatements, linePragma, tableVarName, containerVarName);
                    }
                }
            }
        }

        private void BuildFieldDeclaration(ControlBuilder builder)
        {
            if (!(builder is ContentBuilderInternal))
            {
                CodeMemberField field;
                bool flag = false;
                if (this.Parser.BaseType != null)
                {
                    Type nonPrivateFieldType = System.Web.UI.Util.GetNonPrivateFieldType(this.Parser.BaseType, builder.ID);
                    if (nonPrivateFieldType == null)
                    {
                        nonPrivateFieldType = System.Web.UI.Util.GetNonPrivatePropertyType(this.Parser.BaseType, builder.ID);
                    }
                    if (nonPrivateFieldType != null)
                    {
                        if (nonPrivateFieldType.IsAssignableFrom(builder.ControlType))
                        {
                            return;
                        }
                        if (typeof(Control).IsAssignableFrom(nonPrivateFieldType))
                        {
                            throw new HttpParseException(System.Web.SR.GetString("Base_class_field_with_type_different_from_type_of_control", new object[] { builder.ID, nonPrivateFieldType.FullName, builder.ControlType.FullName }), null, builder.VirtualPath, null, builder.Line);
                        }
                        flag = true;
                    }
                }
                field = new CodeMemberField(CodeDomUtility.BuildGlobalCodeTypeReference(builder.DeclareType), builder.ID) {
                    Attributes = field.Attributes & ~MemberAttributes.AccessMask
                };
                if (flag)
                {
                    field.Attributes |= MemberAttributes.New;
                }
                field.LinePragma = base.CreateCodeLinePragma(builder);
                field.Attributes |= MemberAttributes.Family;
                if (typeof(Control).IsAssignableFrom(builder.DeclareType))
                {
                    field.UserData["WithEvents"] = true;
                }
                base._intermediateClass.Members.Add(field);
            }
        }

        internal virtual CodeExpression BuildPagePropertyReferenceExpression()
        {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pagePropertyName);
        }

        protected CodeMemberMethod BuildPropertyBindingMethod(ControlBuilder builder, bool fControlSkin)
        {
            if (builder is DataBoundLiteralControlBuilder)
            {
                CodeMemberMethod method;
                string str = this.BindingMethodName(builder);
                CodeLinePragma pragma = base.CreateCodeLinePragma(builder);
                method = new CodeMemberMethod {
                    Name = str,
                    Attributes = method.Attributes & ~MemberAttributes.AccessMask,
                    Attributes = method.Attributes | MemberAttributes.Public
                };
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EventArgs), "e"));
                CodeStatementCollection statements = new CodeStatementCollection();
                CodeStatementCollection statements2 = new CodeStatementCollection();
                CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement(builder.ControlType, "target");
                Type bindingContainerType = builder.BindingContainerType;
                CodeVariableDeclarationStatement statement2 = new CodeVariableDeclarationStatement(bindingContainerType, "Container");
                statements.Add(statement2);
                statements.Add(statement);
                CodeAssignStatement statement3 = new CodeAssignStatement(new CodeVariableReferenceExpression(statement.Name), new CodeCastExpression(builder.ControlType, new CodeArgumentReferenceExpression("sender"))) {
                    LinePragma = pragma
                };
                statements2.Add(statement3);
                CodeAssignStatement statement4 = new CodeAssignStatement(new CodeVariableReferenceExpression(statement2.Name), new CodeCastExpression(bindingContainerType, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("target"), "BindingContainer"))) {
                    LinePragma = pragma
                };
                statements2.Add(statement4);
                bool flag = false;
                int num = -1;
                foreach (object obj2 in builder.SubBuilders)
                {
                    num++;
                    if ((obj2 != null) && ((num % 2) != 0))
                    {
                        CodeBlockBuilder builder2 = (CodeBlockBuilder) obj2;
                        if (base._designerMode)
                        {
                            if (!flag)
                            {
                                flag = true;
                                statements.Add(new CodeVariableDeclarationStatement(typeof(object), "__o"));
                            }
                            CodeStatement statement5 = new CodeAssignStatement(new CodeVariableReferenceExpression("__o"), new CodeSnippetExpression(builder2.Content)) {
                                LinePragma = base.CreateCodeLinePragma(builder2)
                            };
                            statements2.Add(statement5);
                        }
                        else
                        {
                            CodeExpression expression = CodeDomUtility.GenerateConvertToString(new CodeSnippetExpression(builder2.Content.Trim()));
                            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("target"), "SetDataBoundString", new CodeExpression[0]);
                            expression2.Parameters.Add(new CodePrimitiveExpression(num / 2));
                            expression2.Parameters.Add(expression);
                            CodeStatement statement6 = new CodeExpressionStatement(expression2) {
                                LinePragma = base.CreateCodeLinePragma(builder2)
                            };
                            statements2.Add(statement6);
                        }
                    }
                }
                foreach (CodeStatement statement7 in statements)
                {
                    method.Statements.Add(statement7);
                }
                foreach (CodeStatement statement8 in statements2)
                {
                    method.Statements.Add(statement8);
                }
                base._sourceDataClass.Members.Add(method);
                return method;
            }
            EventInfo info = DataBindingExpressionBuilder.Event;
            CodeLinePragma linePragma = base.CreateCodeLinePragma(builder);
            CodeMemberMethod member = null;
            CodeStatementCollection methodStatements = null;
            CodeStatementCollection statements4 = null;
            CodeStatementCollection nextStmts = null;
            PropertyEntry previous = null;
            bool flag2 = builder is BindableTemplateBuilder;
            bool flag3 = true;
            bool hasTempObject = false;
            foreach (BoundPropertyEntry entry2 in builder.BoundPropertyEntries)
            {
                if ((!entry2.TwoWayBound || (!flag2 && !entry2.ReadOnlyProperty)) && entry2.IsDataBindingEntry)
                {
                    if (flag3)
                    {
                        flag3 = false;
                        member = new CodeMemberMethod();
                        methodStatements = new CodeStatementCollection();
                        statements4 = new CodeStatementCollection();
                        string expressionBuilderMethodName = this.GetExpressionBuilderMethodName(info.Name, builder);
                        member.Name = expressionBuilderMethodName;
                        member.Attributes &= ~MemberAttributes.AccessMask;
                        member.Attributes |= MemberAttributes.Public;
                        if (base._designerMode)
                        {
                            base.ApplyEditorBrowsableCustomAttribute(member);
                        }
                        foreach (ParameterInfo info3 in info.EventHandlerType.GetMethod("Invoke").GetParameters())
                        {
                            member.Parameters.Add(new CodeParameterDeclarationExpression(info3.ParameterType, info3.Name));
                        }
                        nextStmts = statements4;
                        DataBindingExpressionBuilder.BuildExpressionSetup(builder, methodStatements, statements4);
                        base._sourceDataClass.Members.Add(member);
                    }
                    CodeStatementCollection currentStmts = nextStmts;
                    this.HandleDeviceFilterConditional(ref previous, entry2, statements4, ref currentStmts, out nextStmts);
                    if (entry2.TwoWayBound)
                    {
                        DataBindingExpressionBuilder.BuildEvalExpression(entry2.FieldName, entry2.FormatString, entry2.Name, entry2.Type, builder, methodStatements, currentStmts, linePragma, ref hasTempObject);
                    }
                    else
                    {
                        DataBindingExpressionBuilder.BuildExpressionStatic(entry2, builder, null, methodStatements, currentStmts, linePragma, ref hasTempObject);
                    }
                }
            }
            if (methodStatements != null)
            {
                foreach (CodeStatement statement9 in methodStatements)
                {
                    member.Statements.Add(statement9);
                }
            }
            if (statements4 != null)
            {
                foreach (CodeStatement statement10 in statements4)
                {
                    member.Statements.Add(statement10);
                }
            }
            return member;
        }

        internal void BuildRenderMethod(ControlBuilder builder, bool fTemplate)
        {
            CodeMemberMethod member = new CodeMemberMethod {
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                Name = "__Render" + builder.ID
            };
            if (base._designerMode)
            {
                base.ApplyEditorBrowsableCustomAttribute(member);
            }
            member.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HtmlTextWriter), "__w"));
            member.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control), "parameterContainer"));
            base._sourceDataClass.Members.Add(member);
            bool flag = false;
            if (builder.SubBuilders != null)
            {
                IEnumerator enumerator = builder.SubBuilders.GetEnumerator();
                int num = 0;
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    object current = enumerator.Current;
                    CodeLinePragma pragma = null;
                    if (current is ControlBuilder)
                    {
                        pragma = base.CreateCodeLinePragma((ControlBuilder) current);
                    }
                    if (current is string)
                    {
                        if (!base._designerMode)
                        {
                            this.AddOutputWriteStringStatement(member.Statements, (string) current);
                        }
                    }
                    else if (current is CodeBlockBuilder)
                    {
                        CodeBlockBuilder builder2 = (CodeBlockBuilder) current;
                        if ((builder2.BlockType == CodeBlockType.Expression) || (builder2.BlockType == CodeBlockType.EncodedExpression))
                        {
                            string content = builder2.Content;
                            if (base._designerMode)
                            {
                                if (!flag)
                                {
                                    flag = true;
                                    member.Statements.Add(new CodeVariableDeclarationStatement(typeof(object), "__o"));
                                }
                                CodeStatement statement = new CodeAssignStatement(new CodeVariableReferenceExpression("__o"), new CodeSnippetExpression(content)) {
                                    LinePragma = pragma
                                };
                                member.Statements.Add(statement);
                            }
                            else
                            {
                                CodeStatement outputWriteStatement = this.GetOutputWriteStatement(new CodeSnippetExpression(content), builder2.BlockType == CodeBlockType.EncodedExpression);
                                TextWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                                base._codeDomProvider.GenerateCodeFromStatement(outputWriteStatement, writer, null);
                                CodeSnippetStatement statement3 = new CodeSnippetStatement(writer.ToString().PadLeft((builder2.Column + content.Length) + 3)) {
                                    LinePragma = pragma
                                };
                                member.Statements.Add(statement3);
                            }
                        }
                        else
                        {
                            string str3 = builder2.Content;
                            CodeSnippetStatement statement4 = new CodeSnippetStatement(str3.PadLeft((str3.Length + builder2.Column) - 1)) {
                                LinePragma = pragma
                            };
                            member.Statements.Add(statement4);
                        }
                    }
                    else if ((current is ControlBuilder) && !base._designerMode)
                    {
                        CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression();
                        CodeExpressionStatement statement5 = new CodeExpressionStatement(expression);
                        expression.Method.TargetObject = new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("parameterContainer"), "Controls"), new CodeExpression[] { new CodePrimitiveExpression(num++) });
                        expression.Method.MethodName = "RenderControl";
                        expression.Parameters.Add(new CodeArgumentReferenceExpression("__w"));
                        member.Statements.Add(statement5);
                    }
                }
            }
        }

        protected virtual void BuildSourceDataTreeFromBuilder(ControlBuilder builder, bool fInTemplate, bool topLevelControlInTemplate, PropertyEntry pse)
        {
            if (!(builder is CodeBlockBuilder))
            {
                bool fTemplate = builder is TemplateBuilder;
                if ((builder.ID == null) || fInTemplate)
                {
                    this._controlCount++;
                    builder.ID = "__control" + this._controlCount.ToString(NumberFormatInfo.InvariantInfo);
                    builder.IsGeneratedID = true;
                }
                if (builder.SubBuilders != null)
                {
                    foreach (object obj2 in builder.SubBuilders)
                    {
                        if (obj2 is ControlBuilder)
                        {
                            bool flag2 = (fTemplate && typeof(Control).IsAssignableFrom(((ControlBuilder) obj2).ControlType)) && !(builder is RootBuilder);
                            this.BuildSourceDataTreeFromBuilder((ControlBuilder) obj2, fInTemplate, flag2, null);
                        }
                    }
                }
                foreach (TemplatePropertyEntry entry in builder.TemplatePropertyEntries)
                {
                    bool isMultiple = true;
                    if (entry.PropertyInfo != null)
                    {
                        isMultiple = entry.IsMultiple;
                    }
                    this.BuildSourceDataTreeFromBuilder(entry.Builder, isMultiple, false, entry);
                }
                foreach (ComplexPropertyEntry entry2 in builder.ComplexPropertyEntries)
                {
                    if (!(entry2.Builder is StringPropertyBuilder))
                    {
                        this.BuildSourceDataTreeFromBuilder(entry2.Builder, fInTemplate, false, entry2);
                    }
                }
                if (!builder.IsGeneratedID)
                {
                    this.BuildFieldDeclaration(builder);
                }
                CodeMemberMethod buildMethod = null;
                CodeMemberMethod dataBindingMethod = null;
                if (base._sourceDataClass != null)
                {
                    if (!base._designerMode)
                    {
                        buildMethod = this.BuildBuildMethod(builder, fTemplate, fInTemplate, topLevelControlInTemplate, pse, false);
                    }
                    if (builder.HasAspCode)
                    {
                        this.BuildRenderMethod(builder, fTemplate);
                    }
                    this.BuildExtractMethod(builder);
                    dataBindingMethod = this.BuildPropertyBindingMethod(builder, false);
                }
                builder.ProcessGeneratedCode(base._codeCompileUnit, base._intermediateClass, base._sourceDataClass, buildMethod, dataBindingMethod);
            }
        }

        internal virtual CodeExpression BuildStringPropertyExpression(PropertyEntry pse)
        {
            string str = string.Empty;
            if (pse is SimplePropertyEntry)
            {
                str = (string) ((SimplePropertyEntry) pse).Value;
            }
            else
            {
                ComplexPropertyEntry entry = (ComplexPropertyEntry) pse;
                str = (string) ((StringPropertyBuilder) entry.Builder).BuildObject();
            }
            return CodeDomUtility.GenerateExpressionForValue(pse.PropertyInfo, str, typeof(string));
        }

        protected virtual CodeAssignStatement BuildTemplatePropertyStatement(CodeExpression ctrlRefExpr)
        {
            return new CodeAssignStatement { Left = new CodePropertyReferenceExpression(ctrlRefExpr, "TemplateControl"), Right = new CodeThisReferenceExpression() };
        }

        private string ExtractMethodName(ControlBuilder builder)
        {
            return (extractTemplateValuesMethodPrefix + builder.ID);
        }

        private Type GetCtrlTypeForBuilder(ControlBuilder builder, bool fTemplate)
        {
            if ((!(builder is RootBuilder) || (builder.ControlType == null)) && fTemplate)
            {
                return typeof(Control);
            }
            return builder.ControlType;
        }

        private string GetExpressionBuilderMethodName(string eventName, ControlBuilder builder)
        {
            return ("__" + eventName + builder.ID);
        }

        protected string GetMethodNameForBuilder(string prefix, ControlBuilder builder)
        {
            if (builder is RootBuilder)
            {
                return (prefix + "Tree");
            }
            return (prefix + builder.ID);
        }

        private CodeStatement GetOutputWriteStatement(CodeExpression expr, bool encode)
        {
            if (encode)
            {
                expr = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(HttpUtility)), "HtmlEncode"), new CodeExpression[] { expr });
            }
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression();
            CodeExpressionStatement statement = new CodeExpressionStatement(expression);
            expression.Method.TargetObject = new CodeArgumentReferenceExpression("__w");
            expression.Method.MethodName = "Write";
            expression.Parameters.Add(expr);
            return statement;
        }

        private void HandleDeviceFilterConditional(ref PropertyEntry previous, PropertyEntry current, CodeStatementCollection topStmts, ref CodeStatementCollection currentStmts, out CodeStatementCollection nextStmts)
        {
            bool flag = (previous != null) && StringUtil.EqualsIgnoreCase(previous.Name, current.Name);
            if (current.Filter.Length != 0)
            {
                if (!flag)
                {
                    currentStmts = topStmts;
                    previous = null;
                }
                CodeConditionStatement statement = new CodeConditionStatement();
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "TestDeviceFilter", new CodeExpression[0]);
                expression.Parameters.Add(new CodePrimitiveExpression(current.Filter));
                statement.Condition = expression;
                currentStmts.Add(statement);
                currentStmts = statement.TrueStatements;
                nextStmts = statement.FalseStatements;
                previous = current;
            }
            else
            {
                if (!flag)
                {
                    currentStmts = topStmts;
                }
                nextStmts = topStmts;
                previous = null;
            }
        }

        protected virtual bool UseResourceLiteralString(string s)
        {
            return ((PageParser.EnableLongStringsAsResources && (s.Length >= 0x100)) && base._codeDomProvider.Supports(GeneratorSupport.Win32Resources));
        }

        private TemplateParser Parser
        {
            get
            {
                return this._parser;
            }
        }
    }
}

