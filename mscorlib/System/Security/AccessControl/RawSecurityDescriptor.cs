namespace System.Security.AccessControl
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;

    public sealed class RawSecurityDescriptor : GenericSecurityDescriptor
    {
        private RawAcl _dacl;
        private System.Security.AccessControl.ControlFlags _flags;
        private SecurityIdentifier _group;
        private SecurityIdentifier _owner;
        private byte _rmControl;
        private RawAcl _sacl;

        [SecuritySafeCritical]
        public RawSecurityDescriptor(string sddlForm) : this(BinaryFormFromSddlForm(sddlForm), 0)
        {
        }

        public RawSecurityDescriptor(byte[] binaryForm, int offset)
        {
            SecurityIdentifier identifier;
            SecurityIdentifier identifier2;
            RawAcl acl;
            RawAcl acl2;
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) < 20)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            if (binaryForm[offset] != GenericSecurityDescriptor.Revision)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("AccessControl_InvalidSecurityDescriptorRevision"));
            }
            byte num = binaryForm[offset + 1];
            System.Security.AccessControl.ControlFlags flags = (System.Security.AccessControl.ControlFlags) (binaryForm[offset + 2] + (binaryForm[offset + 3] << 8));
            if ((flags & System.Security.AccessControl.ControlFlags.SelfRelative) == System.Security.AccessControl.ControlFlags.None)
            {
                throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidSecurityDescriptorSelfRelativeForm"), "binaryForm");
            }
            int num2 = GenericSecurityDescriptor.UnmarshalInt(binaryForm, offset + 4);
            if (num2 != 0)
            {
                identifier = new SecurityIdentifier(binaryForm, offset + num2);
            }
            else
            {
                identifier = null;
            }
            int num3 = GenericSecurityDescriptor.UnmarshalInt(binaryForm, offset + 8);
            if (num3 != 0)
            {
                identifier2 = new SecurityIdentifier(binaryForm, offset + num3);
            }
            else
            {
                identifier2 = null;
            }
            int num4 = GenericSecurityDescriptor.UnmarshalInt(binaryForm, offset + 12);
            if (((flags & System.Security.AccessControl.ControlFlags.SystemAclPresent) != System.Security.AccessControl.ControlFlags.None) && (num4 != 0))
            {
                acl = new RawAcl(binaryForm, offset + num4);
            }
            else
            {
                acl = null;
            }
            int num5 = GenericSecurityDescriptor.UnmarshalInt(binaryForm, offset + 0x10);
            if (((flags & System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent) != System.Security.AccessControl.ControlFlags.None) && (num5 != 0))
            {
                acl2 = new RawAcl(binaryForm, offset + num5);
            }
            else
            {
                acl2 = null;
            }
            this.CreateFromParts(flags, identifier, identifier2, acl, acl2);
            if ((flags & System.Security.AccessControl.ControlFlags.RMControlValid) != System.Security.AccessControl.ControlFlags.None)
            {
                this.ResourceManagerControl = num;
            }
        }

        public RawSecurityDescriptor(System.Security.AccessControl.ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, RawAcl systemAcl, RawAcl discretionaryAcl)
        {
            this.CreateFromParts(flags, owner, group, systemAcl, discretionaryAcl);
        }

        [SecurityCritical]
        private static byte[] BinaryFormFromSddlForm(string sddlForm)
        {
            if (sddlForm == null)
            {
                throw new ArgumentNullException("sddlForm");
            }
            IntPtr zero = IntPtr.Zero;
            uint resultSdLength = 0;
            byte[] destination = null;
            try
            {
                if (1 != Win32Native.ConvertStringSdToSd(sddlForm, GenericSecurityDescriptor.Revision, out zero, ref resultSdLength))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 0x57:
                        case 0x538:
                        case 0x53a:
                        case 0x519:
                            throw new ArgumentException(Environment.GetResourceString("ArgumentException_InvalidSDSddlForm"), "sddlForm");

                        case 8:
                            throw new OutOfMemoryException();

                        case 0x539:
                            throw new ArgumentException(Environment.GetResourceString("AccessControl_InvalidSidInSDDLString"), "sddlForm");

                        case 0:
                            goto Label_0092;
                    }
                    throw new SystemException();
                }
            Label_0092:
                destination = new byte[resultSdLength];
                Marshal.Copy(zero, destination, 0, (int) resultSdLength);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Win32Native.LocalFree(zero);
                }
            }
            return destination;
        }

        private void CreateFromParts(System.Security.AccessControl.ControlFlags flags, SecurityIdentifier owner, SecurityIdentifier group, RawAcl systemAcl, RawAcl discretionaryAcl)
        {
            this.SetFlags(flags);
            this.Owner = owner;
            this.Group = group;
            this.SystemAcl = systemAcl;
            this.DiscretionaryAcl = discretionaryAcl;
            this.ResourceManagerControl = 0;
        }

        public void SetFlags(System.Security.AccessControl.ControlFlags flags)
        {
            this._flags = flags | System.Security.AccessControl.ControlFlags.SelfRelative;
        }

        public override System.Security.AccessControl.ControlFlags ControlFlags
        {
            get
            {
                return this._flags;
            }
        }

        public RawAcl DiscretionaryAcl
        {
            get
            {
                return this._dacl;
            }
            set
            {
                this._dacl = value;
            }
        }

        internal override GenericAcl GenericDacl
        {
            get
            {
                return this._dacl;
            }
        }

        internal override GenericAcl GenericSacl
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
                return this._group;
            }
            set
            {
                this._group = value;
            }
        }

        public override SecurityIdentifier Owner
        {
            get
            {
                return this._owner;
            }
            set
            {
                this._owner = value;
            }
        }

        public byte ResourceManagerControl
        {
            get
            {
                return this._rmControl;
            }
            set
            {
                this._rmControl = value;
            }
        }

        public RawAcl SystemAcl
        {
            get
            {
                return this._sacl;
            }
            set
            {
                this._sacl = value;
            }
        }
    }
}

