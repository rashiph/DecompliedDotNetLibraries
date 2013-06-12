namespace System.Security.Cryptography
{
    using System.Collections.ObjectModel;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngPropertyCollection : Collection<CngProperty>
    {
    }
}

