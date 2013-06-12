namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class SHA384Managed : SHA384
    {
        private byte[] _buffer;
        private ulong _count;
        private static readonly ulong[] _K = new ulong[] { 
            0x428a2f98d728ae22L, 0x7137449123ef65cdL, 13096744586834688815L, 16840607885511220156L, 0x3956c25bf348b538L, 0x59f111f1b605d019L, 10538285296894168987L, 12329834152419229976L, 15566598209576043074L, 0x12835b0145706fbeL, 0x243185be4ee4b28cL, 0x550c7dc3d5ffb4e2L, 0x72be5d74f27b896fL, 9286055187155687089L, 11230858885718282805L, 13951009754708518548L, 
            16472876342353939154L, 17275323862435702243L, 0xfc19dc68b8cd5b5L, 0x240ca1cc77ac9c65L, 0x2de92c6f592b0275L, 0x4a7484aa6ea6e483L, 0x5cb0a9dcbd41fbd4L, 0x76f988da831153b5L, 10970295158949994411L, 12119686244451234320L, 12683024718118986047L, 13788192230050041572L, 14330467153632333762L, 15395433587784984357L, 0x6ca6351e003826fL, 0x142929670a0e6e70L, 
            0x27b70a8546d22ffcL, 0x2e1b21385c26c926L, 0x4d2c6dfc5ac42aedL, 0x53380d139d95b3dfL, 0x650a73548baf63deL, 0x766a0abb3c77b2a8L, 9350256976987008742L, 10552545826968843579L, 11727347734174303076L, 12113106623233404929L, 14000437183269869457L, 14369950271660146224L, 15101387698204529176L, 15463397548674623760L, 17586052441742319658L, 0x106aa07032bbd1b8L, 
            0x19a4c116b8d2d0c8L, 0x1e376c085141ab53L, 0x2748774cdf8eeb99L, 0x34b0bcb5e19b48a8L, 0x391c0cb3c5c95a63L, 0x4ed8aa4ae3418acbL, 0x5b9cca4f7763e373L, 0x682e6ff3d6b2b8a3L, 0x748f82ee5defb2fcL, 0x78a5636f43172f60L, 9568029438360202098L, 10144078919501101548L, 10430055236837252648L, 11840083180663258601L, 13761210420658862357L, 14299343276471374635L, 
            14566680578165727644L, 15097957966210449927L, 16922976911328602910L, 17689382322260857208L, 0x6f067aa72176fbaL, 0xa637dc5a2c898a6L, 0x113f9804bef90daeL, 0x1b710b35131c471bL, 0x28db77f523047d84L, 0x32caab7b40c72493L, 0x3c9ebe0a15c9bebcL, 0x431d67c49c100d4cL, 0x4cc5d4becb3e42b6L, 0x597f299cfc657e2aL, 0x5fcb6fab3ad6faecL, 0x6c44198c4a475817L
         };
        private ulong[] _stateSHA384;
        private ulong[] _W;

        public SHA384Managed()
        {
            if (CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Cryptography_NonCompliantFIPSAlgorithm"));
            }
            this._stateSHA384 = new ulong[8];
            this._buffer = new byte[0x80];
            this._W = new ulong[80];
            this.InitializeState();
        }

        [SecurityCritical]
        private byte[] _EndHash()
        {
            byte[] block = new byte[0x30];
            int num = 0x80 - ((int) (this._count & ((ulong) 0x7fL)));
            if (num <= 0x10)
            {
                num += 0x80;
            }
            byte[] partIn = new byte[num];
            partIn[0] = 0x80;
            ulong num2 = this._count * ((ulong) 8L);
            partIn[num - 8] = (byte) ((num2 >> 0x38) & ((ulong) 0xffL));
            partIn[num - 7] = (byte) ((num2 >> 0x30) & ((ulong) 0xffL));
            partIn[num - 6] = (byte) ((num2 >> 40) & ((ulong) 0xffL));
            partIn[num - 5] = (byte) ((num2 >> 0x20) & ((ulong) 0xffL));
            partIn[num - 4] = (byte) ((num2 >> 0x18) & ((ulong) 0xffL));
            partIn[num - 3] = (byte) ((num2 >> 0x10) & ((ulong) 0xffL));
            partIn[num - 2] = (byte) ((num2 >> 8) & ((ulong) 0xffL));
            partIn[num - 1] = (byte) (num2 & ((ulong) 0xffL));
            this._HashData(partIn, 0, partIn.Length);
            Utils.QuadWordToBigEndian(block, this._stateSHA384, 6);
            base.HashValue = block;
            return block;
        }

        [SecurityCritical]
        private unsafe void _HashData(byte[] partIn, int ibStart, int cbSize)
        {
            int byteCount = cbSize;
            int srcOffsetBytes = ibStart;
            int dstOffsetBytes = (int) (this._count & ((ulong) 0x7fL));
            this._count += byteCount;
            fixed (ulong* numRef = this._stateSHA384)
            {
                fixed (byte* numRef2 = this._buffer)
                {
                    fixed (ulong* numRef3 = this._W)
                    {
                        if ((dstOffsetBytes > 0) && ((dstOffsetBytes + byteCount) >= 0x80))
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, dstOffsetBytes, 0x80 - dstOffsetBytes);
                            srcOffsetBytes += 0x80 - dstOffsetBytes;
                            byteCount -= 0x80 - dstOffsetBytes;
                            SHATransform(numRef3, numRef, numRef2);
                            dstOffsetBytes = 0;
                        }
                        while (byteCount >= 0x80)
                        {
                            Buffer.InternalBlockCopy(partIn, srcOffsetBytes, this._buffer, 0, 0x80);
                            srcOffsetBytes += 0x80;
                            byteCount -= 0x80;
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

        private static ulong Ch(ulong x, ulong y, ulong z)
        {
            return ((x & y) ^ ((x ^ ulong.MaxValue) & z));
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

        public override void Initialize()
        {
            this.InitializeState();
            Array.Clear(this._buffer, 0, this._buffer.Length);
            Array.Clear(this._W, 0, this._W.Length);
        }

        private void InitializeState()
        {
            this._count = 0L;
            this._stateSHA384[0] = 14680500436340154072L;
            this._stateSHA384[1] = 0x629a292a367cd507L;
            this._stateSHA384[2] = 10473403895298186519L;
            this._stateSHA384[3] = 0x152fecd8f70e5939L;
            this._stateSHA384[4] = 0x67332667ffc00b31L;
            this._stateSHA384[5] = 10282925794625328401L;
            this._stateSHA384[6] = 15784041429090275239L;
            this._stateSHA384[7] = 0x47b5481dbefa4fa4L;
        }

        private static ulong Maj(ulong x, ulong y, ulong z)
        {
            return (((x & y) ^ (x & z)) ^ (y & z));
        }

        private static ulong RotateRight(ulong x, int n)
        {
            return ((x >> n) | (x << (0x40 - n)));
        }

        [SecurityCritical]
        private static unsafe void SHA384Expand(ulong* x)
        {
            for (int i = 0x10; i < 80; i++)
            {
                x[i] = ((sigma_1(x[i - 2]) + x[i - 7]) + sigma_0(x[i - 15])) + x[i - 0x10];
            }
        }

        [SecurityCritical]
        private static unsafe void SHATransform(ulong* expandedBuffer, ulong* state, byte* block)
        {
            ulong x = state[0];
            ulong y = state[1];
            ulong z = state[2];
            ulong num4 = state[3];
            ulong num5 = state[4];
            ulong num6 = state[5];
            ulong num7 = state[6];
            ulong num8 = state[7];
            Utils.QuadWordFromBigEndian(expandedBuffer, 0x10, block);
            SHA384Expand(expandedBuffer);
            for (int i = 0; i < 80; i++)
            {
                ulong num17 = (((num8 + Sigma_1(num5)) + Ch(num5, num6, num7)) + _K[i]) + expandedBuffer[i];
                ulong num13 = num4 + num17;
                ulong num9 = (num17 + Sigma_0(x)) + Maj(x, y, z);
                i++;
                num17 = (((num7 + Sigma_1(num13)) + Ch(num13, num5, num6)) + _K[i]) + expandedBuffer[i];
                ulong num14 = z + num17;
                ulong num10 = (num17 + Sigma_0(num9)) + Maj(num9, x, y);
                i++;
                num17 = (((num6 + Sigma_1(num14)) + Ch(num14, num13, num5)) + _K[i]) + expandedBuffer[i];
                ulong num16 = y + num17;
                ulong num11 = (num17 + Sigma_0(num10)) + Maj(num10, num9, x);
                i++;
                num17 = (((num5 + Sigma_1(num16)) + Ch(num16, num14, num13)) + _K[i]) + expandedBuffer[i];
                ulong num15 = x + num17;
                ulong num12 = (num17 + Sigma_0(num11)) + Maj(num11, num10, num9);
                i++;
                num17 = (((num13 + Sigma_1(num15)) + Ch(num15, num16, num14)) + _K[i]) + expandedBuffer[i];
                num8 = num9 + num17;
                num4 = (num17 + Sigma_0(num12)) + Maj(num12, num11, num10);
                i++;
                num17 = (((num14 + Sigma_1(num8)) + Ch(num8, num15, num16)) + _K[i]) + expandedBuffer[i];
                num7 = num10 + num17;
                z = (num17 + Sigma_0(num4)) + Maj(num4, num12, num11);
                i++;
                num17 = (((num16 + Sigma_1(num7)) + Ch(num7, num8, num15)) + _K[i]) + expandedBuffer[i];
                num6 = num11 + num17;
                y = (num17 + Sigma_0(z)) + Maj(z, num4, num12);
                i++;
                num17 = (((num15 + Sigma_1(num6)) + Ch(num6, num7, num8)) + _K[i]) + expandedBuffer[i];
                num5 = num12 + num17;
                x = (num17 + Sigma_0(y)) + Maj(y, z, num4);
            }
            state[0] += x;
            ulong* numPtr1 = state + 1;
            numPtr1[0] += y;
            ulong* numPtr2 = state + 2;
            numPtr2[0] += z;
            ulong* numPtr3 = state + 3;
            numPtr3[0] += num4;
            ulong* numPtr4 = state + 4;
            numPtr4[0] += num5;
            ulong* numPtr5 = state + 5;
            numPtr5[0] += num6;
            ulong* numPtr6 = state + 6;
            numPtr6[0] += num7;
            ulong* numPtr7 = state + 7;
            numPtr7[0] += num8;
        }

        private static ulong sigma_0(ulong x)
        {
            return ((RotateRight(x, 1) ^ RotateRight(x, 8)) ^ (x >> 7));
        }

        private static ulong Sigma_0(ulong x)
        {
            return ((RotateRight(x, 0x1c) ^ RotateRight(x, 0x22)) ^ RotateRight(x, 0x27));
        }

        private static ulong sigma_1(ulong x)
        {
            return ((RotateRight(x, 0x13) ^ RotateRight(x, 0x3d)) ^ (x >> 6));
        }

        private static ulong Sigma_1(ulong x)
        {
            return ((RotateRight(x, 14) ^ RotateRight(x, 0x12)) ^ RotateRight(x, 0x29));
        }
    }
}

