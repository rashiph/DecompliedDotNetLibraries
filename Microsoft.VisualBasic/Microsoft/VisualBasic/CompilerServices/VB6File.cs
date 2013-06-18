namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal abstract class VB6File
    {
        protected const int EOF_CHAR = 0x1a;
        protected const int EOF_INDICATOR = -1;
        protected const short FIN_LINEINP = 0;
        protected const short FIN_NUMBER = 3;
        protected const short FIN_NUMTERMCHAR = 6;
        protected const short FIN_QSTRING = 1;
        protected const short FIN_STRING = 2;
        protected const int lchComma = 0x2c;
        protected const int lchCR = 13;
        protected const int lchDoubleQuote = 0x22;
        protected const int lchIntlSpace = 0x3000;
        protected const int lchLF = 10;
        protected const int lchPound = 0x23;
        protected const int lchSpace = 0x20;
        protected const int lchTab = 9;
        internal OpenAccess m_access;
        internal bool m_bPrint;
        protected BinaryReader m_br;
        protected BinaryWriter m_bw;
        protected Encoding m_Encoding;
        internal bool m_eof;
        internal bool m_fAppend;
        internal FileStream m_file;
        internal int m_lCurrentColumn;
        internal int m_lRecordLen;
        internal long m_lRecordStart;
        internal int m_lWidth;
        internal long m_position;
        internal string m_sFullPath;
        internal OpenShare m_share;
        protected StreamReader m_sr;
        protected StreamWriter m_sw;

        protected VB6File()
        {
        }

        protected VB6File(string sPath, OpenAccess access, OpenShare share, int lRecordLen)
        {
            if (((access != OpenAccess.Read) && (access != OpenAccess.ReadWrite)) && (access != OpenAccess.Write))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Access" }));
            }
            this.m_access = access;
            if (((share != OpenShare.Shared) && (share != OpenShare.LockRead)) && ((share != OpenShare.LockReadWrite) && (share != OpenShare.LockWrite)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Share" }));
            }
            this.m_share = share;
            this.m_lRecordLen = lRecordLen;
            this.m_sFullPath = new FileInfo(sPath).FullName;
        }

        private string AddSpaces(string s)
        {
            string negativeSign = Thread.CurrentThread.CurrentCulture.NumberFormat.NegativeSign;
            if (negativeSign.Length == 1)
            {
                if (s[0] == negativeSign[0])
                {
                    return (s + " ");
                }
            }
            else if (Strings.Left(s, negativeSign.Length) == negativeSign)
            {
                return (s + " ");
            }
            return (" " + s + " ");
        }

        internal virtual bool CanInput()
        {
            return false;
        }

        internal virtual bool CanWrite()
        {
            return false;
        }

        protected bool CheckEOF(int lChar)
        {
            if ((lChar != -1) && (lChar != 0x1a))
            {
                return false;
            }
            return true;
        }

        internal virtual void CloseFile()
        {
            this.CloseTheFile();
        }

        protected void CloseTheFile()
        {
            if (this.m_sw != null)
            {
                this.m_sw.Close();
                this.m_sw = null;
            }
            if (this.m_sr != null)
            {
                this.m_sr.Close();
                this.m_sr = null;
            }
            if (this.m_file != null)
            {
                this.m_file.Close();
                this.m_file = null;
            }
        }

        internal Type ComTypeFromVT(VT vtype)
        {
            switch (vtype)
            {
                case VT.Empty:
                    return null;

                case VT.DBNull:
                    return typeof(DBNull);

                case VT.Short:
                    return typeof(short);

                case VT.Integer:
                    return typeof(int);

                case VT.Single:
                    return typeof(float);

                case VT.Double:
                    return typeof(double);

                case VT.Date:
                    return typeof(DateTime);

                case VT.String:
                    return typeof(string);

                case VT.Error:
                    return typeof(Exception);

                case VT.Boolean:
                    return typeof(bool);

                case VT.Variant:
                    return typeof(object);

                case VT.Decimal:
                    return typeof(decimal);

                case VT.Byte:
                    return typeof(byte);

                case VT.Char:
                    return typeof(char);

                case VT.Long:
                    return typeof(long);
            }
            throw ExceptionUtils.VbMakeException(0x1ca);
        }

        internal virtual bool EOF()
        {
            return this.m_eof;
        }

        [SecurityCritical]
        internal string FormatUniversalDate(DateTime dt)
        {
            bool flag;
            string format = "T";
            if (((dt.Year != 0) || (dt.Month != 1)) || (dt.Day != 1))
            {
                flag = true;
                format = "d";
            }
            if ((((dt.Hour + dt.Minute) + dt.Second) != 0) && flag)
            {
                format = "F";
            }
            return dt.ToString(format, FileSystem.m_WriteDateFormatInfo);
        }

        internal virtual void Get(ref bool Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref byte Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref char Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref DateTime Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref decimal Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref double Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref short Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref int Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref long Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref float Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref ValueType Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Get(ref Array Value, long RecordNumber = 0L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal string GetAbsolutePath()
        {
            return this.m_sFullPath;
        }

        internal void GetArrayData(Array arr, Type typ, int FirstBound = -1, int SecondBound = -1, int FixedStringLength = -1)
        {
            int num3;
            int num4;
            object obj2 = null;
            if (arr == null)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_ArrayNotInitialized"));
            }
            if (typ == null)
            {
                typ = arr.GetType().GetElementType();
            }
            VT vtype = VTFromComType(typ);
            if (SecondBound == -1)
            {
                num3 = 0;
                num4 = FirstBound;
            }
            else
            {
                num3 = SecondBound;
                num4 = FirstBound;
            }
            int byteLength = this.GetByteLength(vtype);
            if (((SecondBound == -1) && (byteLength > 0)) && (num4 == arr.GetUpperBound(0)))
            {
                int count = byteLength * (num4 + 1);
                if (count <= (arr.Length * byteLength))
                {
                    Buffer.BlockCopy(this.m_br.ReadBytes(count), 0, arr, 0, count);
                    this.m_position += count;
                    return;
                }
            }
            int num8 = num3;
            for (int i = 0; i <= num8; i++)
            {
                int num9 = num4;
                for (int j = 0; j <= num9; j++)
                {
                    switch (vtype)
                    {
                        case VT.Empty:
                        case VT.DBNull:
                        case VT.Error:
                            goto Label_03AA;

                        case VT.Short:
                            obj2 = this.m_br.ReadInt16();
                            this.m_position += 2L;
                            goto Label_03AA;

                        case VT.Integer:
                            obj2 = this.m_br.ReadInt32();
                            this.m_position += 4L;
                            goto Label_03AA;

                        case VT.Single:
                            obj2 = this.m_br.ReadSingle();
                            this.m_position += 4L;
                            goto Label_03AA;

                        case VT.Double:
                            obj2 = this.m_br.ReadDouble();
                            this.m_position += 8L;
                            goto Label_03AA;

                        case VT.Date:
                            obj2 = DateTime.FromOADate(this.m_br.ReadDouble());
                            this.m_position += 8L;
                            goto Label_03AA;

                        case VT.String:
                            if (FixedStringLength < 0)
                            {
                                break;
                            }
                            obj2 = this.ReadString(FixedStringLength);
                            goto Label_03AA;

                        case VT.Boolean:
                            obj2 = this.m_br.ReadInt16() > 0;
                            this.m_position += 2L;
                            goto Label_03AA;

                        case VT.Variant:
                            if (SecondBound != -1)
                            {
                                goto Label_0315;
                            }
                            obj2 = arr.GetValue(j);
                            goto Label_031F;

                        case VT.Decimal:
                        {
                            long cy = this.m_br.ReadInt64();
                            this.m_position += 8L;
                            obj2 = decimal.FromOACurrency(cy);
                            goto Label_03AA;
                        }
                        case VT.Byte:
                            obj2 = this.m_br.ReadByte();
                            this.m_position += 1L;
                            goto Label_03AA;

                        case VT.Char:
                            obj2 = this.m_br.ReadChar();
                            this.m_position += 1L;
                            goto Label_03AA;

                        case VT.Long:
                            obj2 = this.m_br.ReadInt64();
                            this.m_position += 8L;
                            goto Label_03AA;

                        case VT.Structure:
                            if (SecondBound != -1)
                            {
                                goto Label_033C;
                            }
                            obj2 = arr.GetValue(j);
                            goto Label_0346;

                        default:
                            if ((vtype & VT.Array) == VT.Empty)
                            {
                                throw ExceptionUtils.VbMakeException(0x1ca);
                            }
                            vtype ^= VT.Array;
                            if (vtype == VT.Variant)
                            {
                                throw ExceptionUtils.VbMakeException(13);
                            }
                            if ((((vtype > VT.Variant) && (vtype != VT.Byte)) && ((vtype != VT.Decimal) && (vtype != VT.Char))) && (vtype != VT.Long))
                            {
                                throw ExceptionUtils.VbMakeException(0x1ca);
                            }
                            goto Label_03AA;
                    }
                    obj2 = this.ReadString();
                    goto Label_03AA;
                Label_0315:
                    obj2 = arr.GetValue(j, i);
                Label_031F:
                    this.GetObject(ref obj2, 0L, true);
                    goto Label_03AA;
                Label_033C:
                    obj2 = arr.GetValue(j, i);
                Label_0346:
                    this.GetObject(ref obj2, 0L, false);
                Label_03AA:
                    try
                    {
                        if (SecondBound == -1)
                        {
                            arr.SetValue(obj2, j);
                        }
                        else
                        {
                            arr.SetValue(obj2, j, i);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_ArrayDimensionsDontMatch"));
                    }
                }
            }
        }

        internal Array GetArrayDesc(Type typ)
        {
            int num = this.m_br.ReadInt16();
            this.m_position += 2L;
            if (num == 0)
            {
                return Array.CreateInstance(typ, 0);
            }
            int[] lengths = new int[(num - 1) + 1];
            int[] lowerBounds = new int[(num - 1) + 1];
            int num3 = num - 1;
            for (int i = 0; i <= num3; i++)
            {
                lengths[i] = this.m_br.ReadInt32();
                lowerBounds[i] = this.m_br.ReadInt32();
                this.m_position += 8L;
            }
            return Array.CreateInstance(typ, lengths, lowerBounds);
        }

        internal bool GetBoolean(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            short num = this.m_br.ReadInt16();
            this.m_position += 2L;
            if (num == 0)
            {
                return false;
            }
            return true;
        }

        internal byte GetByte(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            byte num = this.m_br.ReadByte();
            this.m_position += 1L;
            return num;
        }

        private int GetByteLength(VT vtype)
        {
            switch (vtype)
            {
                case VT.Short:
                    return 2;

                case VT.Integer:
                    return 4;

                case VT.Single:
                    return 4;

                case VT.Double:
                    return 8;

                case VT.Byte:
                    return 1;

                case VT.Long:
                    return 8;
            }
            return -1;
        }

        internal char GetChar(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            char ch = this.m_br.ReadChar();
            this.m_position += 1L;
            return ch;
        }

        internal int GetColumn()
        {
            return this.m_lCurrentColumn;
        }

        internal decimal GetCurrency(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            long cy = this.m_br.ReadInt64();
            this.m_position += 8L;
            return decimal.FromOACurrency(cy);
        }

        internal DateTime GetDate(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            double d = this.m_br.ReadDouble();
            this.m_position += 8L;
            return DateTime.FromOADate(d);
        }

        internal decimal GetDecimal(long RecordNumber)
        {
            bool flag;
            this.SetRecord(RecordNumber);
            int num7 = this.m_br.ReadInt16();
            byte scale = this.m_br.ReadByte();
            byte num6 = this.m_br.ReadByte();
            int hi = this.m_br.ReadInt32();
            int lo = this.m_br.ReadInt32();
            int mid = this.m_br.ReadInt32();
            this.m_position += 0x10L;
            if (num6 != 0)
            {
                flag = true;
            }
            return new decimal(lo, mid, hi, flag, scale);
        }

        internal double GetDouble(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            double num = this.m_br.ReadDouble();
            this.m_position += 8L;
            return num;
        }

        internal void GetDynamicArray(ref Array arr, Type t, int FixedStringLength = -1)
        {
            int num3;
            arr = this.GetArrayDesc(t);
            int rank = arr.Rank;
            int upperBound = arr.GetUpperBound(0);
            if (rank == 1)
            {
                num3 = -1;
            }
            else
            {
                num3 = arr.GetUpperBound(1);
            }
            this.GetArrayData(arr, t, upperBound, num3, FixedStringLength);
        }

        private string GetFileInTerm(short iTermType)
        {
            switch (iTermType)
            {
                case 0:
                    return "\r";

                case 1:
                    return "\"";

                case 2:
                    return ",\r";

                case 3:
                    return " ,\t\r";

                case 6:
                    return " ,\t\r";
            }
            throw ExceptionUtils.VbMakeException(5);
        }

        internal void GetFixedArray(long RecordNumber, ref Array arr, Type FieldType, int FirstBound = -1, int SecondBound = -1, int FixedStringLength = -1)
        {
            if (SecondBound == -1)
            {
                arr = Array.CreateInstance(FieldType, (int) (FirstBound + 1));
            }
            else
            {
                arr = Array.CreateInstance(FieldType, (int) (FirstBound + 1), (int) (SecondBound + 1));
            }
            this.SetRecord(RecordNumber);
            this.GetArrayData(arr, FieldType, FirstBound, SecondBound, FixedStringLength);
        }

        internal virtual string GetFixedLengthString(long RecordNumber, int ByteLength)
        {
            this.SetRecord(RecordNumber);
            return this.ReadString(ByteLength);
        }

        internal int GetInteger(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            int num2 = this.m_br.ReadInt32();
            this.m_position += 4L;
            return num2;
        }

        internal virtual string GetLengthPrefixedString(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            if (this.EOF())
            {
                return "";
            }
            return this.ReadString();
        }

        internal long GetLong(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            long num2 = this.m_br.ReadInt64();
            this.m_position += 8L;
            return num2;
        }

        public abstract OpenMode GetMode();
        internal virtual void GetObject(ref object Value, long RecordNumber = 0L, bool ContainedInVariant = true)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal long GetPos()
        {
            return this.m_position;
        }

        protected string GetQuotedString(string Value)
        {
            return ("\"" + Value.Replace("\"", "\"\"") + "\"");
        }

        internal void GetRecord(long RecordNumber, ref ValueType o, bool ContainedInVariant = false)
        {
            if (o == null)
            {
                throw new NullReferenceException();
            }
            this.SetRecord(RecordNumber);
            GetHandler handler = new GetHandler(this);
            IRecordEnum intfRecEnum = handler;
            if (intfRecEnum == null)
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            StructUtils.EnumerateUDT(o, intfRecEnum, true);
        }

        internal short GetShort(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            short num2 = this.m_br.ReadInt16();
            this.m_position += 2L;
            return num2;
        }

        internal float GetSingle(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            float num2 = this.m_br.ReadSingle();
            this.m_position += 4L;
            return num2;
        }

        internal virtual StreamReader GetStreamReader()
        {
            return this.m_sr;
        }

        internal int GetWidth()
        {
            return this.m_lWidth;
        }

        internal virtual void Input(ref bool Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref byte Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref char Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref DateTime Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref decimal Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref double Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref short Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref int Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref long Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        [SecurityCritical]
        internal virtual void Input(ref object obj)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref float Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Input(ref string Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        protected virtual object InputNum(VariantType vt)
        {
            this.ValidateReadable();
            this.SkipWhiteSpaceEOF();
            object obj2 = this.ReadInField(3);
            this.SkipTrailingWhiteSpace();
            return obj2;
        }

        protected virtual void InputObject(ref object Value)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        protected virtual string InputStr()
        {
            string str;
            this.ValidateReadable();
            if (this.SkipWhiteSpaceEOF() == 0x22)
            {
                int num = this.m_sr.Read();
                this.m_position += 1L;
                str = this.ReadInField(1);
            }
            else
            {
                str = this.ReadInField(2);
            }
            this.SkipTrailingWhiteSpace();
            return str;
        }

        internal string InputString(int lLen)
        {
            this.ValidateReadable();
            StringBuilder builder = new StringBuilder(lLen);
            OpenMode mode = this.GetMode();
            int num3 = lLen;
            for (int i = 1; i <= num3; i++)
            {
                int num2;
                if (mode == OpenMode.Binary)
                {
                    num2 = this.m_br.Read();
                    this.m_position += 1L;
                    if (num2 == -1)
                    {
                        break;
                    }
                }
                else
                {
                    if (mode != OpenMode.Input)
                    {
                        throw ExceptionUtils.VbMakeException(0x36);
                    }
                    num2 = this.m_sr.Read();
                    this.m_position += 1L;
                    if ((num2 == -1) | (num2 == 0x1a))
                    {
                        this.m_eof = true;
                        throw ExceptionUtils.VbMakeException(0x3e);
                    }
                }
                if (num2 != 0)
                {
                    builder.Append(Strings.ChrW(num2));
                }
            }
            if (mode == OpenMode.Binary)
            {
                this.m_eof = this.m_br.PeekChar() == -1;
            }
            else
            {
                this.m_eof = this.CheckEOF(this.m_sr.Peek());
            }
            return builder.ToString();
        }

        [SecurityCritical]
        private void InternalWriteHelper(params object[] Output)
        {
            Type type2 = typeof(SpcInfo);
            Type type = type2;
            NumberFormatInfo numberFormat = Utils.GetInvariantCultureInfo().NumberFormat;
            int upperBound = Output.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                object obj2 = Output[i];
                if (obj2 == null)
                {
                    this.WriteString("#ERROR 448#");
                    continue;
                }
                if (type != type2)
                {
                    this.WriteString(",");
                }
                type = obj2.GetType();
                if (type == type2)
                {
                    SpcInfo info4 = (SpcInfo) obj2;
                    this.SPC(info4.Count);
                    continue;
                }
                if (type == typeof(TabInfo))
                {
                    TabInfo ti = (TabInfo) obj2;
                    if (ti.Column >= 0)
                    {
                        this.PrintTab(ti);
                    }
                    continue;
                }
                if (type == typeof(Missing))
                {
                    this.WriteString("#ERROR 448#");
                    continue;
                }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.DBNull:
                    {
                        this.WriteString("#NULL#");
                        continue;
                    }
                    case TypeCode.Boolean:
                    {
                        if (!BooleanType.FromObject(obj2))
                        {
                            break;
                        }
                        this.WriteString("#TRUE#");
                        continue;
                    }
                    case TypeCode.Char:
                    {
                        this.WriteString(StringType.FromChar(Microsoft.VisualBasic.CompilerServices.CharType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.Byte:
                    {
                        this.WriteString(StringType.FromByte(ByteType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.Int16:
                    {
                        this.WriteString(StringType.FromShort(ShortType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.Int32:
                    {
                        this.WriteString(StringType.FromInteger(IntegerType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.Int64:
                    {
                        this.WriteString(StringType.FromLong(LongType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.Single:
                    {
                        this.WriteString(this.IOStrFromSingle(SingleType.FromObject(obj2), numberFormat));
                        continue;
                    }
                    case TypeCode.Double:
                    {
                        this.WriteString(this.IOStrFromDouble(DoubleType.FromObject(obj2), numberFormat));
                        continue;
                    }
                    case TypeCode.Decimal:
                    {
                        this.WriteString(this.IOStrFromDecimal(DecimalType.FromObject(obj2), numberFormat));
                        continue;
                    }
                    case TypeCode.DateTime:
                    {
                        this.WriteString(this.FormatUniversalDate(DateType.FromObject(obj2)));
                        continue;
                    }
                    case TypeCode.String:
                    {
                        this.WriteString(this.GetQuotedString(obj2.ToString()));
                        continue;
                    }
                    default:
                        goto Label_024C;
                }
                this.WriteString("#FALSE#");
                continue;
            Label_024C:
                if ((obj2 is char[]) && (((Array) obj2).Rank == 1))
                {
                    this.WriteString(new string(CharArrayType.FromObject(obj2)));
                }
                else
                {
                    throw ExceptionUtils.VbMakeException(5);
                }
            }
        }

        protected bool IntlIsComma(int lch)
        {
            return (lch == 0x2c);
        }

        protected bool IntlIsDoubleQuote(int lch)
        {
            return (lch == 0x22);
        }

        protected bool IntlIsSpace(int lch)
        {
            return ((lch == 0x20) | (lch == 0x3000));
        }

        private string IOStrFromDecimal(decimal Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString("G29", NumberFormat);
        }

        private string IOStrFromDouble(double Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString(null, NumberFormat);
        }

        private string IOStrFromSingle(float Value, NumberFormatInfo NumberFormat)
        {
            return Value.ToString(null, NumberFormat);
        }

        internal void LengthCheck(int Length)
        {
            if (this.m_lRecordLen != -1)
            {
                if (Length > this.m_lRecordLen)
                {
                    throw ExceptionUtils.VbMakeException(0x3b);
                }
                if ((this.GetPos() + Length) > (this.m_lRecordStart + this.m_lRecordLen))
                {
                    throw ExceptionUtils.VbMakeException(0x3b);
                }
            }
        }

        internal string LineInput()
        {
            this.ValidateReadable();
            string s = this.m_sr.ReadLine();
            if (s == null)
            {
                s = "";
            }
            this.m_position += this.m_Encoding.GetByteCount(s) + 2;
            this.m_eof = this.CheckEOF(this.m_sr.Peek());
            return s;
        }

        internal virtual long LOC()
        {
            if ((this.m_lRecordLen == -1) || (this.GetMode() != OpenMode.Random))
            {
                return (this.m_position + 1L);
            }
            if (this.m_lRecordLen == 0)
            {
                throw ExceptionUtils.VbMakeException(0x33);
            }
            if (this.m_position == 0L)
            {
                return 0L;
            }
            return ((this.m_position / ((long) this.m_lRecordLen)) + 1L);
        }

        internal virtual void Lock()
        {
            this.m_file.Lock(0L, 0x7fffffffL);
        }

        internal virtual void Lock(long Record)
        {
            if (this.m_lRecordLen == -1)
            {
                this.m_file.Lock(Record - 1L, 1L);
            }
            else
            {
                this.m_file.Lock((Record - 1L) * this.m_lRecordLen, (long) this.m_lRecordLen);
            }
        }

        internal virtual void Lock(long RecordStart, long RecordEnd)
        {
            if (this.m_lRecordLen == -1)
            {
                this.m_file.Lock(RecordStart - 1L, (RecordEnd - RecordStart) + 1L);
            }
            else
            {
                this.m_file.Lock((RecordStart - 1L) * this.m_lRecordLen, ((RecordEnd - RecordStart) + 1L) * this.m_lRecordLen);
            }
        }

        internal long LOF()
        {
            return this.m_file.Length;
        }

        internal virtual void OpenFile()
        {
            try
            {
                if (File.Exists(this.m_sFullPath))
                {
                    this.m_file = new FileStream(this.m_sFullPath, FileMode.Open, (FileAccess) this.m_access, (FileShare) this.m_share);
                }
                else
                {
                    this.m_file = new FileStream(this.m_sFullPath, FileMode.Create, (FileAccess) this.m_access, (FileShare) this.m_share);
                }
            }
            catch (SecurityException)
            {
                throw ExceptionUtils.VbMakeException(0x35);
            }
        }

        internal void Print(params object[] Output)
        {
            this.SetPrintMode();
            if ((Output != null) && (Output.Length != 0))
            {
                int upperBound = Output.GetUpperBound(0);
                int num2 = -1;
                int num5 = upperBound;
                for (int i = 0; i <= num5; i++)
                {
                    Type underlyingType;
                    string s = null;
                    object obj2 = Output[i];
                    if (obj2 == null)
                    {
                        underlyingType = null;
                    }
                    else
                    {
                        underlyingType = obj2.GetType();
                        if (underlyingType.IsEnum)
                        {
                            underlyingType = Enum.GetUnderlyingType(underlyingType);
                        }
                    }
                    if (obj2 == null)
                    {
                        s = "";
                    }
                    if (underlyingType == null)
                    {
                        s = "";
                    }
                    else
                    {
                        switch (Type.GetTypeCode(underlyingType))
                        {
                            case TypeCode.DBNull:
                                s = "Null";
                                goto Label_0260;

                            case TypeCode.Boolean:
                                s = StringType.FromBoolean(BooleanType.FromObject(obj2));
                                goto Label_0260;

                            case TypeCode.Char:
                                s = StringType.FromChar(Microsoft.VisualBasic.CompilerServices.CharType.FromObject(obj2));
                                goto Label_0260;

                            case TypeCode.Byte:
                                s = this.AddSpaces(StringType.FromByte(ByteType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Int16:
                                s = this.AddSpaces(StringType.FromShort(ShortType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Int32:
                                s = this.AddSpaces(StringType.FromInteger(IntegerType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Int64:
                                s = this.AddSpaces(StringType.FromLong(LongType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Single:
                                s = this.AddSpaces(StringType.FromSingle(SingleType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Double:
                                s = this.AddSpaces(StringType.FromDouble(DoubleType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.Decimal:
                                s = this.AddSpaces(StringType.FromDecimal(DecimalType.FromObject(obj2)));
                                goto Label_0260;

                            case TypeCode.DateTime:
                                s = StringType.FromDate(DateType.FromObject(obj2)) + " ";
                                goto Label_0260;

                            case TypeCode.String:
                                s = obj2.ToString();
                                goto Label_0260;
                        }
                        if (underlyingType == typeof(TabInfo))
                        {
                            this.PrintTab((TabInfo) obj2);
                            num2 = i;
                            continue;
                        }
                        if (underlyingType == typeof(SpcInfo))
                        {
                            SpcInfo info3 = (SpcInfo) obj2;
                            this.SPC(info3.Count);
                            num2 = i;
                            continue;
                        }
                        if (underlyingType != typeof(Missing))
                        {
                            throw new ArgumentException(Utils.GetResourceString("Argument_UnsupportedIOType1", new string[] { Utils.VBFriendlyName(underlyingType) }));
                        }
                        s = "Error 448";
                    }
                Label_0260:
                    if (num2 != (i - 1))
                    {
                        int column = this.GetColumn();
                        this.SetColumn(column + (14 - (column % 14)));
                    }
                    this.WriteString(s);
                }
            }
        }

        internal void PrintLine(params object[] Output)
        {
            this.Print(Output);
            this.WriteLine(null);
        }

        private void PrintTab(TabInfo ti)
        {
            if (ti.Column == -1)
            {
                int column = this.GetColumn();
                column += 14 - (column % 14);
                this.SetColumn(column);
            }
            else
            {
                this.Tab(ti.Column);
            }
        }

        internal virtual void Put(bool Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(byte Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(char Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(DateTime Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(decimal Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(double Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(short Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(int Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(long Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(object Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(float Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(ValueType Value, long RecordNumber = 0L)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(string Value, long RecordNumber = 0L, bool StringIsFixedLength = false)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal virtual void Put(Array Value, long RecordNumber = 0L, bool ArrayIsDynamic = false, bool StringIsFixedLength = false)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal void PutArrayData(Array arr, Type typ, int FixedStringLength, int FirstBound, int SecondBound)
        {
            int num;
            int upperBound;
            string str = null;
            char[] chArray = null;
            int num5;
            int num6;
            object obj2;
            if (arr == null)
            {
                upperBound = -1;
                num = -1;
            }
            else if (arr.GetUpperBound(0) > FirstBound)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_ArrayDimensionsDontMatch"));
            }
            if (typ == null)
            {
                typ = arr.GetType().GetElementType();
            }
            VT vtype = VTFromComType(typ);
            if (SecondBound == -1)
            {
                num5 = 0;
                num6 = FirstBound;
                if (arr != null)
                {
                    upperBound = arr.GetUpperBound(0);
                }
            }
            else
            {
                num5 = SecondBound;
                num6 = FirstBound;
                if (arr != null)
                {
                    if ((arr.Rank != 2) || (arr.GetUpperBound(1) != SecondBound))
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_ArrayDimensionsDontMatch"));
                    }
                    upperBound = arr.GetUpperBound(0);
                    num = arr.GetUpperBound(1);
                }
            }
            if (vtype == VT.String)
            {
                if (FixedStringLength == 0)
                {
                    if (SecondBound == -1)
                    {
                        obj2 = arr.GetValue(0);
                    }
                    else
                    {
                        obj2 = arr.GetValue(0, 0);
                    }
                    if (obj2 != null)
                    {
                        FixedStringLength = obj2.ToString().Length;
                    }
                }
                if (FixedStringLength == 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidFixedLengthString"));
                }
                if (FixedStringLength > 0)
                {
                    str = Strings.StrDup(FixedStringLength, ' ');
                    chArray = str.ToCharArray();
                }
            }
            int byteLength = this.GetByteLength(vtype);
            if (((SecondBound == -1) && (byteLength > 0)) && (num6 == upperBound))
            {
                int count = byteLength * (num6 + 1);
                if ((this.GetPos() + count) <= (this.m_lRecordStart + this.m_lRecordLen))
                {
                    byte[] dst = new byte[(count - 1) + 1];
                    Buffer.BlockCopy(arr, 0, dst, 0, count);
                    this.m_bw.Write(dst);
                    this.m_position += count;
                    return;
                }
            }
            int num10 = num5;
            for (int i = 0; i <= num10; i++)
            {
                int num11 = num6;
                for (int j = 0; j <= num11; j++)
                {
                    string str2;
                    int byteCount;
                    try
                    {
                        if (SecondBound == -1)
                        {
                            if (j > upperBound)
                            {
                                obj2 = null;
                            }
                            else
                            {
                                obj2 = arr.GetValue(j);
                            }
                        }
                        else if ((j > upperBound) || (i > num))
                        {
                            obj2 = null;
                        }
                        else
                        {
                            obj2 = arr.GetValue(j, i);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        obj2 = 0;
                    }
                    switch (vtype)
                    {
                        case VT.Empty:
                        case VT.DBNull:
                        {
                            continue;
                        }
                        case VT.Short:
                        {
                            this.LengthCheck(2);
                            this.m_bw.Write(ShortType.FromObject(obj2));
                            this.m_position += 2L;
                            continue;
                        }
                        case VT.Integer:
                        {
                            this.LengthCheck(4);
                            this.m_bw.Write(IntegerType.FromObject(obj2));
                            this.m_position += 4L;
                            continue;
                        }
                        case VT.Single:
                        {
                            this.LengthCheck(4);
                            this.m_bw.Write(SingleType.FromObject(obj2));
                            this.m_position += 4L;
                            continue;
                        }
                        case VT.Double:
                        {
                            this.LengthCheck(8);
                            this.m_bw.Write(DoubleType.FromObject(obj2));
                            this.m_position += 8L;
                            continue;
                        }
                        case VT.Date:
                        {
                            this.LengthCheck(8);
                            this.m_bw.Write(DateType.FromObject(obj2).ToOADate());
                            this.m_position += 8L;
                            continue;
                        }
                        case VT.String:
                            if (obj2 != null)
                            {
                                goto Label_0446;
                            }
                            if (FixedStringLength <= 0)
                            {
                                goto Label_043A;
                            }
                            str2 = str;
                            byteCount = FixedStringLength;
                            goto Label_04B0;

                        case VT.Error:
                            throw ExceptionUtils.VbMakeException(13);

                        case VT.Boolean:
                            this.LengthCheck(2);
                            if (!BooleanType.FromObject(obj2))
                            {
                                break;
                            }
                            this.m_bw.Write((short) (-1));
                            goto Label_02F1;

                        case VT.Variant:
                        {
                            this.PutObject(obj2, 0L, true);
                            continue;
                        }
                        case VT.Decimal:
                        {
                            this.LengthCheck(8);
                            this.m_bw.Write(decimal.ToOACurrency(DecimalType.FromObject(obj2)));
                            this.m_position += 8L;
                            continue;
                        }
                        case VT.Byte:
                        {
                            this.LengthCheck(1);
                            this.m_bw.Write(ByteType.FromObject(obj2));
                            this.m_position += 1L;
                            continue;
                        }
                        case VT.Char:
                        {
                            this.LengthCheck(2);
                            this.m_bw.Write(Microsoft.VisualBasic.CompilerServices.CharType.FromObject(obj2));
                            this.m_position += 2L;
                            continue;
                        }
                        case VT.Long:
                        {
                            this.LengthCheck(8);
                            this.m_bw.Write(LongType.FromObject(obj2));
                            this.m_position += 8L;
                            continue;
                        }
                        case VT.Structure:
                        {
                            this.PutObject(obj2, 0L, false);
                            continue;
                        }
                        default:
                            goto Label_058E;
                    }
                    this.m_bw.Write((short) 0);
                Label_02F1:
                    this.m_position += 2L;
                    continue;
                Label_043A:
                    str2 = "";
                    byteCount = 0;
                    goto Label_04B0;
                Label_0446:
                    str2 = obj2.ToString();
                    byteCount = this.m_Encoding.GetByteCount(str2);
                    if ((FixedStringLength > 0) && (byteCount > FixedStringLength))
                    {
                        if (byteCount == str2.Length)
                        {
                            str2 = Strings.Left(str2, FixedStringLength);
                            byteCount = FixedStringLength;
                        }
                        else
                        {
                            byte[] bytes = this.m_Encoding.GetBytes(str2);
                            str2 = this.m_Encoding.GetString(bytes, 0, FixedStringLength);
                            byteCount = this.m_Encoding.GetByteCount(str2);
                        }
                    }
                Label_04B0:
                    if (byteCount > 0x7fff)
                    {
                        throw ExceptionUtils.VbMakeException(new ArgumentException(Utils.GetResourceString("FileIO_StringLengthExceeded")), 5);
                    }
                    if (FixedStringLength > 0)
                    {
                        this.LengthCheck(FixedStringLength);
                        this.m_sw.Write(str2);
                        if (byteCount < FixedStringLength)
                        {
                            this.m_sw.Write(chArray, 0, FixedStringLength - byteCount);
                        }
                        this.m_position += FixedStringLength;
                    }
                    else
                    {
                        this.LengthCheck(byteCount + 2);
                        this.m_bw.Write((short) byteCount);
                        this.m_sw.Write(str2);
                        this.m_position += 2 + byteCount;
                    }
                    continue;
                Label_058E:
                    if ((vtype & VT.Array) != VT.Empty)
                    {
                        throw ExceptionUtils.VbMakeException(13);
                    }
                    throw ExceptionUtils.VbMakeException(0x1ca);
                }
            }
        }

        private void PutArrayDesc(Array arr)
        {
            short rank;
            if (arr == null)
            {
                rank = 0;
            }
            else
            {
                rank = (short) arr.Rank;
            }
            this.m_bw.Write(rank);
            this.m_position += 2L;
            if (rank != 0)
            {
                int num3 = rank - 1;
                for (int i = 0; i <= num3; i++)
                {
                    this.m_bw.Write(arr.GetLength(i));
                    this.m_bw.Write(arr.GetLowerBound(i));
                    this.m_position += 8L;
                }
            }
        }

        internal void PutBoolean(long RecordNumber, bool b, bool ContainedInVariant = false)
        {
            int length = 2;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 11);
            }
            if (b)
            {
                this.m_bw.Write((short) (-1));
            }
            else
            {
                this.m_bw.Write((short) 0);
            }
            this.m_position += length;
        }

        internal void PutByte(long RecordNumber, byte byt, bool ContainedInVariant = false)
        {
            int length = 1;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 0x11);
            }
            this.m_bw.Write(byt);
            this.m_position += length;
        }

        internal void PutChar(long RecordNumber, char ch, bool ContainedInVariant = false)
        {
            int length = 2;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 0x12);
            }
            this.m_bw.Write(ch);
            this.m_position += length;
        }

        internal void PutCurrency(long RecordNumber, decimal dec, bool ContainedInVariant = false)
        {
            int length = 0x10;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 6);
            }
            this.m_bw.Write(decimal.ToOACurrency(dec));
            this.m_position += length;
        }

        internal void PutDate(long RecordNumber, DateTime dt, bool ContainedInVariant = false)
        {
            int length = 8;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 7);
            }
            double num = dt.ToOADate();
            this.m_bw.Write(num);
            this.m_position += length;
        }

        internal void PutDecimal(long RecordNumber, decimal dec, bool ContainedInVariant = false)
        {
            byte num6;
            int length = 0x10;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 14);
            }
            int[] bits = decimal.GetBits(dec);
            byte num = (byte) ((bits[3] & 0x7fffffff) / 0x10000);
            int num3 = bits[0];
            int num4 = bits[1];
            int num2 = bits[2];
            if ((bits[3] & -2147483648) != 0)
            {
                num6 = 0x80;
            }
            this.m_bw.Write((short) 14);
            this.m_bw.Write(num);
            this.m_bw.Write(num6);
            this.m_bw.Write(num2);
            this.m_bw.Write(num3);
            this.m_bw.Write(num4);
            this.m_position += length;
        }

        internal void PutDouble(long RecordNumber, double dbl, bool ContainedInVariant = false)
        {
            int length = 8;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 5);
            }
            this.m_bw.Write(dbl);
            this.m_position += length;
        }

        internal void PutDynamicArray(long RecordNumber, Array arr, bool ContainedInVariant = true, int FixedStringLength = -1)
        {
            int rank;
            int upperBound;
            int num3;
            if (arr == null)
            {
                rank = 0;
            }
            else
            {
                rank = arr.Rank;
                upperBound = arr.GetUpperBound(0);
            }
            switch (rank)
            {
                case 1:
                    num3 = -1;
                    break;

                case 2:
                    num3 = arr.GetUpperBound(1);
                    break;

                case 0:
                    break;

                default:
                    throw new ArgumentException(Utils.GetResourceString("Argument_UnsupportedArrayDimensions"));
            }
            this.SetRecord(RecordNumber);
            if (ContainedInVariant)
            {
                VT vt = VTType(arr);
                this.m_bw.Write((short) vt);
                this.m_position += 2L;
                if ((vt & VT.Array) == VT.Empty)
                {
                    throw ExceptionUtils.VbMakeException(0x1ca);
                }
            }
            this.PutArrayDesc(arr);
            if (rank != 0)
            {
                this.PutArrayData(arr, arr.GetType().GetElementType(), FixedStringLength, upperBound, num3);
            }
        }

        internal void PutEmpty(long RecordNumber)
        {
            this.SetRecord(RecordNumber);
            this.LengthCheck(2);
            this.m_bw.Write((short) 0);
            this.m_position += 2L;
        }

        internal void PutFixedArray(long RecordNumber, Array arr, Type ElementType, int FixedStringLength = -1, int FirstBound = -1, int SecondBound = -1)
        {
            this.SetRecord(RecordNumber);
            if (ElementType == null)
            {
                ElementType = arr.GetType().GetElementType();
            }
            this.PutArrayData(arr, ElementType, FixedStringLength, FirstBound, SecondBound);
        }

        internal void PutFixedLengthString(long RecordNumber, string s, int lengthToWrite)
        {
            char character = ' ';
            if (s == null)
            {
                s = "";
            }
            if (s == "")
            {
                character = '\0';
            }
            int byteCount = this.m_Encoding.GetByteCount(s);
            if (byteCount > lengthToWrite)
            {
                if (byteCount == s.Length)
                {
                    s = Strings.Left(s, lengthToWrite);
                }
                else
                {
                    byte[] bytes = this.m_Encoding.GetBytes(s);
                    s = this.m_Encoding.GetString(bytes, 0, lengthToWrite);
                    byteCount = this.m_Encoding.GetByteCount(s);
                    if (byteCount > lengthToWrite)
                    {
                        for (int i = lengthToWrite - 1; i >= 0; i += -1)
                        {
                            bytes[i] = 0;
                            s = this.m_Encoding.GetString(bytes, 0, lengthToWrite);
                            byteCount = this.m_Encoding.GetByteCount(s);
                            if (byteCount <= lengthToWrite)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            if (byteCount < lengthToWrite)
            {
                s = s + Strings.StrDup(lengthToWrite - byteCount, character);
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(lengthToWrite);
            this.m_sw.Write(s);
            this.m_position += lengthToWrite;
        }

        internal void PutInteger(long RecordNumber, int l, bool ContainedInVariant = false)
        {
            int length = 4;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 3);
            }
            this.m_bw.Write(l);
            this.m_position += length;
        }

        internal void PutLong(long RecordNumber, long l, bool ContainedInVariant = false)
        {
            int length = 8;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 20);
            }
            this.m_bw.Write(l);
            this.m_position += length;
        }

        internal virtual void PutObject(object Value, long RecordNumber = 0L, bool ContainedInVariant = true)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        internal void PutRecord(long RecordNumber, ValueType o)
        {
            if (o == null)
            {
                throw new NullReferenceException();
            }
            this.SetRecord(RecordNumber);
            PutHandler handler = new PutHandler(this);
            IRecordEnum intfRecEnum = handler;
            if (intfRecEnum == null)
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            StructUtils.EnumerateUDT(o, intfRecEnum, false);
        }

        internal void PutShort(long RecordNumber, short i, bool ContainedInVariant = false)
        {
            int length = 2;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 2);
            }
            this.m_bw.Write(i);
            this.m_position += length;
        }

        internal void PutSingle(long RecordNumber, float sng, bool ContainedInVariant = false)
        {
            int length = 4;
            if (ContainedInVariant)
            {
                length += 2;
            }
            this.SetRecord(RecordNumber);
            this.LengthCheck(length);
            if (ContainedInVariant)
            {
                this.m_bw.Write((short) 4);
            }
            this.m_bw.Write(sng);
            this.m_position += length;
        }

        internal void PutString(long RecordNumber, string s)
        {
            if (s == null)
            {
                s = "";
            }
            int byteCount = this.m_Encoding.GetByteCount(s);
            this.SetRecord(RecordNumber);
            this.LengthCheck(byteCount);
            if (byteCount != 0)
            {
                this.m_sw.Write(s);
            }
            this.m_position += byteCount;
        }

        internal void PutStringWithLength(long RecordNumber, string s)
        {
            if (s == null)
            {
                s = "";
            }
            int byteCount = this.m_Encoding.GetByteCount(s);
            this.SetRecord(RecordNumber);
            this.LengthCheck(byteCount + 2);
            this.m_bw.Write((short) byteCount);
            if (byteCount != 0)
            {
                this.m_sw.Write(s);
            }
            this.m_position += byteCount + 2;
        }

        internal void PutVariantString(long RecordNumber, string s)
        {
            if (s == null)
            {
                s = "";
            }
            int byteCount = this.m_Encoding.GetByteCount(s);
            this.SetRecord(RecordNumber);
            this.LengthCheck((byteCount + 2) + 2);
            this.m_bw.Write((short) 8);
            this.m_bw.Write((short) byteCount);
            if (byteCount != 0)
            {
                this.m_sw.Write(s);
            }
            this.m_position += (byteCount + 2) + 2;
        }

        protected string ReadInField(short iTermType)
        {
            StringBuilder builder = new StringBuilder();
            string fileInTerm = this.GetFileInTerm(iTermType);
            int lChar = this.m_sr.Peek();
            if (!this.CheckEOF(lChar))
            {
                while (fileInTerm.IndexOf(Strings.ChrW(lChar)) == -1)
                {
                    lChar = this.m_sr.Read();
                    this.m_position += 1L;
                    if (lChar != 0)
                    {
                        builder.Append(Strings.ChrW(lChar));
                    }
                    lChar = this.m_sr.Peek();
                    if (this.CheckEOF(lChar))
                    {
                        this.m_eof = true;
                        break;
                    }
                }
            }
            else
            {
                this.m_eof = true;
            }
            if ((iTermType == 2) || (iTermType == 3))
            {
                return Strings.RTrim(builder.ToString());
            }
            return builder.ToString();
        }

        protected string ReadString()
        {
            int length = this.m_br.ReadInt16();
            this.m_position += 2L;
            if (length == 0)
            {
                return null;
            }
            this.LengthCheck(length);
            return this.ReadString(length);
        }

        protected string ReadString(int ByteLength)
        {
            if (ByteLength == 0)
            {
                return null;
            }
            byte[] bytes = this.m_br.ReadBytes(ByteLength);
            this.m_position += ByteLength;
            return this.m_Encoding.GetString(bytes);
        }

        internal virtual long Seek()
        {
            return (this.m_position + 1L);
        }

        internal virtual void Seek(long BaseOnePosition)
        {
            if (BaseOnePosition <= 0L)
            {
                throw ExceptionUtils.VbMakeException(0x3f);
            }
            long num = BaseOnePosition - 1L;
            if (num > this.m_file.Length)
            {
                this.m_file.SetLength(num);
            }
            this.m_file.Position = num;
            this.m_position = num;
            this.m_eof = this.m_position >= this.m_file.Length;
            if (this.m_sr != null)
            {
                this.m_sr.DiscardBufferedData();
            }
        }

        internal void SeekOffset(long offset)
        {
            this.m_position = offset;
            this.m_file.Position = offset;
            if (this.m_sr != null)
            {
                this.m_sr.DiscardBufferedData();
            }
        }

        internal void SetColumn(int lColumn)
        {
            if (((this.m_lWidth != 0) && (this.m_lCurrentColumn != 0)) && ((lColumn + 14) > this.m_lWidth))
            {
                this.WriteLine(null);
            }
            else
            {
                this.SPC(lColumn - this.m_lCurrentColumn);
            }
        }

        internal void SetPrintMode()
        {
            switch (this.GetMode())
            {
                case OpenMode.Input:
                case OpenMode.Binary:
                case OpenMode.Random:
                    throw ExceptionUtils.VbMakeException(0x36);
            }
            this.m_bPrint = true;
        }

        internal void SetRecord(long RecordNumber)
        {
            if ((this.m_lRecordLen != 0) && (RecordNumber != 0L))
            {
                long pos;
                if (this.m_lRecordLen == -1)
                {
                    if (RecordNumber == -1L)
                    {
                        return;
                    }
                    pos = RecordNumber - 1L;
                }
                else if (RecordNumber == -1L)
                {
                    pos = this.GetPos();
                    if (pos == 0L)
                    {
                        this.m_lRecordStart = 0L;
                        return;
                    }
                    if ((pos % ((long) this.m_lRecordLen)) == 0L)
                    {
                        this.m_lRecordStart = pos;
                        return;
                    }
                    pos = this.m_lRecordLen * ((pos / ((long) this.m_lRecordLen)) + 1L);
                }
                else if (RecordNumber != 0L)
                {
                    if (this.m_lRecordLen == -1)
                    {
                        pos = RecordNumber;
                    }
                    else
                    {
                        pos = (RecordNumber - 1L) * this.m_lRecordLen;
                    }
                }
                this.SeekOffset(pos);
                this.m_lRecordStart = pos;
            }
        }

        internal void SetWidth(int RecordWidth)
        {
            if ((RecordWidth < 0) || (RecordWidth > 0xff))
            {
                throw ExceptionUtils.VbMakeException(5);
            }
            this.m_lWidth = RecordWidth;
        }

        protected void SkipTrailingWhiteSpace()
        {
            int lChar = this.m_sr.Peek();
            if (this.CheckEOF(lChar))
            {
                this.m_eof = true;
            }
            else
            {
                if ((this.IntlIsSpace(lChar) || this.IntlIsDoubleQuote(lChar)) || (lChar == 9))
                {
                    lChar = this.m_sr.Read();
                    this.m_position += 1L;
                    lChar = this.m_sr.Peek();
                    if (!this.CheckEOF(lChar))
                    {
                        while (this.IntlIsSpace(lChar) || (lChar == 9))
                        {
                            this.m_sr.Read();
                            this.m_position += 1L;
                            lChar = this.m_sr.Peek();
                            if (this.CheckEOF(lChar))
                            {
                                this.m_eof = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        this.m_eof = true;
                        return;
                    }
                }
                if (lChar == 13)
                {
                    lChar = this.m_sr.Read();
                    this.m_position += 1L;
                    if (this.CheckEOF(lChar))
                    {
                        this.m_eof = true;
                        return;
                    }
                    if (this.m_sr.Peek() == 10)
                    {
                        lChar = this.m_sr.Read();
                        this.m_position += 1L;
                    }
                }
                else if (this.IntlIsComma(lChar))
                {
                    lChar = this.m_sr.Read();
                    this.m_position += 1L;
                }
                lChar = this.m_sr.Peek();
                if (this.CheckEOF(lChar))
                {
                    this.m_eof = true;
                }
            }
        }

        protected int SkipWhiteSpace()
        {
            int lChar = this.m_sr.Peek();
            if (!this.CheckEOF(lChar))
            {
                while (this.IntlIsSpace(lChar) || (lChar == 9))
                {
                    this.m_sr.Read();
                    this.m_position += 1L;
                    lChar = this.m_sr.Peek();
                    if (this.CheckEOF(lChar))
                    {
                        this.m_eof = true;
                        return lChar;
                    }
                }
                return lChar;
            }
            this.m_eof = true;
            return lChar;
        }

        protected int SkipWhiteSpaceEOF()
        {
            int lChar = this.SkipWhiteSpace();
            if (this.CheckEOF(lChar))
            {
                throw ExceptionUtils.VbMakeException(0x3e);
            }
            return lChar;
        }

        internal void SPC(int iCount)
        {
            if (iCount <= 0)
            {
                return;
            }
            int column = this.GetColumn();
            int width = this.GetWidth();
            if (width != 0)
            {
                if (iCount >= width)
                {
                    iCount = iCount % width;
                }
                if ((iCount + column) > width)
                {
                    iCount -= width - column;
                    goto Label_0038;
                }
            }
            iCount += column;
            if (iCount >= column)
            {
                goto Label_0041;
            }
        Label_0038:
            this.WriteLine(null);
            column = 0;
        Label_0041:
            if (iCount > column)
            {
                string s = new string(' ', iCount - column);
                this.WriteString(s);
            }
        }

        internal void Tab(int Column)
        {
            if (Column < 1)
            {
                Column = 1;
            }
            Column--;
            int column = this.GetColumn();
            int width = this.GetWidth();
            if ((width != 0) && (Column >= width))
            {
                Column = Column % width;
            }
            if (Column < column)
            {
                this.WriteLine(null);
                column = 0;
            }
            if (Column > column)
            {
                string s = new string(' ', Column - column);
                this.WriteString(s);
            }
        }

        internal virtual void Unlock()
        {
            this.m_file.Unlock(0L, 0x7fffffffL);
        }

        internal virtual void Unlock(long Record)
        {
            if (this.m_lRecordLen == -1)
            {
                this.m_file.Unlock(Record - 1L, 1L);
            }
            else
            {
                this.m_file.Unlock((Record - 1L) * this.m_lRecordLen, (long) this.m_lRecordLen);
            }
        }

        internal virtual void Unlock(long RecordStart, long RecordEnd)
        {
            if (this.m_lRecordLen == -1)
            {
                this.m_file.Unlock(RecordStart - 1L, (RecordEnd - RecordStart) + 1L);
            }
            else
            {
                this.m_file.Unlock((RecordStart - 1L) * this.m_lRecordLen, ((RecordEnd - RecordStart) + 1L) * this.m_lRecordLen);
            }
        }

        private void ValidateReadable()
        {
            if ((this.m_access != OpenAccess.ReadWrite) && (this.m_access != OpenAccess.Read))
            {
                NullReferenceException exception = new NullReferenceException();
                throw new NullReferenceException(exception.Message, new IOException(Utils.GetResourceString("FileOpenedNoRead")));
            }
        }

        protected void ValidateRec(long RecordNumber)
        {
            if (RecordNumber < 1L)
            {
                throw ExceptionUtils.VbMakeException(0x3f);
            }
        }

        internal static VT VTFromComType(Type typ)
        {
            if (typ != null)
            {
                if (typ.IsArray)
                {
                    typ = typ.GetElementType();
                    if (typ.IsArray)
                    {
                        return (VT.Array | VT.Single | VT.String);
                    }
                    VT vt2 = VTFromComType(typ);
                    if ((vt2 & VT.Array) != VT.Empty)
                    {
                        return (VT.Array | VT.Single | VT.String);
                    }
                    return (vt2 | VT.Array);
                }
                if (typ.IsEnum)
                {
                    typ = Enum.GetUnderlyingType(typ);
                }
                if (typ == null)
                {
                    return VT.Empty;
                }
                switch (Type.GetTypeCode(typ))
                {
                    case TypeCode.DBNull:
                        return VT.DBNull;

                    case TypeCode.Boolean:
                        return VT.Boolean;

                    case TypeCode.Char:
                        return VT.Char;

                    case TypeCode.Byte:
                        return VT.Byte;

                    case TypeCode.Int16:
                        return VT.Short;

                    case TypeCode.Int32:
                        return VT.Integer;

                    case TypeCode.Int64:
                        return VT.Long;

                    case TypeCode.Single:
                        return VT.Single;

                    case TypeCode.Double:
                        return VT.Double;

                    case TypeCode.Decimal:
                        return VT.Decimal;

                    case TypeCode.DateTime:
                        return VT.Date;

                    case TypeCode.String:
                        return VT.String;
                }
                if (typ == typeof(Missing))
                {
                    return VT.Error;
                }
                if ((typ == typeof(Exception)) || typ.IsSubclassOf(typeof(Exception)))
                {
                    return VT.Error;
                }
                if (typ.IsValueType)
                {
                    return VT.Structure;
                }
            }
            return VT.Variant;
        }

        internal static VT VTType(object VarName)
        {
            if (VarName == null)
            {
                return VT.Variant;
            }
            return VTFromComType(VarName.GetType());
        }

        [SecurityCritical]
        internal void WriteHelper(params object[] Output)
        {
            this.InternalWriteHelper(Output);
            this.WriteString(",");
        }

        internal virtual void WriteLine(string s)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }

        [SecurityCritical]
        internal void WriteLineHelper(params object[] Output)
        {
            this.InternalWriteHelper(Output);
            this.WriteLine(null);
        }

        internal virtual void WriteString(string s)
        {
            throw ExceptionUtils.VbMakeException(0x36);
        }
    }
}

