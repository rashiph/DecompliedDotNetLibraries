namespace System.Data.OracleClient
{
    using System;
    using System.Text;

    internal sealed class OracleEncoding : Encoding
    {
        private OracleInternalConnection _connection;

        public OracleEncoding(OracleInternalConnection connection)
        {
            this._connection = connection;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return this.GetBytes(chars, index, count, null, 0);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            return (int) this.Handle.GetBytes(chars, charIndex, (uint) charCount, bytes, byteIndex);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetChars(bytes, index, count, null, 0);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return (int) this.Handle.GetChars(bytes, byteIndex, (uint) byteCount, chars, charIndex);
        }

        public override int GetMaxByteCount(int charCount)
        {
            return (charCount * 4);
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        internal OciHandle Handle
        {
            get
            {
                OciHandle sessionHandle = this._connection.SessionHandle;
                if ((sessionHandle != null) && !sessionHandle.IsInvalid)
                {
                    return sessionHandle;
                }
                return this._connection.EnvironmentHandle;
            }
        }
    }
}

