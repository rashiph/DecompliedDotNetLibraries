namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Text;

    public class VerifyNameControl : DirectoryControl
    {
        private int flag;
        private string name;

        public VerifyNameControl() : base("1.2.840.113556.1.4.1338", null, true, true)
        {
        }

        public VerifyNameControl(string serverName) : this()
        {
            if (serverName == null)
            {
                throw new ArgumentNullException("serverName");
            }
            this.name = serverName;
        }

        public VerifyNameControl(string serverName, int flag) : this(serverName)
        {
            this.flag = flag;
        }

        public override byte[] GetValue()
        {
            byte[] bytes = null;
            if (this.ServerName != null)
            {
                bytes = new UnicodeEncoding().GetBytes(this.ServerName);
            }
            base.directoryControlValue = BerConverter.Encode("{io}", new object[] { this.flag, bytes });
            return base.GetValue();
        }

        public int Flag
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

        public string ServerName
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.name = value;
            }
        }
    }
}

