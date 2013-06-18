namespace System.Web.Compilation
{
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.Schema;

    public class AssemblyBuilder
    {
        private AssemblySet _additionalReferencedAssemblies;
        private Hashtable _buildProviders = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private Hashtable _buildProviderToSourceFileMap;
        internal System.CodeDom.Compiler.CodeDomProvider _codeProvider;
        private CompilationSection _compConfig;
        private CompilerType _compilerType;
        private string _cultureName;
        private StringSet _embeddedResourceFiles;
        private int _fileCount;
        private AssemblySet _initialReferencedAssemblies;
        private long _maxBatchGeneratedFileSize;
        private int _maxBatchSize;
        private CodeCompileUnit _miscCodeCompileUnit;
        private ObjectFactoryCodeDomTreeGenerator _objectFactoryGenerator;
        private string _outputAssemblyName;
        private CaseInsensitiveStringSet _registeredTypeNames;
        private StringSet _sourceFiles = new StringSet();
        private System.Web.StringResourceBuilder _stringResourceBuilder;
        private string _tempFilePhysicalPathPrefix;
        private TempFileCollection _tempFiles = new TempFileCollection(HttpRuntime.CodegenDirInternal);
        private long _totalFileLength;
        private const string MySupport = "/define:_MYTYPE=\\\"Web\\\"";
        private static Guid? s_hashMD5Guid;
        private static HashAlgorithm s_md5HashAlgorithm;
        private static string s_vbImportsString;

        internal AssemblyBuilder(CompilationSection compConfig, ICollection referencedAssemblies, CompilerType compilerType, string outputAssemblyName)
        {
            this._compConfig = compConfig;
            this._outputAssemblyName = outputAssemblyName;
            this._initialReferencedAssemblies = AssemblySet.Create(referencedAssemblies);
            this._compilerType = compilerType.Clone();
            if (BuildManager.PrecompilingWithDebugInfo)
            {
                this._compilerType.CompilerParameters.IncludeDebugInformation = true;
            }
            else if (BuildManager.PrecompilingForDeployment)
            {
                this._compilerType.CompilerParameters.IncludeDebugInformation = false;
            }
            else if (DeploymentSection.RetailInternal)
            {
                this._compilerType.CompilerParameters.IncludeDebugInformation = false;
            }
            else if (this._compConfig.AssemblyPostProcessorTypeInternal != null)
            {
                this._compilerType.CompilerParameters.IncludeDebugInformation = true;
            }
            this._tempFiles.KeepFiles = this._compilerType.CompilerParameters.IncludeDebugInformation;
            this._codeProvider = CompilationUtil.CreateCodeDomProviderNonPublic(this._compilerType.CodeDomProviderType);
            this._maxBatchSize = this._compConfig.MaxBatchSize;
            this._maxBatchGeneratedFileSize = this._compConfig.MaxBatchGeneratedFileSize * 0x400;
        }

        private void AddAllowPartiallyTrustedCallersAttribute()
        {
            if (BuildManager.CompileWithAllowPartiallyTrustedCallersAttribute)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(AllowPartiallyTrustedCallersAttribute)));
                this.AddAssemblyAttribute(declaration);
            }
        }

        private void AddAspNetGeneratedCodeAttribute()
        {
            CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(GeneratedCodeAttribute)));
            declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression("ASP.NET")));
            declaration.Arguments.Add(new CodeAttributeArgument(new CodePrimitiveExpression(VersionInfo.SystemWebVersion)));
            this.AddAssemblyAttribute(declaration);
        }

        private void AddAssemblyAttribute(CodeAttributeDeclaration declaration)
        {
            if (this._miscCodeCompileUnit == null)
            {
                this._miscCodeCompileUnit = new CodeCompileUnit();
            }
            this._miscCodeCompileUnit.AssemblyCustomAttributes.Add(declaration);
        }

        private void AddAssemblyCultureAttribute()
        {
            if (this.CultureName != null)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(AssemblyCultureAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(this.CultureName)) });
                this.AddAssemblyAttribute(declaration);
            }
        }

        private void AddAssemblyDelaySignAttribute()
        {
            if (BuildManager.CompileWithDelaySignAttribute)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(AssemblyDelaySignAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(true)) });
                this.AddAssemblyAttribute(declaration);
            }
        }

        private void AddAssemblyKeyContainerAttribute()
        {
            if (!string.IsNullOrEmpty(BuildManager.StrongNameKeyContainer))
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(AssemblyKeyNameAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.StrongNameKeyContainer)) });
                this.AddAssemblyAttribute(declaration);
            }
        }

        private void AddAssemblyKeyFileAttribute()
        {
            if (!string.IsNullOrEmpty(BuildManager.StrongNameKeyFile))
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(AssemblyKeyFileAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.StrongNameKeyFile)) });
                this.AddAssemblyAttribute(declaration);
            }
        }

        public void AddAssemblyReference(Assembly a)
        {
            if (this._additionalReferencedAssemblies == null)
            {
                this._additionalReferencedAssemblies = new AssemblySet();
            }
            this._additionalReferencedAssemblies.Add(a);
        }

        internal void AddAssemblyReference(Assembly a, CodeCompileUnit ccu)
        {
            this.AddAssemblyReference(a);
            Util.AddAssemblyToStringCollection(a, ccu.ReferencedAssemblies);
        }

        internal virtual void AddBuildProvider(System.Web.Compilation.BuildProvider buildProvider)
        {
            object key = buildProvider;
            bool flag = false;
            if (this._compConfig.FolderLevelBuildProviders != null)
            {
                Type t = buildProvider.GetType();
                flag = this._compConfig.FolderLevelBuildProviders.IsFolderLevelBuildProvider(t);
            }
            if ((buildProvider.VirtualPath != null) && !flag)
            {
                key = buildProvider.VirtualPath;
                if (this._buildProviders.ContainsKey(key))
                {
                    return;
                }
            }
            this._buildProviders[key] = buildProvider;
            try
            {
                buildProvider.GenerateCode(this);
            }
            catch (XmlException exception)
            {
                throw new HttpParseException(exception.Message, null, buildProvider.VirtualPath, null, exception.LineNumber);
            }
            catch (XmlSchemaException exception2)
            {
                throw new HttpParseException(exception2.Message, null, buildProvider.VirtualPath, null, exception2.LineNumber);
            }
            catch (Exception exception3)
            {
                throw new HttpParseException(exception3.Message, exception3, buildProvider.VirtualPath, null, 1);
            }
            InternalBuildProvider owningBuildProvider = buildProvider as InternalBuildProvider;
            if (owningBuildProvider != null)
            {
                ICollection compileWithDependencies = owningBuildProvider.GetCompileWithDependencies();
                if (compileWithDependencies != null)
                {
                    foreach (VirtualPath path in compileWithDependencies)
                    {
                        if (!this._buildProviders.ContainsKey(path.VirtualPathString))
                        {
                            this.AddCompileWithBuildProvider(path, owningBuildProvider);
                        }
                    }
                }
            }
        }

        private void AddChecksumPragma(System.Web.Compilation.BuildProvider buildProvider, CodeCompileUnit compileUnit)
        {
            if (((buildProvider != null) && (buildProvider.VirtualPath != null)) && this._compilerType.CompilerParameters.IncludeDebugInformation)
            {
                string path = HostingEnvironment.MapPathInternal(buildProvider.VirtualPath);
                if (File.Exists(path))
                {
                    if (!s_hashMD5Guid.HasValue)
                    {
                        s_hashMD5Guid = new Guid(0x406ea660, 0x64cf, 0x4c82, 0xb6, 240, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);
                    }
                    CodeChecksumPragma pragma = new CodeChecksumPragma();
                    if (this._compConfig.UrlLinePragmas)
                    {
                        pragma.FileName = ErrorFormatter.MakeHttpLinePragma(buildProvider.VirtualPathObject.VirtualPathString);
                    }
                    else
                    {
                        pragma.FileName = path;
                    }
                    pragma.ChecksumAlgorithmId = s_hashMD5Guid.Value;
                    using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        pragma.ChecksumData = this.ComputeHash(stream);
                    }
                    compileUnit.StartDirectives.Add(pragma);
                }
            }
        }

        public void AddCodeCompileUnit(System.Web.Compilation.BuildProvider buildProvider, CodeCompileUnit compileUnit)
        {
            string str;
            this.AddChecksumPragma(buildProvider, compileUnit);
            Util.AddAssembliesToStringCollection(this._initialReferencedAssemblies, compileUnit.ReferencedAssemblies);
            Util.AddAssembliesToStringCollection(this._additionalReferencedAssemblies, compileUnit.ReferencedAssemblies);
            using (new ProcessImpersonationContext())
            {
                TextWriter writer = this.CreateCodeFile(buildProvider, out str);
                try
                {
                    this._codeProvider.GenerateCodeFromCompileUnit(compileUnit, writer, null);
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }
            }
            if (str != null)
            {
                this._totalFileLength += this.GetFileLengthWithAssert(str);
            }
        }

        private void AddCompileWithBuildProvider(VirtualPath virtualPath, System.Web.Compilation.BuildProvider owningBuildProvider)
        {
            System.Web.Compilation.BuildProvider buildProvider = BuildManager.CreateBuildProvider(virtualPath, this._compConfig, this._initialReferencedAssemblies, true);
            buildProvider.SetNoBuildResult();
            SourceFileBuildProvider provider2 = buildProvider as SourceFileBuildProvider;
            if (provider2 != null)
            {
                provider2.OwningBuildProvider = owningBuildProvider;
            }
            this.AddBuildProvider(buildProvider);
        }

        private void AddSecurityRulesAttribute()
        {
            if (!MultiTargetingUtil.IsTargetFramework20 && !MultiTargetingUtil.IsTargetFramework35)
            {
                CodeAttributeDeclaration declaration;
                TrustSection trust = RuntimeConfig.GetAppConfig().Trust;
                Type type = typeof(SecurityRulesAttribute);
                Type enumType = typeof(SecurityRuleSet);
                if (trust.LegacyCasModel)
                {
                    SecurityRuleSet set = SecurityRuleSet.Level1;
                    string name = Enum.GetName(enumType, set);
                    CodeFieldReferenceExpression expression = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType), name);
                    declaration = new CodeAttributeDeclaration(new CodeTypeReference(type), new CodeAttributeArgument[] { new CodeAttributeArgument(expression) });
                    this.AddAssemblyAttribute(declaration);
                }
                else
                {
                    SecurityRuleSet set2 = SecurityRuleSet.Level2;
                    string fieldName = Enum.GetName(enumType, set2);
                    CodeFieldReferenceExpression expression2 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(enumType), fieldName);
                    declaration = new CodeAttributeDeclaration(new CodeTypeReference(type), new CodeAttributeArgument[] { new CodeAttributeArgument(expression2) });
                    this.AddAssemblyAttribute(declaration);
                }
            }
        }

        private void AddTargetFrameworkAttribute()
        {
            if (MultiTargetingUtil.TargetFrameworkVersion.Major >= 4)
            {
                CodeAttributeDeclaration declaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(TargetFrameworkAttribute)), new CodeAttributeArgument[] { new CodeAttributeArgument(new CodePrimitiveExpression(BuildManager.TargetFramework.FullName)) });
                this.AddAssemblyAttribute(declaration);
            }
        }

        internal void AddTypeNames(ICollection typeNames)
        {
            if (typeNames != null)
            {
                if (this._registeredTypeNames == null)
                {
                    this._registeredTypeNames = new CaseInsensitiveStringSet();
                }
                this._registeredTypeNames.AddCollection(typeNames);
            }
        }

        private static void AddVBGlobalNamespaceImports(CompilerParameters compilParams)
        {
            if (s_vbImportsString == null)
            {
                PagesSection pagesAppConfig = MTConfigUtil.GetPagesAppConfig();
                if (pagesAppConfig.Namespaces == null)
                {
                    s_vbImportsString = string.Empty;
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("/imports:");
                    bool flag = false;
                    if (pagesAppConfig.Namespaces.AutoImportVBNamespace)
                    {
                        builder.Append("Microsoft.VisualBasic");
                        flag = true;
                    }
                    foreach (NamespaceInfo info in pagesAppConfig.Namespaces)
                    {
                        if (flag)
                        {
                            builder.Append(',');
                        }
                        builder.Append(info.Namespace);
                        flag = true;
                    }
                    s_vbImportsString = builder.ToString();
                }
            }
            if (s_vbImportsString.Length > 0)
            {
                if (compilParams.CompilerOptions == null)
                {
                    compilParams.CompilerOptions = s_vbImportsString;
                }
                else
                {
                    compilParams.CompilerOptions = s_vbImportsString + " " + compilParams.CompilerOptions;
                }
            }
        }

        private static void AddVBMyFlags(CompilerParameters compilParams)
        {
            if (compilParams.CompilerOptions == null)
            {
                compilParams.CompilerOptions = "/define:_MYTYPE=\\\"Web\\\"";
            }
            else
            {
                compilParams.CompilerOptions = "/define:_MYTYPE=\\\"Web\\\" " + compilParams.CompilerOptions;
            }
        }

        internal CompilerResults Compile()
        {
            if ((this._sourceFiles.Count == 0) && (this._embeddedResourceFiles == null))
            {
                return null;
            }
            if (this._objectFactoryGenerator != null)
            {
                this._miscCodeCompileUnit = this._objectFactoryGenerator.CodeCompileUnit;
            }
            this.AddAssemblyCultureAttribute();
            this.AddAspNetGeneratedCodeAttribute();
            this.AddAllowPartiallyTrustedCallersAttribute();
            this.AddAssemblyDelaySignAttribute();
            this.AddAssemblyKeyFileAttribute();
            this.AddAssemblyKeyContainerAttribute();
            this.AddSecurityRulesAttribute();
            this.AddTargetFrameworkAttribute();
            this.GenerateMiscCodeCompileUnit();
            CompilerParameters compilerParameters = this.GetCompilerParameters();
            string[] array = new string[this._sourceFiles.Count];
            this._sourceFiles.CopyTo(array, 0);
            PerfCounters.IncrementCounter(AppPerfCounter.COMPILATIONS);
            WebBaseEvent.RaiseSystemEvent(this, 0x3eb);
            HttpContext current = HttpContext.Current;
            if ((current != null) && EtwTrace.IsTraceEnabled(5, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_COMPILE_ENTER, current.WorkerRequest);
            }
            CompilerResults results = null;
            try
            {
                try
                {
                    using (new ProcessImpersonationContext())
                    {
                        results = this._codeProvider.CompileAssemblyFromFile(compilerParameters, array);
                    }
                }
                finally
                {
                    if (EtwTrace.IsTraceEnabled(5, 1) && (current != null))
                    {
                        string str2;
                        string str = null;
                        if (this._buildProviders.Count < 20)
                        {
                            IDictionaryEnumerator enumerator = this._buildProviders.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                if (str != null)
                                {
                                    str = str + ",";
                                }
                                str = str + enumerator.Key;
                            }
                        }
                        else
                        {
                            str = string.Format(CultureInfo.InstalledUICulture, System.Web.SR.Resources.GetString("Etw_Batch_Compilation", CultureInfo.InstalledUICulture), new object[] { this._buildProviders.Count });
                        }
                        if ((results != null) && ((results.NativeCompilerReturnValue != 0) || results.Errors.HasErrors))
                        {
                            str2 = System.Web.SR.Resources.GetString("Etw_Failure", CultureInfo.InstalledUICulture);
                        }
                        else
                        {
                            str2 = System.Web.SR.Resources.GetString("Etw_Success", CultureInfo.InstalledUICulture);
                        }
                        EtwTrace.Trace(EtwTraceType.ETW_TYPE_COMPILE_LEAVE, current.WorkerRequest, str, str2);
                    }
                }
            }
            catch
            {
                throw;
            }
            Type assemblyPostProcessorTypeInternal = this._compConfig.AssemblyPostProcessorTypeInternal;
            if (assemblyPostProcessorTypeInternal != null)
            {
                using (IAssemblyPostProcessor processor = (IAssemblyPostProcessor) HttpRuntime.FastCreatePublicInstance(assemblyPostProcessorTypeInternal))
                {
                    processor.PostProcessAssembly(results.PathToAssembly);
                }
            }
            WebBaseEvent.RaiseSystemEvent(this, 0x3ec);
            if (results == null)
            {
                return results;
            }
            this.InvalidateInvalidAssembly(results, compilerParameters);
            this.FixUpLinePragmas(results);
            if (results.Errors.HasErrors)
            {
                foreach (System.Web.Compilation.BuildProvider provider in this.BuildProviders)
                {
                    provider.ProcessCompileErrors(results);
                }
            }
            if (BuildManager.CBMCallback != null)
            {
                foreach (CompilerError error in results.Errors)
                {
                    BuildManager.CBMCallback.ReportCompilerError(error);
                }
            }
            if ((results.NativeCompilerReturnValue == 0) && !results.Errors.HasErrors)
            {
                return results;
            }
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_COMPILING);
            PerfCounters.IncrementCounter(AppPerfCounter.ERRORS_TOTAL);
            throw new HttpCompileException(results, this.GetErrorSourceFileContents(results));
        }

        private byte[] ComputeHash(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            byte[] hash = new byte[0x10];
            if (System.Web.UnsafeNativeMethods.GetSHA1Hash(buffer, buffer.Length, hash, hash.Length) == 0)
            {
                return hash;
            }
            if (s_md5HashAlgorithm == null)
            {
                s_md5HashAlgorithm = new MD5CryptoServiceProvider();
            }
            return s_md5HashAlgorithm.ComputeHash(buffer);
        }

        internal bool ContainsTypeNames(ICollection typeNames)
        {
            if ((this._registeredTypeNames != null) && (typeNames != null))
            {
                foreach (string str in typeNames)
                {
                    if (this._registeredTypeNames.Contains(str))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public TextWriter CreateCodeFile(System.Web.Compilation.BuildProvider buildProvider)
        {
            string str;
            return this.CreateCodeFile(buildProvider, out str);
        }

        internal virtual TextWriter CreateCodeFile(System.Web.Compilation.BuildProvider buildProvider, out string filename)
        {
            string tempFilePhysicalPathWithAssert = this.GetTempFilePhysicalPathWithAssert(this._codeProvider.FileExtension);
            filename = tempFilePhysicalPathWithAssert;
            if (buildProvider != null)
            {
                if (this._buildProviderToSourceFileMap == null)
                {
                    this._buildProviderToSourceFileMap = new Hashtable();
                }
                this._buildProviderToSourceFileMap[buildProvider] = tempFilePhysicalPathWithAssert;
                buildProvider.SetContributedCode();
            }
            this._sourceFiles.Add(tempFilePhysicalPathWithAssert);
            return this.CreateCodeFileWithAssert(tempFilePhysicalPathWithAssert);
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private StreamWriter CreateCodeFileWithAssert(string generatedFilePath)
        {
            return new StreamWriter(new FileStream(generatedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8);
        }

        public Stream CreateEmbeddedResource(System.Web.Compilation.BuildProvider buildProvider, string name)
        {
            if (!Util.IsValidFileName(name))
            {
                throw new ArgumentException(null, name);
            }
            string codegenResourceDir = BuildManager.CodegenResourceDir;
            string fileName = Path.Combine(codegenResourceDir, name);
            this.CreateTempResourceDirectoryIfNecessary();
            this._tempFiles.AddFile(fileName, this._tempFiles.KeepFiles);
            if (this._embeddedResourceFiles == null)
            {
                this._embeddedResourceFiles = new StringSet();
            }
            this._embeddedResourceFiles.Add(fileName);
            InternalSecurityPermissions.FileWriteAccess(codegenResourceDir).Assert();
            return File.OpenWrite(fileName);
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private void CreateTempResourceDirectoryIfNecessary()
        {
            string codegenResourceDir = BuildManager.CodegenResourceDir;
            if (!System.Web.Util.FileUtil.DirectoryExists(codegenResourceDir))
            {
                Directory.CreateDirectory(codegenResourceDir);
            }
        }

        internal static void FixTreatWarningsAsErrors(Type codeDomProviderType, CompilerParameters compilParams)
        {
            if (((codeDomProviderType == typeof(CSharpCodeProvider)) || (codeDomProviderType == typeof(VBCodeProvider))) && (CultureInfo.InvariantCulture.CompareInfo.IndexOf(compilParams.CompilerOptions, "/warnaserror", CompareOptions.IgnoreCase) >= 0))
            {
                compilParams.TreatWarningsAsErrors = false;
            }
        }

        internal static void FixUpCompilerParameters(Type codeDomProviderType, CompilerParameters compilParams)
        {
            if (codeDomProviderType == typeof(CSharpCodeProvider))
            {
                CodeDomUtility.PrependCompilerOption(compilParams, "/nowarn:1659;1699;1701");
            }
            else if (codeDomProviderType == typeof(VBCodeProvider))
            {
                AddVBGlobalNamespaceImports(compilParams);
                AddVBMyFlags(compilParams);
                if (MultiTargetingUtil.TargetFrameworkVersion >= MultiTargetingUtil.Version35)
                {
                    CodeDomUtility.PrependCompilerOption(compilParams, "/nowarn:41008");
                }
            }
            ProcessProviderOptions(codeDomProviderType, compilParams);
            FixTreatWarningsAsErrors(codeDomProviderType, compilParams);
            if (BuildManager.PrecompilingWithCodeAnalysisSymbol)
            {
                CodeDomUtility.PrependCompilerOption(compilParams, "/define:CODE_ANALYSIS");
            }
        }

        private void FixUpLinePragmas(CompilerResults results)
        {
            CompilerError error = null;
            for (int i = results.Errors.Count - 1; i >= 0; i--)
            {
                CompilerError error2 = results.Errors[i];
                string path = ErrorFormatter.ResolveHttpFileName(error2.FileName);
                if (File.Exists(path))
                {
                    error2.FileName = path;
                    if ((error2.Line == 0xdebb0) || (((error2.Line == 0xdebb1) && (error2.ErrorText != null)) && (error2.ErrorText.IndexOf("FrameworkInitialize", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        error = error2;
                        results.Errors.RemoveAt(i);
                    }
                    else if ((error2.Line > 0xdebb0) && (error2.Line < 0xdebe2))
                    {
                        results.Errors.RemoveAt(i);
                    }
                }
            }
            if (error != null)
            {
                string source = Util.StringFromFile(error.FileName);
                int newoffset = CultureInfo.InvariantCulture.CompareInfo.IndexOf(source, "partial class", CompareOptions.IgnoreCase);
                if (newoffset >= 0)
                {
                    error.Line = Util.LineCount(source, 0, newoffset) + 1;
                }
                else
                {
                    error.Line = 1;
                }
                error.ErrorText = System.Web.SR.GetString("Bad_Base_Class_In_Code_File");
                error.ErrorNumber = "ASPNET";
                results.Errors.Insert(0, error);
            }
        }

        private void GenerateMiscCodeCompileUnit()
        {
            if (this._miscCodeCompileUnit != null)
            {
                this.AddCodeCompileUnit(null, this._miscCodeCompileUnit);
            }
        }

        public void GenerateTypeFactory(string typeName)
        {
            if (this._objectFactoryGenerator == null)
            {
                this._objectFactoryGenerator = new ObjectFactoryCodeDomTreeGenerator(this.OutputAssemblyName);
            }
            this._objectFactoryGenerator.AddFactoryMethod(typeName);
        }

        internal System.Web.Compilation.BuildProvider GetBuildProviderFromLinePragma(string linePragma)
        {
            System.Web.Compilation.BuildProvider buildProviderFromLinePragmaInternal = this.GetBuildProviderFromLinePragmaInternal(linePragma);
            SourceFileBuildProvider provider2 = buildProviderFromLinePragmaInternal as SourceFileBuildProvider;
            if (provider2 != null)
            {
                buildProviderFromLinePragmaInternal = provider2.OwningBuildProvider;
            }
            return buildProviderFromLinePragmaInternal;
        }

        private System.Web.Compilation.BuildProvider GetBuildProviderFromLinePragmaInternal(string linePragma)
        {
            if (this._buildProviderToSourceFileMap != null)
            {
                string virtualPathFromHttpLinePragma = ErrorFormatter.GetVirtualPathFromHttpLinePragma(linePragma);
                foreach (System.Web.Compilation.BuildProvider provider in this.BuildProviders)
                {
                    if (provider.VirtualPath != null)
                    {
                        if (virtualPathFromHttpLinePragma != null)
                        {
                            if (System.Web.Util.StringUtil.EqualsIgnoreCase(virtualPathFromHttpLinePragma, provider.VirtualPath))
                            {
                                return provider;
                            }
                        }
                        else
                        {
                            string str2 = HostingEnvironment.MapPathInternal(provider.VirtualPath);
                            if (System.Web.Util.StringUtil.EqualsIgnoreCase(linePragma, str2))
                            {
                                return provider;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal CompilerParameters GetCompilerParameters()
        {
            CompilerParameters compilerParameters = this._compilerType.CompilerParameters;
            string tempDir = this._tempFiles.TempDir;
            if (this.CultureName != null)
            {
                tempDir = Path.Combine(tempDir, this.CultureName);
                Directory.CreateDirectory(tempDir);
                compilerParameters.OutputAssembly = Path.Combine(tempDir, this.OutputAssemblyName + ".resources.dll");
            }
            else
            {
                compilerParameters.OutputAssembly = Path.Combine(tempDir, this.OutputAssemblyName + ".dll");
            }
            if (File.Exists(compilerParameters.OutputAssembly))
            {
                Util.RemoveOrRenameFile(compilerParameters.OutputAssembly);
            }
            compilerParameters.TempFiles = this._tempFiles;
            if ((this._stringResourceBuilder != null) && this._stringResourceBuilder.HasStrings)
            {
                string resFileName = this._tempFiles.AddExtension("res");
                this._stringResourceBuilder.CreateResourceFile(resFileName);
                compilerParameters.Win32Resource = resFileName;
            }
            if (this._embeddedResourceFiles != null)
            {
                foreach (string str3 in (IEnumerable) this._embeddedResourceFiles)
                {
                    compilerParameters.EmbeddedResources.Add(str3);
                }
            }
            if (this._additionalReferencedAssemblies != null)
            {
                foreach (Assembly assembly in (IEnumerable) this._additionalReferencedAssemblies)
                {
                    this._initialReferencedAssemblies.Add(assembly);
                }
            }
            Util.AddAssembliesToStringCollection(this._initialReferencedAssemblies, compilerParameters.ReferencedAssemblies);
            FixUpCompilerParameters(this._compilerType.CodeDomProviderType, compilerParameters);
            return compilerParameters;
        }

        private string GetErrorSourceFileContents(CompilerResults results)
        {
            if (!results.Errors.HasErrors)
            {
                return null;
            }
            string fileName = results.Errors[0].FileName;
            System.Web.Compilation.BuildProvider buildProviderFromLinePragma = this.GetBuildProviderFromLinePragma(fileName);
            if (buildProviderFromLinePragma != null)
            {
                return this.GetGeneratedSourceFromBuildProvider(buildProviderFromLinePragma);
            }
            return Util.StringFromFileIfExists(fileName);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.Read)]
        private long GetFileLengthWithAssert(string filename)
        {
            FileInfo info = new FileInfo(filename);
            return info.Length;
        }

        internal string GetGeneratedSourceFromBuildProvider(System.Web.Compilation.BuildProvider buildProvider)
        {
            string path = (string) this._buildProviderToSourceFileMap[buildProvider];
            return Util.StringFromFileIfExists(path);
        }

        public string GetTempFilePhysicalPath(string extension)
        {
            string str;
            if (!string.IsNullOrEmpty(extension) && (extension[0] == '.'))
            {
                str = this.TempFilePhysicalPathPrefix + this._fileCount++ + extension;
            }
            else
            {
                str = string.Concat(new object[] { this.TempFilePhysicalPathPrefix, this._fileCount++, ".", extension });
            }
            this._tempFiles.AddFile(str, this._tempFiles.KeepFiles);
            InternalSecurityPermissions.PathDiscovery(str).Demand();
            return str;
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        internal string GetTempFilePhysicalPathWithAssert(string extension)
        {
            return this.GetTempFilePhysicalPath(extension);
        }

        private void InvalidateInvalidAssembly(CompilerResults results, CompilerParameters compilParams)
        {
            if ((results != null) && results.Errors.HasErrors)
            {
                foreach (CompilerError error in results.Errors)
                {
                    if (!error.IsWarning && System.Web.Util.StringUtil.EqualsIgnoreCase(error.ErrorNumber, "CS0016"))
                    {
                        if (this.CultureName != null)
                        {
                            DiskBuildResultCache.TryDeleteFile(new FileInfo(Path.Combine(this._tempFiles.TempDir, this.OutputAssemblyName + ".dll")));
                        }
                        DiskBuildResultCache.TryDeleteFile(compilParams.OutputAssembly);
                    }
                }
            }
        }

        private static void ProcessBooleanProviderOption(string providerOptionName, string trueCompilerOption, string falseCompilerOption, IDictionary<string, string> providerOptions, CompilerParameters compilParams)
        {
            if ((providerOptions != null) && (compilParams != null))
            {
                string str = null;
                if (providerOptions.TryGetValue(providerOptionName, out str))
                {
                    bool flag;
                    if (string.IsNullOrEmpty(str))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Property_NullOrEmpty", new object[] { "system.codedom/compilers/compiler/ProviderOption/" + providerOptionName }));
                    }
                    if (!bool.TryParse(str, out flag))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Value_must_be_boolean", new object[] { "system.codedom/compilers/compiler/ProviderOption/" + providerOptionName }));
                    }
                    if (flag)
                    {
                        CodeDomUtility.AppendCompilerOption(compilParams, trueCompilerOption);
                    }
                    else
                    {
                        CodeDomUtility.AppendCompilerOption(compilParams, falseCompilerOption);
                    }
                }
            }
        }

        private static void ProcessProviderOptions(Type codeDomProviderType, CompilerParameters compilParams)
        {
            IDictionary<string, string> providerOptions = CompilationUtil.GetProviderOptions(codeDomProviderType);
            if (providerOptions != null)
            {
                if ((codeDomProviderType == typeof(VBCodeProvider)) || (codeDomProviderType == typeof(CSharpCodeProvider)))
                {
                    ProcessBooleanProviderOption("WarnAsError", "/warnaserror+", "/warnaserror-", providerOptions, compilParams);
                }
                if (((codeDomProviderType != null) && CompilationUtil.IsCompilerVersion35OrAbove(codeDomProviderType)) && (codeDomProviderType == typeof(VBCodeProvider)))
                {
                    ProcessBooleanProviderOption("OptionInfer", "/optionInfer+", "/optionInfer-", providerOptions, compilParams);
                }
            }
        }

        internal ICollection BuildProviders
        {
            get
            {
                return this._buildProviders.Values;
            }
        }

        public System.CodeDom.Compiler.CodeDomProvider CodeDomProvider
        {
            get
            {
                return this._codeProvider;
            }
        }

        internal Type CodeDomProviderType
        {
            get
            {
                return this._compilerType.CodeDomProviderType;
            }
        }

        internal string CultureName
        {
            get
            {
                return this._cultureName;
            }
            set
            {
                this._cultureName = value;
            }
        }

        internal bool IsBatchFull
        {
            get
            {
                if (this._sourceFiles.Count < this._maxBatchSize)
                {
                    return (this._totalFileLength >= this._maxBatchGeneratedFileSize);
                }
                return true;
            }
        }

        private string OutputAssemblyName
        {
            get
            {
                if (this._outputAssemblyName == null)
                {
                    string fileName = Path.GetFileName(this._tempFiles.BasePath);
                    this._outputAssemblyName = "App_Web_" + fileName;
                }
                return this._outputAssemblyName;
            }
        }

        internal System.Web.StringResourceBuilder StringResourceBuilder
        {
            get
            {
                if (this._stringResourceBuilder == null)
                {
                    this._stringResourceBuilder = new System.Web.StringResourceBuilder();
                }
                return this._stringResourceBuilder;
            }
        }

        private string TempFilePhysicalPathPrefix
        {
            get
            {
                if (this._tempFilePhysicalPathPrefix == null)
                {
                    this._tempFilePhysicalPathPrefix = Path.Combine(this._tempFiles.TempDir, this.OutputAssemblyName) + ".";
                    if (this.CultureName != null)
                    {
                        this._tempFilePhysicalPathPrefix = this._tempFilePhysicalPathPrefix + this.CultureName + "_";
                    }
                }
                return this._tempFilePhysicalPathPrefix;
            }
        }
    }
}

