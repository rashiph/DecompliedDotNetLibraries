namespace System.IO.Compression
{
    using System;
    using System.IO;

    internal class GZipDecoder : IFileFormatReader
    {
        private uint actualCrc32;
        private long actualStreamSize;
        private uint expectedCrc32;
        private uint expectedOutputStreamSize;
        private int gzip_header_flag;
        private int gzip_header_xlen;
        private GzipHeaderState gzipFooterSubstate;
        private GzipHeaderState gzipHeaderSubstate;
        private int loopCounter;

        public GZipDecoder()
        {
            this.Reset();
        }

        public bool ReadFooter(InputBuffer input)
        {
            input.SkipToByteBoundary();
            if (this.gzipFooterSubstate == GzipHeaderState.ReadingCRC)
            {
                while (this.loopCounter < 4)
                {
                    int bits = input.GetBits(8);
                    if (bits < 0)
                    {
                        return false;
                    }
                    this.expectedCrc32 |= (uint) (bits << (8 * this.loopCounter));
                    this.loopCounter++;
                }
                this.gzipFooterSubstate = GzipHeaderState.ReadingFileSize;
                this.loopCounter = 0;
            }
            if (this.gzipFooterSubstate == GzipHeaderState.ReadingFileSize)
            {
                if (this.loopCounter == 0)
                {
                    this.expectedOutputStreamSize = 0;
                }
                while (this.loopCounter < 4)
                {
                    int num2 = input.GetBits(8);
                    if (num2 < 0)
                    {
                        return false;
                    }
                    this.expectedOutputStreamSize |= (uint) (num2 << (8 * this.loopCounter));
                    this.loopCounter++;
                }
            }
            return true;
        }

        public bool ReadHeader(InputBuffer input)
        {
            int bits;
            switch (this.gzipHeaderSubstate)
            {
                case GzipHeaderState.ReadingID1:
                    bits = input.GetBits(8);
                    if (bits >= 0)
                    {
                        if (bits != 0x1f)
                        {
                            throw new InvalidDataException(SR.GetString("CorruptedGZipHeader"));
                        }
                        this.gzipHeaderSubstate = GzipHeaderState.ReadingID2;
                        break;
                    }
                    return false;

                case GzipHeaderState.ReadingID2:
                    break;

                case GzipHeaderState.ReadingCM:
                    goto Label_00A5;

                case GzipHeaderState.ReadingFLG:
                    goto Label_00CE;

                case GzipHeaderState.ReadingMMTime:
                    goto Label_00F1;

                case GzipHeaderState.ReadingXFL:
                    goto Label_0128;

                case GzipHeaderState.ReadingOS:
                    goto Label_013D;

                case GzipHeaderState.ReadingXLen1:
                    goto Label_0152;

                case GzipHeaderState.ReadingXLen2:
                    goto Label_017B;

                case GzipHeaderState.ReadingXLenData:
                    goto Label_01A8;

                case GzipHeaderState.ReadingFileName:
                    goto Label_01E5;

                case GzipHeaderState.ReadingComment:
                    goto Label_0212;

                case GzipHeaderState.ReadingCRC16Part1:
                    goto Label_0240;

                case GzipHeaderState.ReadingCRC16Part2:
                    goto Label_026A;

                case GzipHeaderState.Done:
                    goto Label_0280;

                default:
                    throw new InvalidDataException(SR.GetString("UnknownState"));
            }
            bits = input.GetBits(8);
            if (bits < 0)
            {
                return false;
            }
            if (bits != 0x8b)
            {
                throw new InvalidDataException(SR.GetString("CorruptedGZipHeader"));
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingCM;
        Label_00A5:
            bits = input.GetBits(8);
            if (bits < 0)
            {
                return false;
            }
            if (bits != 8)
            {
                throw new InvalidDataException(SR.GetString("UnknownCompressionMode"));
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingFLG;
        Label_00CE:
            bits = input.GetBits(8);
            if (bits < 0)
            {
                return false;
            }
            this.gzip_header_flag = bits;
            this.gzipHeaderSubstate = GzipHeaderState.ReadingMMTime;
            this.loopCounter = 0;
        Label_00F1:
            bits = 0;
            while (this.loopCounter < 4)
            {
                if (input.GetBits(8) < 0)
                {
                    return false;
                }
                this.loopCounter++;
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingXFL;
            this.loopCounter = 0;
        Label_0128:
            if (input.GetBits(8) < 0)
            {
                return false;
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingOS;
        Label_013D:
            if (input.GetBits(8) < 0)
            {
                return false;
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingXLen1;
        Label_0152:
            if ((this.gzip_header_flag & 4) == 0)
            {
                goto Label_01E5;
            }
            bits = input.GetBits(8);
            if (bits < 0)
            {
                return false;
            }
            this.gzip_header_xlen = bits;
            this.gzipHeaderSubstate = GzipHeaderState.ReadingXLen2;
        Label_017B:
            bits = input.GetBits(8);
            if (bits < 0)
            {
                return false;
            }
            this.gzip_header_xlen |= bits << 8;
            this.gzipHeaderSubstate = GzipHeaderState.ReadingXLenData;
            this.loopCounter = 0;
        Label_01A8:
            bits = 0;
            while (this.loopCounter < this.gzip_header_xlen)
            {
                if (input.GetBits(8) < 0)
                {
                    return false;
                }
                this.loopCounter++;
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingFileName;
            this.loopCounter = 0;
        Label_01E5:
            if ((this.gzip_header_flag & 8) == 0)
            {
                this.gzipHeaderSubstate = GzipHeaderState.ReadingComment;
            }
            else
            {
                do
                {
                    bits = input.GetBits(8);
                    if (bits < 0)
                    {
                        return false;
                    }
                }
                while (bits != 0);
                this.gzipHeaderSubstate = GzipHeaderState.ReadingComment;
            }
        Label_0212:
            if ((this.gzip_header_flag & 0x10) == 0)
            {
                this.gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part1;
            }
            else
            {
                do
                {
                    bits = input.GetBits(8);
                    if (bits < 0)
                    {
                        return false;
                    }
                }
                while (bits != 0);
                this.gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part1;
            }
        Label_0240:
            if ((this.gzip_header_flag & 2) == 0)
            {
                this.gzipHeaderSubstate = GzipHeaderState.Done;
                goto Label_0280;
            }
            if (input.GetBits(8) < 0)
            {
                return false;
            }
            this.gzipHeaderSubstate = GzipHeaderState.ReadingCRC16Part2;
        Label_026A:
            if (input.GetBits(8) < 0)
            {
                return false;
            }
            this.gzipHeaderSubstate = GzipHeaderState.Done;
        Label_0280:
            return true;
        }

        public void Reset()
        {
            this.gzipHeaderSubstate = GzipHeaderState.ReadingID1;
            this.gzipFooterSubstate = GzipHeaderState.ReadingCRC;
            this.expectedCrc32 = 0;
            this.expectedOutputStreamSize = 0;
        }

        public void UpdateWithBytesRead(byte[] buffer, int offset, int copied)
        {
            this.actualCrc32 = Crc32Helper.UpdateCrc32(this.actualCrc32, buffer, offset, copied);
            long a = this.actualStreamSize + ((long) ((ulong) copied));
            if (a > 0x100000000L)
            {
                Math.DivRem(a, 0x100000000L, out a);
            }
            this.actualStreamSize = a;
        }

        public void Validate()
        {
            if (this.expectedCrc32 != this.actualCrc32)
            {
                throw new InvalidDataException(SR.GetString("InvalidCRC"));
            }
            if (this.actualStreamSize != this.expectedOutputStreamSize)
            {
                throw new InvalidDataException(SR.GetString("InvalidStreamSize"));
            }
        }

        internal enum GzipHeaderState
        {
            ReadingID1,
            ReadingID2,
            ReadingCM,
            ReadingFLG,
            ReadingMMTime,
            ReadingXFL,
            ReadingOS,
            ReadingXLen1,
            ReadingXLen2,
            ReadingXLenData,
            ReadingFileName,
            ReadingComment,
            ReadingCRC16Part1,
            ReadingCRC16Part2,
            Done,
            ReadingCRC,
            ReadingFileSize
        }

        [Flags]
        internal enum GZipOptionalHeaderFlags
        {
            CommentFlag = 0x10,
            CRCFlag = 2,
            ExtraFieldsFlag = 4,
            FileNameFlag = 8
        }
    }
}

