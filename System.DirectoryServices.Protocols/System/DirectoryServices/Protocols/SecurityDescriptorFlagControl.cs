namespace System.DirectoryServices.Protocols
{
    using System;

    public class SecurityDescriptorFlagControl : DirectoryControl
    {
        private System.DirectoryServices.Protocols.SecurityMasks flag;

        public SecurityDescriptorFlagControl() : base("1.2.840.113556.1.4.801", null, true, true)
        {
        }

        public SecurityDescriptorFlagControl(System.DirectoryServices.Protocols.SecurityMasks masks) : this()
        {
            this.SecurityMasks = masks;
        }

        public override byte[] GetValue()
        {
            base.directoryControlValue = BerConverter.Encode("{i}", new object[] { (int) this.flag });
            return base.GetValue();
        }

        public System.DirectoryServices.Protocols.SecurityMasks SecurityMasks
        {
            get
            {
                return this.flag;
            }
            set
            {
                this.flag = value;
            }
        }
    }
}

