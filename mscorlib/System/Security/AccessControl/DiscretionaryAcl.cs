namespace System.Security.AccessControl
{
    using System;
    using System.Security.Principal;

    public sealed class DiscretionaryAcl : CommonAcl
    {
        private static SecurityIdentifier _sidEveryone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        private bool everyOneFullAccessForNullDacl;

        public DiscretionaryAcl(bool isContainer, bool isDS, int capacity) : this(isContainer, isDS, isDS ? GenericAcl.AclRevisionDS : GenericAcl.AclRevision, capacity)
        {
        }

        public DiscretionaryAcl(bool isContainer, bool isDS, RawAcl rawAcl) : this(isContainer, isDS, rawAcl, false)
        {
        }

        public DiscretionaryAcl(bool isContainer, bool isDS, byte revision, int capacity) : base(isContainer, isDS, revision, capacity)
        {
        }

        internal DiscretionaryAcl(bool isContainer, bool isDS, RawAcl rawAcl, bool trusted) : base(isContainer, isDS, (rawAcl == null) ? new RawAcl(isDS ? GenericAcl.AclRevisionDS : GenericAcl.AclRevision, 0) : rawAcl, trusted, true)
        {
        }

        public void AddAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckAccessType(accessType);
            base.CheckFlags(inheritanceFlags, propagationFlags);
            this.everyOneFullAccessForNullDacl = false;
            base.AddQualifiedAce(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void AddAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckAccessType(accessType);
            base.CheckFlags(inheritanceFlags, propagationFlags);
            this.everyOneFullAccessForNullDacl = false;
            base.AddQualifiedAce(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), objectFlags, objectType, inheritedObjectType);
        }

        internal static DiscretionaryAcl CreateAllowEveryoneFullAccess(bool isDS, bool isContainer)
        {
            DiscretionaryAcl acl = new DiscretionaryAcl(isContainer, isDS, 1);
            acl.AddAccess(AccessControlType.Allow, _sidEveryone, -1, isContainer ? (InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit) : InheritanceFlags.None, PropagationFlags.None);
            acl.everyOneFullAccessForNullDacl = true;
            return acl;
        }

        internal override void OnAclModificationTried()
        {
            this.everyOneFullAccessForNullDacl = false;
        }

        public bool RemoveAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckAccessType(accessType);
            this.everyOneFullAccessForNullDacl = false;
            return base.RemoveQualifiedAces(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), false, ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public bool RemoveAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckAccessType(accessType);
            this.everyOneFullAccessForNullDacl = false;
            return base.RemoveQualifiedAces(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), false, objectFlags, objectType, inheritedObjectType);
        }

        public void RemoveAccessSpecific(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckAccessType(accessType);
            this.everyOneFullAccessForNullDacl = false;
            base.RemoveQualifiedAcesSpecific(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void RemoveAccessSpecific(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckAccessType(accessType);
            this.everyOneFullAccessForNullDacl = false;
            base.RemoveQualifiedAcesSpecific(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), objectFlags, objectType, inheritedObjectType);
        }

        public void SetAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
        {
            base.CheckAccessType(accessType);
            base.CheckFlags(inheritanceFlags, propagationFlags);
            this.everyOneFullAccessForNullDacl = false;
            base.SetQualifiedAce(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), ObjectAceFlags.None, Guid.Empty, Guid.Empty);
        }

        public void SetAccess(AccessControlType accessType, SecurityIdentifier sid, int accessMask, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, ObjectAceFlags objectFlags, Guid objectType, Guid inheritedObjectType)
        {
            if (!base.IsDS)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_OnlyValidForDS"));
            }
            base.CheckAccessType(accessType);
            base.CheckFlags(inheritanceFlags, propagationFlags);
            this.everyOneFullAccessForNullDacl = false;
            base.SetQualifiedAce(sid, (accessType == AccessControlType.Allow) ? AceQualifier.AccessAllowed : AceQualifier.AccessDenied, accessMask, GenericAce.AceFlagsFromInheritanceFlags(inheritanceFlags, propagationFlags), objectFlags, objectType, inheritedObjectType);
        }

        internal bool EveryOneFullAccessForNullDacl
        {
            get
            {
                return this.everyOneFullAccessForNullDacl;
            }
            set
            {
                this.everyOneFullAccessForNullDacl = value;
            }
        }
    }
}

