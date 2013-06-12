namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class RIPEMD160Managed : RIPEMD160
    {
        private uint[] _blockDWords;
        private byte[] _buffer;
        private long _count;
        private uint[] _stateMD160;

        public RIPEMD160Managed()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            this._stateMD160 = new uint[5];
            this._blockDWords = new uint[0x10];
            this._buffer = new byte[0x40];
            this.InitializeState();
        }

        [SecurityCritical]
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
            partIn[num - 1] = (byte) ((num2 >> 0x38) & 0xffL);
            partIn[num - 2] = (byte) ((num2 >> 0x30) & 0xffL);
            partIn[num - 3] = (byte) ((num2 >> 40) & 0xffL);
            partIn[num - 4] = (byte) ((num2 >> 0x20) & 0xffL);
            partIn[num - 5] = (byte) ((num2 >> 0x18) & 0xffL);
            partIn[num - 6] = (byte) ((num2 >> 0x10) & 0xffL);
            partIn[num - 7] = (byte) ((num2 >> 8) & 0xffL);
            partIn[num - 8] = (byte) (num2 & 0xffL);
            this._HashData(partIn, 0, partIn.Length);
            Utils.DWORDToLittleEndian(block, this._stateMD160, 5);
            base.HashValue = block;
            return block;
        }

        [SecurityCritical]
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int byteCount = cbSize;
            int srcOffsetBytes = ibStart;
            int dstOffsetBytes = (int) (this._count & 0x3fL);
            this._count += byteCount;
            fixed (uint* numRef = this._stateMD160)
            {
                fixed (byte* numRef2 = this._buffer)
                {
                    fixed (uint* numRef3 = this._blockDWords)
                    {
                        if ((dstOffsetBytes > 0) && ((dstOffsetBytes + byteCount) >= 0x40))
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, dstOffsetBytes, 0x40 - dstOffsetBytes);
                            srcOffsetBytes += 0x40 - dstOffsetBytes;
                            byteCount -= 0x40 - dstOffsetBytes;
                            MDTransform(numRef3, numRef, numRef2);
                            dstOffsetBytes = 0;
                        }
                        while (byteCount >= 0x40)
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, 0, 0x40);
                            srcOffsetBytes += 0x40;
                            byteCount -= 0x40;
                            MDTransform(numRef3, numRef, numRef2);
                        }
                        if (byteCount > 0)
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, dstOffsetBytes, byteCount);
                        }
                    }
                }
            }
        }

        private static uint F(uint x, uint y, uint z)
        {
            return ((x ^ y) ^ z);
        }

        private static uint G(uint x, uint y, uint z)
        {
            return ((x & y) | (~x & z));
        }

        private static uint H(uint x, uint y, uint z)
        {
            return ((x | ~y) ^ z);
        }

        [SecuritySafeCritical]
        protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
        {
            this._HashData(rgb, ibStart, cbSize);
        }

        [SecuritySafeCritical]
        protected override byte[] HashFinal()
        {
            return this._EndHash();
        }

        private static uint I(uint x, uint y, uint z)
        {
            return ((x & z) | (y & ~z));
        }

        public override void Initialize()
        {
            this.InitializeState();
            Array.Clear(this._blockDWords, 0, this._blockDWords.Length);
            Array.Clear(this._buffer, 0, this._buffer.Length);
        }

        private void InitializeState()
        {
            this._count = 0L;
            this._stateMD160[0] = 0x67452301;
            this._stateMD160[1] = 0xefcdab89;
            this._stateMD160[2] = 0x98badcfe;
            this._stateMD160[3] = 0x10325476;
            this._stateMD160[4] = 0xc3d2e1f0;
        }

        private static uint J(uint x, uint y, uint z)
        {
            return (x ^ (y | ~z));
        }

        [SecurityCritical]
        private static unsafe void MDTransform(uint* blockDWords, uint* state, byte* block)
        {
            uint x = state[0];
            uint num2 = state[1];
            uint y = state[2];
            uint z = state[3];
            uint num5 = state[4];
            uint num6 = x;
            uint num7 = num2;
            uint num8 = y;
            uint num9 = z;
            uint num10 = num5;
            Utils.DWORDFromLittleEndian(blockDWords, 0x10, block);
            x += blockDWords[0] + F(num2, y, z);
            x = ((x << 11) | (x >> 0x15)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += blockDWords[1] + F(x, num2, y);
            num5 = ((num5 << 14) | (num5 >> 0x12)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += blockDWords[2] + F(num5, x, num2);
            z = ((z << 15) | (z >> 0x11)) + y;
            x = (x << 10) | (x >> 0x16);
            y += blockDWords[3] + F(z, num5, x);
            y = ((y << 12) | (y >> 20)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += blockDWords[4] + F(y, z, num5);
            num2 = ((num2 << 5) | (num2 >> 0x1b)) + x;
            z = (z << 10) | (z >> 0x16);
            x += blockDWords[5] + F(num2, y, z);
            x = ((x << 8) | (x >> 0x18)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += blockDWords[6] + F(x, num2, y);
            num5 = ((num5 << 7) | (num5 >> 0x19)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += blockDWords[7] + F(num5, x, num2);
            z = ((z << 9) | (z >> 0x17)) + y;
            x = (x << 10) | (x >> 0x16);
            y += blockDWords[8] + F(z, num5, x);
            y = ((y << 11) | (y >> 0x15)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += blockDWords[9] + F(y, z, num5);
            num2 = ((num2 << 13) | (num2 >> 0x13)) + x;
            z = (z << 10) | (z >> 0x16);
            x += blockDWords[10] + F(num2, y, z);
            x = ((x << 14) | (x >> 0x12)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += blockDWords[11] + F(x, num2, y);
            num5 = ((num5 << 15) | (num5 >> 0x11)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += blockDWords[12] + F(num5, x, num2);
            z = ((z << 6) | (z >> 0x1a)) + y;
            x = (x << 10) | (x >> 0x16);
            y += blockDWords[13] + F(z, num5, x);
            y = ((y << 7) | (y >> 0x19)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += blockDWords[14] + F(y, z, num5);
            num2 = ((num2 << 9) | (num2 >> 0x17)) + x;
            z = (z << 10) | (z >> 0x16);
            x += blockDWords[15] + F(num2, y, z);
            x = ((x << 8) | (x >> 0x18)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (G(x, num2, y) + blockDWords[7]) + 0x5a827999;
            num5 = ((num5 << 7) | (num5 >> 0x19)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (G(num5, x, num2) + blockDWords[4]) + 0x5a827999;
            z = ((z << 6) | (z >> 0x1a)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (G(z, num5, x) + blockDWords[13]) + 0x5a827999;
            y = ((y << 8) | (y >> 0x18)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (G(y, z, num5) + blockDWords[1]) + 0x5a827999;
            num2 = ((num2 << 13) | (num2 >> 0x13)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (G(num2, y, z) + blockDWords[10]) + 0x5a827999;
            x = ((x << 11) | (x >> 0x15)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (G(x, num2, y) + blockDWords[6]) + 0x5a827999;
            num5 = ((num5 << 9) | (num5 >> 0x17)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (G(num5, x, num2) + blockDWords[15]) + 0x5a827999;
            z = ((z << 7) | (z >> 0x19)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (G(z, num5, x) + blockDWords[3]) + 0x5a827999;
            y = ((y << 15) | (y >> 0x11)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (G(y, z, num5) + blockDWords[12]) + 0x5a827999;
            num2 = ((num2 << 7) | (num2 >> 0x19)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (G(num2, y, z) + blockDWords[0]) + 0x5a827999;
            x = ((x << 12) | (x >> 20)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (G(x, num2, y) + blockDWords[9]) + 0x5a827999;
            num5 = ((num5 << 15) | (num5 >> 0x11)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (G(num5, x, num2) + blockDWords[5]) + 0x5a827999;
            z = ((z << 9) | (z >> 0x17)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (G(z, num5, x) + blockDWords[2]) + 0x5a827999;
            y = ((y << 11) | (y >> 0x15)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (G(y, z, num5) + blockDWords[14]) + 0x5a827999;
            num2 = ((num2 << 7) | (num2 >> 0x19)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (G(num2, y, z) + blockDWords[11]) + 0x5a827999;
            x = ((x << 13) | (x >> 0x13)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (G(x, num2, y) + blockDWords[8]) + 0x5a827999;
            num5 = ((num5 << 12) | (num5 >> 20)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (H(num5, x, num2) + blockDWords[3]) + 0x6ed9eba1;
            z = ((z << 11) | (z >> 0x15)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (H(z, num5, x) + blockDWords[10]) + 0x6ed9eba1;
            y = ((y << 13) | (y >> 0x13)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (H(y, z, num5) + blockDWords[14]) + 0x6ed9eba1;
            num2 = ((num2 << 6) | (num2 >> 0x1a)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (H(num2, y, z) + blockDWords[4]) + 0x6ed9eba1;
            x = ((x << 7) | (x >> 0x19)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (H(x, num2, y) + blockDWords[9]) + 0x6ed9eba1;
            num5 = ((num5 << 14) | (num5 >> 0x12)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (H(num5, x, num2) + blockDWords[15]) + 0x6ed9eba1;
            z = ((z << 9) | (z >> 0x17)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (H(z, num5, x) + blockDWords[8]) + 0x6ed9eba1;
            y = ((y << 13) | (y >> 0x13)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (H(y, z, num5) + blockDWords[1]) + 0x6ed9eba1;
            num2 = ((num2 << 15) | (num2 >> 0x11)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (H(num2, y, z) + blockDWords[2]) + 0x6ed9eba1;
            x = ((x << 14) | (x >> 0x12)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (H(x, num2, y) + blockDWords[7]) + 0x6ed9eba1;
            num5 = ((num5 << 8) | (num5 >> 0x18)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (H(num5, x, num2) + blockDWords[0]) + 0x6ed9eba1;
            z = ((z << 13) | (z >> 0x13)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (H(z, num5, x) + blockDWords[6]) + 0x6ed9eba1;
            y = ((y << 6) | (y >> 0x1a)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (H(y, z, num5) + blockDWords[13]) + 0x6ed9eba1;
            num2 = ((num2 << 5) | (num2 >> 0x1b)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (H(num2, y, z) + blockDWords[11]) + 0x6ed9eba1;
            x = ((x << 12) | (x >> 20)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (H(x, num2, y) + blockDWords[5]) + 0x6ed9eba1;
            num5 = ((num5 << 7) | (num5 >> 0x19)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (H(num5, x, num2) + blockDWords[12]) + 0x6ed9eba1;
            z = ((z << 5) | (z >> 0x1b)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (I(z, num5, x) + blockDWords[1]) + 0x8f1bbcdc;
            y = ((y << 11) | (y >> 0x15)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (I(y, z, num5) + blockDWords[9]) + 0x8f1bbcdc;
            num2 = ((num2 << 12) | (num2 >> 20)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (I(num2, y, z) + blockDWords[11]) + 0x8f1bbcdc;
            x = ((x << 14) | (x >> 0x12)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (I(x, num2, y) + blockDWords[10]) + 0x8f1bbcdc;
            num5 = ((num5 << 15) | (num5 >> 0x11)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (I(num5, x, num2) + blockDWords[0]) + 0x8f1bbcdc;
            z = ((z << 14) | (z >> 0x12)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (I(z, num5, x) + blockDWords[8]) + 0x8f1bbcdc;
            y = ((y << 15) | (y >> 0x11)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (I(y, z, num5) + blockDWords[12]) + 0x8f1bbcdc;
            num2 = ((num2 << 9) | (num2 >> 0x17)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (I(num2, y, z) + blockDWords[4]) + 0x8f1bbcdc;
            x = ((x << 8) | (x >> 0x18)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (I(x, num2, y) + blockDWords[13]) + 0x8f1bbcdc;
            num5 = ((num5 << 9) | (num5 >> 0x17)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (I(num5, x, num2) + blockDWords[3]) + 0x8f1bbcdc;
            z = ((z << 14) | (z >> 0x12)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (I(z, num5, x) + blockDWords[7]) + 0x8f1bbcdc;
            y = ((y << 5) | (y >> 0x1b)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (I(y, z, num5) + blockDWords[15]) + 0x8f1bbcdc;
            num2 = ((num2 << 6) | (num2 >> 0x1a)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (I(num2, y, z) + blockDWords[14]) + 0x8f1bbcdc;
            x = ((x << 8) | (x >> 0x18)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (I(x, num2, y) + blockDWords[5]) + 0x8f1bbcdc;
            num5 = ((num5 << 6) | (num5 >> 0x1a)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (I(num5, x, num2) + blockDWords[6]) + 0x8f1bbcdc;
            z = ((z << 5) | (z >> 0x1b)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (I(z, num5, x) + blockDWords[2]) + 0x8f1bbcdc;
            y = ((y << 12) | (y >> 20)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (J(y, z, num5) + blockDWords[4]) + 0xa953fd4e;
            num2 = ((num2 << 9) | (num2 >> 0x17)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (J(num2, y, z) + blockDWords[0]) + 0xa953fd4e;
            x = ((x << 15) | (x >> 0x11)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (J(x, num2, y) + blockDWords[5]) + 0xa953fd4e;
            num5 = ((num5 << 5) | (num5 >> 0x1b)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (J(num5, x, num2) + blockDWords[9]) + 0xa953fd4e;
            z = ((z << 11) | (z >> 0x15)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (J(z, num5, x) + blockDWords[7]) + 0xa953fd4e;
            y = ((y << 6) | (y >> 0x1a)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (J(y, z, num5) + blockDWords[12]) + 0xa953fd4e;
            num2 = ((num2 << 8) | (num2 >> 0x18)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (J(num2, y, z) + blockDWords[2]) + 0xa953fd4e;
            x = ((x << 13) | (x >> 0x13)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (J(x, num2, y) + blockDWords[10]) + 0xa953fd4e;
            num5 = ((num5 << 12) | (num5 >> 20)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (J(num5, x, num2) + blockDWords[14]) + 0xa953fd4e;
            z = ((z << 5) | (z >> 0x1b)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (J(z, num5, x) + blockDWords[1]) + 0xa953fd4e;
            y = ((y << 12) | (y >> 20)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (J(y, z, num5) + blockDWords[3]) + 0xa953fd4e;
            num2 = ((num2 << 13) | (num2 >> 0x13)) + x;
            z = (z << 10) | (z >> 0x16);
            x += (J(num2, y, z) + blockDWords[8]) + 0xa953fd4e;
            x = ((x << 14) | (x >> 0x12)) + num5;
            y = (y << 10) | (y >> 0x16);
            num5 += (J(x, num2, y) + blockDWords[11]) + 0xa953fd4e;
            num5 = ((num5 << 11) | (num5 >> 0x15)) + z;
            num2 = (num2 << 10) | (num2 >> 0x16);
            z += (J(num5, x, num2) + blockDWords[6]) + 0xa953fd4e;
            z = ((z << 8) | (z >> 0x18)) + y;
            x = (x << 10) | (x >> 0x16);
            y += (J(z, num5, x) + blockDWords[15]) + 0xa953fd4e;
            y = ((y << 5) | (y >> 0x1b)) + num2;
            num5 = (num5 << 10) | (num5 >> 0x16);
            num2 += (J(y, z, num5) + blockDWords[13]) + 0xa953fd4e;
            num2 = ((num2 << 6) | (num2 >> 0x1a)) + x;
            z = (z << 10) | (z >> 0x16);
            num6 += (J(num7, num8, num9) + blockDWords[5]) + 0x50a28be6;
            num6 = ((num6 << 8) | (num6 >> 0x18)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (J(num6, num7, num8) + blockDWords[14]) + 0x50a28be6;
            num10 = ((num10 << 9) | (num10 >> 0x17)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (J(num10, num6, num7) + blockDWords[7]) + 0x50a28be6;
            num9 = ((num9 << 9) | (num9 >> 0x17)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (J(num9, num10, num6) + blockDWords[0]) + 0x50a28be6;
            num8 = ((num8 << 11) | (num8 >> 0x15)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (J(num8, num9, num10) + blockDWords[9]) + 0x50a28be6;
            num7 = ((num7 << 13) | (num7 >> 0x13)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (J(num7, num8, num9) + blockDWords[2]) + 0x50a28be6;
            num6 = ((num6 << 15) | (num6 >> 0x11)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (J(num6, num7, num8) + blockDWords[11]) + 0x50a28be6;
            num10 = ((num10 << 15) | (num10 >> 0x11)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (J(num10, num6, num7) + blockDWords[4]) + 0x50a28be6;
            num9 = ((num9 << 5) | (num9 >> 0x1b)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (J(num9, num10, num6) + blockDWords[13]) + 0x50a28be6;
            num8 = ((num8 << 7) | (num8 >> 0x19)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (J(num8, num9, num10) + blockDWords[6]) + 0x50a28be6;
            num7 = ((num7 << 7) | (num7 >> 0x19)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (J(num7, num8, num9) + blockDWords[15]) + 0x50a28be6;
            num6 = ((num6 << 8) | (num6 >> 0x18)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (J(num6, num7, num8) + blockDWords[8]) + 0x50a28be6;
            num10 = ((num10 << 11) | (num10 >> 0x15)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (J(num10, num6, num7) + blockDWords[1]) + 0x50a28be6;
            num9 = ((num9 << 14) | (num9 >> 0x12)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (J(num9, num10, num6) + blockDWords[10]) + 0x50a28be6;
            num8 = ((num8 << 14) | (num8 >> 0x12)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (J(num8, num9, num10) + blockDWords[3]) + 0x50a28be6;
            num7 = ((num7 << 12) | (num7 >> 20)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (J(num7, num8, num9) + blockDWords[12]) + 0x50a28be6;
            num6 = ((num6 << 6) | (num6 >> 0x1a)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (I(num6, num7, num8) + blockDWords[6]) + 0x5c4dd124;
            num10 = ((num10 << 9) | (num10 >> 0x17)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (I(num10, num6, num7) + blockDWords[11]) + 0x5c4dd124;
            num9 = ((num9 << 13) | (num9 >> 0x13)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (I(num9, num10, num6) + blockDWords[3]) + 0x5c4dd124;
            num8 = ((num8 << 15) | (num8 >> 0x11)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (I(num8, num9, num10) + blockDWords[7]) + 0x5c4dd124;
            num7 = ((num7 << 7) | (num7 >> 0x19)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (I(num7, num8, num9) + blockDWords[0]) + 0x5c4dd124;
            num6 = ((num6 << 12) | (num6 >> 20)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (I(num6, num7, num8) + blockDWords[13]) + 0x5c4dd124;
            num10 = ((num10 << 8) | (num10 >> 0x18)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (I(num10, num6, num7) + blockDWords[5]) + 0x5c4dd124;
            num9 = ((num9 << 9) | (num9 >> 0x17)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (I(num9, num10, num6) + blockDWords[10]) + 0x5c4dd124;
            num8 = ((num8 << 11) | (num8 >> 0x15)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (I(num8, num9, num10) + blockDWords[14]) + 0x5c4dd124;
            num7 = ((num7 << 7) | (num7 >> 0x19)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (I(num7, num8, num9) + blockDWords[15]) + 0x5c4dd124;
            num6 = ((num6 << 7) | (num6 >> 0x19)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (I(num6, num7, num8) + blockDWords[8]) + 0x5c4dd124;
            num10 = ((num10 << 12) | (num10 >> 20)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (I(num10, num6, num7) + blockDWords[12]) + 0x5c4dd124;
            num9 = ((num9 << 7) | (num9 >> 0x19)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (I(num9, num10, num6) + blockDWords[4]) + 0x5c4dd124;
            num8 = ((num8 << 6) | (num8 >> 0x1a)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (I(num8, num9, num10) + blockDWords[9]) + 0x5c4dd124;
            num7 = ((num7 << 15) | (num7 >> 0x11)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (I(num7, num8, num9) + blockDWords[1]) + 0x5c4dd124;
            num6 = ((num6 << 13) | (num6 >> 0x13)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (I(num6, num7, num8) + blockDWords[2]) + 0x5c4dd124;
            num10 = ((num10 << 11) | (num10 >> 0x15)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (H(num10, num6, num7) + blockDWords[15]) + 0x6d703ef3;
            num9 = ((num9 << 9) | (num9 >> 0x17)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (H(num9, num10, num6) + blockDWords[5]) + 0x6d703ef3;
            num8 = ((num8 << 7) | (num8 >> 0x19)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (H(num8, num9, num10) + blockDWords[1]) + 0x6d703ef3;
            num7 = ((num7 << 15) | (num7 >> 0x11)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (H(num7, num8, num9) + blockDWords[3]) + 0x6d703ef3;
            num6 = ((num6 << 11) | (num6 >> 0x15)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (H(num6, num7, num8) + blockDWords[7]) + 0x6d703ef3;
            num10 = ((num10 << 8) | (num10 >> 0x18)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (H(num10, num6, num7) + blockDWords[14]) + 0x6d703ef3;
            num9 = ((num9 << 6) | (num9 >> 0x1a)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (H(num9, num10, num6) + blockDWords[6]) + 0x6d703ef3;
            num8 = ((num8 << 6) | (num8 >> 0x1a)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (H(num8, num9, num10) + blockDWords[9]) + 0x6d703ef3;
            num7 = ((num7 << 14) | (num7 >> 0x12)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (H(num7, num8, num9) + blockDWords[11]) + 0x6d703ef3;
            num6 = ((num6 << 12) | (num6 >> 20)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (H(num6, num7, num8) + blockDWords[8]) + 0x6d703ef3;
            num10 = ((num10 << 13) | (num10 >> 0x13)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (H(num10, num6, num7) + blockDWords[12]) + 0x6d703ef3;
            num9 = ((num9 << 5) | (num9 >> 0x1b)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (H(num9, num10, num6) + blockDWords[2]) + 0x6d703ef3;
            num8 = ((num8 << 14) | (num8 >> 0x12)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (H(num8, num9, num10) + blockDWords[10]) + 0x6d703ef3;
            num7 = ((num7 << 13) | (num7 >> 0x13)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (H(num7, num8, num9) + blockDWords[0]) + 0x6d703ef3;
            num6 = ((num6 << 13) | (num6 >> 0x13)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (H(num6, num7, num8) + blockDWords[4]) + 0x6d703ef3;
            num10 = ((num10 << 7) | (num10 >> 0x19)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (H(num10, num6, num7) + blockDWords[13]) + 0x6d703ef3;
            num9 = ((num9 << 5) | (num9 >> 0x1b)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (G(num9, num10, num6) + blockDWords[8]) + 0x7a6d76e9;
            num8 = ((num8 << 15) | (num8 >> 0x11)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (G(num8, num9, num10) + blockDWords[6]) + 0x7a6d76e9;
            num7 = ((num7 << 5) | (num7 >> 0x1b)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (G(num7, num8, num9) + blockDWords[4]) + 0x7a6d76e9;
            num6 = ((num6 << 8) | (num6 >> 0x18)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (G(num6, num7, num8) + blockDWords[1]) + 0x7a6d76e9;
            num10 = ((num10 << 11) | (num10 >> 0x15)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (G(num10, num6, num7) + blockDWords[3]) + 0x7a6d76e9;
            num9 = ((num9 << 14) | (num9 >> 0x12)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (G(num9, num10, num6) + blockDWords[11]) + 0x7a6d76e9;
            num8 = ((num8 << 14) | (num8 >> 0x12)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (G(num8, num9, num10) + blockDWords[15]) + 0x7a6d76e9;
            num7 = ((num7 << 6) | (num7 >> 0x1a)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (G(num7, num8, num9) + blockDWords[0]) + 0x7a6d76e9;
            num6 = ((num6 << 14) | (num6 >> 0x12)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (G(num6, num7, num8) + blockDWords[5]) + 0x7a6d76e9;
            num10 = ((num10 << 6) | (num10 >> 0x1a)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (G(num10, num6, num7) + blockDWords[12]) + 0x7a6d76e9;
            num9 = ((num9 << 9) | (num9 >> 0x17)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (G(num9, num10, num6) + blockDWords[2]) + 0x7a6d76e9;
            num8 = ((num8 << 12) | (num8 >> 20)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += (G(num8, num9, num10) + blockDWords[13]) + 0x7a6d76e9;
            num7 = ((num7 << 9) | (num7 >> 0x17)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += (G(num7, num8, num9) + blockDWords[9]) + 0x7a6d76e9;
            num6 = ((num6 << 12) | (num6 >> 20)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += (G(num6, num7, num8) + blockDWords[7]) + 0x7a6d76e9;
            num10 = ((num10 << 5) | (num10 >> 0x1b)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += (G(num10, num6, num7) + blockDWords[10]) + 0x7a6d76e9;
            num9 = ((num9 << 15) | (num9 >> 0x11)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += (G(num9, num10, num6) + blockDWords[14]) + 0x7a6d76e9;
            num8 = ((num8 << 8) | (num8 >> 0x18)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += F(num8, num9, num10) + blockDWords[12];
            num7 = ((num7 << 8) | (num7 >> 0x18)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += F(num7, num8, num9) + blockDWords[15];
            num6 = ((num6 << 5) | (num6 >> 0x1b)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += F(num6, num7, num8) + blockDWords[10];
            num10 = ((num10 << 12) | (num10 >> 20)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += F(num10, num6, num7) + blockDWords[4];
            num9 = ((num9 << 9) | (num9 >> 0x17)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += F(num9, num10, num6) + blockDWords[1];
            num8 = ((num8 << 12) | (num8 >> 20)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += F(num8, num9, num10) + blockDWords[5];
            num7 = ((num7 << 5) | (num7 >> 0x1b)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += F(num7, num8, num9) + blockDWords[8];
            num6 = ((num6 << 14) | (num6 >> 0x12)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += F(num6, num7, num8) + blockDWords[7];
            num10 = ((num10 << 6) | (num10 >> 0x1a)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += F(num10, num6, num7) + blockDWords[6];
            num9 = ((num9 << 8) | (num9 >> 0x18)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += F(num9, num10, num6) + blockDWords[2];
            num8 = ((num8 << 13) | (num8 >> 0x13)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += F(num8, num9, num10) + blockDWords[13];
            num7 = ((num7 << 6) | (num7 >> 0x1a)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num6 += F(num7, num8, num9) + blockDWords[14];
            num6 = ((num6 << 5) | (num6 >> 0x1b)) + num10;
            num8 = (num8 << 10) | (num8 >> 0x16);
            num10 += F(num6, num7, num8) + blockDWords[0];
            num10 = ((num10 << 15) | (num10 >> 0x11)) + num9;
            num7 = (num7 << 10) | (num7 >> 0x16);
            num9 += F(num10, num6, num7) + blockDWords[3];
            num9 = ((num9 << 13) | (num9 >> 0x13)) + num8;
            num6 = (num6 << 10) | (num6 >> 0x16);
            num8 += F(num9, num10, num6) + blockDWords[9];
            num8 = ((num8 << 11) | (num8 >> 0x15)) + num7;
            num10 = (num10 << 10) | (num10 >> 0x16);
            num7 += F(num8, num9, num10) + blockDWords[11];
            num7 = ((num7 << 11) | (num7 >> 0x15)) + num6;
            num9 = (num9 << 10) | (num9 >> 0x16);
            num9 += y + state[1];
            state[1] = (state[2] + z) + num10;
            state[2] = (state[3] + num5) + num6;
            state[3] = (state[4] + x) + num7;
            state[4] = (state[0] + num2) + num8;
            state[0] = num9;
        }
    }
}

