namespace System.Web.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Security.Permissions;
    using System.Web;

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ClientBuildManagerCallback : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public virtual void ReportCompilerError(CompilerError error)
        {
        }

        public virtual void ReportParseError(ParserError error)
        {
        }

        public virtual void ReportProgress(string message)
        {
        }
    }
}

