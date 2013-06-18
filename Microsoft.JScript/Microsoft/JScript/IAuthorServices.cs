namespace Microsoft.JScript
{
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Guid("9E2B453C-6EAA-4329-A619-62E4889C8C8A")]
    public interface IAuthorServices
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        IColorizeText GetColorizer();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        IParseText GetCodeSense();
    }
}

