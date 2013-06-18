namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("0E4EFFC0-2387-11d3-B372-00105A98B7CE"), ComVisible(true)]
    public class JSAuthor : IAuthorServices
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual IParseText GetCodeSense()
        {
            return new JSCodeSense();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public virtual IColorizeText GetColorizer()
        {
            return new JSColorizer();
        }
    }
}

