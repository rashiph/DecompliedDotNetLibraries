namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class CryptoKeyAuditRule : AuditRule
    {
        public CryptoKeyAuditRule(IdentityReference identity, System.Security.AccessControl.CryptoKeyRights cryptoKeyRights, AuditFlags flags) : this(identity, AccessMaskFromRights(cryptoKeyRights), false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        public CryptoKeyAuditRule(string identity, System.Security.AccessControl.CryptoKeyRights cryptoKeyRights, AuditFlags flags) : this(new NTAccount(identity), AccessMaskFromRights(cryptoKeyRights), false, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        private CryptoKeyAuditRule(IdentityReference identity, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags) : base(identity, accessMask, isInherited, inheritanceFlags, propagationFlags, flags)
        {
        }

        private static int AccessMaskFromRights(System.Security.AccessControl.CryptoKeyRights cryptoKeyRights)
        {
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

