namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class SHA1Managed : SHA1
    {
        private byte[] _buffer;
        private long _count;
        private uint[] _expandedBuffer;
        private uint[] _stateSHA1;

        public SHA1Managed()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            this._stateSHA1 = new uint[5];
            this._buffer = new byte[0x40];
            this._expandedBuffer = new uint[80];
            this.InitializeState();
        }

        private byte[] _EndHash()
        {
            byte[] block = new byte[20];
            int num = 0x40 - ((int) (this._count & 0x3fL));
            if (num <= 8)
            {
                num += 0x40;
            }
            byte[] partIn = new byte[num];
            partIn[0] = 0x80;
            long num2 = this._count * 8L;
            partIn[num - 8] = (byte) ((num2 >> 0x38) & 0xffL);
            partIn[num - 7] = (byte) ((num2 >> 0x30) & 0xffL);
            partIn[num - 6] = (byte) ((num2 >> 40) & 0xffL);
            partIn[num - 5] = (byte) ((num2 >> 0x20) & 0xffL);
            partIn[num - 4] = (byte) ((num2 >> 0x18) & 0xffL);
            partIn[num - 3] = (byte) ((num2 >> 0x10) & 0xffL);
            partIn[num - 2] = (byte) ((num2 >> 8) & 0xffL);
            partIn[num - 1] = (byte) (num2 & 0xffL);
            this._HashData(partIn, 0, partIn.Length);
            Utils.DWORDToBigEndian(block, this._stateSHA1, 5);
            base.HashValue = block;
            return block;
        }

        [SecuritySafeCritical]
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int byteCount = cbSize;
            int srcOffsetBytes = ibStart;
            int dstOffsetBytes = (int) (this._count & 0x3fL);
            this._count += byteCount;
            fixed (uint* numRef = this._stateSHA1)
            {
                fixed (byte* numRef2 = this._buffer)
                {
                    fixed (uint* numRef3 = this._expandedBuffer)
                    {
                        if ((dstOffsetBytes > 0) && ((dstOffsetBytes + byteCount) >= 0x40))
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, dstOffsetBytes, 0x40 - dstOffsetBytes);
                            srcOffsetBytes += 0x40 - dstOffsetBytes;
                            byteCount -= 0x40 - dstOffsetBytes;
                            SHATransform(numRef3, numRef, numRef2);
                            dstOffsetBytes = 0;
                        }
                        while (byteCount >= 0x40)
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, 0, 0x40);
                            srcOffsetBytes += 0x40;
                            byteCount -= 0x40;
                            SHATransform(numRef3, numRef, numRef2);
                        }
                        if (byteCount > 0)
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, dstOffsetBytes, byteCount);
                        }
                    }
                }
            }
        }

        protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
        {
            this._HashData(rgb, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            return this._EndHash();
        }

        public override void Initialize()
        {
            this.InitializeState();
            Array.Clear(this._buffer, 0, this._buffer.Length);
            Array.Clear(this._expandedBuffer, 0, this._expandedBuffer.Length);
        }

        private void InitializeState()
        {
            this._count = 0L;
            this._stateSHA1[0] = 0x67452301;
            this._stateSHA1[1] = 0xefcdab89;
            this._stateSHA1[2] = 0x98badcfe;
            this._stateSHA1[3] = 0x10325476;
            this._stateSHA1[4] = 0xc3d2e1f0;
        }

        [SecurityCritical]
        private static unsafe void SHAExpand(uint* x)
        {
            for (int i = 0x10; i < 80; i++)
            {
                uint num2 = ((x[i - 3] ^ x[i - 8]) ^ x[i - 14]) ^ x[i - 0x10];
                x[i] = (num2 << 1) | (num2 >> 0x1f);
            }
        }

        [SecurityCritical]
        private static unsafe void SHATransform(uint* expandedBuffer, uint* state, byte* block)
        {
            uint num = state[0];
            uint num2 = state[1];
            uint num3 = state[2];
            uint num4 = state[3];
            uint num5 = state[4];
            Utils.DWORDFromBigEndian(expandedBuffer, 0x10, block);
            SHAExpand(expandedBuffer);
            int index = 0;
            while (index < 20)
            {
                num5 += ((((num << 5) | (num >> 0x1b)) + (num4 ^ (num2 & (num3 ^ num4)))) + expandedBuffer[index]) + 0x5a827999;
                num2 = (num2 << 30) | (num2 >> 2);
                num4 += ((((num5 << 5) | (num5 >> 0x1b)) + (num3 ^ (num & (num2 ^ num3)))) + expandedBuffer[index + 1]) + 0x5a827999;
                num = (num << 30) | (num >> 2);
                num3 += ((((num4 << 5) | (num4 >> 0x1b)) + (num2 ^ (num5 & (num ^ num2)))) + expandedBuffer[index + 2]) + 0x5a827999;
                num5 = (num5 << 30) | (num5 >> 2);
                num2 += ((((num3 << 5) | (num3 >> 0x1b)) + (num ^ (num4 & (num5 ^ num)))) + expandedBuffer[index + 3]) + 0x5a827999;
                num4 = (num4 << 30) | (num4 >> 2);
                num += ((((num2 << 5) | (num2 >> 0x1b)) + (num5 ^ (num3 & (num4 ^ num5)))) + expandedBuffer[index + 4]) + 0x5a827999;
                num3 = (num3 << 30) | (num3 >> 2);
                index += 5;
            }
            while (index < 40)
            {
                num5 += ((((num << 5) | (num >> 0x1b)) + ((num2 ^ num3) ^ num4)) + expandedBuffer[index]) + 0x6ed9eba1;
                num2 = (num2 << 30) | (num2 >> 2);
                num4 += ((((num5 << 5) | (num5 >> 0x1b)) + ((num ^ num2) ^ num3)) + expandedBuffer[index + 1]) + 0x6ed9eba1;
                num = (num << 30) | (num >> 2);
                num3 += ((((num4 << 5) | (num4 >> 0x1b)) + ((num5 ^ num) ^ num2)) + expandedBuffer[index + 2]) + 0x6ed9eba1;
                num5 = (num5 << 30) | (num5 >> 2);
                num2 += ((((num3 << 5) | (num3 >> 0x1b)) + ((num4 ^ num5) ^ num)) + expandedBuffer[index + 3]) + 0x6ed9eba1;
                num4 = (num4 << 30) | (num4 >> 2);
                num += ((((num2 << 5) | (num2 >> 0x1b)) + ((num3 ^ num4) ^ num5)) + expandedBuffer[index + 4]) + 0x6ed9eba1;
                num3 = (num3 << 30) | (num3 >> 2);
                index += 5;
            }
            while (index < 60)
            {
                num5 += ((((num << 5) | (num >> 0x1b)) + ((num2 & num3) | (num4 & (num2 | num3)))) + expandedBuffer[index]) + 0x8f1bbcdc;
                num2 = (num2 << 30) | (num2 >> 2);
                num4 += ((((num5 << 5) | (num5 >> 0x1b)) + ((num & num2) | (num3 & (num | num2)))) + expandedBuffer[index + 1]) + 0x8f1bbcdc;
                num = (num << 30) | (num >> 2);
                num3 += ((((num4 << 5) | (num4 >> 0x1b)) + ((num5 & num) | (num2 & (num5 | num)))) + expandedBuffer[index + 2]) + 0x8f1bbcdc;
                num5 = (num5 << 30) | (num5 >> 2);
                num2 += ((((num3 << 5) | (num3 >> 0x1b)) + ((num4 & num5) | (num & (num4 | num5)))) + expandedBuffer[index + 3]) + 0x8f1bbcdc;
                num4 = (num4 << 30) | (num4 >> 2);
                num += ((((num2 << 5) | (num2 >> 0x1b)) + ((num3 & num4) | (num5 & (num3 | num4)))) + expandedBuffer[index + 4]) + 0x8f1bbcdc;
                num3 = (num3 << 30) | (num3 >> 2);
                index += 5;
            }
            while (index < 80)
            {
                num5 += ((((num << 5) | (num >> 0x1b)) + ((num2 ^ num3) ^ num4)) + expandedBuffer[index]) + 0xca62c1d6;
                num2 = (num2 << 30) | (num2 >> 2);
                num4 += ((((num5 << 5) | (num5 >> 0x1b)) + ((num ^ num2) ^ num3)) + expandedBuffer[index + 1]) + 0xca62c1d6;
                num = (num << 30) | (num >> 2);
                num3 += ((((num4 << 5) | (num4 >> 0x1b)) + ((num5 ^ num) ^ num2)) + expandedBuffer[index + 2]) + 0xca62c1d6;
                num5 = (num5 << 30) | (num5 >> 2);
                num2 += ((((num3 << 5) | (num3 >> 0x1b)) + ((num4 ^ num5) ^ num)) + expandedBuffer[index + 3]) + 0xca62c1d6;
                num4 = (num4 << 30) | (num4 >> 2);
                num += ((((num2 << 5) | (num2 >> 0x1b)) + ((num3 ^ num4) ^ num5)) + expandedBuffer[index + 4]) + 0xca62c1d6;
                num3 = (num3 << 30) | (num3 >> 2);
                index += 5;
            }
            state[0] += num;
            uint* numPtr1 = state + 1;
            numPtr1[0] += num2;
            uint* numPtr2 = state + 2;
            numPtr2[0] += num3;
            uint* numPtr3 = state + 3;
            numPtr3[0] += num4;
            uint* numPtr4 = state + 4;
            numPtr4[0] += num5;
        }
    }
}

