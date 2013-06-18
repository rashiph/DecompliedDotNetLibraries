namespace Microsoft.Compiler.VisualBasic
{
    using System;
    using System.Collections.Generic;

    internal class ImportScope : IImportScope
    {
        private static ImportScope m_empty;

        private ImportScope()
        {
        }

        public virtual IList<Import> GetImports()
        {
            return null;
        }

        internal static ImportScope Empty
        {
            get
            {
                if (m_empty == null)
                {
                    m_empty = new ImportScope();
                }
                return m_empty;
            }
        }
    }
}

