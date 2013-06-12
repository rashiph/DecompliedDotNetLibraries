namespace System.Web.UI
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Util;

    public abstract class TemplateParser : BaseParser, IAssemblyDependencyParser
    {
        internal HttpStaticObjectsCollection _applicationObjects;
        private AssemblySet _assemblyDependencies;
        private Type _baseType;
        private string _baseTypeName;
        private string _baseTypeNamespace;
        private Stack _builderStack;
        private StringSet _circularReferenceChecker;
        private VirtualPath _codeFileVirtualPath;
        private CompilationSection _compConfig;
        private System.Web.UI.CompilationMode _compilationMode;
        private string _compilerOptions;
        private System.Web.Compilation.CompilerType _compilerType;
        private int _controlCount;
        private ScriptBlockData _currentScript;
        private IDesignerHost _designerHost;
        private EventHandler _designTimeDataBindHandler;
        private string _generatedClassName;
        private string _generatedNamespace;
        private string _id;
        private StringSet _idList;
        private Stack _idListStack;
        private ArrayList _implementedInterfaces;
        private IImplicitResourceProvider _implicitResourceProvider;
        internal int _lineNumber;
        private StringBuilder _literalBuilder;
        internal IDictionary _mainDirectiveConfigSettings;
        private Hashtable _namespaceEntries;
        private ArrayList _pageObjectList;
        internal PageParserFilter _pageParserFilter;
        private PagesSection _pagesConfig;
        private ParserErrorCollection _parserErrors;
        private ICollection _referencedAssemblies;
        private System.Web.UI.RootBuilder _rootBuilder;
        private ArrayList _scriptList;
        private int _scriptStartLineNumber;
        internal HttpStaticObjectsCollection _sessionObjects;
        private StringSet _sourceDependencies;
        private string _text;
        private HashCodeCombiner _typeHashCode = new HashCodeCombiner();
        private MainTagNameToTypeMapper _typeMapper;
        private ITypeResolutionService _typeResolutionService;
        private int _warningLevel = -1;
        internal const int aspCompatMode = 0x40;
        internal const int asyncMode = 0x800000;
        private const int attemptedImplicitResources = 0x40000;
        internal const int buffer = 0x80000;
        internal const int calledFromParseControlFlag = 0x4000000;
        internal const string CodeFileBaseClassAttributeName = "codefilebaseclass";
        private const int debug = 0x4000;
        internal SimpleBitVector32 flags;
        private const int hasCodeBehind = 0x80;
        private const int hasDebugAttribute = 0x2000;
        private const int ignoreControlProperties = 0x20;
        private const int ignoreNextSpaceString = 8;
        private const int ignoreParseErrors = 0x200;
        private const int ignoreParserFilter = 0x2000000;
        private const int ignoreScriptTag = 4;
        private const int inDesigner = 0x100;
        private const int inScriptTag = 2;
        private const int isServerTag = 1;
        private const int mainDirectiveHandled = 0x800;
        private const int mainDirectiveSpecified = 0x400;
        internal const int noAutoEventWireup = 0x20000;
        private const int noLinePragmas = 0x8000;
        internal const int readOnlySessionState = 0x200000;
        internal const int requiresCompilation = 0x10;
        internal const int requiresSessionState = 0x100000;
        private static char[] s_newlineChars = new char[] { '\r', '\n' };
        private const int strict = 0x10000;
        private const int throwOnFirstParseError = 0x1000000;
        private const int useExplicit = 0x1000;
        internal const int validateRequest = 0x400000;

        internal TemplateParser()
        {
            this.ThrowOnFirstParseError = true;
        }

        private void AddAssemblyDependencies(AssemblySet assemblyDependencies)
        {
            if (assemblyDependencies != null)
            {
                foreach (Assembly assembly in (IEnumerable) assemblyDependencies)
                {
                    this.AddAssemblyDependency(assembly);
                }
            }
        }

        internal void AddAssemblyDependency(Assembly assembly)
        {
            this.AddAssemblyDependency(assembly, false);
        }

        internal Assembly AddAssemblyDependency(string assemblyName)
        {
            return this.AddAssemblyDependency(assemblyName, false);
        }

        internal void AddAssemblyDependency(Assembly assembly, bool addDependentAssemblies)
        {
            if (this._assemblyDependencies == null)
            {
                this._assemblyDependencies = new AssemblySet();
            }
            if (this._typeResolutionService != null)
            {
                this._typeResolutionService.ReferenceAssembly(assembly.GetName());
            }
            this._assemblyDependencies.Add(assembly);
            if (addDependentAssemblies)
            {
                AssemblySet referencedAssemblies = System.Web.UI.Util.GetReferencedAssemblies(assembly);
                this.AddAssemblyDependencies(referencedAssemblies);
            }
        }

        internal Assembly AddAssemblyDependency(string assemblyName, bool addDependentAssemblies)
        {
            Assembly assembly = this.LoadAssembly(assemblyName, !this.FInDesigner);
            if (assembly != null)
            {
                this.AddAssemblyDependency(assembly, addDependentAssemblies);
            }
            return assembly;
        }

        private void AddBaseTypeDependencies(Type type)
        {
            Assembly assembly = type.Module.Assembly;
            if (((assembly != typeof(string).Assembly) && (assembly != typeof(Page).Assembly)) && (assembly != typeof(Uri).Assembly))
            {
                this.AddAssemblyDependency(assembly);
                if (type.BaseType != null)
                {
                    this.AddBaseTypeDependencies(type.BaseType);
                }
                foreach (Type type2 in type.GetInterfaces())
                {
                    this.AddBaseTypeDependencies(type2);
                }
            }
        }

        internal void AddBuildResultDependency(BuildResult result)
        {
            if (this._pageParserFilter != null)
            {
                this._pageParserFilter.OnDirectDependencyAdded();
            }
            if (result.VirtualPathDependencies != null)
            {
                foreach (string str in result.VirtualPathDependencies)
                {
                    if (this._pageParserFilter != null)
                    {
                        this._pageParserFilter.OnDependencyAdded();
                    }
                    this.AddSourceDependency2(VirtualPath.Create(str));
                }
            }
        }

        internal void AddControl(Type type, IDictionary attributes)
        {
            ControlBuilder parentBuilder = ((BuilderStackEntry) this.BuilderStack.Peek())._builder;
            ControlBuilder subBuilder = ControlBuilder.CreateBuilderFromType(this, parentBuilder, type, null, null, attributes, this._lineNumber, base.CurrentVirtualPath.VirtualPathString);
            this.AppendSubBuilder(parentBuilder, subBuilder);
        }

        internal void AddImportEntry(string ns)
        {
            if (this._namespaceEntries != null)
            {
                this._namespaceEntries = (Hashtable) this._namespaceEntries.Clone();
            }
            else
            {
                this._namespaceEntries = new Hashtable();
            }
            NamespaceEntry entry = new NamespaceEntry {
                Namespace = ns,
                Line = this._lineNumber,
                VirtualPath = base.CurrentVirtualPathString
            };
            this._namespaceEntries[ns] = entry;
        }

        private void AddLiteral(string literal)
        {
            if (this._literalBuilder == null)
            {
                this._literalBuilder = new StringBuilder();
            }
            this._literalBuilder.Append(literal);
        }

        internal void AddSourceDependency(VirtualPath fileName)
        {
            if (this._pageParserFilter != null)
            {
                this._pageParserFilter.OnDependencyAdded();
                this._pageParserFilter.OnDirectDependencyAdded();
            }
            this.AddSourceDependency2(fileName);
        }

        private void AddSourceDependency2(VirtualPath fileName)
        {
            if (this._sourceDependencies == null)
            {
                this._sourceDependencies = new CaseInsensitiveStringSet();
            }
            this._sourceDependencies.Add(fileName.VirtualPathString);
        }

        internal void AddTypeDependency(Type type)
        {
            this.AddBaseTypeDependencies(type);
            if ((type.Namespace != null) && BaseCodeDomTreeGenerator.IsAspNetNamespace(type.Namespace))
            {
                this.AddImportEntry(type.Namespace);
            }
        }

        private void AppendSubBuilder(ControlBuilder builder, ControlBuilder subBuilder)
        {
            if (subBuilder is ObjectTagBuilder)
            {
                this.ProcessObjectTag((ObjectTagBuilder) subBuilder);
            }
            else
            {
                builder.AppendSubBuilder(subBuilder);
            }
        }

        internal virtual void CheckObjectTagScope(ref ObjectTagScope scope)
        {
            if (scope == ObjectTagScope.Default)
            {
                scope = ObjectTagScope.Page;
            }
            if (scope != ObjectTagScope.Page)
            {
                throw new HttpException(System.Web.SR.GetString("App_session_only_valid_in_global_asax"));
            }
        }

        private static CodeConstructType CodeConstructTypeFromCodeBlockType(CodeBlockType blockType)
        {
            switch (blockType)
            {
                case CodeBlockType.Code:
                    return CodeConstructType.CodeSnippet;

                case CodeBlockType.Expression:
                    return CodeConstructType.ExpressionSnippet;

                case CodeBlockType.DataBinding:
                    return CodeConstructType.DataBindingSnippet;

                case CodeBlockType.EncodedExpression:
                    return CodeConstructType.EncodedExpressionSnippet;
            }
            return CodeConstructType.CodeSnippet;
        }

        internal virtual System.Web.UI.RootBuilder CreateDefaultFileLevelBuilder()
        {
            return new System.Web.UI.RootBuilder();
        }

        private static ParsedAttributeCollection CreateEmptyAttributeBag()
        {
            return new ParsedAttributeCollection();
        }

        private void CreateModifiedMainDirectiveFileIfNeeded(string text, Match match, ParsedAttributeCollection mainDirective, Encoding fileEncoding)
        {
            TextWriter updatableDeploymentTargetWriter = BuildManager.GetUpdatableDeploymentTargetWriter(base.CurrentVirtualPath, fileEncoding);
            if (updatableDeploymentTargetWriter != null)
            {
                using (updatableDeploymentTargetWriter)
                {
                    updatableDeploymentTargetWriter.Write(text.Substring(0, match.Index));
                    updatableDeploymentTargetWriter.Write("<%@ " + this.DefaultDirectiveName);
                    foreach (DictionaryEntry entry in (IEnumerable) mainDirective)
                    {
                        string key = (string) entry.Key;
                        string str2 = (string) entry.Value;
                        if (!StringUtil.EqualsIgnoreCase(key, "codefile") && !StringUtil.EqualsIgnoreCase(key, "codefilebaseclass"))
                        {
                            if (StringUtil.EqualsIgnoreCase(key, "inherits"))
                            {
                                str2 = "__ASPNET_INHERITS";
                            }
                            updatableDeploymentTargetWriter.Write(" ");
                            updatableDeploymentTargetWriter.Write(key);
                            updatableDeploymentTargetWriter.Write("=\"");
                            updatableDeploymentTargetWriter.Write(str2);
                            updatableDeploymentTargetWriter.Write("\"");
                        }
                    }
                    updatableDeploymentTargetWriter.Write(" %>");
                    updatableDeploymentTargetWriter.Write(text.Substring(match.Index + match.Length));
                }
            }
        }

        private void DetectSpecialServerTagError(string text, int textPos)
        {
            if (!this.IgnoreParseErrors)
            {
                if ((text.Length > (textPos + 1)) && (text[textPos + 1] == '%'))
                {
                    this.ProcessError(System.Web.SR.GetString("Malformed_server_block"));
                }
                else
                {
                    Match match = BaseParser.gtRegex.Match(text, textPos);
                    if (match.Success)
                    {
                        string input = text.Substring(textPos, (match.Index - textPos) + 2);
                        match = BaseParser.runatServerRegex.Match(input);
                        if (match.Success)
                        {
                            Match match2 = BaseParser.ltRegex.Match(input, 1);
                            if (!match2.Success || (match2.Index >= match.Index))
                            {
                                string str2 = BaseParser.serverTagsRegex.Replace(input, string.Empty);
                                if ((str2 != input) && base.TagRegex.Match(str2).Success)
                                {
                                    this.ProcessError(System.Web.SR.GetString("Server_tags_cant_contain_percent_constructs"));
                                }
                                else
                                {
                                    this.ProcessError(System.Web.SR.GetString("Malformed_server_tag"));
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void EnsureCodeAllowed()
        {
            if (!this.IsCodeAllowed)
            {
                this.ProcessError(System.Web.SR.GetString("Code_not_allowed"));
            }
            this.flags[0x10] = true;
        }

        private void EnsureRootBuilderCreated()
        {
            if (this._rootBuilder == null)
            {
                if (this.BaseType == this.DefaultBaseType)
                {
                    this._rootBuilder = this.CreateDefaultFileLevelBuilder();
                }
                else
                {
                    Type fileLevelControlBuilderType = this.GetFileLevelControlBuilderType();
                    if (fileLevelControlBuilderType == null)
                    {
                        this._rootBuilder = this.CreateDefaultFileLevelBuilder();
                    }
                    else
                    {
                        this._rootBuilder = (System.Web.UI.RootBuilder) HttpRuntime.CreateNonPublicInstance(fileLevelControlBuilderType);
                    }
                }
                this._rootBuilder.Line = 1;
                this._rootBuilder.Init(this, null, null, null, null, null);
                this._rootBuilder.SetTypeMapper(this.TypeMapper);
                this._rootBuilder.VirtualPath = base.CurrentVirtualPath;
                this._builderStack = new Stack();
                this._builderStack.Push(new BuilderStackEntry(this.RootBuilder, null, null, 0, null, 0));
            }
        }

        private Type GetFileLevelControlBuilderType()
        {
            FileLevelControlBuilderAttribute attribute = null;
            object[] customAttributes = this.BaseType.GetCustomAttributes(typeof(FileLevelControlBuilderAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                attribute = (FileLevelControlBuilderAttribute) customAttributes[0];
            }
            if (attribute == null)
            {
                return null;
            }
            System.Web.UI.Util.CheckAssignableType(this.DefaultFileLevelBuilderType, attribute.BuilderType);
            return attribute.BuilderType;
        }

        internal IImplicitResourceProvider GetImplicitResourceProvider()
        {
            if (this.FInDesigner)
            {
                return null;
            }
            if (!this.flags[0x40000])
            {
                this.flags[0x40000] = true;
                IResourceProvider localResourceProvider = ResourceExpressionBuilder.GetLocalResourceProvider(this._rootBuilder.VirtualPath);
                if (localResourceProvider == null)
                {
                    return null;
                }
                this._implicitResourceProvider = localResourceProvider as IImplicitResourceProvider;
                if (this._implicitResourceProvider == null)
                {
                    this._implicitResourceProvider = new DefaultImplicitResourceProvider(localResourceProvider);
                }
            }
            return this._implicitResourceProvider;
        }

        private string GetLiteral()
        {
            if (this._literalBuilder == null)
            {
                return null;
            }
            return this._literalBuilder.ToString();
        }

        internal Type GetType(string typeName)
        {
            return this.GetType(typeName, false);
        }

        internal Type GetType(string typeName, bool ignoreCase)
        {
            return this.GetType(typeName, ignoreCase, true);
        }

        internal Type GetType(string typeName, bool ignoreCase, bool throwOnError)
        {
            Assembly assembly = null;
            int length = System.Web.UI.Util.CommaIndexInTypeName(typeName);
            if (length > 0)
            {
                string assemblyName = typeName.Substring(length + 1).Trim();
                typeName = typeName.Substring(0, length).Trim();
                try
                {
                    assembly = this.LoadAssembly(assemblyName, !this.FInDesigner);
                }
                catch
                {
                    throw new HttpException(System.Web.SR.GetString("Assembly_not_compiled", new object[] { assemblyName }));
                }
            }
            if (assembly != null)
            {
                return assembly.GetType(typeName, throwOnError, ignoreCase);
            }
            Type type = System.Web.UI.Util.GetTypeFromAssemblies(this._referencedAssemblies, typeName, ignoreCase);
            if (type != null)
            {
                return type;
            }
            type = System.Web.UI.Util.GetTypeFromAssemblies(this.AssemblyDependencies, typeName, ignoreCase);
            if (type != null)
            {
                return type;
            }
            if (throwOnError)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_type", new object[] { typeName }));
            }
            return null;
        }

        internal virtual void HandlePostParse()
        {
            if (!this.flags[0x800])
            {
                this.ProcessMainDirective(this._mainDirectiveConfigSettings);
                this.flags[0x800] = true;
            }
            if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowBaseType(this.BaseType))
            {
                throw new HttpException(System.Web.SR.GetString("Base_type_not_allowed", new object[] { this.BaseType.FullName }));
            }
            if (this.BuilderStack.Count > 1)
            {
                BuilderStackEntry entry = (BuilderStackEntry) this._builderStack.Peek();
                string message = System.Web.SR.GetString("Unexpected_eof_looking_for_tag", new object[] { entry._tagName });
                this.ProcessException(new HttpParseException(message, null, entry.VirtualPath, entry._inputText, entry.Line));
            }
            else
            {
                if (this._compilerType == null)
                {
                    if (!this.FInDesigner)
                    {
                        this._compilerType = CompilationUtil.GetDefaultLanguageCompilerInfo(this._compConfig, base.CurrentVirtualPath);
                    }
                    else
                    {
                        this._compilerType = CompilationUtil.GetCodeDefaultLanguageCompilerInfo();
                    }
                }
                CompilerParameters compilerParameters = this._compilerType.CompilerParameters;
                if (this.flags[0x2000])
                {
                    compilerParameters.IncludeDebugInformation = this.flags[0x4000];
                }
                if (compilerParameters.IncludeDebugInformation)
                {
                    HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "Debugging_not_supported_in_low_trust");
                }
                if (this._warningLevel >= 0)
                {
                    compilerParameters.WarningLevel = this._warningLevel;
                    compilerParameters.TreatWarningsAsErrors = this._warningLevel > 0;
                }
                if (this._compilerOptions != null)
                {
                    compilerParameters.CompilerOptions = this._compilerOptions;
                }
                if (this._pageParserFilter != null)
                {
                    this._pageParserFilter.ParseComplete(this.RootBuilder);
                }
            }
        }

        private Assembly ImportSourceFile(VirtualPath virtualPath)
        {
            if (this.CompilationMode == System.Web.UI.CompilationMode.Never)
            {
                return null;
            }
            virtualPath = base.ResolveVirtualPath(virtualPath);
            if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowVirtualReference(this.CompConfig, virtualPath))
            {
                this.ProcessError(System.Web.SR.GetString("Reference_not_allowed", new object[] { virtualPath }));
            }
            this.AddSourceDependency(virtualPath);
            BuildResultCompiledAssembly vPathBuildResult = BuildManager.GetVPathBuildResult(virtualPath) as BuildResultCompiledAssembly;
            if (vPathBuildResult == null)
            {
                this.ProcessError(System.Web.SR.GetString("Not_a_src_file", new object[] { virtualPath }));
            }
            Assembly resultAssembly = vPathBuildResult.ResultAssembly;
            this.AddAssemblyDependency(resultAssembly, true);
            return resultAssembly;
        }

        internal bool IsExpressionBuilderValue(string val)
        {
            return ControlBuilder.expressionBuilderRegex.Match(val, 0).Success;
        }

        internal Assembly LoadAssembly(string assemblyName, bool throwOnFail)
        {
            if (this._typeResolutionService != null)
            {
                AssemblyName name = new AssemblyName(assemblyName);
                return this._typeResolutionService.GetAssembly(name, throwOnFail);
            }
            return this._compConfig.LoadAssembly(assemblyName, throwOnFail);
        }

        internal Type MapStringToType(string typeName, IDictionary attribs)
        {
            return this.RootBuilder.GetChildControlType(typeName, attribs);
        }

        private bool MaybeTerminateControl(string tagName, int textPos)
        {
            BuilderStackEntry entry = (BuilderStackEntry) this.BuilderStack.Peek();
            ControlBuilder subBuilder = entry._builder;
            if ((entry._tagName == null) || !StringUtil.EqualsIgnoreCase(entry._tagName, tagName))
            {
                return false;
            }
            if (entry._repeatCount > 0)
            {
                entry._repeatCount--;
                return false;
            }
            this.ProcessLiteral();
            if (subBuilder.NeedsTagInnerText())
            {
                try
                {
                    subBuilder.SetTagInnerText(entry._inputText.Substring(entry._textPos, textPos - entry._textPos));
                }
                catch (Exception exception)
                {
                    if (!this.IgnoreParseErrors)
                    {
                        this._lineNumber = subBuilder.Line;
                        this.ProcessException(exception);
                        return true;
                    }
                }
            }
            if ((subBuilder is TemplateBuilder) && ((TemplateBuilder) subBuilder).AllowMultipleInstances)
            {
                this._idList = (StringSet) this._idListStack.Pop();
            }
            this._builderStack.Pop();
            this.AppendSubBuilder(((BuilderStackEntry) this._builderStack.Peek())._builder, subBuilder);
            subBuilder.CloseControl();
            return true;
        }

        internal void OnFoundAttributeRequiringCompilation(string attribName)
        {
            if (!this.IsCodeAllowed)
            {
                this.ProcessError(System.Web.SR.GetString("Attrib_not_allowed", new object[] { attribName }));
            }
            this.flags[0x10] = true;
        }

        internal void OnFoundDirectiveRequiringCompilation(string directiveName)
        {
            if (!this.IsCodeAllowed)
            {
                this.ProcessError(System.Web.SR.GetString("Directive_not_allowed", new object[] { directiveName }));
            }
            this.flags[0x10] = true;
        }

        internal void OnFoundEventHandler(string directiveName)
        {
            if (!this.IsCodeAllowed)
            {
                this.ProcessError(System.Web.SR.GetString("Event_not_allowed", new object[] { directiveName }));
            }
            this.flags[0x10] = true;
        }

        private bool PageParserFilterProcessedCodeBlock(CodeConstructType codeConstructType, string code, int lineNumber)
        {
            bool flag;
            if ((this._pageParserFilter == null) || (this.CompilationMode == System.Web.UI.CompilationMode.Never))
            {
                return false;
            }
            int num = this._lineNumber;
            this._lineNumber = lineNumber;
            try
            {
                flag = this._pageParserFilter.ProcessCodeConstruct(codeConstructType, code);
            }
            finally
            {
                this._lineNumber = num;
            }
            return flag;
        }

        internal bool PageParserFilterProcessedDataBindingAttribute(string controlId, string attributeName, string code)
        {
            return (((this._pageParserFilter != null) && (this.CompilationMode != System.Web.UI.CompilationMode.Never)) && this._pageParserFilter.ProcessDataBindingAttribute(controlId, attributeName, code));
        }

        internal bool PageParserFilterProcessedEventHookupAttribute(string controlId, string eventName, string handlerName)
        {
            return (((this._pageParserFilter != null) && (this.CompilationMode != System.Web.UI.CompilationMode.Never)) && this._pageParserFilter.ProcessEventHookup(controlId, eventName, handlerName));
        }

        internal void Parse()
        {
            Thread currentThread = Thread.CurrentThread;
            CultureInfo currentCulture = currentThread.CurrentCulture;
            currentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                try
                {
                    this.PrepareParse();
                    this.ParseInternal();
                    this.HandlePostParse();
                }
                finally
                {
                    currentThread.CurrentCulture = currentCulture;
                }
            }
            catch
            {
                throw;
            }
        }

        internal void Parse(ICollection referencedAssemblies, VirtualPath virtualPath)
        {
            this._referencedAssemblies = referencedAssemblies;
            base.CurrentVirtualPath = virtualPath;
            this.Parse();
        }

        internal static Control ParseControl(string content, VirtualPath virtualPath, bool ignoreFilter)
        {
            if (content == null)
            {
                return null;
            }
            ITemplate template = ParseTemplate(content, virtualPath, ignoreFilter);
            Control container = new Control();
            template.InstantiateIn(container);
            return container;
        }

        protected void ParseFile(string physicalPath, string virtualPath)
        {
            this.ParseFile(physicalPath, VirtualPath.Create(virtualPath));
        }

        internal void ParseFile(string physicalPath, VirtualPath virtualPath)
        {
            string o = (physicalPath != null) ? physicalPath : virtualPath.VirtualPathString;
            if (this._circularReferenceChecker.Contains(o))
            {
                this.ProcessError(System.Web.SR.GetString("Circular_include"));
            }
            else
            {
                this._circularReferenceChecker.Add(o);
                try
                {
                    StreamReader reader;
                    if (physicalPath != null)
                    {
                        using (reader = System.Web.UI.Util.ReaderFromFile(physicalPath, base.CurrentVirtualPath))
                        {
                            this.ParseReader(reader, virtualPath);
                            return;
                        }
                    }
                    using (Stream stream = virtualPath.OpenFile())
                    {
                        reader = System.Web.UI.Util.ReaderFromStream(stream, base.CurrentVirtualPath);
                        this.ParseReader(reader, virtualPath);
                    }
                }
                finally
                {
                    this._circularReferenceChecker.Remove(o);
                }
            }
        }

        internal virtual void ParseInternal()
        {
            if (this._text != null)
            {
                this.ParseString(this._text, base.CurrentVirtualPath, Encoding.UTF8);
            }
            else
            {
                this.AddSourceDependency(base.CurrentVirtualPath);
                this.ParseFile(null, base.CurrentVirtualPath.VirtualPathString);
            }
        }

        private void ParseReader(StreamReader reader, VirtualPath virtualPath)
        {
            string text = reader.ReadToEnd();
            this._text = text;
            this.ParseString(text, virtualPath, reader.CurrentEncoding);
        }

        internal void ParseString(string text, VirtualPath virtualPath, Encoding fileEncoding)
        {
            VirtualPath currentVirtualPath = base.CurrentVirtualPath;
            int num = this._lineNumber;
            base.CurrentVirtualPath = virtualPath;
            this._lineNumber = 1;
            this.flags[8] = true;
            try
            {
                this.ParseStringInternal(text, fileEncoding);
                if (this.HasParserErrors)
                {
                    ParserError error = this.ParserErrors[0];
                    Exception innerException = error.Exception;
                    if (innerException == null)
                    {
                        innerException = new HttpException(error.ErrorText);
                    }
                    HttpParseException exception2 = new HttpParseException(error.ErrorText, innerException, error.VirtualPath, this.Text, error.Line);
                    for (int i = 1; i < this.ParserErrors.Count; i++)
                    {
                        exception2.ParserErrors.Add(this.ParserErrors[i]);
                    }
                    throw exception2;
                }
                this.ThrowOnFirstParseError = true;
            }
            catch (Exception exception3)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_PRE_PROCESSING);
                PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
                if (HttpException.GetErrorFormatter(exception3) == null)
                {
                    throw new HttpParseException(exception3.Message, exception3, base.CurrentVirtualPath, text, this._lineNumber);
                }
                throw;
            }
            finally
            {
                base.CurrentVirtualPath = currentVirtualPath;
                this._lineNumber = num;
            }
        }

        private void ParseStringInternal(string text, Encoding fileEncoding)
        {
            Match match;
            int startat = 0;
            int num2 = text.LastIndexOf('>');
            Regex tagRegex = base.TagRegex;
        Label_0012:
            if ((match = BaseParser.textRegex.Match(text, startat)).Success)
            {
                this.AddLiteral(match.ToString());
                this._lineNumber += System.Web.UI.Util.LineCount(text, startat, match.Index + match.Length);
                startat = match.Index + match.Length;
            }
            if (startat == text.Length)
            {
                goto Label_0332;
            }
            bool flag = false;
            if (!this.flags[2] && (match = BaseParser.directiveRegex.Match(text, startat)).Success)
            {
                ParsedAttributeCollection attributes;
                string str;
                this.ProcessLiteral();
                string directiveName = this.ProcessAttributes(match, out attributes, true, out str);
                try
                {
                    this.PreprocessDirective(directiveName, attributes);
                    this.ProcessDirective(directiveName, attributes);
                }
                catch (Exception exception)
                {
                    this.ProcessException(exception);
                }
                if ((directiveName.Length == 0) && (this._codeFileVirtualPath != null))
                {
                    this.CreateModifiedMainDirectiveFileIfNeeded(text, match, attributes, fileEncoding);
                }
                this.flags[8] = true;
            }
            else
            {
                if ((match = BaseParser.includeRegex.Match(text, startat)).Success)
                {
                    try
                    {
                        this.ProcessServerInclude(match);
                        goto Label_02BD;
                    }
                    catch (Exception exception2)
                    {
                        this.ProcessException(exception2);
                        goto Label_02BD;
                    }
                }
                if (!(match = BaseParser.commentRegex.Match(text, startat)).Success)
                {
                    if (!this.flags[2] && (match = BaseParser.aspExprRegex.Match(text, startat)).Success)
                    {
                        this.ProcessCodeBlock(match, CodeBlockType.Expression, text);
                    }
                    else if (!this.flags[2] && (match = BaseParser.aspEncodedExprRegex.Match(text, startat)).Success)
                    {
                        this.ProcessCodeBlock(match, CodeBlockType.EncodedExpression, text);
                    }
                    else if (!this.flags[2] && (match = BaseParser.databindExprRegex.Match(text, startat)).Success)
                    {
                        this.ProcessCodeBlock(match, CodeBlockType.DataBinding, text);
                    }
                    else if (!this.flags[2] && (match = BaseParser.aspCodeRegex.Match(text, startat)).Success)
                    {
                        string str3 = match.Groups["code"].Value.Trim();
                        if (str3.StartsWith("$", StringComparison.Ordinal))
                        {
                            this.ProcessError(System.Web.SR.GetString("ExpressionBuilder_LiteralExpressionsNotAllowed", new object[] { match.ToString(), str3 }));
                        }
                        else
                        {
                            this.ProcessCodeBlock(match, CodeBlockType.Code, text);
                        }
                    }
                    else
                    {
                        if ((!this.flags[2] && (num2 > startat)) && (match = tagRegex.Match(text, startat)).Success)
                        {
                            try
                            {
                                if (!this.ProcessBeginTag(match, text))
                                {
                                    flag = true;
                                }
                                goto Label_02BD;
                            }
                            catch (Exception exception3)
                            {
                                this.ProcessException(exception3);
                                goto Label_02BD;
                            }
                        }
                        if ((match = BaseParser.endtagRegex.Match(text, startat)).Success && !this.ProcessEndTag(match))
                        {
                            flag = true;
                        }
                    }
                }
            }
        Label_02BD:
            if (((match == null) || !match.Success) || flag)
            {
                if (!flag && !this.flags[2])
                {
                    this.DetectSpecialServerTagError(text, startat);
                }
                startat++;
                this.AddLiteral("<");
            }
            else
            {
                this._lineNumber += System.Web.UI.Util.LineCount(text, startat, match.Index + match.Length);
                startat = match.Index + match.Length;
            }
            if (startat != text.Length)
            {
                goto Label_0012;
            }
        Label_0332:
            if (this.flags[2] && !this.IgnoreParseErrors)
            {
                this._lineNumber = this._scriptStartLineNumber;
                this.ProcessError(System.Web.SR.GetString("Unexpected_eof_looking_for_tag", new object[] { "script" }));
            }
            else
            {
                this.ProcessLiteral();
            }
        }

        private static ITemplate ParseTemplate(string content, VirtualPath virtualPath, bool ignoreFilter)
        {
            TemplateParser parser = new UserControlParser();
            return parser.ParseTemplateInternal(content, virtualPath, ignoreFilter);
        }

        private ITemplate ParseTemplateInternal(string content, VirtualPath virtualPath, bool ignoreFilter)
        {
            base.CurrentVirtualPath = virtualPath;
            this.CompilationMode = System.Web.UI.CompilationMode.Never;
            this._text = content;
            this.flags[0x2000000] = ignoreFilter;
            this.flags[0x4000000] = true;
            this.Parse();
            return this.RootBuilder;
        }

        internal virtual void PostProcessMainDirectiveAttributes(IDictionary parseData)
        {
            string virtualPath = (string) parseData["src"];
            Assembly assembly = null;
            if (virtualPath != null)
            {
                try
                {
                    assembly = this.ImportSourceFile(VirtualPath.Create(virtualPath));
                }
                catch (Exception exception)
                {
                    this.ProcessException(exception);
                }
            }
            string codeFileBaseTypeName = (string) parseData["codefilebaseclass"];
            if ((codeFileBaseTypeName != null) && (this._codeFileVirtualPath == null))
            {
                throw new HttpException(System.Web.SR.GetString("CodeFileBaseClass_Without_Codefile"));
            }
            string baseTypeName = (string) parseData["inherits"];
            if (baseTypeName != null)
            {
                try
                {
                    this.ProcessInheritsAttribute(baseTypeName, codeFileBaseTypeName, virtualPath, assembly);
                    return;
                }
                catch (Exception exception2)
                {
                    this.ProcessException(exception2);
                    return;
                }
            }
            if (this._codeFileVirtualPath != null)
            {
                throw new HttpException(System.Web.SR.GetString("Codefile_without_inherits"));
            }
        }

        internal virtual void PrepareParse()
        {
            if (this._circularReferenceChecker == null)
            {
                this._circularReferenceChecker = new CaseInsensitiveStringSet();
            }
            this._baseType = this.DefaultBaseType;
            this._mainDirectiveConfigSettings = CreateEmptyAttributeBag();
            if (!this.FInDesigner)
            {
                this._compConfig = MTConfigUtil.GetCompilationConfig(base.CurrentVirtualPath);
                this._pagesConfig = MTConfigUtil.GetPagesConfig(base.CurrentVirtualPath);
            }
            this.ProcessConfigSettings();
            this._typeMapper = new MainTagNameToTypeMapper(this as BaseTemplateParser);
            this._typeMapper.RegisterTag("object", typeof(ObjectTag));
            this._sourceDependencies = new CaseInsensitiveStringSet();
            this._idListStack = new Stack();
            this._idList = new CaseInsensitiveStringSet();
            this._scriptList = new ArrayList();
        }

        internal void PreprocessDirective(string directiveName, IDictionary directive)
        {
            if (this._pageParserFilter != null)
            {
                if (directiveName.Length == 0)
                {
                    directiveName = this.DefaultDirectiveName;
                }
                this._pageParserFilter.PreprocessDirective(directiveName, directive);
            }
        }

        private string ProcessAttributes(Match match, out ParsedAttributeCollection attribs, bool fDirective, out string duplicateAttribute)
        {
            string strA = string.Empty;
            attribs = CreateEmptyAttributeBag();
            CaptureCollection captures = match.Groups["attrname"].Captures;
            CaptureCollection captures2 = match.Groups["attrval"].Captures;
            CaptureCollection captures3 = null;
            if (fDirective)
            {
                captures3 = match.Groups["equal"].Captures;
            }
            this.flags[1] = false;
            this._id = null;
            duplicateAttribute = null;
            for (int i = 0; i < captures.Count; i++)
            {
                string input = captures[i].ToString();
                if (fDirective)
                {
                    input = input.ToLower(CultureInfo.InvariantCulture);
                }
                string s = captures2[i].ToString();
                string propName = string.Empty;
                string deviceName = System.Web.UI.Util.ParsePropertyDeviceFilter(input, out propName);
                s = HttpUtility.HtmlDecode(s);
                bool flag = false;
                if (fDirective)
                {
                    flag = captures3[i].ToString().Length > 0;
                }
                if (StringUtil.EqualsIgnoreCase(propName, "id"))
                {
                    this._id = s;
                }
                else if (StringUtil.EqualsIgnoreCase(propName, "runat"))
                {
                    this.ValidateBuiltInAttribute(deviceName, propName, s);
                    if (!StringUtil.EqualsIgnoreCase(s, "server"))
                    {
                        this.ProcessError(System.Web.SR.GetString("Runat_can_only_be_server"));
                    }
                    this.flags[1] = true;
                    input = null;
                }
                else if (this.FInDesigner && StringUtil.EqualsIgnoreCase(propName, "ignoreParentFrozen"))
                {
                    input = null;
                }
                if (input != null)
                {
                    if ((fDirective && !flag) && (i == 0))
                    {
                        strA = input;
                        if (string.Compare(strA, this.DefaultDirectiveName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            strA = string.Empty;
                        }
                    }
                    else
                    {
                        try
                        {
                            if ((fDirective && (strA.Length > 0)) && (deviceName.Length > 0))
                            {
                                this.ProcessError(System.Web.SR.GetString("Device_unsupported_in_directive", new object[] { strA }));
                            }
                            else
                            {
                                attribs.AddFilteredAttribute(deviceName, propName, s);
                            }
                        }
                        catch (ArgumentException)
                        {
                            duplicateAttribute = input;
                        }
                        catch (Exception exception)
                        {
                            this.ProcessException(exception);
                        }
                    }
                }
            }
            if ((duplicateAttribute != null) && fDirective)
            {
                this.ProcessError(System.Web.SR.GetString("Duplicate_attr_in_directive", new object[] { duplicateAttribute }));
            }
            return strA;
        }

        private bool ProcessBeginTag(Match match, string inputText)
        {
            ParsedAttributeCollection attributes;
            string str2;
            string str3;
            string str = match.Groups["tagname"].Value;
            this.ProcessAttributes(match, out attributes, false, out str2);
            bool success = match.Groups["empty"].Success;
            if (StringUtil.EqualsIgnoreCase(str, "script") && this.flags[1])
            {
                this.ProcessScriptTag(match, inputText, attributes, success);
                return true;
            }
            if (!this.flags[0x800])
            {
                this.ProcessMainDirective(this._mainDirectiveConfigSettings);
                this.flags[0x800] = true;
            }
            ControlBuilder parentBuilder = null;
            ControlBuilder builder = null;
            Type childType = null;
            string filter = System.Web.UI.Util.ParsePropertyDeviceFilter(str, out str3);
            if (this.BuilderStack.Count > 1)
            {
                parentBuilder = ((BuilderStackEntry) this._builderStack.Peek())._builder;
                if (parentBuilder is StringPropertyBuilder)
                {
                    return false;
                }
                builder = parentBuilder.CreateChildBuilder(filter, str3, attributes, this, parentBuilder, this._id, this._lineNumber, base.CurrentVirtualPath, ref childType, false);
            }
            if ((builder == null) && this.flags[1])
            {
                builder = this.RootBuilder.CreateChildBuilder(filter, str3, attributes, this, parentBuilder, this._id, this._lineNumber, base.CurrentVirtualPath, ref childType, false);
            }
            if (((builder == null) && (this._builderStack.Count > 1)) && !success)
            {
                BuilderStackEntry entry = (BuilderStackEntry) this._builderStack.Peek();
                if (StringUtil.EqualsIgnoreCase(str, entry._tagName))
                {
                    entry._repeatCount++;
                }
            }
            if (builder == null)
            {
                if (!this.flags[1] || this.IgnoreParseErrors)
                {
                    return false;
                }
                this.ProcessError(System.Web.SR.GetString("Unknown_server_tag", new object[] { str }));
                return true;
            }
            if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowControlInternal(childType, builder))
            {
                this.ProcessError(System.Web.SR.GetString("Control_type_not_allowed", new object[] { childType.FullName }));
                return true;
            }
            if (str2 != null)
            {
                this.ProcessError(System.Web.SR.GetString("Duplicate_attr_in_tag", new object[] { str2 }));
            }
            this._id = builder.ID;
            if (this._id != null)
            {
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(this._id))
                {
                    this.ProcessError(System.Web.SR.GetString("Invalid_identifier", new object[] { this._id }));
                    return true;
                }
                if (this._idList.Contains(this._id))
                {
                    this.ProcessError(System.Web.SR.GetString("Id_already_used", new object[] { this._id }));
                    return true;
                }
                this._idList.Add(this._id);
            }
            else if (this.flags[1])
            {
                PartialCachingAttribute attribute = (PartialCachingAttribute) TypeDescriptor.GetAttributes(childType)[typeof(PartialCachingAttribute)];
                if (!(builder.Parser is PageThemeParser) && (attribute != null))
                {
                    this._id = "_ctrl_" + this._controlCount.ToString(NumberFormatInfo.InvariantInfo);
                    builder.ID = this._id;
                    this._controlCount++;
                    builder.PreprocessAttribute(string.Empty, "id", this._id, false);
                }
            }
            this.ProcessLiteral();
            if (childType != null)
            {
                this.UpdateTypeHashCode(childType.FullName);
            }
            if (!success && builder.HasBody())
            {
                if ((builder is TemplateBuilder) && ((TemplateBuilder) builder).AllowMultipleInstances)
                {
                    this._idListStack.Push(this._idList);
                    this._idList = new CaseInsensitiveStringSet();
                }
                this._builderStack.Push(new BuilderStackEntry(builder, str, base.CurrentVirtualPathString, this._lineNumber, inputText, match.Index + match.Length));
            }
            else
            {
                parentBuilder = ((BuilderStackEntry) this._builderStack.Peek())._builder;
                this.AppendSubBuilder(parentBuilder, builder);
                builder.CloseControl();
            }
            return true;
        }

        private void ProcessCodeBlock(Match match, CodeBlockType blockType, string text)
        {
            this.ProcessLiteral();
            Group group = match.Groups["code"];
            string s = group.Value.Replace(@"%\>", "%>");
            int lineNumber = this._lineNumber;
            int column = -1;
            if (blockType != CodeBlockType.Code)
            {
                int length = -1;
                for (int i = 0; (i < s.Length) && char.IsWhiteSpace(s[i]); i++)
                {
                    if ((s[i] == '\r') || ((s[i] == '\n') && ((i == 0) || (s[i - 1] != '\r'))))
                    {
                        lineNumber++;
                        length = i;
                    }
                    else if (s[i] == '\n')
                    {
                        length = i;
                    }
                }
                if (length >= 0)
                {
                    s = s.Substring(length + 1);
                    column = 1;
                }
                length = -1;
                for (int j = s.Length - 1; (j >= 0) && char.IsWhiteSpace(s[j]); j--)
                {
                    if ((s[j] == '\r') || (s[j] == '\n'))
                    {
                        length = j;
                    }
                }
                if (length >= 0)
                {
                    s = s.Substring(0, length);
                }
                if (!this.IgnoreParseErrors && System.Web.UI.Util.IsWhiteSpaceString(s))
                {
                    this.ProcessError(System.Web.SR.GetString("Empty_expression"));
                    return;
                }
            }
            if (column < 0)
            {
                int num6 = text.LastIndexOfAny(s_newlineChars, group.Index - 1);
                column = group.Index - num6;
            }
            ControlBuilder builder = ((BuilderStackEntry) this.BuilderStack.Peek())._builder;
            if (!this.PageParserFilterProcessedCodeBlock(CodeConstructTypeFromCodeBlockType(blockType), s, lineNumber))
            {
                this.EnsureCodeAllowed();
                ControlBuilder subBuilder = new CodeBlockBuilder(blockType, s, lineNumber, column, base.CurrentVirtualPath);
                this.AppendSubBuilder(builder, subBuilder);
            }
            if (blockType == CodeBlockType.Code)
            {
                this.flags[8] = true;
            }
        }

        private void ProcessCodeFile(VirtualPath codeFileVirtualPath)
        {
            this._codeFileVirtualPath = base.ResolveVirtualPath(codeFileVirtualPath);
            System.Web.Compilation.CompilerType compilerInfoFromVirtualPath = CompilationUtil.GetCompilerInfoFromVirtualPath(this._codeFileVirtualPath);
            if ((this._compilerType != null) && (this._compilerType.CodeDomProviderType != compilerInfoFromVirtualPath.CodeDomProviderType))
            {
                this.ProcessError(System.Web.SR.GetString("Inconsistent_CodeFile_Language"));
            }
            else
            {
                BuildManager.ValidateCodeFileVirtualPath(this._codeFileVirtualPath);
                System.Web.UI.Util.CheckVirtualFileExists(this._codeFileVirtualPath);
                this._compilerType = compilerInfoFromVirtualPath;
                this.AddSourceDependency(this._codeFileVirtualPath);
            }
        }

        internal virtual void ProcessConfigSettings()
        {
            if (this._compConfig != null)
            {
                this.flags[0x1000] = this._compConfig.Explicit;
                this.flags[0x10000] = this._compConfig.Strict;
            }
            if (this.PagesConfig != null)
            {
                this._namespaceEntries = this.PagesConfig.Namespaces.NamespaceEntries;
                if (this._namespaceEntries != null)
                {
                    this._namespaceEntries = (Hashtable) this._namespaceEntries.Clone();
                }
                if (!this.flags[0x2000000])
                {
                    this._pageParserFilter = PageParserFilter.Create(this.PagesConfig, base.CurrentVirtualPath, this);
                }
            }
        }

        internal virtual void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (directiveName.Length == 0)
            {
                if (!this.FInDesigner)
                {
                    if (this.flags[0x400])
                    {
                        this.ProcessError(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { this.DefaultDirectiveName }));
                    }
                    else
                    {
                        if (this._mainDirectiveConfigSettings != null)
                        {
                            foreach (DictionaryEntry entry in this._mainDirectiveConfigSettings)
                            {
                                if (!directive.Contains(entry.Key))
                                {
                                    directive[entry.Key] = entry.Value;
                                }
                            }
                        }
                        this.ProcessMainDirective(directive);
                        this.flags[0x400] = true;
                        this.flags[0x800] = true;
                    }
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly"))
            {
                string andRemoveNonEmptyAttribute = System.Web.UI.Util.GetAndRemoveNonEmptyAttribute(directive, "name");
                VirtualPath andRemoveVirtualPathAttribute = System.Web.UI.Util.GetAndRemoveVirtualPathAttribute(directive, "src");
                System.Web.UI.Util.CheckUnknownDirectiveAttributes(directiveName, directive);
                if ((andRemoveNonEmptyAttribute != null) && (andRemoveVirtualPathAttribute != null))
                {
                    this.ProcessError(System.Web.SR.GetString("Attributes_mutually_exclusive", new object[] { "Name", "Src" }));
                }
                if (andRemoveNonEmptyAttribute != null)
                {
                    this.AddAssemblyDependency(andRemoveNonEmptyAttribute);
                }
                else if (andRemoveVirtualPathAttribute != null)
                {
                    this.ImportSourceFile(andRemoveVirtualPathAttribute);
                }
                else
                {
                    this.ProcessError(System.Web.SR.GetString("Missing_attr", new object[] { "name" }));
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "import"))
            {
                this.ProcessImportDirective(directiveName, directive);
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "implements"))
            {
                this.OnFoundDirectiveRequiringCompilation(directiveName);
                string andRemoveRequiredAttribute = System.Web.UI.Util.GetAndRemoveRequiredAttribute(directive, "interface");
                System.Web.UI.Util.CheckUnknownDirectiveAttributes(directiveName, directive);
                Type type = this.GetType(andRemoveRequiredAttribute);
                if (!type.IsInterface)
                {
                    this.ProcessError(System.Web.SR.GetString("Invalid_type_to_implement", new object[] { andRemoveRequiredAttribute }));
                }
                else
                {
                    if (this._implementedInterfaces == null)
                    {
                        this._implementedInterfaces = new ArrayList();
                    }
                    this._implementedInterfaces.Add(type);
                }
            }
            else if (!this.FInDesigner)
            {
                this.ProcessError(System.Web.SR.GetString("Unknown_directive", new object[] { directiveName }));
            }
        }

        private bool ProcessEndTag(Match match)
        {
            string tagName = match.Groups["tagname"].Value;
            if (!this.flags[2])
            {
                return this.MaybeTerminateControl(tagName, match.Index);
            }
            if (!StringUtil.EqualsIgnoreCase(tagName, "script"))
            {
                return false;
            }
            this.ProcessServerScript();
            this.flags[2] = false;
            this.flags[4] = false;
            return true;
        }

        protected void ProcessError(string message)
        {
            if (!this.IgnoreParseErrors)
            {
                if (this.ThrowOnFirstParseError)
                {
                    throw new HttpException(message);
                }
                ParserError error = new ParserError(message, base.CurrentVirtualPath, this._lineNumber);
                this.ParserErrors.Add(error);
                BuildManager.ReportParseError(error);
            }
        }

        protected void ProcessException(Exception ex)
        {
            if (!this.IgnoreParseErrors)
            {
                ParserError error;
                if (this.ThrowOnFirstParseError || (ex is HttpCompileException))
                {
                    if (ex is HttpParseException)
                    {
                        throw ex;
                    }
                    throw new HttpParseException(ex.Message, ex);
                }
                HttpParseException exception = ex as HttpParseException;
                if (exception != null)
                {
                    error = new ParserError(exception.Message, exception.VirtualPath, exception.Line);
                }
                else
                {
                    error = new ParserError(ex.Message, base.CurrentVirtualPath, this._lineNumber);
                }
                error.Exception = ex;
                this.ParserErrors.Add(error);
                if ((exception == null) || base.CurrentVirtualPath.Equals(exception.VirtualPathObject))
                {
                    BuildManager.ReportParseError(error);
                }
            }
        }

        private void ProcessImportDirective(string directiveName, IDictionary directive)
        {
            string andRemoveNonEmptyNoSpaceAttribute = System.Web.UI.Util.GetAndRemoveNonEmptyNoSpaceAttribute(directive, "namespace");
            if (andRemoveNonEmptyNoSpaceAttribute == null)
            {
                this.ProcessError(System.Web.SR.GetString("Missing_attr", new object[] { "namespace" }));
            }
            else
            {
                this.AddImportEntry(andRemoveNonEmptyNoSpaceAttribute);
            }
            System.Web.UI.Util.CheckUnknownDirectiveAttributes(directiveName, directive);
        }

        private void ProcessInheritsAttribute(string baseTypeName, string codeFileBaseTypeName, string src, Assembly assembly)
        {
            if (this._codeFileVirtualPath != null)
            {
                this._baseTypeName = System.Web.UI.Util.GetNonEmptyFullClassNameAttribute("inherits", baseTypeName, ref this._baseTypeNamespace);
                baseTypeName = codeFileBaseTypeName;
                if (baseTypeName == null)
                {
                    return;
                }
            }
            Type c = null;
            if (assembly != null)
            {
                c = assembly.GetType(baseTypeName, false, true);
            }
            else
            {
                try
                {
                    c = this.GetType(baseTypeName);
                }
                catch
                {
                    if (this._generatedNamespace == null)
                    {
                        throw;
                    }
                    if (baseTypeName.IndexOf('.') >= 0)
                    {
                        throw;
                    }
                    try
                    {
                        string typeName = this._generatedNamespace + "." + baseTypeName;
                        c = this.GetType(typeName);
                    }
                    catch
                    {
                    }
                    if (c == null)
                    {
                        throw;
                    }
                }
            }
            if (c == null)
            {
                this.ProcessError(System.Web.SR.GetString("Non_existent_base_type", new object[] { baseTypeName, src }));
            }
            else if (!this.DefaultBaseType.IsAssignableFrom(c))
            {
                this.ProcessError(System.Web.SR.GetString("Invalid_type_to_inherit_from", new object[] { baseTypeName, this._baseType.FullName }));
            }
            else
            {
                if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowBaseType(c))
                {
                    throw new HttpException(System.Web.SR.GetString("Base_type_not_allowed", new object[] { c.FullName }));
                }
                this._baseType = c;
                this.EnsureRootBuilderCreated();
                this.AddTypeDependency(this._baseType);
                this.flags[0x80] = true;
            }
        }

        private void ProcessLanguageAttribute(string language)
        {
            if ((language != null) && !this.FInDesigner)
            {
                System.Web.Compilation.CompilerType compilerInfoFromLanguage = CompilationUtil.GetCompilerInfoFromLanguage(base.CurrentVirtualPath, language);
                if ((this._compilerType != null) && (this._compilerType.CodeDomProviderType != compilerInfoFromLanguage.CodeDomProviderType))
                {
                    this.ProcessError(System.Web.SR.GetString("Mixed_lang_not_supported", new object[] { language }));
                }
                else
                {
                    this._compilerType = compilerInfoFromLanguage;
                }
            }
        }

        private void ProcessLiteral()
        {
            string literal = this.GetLiteral();
            if (string.IsNullOrEmpty(literal))
            {
                this.flags[8] = false;
            }
            else
            {
                if (this.FApplicationFile)
                {
                    int offset = System.Web.UI.Util.FirstNonWhiteSpaceIndex(literal);
                    if ((offset >= 0) && !this.IgnoreParseErrors)
                    {
                        this._lineNumber -= System.Web.UI.Util.LineCount(literal, offset, literal.Length);
                        this.ProcessError(System.Web.SR.GetString("Invalid_app_file_content"));
                    }
                }
                else
                {
                    bool flag = false;
                    if (this.flags[8])
                    {
                        this.flags[8] = false;
                        if (System.Web.UI.Util.IsWhiteSpaceString(literal))
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        if (!this.flags[0x800])
                        {
                            this.ProcessMainDirective(this._mainDirectiveConfigSettings);
                            this.flags[0x800] = true;
                        }
                        ControlBuilder builder = ((BuilderStackEntry) this.BuilderStack.Peek())._builder;
                        try
                        {
                            builder.AppendLiteralString(literal);
                        }
                        catch (Exception exception)
                        {
                            if (!this.IgnoreParseErrors)
                            {
                                int num2 = System.Web.UI.Util.FirstNonWhiteSpaceIndex(literal);
                                if (num2 < 0)
                                {
                                    num2 = 0;
                                }
                                this._lineNumber -= System.Web.UI.Util.LineCount(literal, num2, literal.Length);
                                this.ProcessException(exception);
                            }
                        }
                        this.UpdateTypeHashCode("string");
                    }
                }
                this._literalBuilder = null;
            }
        }

        internal virtual void ProcessMainDirective(IDictionary mainDirective)
        {
            IDictionary parseData = new HybridDictionary();
            ParsedAttributeCollection attribs = null;
            foreach (DictionaryEntry entry in mainDirective)
            {
                string key = (string) entry.Key;
                string deviceName = System.Web.UI.Util.ParsePropertyDeviceFilter(key, out key);
                try
                {
                    if (!this.ProcessMainDirectiveAttribute(deviceName, key, (string) entry.Value, parseData))
                    {
                        if (attribs == null)
                        {
                            attribs = CreateEmptyAttributeBag();
                        }
                        attribs.AddFilteredAttribute(deviceName, key, (string) entry.Value);
                    }
                }
                catch (Exception exception)
                {
                    this.ProcessException(exception);
                }
            }
            this.PostProcessMainDirectiveAttributes(parseData);
            this.RootBuilder.SetControlType(this.BaseType);
            if (attribs != null)
            {
                this.RootBuilder.ProcessImplicitResources(attribs);
                foreach (FilteredAttributeDictionary dictionary2 in attribs.GetFilteredAttributeDictionaries())
                {
                    string filter = dictionary2.Filter;
                    foreach (DictionaryEntry entry2 in (IEnumerable) dictionary2)
                    {
                        string attribName = (string) entry2.Key;
                        this.ProcessUnknownMainDirectiveAttribute(filter, attribName, (string) entry2.Value);
                    }
                }
            }
        }

        internal virtual bool ProcessMainDirectiveAttribute(string deviceName, string name, string value, IDictionary parseData)
        {
            switch (name)
            {
                case "description":
                case "codebehind":
                    break;

                case "debug":
                    this.flags[0x4000] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    if (this.flags[0x4000] && !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
                    {
                        throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "debug" }));
                    }
                    this.flags[0x2000] = true;
                    break;

                case "linepragmas":
                    this.flags[0x8000] = !System.Web.UI.Util.GetBooleanAttribute(name, value);
                    break;

                case "warninglevel":
                    this._warningLevel = System.Web.UI.Util.GetNonNegativeIntegerAttribute(name, value);
                    break;

                case "compileroptions":
                {
                    this.OnFoundAttributeRequiringCompilation(name);
                    string compilerOptions = value.Trim();
                    CompilationUtil.CheckCompilerOptionsAllowed(compilerOptions, false, null, 0);
                    this._compilerOptions = compilerOptions;
                    break;
                }
                case "explicit":
                    this.flags[0x1000] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    break;

                case "strict":
                    this.flags[0x10000] = System.Web.UI.Util.GetBooleanAttribute(name, value);
                    break;

                case "language":
                {
                    this.ValidateBuiltInAttribute(deviceName, name, value);
                    string nonEmptyAttribute = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    this.ProcessLanguageAttribute(nonEmptyAttribute);
                    break;
                }
                case "src":
                    this.OnFoundAttributeRequiringCompilation(name);
                    parseData[name] = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    break;

                case "inherits":
                    parseData[name] = System.Web.UI.Util.GetNonEmptyAttribute(name, value);
                    break;

                case "classname":
                    this._generatedClassName = System.Web.UI.Util.GetNonEmptyFullClassNameAttribute(name, value, ref this._generatedNamespace);
                    break;

                case "codefile":
                    this.OnFoundAttributeRequiringCompilation(name);
                    try
                    {
                        this.ProcessCodeFile(VirtualPath.Create(System.Web.UI.Util.GetNonEmptyAttribute(name, value)));
                    }
                    catch (Exception exception)
                    {
                        this.ProcessException(exception);
                    }
                    break;

                default:
                    return false;
            }
            this.ValidateBuiltInAttribute(deviceName, name, value);
            return true;
        }

        private void ProcessObjectTag(ObjectTagBuilder objectBuilder)
        {
            ObjectTagScope scope = objectBuilder.Scope;
            this.CheckObjectTagScope(ref scope);
            switch (scope)
            {
                case ObjectTagScope.Page:
                case ObjectTagScope.AppInstance:
                    if (this._pageObjectList == null)
                    {
                        this._pageObjectList = new ArrayList();
                    }
                    this._pageObjectList.Add(objectBuilder);
                    return;

                case ObjectTagScope.Session:
                    if (this._sessionObjects == null)
                    {
                        this._sessionObjects = new HttpStaticObjectsCollection();
                    }
                    this._sessionObjects.Add(objectBuilder.ID, objectBuilder.ObjectType, objectBuilder.LateBound);
                    return;

                case ObjectTagScope.Application:
                    if (this._applicationObjects == null)
                    {
                        this._applicationObjects = new HttpStaticObjectsCollection();
                    }
                    this._applicationObjects.Add(objectBuilder.ID, objectBuilder.ObjectType, objectBuilder.LateBound);
                    break;
            }
        }

        private void ProcessScriptTag(Match match, string text, IDictionary attribs, bool fSelfClosed)
        {
            this.ProcessLiteral();
            this.flags[8] = true;
            VirtualPath andRemoveVirtualPathAttribute = System.Web.UI.Util.GetAndRemoveVirtualPathAttribute(attribs, "src");
            if (andRemoveVirtualPathAttribute != null)
            {
                this.EnsureCodeAllowed();
                andRemoveVirtualPathAttribute = base.ResolveVirtualPath(andRemoveVirtualPathAttribute);
                HttpRuntime.CheckVirtualFilePermission(andRemoveVirtualPathAttribute.VirtualPathString);
                this.AddSourceDependency(andRemoveVirtualPathAttribute);
                this.ProcessLanguageAttribute((string) attribs["language"]);
                this._currentScript = new ScriptBlockData(1, 1, andRemoveVirtualPathAttribute.VirtualPathString);
                this._currentScript.Script = System.Web.UI.Util.StringFromVirtualPath(andRemoveVirtualPathAttribute);
                this._scriptList.Add(this._currentScript);
                this._currentScript = null;
                if (!fSelfClosed)
                {
                    this.flags[2] = true;
                    this._scriptStartLineNumber = this._lineNumber;
                    this.flags[4] = true;
                }
            }
            else
            {
                this.ProcessLanguageAttribute((string) attribs["language"]);
                int num = match.Index + match.Length;
                int num2 = text.LastIndexOfAny(s_newlineChars, num - 1);
                int column = num - num2;
                this._currentScript = new ScriptBlockData(this._lineNumber, column, base.CurrentVirtualPathString);
                if (fSelfClosed)
                {
                    this.ProcessError(System.Web.SR.GetString("Script_tag_without_src_must_have_content"));
                }
                this.flags[2] = true;
                this._scriptStartLineNumber = this._lineNumber;
            }
        }

        private void ProcessServerInclude(Match match)
        {
            if (this.flags[2])
            {
                throw new HttpException(System.Web.SR.GetString("Include_not_allowed_in_server_script_tag"));
            }
            this.ProcessLiteral();
            string str = match.Groups["pathtype"].Value;
            string str2 = match.Groups["filename"].Value;
            if (str2.Length == 0)
            {
                this.ProcessError(System.Web.SR.GetString("Empty_file_name"));
            }
            else
            {
                VirtualPath currentVirtualPath = base.CurrentVirtualPath;
                string path = null;
                if (StringUtil.EqualsIgnoreCase(str, "file"))
                {
                    if (UrlPath.IsAbsolutePhysicalPath(str2))
                    {
                        path = str2;
                    }
                    else
                    {
                        bool flag = true;
                        try
                        {
                            currentVirtualPath = base.ResolveVirtualPath(VirtualPath.Create(str2));
                        }
                        catch
                        {
                            flag = false;
                        }
                        if (flag)
                        {
                            HttpRuntime.CheckVirtualFilePermission(currentVirtualPath.VirtualPathString);
                            this.AddSourceDependency(currentVirtualPath);
                        }
                        else
                        {
                            path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(base.CurrentVirtualPath.MapPath()), str2.Replace('/', '\\')));
                        }
                    }
                }
                else if (StringUtil.EqualsIgnoreCase(str, "virtual"))
                {
                    currentVirtualPath = base.ResolveVirtualPath(VirtualPath.Create(str2));
                    HttpRuntime.CheckVirtualFilePermission(currentVirtualPath.VirtualPathString);
                    this.AddSourceDependency(currentVirtualPath);
                }
                else
                {
                    this.ProcessError(System.Web.SR.GetString("Only_file_virtual_supported_on_server_include"));
                    return;
                }
                if (path != null)
                {
                    HttpRuntime.CheckFilePermission(path);
                }
                if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowServerSideInclude(currentVirtualPath.VirtualPathString))
                {
                    this.ProcessError(System.Web.SR.GetString("Include_not_allowed", new object[] { currentVirtualPath }));
                }
                this.ParseFile(path, currentVirtualPath);
                this.flags[8] = true;
            }
        }

        private void ProcessServerScript()
        {
            string literal = this.GetLiteral();
            if (string.IsNullOrEmpty(literal))
            {
                if (!this.IgnoreParseErrors)
                {
                    return;
                }
                literal = string.Empty;
            }
            if (!this.flags[4] && !this.PageParserFilterProcessedCodeBlock(CodeConstructType.ScriptTag, literal, this._currentScript.Line))
            {
                this.EnsureCodeAllowed();
                this._currentScript.Script = literal;
                this._scriptList.Add(this._currentScript);
                this._currentScript = null;
            }
            this._literalBuilder = null;
        }

        internal virtual void ProcessUnknownMainDirectiveAttribute(string filter, string attribName, string value)
        {
            this.ProcessError(System.Web.SR.GetString("Attr_not_supported_in_directive", new object[] { attribName, this.DefaultDirectiveName }));
        }

        internal void UpdateTypeHashCode(string text)
        {
            this._typeHashCode.AddObject(text);
        }

        internal void ValidateBuiltInAttribute(string deviceName, string name, string value)
        {
            if (this.IsExpressionBuilderValue(value))
            {
                this.ProcessError(System.Web.SR.GetString("Illegal_Resource_Builder", new object[] { name }));
            }
            if (deviceName.Length > 0)
            {
                this.ProcessError(System.Web.SR.GetString("Illegal_Device", new object[] { name }));
            }
        }

        internal HttpStaticObjectsCollection ApplicationObjects
        {
            get
            {
                return this._applicationObjects;
            }
        }

        internal AssemblySet AssemblyDependencies
        {
            get
            {
                return this._assemblyDependencies;
            }
        }

        internal Type BaseType
        {
            get
            {
                return this._baseType;
            }
            set
            {
                this._baseType = value;
            }
        }

        internal string BaseTypeName
        {
            get
            {
                return this._baseTypeName;
            }
        }

        internal string BaseTypeNamespace
        {
            get
            {
                return this._baseTypeNamespace;
            }
        }

        internal Stack BuilderStack
        {
            get
            {
                this.EnsureRootBuilderCreated();
                return this._builderStack;
            }
        }

        internal VirtualPath CodeFileVirtualPath
        {
            get
            {
                return this._codeFileVirtualPath;
            }
        }

        internal CompilationSection CompConfig
        {
            get
            {
                return this._compConfig;
            }
        }

        internal System.Web.UI.CompilationMode CompilationMode
        {
            get
            {
                if (BuildManager.PrecompilingForDeployment)
                {
                    return System.Web.UI.CompilationMode.Always;
                }
                return this._compilationMode;
            }
            set
            {
                if ((value == System.Web.UI.CompilationMode.Never) && this.flags[0x10])
                {
                    this.ProcessError(System.Web.SR.GetString("Compilmode_not_allowed"));
                }
                this._compilationMode = value;
            }
        }

        internal System.Web.Compilation.CompilerType CompilerType
        {
            get
            {
                return this._compilerType;
            }
        }

        internal CompilerParameters CompilParams
        {
            get
            {
                return this._compilerType.CompilerParameters;
            }
        }

        internal abstract Type DefaultBaseType { get; }

        internal abstract string DefaultDirectiveName { get; }

        internal virtual Type DefaultFileLevelBuilderType
        {
            get
            {
                return typeof(System.Web.UI.RootBuilder);
            }
        }

        internal IDesignerHost DesignerHost
        {
            get
            {
                return this._designerHost;
            }
            set
            {
                this._designerHost = value;
                this._typeResolutionService = null;
                if (this._designerHost != null)
                {
                    this._typeResolutionService = (ITypeResolutionService) this._designerHost.GetService(typeof(ITypeResolutionService));
                    if (this._typeResolutionService == null)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("TypeResService_Needed"));
                    }
                }
            }
        }

        internal EventHandler DesignTimeDataBindHandler
        {
            get
            {
                return this._designTimeDataBindHandler;
            }
            set
            {
                this._designTimeDataBindHandler = value;
            }
        }

        internal virtual bool FApplicationFile
        {
            get
            {
                return false;
            }
        }

        internal bool FExplicit
        {
            get
            {
                return this.flags[0x1000];
            }
        }

        internal virtual bool FInDesigner
        {
            get
            {
                return this.flags[0x100];
            }
            set
            {
                this.flags[0x100] = value;
            }
        }

        internal bool FLinePragmas
        {
            get
            {
                return !this.flags[0x8000];
            }
        }

        internal bool FStrict
        {
            get
            {
                return this.flags[0x10000];
            }
        }

        internal string GeneratedClassName
        {
            get
            {
                return this._generatedClassName;
            }
        }

        internal string GeneratedNamespace
        {
            get
            {
                if (this._generatedNamespace == null)
                {
                    return "ASP";
                }
                return this._generatedNamespace;
            }
        }

        internal bool HasCodeBehind
        {
            get
            {
                return this.flags[0x80];
            }
        }

        private bool HasParserErrors
        {
            get
            {
                return ((this._parserErrors != null) && (this._parserErrors.Count > 0));
            }
        }

        internal bool IgnoreControlProperties
        {
            get
            {
                return this.flags[0x20];
            }
            set
            {
                this.flags[0x20] = value;
            }
        }

        internal virtual bool IgnoreParseErrors
        {
            get
            {
                return this.flags[0x200];
            }
            set
            {
                this.flags[0x200] = value;
            }
        }

        internal ArrayList ImplementedInterfaces
        {
            get
            {
                return this._implementedInterfaces;
            }
        }

        internal virtual bool IsCodeAllowed
        {
            get
            {
                if (this.CompilationMode == System.Web.UI.CompilationMode.Never)
                {
                    return false;
                }
                if ((this._pageParserFilter != null) && !this._pageParserFilter.AllowCode)
                {
                    return false;
                }
                return true;
            }
        }

        internal Hashtable NamespaceEntries
        {
            get
            {
                return this._namespaceEntries;
            }
        }

        internal ArrayList PageObjectList
        {
            get
            {
                return this._pageObjectList;
            }
        }

        internal PagesSection PagesConfig
        {
            get
            {
                return this._pagesConfig;
            }
        }

        private ParserErrorCollection ParserErrors
        {
            get
            {
                if (this._parserErrors == null)
                {
                    this._parserErrors = new ParserErrorCollection();
                }
                return this._parserErrors;
            }
        }

        internal virtual bool RequiresCompilation
        {
            get
            {
                return true;
            }
        }

        internal System.Web.UI.RootBuilder RootBuilder
        {
            get
            {
                this.EnsureRootBuilderCreated();
                return this._rootBuilder;
            }
        }

        internal ArrayList ScriptList
        {
            get
            {
                return this._scriptList;
            }
        }

        internal HttpStaticObjectsCollection SessionObjects
        {
            get
            {
                return this._sessionObjects;
            }
        }

        internal StringSet SourceDependencies
        {
            get
            {
                return this._sourceDependencies;
            }
        }

        ICollection IAssemblyDependencyParser.AssemblyDependencies
        {
            get
            {
                return this.AssemblyDependencies;
            }
        }

        internal List<TagNamespaceRegisterEntry> TagRegisterEntries
        {
            get
            {
                return this.TypeMapper.TagRegisterEntries;
            }
        }

        internal string Text
        {
            get
            {
                return this._text;
            }
            set
            {
                this._text = value;
            }
        }

        internal bool ThrowOnFirstParseError
        {
            get
            {
                return this.flags[0x1000000];
            }
            set
            {
                this.flags[0x1000000] = value;
            }
        }

        internal int TypeHashCode
        {
            get
            {
                return this._typeHashCode.CombinedHash32;
            }
        }

        internal MainTagNameToTypeMapper TypeMapper
        {
            get
            {
                return this._typeMapper;
            }
        }

        internal ICollection UserControlRegisterEntries
        {
            get
            {
                return this.TypeMapper.UserControlRegisterEntries;
            }
        }
    }
}

