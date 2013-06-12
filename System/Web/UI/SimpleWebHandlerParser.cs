namespace System.Web.UI
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Util;

    public abstract class SimpleWebHandlerParser : IAssemblyDependencyParser
    {
        private SimpleHandlerBuildProvider _buildProvider;
        private System.Web.Compilation.CompilerType _compilerType;
        private bool _fFoundMainDirective;
        private bool _ignoreParseErrors;
        private int _lineNumber;
        private AssemblySet _linkedAssemblies;
        private TextReader _reader;
        private ICollection _referencedAssemblies;
        private StringSet _sourceDependencies;
        private string _sourceString;
        private int _startColumn;
        private string _typeName;
        private VirtualPath _virtualPath;
        private static readonly Regex directiveRegex = new SimpleDirectiveRegex();
        private static char[] s_newlineChars = new char[] { '\r', '\n' };

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        protected SimpleWebHandlerParser(HttpContext context, string virtualPath, string physicalPath)
        {
            this._virtualPath = VirtualPath.Create(virtualPath);
        }

        private void AddAssemblyDependency(Assembly assembly)
        {
            if (this._linkedAssemblies == null)
            {
                this._linkedAssemblies = new AssemblySet();
            }
            this._linkedAssemblies.Add(assembly);
        }

        private void AddAssemblyDependency(string assemblyName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            this.AddAssemblyDependency(assembly);
        }

        internal void AddSourceDependency(VirtualPath fileName)
        {
            if (this._sourceDependencies == null)
            {
                this._sourceDependencies = new CaseInsensitiveStringSet();
            }
            this._sourceDependencies.Add(fileName.VirtualPathString);
        }

        internal CodeCompileUnit GetCodeModel()
        {
            if (this._sourceString == null)
            {
                return null;
            }
            return new CodeSnippetCompileUnit(this._sourceString) { LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(this._virtualPath.VirtualPathString, this._lineNumber) };
        }

        protected Type GetCompiledTypeFromCache()
        {
            BuildResultCompiledType vPathBuildResult = (BuildResultCompiledType) BuildManager.GetVPathBuildResult(this._virtualPath);
            return vPathBuildResult.ResultType;
        }

        internal IDictionary GetLinePragmasTable()
        {
            LinePragmaCodeInfo info = new LinePragmaCodeInfo {
                _startLine = this._lineNumber,
                _startColumn = this._startColumn,
                _startGeneratedColumn = 1,
                _codeLength = -1,
                _isCodeNugget = false
            };
            IDictionary dictionary = new Hashtable();
            dictionary[this._lineNumber] = info;
            return dictionary;
        }

        private Type GetType(string typeName)
        {
            Type type;
            if (Util.TypeNameContainsAssembly(typeName))
            {
                try
                {
                    type = Type.GetType(typeName, true);
                }
                catch (Exception exception)
                {
                    throw new HttpParseException(null, exception, this._virtualPath, this._sourceString, this._lineNumber);
                }
                return type;
            }
            type = Util.GetTypeFromAssemblies(this._referencedAssemblies, typeName, false);
            if (type == null)
            {
                type = Util.GetTypeFromAssemblies(this._linkedAssemblies, typeName, false);
                if (type == null)
                {
                    throw new HttpParseException(System.Web.SR.GetString("Could_not_create_type", new object[] { typeName }), null, this._virtualPath, this._sourceString, this._lineNumber);
                }
            }
            return type;
        }

        internal Type GetTypeToCache(Assembly builtAssembly)
        {
            Type t = null;
            if (builtAssembly != null)
            {
                t = builtAssembly.GetType(this._typeName);
            }
            if (t == null)
            {
                t = this.GetType(this._typeName);
            }
            try
            {
                this.ValidateBaseType(t);
            }
            catch (Exception exception)
            {
                throw new HttpParseException(exception.Message, exception, this._virtualPath, this._sourceString, this._lineNumber);
            }
            return t;
        }

        private void ImportSourceFile(VirtualPath virtualPath)
        {
            VirtualPath fileName = this._virtualPath.Parent.Combine(virtualPath);
            this.AddSourceDependency(fileName);
            CompilationUtil.GetCompilerInfoFromVirtualPath(fileName);
            BuildResultCompiledAssembly vPathBuildResult = (BuildResultCompiledAssembly) BuildManager.GetVPathBuildResult(fileName);
            Assembly resultAssembly = vPathBuildResult.ResultAssembly;
            this.AddAssemblyDependency(resultAssembly);
        }

        internal virtual bool IsMainDirective(string directiveName)
        {
            return (string.Compare(directiveName, this.DefaultDirectiveName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal void Parse(ICollection referencedAssemblies)
        {
            this._referencedAssemblies = referencedAssemblies;
            this.AddSourceDependency(this._virtualPath);
            using (this._reader = this._buildProvider.OpenReaderInternal())
            {
                this.ParseReader();
            }
        }

        private void ParseReader()
        {
            string text = this._reader.ReadToEnd();
            try
            {
                this.ParseString(text);
            }
            catch (Exception exception)
            {
                throw new HttpParseException(exception.Message, exception, this._virtualPath, text, this._lineNumber);
            }
        }

        private void ParseString(string text)
        {
            int startat = 0;
            this._lineNumber = 1;
            while (true)
            {
                Match match = directiveRegex.Match(text, startat);
                if (!match.Success)
                {
                    break;
                }
                this._lineNumber += Util.LineCount(text, startat, match.Index);
                startat = match.Index;
                IDictionary attribs = CollectionsUtil.CreateCaseInsensitiveSortedList();
                string directiveName = this.ProcessAttributes(match, attribs);
                this.ProcessDirective(directiveName, attribs);
                this._lineNumber += Util.LineCount(text, startat, match.Index + match.Length);
                startat = match.Index + match.Length;
                int num2 = text.LastIndexOfAny(s_newlineChars, startat - 1);
                this._startColumn = startat - num2;
            }
            if (!this._fFoundMainDirective && !this.IgnoreParseErrors)
            {
                throw new HttpException(System.Web.SR.GetString("Missing_directive", new object[] { this.DefaultDirectiveName }));
            }
            string s = text.Substring(startat);
            if (!Util.IsWhiteSpaceString(s))
            {
                this._sourceString = s;
            }
        }

        private string ProcessAttributes(Match match, IDictionary attribs)
        {
            string str = string.Empty;
            CaptureCollection captures = match.Groups["attrname"].Captures;
            CaptureCollection captures2 = match.Groups["attrval"].Captures;
            CaptureCollection captures3 = null;
            captures3 = match.Groups["equal"].Captures;
            for (int i = 0; i < captures.Count; i++)
            {
                string key = captures[i].ToString();
                string str3 = captures2[i].ToString();
                bool flag = captures3[i].ToString().Length > 0;
                if (key != null)
                {
                    if (!flag && (i == 0))
                    {
                        str = key;
                    }
                    else
                    {
                        try
                        {
                            if (attribs != null)
                            {
                                attribs.Add(key, str3);
                            }
                        }
                        catch (ArgumentException)
                        {
                            if (!this.IgnoreParseErrors)
                            {
                                throw new HttpException(System.Web.SR.GetString("Duplicate_attr_in_tag", new object[] { key }));
                            }
                        }
                    }
                }
            }
            return str;
        }

        private static void ProcessCompilationParams(IDictionary directive, CompilerParameters compilParams)
        {
            bool val = false;
            if (Util.GetAndRemoveBooleanAttribute(directive, "debug", ref val))
            {
                compilParams.IncludeDebugInformation = val;
            }
            if (compilParams.IncludeDebugInformation && !HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Medium))
            {
                throw new HttpException(System.Web.SR.GetString("Insufficient_trust_for_attribute", new object[] { "debug" }));
            }
            int num = 0;
            if (Util.GetAndRemoveNonNegativeIntegerAttribute(directive, "warninglevel", ref num))
            {
                compilParams.WarningLevel = num;
                if (num > 0)
                {
                    compilParams.TreatWarningsAsErrors = true;
                }
            }
            string andRemoveNonEmptyAttribute = Util.GetAndRemoveNonEmptyAttribute(directive, "compileroptions");
            if (andRemoveNonEmptyAttribute != null)
            {
                CompilationUtil.CheckCompilerOptionsAllowed(andRemoveNonEmptyAttribute, false, null, 0);
                compilParams.CompilerOptions = andRemoveNonEmptyAttribute;
            }
        }

        internal virtual void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (directiveName.Length == 0)
            {
                directiveName = this.DefaultDirectiveName;
            }
            if (this.IsMainDirective(directiveName))
            {
                if (this._fFoundMainDirective && !this.IgnoreParseErrors)
                {
                    throw new HttpException(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { this.DefaultDirectiveName }));
                }
                this._fFoundMainDirective = true;
                directive.Remove("description");
                directive.Remove("codebehind");
                string andRemoveNonEmptyAttribute = Util.GetAndRemoveNonEmptyAttribute(directive, "language");
                if (andRemoveNonEmptyAttribute != null)
                {
                    this._compilerType = this._buildProvider.GetDefaultCompilerTypeForLanguageInternal(andRemoveNonEmptyAttribute);
                }
                else
                {
                    this._compilerType = this._buildProvider.GetDefaultCompilerTypeInternal();
                }
                this._typeName = Util.GetAndRemoveRequiredAttribute(directive, "class");
                if (this._compilerType.CompilerParameters != null)
                {
                    ProcessCompilationParams(directive, this._compilerType.CompilerParameters);
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "assembly"))
            {
                string assemblyName = Util.GetAndRemoveNonEmptyAttribute(directive, "name");
                VirtualPath andRemoveVirtualPathAttribute = Util.GetAndRemoveVirtualPathAttribute(directive, "src");
                if (((assemblyName != null) && (andRemoveVirtualPathAttribute != null)) && !this.IgnoreParseErrors)
                {
                    throw new HttpException(System.Web.SR.GetString("Attributes_mutually_exclusive", new object[] { "Name", "Src" }));
                }
                if (assemblyName == null)
                {
                    if (andRemoveVirtualPathAttribute == null)
                    {
                        if (!this.IgnoreParseErrors)
                        {
                            throw new HttpException(System.Web.SR.GetString("Missing_attr", new object[] { "name" }));
                        }
                    }
                    else
                    {
                        this.ImportSourceFile(andRemoveVirtualPathAttribute);
                    }
                }
                else
                {
                    this.AddAssemblyDependency(assemblyName);
                }
            }
            else if (!this.IgnoreParseErrors)
            {
                throw new HttpException(System.Web.SR.GetString("Unknown_directive", new object[] { directiveName }));
            }
            Util.CheckUnknownDirectiveAttributes(directiveName, directive);
        }

        internal void SetBuildProvider(SimpleHandlerBuildProvider buildProvider)
        {
            this._buildProvider = buildProvider;
        }

        internal virtual void ValidateBaseType(Type t)
        {
        }

        internal ICollection AssemblyDependencies
        {
            get
            {
                return this._linkedAssemblies;
            }
        }

        internal System.Web.Compilation.CompilerType CompilerType
        {
            get
            {
                return this._compilerType;
            }
        }

        protected abstract string DefaultDirectiveName { get; }

        internal bool HasInlineCode
        {
            get
            {
                return (this._sourceString != null);
            }
        }

        internal bool IgnoreParseErrors
        {
            get
            {
                return this._ignoreParseErrors;
            }
            set
            {
                this._ignoreParseErrors = value;
            }
        }

        internal ICollection SourceDependencies
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

        internal string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

