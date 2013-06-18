namespace System.DirectoryServices.Protocols
{
    using System;

    public class PageResultRequestControl : DirectoryControl
    {
        private byte[] pageCookie;
        private int size;

        public PageResultRequestControl() : base("1.2.840.113556.1.4.319", null, true, true)
        {
            this.size = 0x200;
        }

        public PageResultRequestControl(int pageSize) : this()
        {
            this.PageSize = pageSize;
        }

        public PageResultRequestControl(byte[] cookie) : this()
        {
            this.pageCookie = cookie;
        }

        public override byte[] GetValue()
        {
            object[] objArray = new object[] { this.size, this.pageCookie };
            base.directoryControlValue = BerConverter.Encode("{io}", objArray);
            return base.GetValue();
        }

        public byte[] Cookie
        {
            get
            {
                if (this.pageCookie == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.pageCookie.Length];
                for (int i = 0; i < this.pageCookie.Length; i++)
                {
                    buffer[i] = this.pageCookie[i];
                }
                return buffer;
            }
            set
            {
                this.pageCookie = value;
            }
        }

        public int PageSize
        {
            get
            {
                return this.size;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.size = value;
            }
        }
    }
}

