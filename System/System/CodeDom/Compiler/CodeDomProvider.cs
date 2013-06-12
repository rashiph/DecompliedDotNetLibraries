namespace System.CodeDom.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ToolboxItem(false), ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeDomProvider : Component
    {
        protected CodeDomProvider()
        {
        }

        public virtual CompilerResults CompileAssemblyFromDom(CompilerParameters options, params CodeCompileUnit[] compilationUnits)
        {
            return this.CreateCompilerHelper().CompileAssemblyFromDomBatch(options, compilationUnits);
        }

        public virtual CompilerResults CompileAssemblyFromFile(CompilerParameters options, params string[] fileNames)
        {
            return this.CreateCompilerHelper().CompileAssemblyFromFileBatch(options, fileNames);
        }

        public virtual CompilerResults CompileAssemblyFromSource(CompilerParameters options, params string[] sources)
        {
            return this.CreateCompilerHelper().CompileAssemblyFromSourceBatch(options, sources);
        }

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public abstract ICodeCompiler CreateCompiler();
        private ICodeCompiler CreateCompilerHelper()
        {
            ICodeCompiler compiler = this.CreateCompiler();
            if (compiler == null)
            {
                throw new NotImplementedException(System.SR.GetString("NotSupported_CodeDomAPI"));
            }
            return compiler;
        }

        public virtual string CreateEscapedIdentifier(string value)
        {
            return this.CreateGeneratorHelper().CreateEscapedIdentifier(value);
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public abstract ICodeGenerator CreateGenerator();
        public virtual ICodeGenerator CreateGenerator(TextWriter output)
        {
            return this.CreateGenerator();
        }

        public virtual ICodeGenerator CreateGenerator(string fileName)
        {
            return this.CreateGenerator();
        }

        private ICodeGenerator CreateGeneratorHelper()
        {
            ICodeGenerator generator = this.CreateGenerator();
            if (generator == null)
            {
                throw new NotImplementedException(System.SR.GetString("NotSupported_CodeDomAPI"));
            }
            return generator;
        }

        [Obsolete("Callers should not use the ICodeParser interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public virtual ICodeParser CreateParser()
        {
            return null;
        }

        private ICodeParser CreateParserHelper()
        {
            ICodeParser parser = this.CreateParser();
            if (parser == null)
            {
                throw new NotImplementedException(System.SR.GetString("NotSupported_CodeDomAPI"));
            }
            return parser;
        }

        [ComVisible(false)]
        public static CodeDomProvider CreateProvider(string language)
        {
            return GetCompilerInfo(language).CreateProvider();
        }

        [ComVisible(false)]
        public static CodeDomProvider CreateProvider(string language, IDictionary<string, string> providerOptions)
        {
            return GetCompilerInfo(language).CreateProvider(providerOptions);
        }

        public virtual string CreateValidIdentifier(string value)
        {
            return this.CreateGeneratorHelper().CreateValidIdentifier(value);
        }

        public virtual void GenerateCodeFromCompileUnit(CodeCompileUnit compileUnit, TextWriter writer, CodeGeneratorOptions options)
        {
            this.CreateGeneratorHelper().GenerateCodeFromCompileUnit(compileUnit, writer, options);
        }

        public virtual void GenerateCodeFromExpression(CodeExpression expression, TextWriter writer, CodeGeneratorOptions options)
        {
            this.CreateGeneratorHelper().GenerateCodeFromExpression(expression, writer, options);
        }

        public virtual void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            throw new NotImplementedException(System.SR.GetString("NotSupported_CodeDomAPI"));
        }

        public virtual void GenerateCodeFromNamespace(CodeNamespace codeNamespace, TextWriter writer, CodeGeneratorOptions options)
        {
            this.CreateGeneratorHelper().GenerateCodeFromNamespace(codeNamespace, writer, options);
        }

        public virtual void GenerateCodeFromStatement(CodeStatement statement, TextWriter writer, CodeGeneratorOptions options)
        {
            this.CreateGeneratorHelper().GenerateCodeFromStatement(statement, writer, options);
        }

        public virtual void GenerateCodeFromType(CodeTypeDeclaration codeType, TextWriter writer, CodeGeneratorOptions options)
        {
            this.CreateGeneratorHelper().GenerateCodeFromType(codeType, writer, options);
        }

        [ComVisible(false)]
        public static CompilerInfo[] GetAllCompilerInfo()
        {
            return (CompilerInfo[]) Config._allCompilerInfo.ToArray(typeof(CompilerInfo));
        }

        [ComVisible(false)]
        public static CompilerInfo GetCompilerInfo(string language)
        {
            CompilerInfo compilerInfoForLanguageNoThrow = GetCompilerInfoForLanguageNoThrow(language);
            if (compilerInfoForLanguageNoThrow == null)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("CodeDomProvider_NotDefined"));
            }
            return compilerInfoForLanguageNoThrow;
        }

        private static CompilerInfo GetCompilerInfoForExtensionNoThrow(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException("extension");
            }
            return (CompilerInfo) Config._compilerExtensions[extension.Trim()];
        }

        private static CompilerInfo GetCompilerInfoForLanguageNoThrow(string language)
        {
            if (language == null)
            {
                throw new ArgumentNullException("language");
            }
            return (CompilerInfo) Config._compilerLanguages[language.Trim()];
        }

        public virtual TypeConverter GetConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type);
        }

        [ComVisible(false)]
        public static string GetLanguageFromExtension(string extension)
        {
            CompilerInfo compilerInfoForExtensionNoThrow = GetCompilerInfoForExtensionNoThrow(extension);
            if (compilerInfoForExtensionNoThrow == null)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("CodeDomProvider_NotDefined"));
            }
            return compilerInfoForExtensionNoThrow._compilerLanguages[0];
        }

        public virtual string GetTypeOutput(CodeTypeReference type)
        {
            return this.CreateGeneratorHelper().GetTypeOutput(type);
        }

        [ComVisible(false)]
        public static bool IsDefinedExtension(string extension)
        {
            return (GetCompilerInfoForExtensionNoThrow(extension) != null);
        }

        [ComVisible(false)]
        public static bool IsDefinedLanguage(string language)
        {
            return (GetCompilerInfoForLanguageNoThrow(language) != null);
        }

        public virtual bool IsValidIdentifier(string value)
        {
            return this.CreateGeneratorHelper().IsValidIdentifier(value);
        }

        public virtual CodeCompileUnit Parse(TextReader codeStream)
        {
            return this.CreateParserHelper().Parse(codeStream);
        }

        public virtual bool Supports(GeneratorSupport generatorSupport)
        {
            return this.CreateGeneratorHelper().Supports(generatorSupport);
        }

        private static CodeDomCompilationConfiguration Config
        {
            get
            {
                CodeDomCompilationConfiguration section = (CodeDomCompilationConfiguration) System.Configuration.PrivilegedConfigurationManager.GetSection("system.codedom");
                if (section == null)
                {
                    return CodeDomCompilationConfiguration.Default;
                }
                return section;
            }
        }

        public virtual string FileExtension
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual System.CodeDom.Compiler.LanguageOptions LanguageOptions
        {
            get
            {
                return System.CodeDom.Compiler.LanguageOptions.None;
            }
        }
    }
}

