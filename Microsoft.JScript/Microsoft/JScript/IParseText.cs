namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("C1468187-3DA1-49df-ADF8-5F8600E59EA8"), ComVisible(true)]
    public interface IParseText
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Parse(string code, IErrorHandler error);
    }
}

