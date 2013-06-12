namespace System.IO.Pipes
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class PipeAuditRule : AuditRule
    {
        public PipeAuditRule(IdentityReference identity, System.IO.Pipes.PipeAccessRights rights, AuditFlags flags) : this(identity, AccessMaskFromRights(rights), false, flags)
        {
        }

        public PipeAuditRule(string identity, System.IO.Pipes.PipeAccessRights rights, AuditFlags flags) : this(new NTAccount(identity), AccessMaskFromRights(rights), false, flags)
        {
        }

        internal PipeAuditRule(IdentityReference identity, int accessMask, bool isInherited, AuditFlags flags) : base(identity, accessMask, isInherited, InheritanceFlags.None, PropagationFlags.None, flags)
        {
        }

        private static int AccessMaskFromRights(System.IO.Pipes.PipeAccessRights rights)
        {
            if ((rights < 0) || (rights > (System.IO.Pipes.PipeAccessRights.AccessSystemSecurity | System.IO.Pipes.PipeAccessRights.FullControl)))
            {
                throw new ArgumentOutOfRangeException("rights", System.SR.GetString("ArgumentOutOfRange_NeedValidPipeAccessRights"));
            }
            return (int) rights;
        }

        public System.IO.Pipes.PipeAccessRights PipeAccessRights
        {
            get
            {
                return PipeAccessRule.RightsFromAccessMask(base.AccessMask);
            }
        }
    }
}

