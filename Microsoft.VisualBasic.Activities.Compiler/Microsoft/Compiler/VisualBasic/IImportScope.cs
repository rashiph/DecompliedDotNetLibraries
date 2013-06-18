namespace Microsoft.Compiler.VisualBasic
{
    using System.Collections.Generic;

    internal interface IImportScope
    {
        IList<Import> GetImports();
    }
}

