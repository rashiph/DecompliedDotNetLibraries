namespace System.ServiceModel.Activation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.Web.Compilation;

    [SecurityCritical(SecurityCriticalScope.Everything), BuildProviderAppliesTo(BuildProviderAppliesTo.Web), ServiceActivationBuildProvider]
    public sealed class ServiceBuildProvider : BuildProvider
    {
        private ServiceParser parser;

        private void EnsureParsed()
        {
            if (this.parser == null)
            {
                this.parser = new ServiceParser(base.VirtualPath, this);
                this.parser.Parse(base.ReferencedAssemblies);
            }
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            this.GenerateCodeCore(assemblyBuilder);
        }

        private void GenerateCodeCore(AssemblyBuilder assemblyBuilder)
        {
            if (assemblyBuilder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assemblyBuilder");
            }
            CodeCompileUnit codeModel = this.parser.GetCodeModel();
            if (codeModel != null)
            {
                assemblyBuilder.AddCodeCompileUnit(this, codeModel);
                if (this.parser.AssemblyDependencies != null)
                {
                    foreach (Assembly assembly in this.parser.AssemblyDependencies)
                    {
                        assemblyBuilder.AddAssemblyReference(assembly);
                    }
                }
            }
        }

        private CompilerType GetCodeCompilerType()
        {
            this.EnsureParsed();
            return this.parser.CompilerType;
        }

        protected override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            CodeSnippetCompileUnit codeModel = this.parser.GetCodeModel() as CodeSnippetCompileUnit;
            linePragmasTable = this.parser.GetLinePragmasTable();
            return codeModel;
        }

        public override string GetCustomString(CompilerResults results)
        {
            return this.GetCustomStringCore(results);
        }

        private string GetCustomStringCore(CompilerResults results)
        {
            return this.parser.CreateParseString((results == null) ? null : results.CompiledAssembly);
        }

        internal CompilerType GetDefaultCompilerTypeForLanguageInternal(string language)
        {
            return base.GetDefaultCompilerTypeForLanguage(language);
        }

        internal CompilerType GetDefaultCompilerTypeInternal()
        {
            return base.GetDefaultCompilerType();
        }

        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        }

        internal TextReader OpenReaderInternal()
        {
            return base.OpenReader();
        }

        public override CompilerType CodeCompilerType
        {
            get
            {
                return this.GetCodeCompilerType();
            }
        }

        public override ICollection VirtualPathDependencies
        {
            get
            {
                return this.parser.SourceDependencies;
            }
        }
    }
}

