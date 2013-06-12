namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class SHA256Managed : SHA256
    {
        private byte[] _buffer;
        private long _count;
        private static readonly uint[] _K = new uint[] { 
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5, 0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174, 
            0xe49b69c1, 0xefbe4786, 0xfc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da, 0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x6ca6351, 0x14292967, 
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85, 0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070, 
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3, 0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
         };
        private uint[] _stateSHA256;
        private uint[] _W;

        public SHA256Managed()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            this._stateSHA256 = new uint[8];
            this._buffer = new byte[0x40];
            this._W = new uint[0x40];
            this.InitializeState();
        }

        private byte[] _EndHash()
        {
            byte[] block = new byte[0x20];
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
            Utils.DWORDToBigEndian(block, this._stateSHA256, 8);
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
            fixed (uint* numRef = this._stateSHA256)
            {
                fixed (byte* numRef2 = this._buffer)
                {
                    fixed (uint* numRef3 = this._W)
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

        private static uint Ch(uint x, uint y, uint z)
        {
            return ((x & y) ^ ((x ^ uint.MaxValue) & z));
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
            Array.Clear(this._W, 0, this._W.Length);
        }

        private void InitializeState()
        {
            this._count = 0L;
            this._stateSHA256[0] = 0x6a09e667;
            this._stateSHA256[1] = 0xbb67ae85;
            this._stateSHA256[2] = 0x3c6ef372;
            this._stateSHA256[3] = 0xa54ff53a;
            this._stateSHA256[4] = 0x510e527f;
            this._stateSHA256[5] = 0x9b05688c;
            this._stateSHA256[6] = 0x1f83d9ab;
            this._stateSHA256[7] = 0x5be0cd19;
        }

        private static uint Maj(uint x, uint y, uint z)
        {
            return (((x & y) ^ (x & z)) ^ (y & z));
        }

        private static uint RotateRight(uint x, int n)
        {
            return ((x >> n) | (x << (0x20 - n)));
        }

        [SecurityCritical]
        private static unsafe void SHA256Expand(uint* x)
        {
            for (int i = 0x10; i < 0x40; i++)
            {
                x[i] = ((sigma_1(x[i - 2]) + x[i - 7]) + sigma_0(x[i - 15])) + x[i - 0x10];
            }
        }

        [SecurityCritical]
        private static unsafe void SHATransform(uint* expandedBuffer, uint* state, byte* block)
        {
            uint x = state[0];
            uint y = state[1];
            uint z = state[2];
            uint num4 = state[3];
            uint num5 = state[4];
            uint num6 = state[5];
            uint num8 = state[6];
            uint num7 = state[7];
            Utils.DWORDFromBigEndian(expandedBuffer, 0x10, block);
            SHA256Expand(expandedBuffer);
            for (int i = 0; i < 0x40; i++)
            {
                uint num17 = (((num7 + Sigma_1(num5)) + Ch(num5, num6, num8)) + _K[i]) + expandedBuffer[i];
                uint num13 = num4 + num17;
                uint num9 = (num17 + Sigma_0(x)) + Maj(x, y, z);
                i++;
                num17 = (((num8 + Sigma_1(num13)) + Ch(num13, num5, num6)) + _K[i]) + expandedBuffer[i];
                uint num14 = z + num17;
                uint num10 = (num17 + Sigma_0(num9)) + Maj(num9, x, y);
                i++;
                num17 = (((num6 + Sigma_1(num14)) + Ch(num14, num13, num5)) + _K[i]) + expandedBuffer[i];
                uint num16 = y + num17;
                uint num11 = (num17 + Sigma_0(num10)) + Maj(num10, num9, x);
                i++;
                num17 = (((num5 + Sigma_1(num16)) + Ch(num16, num14, num13)) + _K[i]) + expandedBuffer[i];
                uint num15 = x + num17;
                uint num12 = (num17 + Sigma_0(num11)) + Maj(num11, num10, num9);
                i++;
                num17 = (((num13 + Sigma_1(num15)) + Ch(num15, num16, num14)) + _K[i]) + expandedBuffer[i];
                num7 = num9 + num17;
                num4 = (num17 + Sigma_0(num12)) + Maj(num12, num11, num10);
                i++;
                num17 = (((num14 + Sigma_1(num7)) + Ch(num7, num15, num16)) + _K[i]) + expandedBuffer[i];
                num8 = num10 + num17;
                z = (num17 + Sigma_0(num4)) + Maj(num4, num12, num11);
                i++;
                num17 = (((num16 + Sigma_1(num8)) + Ch(num8, num7, num15)) + _K[i]) + expandedBuffer[i];
                num6 = num11 + num17;
                y = (num17 + Sigma_0(z)) + Maj(z, num4, num12);
                i++;
                num17 = (((num15 + Sigma_1(num6)) + Ch(num6, num8, num7)) + _K[i]) + expandedBuffer[i];
                num5 = num12 + num17;
                x = (num17 + Sigma_0(y)) + Maj(y, z, num4);
            }
            state[0] += x;
            uint* numPtr1 = state + 1;
            numPtr1[0] += y;
            uint* numPtr2 = state + 2;
            numPtr2[0] += z;
            uint* numPtr3 = state + 3;
            numPtr3[0] += num4;
            uint* numPtr4 = state + 4;
            numPtr4[0] += num5;
            uint* numPtr5 = state + 5;
            numPtr5[0] += num6;
            uint* numPtr6 = state + 6;
            numPtr6[0] += num8;
            uint* numPtr7 = state + 7;
            numPtr7[0] += num7;
        }

        private static uint sigma_0(uint x)
        {
            return ((RotateRight(x, 7) ^ RotateRight(x, 0x12)) ^ (x >> 3));
        }

        private static uint Sigma_0(uint x)
        {
            return ((RotateRight(x, 2) ^ RotateRight(x, 13)) ^ RotateRight(x, 0x16));
        }

        private static uint sigma_1(uint x)
        {
            return ((RotateRight(x, 0x11) ^ RotateRight(x, 0x13)) ^ (x >> 10));
        }

        private static uint Sigma_1(uint x)
        {
            return ((RotateRight(x, 6) ^ RotateRight(x, 11)) ^ RotateRight(x, 0x19));
        }
    }
}

