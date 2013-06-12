namespace System.Security.AccessControl
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Principal;

    public sealed class CompoundAce : KnownAce
    {
        private System.Security.AccessControl.CompoundAceType _compoundAceType;
        private const int AceTypeLength = 4;

        public CompoundAce(AceFlags flags, int accessMask, System.Security.AccessControl.CompoundAceType compoundAceType, SecurityIdentifier sid) : base(AceType.AccessAllowedCompound, flags, accessMask, sid)
        {
            this._compoundAceType = compoundAceType;
        }

        public override void GetBinaryForm(byte[] binaryForm, int offset)
        {
            base.MarshalHeader(binaryForm, offset);
            int index = offset + 4;
            int num2 = 0;
            binaryForm[index] = (byte) base.AccessMask;
            binaryForm[index + 1] = (byte) (base.AccessMask >> 8);
            binaryForm[index + 2] = (byte) (base.AccessMask >> 0x10);
            binaryForm[index + 3] = (byte) (base.AccessMask >> 0x18);
            num2 += 4;
            binaryForm[index + num2] = (byte) ((ushort) this.CompoundAceType);
            binaryForm[(index + num2) + 1] = (byte) (((ushort) this.CompoundAceType) >> 8);
            binaryForm[(index + num2) + 2] = 0;
            binaryForm[(index + num2) + 3] = 0;
            num2 += 4;
            base.SecurityIdentifier.GetBinaryForm(binaryForm, index + num2);
        }

        internal static bool ParseBinaryForm(byte[] binaryForm, int offset, out int accessMask, out System.Security.AccessControl.CompoundAceType compoundAceType, out SecurityIdentifier sid)
        {
            GenericAce.VerifyHeader(binaryForm, offset);
            if ((binaryForm.Length - offset) >= (12 + SecurityIdentifier.MinBinaryLength))
            {
                int index = offset + 4;
                int num2 = 0;
                accessMask = ((binaryForm[index] + (binaryForm[index + 1] << 8)) + (binaryForm[index + 2] << 0x10)) + (binaryForm[index + 3] << 0x18);
                num2 += 4;
                compoundAceType = (System.Security.AccessControl.CompoundAceType) (binaryForm[index + num2] + (binaryForm[(index + num2) + 1] << 8));
                num2 += 4;
                sid = new SecurityIdentifier(binaryForm, index + num2);
                return true;
            }
            accessMask = 0;
            compoundAceType = (System.Security.AccessControl.CompoundAceType) 0;
            sid = null;
            return false;
        }

        public override int BinaryLength
        {
            get
            {
                return (12 + base.SecurityIdentifier.BinaryLength);
            }
        }

        public System.Security.AccessControl.CompoundAceType CompoundAceType
        {
            get
            {
                return this._compoundAceType;
            }
            set
            {
                this._compoundAceType = value;
            }
        }
    }
}

