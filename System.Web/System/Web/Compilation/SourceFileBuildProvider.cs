namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Web.UI;

    internal sealed class SourceFileBuildProvider : InternalBuildProvider
    {
        private BuildProvider _owningBuildProvider;
        private CodeSnippetCompileUnit _snippetCompileUnit;

        private void EnsureCodeCompileUnit()
        {
            if (this._snippetCompileUnit == null)
            {
                string str = Util.StringFromVirtualPath(base.VirtualPathObject);
                this._snippetCompileUnit = new CodeSnippetCompileUnit(str);
                this._snippetCompileUnit.LinePragma = BaseCodeDomTreeGenerator.CreateCodeLinePragmaHelper(base.VirtualPath, 1);
            }
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            this.EnsureCodeCompileUnit();
            assemblyBuilder.AddCodeCompileUnit(this, this._snippetCompileUnit);
        }

        protected internal override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            this.EnsureCodeCompileUnit();
            linePragmasTable = new Hashtable();
            linePragmasTable[1] = this._snippetCompileUnit.LinePragma;
            return this._snippetCompileUnit;
        }

        public override CompilerType CodeCompilerType
        {
            get
            {
                return CompilationUtil.GetCompilerInfoFromVirtualPath(base.VirtualPathObject);
            }
        }

        internal BuildProvider OwningBuildProvider
        {
            get
            {
                return this._owningBuildProvider;
            }
            set
            {
                this._owningBuildProvider = value;
            }
        }
    }
}

