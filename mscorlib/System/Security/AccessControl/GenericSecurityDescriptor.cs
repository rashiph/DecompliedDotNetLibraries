namespace System.Security.AccessControl
{
    using System;
    using System.Security;
    using System.Security.Principal;

    public abstract class GenericSecurityDescriptor
    {
        internal const int DaclFoundAt = 0x10;
        internal const int GroupFoundAt = 8;
        internal const int HeaderLength = 20;
        internal const int OwnerFoundAt = 4;
        internal const int SaclFoundAt = 12;

        protected GenericSecurityDescriptor()
        {
        }

        public void GetBinaryForm(byte[] binaryForm, int offset)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) < this.BinaryLength)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            int num = offset;
            int binaryLength = this.BinaryLength;
            byte num2 = ((this is RawSecurityDescriptor) && ((this.ControlFlags & System.Security.AccessControl.ControlFlags.RMControlValid) != System.Security.AccessControl.ControlFlags.None)) ? (this as RawSecurityDescriptor).ResourceManagerControl : ((byte) 0);
            int controlFlags = (int) this.ControlFlags;
            if (this.IsCraftedAefaDacl)
            {
                controlFlags &= -5;
            }
            binaryForm[offset] = Revision;
            binaryForm[offset + 1] = num2;
            binaryForm[offset + 2] = (byte) controlFlags;
            binaryForm[offset + 3] = (byte) (controlFlags >> 8);
            int num4 = offset + 4;
            int num5 = offset + 8;
            int num6 = offset + 12;
            int num7 = offset + 0x10;
            offset += 20;
            if (this.Owner != null)
            {
                MarshalInt(binaryForm, num4, offset - num);
                this.Owner.GetBinaryForm(binaryForm, offset);
                offset += this.Owner.BinaryLength;
            }
            else
            {
                MarshalInt(binaryForm, num4, 0);
            }
            if (this.Group != null)
            {
                MarshalInt(binaryForm, num5, offset - num);
                this.Group.GetBinaryForm(binaryForm, offset);
                offset += this.Group.BinaryLength;
            }
            else
            {
                MarshalInt(binaryForm, num5, 0);
            }
            if (((this.ControlFlags & System.Security.AccessControl.ControlFlags.SystemAclPresent) != System.Security.AccessControl.ControlFlags.None) && (this.GenericSacl != null))
            {
                MarshalInt(binaryForm, num6, offset - num);
                this.GenericSacl.GetBinaryForm(binaryForm, offset);
                offset += this.GenericSacl.BinaryLength;
            }
            else
            {
                MarshalInt(binaryForm, num6, 0);
            }
            if ((((this.ControlFlags & System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent) != System.Security.AccessControl.ControlFlags.None) && (this.GenericDacl != null)) && !this.IsCraftedAefaDacl)
            {
                MarshalInt(binaryForm, num7, offset - num);
                this.GenericDacl.GetBinaryForm(binaryForm, offset);
                offset += this.GenericDacl.BinaryLength;
            }
            else
            {
                MarshalInt(binaryForm, num7, 0);
            }
        }

        [SecuritySafeCritical]
        public string GetSddlForm(AccessControlSections includeSections)
        {
            string str;
            byte[] binaryForm = new byte[this.BinaryLength];
            this.GetBinaryForm(binaryForm, 0);
            SecurityInfos si = 0;
            if ((includeSections & AccessControlSections.Owner) != AccessControlSections.None)
            {
                si |= SecurityInfos.Owner;
            }
            if ((includeSections & AccessControlSections.Group) != AccessControlSections.None)
            {
                si |= SecurityInfos.Group;
            }
            if ((includeSections & AccessControlSections.Audit) != AccessControlSections.None)
            {
                si |= SecurityInfos.SystemAcl;
            }
            if ((includeSections & AccessControlSections.Access) != AccessControlSections.None)
            {
                si |= SecurityInfos.DiscretionaryAcl;
            }
            int num = System.Security.AccessControl.Win32.ConvertSdToSddl(binaryForm, 1, si, out str);
            switch (num)
            {
                case 0x57:
                case 0x519:
                    throw new InvalidOperationException();
            }
            if (num != 0)
            {
                throw new InvalidOperationException();
            }
            return str;
        }

        public static bool IsSddlConversionSupported()
        {
            return true;
        }

        private static void MarshalInt(byte[] binaryForm, int offset, int number)
        {
            binaryForm[offset] = (byte) number;
            binaryForm[offset + 1] = (byte) (number >> 8);
            binaryForm[offset + 2] = (byte) (number >> 0x10);
            binaryForm[offset + 3] = (byte) (number >> 0x18);
        }

        internal static int UnmarshalInt(byte[] binaryForm, int offset)
        {
            return (((binaryForm[offset] + (binaryForm[offset + 1] << 8)) + (binaryForm[offset + 2] << 0x10)) + (binaryForm[offset + 3] << 0x18));
        }

        public int BinaryLength
        {
            get
            {
                int num = 20;
                if (this.Owner != null)
                {
                    num += this.Owner.BinaryLength;
                }
                if (this.Group != null)
                {
                    num += this.Group.BinaryLength;
                }
                if (((this.ControlFlags & System.Security.AccessControl.ControlFlags.SystemAclPresent) != System.Security.AccessControl.ControlFlags.None) && (this.GenericSacl != null))
                {
                    num += this.GenericSacl.BinaryLength;
                }
                if ((((this.ControlFlags & System.Security.AccessControl.ControlFlags.DiscretionaryAclPresent) != System.Security.AccessControl.ControlFlags.None) && (this.GenericDacl != null)) && !this.IsCraftedAefaDacl)
                {
                    num += this.GenericDacl.BinaryLength;
                }
                return num;
            }
        }

        public abstract System.Security.AccessControl.ControlFlags ControlFlags { get; }

        internal abstract GenericAcl GenericDacl { get; }

        internal abstract GenericAcl GenericSacl { get; }

        public abstract SecurityIdentifier Group { get; set; }

        private bool IsCraftedAefaDacl
        {
            get
            {
                return ((this.GenericDacl is DiscretionaryAcl) && (this.GenericDacl as DiscretionaryAcl).EveryOneFullAccessForNullDacl);
            }
        }

        public abstract SecurityIdentifier Owner { get; set; }

        public static byte Revision
        {
            get
            {
                return 1;
            }
        }
    }
}

