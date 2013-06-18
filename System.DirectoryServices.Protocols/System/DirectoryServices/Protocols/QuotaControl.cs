namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Security.Principal;

    public class QuotaControl : DirectoryControl
    {
        private byte[] sid;

        public QuotaControl() : base("1.2.840.113556.1.4.1852", null, true, true)
        {
        }

        public QuotaControl(SecurityIdentifier querySid) : this()
        {
            this.QuerySid = querySid;
        }

        public override byte[] GetValue()
        {
            base.directoryControlValue = BerConverter.Encode("{o}", new object[] { this.sid });
            return base.GetValue();
        }

        public SecurityIdentifier QuerySid
        {
            get
            {
                if (this.sid == null)
                {
                    return null;
                }
                return new SecurityIdentifier(this.sid, 0);
            }
            set
            {
                if (value == null)
                {
                    this.sid = null;
                }
                else
                {
                    this.sid = new byte[value.BinaryLength];
                    value.GetBinaryForm(this.sid, 0);
                }
            }
        }
    }
}

