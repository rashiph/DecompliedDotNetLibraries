namespace System.ServiceModel.Description
{
    using System;

    internal static class PrincipalPermissionModeHelper
    {
        public static bool IsDefined(PrincipalPermissionMode principalPermissionMode)
        {
            if (((principalPermissionMode != PrincipalPermissionMode.None) && (principalPermissionMode != PrincipalPermissionMode.UseWindowsGroups)) && (principalPermissionMode != PrincipalPermissionMode.UseAspNetRoles))
            {
                return (principalPermissionMode == PrincipalPermissionMode.Custom);
            }
            return true;
        }
    }
}

