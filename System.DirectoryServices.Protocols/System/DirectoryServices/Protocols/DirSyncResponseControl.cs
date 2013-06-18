namespace System.DirectoryServices.Protocols
{
    using System;

    public class DirSyncResponseControl : DirectoryControl
    {
        private byte[] dirsyncCookie;
        private bool moreResult;
        private int size;

        internal DirSyncResponseControl(byte[] cookie, bool moreData, int resultSize, bool criticality, byte[] controlValue) : base("1.2.840.113556.1.4.841", controlValue, criticality, true)
        {
            this.dirsyncCookie = cookie;
            this.moreResult = moreData;
            this.size = resultSize;
        }

        public byte[] Cookie
        {
            get
            {
                if (this.dirsyncCookie == null)
                {
                    return new byte[0];
                }
                byte[] buffer = new byte[this.dirsyncCookie.Length];
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = this.dirsyncCookie[i];
                }
                return buffer;
            }
        }

        public bool MoreData
        {
            get
            {
                return this.moreResult;
            }
        }

        public int ResultSize
        {
            get
            {
                return this.size;
            }
        }
    }
}

