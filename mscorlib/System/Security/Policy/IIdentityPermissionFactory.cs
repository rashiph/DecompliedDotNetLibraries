namespace System.Security.Policy
{
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public interface IIdentityPermissionFactory
    {
        IPermission CreateIdentityPermission(Evidence evidence);
    }
}

