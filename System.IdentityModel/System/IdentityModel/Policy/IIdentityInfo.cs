namespace System.IdentityModel.Policy
{
    using System.Security.Principal;

    internal interface IIdentityInfo
    {
        IIdentity Identity { get; }
    }
}

