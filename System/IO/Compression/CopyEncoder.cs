namespace System.IO.Compression
{
    using System;

    internal class CopyEncoder
    {
        private const int MaxUncompressedBlockSize = 0x10000;
        private const int PaddingSize = 5;

        public void GetBlock(DeflateInput input, OutputBuffer output, bool isFinal)
        {
            int count = 0;
            if (input != null)
            {
                count = Math.Min(input.Count, (output.FreeBytes - 5) - output.BitsInBuffer);
                if (count > 0xfffb)
                {
                    count = 0xfffb;
                }
            }
            if (isFinal)
            {
                output.WriteBits(3, 1);
            }
            else
            {
                output.WriteBits(3, 0);
            }
            output.FlushBits();
            this.WriteLenNLen((ushort) count, output);
            if ((input != null) && (count > 0))
            {
                output.WriteBytes(input.Buffer, input.StartIndex, count);
                input.ConsumeBytes(count);
            }
        }

        private void WriteLenNLen(ushort len, OutputBuffer output)
        {
            output.WriteUInt16(len);
            ushort num = ~len;
            output.WriteUInt16(num);
        }
    }
}

