namespace Microsoft.JScript.Vsa
{
    using Microsoft.JScript;
    using System;

    internal class DefaultVsaSite : BaseVsaSite
    {
        public override bool OnCompilerError(IJSVsaError error)
        {
            throw ((JScriptException) error);
        }
    }
}

