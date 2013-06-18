namespace System.DirectoryServices.Protocols
{
    using System;

    public class PageResultResponseControl : DirectoryControl
    {
        private int count;
        private byte[] pageCookie;

        internal PageResultResponseControl(int count, byte[] cookie, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.319", controlValue, criticality, true)
        {
            this.count = count;
            this.pageCookie = cookie;
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
        }

        public int TotalCount
        {
            get
            {
                return this.count;
            }
        }
    }
}

