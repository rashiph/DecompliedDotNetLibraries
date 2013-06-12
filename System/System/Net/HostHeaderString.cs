namespace System.Net
{
    using System;
    using System.Text;

    internal class HostHeaderString
    {
        private byte[] m_Bytes;
        private bool m_Converted;
        private string m_String;

        internal HostHeaderString()
        {
            this.Init(null);
        }

        internal HostHeaderString(string s)
        {
            this.Init(s);
        }

        private void Convert()
        {
            if ((this.m_String != null) && !this.m_Converted)
            {
                this.m_Bytes = Encoding.Default.GetBytes(this.m_String);
                string strB = Encoding.Default.GetString(this.m_Bytes);
                if (string.Compare(this.m_String, strB, StringComparison.Ordinal) != 0)
                {
                    this.m_Bytes = Encoding.UTF8.GetBytes(this.m_String);
                }
            }
        }

        internal void Copy(byte[] destBytes, int destByteIndex)
        {
            this.Convert();
            Array.Copy(this.m_Bytes, 0, destBytes, destByteIndex, this.m_Bytes.Length);
        }

        private void Init(string s)
        {
            this.m_String = s;
            this.m_Converted = false;
            this.m_Bytes = null;
        }

        internal int ByteCount
        {
            get
            {
                this.Convert();
                return this.m_Bytes.Length;
            }
        }

        internal byte[] Bytes
        {
            get
            {
                this.Convert();
                return this.m_Bytes;
            }
        }

        internal string String
        {
            get
            {
                return this.m_String;
            }
            set
            {
                this.Init(value);
            }
        }
    }
}

