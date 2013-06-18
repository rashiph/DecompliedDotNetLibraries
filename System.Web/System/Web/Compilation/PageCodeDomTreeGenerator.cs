namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Reflection;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.Util;

    internal class PageCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator
    {
        private const string _masterPropertyName = "Master";
        private PageParser _pageParser;
        private const string _previousPagePropertyName = "PreviousPage";
        private const string _styleSheetThemePropertyName = "StyleSheetTheme";
        internal const int DebugScriptTimeout = 0x1c9c380;
        private const string dependenciesLocalName = "dependencies";
        private const string fileDependenciesName = "__fileDependencies";
        private const string outputCacheSettingsFieldName = "__outputCacheSettings";
        private const string outputCacheSettingsLocalName = "outputCacheSettings";

        internal PageCodeDomTreeGenerator(PageParser pageParser) : base(pageParser)
        {
            this._pageParser = pageParser;
        }

        private void BuildAspCompatMethods()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "BeginProcessRequest";
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            method.Attributes |= MemberAttributes.Public;
            method.ImplementationTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext), "context"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(AsyncCallback), "cb"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "data"));
            method.ReturnType = new CodeTypeReference(typeof(IAsyncResult));
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "AspCompatBeginProcessRequest" }
            };
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("context"));
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("cb"));
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("data"));
            method.Statements.Add(new CodeMethodReturnStatement(expression2));
            base._sourceDataClass.Members.Add(method);
            method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "EndProcessRequest";
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            method.Attributes |= MemberAttributes.Public;
            method.ImplementationTypes.Add(typeof(IHttpAsyncHandler));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IAsyncResult), "ar"));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "AspCompatEndProcessRequest" }
            };
            expression.Parameters.Add(new CodeArgumentReferenceExpression("ar"));
            method.Statements.Add(expression);
            base._sourceDataClass.Members.Add(method);
        }

        private void BuildAsyncPageMethods()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "BeginProcessRequest";
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            method.Attributes |= MemberAttributes.Public;
            method.ImplementationTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext), "context"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(AsyncCallback), "cb"));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "data"));
            method.ReturnType = new CodeTypeReference(typeof(IAsyncResult));
            CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "AsyncPageBeginProcessRequest" }
            };
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("context"));
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("cb"));
            expression2.Parameters.Add(new CodeArgumentReferenceExpression("data"));
            method.Statements.Add(new CodeMethodReturnStatement(expression2));
            base._sourceDataClass.Members.Add(method);
            method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "EndProcessRequest";
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            method.Attributes |= MemberAttributes.Public;
            method.ImplementationTypes.Add(typeof(IHttpAsyncHandler));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IAsyncResult), "ar"));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "AsyncPageEndProcessRequest" }
            };
            expression.Parameters.Add(new CodeArgumentReferenceExpression("ar"));
            method.Statements.Add(expression);
            base._sourceDataClass.Members.Add(method);
        }

        protected override void BuildDefaultConstructor()
        {
            base.BuildDefaultConstructor();
            if (base.CompilParams.IncludeDebugInformation)
            {
                CodeAssignStatement statement = new CodeAssignStatement {
                    Left = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Server"), "ScriptTimeout"),
                    Right = new CodePrimitiveExpression(0x1c9c380)
                };
                base._ctor.Statements.Add(statement);
            }
            if (this.Parser.TransactionMode != 0)
            {
                base._ctor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TransactionMode"), new CodePrimitiveExpression(this.Parser.TransactionMode)));
            }
            if (this.Parser.AspCompatMode)
            {
                base._ctor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "AspCompatMode"), new CodePrimitiveExpression(this.Parser.AspCompatMode)));
            }
            if (this.Parser.AsyncMode)
            {
                base._ctor.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "AsyncMode"), new CodePrimitiveExpression(this.Parser.AsyncMode)));
            }
            if (this.Parser.OutputCacheParameters != null)
            {
                OutputCacheParameters outputCacheParameters = this.Parser.OutputCacheParameters;
                if (((outputCacheParameters.CacheProfile != null) && (outputCacheParameters.CacheProfile.Length != 0)) || ((outputCacheParameters.Duration != 0) || (outputCacheParameters.Location == OutputCacheLocation.None)))
                {
                    CodeMemberField field;
                    field = new CodeMemberField(typeof(OutputCacheParameters), "__outputCacheSettings") {
                        Attributes = field.Attributes | MemberAttributes.Static,
                        InitExpression = new CodePrimitiveExpression(null)
                    };
                    base._sourceDataClass.Members.Add(field);
                    CodeConditionStatement statement2 = new CodeConditionStatement {
                        Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(base._classTypeExpr, "__outputCacheSettings"), CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
                    };
                    CodeVariableDeclarationStatement statement3 = new CodeVariableDeclarationStatement {
                        Type = new CodeTypeReference(typeof(OutputCacheParameters)),
                        Name = "outputCacheSettings"
                    };
                    statement2.TrueStatements.Insert(0, statement3);
                    CodeObjectCreateExpression right = new CodeObjectCreateExpression {
                        CreateType = new CodeTypeReference(typeof(OutputCacheParameters))
                    };
                    CodeVariableReferenceExpression left = new CodeVariableReferenceExpression("outputCacheSettings");
                    CodeAssignStatement statement4 = new CodeAssignStatement(left, right);
                    statement2.TrueStatements.Add(statement4);
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.CacheProfile))
                    {
                        CodeAssignStatement statement5 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "CacheProfile"), new CodePrimitiveExpression(outputCacheParameters.CacheProfile));
                        statement2.TrueStatements.Add(statement5);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.Duration))
                    {
                        CodeAssignStatement statement6 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "Duration"), new CodePrimitiveExpression(outputCacheParameters.Duration));
                        statement2.TrueStatements.Add(statement6);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.Enabled))
                    {
                        CodeAssignStatement statement7 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "Enabled"), new CodePrimitiveExpression(outputCacheParameters.Enabled));
                        statement2.TrueStatements.Add(statement7);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.Location))
                    {
                        CodeAssignStatement statement8 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "Location"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(OutputCacheLocation)), outputCacheParameters.Location.ToString()));
                        statement2.TrueStatements.Add(statement8);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.NoStore))
                    {
                        CodeAssignStatement statement9 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "NoStore"), new CodePrimitiveExpression(outputCacheParameters.NoStore));
                        statement2.TrueStatements.Add(statement9);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.SqlDependency))
                    {
                        CodeAssignStatement statement10 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "SqlDependency"), new CodePrimitiveExpression(outputCacheParameters.SqlDependency));
                        statement2.TrueStatements.Add(statement10);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.VaryByControl))
                    {
                        CodeAssignStatement statement11 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "VaryByControl"), new CodePrimitiveExpression(outputCacheParameters.VaryByControl));
                        statement2.TrueStatements.Add(statement11);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.VaryByCustom))
                    {
                        CodeAssignStatement statement12 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "VaryByCustom"), new CodePrimitiveExpression(outputCacheParameters.VaryByCustom));
                        statement2.TrueStatements.Add(statement12);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.VaryByContentEncoding))
                    {
                        CodeAssignStatement statement13 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "VaryByContentEncoding"), new CodePrimitiveExpression(outputCacheParameters.VaryByContentEncoding));
                        statement2.TrueStatements.Add(statement13);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.VaryByHeader))
                    {
                        CodeAssignStatement statement14 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "VaryByHeader"), new CodePrimitiveExpression(outputCacheParameters.VaryByHeader));
                        statement2.TrueStatements.Add(statement14);
                    }
                    if (outputCacheParameters.IsParameterSet(OutputCacheParameter.VaryByParam))
                    {
                        CodeAssignStatement statement15 = new CodeAssignStatement(new CodePropertyReferenceExpression(left, "VaryByParam"), new CodePrimitiveExpression(outputCacheParameters.VaryByParam));
                        statement2.TrueStatements.Add(statement15);
                    }
                    CodeFieldReferenceExpression expression3 = new CodeFieldReferenceExpression(base._classTypeExpr, "__outputCacheSettings");
                    CodeAssignStatement statement16 = new CodeAssignStatement(expression3, left);
                    statement2.TrueStatements.Add(statement16);
                    base._ctor.Statements.Add(statement2);
                }
            }
        }

        protected override void BuildFrameworkInitializeMethodContents(CodeMemberMethod method)
        {
            if (this.Parser.StyleSheetTheme != null)
            {
                CodeExpression right = new CodePrimitiveExpression(this.Parser.StyleSheetTheme);
                CodeExpression left = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "StyleSheetTheme");
                CodeAssignStatement statement = new CodeAssignStatement(left, right);
                method.Statements.Add(statement);
            }
            base.BuildFrameworkInitializeMethodContents(method);
            CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "AddWrappedFileDependencies" }
            };
            expression3.Parameters.Add(new CodeFieldReferenceExpression(base._classTypeExpr, "__fileDependencies"));
            method.Statements.Add(expression3);
            if (this.Parser.OutputCacheParameters != null)
            {
                OutputCacheParameters outputCacheParameters = this.Parser.OutputCacheParameters;
                if (((outputCacheParameters.CacheProfile != null) && (outputCacheParameters.CacheProfile.Length != 0)) || ((outputCacheParameters.Duration != 0) || (outputCacheParameters.Location == OutputCacheLocation.None)))
                {
                    CodeMethodInvokeExpression expression4 = new CodeMethodInvokeExpression {
                        Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "InitOutputCache" }
                    };
                    expression4.Parameters.Add(new CodeFieldReferenceExpression(base._classTypeExpr, "__outputCacheSettings"));
                    method.Statements.Add(expression4);
                }
            }
            if (this.Parser.TraceEnabled != TraceEnable.Default)
            {
                method.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TraceEnabled"), new CodePrimitiveExpression(this.Parser.TraceEnabled == TraceEnable.Enable)));
            }
            if (this.Parser.TraceMode != TraceMode.Default)
            {
                method.Statements.Add(new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TraceModeValue"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(TraceMode)), this.Parser.TraceMode.ToString())));
            }
            if (this.Parser.ValidateRequest)
            {
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                    Method = { TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Request"), MethodName = "ValidateInput" }
                };
                method.Statements.Add(new CodeExpressionStatement(expression));
            }
        }

        private void BuildGetTypeHashCodeMethod()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "GetTypeHashCode";
            method.ReturnType = new CodeTypeReference(typeof(int));
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            method.Attributes |= MemberAttributes.Public | MemberAttributes.Override;
            base._sourceDataClass.Members.Add(method);
            method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(this.Parser.TypeHashCode)));
        }

        protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements)
        {
            CodeMemberField field;
            base.BuildInitStatements(trueStatements, topLevelStatements);
            field = new CodeMemberField(typeof(object), "__fileDependencies") {
                Attributes = field.Attributes | MemberAttributes.Static
            };
            base._sourceDataClass.Members.Add(field);
            CodeVariableDeclarationStatement statement = new CodeVariableDeclarationStatement {
                Type = new CodeTypeReference(typeof(string[])),
                Name = "dependencies"
            };
            topLevelStatements.Insert(0, statement);
            StringSet set = new StringSet();
            set.AddCollection(this.Parser.SourceDependencies);
            CodeAssignStatement statement2 = new CodeAssignStatement {
                Left = new CodeVariableReferenceExpression("dependencies"),
                Right = new CodeArrayCreateExpression(typeof(string), set.Count)
            };
            trueStatements.Add(statement2);
            int num = 0;
            foreach (string str in (IEnumerable) set)
            {
                CodeAssignStatement statement3 = new CodeAssignStatement {
                    Left = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("dependencies"), new CodeExpression[] { new CodePrimitiveExpression(num++) })
                };
                string str2 = UrlPath.MakeVirtualPathAppRelative(str);
                statement3.Right = new CodePrimitiveExpression(str2);
                trueStatements.Add(statement3);
            }
            CodeAssignStatement statement4 = new CodeAssignStatement {
                Left = new CodeFieldReferenceExpression(base._classTypeExpr, "__fileDependencies")
            };
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeThisReferenceExpression(), MethodName = "GetWrappedFileDependencies" }
            };
            expression.Parameters.Add(new CodeVariableReferenceExpression("dependencies"));
            statement4.Right = expression;
            trueStatements.Add(statement4);
        }

        protected override void BuildMiscClassMembers()
        {
            base.BuildMiscClassMembers();
            if (!base._designerMode && (base._sourceDataClass != null))
            {
                this.BuildGetTypeHashCodeMethod();
                if (this.Parser.AspCompatMode)
                {
                    this.BuildAspCompatMethods();
                }
                if (this.Parser.AsyncMode)
                {
                    this.BuildAsyncPageMethods();
                }
                this.BuildProcessRequestOverride();
            }
            if (this.Parser.PreviousPageType != null)
            {
                base.BuildStronglyTypedProperty("PreviousPage", this.Parser.PreviousPageType);
            }
            if (this.Parser.MasterPageType != null)
            {
                base.BuildStronglyTypedProperty("Master", this.Parser.MasterPageType);
            }
        }

        internal override CodeExpression BuildPagePropertyReferenceExpression()
        {
            return new CodeThisReferenceExpression();
        }

        private void BuildProcessRequestOverride()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            base.AddDebuggerNonUserCodeAttribute(method);
            method.Name = "ProcessRequest";
            method.Attributes &= ~MemberAttributes.AccessMask;
            method.Attributes &= ~MemberAttributes.ScopeMask;
            MethodInfo info = null;
            if (this.Parser.BaseType != typeof(Page))
            {
                info = this.Parser.BaseType.GetMethod("ProcessRequest", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
            }
            base._sourceDataClass.BaseTypes.Add(new CodeTypeReference(typeof(IHttpHandler)));
            if ((info != null) && (info.DeclaringType != typeof(Page)))
            {
                method.Attributes |= MemberAttributes.Public | MemberAttributes.New;
            }
            else
            {
                method.Attributes |= MemberAttributes.Public | MemberAttributes.Override;
            }
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext), "context"));
            CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression {
                Method = { TargetObject = new CodeBaseReferenceExpression(), MethodName = "ProcessRequest" }
            };
            expression.Parameters.Add(new CodeArgumentReferenceExpression("context"));
            method.Statements.Add(expression);
            base._sourceDataClass.Members.Add(method);
        }

        protected override void GenerateInterfaces()
        {
            base.GenerateInterfaces();
            if (this.Parser.FRequiresSessionState)
            {
                base._intermediateClass.BaseTypes.Add(new CodeTypeReference(typeof(IRequiresSessionState)));
            }
            if (this.Parser.FReadOnlySessionState)
            {
                base._intermediateClass.BaseTypes.Add(new CodeTypeReference(typeof(IReadOnlySessionState)));
            }
            if ((!base._designerMode && (base._sourceDataClass != null)) && (this.Parser.AspCompatMode || this.Parser.AsyncMode))
            {
                base._sourceDataClass.BaseTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
            }
        }

        private PageParser Parser
        {
            get
            {
                return this._pageParser;
            }
        }
    }
}

