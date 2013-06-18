namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class VB6BinaryFile : VB6RandomFile
    {
        public VB6BinaryFile(string FileName, OpenAccess access, OpenShare share) : base(FileName, access, share, -1)
        {
        }

        internal override bool CanInput()
        {
            return true;
        }

        internal override bool CanWrite()
        {
            return true;
        }

        internal override void Get(ref string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            int byteCount;
            this.ValidateReadable();
            if (Value == null)
            {
                byteCount = 0;
            }
            else
            {
                byteCount = base.m_Encoding.GetByteCount(Value);
            }
            Value = this.GetFixedLengthString(RecordNumber, byteCount);
        }

        public override OpenMode GetMode()
        {
            return OpenMode.Binary;
        }

        internal override void Input(ref bool Value)
        {
            Value = BooleanType.FromString(this.InputStr());
        }

        internal override void Input(ref byte Value)
        {
            Value = ByteType.FromObject(this.InputNum(VariantType.Byte));
        }

        internal override void Input(ref char Value)
        {
            string str = this.InputStr();
            if (str.Length > 0)
            {
                Value = str[0];
            }
            else
            {
                Value = '\0';
            }
        }

        internal override void Input(ref DateTime Value)
        {
            Value = DateType.FromString(this.InputStr(), Utils.GetCultureInfo());
        }

        internal override void Input(ref decimal Value)
        {
            Value = DecimalType.FromObject(this.InputNum(VariantType.Decimal));
        }

        internal override void Input(ref double Value)
        {
            Value = DoubleType.FromObject(this.InputNum(VariantType.Double));
        }

        internal override void Input(ref short Value)
        {
            Value = ShortType.FromObject(this.InputNum(VariantType.Short));
        }

        internal override void Input(ref int Value)
        {
            Value = IntegerType.FromObject(this.InputNum(VariantType.Integer));
        }

        internal override void Input(ref long Value)
        {
            Value = LongType.FromObject(this.InputNum(VariantType.Long));
        }

        [SecurityCritical]
        internal override void Input(ref object Value)
        {
            Value = this.InputStr();
        }

        internal override void Input(ref float Value)
        {
            Value = SingleType.FromObject(this.InputNum(VariantType.Single));
        }

        internal override void Input(ref string Value)
        {
            Value = this.InputStr();
        }

        protected override string InputStr()
        {
            string str;
            if ((base.m_access != OpenAccess.ReadWrite) && (base.m_access != OpenAccess.Read))
            {
                NullReferenceException exception = new NullReferenceException();
                throw new NullReferenceException(exception.Message, new IOException(Utils.GetResourceString("FileOpenedNoRead")));
            }
            if (this.SkipWhiteSpaceEOF() == 0x22)
            {
                int num = base.m_sr.Read();
                base.m_position += 1L;
                str = this.ReadInField(1);
            }
            else
            {
                str = this.ReadInField(2);
            }
            this.SkipTrailingWhiteSpace();
            return str;
        }

        internal override long LOC()
        {
            return base.m_position;
        }

        internal override void Lock(long lStart, long lEnd)
        {
            long lRecordLen;
            if (lStart > lEnd)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            if (base.m_lRecordLen == -1)
            {
                lRecordLen = 1L;
            }
            else
            {
                lRecordLen = base.m_lRecordLen;
            }
            long position = (lStart - 1L) * lRecordLen;
            long length = ((lEnd - lStart) + 1L) * lRecordLen;
            base.m_file.Lock(position, length);
        }

        internal override void Put(string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            this.ValidateWriteable();
            this.PutString(RecordNumber, Value);
        }

        internal override long Seek()
        {
            return (base.m_position + 1L);
        }

        internal override void Seek(long BaseOnePosition)
        {
            if (BaseOnePosition <= 0L)
            {
                throw ExceptionUtils.VbMakeException(0x3f);
            }
            long num = BaseOnePosition - 1L;
            base.m_file.Position = num;
            base.m_position = num;
            if (base.m_sr != null)
            {
                base.m_sr.DiscardBufferedData();
            }
        }

        internal override void Unlock(long lStart, long lEnd)
        {
            long lRecordLen;
            if (lStart > lEnd)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            if (base.m_lRecordLen == -1)
            {
                lRecordLen = 1L;
            }
            else
            {
                lRecordLen = base.m_lRecordLen;
            }
            long position = (lStart - 1L) * lRecordLen;
            long length = ((lEnd - lStart) + 1L) * lRecordLen;
            base.m_file.Unlock(position, length);
        }
    }
}

