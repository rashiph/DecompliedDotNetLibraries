namespace Microsoft.VisualBasic.ApplicationServices
{
    using Microsoft.VisualBasic.CompilerServices;
    using Microsoft.VisualBasic.MyServices.Internal;
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class WebUser : User
    {
        protected override IPrincipal InternalPrincipal
        {
            get
            {
                object current = SkuSafeHttpContext.Current;
                if (current == null)
                {
                    throw ExceptionUtils.GetInvalidOperationException("WebNotSupportedOnThisSKU", new string[0]);
                }
                return (IPrincipal) NewLateBinding.LateGet(current, null, "User", new object[0], null, null, null);
            }
            set
            {
                object current = SkuSafeHttpContext.Current;
                if (current == null)
                {
                    throw ExceptionUtils.GetInvalidOperationException("WebNotSupportedOnThisSKU", new string[0]);
                }
                NewLateBinding.LateSet(current, null, "User", new object[] { value }, null, null);
            }
        }
    }
}

