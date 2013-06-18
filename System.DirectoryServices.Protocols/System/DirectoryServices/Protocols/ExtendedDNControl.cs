namespace System.DirectoryServices.Protocols
{
    using System;
    using System.ComponentModel;

    public class ExtendedDNControl : DirectoryControl
    {
        private ExtendedDNFlag format;

        public ExtendedDNControl() : base("1.2.840.113556.1.4.529", null, true, true)
        {
        }

        public ExtendedDNControl(ExtendedDNFlag flag) : this()
        {
            this.Flag = flag;
        }

        public override byte[] GetValue()
        {
            base.directoryControlValue = BerConverter.Encode("{i}", new object[] { (int) this.format });
            return base.GetValue();
        }

        public ExtendedDNFlag Flag
        {
            get
            {
                return this.format;
            }
            set
            {
                if ((value < ExtendedDNFlag.HexString) || (value > ExtendedDNFlag.StandardString))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ExtendedDNFlag));
                }
                this.format = value;
            }
        }
    }
}

