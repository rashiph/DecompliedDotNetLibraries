namespace System.CodeDom.Compiler
{
    using System;
    using System.CodeDom;
    using System.IO;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class CodeParser : ICodeParser
    {
        protected CodeParser()
        {
        }

        public abstract CodeCompileUnit Parse(TextReader codeStream);
    }
}

