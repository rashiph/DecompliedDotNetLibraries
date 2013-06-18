namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using System.Web.Util;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    internal abstract class SimpleHandlerBuildProvider : InternalBuildProvider
    {
        private SimpleWebHandlerParser _parser;

        protected SimpleHandlerBuildProvider()
        {
        }

        protected abstract SimpleWebHandlerParser CreateParser();
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            CodeCompileUnit codeModel = this._parser.GetCodeModel();
            if (codeModel != null)
            {
                assemblyBuilder.AddCodeCompileUnit(this, codeModel);
                if (this._parser.AssemblyDependencies != null)
                {
                    foreach (Assembly assembly in this._parser.AssemblyDependencies)
                    {
                        assemblyBuilder.AddAssemblyReference(assembly, codeModel);
                    }
                }
            }
        }

        protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            CodeCompileUnit codeModel = this._parser.GetCodeModel();
            linePragmasTable = this._parser.GetLinePragmasTable();
            return codeModel;
        }

        internal CompilerType GetDefaultCompilerTypeForLanguageInternal(string language)
        {
            return base.GetDefaultCompilerTypeForLanguage(language);
        }

        internal CompilerType GetDefaultCompilerTypeInternal()
        {
            return base.GetDefaultCompilerType();
        }

        public override Type GetGeneratedType(CompilerResults results)
        {
            if (this._parser.HasInlineCode)
            {
                return this._parser.GetTypeToCache(results.CompiledAssembly);
            }
            return this._parser.GetTypeToCache(null);
        }

        internal override ICollection GetGeneratedTypeNames()
        {
            return new SingleObjectCollection(this._parser.TypeName);
        }

        internal TextReader OpenReaderInternal()
        {
            return base.OpenReader();
        }

        internal override IAssemblyDependencyParser AssemblyDependencyParser
        {
            get
            {
                return this._parser;
            }
        }

        public override CompilerType CodeCompilerType
        {
            get
            {
                this._parser = this.CreateParser();
                this._parser.SetBuildProvider(this);
                this._parser.IgnoreParseErrors = this.IgnoreParseErrors;
                this._parser.Parse(base.ReferencedAssemblies);
                return this._parser.CompilerType;
            }
        }

        public override ICollection VirtualPathDependencies
        {
            get
            {
                return this._parser.SourceDependencies;
            }
        }
    }
}

