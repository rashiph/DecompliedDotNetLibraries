namespace Microsoft.JScript
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Security.Permissions;

    [DesignerCategory("code"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class JScriptCodeProvider : CodeDomProvider
    {
        private JSCodeGenerator generator = new JSCodeGenerator();

        public override ICodeCompiler CreateCompiler()
        {
            return this.generator;
        }

        public override ICodeGenerator CreateGenerator()
        {
            return this.generator;
        }

        public override string FileExtension
        {
            get
            {
                return "js";
            }
        }
    }
}

