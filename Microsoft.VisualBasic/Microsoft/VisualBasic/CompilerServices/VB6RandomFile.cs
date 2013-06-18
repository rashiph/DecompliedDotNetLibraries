namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class VB6RandomFile : VB6File
    {
        public VB6RandomFile(string FileName, OpenAccess access, OpenShare share, int lRecordLen) : base(FileName, access, share, lRecordLen)
        {
        }

        internal override void CloseFile()
        {
            if (base.m_sw != null)
            {
                base.m_sw.Flush();
            }
            this.CloseTheFile();
        }

        internal override bool EOF()
        {
            base.m_eof = base.m_position >= base.m_file.Length;
            return base.m_eof;
        }

        internal override void Get(ref bool Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetBoolean(RecordNumber);
        }

        internal override void Get(ref byte Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetByte(RecordNumber);
        }

        internal override void Get(ref char Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetChar(RecordNumber);
        }

        internal override void Get(ref DateTime Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetDate(RecordNumber);
        }

        internal override void Get(ref decimal Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetCurrency(RecordNumber);
        }

        internal override void Get(ref double Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetDouble(RecordNumber);
        }

        internal override void Get(ref short Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetShort(RecordNumber);
        }

        internal override void Get(ref int Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetInteger(RecordNumber);
        }

        internal override void Get(ref long Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetLong(RecordNumber);
        }

        internal override void Get(ref float Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            Value = this.GetSingle(RecordNumber);
        }

        internal override void Get(ref ValueType Value, long RecordNumber = 0L)
        {
            this.ValidateReadable();
            this.GetRecord(RecordNumber, ref Value, false);
        }

        internal override void Get(ref string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            this.ValidateReadable();
            if (StringIsFixedLength)
            {
                int byteCount;
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
            else
            {
                Value = this.GetLengthPrefixedString(RecordNumber);
            }
        }

        internal override void Get(ref Array Value, long RecordNumber = 0L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            this.ValidateReadable();
            if (Value == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_ArrayNotInitialized"));
            }
            Type elementType = Value.GetType().GetElementType();
            int fixedStringLength = -1;
            int rank = Value.Rank;
            int firstBound = -1;
            int secondBound = -1;
            this.SetRecord(RecordNumber);
            if (base.m_file.Position < base.m_file.Length)
            {
                if (StringIsFixedLength && (elementType == typeof(string)))
                {
                    object obj2;
                    switch (rank)
                    {
                        case 1:
                            obj2 = Value.GetValue(0);
                            break;

                        case 2:
                            obj2 = Value.GetValue(0, 0);
                            break;

                        default:
                            throw new ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"));
                    }
                    if (obj2 == null)
                    {
                        fixedStringLength = 0;
                    }
                    else
                    {
                        fixedStringLength = ((string) obj2).Length;
                    }
                    if (fixedStringLength == 0)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidFixedLengthString"));
                    }
                }
                if (ArrayIsDynamic)
                {
                    Value = this.GetArrayDesc(elementType);
                    rank = Value.Rank;
                }
                firstBound = Value.GetUpperBound(0);
                switch (rank)
                {
                    case 1:
                        break;

                    case 2:
                        secondBound = Value.GetUpperBound(1);
                        break;

                    default:
                        throw new ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"));
                }
                if (ArrayIsDynamic)
                {
                    this.GetArrayData(Value, elementType, firstBound, secondBound, fixedStringLength);
                }
                else
                {
                    this.GetFixedArray(RecordNumber, ref Value, elementType, firstBound, secondBound, fixedStringLength);
                }
            }
        }

        public override OpenMode GetMode()
        {
            return OpenMode.Random;
        }

        internal override void GetObject(ref object Value, long RecordNumber = 0L, bool ContainedInVariant = true)
        {
            Type type = null;
            VT variant;
            this.ValidateReadable();
            this.SetRecord(RecordNumber);
            if (ContainedInVariant)
            {
                variant = (VT) base.m_br.ReadInt16();
                base.m_position += 2L;
            }
            else
            {
                type = Value.GetType();
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Object:
                        if (!type.IsValueType)
                        {
                            variant = VT.Variant;
                        }
                        else
                        {
                            variant = VT.Structure;
                        }
                        goto Label_00D7;

                    case TypeCode.Boolean:
                        variant = VT.Boolean;
                        goto Label_00D7;

                    case TypeCode.Char:
                        variant = VT.Char;
                        goto Label_00D7;

                    case TypeCode.Byte:
                        variant = VT.Byte;
                        goto Label_00D7;

                    case TypeCode.Int16:
                        variant = VT.Short;
                        goto Label_00D7;

                    case TypeCode.Int32:
                        variant = VT.Integer;
                        goto Label_00D7;

                    case TypeCode.Int64:
                        variant = VT.Long;
                        goto Label_00D7;

                    case TypeCode.Single:
                        variant = VT.Single;
                        goto Label_00D7;

                    case TypeCode.Double:
                        variant = VT.Double;
                        goto Label_00D7;

                    case TypeCode.Decimal:
                        variant = VT.Decimal;
                        goto Label_00D7;

                    case TypeCode.DateTime:
                        variant = VT.Date;
                        goto Label_00D7;

                    case TypeCode.String:
                        variant = VT.String;
                        goto Label_00D7;
                }
                variant = VT.Variant;
            }
        Label_00D7:
            if ((variant & VT.Array) != VT.Empty)
            {
                Array arr = null;
                VT vtype = variant ^ VT.Array;
                this.GetDynamicArray(ref arr, this.ComTypeFromVT(vtype), -1);
                Value = arr;
            }
            else
            {
                switch (variant)
                {
                    case VT.String:
                        Value = this.GetLengthPrefixedString(0L);
                        return;

                    case VT.Short:
                        Value = this.GetShort(0L);
                        return;

                    case VT.Integer:
                        Value = this.GetInteger(0L);
                        return;

                    case VT.Long:
                        Value = this.GetLong(0L);
                        return;

                    case VT.Byte:
                        Value = this.GetByte(0L);
                        return;

                    case VT.Date:
                        Value = this.GetDate(0L);
                        return;

                    case VT.Double:
                        Value = this.GetDouble(0L);
                        return;

                    case VT.Single:
                        Value = this.GetSingle(0L);
                        return;

                    case VT.Currency:
                        Value = this.GetCurrency(0L);
                        return;

                    case VT.Decimal:
                        Value = this.GetDecimal(0L);
                        return;

                    case VT.Boolean:
                        Value = this.GetBoolean(0L);
                        return;

                    case VT.Char:
                        Value = this.GetChar(0L);
                        return;

                    case VT.Structure:
                    {
                        ValueType o = (ValueType) Value;
                        this.GetRecord(0L, ref o, false);
                        Value = o;
                        return;
                    }
                }
                if ((variant == VT.DBNull) && ContainedInVariant)
                {
                    Value = DBNull.Value;
                }
                else
                {
                    if (variant == VT.DBNull)
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { "DBNull" })), 5);
                    }
                    if (variant == VT.Empty)
                    {
                        Value = null;
                    }
                    else
                    {
                        if (variant == VT.Currency)
                        {
                            throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { "Currency" })), 5);
                        }
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { type.FullName })), 5);
                    }
                }
            }
        }

        internal override StreamReader GetStreamReader()
        {
            return new StreamReader(base.m_file, base.m_Encoding);
        }

        internal override long LOC()
        {
            if (base.m_lRecordLen == 0)
            {
                throw ExceptionUtils.VbMakeException(0x33);
            }
            return (((base.m_position + base.m_lRecordLen) - 1L) / ((long) base.m_lRecordLen));
        }

        internal override void Lock(long lStart, long lEnd)
        {
            if (lStart > lEnd)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            long position = (lStart - 1L) * base.m_lRecordLen;
            long length = ((lEnd - lStart) + 1L) * base.m_lRecordLen;
            base.m_file.Lock(position, length);
        }

        internal override void OpenFile()
        {
            FileMode open;
            if (File.Exists(base.m_sFullPath))
            {
                open = FileMode.Open;
            }
            else if (base.m_access == OpenAccess.Read)
            {
                open = FileMode.OpenOrCreate;
            }
            else
            {
                open = FileMode.Create;
            }
            if (base.m_access == OpenAccess.Default)
            {
                base.m_access = OpenAccess.ReadWrite;
                try
                {
                    this.OpenFileHelper(open, base.m_access);
                    goto Label_0094;
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    base.m_access = OpenAccess.Write;
                    try
                    {
                        this.OpenFileHelper(open, base.m_access);
                        goto Label_0094;
                    }
                    catch (StackOverflowException exception4)
                    {
                        throw exception4;
                    }
                    catch (OutOfMemoryException exception5)
                    {
                        throw exception5;
                    }
                    catch (ThreadAbortException exception6)
                    {
                        throw exception6;
                    }
                    catch (Exception)
                    {
                        base.m_access = OpenAccess.Read;
                        this.OpenFileHelper(open, base.m_access);
                        goto Label_0094;
                    }
                }
            }
            this.OpenFileHelper(open, base.m_access);
        Label_0094:
            base.m_Encoding = Utils.GetFileIOEncoding();
            Stream file = base.m_file;
            if ((base.m_access == OpenAccess.Write) || (base.m_access == OpenAccess.ReadWrite))
            {
                base.m_sw = new StreamWriter(file, base.m_Encoding);
                base.m_sw.AutoFlush = true;
                base.m_bw = new BinaryWriter(file, base.m_Encoding);
            }
            if ((base.m_access == OpenAccess.Read) || (base.m_access == OpenAccess.ReadWrite))
            {
                base.m_br = new BinaryReader(file, base.m_Encoding);
                if (this.GetMode() == OpenMode.Binary)
                {
                    base.m_sr = new StreamReader(file, base.m_Encoding, false, 0x80);
                }
            }
        }

        private void OpenFileHelper(FileMode fm, OpenAccess fa)
        {
            try
            {
                base.m_file = new FileStream(base.m_sFullPath, fm, (FileAccess) fa, (FileShare) base.m_share);
            }
            catch (FileNotFoundException exception)
            {
                throw ExceptionUtils.VbMakeException(exception, 0x35);
            }
            catch (DirectoryNotFoundException exception2)
            {
                throw ExceptionUtils.VbMakeException(exception2, 0x4c);
            }
            catch (SecurityException exception3)
            {
                throw ExceptionUtils.VbMakeException(exception3, 0x35);
            }
            catch (IOException exception4)
            {
                throw ExceptionUtils.VbMakeException(exception4, 0x4b);
            }
            catch (UnauthorizedAccessException exception5)
            {
                throw ExceptionUtils.VbMakeException(exception5, 0x4b);
            }
            catch (ArgumentException exception6)
            {
                throw ExceptionUtils.VbMakeException(exception6, 0x4b);
            }
            catch (StackOverflowException exception7)
            {
                throw exception7;
            }
            catch (OutOfMemoryException exception8)
            {
                throw exception8;
            }
            catch (ThreadAbortException exception9)
            {
                throw exception9;
            }
            catch (Exception)
            {
                throw ExceptionUtils.VbMakeException(0x33);
            }
        }

        internal override void Put(bool Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutBoolean(RecordNumber, Value, false);
        }

        internal override void Put(byte Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutByte(RecordNumber, Value, false);
        }

        internal override void Put(char Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutChar(RecordNumber, Value, false);
        }

        internal override void Put(DateTime Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutDate(RecordNumber, Value, false);
        }

        internal override void Put(decimal Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutCurrency(RecordNumber, Value, false);
        }

        internal override void Put(double Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutDouble(RecordNumber, Value, false);
        }

        internal override void Put(short Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutShort(RecordNumber, Value, false);
        }

        internal override void Put(int Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutInteger(RecordNumber, Value, false);
        }

        internal override void Put(long Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutLong(RecordNumber, Value, false);
        }

        internal override void Put(float Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutSingle(RecordNumber, Value, false);
        }

        internal override void Put(ValueType Value, long RecordNumber = 0L)
        {
            this.ValidateWriteable();
            this.PutRecord(RecordNumber, Value);
        }

        internal override void Put(string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            this.ValidateWriteable();
            if (StringIsFixedLength)
            {
                this.PutString(RecordNumber, Value);
            }
            else
            {
                this.PutStringWithLength(RecordNumber, Value);
            }
        }

        internal override void Put(Array Value, long RecordNumber = 0L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            this.ValidateWriteable();
            if (Value == null)
            {
                this.PutEmpty(RecordNumber);
            }
            else
            {
                int upperBound = Value.GetUpperBound(0);
                int secondBound = -1;
                int fixedStringLength = -1;
                if (Value.Rank == 2)
                {
                    secondBound = Value.GetUpperBound(1);
                }
                if (StringIsFixedLength)
                {
                    fixedStringLength = 0;
                }
                Type elementType = Value.GetType().GetElementType();
                if (ArrayIsDynamic)
                {
                    this.PutDynamicArray(RecordNumber, Value, false, fixedStringLength);
                }
                else
                {
                    this.PutFixedArray(RecordNumber, Value, elementType, fixedStringLength, upperBound, secondBound);
                }
            }
        }

        internal override void PutObject(object Value, long RecordNumber = 0L, bool ContainedInVariant = true)
        {
            this.ValidateWriteable();
            if (Value == null)
            {
                this.PutEmpty(RecordNumber);
            }
            else
            {
                Type enumType = Value.GetType();
                if (enumType == null)
                {
                    throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { "Empty" })), 5);
                }
                if (enumType.IsArray)
                {
                    this.PutDynamicArray(RecordNumber, (Array) Value, true, -1);
                }
                else
                {
                    if (enumType.IsEnum)
                    {
                        enumType = Enum.GetUnderlyingType(enumType);
                    }
                    switch (Type.GetTypeCode(enumType))
                    {
                        case TypeCode.DBNull:
                            this.PutShort(RecordNumber, 1, false);
                            return;

                        case TypeCode.Boolean:
                            this.PutBoolean(RecordNumber, BooleanType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Char:
                            this.PutChar(RecordNumber, Microsoft.VisualBasic.CompilerServices.CharType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Byte:
                            this.PutByte(RecordNumber, ByteType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Int16:
                            this.PutShort(RecordNumber, ShortType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Int32:
                            this.PutInteger(RecordNumber, IntegerType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Int64:
                            this.PutLong(RecordNumber, LongType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Single:
                            this.PutSingle(RecordNumber, SingleType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Double:
                            this.PutDouble(RecordNumber, DoubleType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.Decimal:
                            this.PutDecimal(RecordNumber, DecimalType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.DateTime:
                            this.PutDate(RecordNumber, DateType.FromObject(Value), ContainedInVariant);
                            return;

                        case TypeCode.String:
                            this.PutVariantString(RecordNumber, Value.ToString());
                            return;
                    }
                    if (enumType == typeof(Missing))
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { "Missing" })), 5);
                    }
                    if (enumType.IsValueType && !ContainedInVariant)
                    {
                        this.PutRecord(RecordNumber, (ValueType) Value);
                    }
                    else
                    {
                        if (ContainedInVariant && enumType.IsValueType)
                        {
                            throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_PutObjectOfValueType1", new string[] { Utils.VBFriendlyName(enumType, Value) })), 5);
                        }
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { Utils.VBFriendlyName(enumType, Value) })), 5);
                    }
                }
            }
        }

        internal override long Seek()
        {
            return (this.LOC() + 1L);
        }

        internal override void Seek(long Position)
        {
            this.SetRecord(Position);
        }

        internal override void Unlock(long lStart, long lEnd)
        {
            if (lStart > lEnd)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Start" }));
            }
            long position = (lStart - 1L) * base.m_lRecordLen;
            long length = ((lEnd - lStart) + 1L) * base.m_lRecordLen;
            base.m_file.Unlock(position, length);
        }

        protected void ValidateReadable()
        {
            if ((base.m_access != OpenAccess.ReadWrite) && (base.m_access != OpenAccess.Read))
            {
                throw ExceptionUtils.VbMakeExceptionEx(0x4b, Utils.GetResourceString("FileOpenedNoRead"));
            }
        }

        protected void ValidateWriteable()
        {
            if ((base.m_access != OpenAccess.ReadWrite) && (base.m_access != OpenAccess.Write))
            {
                throw ExceptionUtils.VbMakeExceptionEx(0x4b, Utils.GetResourceString("FileOpenedNoWrite"));
            }
        }
    }
}

