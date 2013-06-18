namespace System.Runtime
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class PartialTrustHelpers
    {
        [SecurityCritical]
        private static Type aptca;

        [SecurityCritical]
        internal static SecurityContext CaptureSecurityContextNoIdentityFlow()
        {
            if (SecurityContext.IsWindowsIdentityFlowSuppressed())
            {
                return SecurityContext.Capture();
            }
            using (SecurityContext.SuppressFlowWindowsIdentity())
            {
                return SecurityContext.Capture();
            }
        }

        [SecurityCritical]
        internal static bool CheckAppDomainPermissions(PermissionSet permissions)
        {
            return (AppDomain.CurrentDomain.IsHomogenous && permissions.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet));
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical, PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        private static void DemandForFullTrust()
        {
        }

        [SecurityCritical]
        internal static bool HasEtwPermissions()
        {
            PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted);
            return CheckAppDomainPermissions(permissions);
        }

        [SecurityCritical]
        private static bool IsAssemblyAptca(Assembly assembly)
        {
            if (aptca == null)
            {
                aptca = typeof(AllowPartiallyTrustedCallersAttribute);
            }
            return (assembly.GetCustomAttributes(aptca, false).Length > 0);
        }

        [SecurityCritical, FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private static bool IsAssemblySigned(Assembly assembly)
        {
            byte[] publicKeyToken = assembly.GetName().GetPublicKeyToken();
            return ((publicKeyToken != null) & (publicKeyToken.Length > 0));
        }

        [SecurityCritical]
        internal static bool IsInFullTrust()
        {
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                return AppDomain.CurrentDomain.IsFullyTrusted;
            }
            if (!SecurityManager.CurrentThreadRequiresSecurityContextCapture())
            {
                return true;
            }
            try
            {
                DemandForFullTrust();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        [SecurityCritical]
        internal static bool IsTypeAptca(Type type)
        {
            Assembly assembly = type.Assembly;
            if (!IsAssemblyAptca(assembly))
            {
                return !IsAssemblySigned(assembly);
            }
            return true;
        }

        internal static bool ShouldFlowSecurityContext
        {
            [SecurityCritical]
            get
            {
                if (AppDomain.CurrentDomain.IsHomogenous)
                {
                    return false;
                }
                return SecurityManager.CurrentThreadRequiresSecurityContextCapture();
            }
        }
    }
}

