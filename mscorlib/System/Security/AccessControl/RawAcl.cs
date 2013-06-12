namespace System.Security.AccessControl
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public sealed class RawAcl : GenericAcl
    {
        private ArrayList _aces;
        private int _binaryLength;
        private byte _revision;

        public RawAcl(byte revision, int capacity)
        {
            this._binaryLength = 8;
            this._revision = revision;
            this._aces = new ArrayList(capacity);
        }

        public RawAcl(byte[] binaryForm, int offset)
        {
            this._binaryLength = 8;
            this.SetBinaryForm(binaryForm, offset);
        }

        public override void GetBinaryForm(byte[] binaryForm, int offset)
        {
            this.MarshalHeader(binaryForm, offset);
            offset += 8;
            for (int i = 0; i < this.Count; i++)
            {
                GenericAce ace = this._aces[i] as GenericAce;
                ace.GetBinaryForm(binaryForm, offset);
                int binaryLength = ace.BinaryLength;
                if ((binaryLength % 4) != 0)
                {
                    throw new SystemException();
                }
                offset += binaryLength;
            }
        }

        public void InsertAce(int index, GenericAce ace)
        {
            if (ace == null)
            {
                throw new ArgumentNullException("ace");
            }
            if ((this._binaryLength + ace.BinaryLength) > GenericAcl.MaxBinaryLength)
            {
                throw new OverflowException(Environment.GetResourceString("AccessControl_AclTooLong"));
            }
            this._aces.Insert(index, ace);
            this._binaryLength += ace.BinaryLength;
        }

        private void MarshalHeader(byte[] binaryForm, int offset)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this.BinaryLength > GenericAcl.MaxBinaryLength)
            {
                throw new InvalidOperationException(Environment.GetResourceString("AccessControl_AclTooLong"));
            }
            if ((binaryForm.Length - offset) < this.BinaryLength)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            binaryForm[offset] = this.Revision;
            binaryForm[offset + 1] = 0;
            binaryForm[offset + 2] = (byte) this.BinaryLength;
            binaryForm[offset + 3] = (byte) (this.BinaryLength >> 8);
            binaryForm[offset + 4] = (byte) this.Count;
            binaryForm[offset + 5] = (byte) (this.Count >> 8);
            binaryForm[offset + 6] = 0;
            binaryForm[offset + 7] = 0;
        }

        public void RemoveAce(int index)
        {
            GenericAce ace = this._aces[index] as GenericAce;
            this._aces.RemoveAt(index);
            this._binaryLength -= ace.BinaryLength;
        }

        internal void SetBinaryForm(byte[] binaryForm, int offset)
        {
            int num;
            int num2;
            VerifyHeader(binaryForm, offset, out this._revision, out num, out num2);
            num2 += offset;
            offset += 8;
            this._aces = new ArrayList(num);
            this._binaryLength = 8;
            for (int i = 0; i < num; i++)
            {
                GenericAce ace = GenericAce.CreateFromBinaryForm(binaryForm, offset);
                int binaryLength = ace.BinaryLength;
                if ((this._binaryLength + binaryLength) > GenericAcl.MaxBinaryLength)
                {
                    throw new ArgumentException(Environment.GetResourceString("ArgumentException_InvalidAclBinaryForm"), "binaryForm");
                }
                this._aces.Add(ace);
                if ((binaryLength % 4) != 0)
                {
                    throw new SystemException();
                }
                this._binaryLength += binaryLength;
                if (this._revision == GenericAcl.AclRevisionDS)
                {
                    offset += binaryForm[offset + 2] + (binaryForm[offset + 3] << 8);
                }
                else
                {
                    offset += binaryLength;
                }
                if (offset > num2)
                {
                    throw new ArgumentException(Environment.GetResourceString("ArgumentException_InvalidAclBinaryForm"), "binaryForm");
                }
            }
        }

        private static void VerifyHeader(byte[] binaryForm, int offset, out byte revision, out int count, out int length)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) >= 8)
            {
                revision = binaryForm[offset];
                length = binaryForm[offset + 2] + (binaryForm[offset + 3] << 8);
                count = binaryForm[offset + 4] + (binaryForm[offset + 5] << 8);
                if (length <= (binaryForm.Length - offset))
                {
                    return;
                }
            }
            throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
        }

        public override int BinaryLength
        {
            get
            {
                return this._binaryLength;
            }
        }

        public override int Count
        {
            get
            {
                return this._aces.Count;
            }
        }

        public override GenericAce this[int index]
        {
            get
            {
                return (this._aces[index] as GenericAce);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((value.BinaryLength % 4) != 0)
                {
                    throw new SystemException();
                }
                int num = (this.BinaryLength - ((index < this._aces.Count) ? (this._aces[index] as GenericAce).BinaryLength : 0)) + value.BinaryLength;
                if (num > GenericAcl.MaxBinaryLength)
                {
                    throw new OverflowException(Environment.GetResourceString("AccessControl_AclTooLong"));
                }
                this._aces[index] = value;
                this._binaryLength = num;
            }
        }

        public override byte Revision
        {
            get
            {
                return this._revision;
            }
        }
    }
}

