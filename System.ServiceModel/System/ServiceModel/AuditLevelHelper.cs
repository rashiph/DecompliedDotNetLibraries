namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;

    internal static class AuditLevelHelper
    {
        public static bool IsDefined(AuditLevel auditLevel)
        {
            if (((auditLevel != AuditLevel.None) && (auditLevel != AuditLevel.Success)) && (auditLevel != AuditLevel.Failure))
            {
                return (auditLevel == AuditLevel.SuccessOrFailure);
            }
            return true;
        }

        public static void Validate(AuditLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(AuditLevel)));
            }
        }
    }
}

