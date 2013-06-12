namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class CryptoKeyAccessRule : AccessRule
    {
        public CryptoKeyAccessRule(IdentityReference identity, System.Security.AccessControl.CryptoKeyRights cryptoKeyRights, AccessControlType type) : this(identity, AccessMaskFromRights(cryptoKeyRights, type), false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        public CryptoKeyAccessRule(string identity, System.Security.AccessControl.CryptoKeyRights cryptoKeyRights, AccessControlType type) : this(new NTAccount(identity), AccessMaskFromRights(cryptoKeyRights, type), false, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        private CryptoKeyAccessRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, type)
        {
        }

        private static int AccessMaskFromRights(System.Security.AccessControl.CryptoKeyRights cryptoKeyRights, AccessControlType controlType)
        {
            if (controlType == AccessControlType.Allow)
            {
                cryptoKeyRights |= System.Security.AccessControl.CryptoKeyRights.Synchronize;
            }
            else
            {
                if (controlType != AccessControlType.Deny)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidEnumValue", new object[] { controlType, "controlType" }), "controlType");
                }
                if (cryptoKeyRights != System.Security.AccessControl.CryptoKeyRights.FullControl)
                {
                    cryptoKeyRights &= ~System.Security.AccessControl.CryptoKeyRights.Synchronize;
                }
            }
            return (int) cryptoKeyRights;
        }

        internal static System.Security.AccessControl.CryptoKeyRights RightsFromAccessMask(int accessMask)
        {
            return (System.Security.AccessControl.CryptoKeyRights) accessMask;
        }

        public System.Security.AccessControl.CryptoKeyRights CryptoKeyRights
        {
            get
            {
                return RightsFromAccessMask(base.AccessMask);
            }
        }
    }
}

