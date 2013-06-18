namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel;

    internal static class ProtectionLevelHelper
    {
        internal static int GetOrdinal(ProtectionLevel? p)
        {
            if (!p.HasValue)
            {
                return 1;
            }
            switch (p.Value)
            {
                case ProtectionLevel.None:
                    return 2;

                case ProtectionLevel.Sign:
                    return 3;

                case ProtectionLevel.EncryptAndSign:
                    return 4;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("p", p.Value, typeof(ProtectionLevel)));
        }

        internal static bool IsDefined(ProtectionLevel value)
        {
            if ((value != ProtectionLevel.None) && (value != ProtectionLevel.Sign))
            {
                return (value == ProtectionLevel.EncryptAndSign);
            }
            return true;
        }

        internal static bool IsStronger(ProtectionLevel v1, ProtectionLevel v2)
        {
            return (((v1 == ProtectionLevel.EncryptAndSign) && (v2 != ProtectionLevel.EncryptAndSign)) || ((v1 == ProtectionLevel.Sign) && (v2 == ProtectionLevel.None)));
        }

        internal static bool IsStrongerOrEqual(ProtectionLevel v1, ProtectionLevel v2)
        {
            return ((v1 == ProtectionLevel.EncryptAndSign) || ((v1 == ProtectionLevel.Sign) && (v2 != ProtectionLevel.EncryptAndSign)));
        }

        internal static ProtectionLevel Max(ProtectionLevel v1, ProtectionLevel v2)
        {
            if (!IsStronger(v1, v2))
            {
                return v2;
            }
            return v1;
        }

        internal static void Validate(ProtectionLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(ProtectionLevel)));
            }
        }
    }
}

