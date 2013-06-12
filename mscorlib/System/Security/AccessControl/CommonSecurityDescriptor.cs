namespace System.Security.AccessControl
{
    using System;
    using System.Security;
    using System.Security.Principal;

    public sealed class CommonSecurityDescriptor : GenericSecurityDescriptor
    {
        private System.Security.AccessControl.DiscretionaryAcl _dacl;
        private bool _isContainer;
        private bool _isDS;
        private RawSecurityDescriptor _rawSd;
        private System.Security.AccessControl.SystemAcl _sacl;

        public CommonSecurityDescriptor(bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor) : this(isContainer, isDS, rawSecurityDescriptor, false)
        {
        }

        [SecuritySafeCritical]
        public CommonSecurityDescriptor(bool isContainer, bool isDS, string sddlForm) : this(isContainer, isDS, new RawSecurityDescriptor(sddlForm), true)
        {
        }

        internal CommonSecurityDescriptor(bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor, bool trusted)
        {
            if (rawSecurityDescriptor == null)
            {
                throw new ArgumentNullException("rawSecurityDescriptor");
            }
            this.CreateFromParts(isContainer, isDS, rawSecurityDescriptor.ControlFlags, rawSecurityDescriptor.Owner, rawSecurityDescriptor.Group, (rawSecurityDescriptor.SystemAcl == null) ? null : new System.Security.AccessControl.SystemAcl(isContainer, isDS, rawSecurityDescriptor.SystemAcl, trusted), (rawSecurityDescriptor.DiscretionaryAcl == null) ? null : new System.Security.AccessControl.DiscretionaryAcl(isContainer, isDS, rawSecurityDescriptor.DiscretionaryAcl, trusted));
        }

        public CommonSecurityDescriptor(bool isContainer, bool isDS, byte[] binaryForm, int offset) : this(isContainer, isDS, new RawSecurityDescriptor(binaryForm, offset), true)
        {
        }

        private CommonSecurityDescriptor(bool isContainer, bool isDS, System.Security.AccessControl.ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, RawAcl systemAcl, RawAcl discretionaryAcl) : this(isContainer, isDS, flags, owner, group, (systemAcl == null) ? null : new System.Security.AccessControl.SystemAcl(isContainer, isDS, systemAcl), (discretionaryAcl == null) ? null : new System.Security.AccessControl.DiscretionaryAcl(isContainer, isDS, discretionaryAcl))
        {
        }

        public CommonSecurityDescriptor(bool isContainer, bool isDS, System.Security.AccessControl.ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, System.Security.AccessControl.SystemAcl systemAcl, System.Security.AccessControl.DiscretionaryAcl discretionaryAcl)
        {
            this.CreateFromParts(isContainer, isDS, flags, owner, group, systemAcl, discretionaryAcl);
        }

        internal void AddControlFlags(System.Security.AccessControl.ControlFlags flags)
        {
            this._rawSd.SetFlags(this._rawSd.ControlFlags | flags);
        }

        private void CreateFromParts(bool isContainer, bool isDS, System.Security.AccessControl.ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, System.Security.AccessControl.SystemAcl systemAcl, System.Security.AccessControl.DiscretionaryAcl discretionaryAcl)
        {
            if ((systemAcl != null) && (systemAcl.IsContainer != isContainer))
            {
                throw new ArgumentException(Environment.GetResourceString(isContainer ? "AccessControl_MustSpecifyContainerAcl" : "AccessControl_MustSpecifyLeafObjectAcl"), "systemAcl");
            }
            if ((discretionaryAcl != null) && (discretionaryAcl.IsContainer != isContainer))
            {
                throw new ArgumentException(Environment.GetResourceString(isContainer ? "AccessControl_MustSpecifyContainerAcl" : "AccessControl_MustSpecifyLeafObjectAcl"), "discretionaryAcl");
            }
            this._isContainer = isContainer;
            if ((systemAcl != null) && (systemAcl.IsDS != isDS))
            {
                throw new ArgumentException(Environment.GetResourceString(isDS ? "AccessControl_MustSpecifyDirectoryObjectAcl" : "AccessControl_MustSpecifyNonDirectoryObjectAcl"), "systemAcl");
            }
            if ((discretionaryAcl != null) && (discretionaryAcl.IsDS != isDS))
            {
                throw new ArgumentException(Environment.GetResourceString(isDS ? "AccessControl_MustSpecifyDirectoryObjectAcl" : "AccessControl_MustSpecifyNonDirectoryObjectAcl"), "discretionaryAcl");
            }
            this._isDS = isDS;
            this._sacl = systemAcl;
            if (discretionaryAcl == null)
            {
                discretionaryAcl = System.Security.AccessControl.DiscretionaryAcl.CreateAllowEveryoneFullAccess(this._isDS, this._isContainer);
            }
            this._dacl = discretionaryAcl;
            System.Security.AccessControl.ControlFlags flags2 = flags | System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent;
            if (systemAcl == null)
            {
                flags2 &= ~System.Security.AccessControl.ControlFlags.SystemAclPresent;
            }
            else
            {
                flags2 |= System.Security.AccessControl.ControlFlags.SystemAclPresent;
            }
            this._rawSd = new RawSecurityDescriptor(flags2, owner, group, (systemAcl == null) ? null : systemAcl.RawAcl, discretionaryAcl.RawAcl);
        }

        public void PurgeAccessControl(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            if (this.DiscretionaryAcl != null)
            {
                this.DiscretionaryAcl.Purge(sid);
            }
        }

        public void PurgeAudit(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            if (this.SystemAcl != null)
            {
                this.SystemAcl.Purge(sid);
            }
        }

        internal void RemoveControlFlags(System.Security.AccessControl.ControlFlags flags)
        {
            this._rawSd.SetFlags(this._rawSd.ControlFlags & ~flags);
        }

        public void SetDiscretionaryAclProtection(bool isProtected, bool preserveInheritance)
        {
            if (!isProtected)
            {
                this.RemoveControlFlags(System.Security.AccessControl.ControlFlags.DiscretionaryAclProtected);
            }
            else
            {
                if (!preserveInheritance && (this.DiscretionaryAcl != null))
                {
                    this.DiscretionaryAcl.RemoveInheritedAces();
                }
                this.AddControlFlags(System.Security.AccessControl.ControlFlags.DiscretionaryAclProtected);
            }
            if ((this.DiscretionaryAcl != null) && this.DiscretionaryAcl.EveryOneFullAccessForNullDacl)
            {
                this.DiscretionaryAcl.EveryOneFullAccessForNullDacl = false;
            }
        }

        public void SetSystemAclProtection(bool isProtected, bool preserveInheritance)
        {
            if (!isProtected)
            {
                this.RemoveControlFlags(System.Security.AccessControl.ControlFlags.SystemAclProtected);
            }
            else
            {
                if (!preserveInheritance && (this.SystemAcl != null))
                {
                    this.SystemAcl.RemoveInheritedAces();
                }
                this.AddControlFlags(System.Security.AccessControl.ControlFlags.SystemAclProtected);
            }
        }

        internal void UpdateControlFlags(System.Security.AccessControl.ControlFlags flagsToUpdate, System.Security.AccessControl.ControlFlags newFlags)
        {
            System.Security.AccessControl.ControlFlags flags = newFlags | (this._rawSd.ControlFlags & ~flagsToUpdate);
            this._rawSd.SetFlags(flags);
        }

        public override System.Security.AccessControl.ControlFlags ControlFlags
        {
            get
            {
                return this._rawSd.ControlFlags;
            }
        }

        public System.Security.AccessControl.DiscretionaryAcl DiscretionaryAcl
        {
            get
            {
                return this._dacl;
            }
            set
            {
                if (value != null)
                {
                    if (value.IsContainer != this.IsContainer)
                    {
                        throw new ArgumentException(Environment.GetResourceString(this.IsContainer ? "AccessControl_MustSpecifyContainerAcl" : "AccessControl_MustSpecifyLeafObjectAcl"), "value");
                    }
                    if (value.IsDS != this.IsDS)
                    {
                        throw new ArgumentException(Environment.GetResourceString(this.IsDS ? "AccessControl_MustSpecifyDirectoryObjectAcl" : "AccessControl_MustSpecifyNonDirectoryObjectAcl"), "value");
                    }
                }
                if (value == null)
                {
                    this._dacl = System.Security.AccessControl.DiscretionaryAcl.CreateAllowEveryoneFullAccess(this.IsDS, this.IsContainer);
                }
                else
                {
                    this._dacl = value;
                }
                this._rawSd.DiscretionaryAcl = this._dacl.RawAcl;
                this.AddControlFlags(System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent);
            }
        }

        internal sealed override GenericAcl GenericDacl
        {
            get
            {
                return this._dacl;
            }
        }

        internal sealed override GenericAcl GenericSacl
        {
            get
            {
                return this._sacl;
            }
        }

        public override SecurityIdentifier Group
        {
            get
            {
                return this._rawSd.Group;
            }
            set
            {
                this._rawSd.Group = value;
            }
        }

        public bool IsContainer
        {
            get
            {
                return this._isContainer;
            }
        }

        public bool IsDiscretionaryAclCanonical
        {
            get
            {
                if (this.DiscretionaryAcl != null)
                {
                    return this.DiscretionaryAcl.IsCanonical;
                }
                return true;
            }
        }

        internal bool IsDiscretionaryAclPresent
        {
            get
            {
                return ((this._rawSd.ControlFlags & System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent) != System.Security.AccessControl.ControlFlags.None);
            }
        }

        public bool IsDS
        {
            get
            {
                return this._isDS;
            }
        }

        public bool IsSystemAclCanonical
        {
            get
            {
                if (this.SystemAcl != null)
                {
                    return this.SystemAcl.IsCanonical;
                }
                return true;
            }
        }

        internal bool IsSystemAclPresent
        {
            get
            {
                return ((this._rawSd.ControlFlags & System.Security.AccessControl.ControlFlags.SystemAclPresent) != System.Security.AccessControl.ControlFlags.None);
            }
        }

        public override SecurityIdentifier Owner
        {
            get
            {
                return this._rawSd.Owner;
            }
            set
            {
                this._rawSd.Owner = value;
            }
        }

        public System.Security.AccessControl.SystemAcl SystemAcl
        {
            get
            {
                return this._sacl;
            }
            set
            {
                if (value != null)
                {
                    if (value.IsContainer != this.IsContainer)
                    {
                        throw new ArgumentException(Environment.GetResourceString(this.IsContainer ? "AccessControl_MustSpecifyContainerAcl" : "AccessControl_MustSpecifyLeafObjectAcl"), "value");
                    }
                    if (value.IsDS != this.IsDS)
                    {
                        throw new ArgumentException(Environment.GetResourceString(this.IsDS ? "AccessControl_MustSpecifyDirectoryObjectAcl" : "AccessControl_MustSpecifyNonDirectoryObjectAcl"), "value");
                    }
                }
                this._sacl = value;
                if (this._sacl != null)
                {
                    this._rawSd.SystemAcl = this._sacl.RawAcl;
                    this.AddControlFlags(System.Security.AccessControl.ControlFlags.SystemAclPresent);
                }
                else
                {
                    this._rawSd.SystemAcl = null;
                    this.RemoveControlFlags(System.Security.AccessControl.ControlFlags.SystemAclPresent);
                }
            }
        }
    }
}

