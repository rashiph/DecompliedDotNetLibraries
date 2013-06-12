namespace System.Security.AccessControl
{
    using System;
    using System.Globalization;

    public sealed class CustomAce : GenericAce
    {
        private byte[] _opaque;
        public static readonly int MaxOpaqueLength = 0xfffb;

        public CustomAce(AceType type, AceFlags flags, byte[] opaque) : base(type, flags)
        {
            if (type <= AceType.SystemAlarmCallbackObject)
            {
                throw new ArgumentOutOfRangeException("type", Environment.GetResourceString("ArgumentOutOfRange_InvalidUserDefinedAceType"));
            }
            this.SetOpaque(opaque);
        }

        public override void GetBinaryForm(byte[] binaryForm, int offset)
        {
            base.MarshalHeader(binaryForm, offset);
            offset += 4;
            if (this.OpaqueLength != 0)
            {
                if (this.OpaqueLength > MaxOpaqueLength)
                {
                    throw new SystemException();
                }
                this.GetOpaque().CopyTo(binaryForm, offset);
            }
        }

        public byte[] GetOpaque()
        {
            return this._opaque;
        }

        public void SetOpaque(byte[] opaque)
        {
            if (opaque != null)
            {
                if (opaque.Length > MaxOpaqueLength)
                {
                    throw new ArgumentOutOfRangeException("opaque", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_ArrayLength"), new object[] { 0, MaxOpaqueLength }));
                }
                if ((opaque.Length % 4) != 0)
                {
                    throw new ArgumentOutOfRangeException("opaque", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_ArrayLengthMultiple"), new object[] { 4 }));
                }
            }
            this._opaque = opaque;
        }

        public override int BinaryLength
        {
            get
            {
                return (4 + this.OpaqueLength);
            }
        }

        public int OpaqueLength
        {
            get
            {
                if (this._opaque == null)
                {
                    return 0;
                }
                return this._opaque.Length;
            }
        }
    }
}

