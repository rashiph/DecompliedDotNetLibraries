namespace System.Web.Hosting
{
    using System;
    using System.Text;
    using System.Web;

    internal class RecyclableByteBuffer
    {
        private byte[] _byteBuffer = ((byte[]) s_ByteBufferAllocator.GetBuffer());
        private char[] _charBuffer;
        private int _offset;
        private bool _recyclable = true;
        private const int BUFFER_SIZE = 0x1000;
        private const int MAX_FREE_BUFFERS = 0x40;
        private static UbyteBufferAllocator s_ByteBufferAllocator = new UbyteBufferAllocator(0x1000, 0x40);
        private static CharBufferAllocator s_CharBufferAllocator = new CharBufferAllocator(0x1000, 0x40);

        internal RecyclableByteBuffer()
        {
        }

        private int CalcLength()
        {
            if (this._byteBuffer != null)
            {
                int length = this._byteBuffer.Length;
                for (int i = this._offset; i < length; i++)
                {
                    if (this._byteBuffer[i] == 0)
                    {
                        return (i - this._offset);
                    }
                }
            }
            return 0;
        }

        internal void Dispose()
        {
            if (this._recyclable)
            {
                if (this._byteBuffer != null)
                {
                    s_ByteBufferAllocator.ReuseBuffer(this._byteBuffer);
                }
                if (this._charBuffer != null)
                {
                    s_CharBufferAllocator.ReuseBuffer(this._charBuffer);
                }
            }
            this._byteBuffer = null;
            this._charBuffer = null;
        }

        private char[] GetDecodedCharBuffer(Encoding encoding, ref int len)
        {
            if (this._charBuffer == null)
            {
                if (len == 0)
                {
                    this._charBuffer = new char[0];
                }
                else if (this._recyclable)
                {
                    this._charBuffer = (char[]) s_CharBufferAllocator.GetBuffer();
                    len = encoding.GetChars(this._byteBuffer, this._offset, len, this._charBuffer, 0);
                }
                else
                {
                    this._charBuffer = encoding.GetChars(this._byteBuffer, this._offset, len);
                    len = this._charBuffer.Length;
                }
            }
            return this._charBuffer;
        }

        internal string GetDecodedString(Encoding encoding, int len)
        {
            return encoding.GetString(this._byteBuffer, 0, len);
        }

        internal string[] GetDecodedTabSeparatedStrings(Encoding encoding, int numStrings, int numSkipStrings)
        {
            int num3;
            if (numSkipStrings > 0)
            {
                this.Skip(numSkipStrings);
            }
            int len = this.CalcLength();
            char[] decodedCharBuffer = this.GetDecodedCharBuffer(encoding, ref len);
            string[] strArray = new string[numStrings];
            int startIndex = 0;
            int num4 = 0;
            for (int i = 0; i < numStrings; i++)
            {
                num3 = len;
                for (int j = startIndex; j < len; j++)
                {
                    if (decodedCharBuffer[j] == '\t')
                    {
                        num3 = j;
                        break;
                    }
                }
                if (num3 > startIndex)
                {
                    strArray[i] = new string(decodedCharBuffer, startIndex, num3 - startIndex);
                }
                else
                {
                    strArray[i] = string.Empty;
                }
                num4++;
                if (num3 == len)
                {
                    break;
                }
                startIndex = num3 + 1;
            }
            if (num4 < numStrings)
            {
                len = this.CalcLength();
                startIndex = this._offset;
                for (int k = 0; k < numStrings; k++)
                {
                    num3 = len;
                    for (int m = startIndex; m < len; m++)
                    {
                        if (this._byteBuffer[m] == 9)
                        {
                            num3 = m;
                            break;
                        }
                    }
                    if (num3 > startIndex)
                    {
                        strArray[k] = encoding.GetString(this._byteBuffer, startIndex, num3 - startIndex);
                    }
                    else
                    {
                        strArray[k] = string.Empty;
                    }
                    if (num3 == len)
                    {
                        return strArray;
                    }
                    startIndex = num3 + 1;
                }
            }
            return strArray;
        }

        internal void Resize(int newSize)
        {
            this._byteBuffer = new byte[newSize];
            this._recyclable = false;
        }

        private void Skip(int count)
        {
            if (count > 0)
            {
                int length = this._byteBuffer.Length;
                int num2 = 0;
                for (int i = 0; i < length; i++)
                {
                    if ((this._byteBuffer[i] == 9) && (++num2 == count))
                    {
                        this._offset = i + 1;
                        return;
                    }
                }
            }
        }

        internal byte[] Buffer
        {
            get
            {
                return this._byteBuffer;
            }
        }
    }
}

