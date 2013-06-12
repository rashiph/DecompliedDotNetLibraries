namespace System.IO.Pipes
{
    using System;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class PipeAccessRule : AccessRule
    {
        public PipeAccessRule(IdentityReference identity, System.IO.Pipes.PipeAccessRights rights, AccessControlType type) : this(identity, AccessMaskFromRights(rights, type), false, type)
        {
        }

        public PipeAccessRule(string identity, System.IO.Pipes.PipeAccessRights rights, AccessControlType type) : this(new NTAccount(identity), AccessMaskFromRights(rights, type), false, type)
        {
        }

        internal PipeAccessRule(IdentityReference identity, int accessMask, bool isInherited, AccessControlType type) : base(identity, accessMask, isInherited, InheritanceFlags.None, PropagationFlags.None, type)
        {
        }

        internal static int AccessMaskFromRights(System.IO.Pipes.PipeAccessRights rights, AccessControlType controlType)
        {
            if ((rights < 0) || (rights > (System.IO.Pipes.PipeAccessRights.AccessSystemSecurity | System.IO.Pipes.PipeAccessRights.FullControl)))
            {
                throw new ArgumentOutOfRangeException("rights", System.SR.GetString("ArgumentOutOfRange_NeedValidPipeAccessRights"));
            }
            if (controlType == AccessControlType.Allow)
            {
                rights |= System.IO.Pipes.PipeAccessRights.Synchronize;
            }
            else if ((controlType == AccessControlType.Deny) && (rights != System.IO.Pipes.PipeAccessRights.FullControl))
            {
                rights &= ~System.IO.Pipes.PipeAccessRights.Synchronize;
            }
            return (int) rights;
        }

        internal static System.IO.Pipes.PipeAccessRights RightsFromAccessMask(int accessMask)
        {
            return (System.IO.Pipes.PipeAccessRights) accessMask;
        }

        public System.IO.Pipes.PipeAccessRights PipeAccessRights
        {
            get
            {
                return RightsFromAccessMask(base.AccessMask);
            }
        }
    }
}

