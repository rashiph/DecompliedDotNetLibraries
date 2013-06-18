namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public class BaseVsaSite : IJSVsaSite
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual void GetCompiledState(out byte[] pe, out byte[] debugInfo)
        {
            pe = this.Assembly;
            debugInfo = this.DebugInfo;
        }

        public virtual object GetEventSourceInstance(string itemName, string eventSourceName)
        {
            throw new JSVsaException(JSVsaError.CallbackUnexpected);
        }

        public virtual object GetGlobalInstance(string name)
        {
            throw new JSVsaException(JSVsaError.CallbackUnexpected);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual void Notify(string notify, object optional)
        {
            throw new JSVsaException(JSVsaError.CallbackUnexpected);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual bool OnCompilerError(IJSVsaError error)
        {
            return false;
        }

        public virtual byte[] Assembly
        {
            get
            {
                throw new JSVsaException(JSVsaError.GetCompiledStateFailed);
            }
        }

        public virtual byte[] DebugInfo
        {
            get
            {
                return null;
            }
        }
    }
}

