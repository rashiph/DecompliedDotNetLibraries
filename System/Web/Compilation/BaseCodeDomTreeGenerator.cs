namespace System.Web.Compilation
{
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Profile;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class BaseCodeDomTreeGenerator
    {
        protected CodeTypeReferenceExpression _classTypeExpr;
        protected CodeCompileUnit _codeCompileUnit;
        protected CodeDomProvider _codeDomProvider;
        private CompilerParameters _compilParams;
        protected CodeConstructor _ctor;
        private const int _defaultColumnOffset = 4;
        protected bool _designerMode;
        private const string _dummyVariable = "__dummyVar";
        private static IDictionary _generatedColumnOffsetDictionary;
        protected CodeTypeDeclaration _intermediateClass;
        private IDictionary _linePragmasTable;
        private TemplateParser _parser;
        private int _pragmaIdGenerator = 1;
        protected CodeTypeDeclaration _sourceDataClass;
        private CodeNamespace _sourceDataNamespace;
        protected StringResourceBuilder _stringResourceBuilder;
        private static bool _urlLinePragmas = MTConfigUtil.GetCompilationAppConfig().UrlLinePragmas;
        protected bool _usingVJSCompiler;
        private VirtualPath _virtualPath;
        internal const string defaultNamespace = "ASP";
        private const string initializedFieldName = "__initialized";
        internal const string internalAspNamespace = "__ASP";

        protected BaseCodeDomTreeGenerator(TemplateParser parser)
        {
            this._parser = parser;
        }

        protected void AddDebuggerNonUserCodeAttribute(CodeMemberMethod method)
        {
            if ((method != null) && this.Parser.FLinePragmas)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(DebuggerNonUserCodeAttribute)));
                method.CustomAttributes.Add(declaration);
            }
        }

        protected void ApplyEditorBrowsableCustomAttribute(CodeTypeMember member)
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration {
                Name = typeof(EditorBrowsableAttribute).FullName
            };
            declaration.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EditorBrowsableState)), "Never")));
            member.CustomAttributes.Add(declaration);
        }

        protected void BuildAccessorProperty(string propName, CodeFieldReferenceExpression fieldRef, Type propType, MemberAttributes attributes, CodeAttributeDeclarationCollection attrDeclarations)
        {
            CodeMemberProperty property = new CodeMemberProperty {
                Attributes = attributes,
                Name = propName,
                Type = new CodeTypeReference(propType)
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(fieldRef));
            property.SetStatements.Add(new CodeAssignStatement(fieldRef, new CodePropertySetValueReferenceExpression()));
            if (attrDeclarations != null)
            {
                property.CustomAttributes = attrDeclarations;
            }
            this._sourceDataClass.Members.Add(property);
        }

        private void BuildApplicationObjectProperties()
        {
            if (this.Parser.ApplicationObjects != null)
            {
                this.BuildObjectPropertiesHelper(this.Parser.ApplicationObjects.Objects, true);
            }
        }

        protected virtual void BuildDefaultConstructor()
        {
            CodeMemberField field;
            this._ctor.Attributes &= ~MemberAttributes.AccessMask;
            this._ctor.Attributes |= MemberAttributes.Public;
            field = new CodeMemberField(typeof(bool), "__initialized") {
                Attributes = field.Attributes | MemberAttributes.Static
            };
            this._sourceDataClass.Members.Add(field);
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(this._classTypeExpr, "__initialized"), CodeBinaryOperatorType.ValueEquality, new CodePrimitiveExpression(false))
            };
            this.BuildInitStatements(statement.TrueStatements, this._ctor.Statements);
            statement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(this._classTypeExpr, "__initialized"), new CodePrimitiveExpression(true)));
            this._ctor.Statements.Add(statement);
        }

        protected void BuildFieldAndAccessorProperty(string propName, string fieldName, Type propType, bool fStatic, CodeAttributeDeclarationCollection attrDeclarations)
        {
            CodeMemberField field = new CodeMemberField(propType, fieldName);
            if (fStatic)
            {
                field.Attributes |= MemberAttributes.Static;
            }
            this._sourceDataClass.Members.Add(field);
            CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
            this.BuildAccessorProperty(propName, fieldRef, propType, MemberAttributes.Public, attrDeclarations);
        }

        protected virtual void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements)
        {
        }

        private void BuildInjectedGetPropertyMethod(string propName, Type propType, CodeExpression propertyInitExpression, bool fPublicProp)
        {
            string fieldName = "cached" + propName;
            CodeExpression left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
            this._sourceDataClass.Members.Add(new CodeMemberField(propType, fieldName));
            CodeMemberProperty property = new CodeMemberProperty();
            if (fPublicProp)
            {
                property.Attributes &= ~MemberAttributes.AccessMask;
                property.Attributes |= MemberAttributes.Public;
            }
            property.Name = propName;
            property.Type = new CodeTypeReference(propType);
            CodeConditionStatement statement = new CodeConditionStatement {
                Condition = new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null))
            };
            statement.TrueStatements.Add(new CodeAssignStatement(left, propertyInitExpression));
            property.GetStatements.Add(statement);
            property.GetStatements.Add(new CodeMethodReturnStatement(left));
            this._sourceDataClass.Members.Add(property);
        }

        protected virtual void BuildMiscClassMembers()
        {
            if (this.NeedProfileProperty)
            {
                this.BuildProfileProperty();
            }
            if (this._sourceDataClass != null)
            {
                this.BuildApplicationObjectProperties();
                this.BuildSessionObjectProperties();
                this.BuildPageObjectProperties();
                foreach (ScriptBlockData data in this.Parser.ScriptList)
                {
                    string script = data.Script;
                    CodeSnippetTypeMember member = new CodeSnippetTypeMember(script.PadLeft((script.Length + data.Column) - 1)) {
                        LinePragma = this.CreateCodeLinePragma(data.VirtualPath, data.Line, data.Column, data.Column, data.Script.Length, false)
                    };
                    this._sourceDataClass.Members.Add(member);
                }
            }
        }

        private void BuildObjectPropertiesHelper(IDictionary objects, bool useApplicationState)
        {
            IDictionaryEnumerator enumerator = objects.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) enumerator.Value;
                CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), useApplicationState ? "Application" : "Session"), "StaticObjects");
                CodeMethodInvokeExpression expression = new CodeMethodInvokeExpression(targetObject, "GetObject", new CodeExpression[0]);
                expression.Parameters.Add(new CodePrimitiveExpression(entry.Name));
                Type declaredType = entry.DeclaredType;
                if (useApplicationState)
                {
                    this.BuildInjectedGetPropertyMethod(entry.Name, declaredType, new CodeCastExpression(declaredType, expression), false);
                }
                else
                {
                    CodeMemberProperty property = new CodeMemberProperty {
                        Name = entry.Name,
                        Type = new CodeTypeReference(declaredType)
                    };
                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(declaredType, expression)));
                    this._sourceDataClass.Members.Add(property);
                }
            }
        }

        private void BuildPageObjectProperties()
        {
            if (this.Parser.PageObjectList != null)
            {
                foreach (ObjectTagBuilder builder in this.Parser.PageObjectList)
                {
                    CodeExpression expression;
                    if (builder.Progid != null)
                    {
                        CodeMethodInvokeExpression expression2 = new CodeMethodInvokeExpression {
                            Method = { TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Server"), MethodName = "CreateObject" }
                        };
                        expression2.Parameters.Add(new CodePrimitiveExpression(builder.Progid));
                        expression = expression2;
                    }
                    else if (builder.Clsid != null)
                    {
                        CodeMethodInvokeExpression expression3 = new CodeMethodInvokeExpression {
                            Method = { TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Server"), MethodName = "CreateObjectFromClsid" }
                        };
                        expression3.Parameters.Add(new CodePrimitiveExpression(builder.Clsid));
                        expression = expression3;
                    }
                    else
                    {
                        expression = new CodeObjectCreateExpression(builder.ObjectType, new CodeExpression[0]);
                    }
                    this.BuildInjectedGetPropertyMethod(builder.ID, builder.DeclaredType, expression, this.IsGlobalAsaxGenerator);
                }
            }
        }

        private void BuildProfileProperty()
        {
            if (ProfileManager.Enabled)
            {
                CodeMemberProperty property;
                string profileClassName = ProfileBase.GetProfileClassName();
                property = new CodeMemberProperty {
                    Attributes = property.Attributes & ~MemberAttributes.AccessMask,
                    Attributes = property.Attributes & ~MemberAttributes.ScopeMask,
                    Attributes = property.Attributes | (MemberAttributes.Family | MemberAttributes.Final),
                    Name = "Profile"
                };
                if (this._designerMode)
                {
                    this.ApplyEditorBrowsableCustomAttribute(property);
                }
                property.Type = new CodeTypeReference(profileClassName);
                CodePropertyReferenceExpression targetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Context");
                targetObject = new CodePropertyReferenceExpression(targetObject, "Profile");
                property.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(profileClassName, targetObject)));
                this._intermediateClass.Members.Add(property);
            }
        }

        private void BuildSessionObjectProperties()
        {
            if (this.Parser.SessionObjects != null)
            {
                this.BuildObjectPropertiesHelper(this.Parser.SessionObjects.Objects, false);
            }
        }

        private bool BuildSourceDataTree()
        {
            this._compilParams = this.Parser.CompilParams;
            this._codeCompileUnit = new CodeCompileUnit();
            this._codeCompileUnit.UserData["AllowLateBound"] = !this.Parser.FStrict;
            this._codeCompileUnit.UserData["RequireVariableDeclaration"] = this.Parser.FExplicit;
            this._usingVJSCompiler = this._codeDomProvider.FileExtension == ".jsl";
            this._sourceDataNamespace = new CodeNamespace(this.Parser.GeneratedNamespace);
            string generatedClassName = this.GetGeneratedClassName();
            if (this.Parser.BaseTypeName != null)
            {
                CodeNamespace namespace2 = new CodeNamespace(this.Parser.BaseTypeNamespace);
                this._codeCompileUnit.Namespaces.Add(namespace2);
                this._intermediateClass = new CodeTypeDeclaration(this.Parser.BaseTypeName);
                if (this._designerMode)
                {
                    this._intermediateClass.UserData["BaseClassDefinition"] = this.Parser.DefaultBaseType;
                }
                else
                {
                    this._intermediateClass.UserData["BaseClassDefinition"] = this.Parser.BaseType;
                }
                namespace2.Types.Add(this._intermediateClass);
                this._intermediateClass.IsPartial = true;
                if (!this.PrecompilingForUpdatableDeployment)
                {
                    this._sourceDataClass = new CodeTypeDeclaration(generatedClassName);
                    this._sourceDataClass.BaseTypes.Add(CodeDomUtility.BuildGlobalCodeTypeReference(Util.MakeFullTypeName(this.Parser.BaseTypeNamespace, this.Parser.BaseTypeName)));
                    this._sourceDataNamespace.Types.Add(this._sourceDataClass);
                }
            }
            else
            {
                this._intermediateClass = new CodeTypeDeclaration(generatedClassName);
                this._intermediateClass.BaseTypes.Add(CodeDomUtility.BuildGlobalCodeTypeReference(this.Parser.BaseType));
                this._sourceDataNamespace.Types.Add(this._intermediateClass);
                this._sourceDataClass = this._intermediateClass;
            }
            this._codeCompileUnit.Namespaces.Add(this._sourceDataNamespace);
            if (this.PrecompilingForUpdatableDeployment && (this.Parser.CodeFileVirtualPath == null))
            {
                return false;
            }
            this.GenerateClassAttributes();
            if (this._codeDomProvider is VBCodeProvider)
            {
                this._sourceDataNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.VisualBasic"));
            }
            if (this.Parser.NamespaceEntries != null)
            {
                foreach (NamespaceEntry entry in this.Parser.NamespaceEntries.Values)
                {
                    CodeLinePragma pragma;
                    if (entry.VirtualPath != null)
                    {
                        pragma = this.CreateCodeLinePragma(entry.VirtualPath, entry.Line);
                    }
                    else
                    {
                        pragma = null;
                    }
                    CodeNamespaceImport import = new CodeNamespaceImport(entry.Namespace) {
                        LinePragma = pragma
                    };
                    this._sourceDataNamespace.Imports.Add(import);
                }
            }
            if (this._sourceDataClass != null)
            {
                CodeTypeReference type = CodeDomUtility.BuildGlobalCodeTypeReference(Util.MakeFullTypeName(this._sourceDataNamespace.Name, this._sourceDataClass.Name));
                this._classTypeExpr = new CodeTypeReferenceExpression(type);
            }
            this.GenerateInterfaces();
            this.BuildMiscClassMembers();
            if (!this._designerMode && (this._sourceDataClass != null))
            {
                this._ctor = new CodeConstructor();
                this.AddDebuggerNonUserCodeAttribute(this._ctor);
                this._sourceDataClass.Members.Add(this._ctor);
                this.BuildDefaultConstructor();
            }
            return true;
        }

        protected CodeLinePragma CreateCodeLinePragma(ControlBuilder builder)
        {
            string pageVirtualPath = builder.PageVirtualPath;
            int line = builder.Line;
            int column = 1;
            int generatedColumn = 1;
            int codeLength = -1;
            CodeBlockBuilder builder2 = builder as CodeBlockBuilder;
            if (builder2 != null)
            {
                column = builder2.Column;
                codeLength = builder2.Content.Length;
                if (builder2.BlockType == CodeBlockType.Code)
                {
                    generatedColumn = column;
                }
                else
                {
                    generatedColumn = "__o".Length + GetGeneratedColumnOffset(this._codeDomProvider);
                }
            }
            return this.CreateCodeLinePragma(pageVirtualPath, line, column, generatedColumn, codeLength);
        }

        protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber)
        {
            return this.CreateCodeLinePragma(virtualPath, lineNumber, 1, 1, -1, true);
        }

        protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber, int column, int generatedColumn, int codeLength)
        {
            return this.CreateCodeLinePragma(virtualPath, lineNumber, column, generatedColumn, codeLength, true);
        }

        protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber, int column, int generatedColumn, int codeLength, bool isCodeNugget)
        {
            if (!this.Parser.FLinePragmas)
            {
                return null;
            }
            if (string.IsNullOrEmpty(virtualPath))
            {
                return null;
            }
            if (this._designerMode)
            {
                if (codeLength < 0)
                {
                    return null;
                }
                LinePragmaCodeInfo info = new LinePragmaCodeInfo {
                    _startLine = lineNumber,
                    _startColumn = column,
                    _startGeneratedColumn = generatedColumn,
                    _codeLength = codeLength,
                    _isCodeNugget = isCodeNugget
                };
                lineNumber = this._pragmaIdGenerator++;
                if (this._linePragmasTable == null)
                {
                    this._linePragmasTable = new Hashtable();
                }
                this._linePragmasTable[lineNumber] = info;
            }
            return CreateCodeLinePragmaHelper(virtualPath, lineNumber);
        }

        internal static CodeLinePragma CreateCodeLinePragmaHelper(string virtualPath, int lineNumber)
        {
            string path = null;
            if (UrlPath.IsAbsolutePhysicalPath(virtualPath))
            {
                path = virtualPath;
            }
            else if (_urlLinePragmas)
            {
                path = ErrorFormatter.MakeHttpLinePragma(virtualPath);
            }
            else
            {
                try
                {
                    path = HostingEnvironment.MapPathInternal(virtualPath);
                    if (!File.Exists(path))
                    {
                        path = ErrorFormatter.MakeHttpLinePragma(virtualPath);
                    }
                }
                catch
                {
                    path = ErrorFormatter.MakeHttpLinePragma(virtualPath);
                }
            }
            return new CodeLinePragma(path, lineNumber);
        }

        protected virtual void GenerateClassAttributes()
        {
            if (this.CompilParams.IncludeDebugInformation && (this._sourceDataClass != null))
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration("System.Runtime.CompilerServices.CompilerGlobalScopeAttribute");
                this._sourceDataClass.CustomAttributes.Add(declaration);
            }
        }

        protected virtual void GenerateInterfaces()
        {
            if (this.Parser.ImplementedInterfaces != null)
            {
                foreach (Type type in this.Parser.ImplementedInterfaces)
                {
                    this._intermediateClass.BaseTypes.Add(new CodeTypeReference(type));
                }
            }
        }

        internal CodeCompileUnit GetCodeDomTree(CodeDomProvider codeDomProvider, StringResourceBuilder stringResourceBuilder, VirtualPath virtualPath)
        {
            this._codeDomProvider = codeDomProvider;
            this._stringResourceBuilder = stringResourceBuilder;
            this._virtualPath = virtualPath;
            if (!this.BuildSourceDataTree())
            {
                return null;
            }
            if (this.Parser.RootBuilder != null)
            {
                this.Parser.RootBuilder.OnCodeGenerationComplete();
            }
            return this._codeCompileUnit;
        }

        protected virtual string GetGeneratedClassName()
        {
            if (this.Parser.GeneratedClassName != null)
            {
                return this.Parser.GeneratedClassName;
            }
            string fileName = this._virtualPath.FileName;
            string appRelativeVirtualPathStringOrNull = this._virtualPath.Parent.AppRelativeVirtualPathStringOrNull;
            if (appRelativeVirtualPathStringOrNull != null)
            {
                fileName = appRelativeVirtualPathStringOrNull.Substring(2) + fileName;
            }
            fileName = Util.MakeValidTypeNameFromString(fileName).ToLowerInvariant();
            string str3 = (this.Parser.BaseTypeName != null) ? this.Parser.BaseTypeName : this.Parser.BaseType.Name;
            if (StringUtil.EqualsIgnoreCase(fileName, str3))
            {
                fileName = "_" + fileName;
            }
            return fileName;
        }

        private static int GetGeneratedColumnOffset(CodeDomProvider codeDomProvider)
        {
            object obj2 = null;
            if (_generatedColumnOffsetDictionary == null)
            {
                _generatedColumnOffsetDictionary = new ListDictionary();
            }
            else
            {
                obj2 = _generatedColumnOffsetDictionary[codeDomProvider.GetType()];
            }
            if (obj2 != null)
            {
                return (int) obj2;
            }
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace("ASP");
            compileUnit.Namespaces.Add(namespace2);
            CodeTypeDeclaration declaration = new CodeTypeDeclaration("ColumnOffsetCalculator") {
                IsClass = true
            };
            namespace2.Types.Add(declaration);
            CodeMemberMethod method = new CodeMemberMethod {
                ReturnType = new CodeTypeReference(typeof(void)),
                Name = "GenerateMethod"
            };
            declaration.Members.Add(method);
            CodeStatement statement = new CodeAssignStatement(new CodeVariableReferenceExpression("__o"), new CodeSnippetExpression("__dummyVar"));
            method.Statements.Add(statement);
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb, CultureInfo.InvariantCulture);
            codeDomProvider.GenerateCodeFromCompileUnit(compileUnit, writer, null);
            StringReader reader = new StringReader(sb.ToString());
            string str = null;
            int num = 4;
            while ((str = reader.ReadLine()) != null)
            {
                int index = 0;
                index = str.TrimStart(new char[0]).IndexOf("__dummyVar", StringComparison.Ordinal);
                if (index != -1)
                {
                    num = (index - "__o".Length) + 1;
                }
            }
            _generatedColumnOffsetDictionary[codeDomProvider.GetType()] = num;
            return num;
        }

        internal string GetInstantiatableFullTypeName()
        {
            if (this.PrecompilingForUpdatableDeployment)
            {
                return null;
            }
            return Util.MakeFullTypeName(this._sourceDataNamespace.Name, this._sourceDataClass.Name);
        }

        internal string GetIntermediateFullTypeName()
        {
            return Util.MakeFullTypeName(this.Parser.BaseTypeNamespace, this._intermediateClass.Name);
        }

        internal static bool IsAspNetNamespace(string ns)
        {
            return (ns == "ASP");
        }

        internal void SetDesignerMode()
        {
            this._designerMode = true;
        }

        protected CompilerParameters CompilParams
        {
            get
            {
                return this._compilParams;
            }
        }

        protected virtual bool IsGlobalAsaxGenerator
        {
            get
            {
                return false;
            }
        }

        internal IDictionary LinePragmasTable
        {
            get
            {
                return this._linePragmasTable;
            }
        }

        protected virtual bool NeedProfileProperty
        {
            get
            {
                return true;
            }
        }

        private TemplateParser Parser
        {
            get
            {
                return this._parser;
            }
        }

        private bool PrecompilingForUpdatableDeployment
        {
            get
            {
                if (this.IsGlobalAsaxGenerator)
                {
                    return false;
                }
                return BuildManager.PrecompilingForUpdatableDeployment;
            }
        }
    }
}

