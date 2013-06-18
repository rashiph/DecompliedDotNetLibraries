namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Web.UI;

    internal abstract class TemplateControlCodeDomTreeGenerator : BaseTemplateCodeDomTreeGenerator
    {
        private TemplateControlParser _tcParser;
        internal const int badBaseClassLineMarker = 0xdebb0;
        private const string literalMemoryBlockName = "__literals";
        private const string stringResourcePointerName = "__stringResource";

        internal TemplateControlCodeDomTreeGenerator(TemplateControlParser tcParser) : base(tcParser)
        {
            this._tcParser = tcParser;
        }

        private void BuildApplicationInstanceProperty()
        {
            CodeMemberProperty property;
            Type globalAsaxType = BuildManager.GetGlobalAsaxType();
            property = new CodeMemberProperty {
                Attributes = property.Attributes & ~MemberAttributes.AccessMask,
                Attributes = property.Attributes & ~MemberAttributes.ScopeMask,
                Attributes = property.Attributes | (MemberAttributes.Family | MemberAttributes.Final)
            };
            if (base._designerMode)
            {
                base.ApplyEditorBrowsableCustomAttribute(property);
            }
            property.Name = "ApplicationInstance";
            property.Type = new CodeTypeReference(globalAsaxType);
            CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Context");
            targetObject = new CodePropertyReferenceExpression(targetObject, "ApplicationInstance");
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(globalAsaxType, targetObject)));
            base._intermediateClass.Members.Add(property);
        }

        private void BuildAutomaticEventHookup()
        {
            if ((base._sourceDataClass != null) && !this.Parser.FAutoEventWireup)
            {
                CodeMemberProperty property;
                property = new CodeMemberProperty {
                    Attributes = property.Attributes & ~MemberAttributes.AccessMask,
                    Attributes = property.Attributes & ~MemberAttributes.ScopeMask,
                    Attributes = property.Attributes | (MemberAttributes.Family | MemberAttributes.Override),
                    Name = "SupportAutoEvents",
                    Type = new CodeTypeReference(typeof(bool))
                };
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
                base._sourceDataClass.Members.Add(property);
            }
        }

        private void BuildFrameworkInitializeMethod()
        {
            if (base._sourceDataClass != null)
            {
                CodeMemberMethod method = new CodeMemberMethod();
                base.AddDebuggerNonUserCodeAttribute(method);
                method.Attributes &= ~MemberAttributes.AccessMask;
                method.Attributes &= ~MemberAttributes.ScopeMask;
                method.Attributes |= MemberAttributes.Family | MemberAttributes.Override;
                method.Name = "FrameworkInitialize";
                this.BuildFrameworkInitializeMethodContents(method);
                if (!base._designerMode && (this.Parser.CodeFileVirtualPath != null))
                {
                    method.LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(this.Parser.CodeFileVirtualPath.VirtualPathString, 0xdebb0);
                }
                base._sourceDataClass.Members.Add(method);
            }
        }

        protected virtual void BuildFrameworkInitializeMethodContents(CodeMemberMethod method)
        {
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), method.Name, new CodeExpression[0]);
            method.Statements.Add(new CodeExpressionStatement(expression));
            if (base._stringResourceBuilder.HasStrings)
            {
                CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "SetStringResourcePointer", new CodeExpression[0]);
                expression2.Parameters.Add(new CodeFieldReferenceExpression(base._classTypeExpr, "__stringResource"));
                expression2.Parameters.Add(new CodePrimitiveExpression(0));
                method.Statements.Add(new CodeExpressionStatement(expression2));
            }
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "__BuildControlTree" }
            };
            expression3.Parameters.Add(new CodeThisReferenceExpression());
            method.Statements.Add(new CodeExpressionStatement(expression3));
        }

        protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements)
        {
            base.BuildInitStatements(trueStatements, topLevelStatements);
            if (base._stringResourceBuilder.HasStrings)
            {
                CodeMemberField field;
                field = new CodeMemberField(typeof(object), "__stringResource") {
                    Attributes = field.Attributes | MemberAttributes.Static
                };
                base._sourceDataClass.Members.Add(field);
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodeFieldReferenceExpression(base._classTypeExpr, "__stringResource")
                };
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                    Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "ReadStringResource" }
                };
                statement.Right = expression;
                trueStatements.Add(statement);
            }
            CodeAssignStatement statement2 = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeCastExpression(CodeDomUtility.BuildGlobalCodeTypeReference(this.Parser.BaseType), new CodeThisReferenceExpression()), "AppRelativeVirtualPath"), new CodePrimitiveExpression(this.Parser.CurrentVirtualPath.AppRelativeVirtualPathString));
            if (!base._designerMode && (this.Parser.CodeFileVirtualPath != null))
            {
                statement2.LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(this.Parser.CodeFileVirtualPath.VirtualPathString, 0xdebb0);
            }
            topLevelStatements.Add(statement2);
        }

        protected override void BuildMiscClassMembers()
        {
            base.BuildMiscClassMembers();
            if (!base._designerMode)
            {
                this.BuildAutomaticEventHookup();
            }
            this.BuildApplicationInstanceProperty();
            this.BuildSourceDataTreeFromBuilder(this.Parser.RootBuilder, false, false, null);
            if (!base._designerMode)
            {
                this.BuildFrameworkInitializeMethod();
            }
        }

        internal void BuildStronglyTypedProperty(string propertyName, Type propertyType)
        {
            if (!base._usingVJSCompiler)
            {
                CodeMemberProperty property;
                property = new CodeMemberProperty {
                    Attributes = property.Attributes & ~MemberAttributes.AccessMask,
                    Attributes = property.Attributes & ~MemberAttributes.ScopeMask,
                    Attributes = property.Attributes | (MemberAttributes.Public | MemberAttributes.New | MemberAttributes.Final),
                    Name = propertyName,
                    Type = new CodeTypeReference(propertyType)
                };
                CodePropertyReferenceExpression expression = new CodePropertyReferenceExpression(new CodeBaseReferenceExpression(), propertyName);
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(propertyType, expression)));
                base._intermediateClass.Members.Add(property);
            }
        }

        private TemplateControlParser Parser
        {
            get
            {
                return this._tcParser;
            }
        }
    }
}

