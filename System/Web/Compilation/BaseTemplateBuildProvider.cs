namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class BaseTemplateBuildProvider : InternalBuildProvider
    {
        private string _instantiatableFullTypeName;
        private string _intermediateFullTypeName;
        private TemplateParser _parser;

        protected BaseTemplateBuildProvider()
        {
        }

        internal override BuildResultCompiledType CreateBuildResult(Type t)
        {
            return new BuildResultCompiledTemplateType(t);
        }

        internal abstract BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser);
        protected abstract TemplateParser CreateParser();
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            if (this.Parser.RequiresCompilation)
            {
                BaseCodeDomTreeGenerator generator = this.CreateCodeDomTreeGenerator(this._parser);
                CodeCompileUnit ccu = generator.GetCodeDomTree(assemblyBuilder.CodeDomProvider, assemblyBuilder.StringResourceBuilder, base.VirtualPathObject);
                if (ccu != null)
                {
                    if (this._parser.AssemblyDependencies != null)
                    {
                        foreach (Assembly assembly in (IEnumerable) this._parser.AssemblyDependencies)
                        {
                            assemblyBuilder.AddAssemblyReference(assembly, ccu);
                        }
                    }
                    assemblyBuilder.AddCodeCompileUnit(this, ccu);
                }
                this._instantiatableFullTypeName = generator.GetInstantiatableFullTypeName();
                if (this._instantiatableFullTypeName != null)
                {
                    assemblyBuilder.GenerateTypeFactory(this._instantiatableFullTypeName);
                }
                this._intermediateFullTypeName = generator.GetIntermediateFullTypeName();
            }
        }

        protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            CodeDomProvider codeDomProvider = CompilationUtil.CreateCodeDomProviderNonPublic(this._parser.CompilerType.CodeDomProviderType);
            BaseCodeDomTreeGenerator generator = this.CreateCodeDomTreeGenerator(this._parser);
            generator.SetDesignerMode();
            CodeCompileUnit unit = generator.GetCodeDomTree(codeDomProvider, new StringResourceBuilder(), base.VirtualPathObject);
            linePragmasTable = generator.LinePragmasTable;
            return unit;
        }

        internal override ICollection GetCompileWithDependencies()
        {
            if (this._parser.CodeFileVirtualPath == null)
            {
                return null;
            }
            return new SingleObjectCollection(this._parser.CodeFileVirtualPath);
        }

        public override Type GetGeneratedType(CompilerResults results)
        {
            bool useDelayLoadTypeIfEnabled = false;
            return this.GetGeneratedType(results, useDelayLoadTypeIfEnabled);
        }

        internal Type GetGeneratedType(CompilerResults results, bool useDelayLoadTypeIfEnabled)
        {
            string str;
            if (!this.Parser.RequiresCompilation)
            {
                return null;
            }
            if (this._instantiatableFullTypeName == null)
            {
                if (this.Parser.CodeFileVirtualPath == null)
                {
                    return this.Parser.BaseType;
                }
                str = this._intermediateFullTypeName;
            }
            else
            {
                str = this._instantiatableFullTypeName;
            }
            if (useDelayLoadTypeIfEnabled && DelayLoadType.Enabled)
            {
                return new DelayLoadType(Util.GetAssemblyNameFromFileName(Path.GetFileName(results.PathToAssembly)), str);
            }
            return results.CompiledAssembly.GetType(str);
        }

        internal override ICollection GetGeneratedTypeNames()
        {
            if ((this._parser.GeneratedClassName == null) && (this._parser.BaseTypeName == null))
            {
                return null;
            }
            ArrayList list = new ArrayList();
            if (this._parser.GeneratedClassName != null)
            {
                list.Add(this._parser.GeneratedClassName);
            }
            if (this._parser.BaseTypeName != null)
            {
                list.Add(Util.MakeFullTypeName(this._parser.BaseTypeNamespace, this._parser.BaseTypeName));
            }
            return list;
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
                if (this.IgnoreParseErrors)
                {
                    this._parser.IgnoreParseErrors = true;
                }
                if (base.IgnoreControlProperties)
                {
                    this._parser.IgnoreControlProperties = true;
                }
                if (!base.ThrowOnFirstParseError)
                {
                    this._parser.ThrowOnFirstParseError = false;
                }
                this._parser.Parse(base.ReferencedAssemblies, base.VirtualPathObject);
                if (!this.Parser.RequiresCompilation)
                {
                    return null;
                }
                return this._parser.CompilerType;
            }
        }

        internal TemplateParser Parser
        {
            get
            {
                return this._parser;
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

