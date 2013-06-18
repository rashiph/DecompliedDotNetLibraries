namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Security;

    internal static class AuditLogLocationHelper
    {
        public static bool IsDefined(AuditLogLocation auditLogLocation)
        {
            if ((auditLogLocation == AuditLogLocation.Security) && !SecurityAuditHelper.IsSecurityAuditSupported)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PlatformNotSupportedException(System.ServiceModel.SR.GetString("SecurityAuditPlatformNotSupported")));
            }
            if ((auditLogLocation != AuditLogLocation.Default) && (auditLogLocation != AuditLogLocation.Application))
            {
                return (auditLogLocation == AuditLogLocation.Security);
            }
            return true;
        }

        public static void Validate(AuditLogLocation value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(AuditLogLocation)));
            }
        }
    }
}

