namespace Microsoft.Compiler.VisualBasic
{
    using System;

    internal interface IScriptScope
    {
        Type FindVariable(string name);
    }
}

