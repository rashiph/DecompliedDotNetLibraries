namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal sealed class CompilerContext
    {
        private static CompilerContext m_empty;
        private IImportScope m_importScope;
        private CompilerOptions m_options;
        private IScriptScope m_scriptScope;
        private ITypeScope m_typeScope;

        public CompilerContext(IScriptScope scriptScope, ITypeScope typeScope, IImportScope importScope, CompilerOptions options)
        {
            this.m_scriptScope = scriptScope;
            this.m_typeScope = typeScope;
            this.m_importScope = importScope;
            this.m_options = options;
        }

        internal static CompilerContext Empty
        {
            get
            {
                if (m_empty == null)
                {
                    m_empty = new CompilerContext(null, null, null, null);
                }
                return m_empty;
            }
        }

        public IImportScope ImportScope
        {
            get
            {
                if (this.m_importScope == null)
                {
                    this.m_importScope = Microsoft.Compiler.VisualBasic.ImportScope.Empty;
                }
                return this.m_importScope;
            }
        }

        public CompilerOptions Options
        {
            get
            {
                if (this.m_options == null)
                {
                    this.m_options = new CompilerOptions();
                }
                return this.m_options;
            }
        }

        public IScriptScope ScriptScope
        {
            get
            {
                if (this.m_scriptScope == null)
                {
                    this.m_scriptScope = Microsoft.Compiler.VisualBasic.ScriptScope.Empty;
                }
                return this.m_scriptScope;
            }
        }

        public ITypeScope TypeScope
        {
            get
            {
                if (this.m_typeScope == null)
                {
                    this.m_typeScope = Microsoft.Compiler.VisualBasic.TypeScope.Empty;
                }
                return this.m_typeScope;
            }
        }
    }
}

