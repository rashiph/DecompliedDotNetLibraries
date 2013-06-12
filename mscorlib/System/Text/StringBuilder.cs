namespace System.Text
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public sealed class StringBuilder : ISerializable
    {
        private const string CapacityField = "Capacity";
        internal const int DefaultCapacity = 0x10;
        internal char[] m_ChunkChars;
        internal int m_ChunkLength;
        internal int m_ChunkOffset;
        internal StringBuilder m_ChunkPrevious;
        internal int m_MaxCapacity;
        private const string MaxCapacityField = "m_MaxCapacity";
        internal const int MaxChunkSize = 0x1f40;
        private const string StringValueField = "m_StringValue";
        private const string ThreadIDField = "m_currentThread";

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder() : this(0x10)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder(int capacity) : this(string.Empty, capacity)
        {
        }

        public StringBuilder(string value) : this(value, 0x10)
        {
        }

        private StringBuilder(StringBuilder from)
        {
            this.m_ChunkLength = from.m_ChunkLength;
            this.m_ChunkOffset = from.m_ChunkOffset;
            this.m_ChunkChars = from.m_ChunkChars;
            this.m_ChunkPrevious = from.m_ChunkPrevious;
            this.m_MaxCapacity = from.m_MaxCapacity;
        }

        [SecuritySafeCritical]
        public StringBuilder(int capacity, int maxCapacity)
        {
            if (capacity > maxCapacity)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
            }
            if (maxCapacity < 1)
            {
                throw new ArgumentOutOfRangeException("maxCapacity", Environment.GetResourceString("ArgumentOutOfRange_SmallMaxCapacity"));
            }
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_MustBePositive", new object[] { "capacity" }));
            }
            if (capacity == 0)
            {
                capacity = Math.Min(0x10, maxCapacity);
            }
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkChars = new char[capacity];
        }

        [SecurityCritical]
        private StringBuilder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            int length = 0;
            string str = null;
            int num2 = 0x7fffffff;
            bool flag = false;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string name = enumerator.Name;
                if (name != null)
                {
                    if (!(name == "m_MaxCapacity"))
                    {
                        if (name == "m_StringValue")
                        {
                            goto Label_0070;
                        }
                        if (name == "Capacity")
                        {
                            goto Label_007E;
                        }
                    }
                    else
                    {
                        num2 = info.GetInt32("m_MaxCapacity");
                    }
                }
                continue;
            Label_0070:
                str = info.GetString("m_StringValue");
                continue;
            Label_007E:
                length = info.GetInt32("Capacity");
                flag = true;
            }
            if (str == null)
            {
                str = string.Empty;
            }
            if ((num2 < 1) || (str.Length > num2))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_StringBuilderMaxCapacity"));
            }
            if (!flag)
            {
                length = 0x10;
                if (length < str.Length)
                {
                    length = str.Length;
                }
                if (length > num2)
                {
                    length = num2;
                }
            }
            if (((length < 0) || (length < str.Length)) || (length > num2))
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_StringBuilderCapacity"));
            }
            this.m_MaxCapacity = num2;
            this.m_ChunkChars = new char[length];
            str.CopyTo(0, this.m_ChunkChars, 0, str.Length);
            this.m_ChunkLength = str.Length;
            this.m_ChunkPrevious = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder(string value, int capacity) : this(value, 0, (value != null) ? value.Length : 0, capacity)
        {
        }

        private StringBuilder(int size, int maxCapacity, StringBuilder previousBlock)
        {
            this.m_ChunkChars = new char[size];
            this.m_MaxCapacity = maxCapacity;
            this.m_ChunkPrevious = previousBlock;
            if (previousBlock != null)
            {
                this.m_ChunkOffset = previousBlock.m_ChunkOffset + previousBlock.m_ChunkLength;
            }
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder(string value, int startIndex, int length, int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_MustBePositive", new object[] { "capacity" }));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", new object[] { "length" }));
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (value == null)
            {
                value = string.Empty;
            }
            if (startIndex > (value.Length - length))
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
            }
            this.m_MaxCapacity = 0x7fffffff;
            if (capacity == 0)
            {
                capacity = 0x10;
            }
            if (capacity < length)
            {
                capacity = length;
            }
            this.m_ChunkChars = new char[capacity];
            this.m_ChunkLength = length;
            fixed (char* str = ((char*) value))
            {
                char* chPtr = str;
                ThreadSafeCopy(chPtr + startIndex, this.m_ChunkChars, 0, length);
            }
        }

        public StringBuilder Append(bool value)
        {
            return this.Append(value.ToString());
        }

        public StringBuilder Append(byte value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [SecuritySafeCritical]
        public StringBuilder Append(char value)
        {
            if (this.m_ChunkLength < this.m_ChunkChars.Length)
            {
                this.m_ChunkChars[this.m_ChunkLength++] = value;
            }
            else
            {
                this.Append(value, 1);
            }
            return this;
        }

        public StringBuilder Append(decimal value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public StringBuilder Append(double value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public StringBuilder Append(short value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder Append(int value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public StringBuilder Append(long value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder Append(object value)
        {
            if (value == null)
            {
                return this;
            }
            return this.Append(value.ToString());
        }

        [CLSCompliant(false)]
        public StringBuilder Append(sbyte value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        public StringBuilder Append(float value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Append(string value)
        {
            if (value != null)
            {
                char[] chunkChars = this.m_ChunkChars;
                int chunkLength = this.m_ChunkLength;
                int length = value.Length;
                int num3 = chunkLength + length;
                if (num3 < chunkChars.Length)
                {
                    if (length <= 2)
                    {
                        if (length > 0)
                        {
                            chunkChars[chunkLength] = value[0];
                        }
                        if (length > 1)
                        {
                            chunkChars[chunkLength + 1] = value[1];
                        }
                    }
                    else
                    {
                        fixed (char* str = ((char*) value))
                        {
                            char* smem = str;
                            fixed (char* chRef = &(chunkChars[chunkLength]))
                            {
                                string.wstrcpy(chRef, smem, length);
                            }
                        }
                    }
                    this.m_ChunkLength = num3;
                }
                else
                {
                    this.AppendHelper(value);
                }
            }
            return this;
        }

        [CLSCompliant(false)]
        public StringBuilder Append(ushort value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [CLSCompliant(false)]
        public StringBuilder Append(uint value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [CLSCompliant(false)]
        public StringBuilder Append(ulong value)
        {
            return this.Append(value.ToString(CultureInfo.CurrentCulture));
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Append(char[] value)
        {
            if ((value != null) && (value.Length > 0))
            {
                fixed (char* chRef = value)
                {
                    this.Append(chRef, value.Length);
                }
            }
            return this;
        }

        [SecuritySafeCritical]
        public StringBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
            {
                throw new ArgumentOutOfRangeException("repeatCount", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
            }
            if (repeatCount != 0)
            {
                int chunkLength = this.m_ChunkLength;
                while (repeatCount > 0)
                {
                    if (chunkLength < this.m_ChunkChars.Length)
                    {
                        this.m_ChunkChars[chunkLength++] = value;
                        repeatCount--;
                    }
                    else
                    {
                        this.m_ChunkLength = chunkLength;
                        this.ExpandByABlock(repeatCount);
                        chunkLength = 0;
                    }
                }
                this.m_ChunkLength = chunkLength;
            }
            return this;
        }

        [SecuritySafeCritical]
        internal unsafe StringBuilder Append(char* value, int valueCount)
        {
            int num = valueCount + this.m_ChunkLength;
            if (num <= this.m_ChunkChars.Length)
            {
                ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, valueCount);
                this.m_ChunkLength = num;
            }
            else
            {
                int count = this.m_ChunkChars.Length - this.m_ChunkLength;
                if (count > 0)
                {
                    ThreadSafeCopy(value, this.m_ChunkChars, this.m_ChunkLength, count);
                    this.m_ChunkLength = this.m_ChunkChars.Length;
                }
                int minBlockCharCount = valueCount - count;
                this.ExpandByABlock(minBlockCharCount);
                ThreadSafeCopy(value + count, this.m_ChunkChars, 0, minBlockCharCount);
                this.m_ChunkLength = minBlockCharCount;
            }
            return this;
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Append(char[] value, int startIndex, int charCount)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (value == null)
            {
                if ((startIndex != 0) || (charCount != 0))
                {
                    throw new ArgumentNullException("value");
                }
                return this;
            }
            if (charCount > (value.Length - startIndex))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (charCount != 0)
            {
                fixed (char* chRef = &(value[startIndex]))
                {
                    this.Append(chRef, charCount);
                }
            }
            return this;
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Append(string value, int startIndex, int count)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (value == null)
            {
                if ((startIndex != 0) || (count != 0))
                {
                    throw new ArgumentNullException("value");
                }
                return this;
            }
            if (count != 0)
            {
                if (startIndex > (value.Length - count))
                {
                    throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                fixed (char* str = ((char*) value))
                {
                    char* chPtr = str;
                    this.Append(chPtr + startIndex, count);
                }
            }
            return this;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public StringBuilder AppendFormat(string format, object arg0)
        {
            return this.AppendFormat(null, format, new object[] { arg0 });
        }

        public StringBuilder AppendFormat(string format, params object[] args)
        {
            return this.AppendFormat(null, format, args);
        }

        [SecuritySafeCritical]
        public StringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            if ((format == null) || (args == null))
            {
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            int num = 0;
            int length = format.Length;
            char ch = '\0';
            ICustomFormatter formatter = null;
            if (provider != null)
            {
                formatter = (ICustomFormatter) provider.GetFormat(typeof(ICustomFormatter));
            }
        Label_0096:
            while (num < length)
            {
                ch = format[num];
                num++;
                if (ch == '}')
                {
                    if ((num < length) && (format[num] == '}'))
                    {
                        num++;
                    }
                    else
                    {
                        FormatError();
                    }
                }
                if (ch == '{')
                {
                    if ((num < length) && (format[num] == '{'))
                    {
                        num++;
                    }
                    else
                    {
                        num--;
                        break;
                    }
                }
                this.Append(ch);
            }
            if (num == length)
            {
                return this;
            }
            num++;
            if (((num == length) || ((ch = format[num]) < '0')) || (ch > '9'))
            {
                FormatError();
            }
            int index = 0;
            do
            {
                index = ((index * 10) + ch) - 0x30;
                num++;
                if (num == length)
                {
                    FormatError();
                }
                ch = format[num];
            }
            while (((ch >= '0') && (ch <= '9')) && (index < 0xf4240));
            if (index >= args.Length)
            {
                throw new FormatException(Environment.GetResourceString("Format_IndexOutOfRange"));
            }
            while ((num < length) && ((ch = format[num]) == ' '))
            {
                num++;
            }
            bool flag = false;
            int num4 = 0;
            if (ch == ',')
            {
                num++;
                while ((num < length) && (format[num] == ' '))
                {
                    num++;
                }
                if (num == length)
                {
                    FormatError();
                }
                ch = format[num];
                if (ch == '-')
                {
                    flag = true;
                    num++;
                    if (num == length)
                    {
                        FormatError();
                    }
                    ch = format[num];
                }
                if ((ch < '0') || (ch > '9'))
                {
                    FormatError();
                }
                do
                {
                    num4 = ((num4 * 10) + ch) - 0x30;
                    num++;
                    if (num == length)
                    {
                        FormatError();
                    }
                    ch = format[num];
                }
                while (((ch >= '0') && (ch <= '9')) && (num4 < 0xf4240));
            }
            while ((num < length) && ((ch = format[num]) == ' '))
            {
                num++;
            }
            object arg = args[index];
            StringBuilder builder = null;
            if (ch == ':')
            {
                num++;
                while (true)
                {
                    if (num == length)
                    {
                        FormatError();
                    }
                    ch = format[num];
                    num++;
                    switch (ch)
                    {
                        case '{':
                            if ((num < length) && (format[num] == '{'))
                            {
                                num++;
                            }
                            else
                            {
                                FormatError();
                            }
                            break;

                        case '}':
                            if ((num < length) && (format[num] == '}'))
                            {
                                num++;
                            }
                            else
                            {
                                num--;
                                goto Label_0250;
                            }
                            break;
                    }
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }
                    builder.Append(ch);
                }
            }
        Label_0250:
            if (ch != '}')
            {
                FormatError();
            }
            num++;
            string str = null;
            string str2 = null;
            if (formatter != null)
            {
                if (builder != null)
                {
                    str = builder.ToString();
                }
                str2 = formatter.Format(str, arg, provider);
            }
            if (str2 == null)
            {
                IFormattable formattable = arg as IFormattable;
                if (formattable != null)
                {
                    if ((str == null) && (builder != null))
                    {
                        str = builder.ToString();
                    }
                    str2 = formattable.ToString(str, provider);
                }
                else if (arg != null)
                {
                    str2 = arg.ToString();
                }
            }
            if (str2 == null)
            {
                str2 = string.Empty;
            }
            int repeatCount = num4 - str2.Length;
            if (!flag && (repeatCount > 0))
            {
                this.Append(' ', repeatCount);
            }
            this.Append(str2);
            if (flag && (repeatCount > 0))
            {
                this.Append(' ', repeatCount);
            }
            goto Label_0096;
        }

        public StringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            return this.AppendFormat(null, format, new object[] { arg0, arg1 });
        }

        public StringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            return this.AppendFormat(null, format, new object[] { arg0, arg1, arg2 });
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private unsafe void AppendHelper(string value)
        {
            fixed (char* str = ((char*) value))
            {
                char* chPtr = str;
                this.Append(chPtr, value.Length);
            }
        }

        [ComVisible(false)]
        public StringBuilder AppendLine()
        {
            return this.Append(Environment.NewLine);
        }

        [ComVisible(false)]
        public StringBuilder AppendLine(string value)
        {
            this.Append(value);
            return this.Append(Environment.NewLine);
        }

        public StringBuilder Clear()
        {
            this.Length = 0;
            return this;
        }

        [ComVisible(false), SecuritySafeCritical]
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("Arg_NegativeArgCount"));
            }
            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", new object[] { "destinationIndex" }));
            }
            if (destinationIndex > (destination.Length - count))
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_OffsetOut"));
            }
            if (sourceIndex > this.Length)
            {
                throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (sourceIndex > (this.Length - count))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_LongerThanSrcString"));
            }
            StringBuilder chunkPrevious = this;
            int num = sourceIndex + count;
            int num2 = destinationIndex + count;
            while (count > 0)
            {
                int chunkLength = num - chunkPrevious.m_ChunkOffset;
                if (chunkLength >= 0)
                {
                    if (chunkLength > chunkPrevious.m_ChunkLength)
                    {
                        chunkLength = chunkPrevious.m_ChunkLength;
                    }
                    int num4 = count;
                    int num5 = chunkLength - count;
                    if (num5 < 0)
                    {
                        num4 += num5;
                        num5 = 0;
                    }
                    num2 -= num4;
                    count -= num4;
                    ThreadSafeCopy(chunkPrevious.m_ChunkChars, num5, destination, num2, num4);
                }
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
            }
        }

        public int EnsureCapacity(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
            }
            if (this.Capacity < capacity)
            {
                this.Capacity = capacity;
            }
            return this.Capacity;
        }

        public bool Equals(StringBuilder sb)
        {
            if (sb != null)
            {
                if (((this.Capacity != sb.Capacity) || (this.MaxCapacity != sb.MaxCapacity)) || (this.Length != sb.Length))
                {
                    return false;
                }
                if (sb == this)
                {
                    return true;
                }
                StringBuilder chunkPrevious = this;
                int chunkLength = chunkPrevious.m_ChunkLength;
                StringBuilder builder2 = sb;
                int index = builder2.m_ChunkLength;
                do
                {
                    chunkLength--;
                    index--;
                    while (chunkLength < 0)
                    {
                        chunkPrevious = chunkPrevious.m_ChunkPrevious;
                        if (chunkPrevious == null)
                        {
                            break;
                        }
                        chunkLength = chunkPrevious.m_ChunkLength + chunkLength;
                    }
                    while (index < 0)
                    {
                        builder2 = builder2.m_ChunkPrevious;
                        if (builder2 == null)
                        {
                            break;
                        }
                        index = builder2.m_ChunkLength + index;
                    }
                    if (chunkLength < 0)
                    {
                        return (index < 0);
                    }
                }
                while ((index >= 0) && (chunkPrevious.m_ChunkChars[chunkLength] == builder2.m_ChunkChars[index]));
            }
            return false;
        }

        private void ExpandByABlock(int minBlockCharCount)
        {
            if ((minBlockCharCount + this.Length) > this.m_MaxCapacity)
            {
                throw new ArgumentOutOfRangeException("requiredLength", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
            }
            int num = Math.Max(minBlockCharCount, Math.Min(this.Length, 0x1f40));
            this.m_ChunkPrevious = new StringBuilder(this);
            this.m_ChunkOffset += this.m_ChunkLength;
            this.m_ChunkLength = 0;
            if ((this.m_ChunkOffset + num) < num)
            {
                this.m_ChunkChars = null;
                throw new OutOfMemoryException();
            }
            this.m_ChunkChars = new char[num];
        }

        private StringBuilder FindChunkForByte(int byteIndex)
        {
            StringBuilder chunkPrevious = this;
            while ((chunkPrevious.m_ChunkOffset * 2) > byteIndex)
            {
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
            }
            return chunkPrevious;
        }

        private StringBuilder FindChunkForIndex(int index)
        {
            StringBuilder chunkPrevious = this;
            while (chunkPrevious.m_ChunkOffset > index)
            {
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
            }
            return chunkPrevious;
        }

        private static void FormatError()
        {
            throw new FormatException(Environment.GetResourceString("Format_InvalidString"));
        }

        public StringBuilder Insert(int index, bool value)
        {
            return this.Insert(index, value.ToString(), 1);
        }

        public StringBuilder Insert(int index, byte value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Insert(int index, char value)
        {
            this.Insert(index, &value, 1);
            return this;
        }

        public StringBuilder Insert(int index, decimal value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, double value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, short value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, int value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, long value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, object value)
        {
            if (value == null)
            {
                return this;
            }
            return this.Insert(index, value.ToString(), 1);
        }

        [CLSCompliant(false)]
        public StringBuilder Insert(int index, sbyte value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        public StringBuilder Insert(int index, float value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Insert(int index, string value)
        {
            if (index > this.Length)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (value != null)
            {
                fixed (char* str = ((char*) value))
                {
                    char* chPtr = str;
                    this.Insert(index, chPtr, value.Length);
                }
            }
            return this;
        }

        public StringBuilder Insert(int index, char[] value)
        {
            if (index > this.Length)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (value != null)
            {
                this.Insert(index, value, 0, value.Length);
            }
            return this;
        }

        [CLSCompliant(false)]
        public StringBuilder Insert(int index, ushort value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        [CLSCompliant(false)]
        public StringBuilder Insert(int index, uint value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        [CLSCompliant(false)]
        public StringBuilder Insert(int index, ulong value)
        {
            return this.Insert(index, value.ToString(CultureInfo.CurrentCulture), 1);
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Insert(int index, string value, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            int length = this.Length;
            if (index > length)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (((value != null) && (value.Length != 0)) && (count != 0))
            {
                StringBuilder builder;
                int num3;
                long num2 = value.Length * count;
                if (num2 > (this.MaxCapacity - this.Length))
                {
                    throw new OutOfMemoryException();
                }
                this.MakeRoom(index, (int) num2, out builder, out num3, false);
                fixed (char* str = ((char*) value))
                {
                    char* chPtr = str;
                    while (count > 0)
                    {
                        this.ReplaceInPlaceAtChunk(ref builder, ref num3, chPtr, value.Length);
                        count--;
                    }
                }
            }
            return this;
        }

        [SecuritySafeCritical]
        private unsafe void Insert(int index, char* value, int valueCount)
        {
            if (index > this.Length)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (valueCount > 0)
            {
                StringBuilder builder;
                int num;
                this.MakeRoom(index, valueCount, out builder, out num, false);
                this.ReplaceInPlaceAtChunk(ref builder, ref num, value, valueCount);
            }
        }

        [SecuritySafeCritical]
        public unsafe StringBuilder Insert(int index, char[] value, int startIndex, int charCount)
        {
            int length = this.Length;
            if (index > length)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (value == null)
            {
                if ((startIndex != 0) || (charCount != 0))
                {
                    throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_String"));
                }
                return this;
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (startIndex > (value.Length - charCount))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (charCount > 0)
            {
                fixed (char* chRef = &(value[startIndex]))
                {
                    this.Insert(index, chRef, charCount);
                }
            }
            return this;
        }

        [ForceTokenStabilization, SecurityCritical]
        internal unsafe void InternalCopy(IntPtr dest, int len)
        {
            if (len != 0)
            {
                bool flag = true;
                byte* numPtr = (byte*) dest.ToPointer();
                StringBuilder chunkPrevious = this.FindChunkForByte(len);
                do
                {
                    int num = chunkPrevious.m_ChunkOffset * 2;
                    int num2 = chunkPrevious.m_ChunkLength * 2;
                    fixed (char* chRef = chunkPrevious.m_ChunkChars)
                    {
                        byte* src = (byte*) chRef;
                        if (flag)
                        {
                            flag = false;
                            Buffer.memcpyimpl(src, numPtr + num, len - num);
                        }
                        else
                        {
                            Buffer.memcpyimpl(src, numPtr + num, num2);
                        }
                    }
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                }
                while (chunkPrevious != null);
            }
        }

        [SecuritySafeCritical]
        private unsafe void MakeRoom(int index, int count, out StringBuilder chunk, out int indexInChunk, bool doneMoveFollowingChars)
        {
            if ((count + this.Length) > this.m_MaxCapacity)
            {
                throw new ArgumentOutOfRangeException("requiredLength", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
            }
            chunk = this;
            while (chunk.m_ChunkOffset > index)
            {
                chunk.m_ChunkOffset += count;
                chunk = chunk.m_ChunkPrevious;
            }
            indexInChunk = index - chunk.m_ChunkOffset;
            if ((!doneMoveFollowingChars && (chunk.m_ChunkLength <= 0x20)) && ((chunk.m_ChunkChars.Length - chunk.m_ChunkLength) >= count))
            {
                int chunkLength = chunk.m_ChunkLength;
                while (chunkLength > indexInChunk)
                {
                    chunkLength--;
                    chunk.m_ChunkChars[chunkLength + count] = chunk.m_ChunkChars[chunkLength];
                }
                chunk.m_ChunkLength += count;
            }
            else
            {
                StringBuilder builder = new StringBuilder(Math.Max(count, 0x10), chunk.m_MaxCapacity, chunk.m_ChunkPrevious) {
                    m_ChunkLength = count
                };
                int num2 = Math.Min(count, indexInChunk);
                if (num2 > 0)
                {
                    fixed (char* chRef = chunk.m_ChunkChars)
                    {
                        ThreadSafeCopy(chRef, builder.m_ChunkChars, 0, num2);
                        int num3 = indexInChunk - num2;
                        if (num3 >= 0)
                        {
                            ThreadSafeCopy(chRef + num2, chunk.m_ChunkChars, 0, num3);
                            indexInChunk = num3;
                        }
                    }
                }
                chunk.m_ChunkPrevious = builder;
                chunk.m_ChunkOffset += count;
                if (num2 < count)
                {
                    chunk = builder;
                    indexInChunk = num2;
                }
            }
        }

        private StringBuilder Next(StringBuilder chunk)
        {
            if (chunk == this)
            {
                return null;
            }
            return this.FindChunkForIndex(chunk.m_ChunkOffset + chunk.m_ChunkLength);
        }

        [SecuritySafeCritical]
        public StringBuilder Remove(int startIndex, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (length > (this.Length - startIndex))
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((this.Length == length) && (startIndex == 0))
            {
                this.Length = 0;
                return this;
            }
            if (length > 0)
            {
                StringBuilder builder;
                int num;
                this.Remove(startIndex, length, out builder, out num);
            }
            return this;
        }

        private void Remove(int startIndex, int count, out StringBuilder chunk, out int indexInChunk)
        {
            int num = startIndex + count;
            chunk = this;
            StringBuilder builder = null;
            int sourceIndex = 0;
            while (true)
            {
                if ((num - chunk.m_ChunkOffset) >= 0)
                {
                    if (builder == null)
                    {
                        builder = chunk;
                        sourceIndex = num - builder.m_ChunkOffset;
                    }
                    if ((startIndex - chunk.m_ChunkOffset) >= 0)
                    {
                        indexInChunk = startIndex - chunk.m_ChunkOffset;
                        int destinationIndex = indexInChunk;
                        int num4 = builder.m_ChunkLength - sourceIndex;
                        if (builder != chunk)
                        {
                            destinationIndex = 0;
                            chunk.m_ChunkLength = indexInChunk;
                            builder.m_ChunkPrevious = chunk;
                            builder.m_ChunkOffset = chunk.m_ChunkOffset + chunk.m_ChunkLength;
                            if (indexInChunk == 0)
                            {
                                builder.m_ChunkPrevious = chunk.m_ChunkPrevious;
                                chunk = builder;
                            }
                        }
                        builder.m_ChunkLength -= sourceIndex - destinationIndex;
                        if (destinationIndex != sourceIndex)
                        {
                            ThreadSafeCopy(builder.m_ChunkChars, sourceIndex, builder.m_ChunkChars, destinationIndex, num4);
                        }
                        return;
                    }
                }
                else
                {
                    chunk.m_ChunkOffset -= count;
                }
                chunk = chunk.m_ChunkPrevious;
            }
        }

        public StringBuilder Replace(char oldChar, char newChar)
        {
            return this.Replace(oldChar, newChar, 0, this.Length);
        }

        [SecuritySafeCritical]
        public StringBuilder Replace(string oldValue, string newValue)
        {
            return this.Replace(oldValue, newValue, 0, this.Length);
        }

        [SecuritySafeCritical]
        public StringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            int num3;
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            int num2 = startIndex + count;
            StringBuilder chunkPrevious = this;
        Label_0048:
            num3 = num2 - chunkPrevious.m_ChunkOffset;
            int num4 = startIndex - chunkPrevious.m_ChunkOffset;
            if (num3 >= 0)
            {
                int index = Math.Max(num4, 0);
                int num6 = Math.Min(chunkPrevious.m_ChunkLength, num3);
                while (index < num6)
                {
                    if (chunkPrevious.m_ChunkChars[index] == oldChar)
                    {
                        chunkPrevious.m_ChunkChars[index] = newChar;
                    }
                    index++;
                }
            }
            if (num4 < 0)
            {
                chunkPrevious = chunkPrevious.m_ChunkPrevious;
                goto Label_0048;
            }
            return this;
        }

        [SecuritySafeCritical]
        public StringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            int length = this.Length;
            if (startIndex > length)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (oldValue == null)
            {
                throw new ArgumentNullException("oldValue");
            }
            if (oldValue.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "oldValue");
            }
            if (newValue == null)
            {
                newValue = "";
            }
            int num1 = newValue.Length;
            int num5 = oldValue.Length;
            int[] sourceArray = null;
            int replacementsCount = 0;
            StringBuilder chunk = this.FindChunkForIndex(startIndex);
            int indexInChunk = startIndex - chunk.m_ChunkOffset;
            while (count > 0)
            {
                if (this.StartsWith(chunk, indexInChunk, count, oldValue))
                {
                    if (sourceArray == null)
                    {
                        sourceArray = new int[5];
                    }
                    else if (replacementsCount >= sourceArray.Length)
                    {
                        int[] destinationArray = new int[((sourceArray.Length * 3) / 2) + 4];
                        Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                        sourceArray = destinationArray;
                    }
                    sourceArray[replacementsCount++] = indexInChunk;
                    indexInChunk += oldValue.Length;
                    count -= oldValue.Length;
                }
                else
                {
                    indexInChunk++;
                    count--;
                }
                if ((indexInChunk >= chunk.m_ChunkLength) || (count == 0))
                {
                    int index = indexInChunk + chunk.m_ChunkOffset;
                    this.ReplaceAllInChunk(sourceArray, replacementsCount, chunk, oldValue.Length, newValue);
                    index += (newValue.Length - oldValue.Length) * replacementsCount;
                    replacementsCount = 0;
                    chunk = this.FindChunkForIndex(index);
                    indexInChunk = index - chunk.m_ChunkOffset;
                }
            }
            return this;
        }

        [SecuritySafeCritical]
        private unsafe void ReplaceAllInChunk(int[] replacements, int replacementsCount, StringBuilder sourceChunk, int removeCount, string value)
        {
            if (replacementsCount > 0)
            {
                fixed (char* str = ((char*) value))
                {
                    char* chPtr = str;
                    int count = (value.Length - removeCount) * replacementsCount;
                    StringBuilder chunk = sourceChunk;
                    int indexInChunk = replacements[0];
                    if (count > 0)
                    {
                        this.MakeRoom(chunk.m_ChunkOffset + indexInChunk, count, out chunk, out indexInChunk, true);
                    }
                    int index = 0;
                Label_0044:
                    this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chPtr, value.Length);
                    int num4 = replacements[index] + removeCount;
                    index++;
                    if (index < replacementsCount)
                    {
                        int num5 = replacements[index];
                        if (count != 0)
                        {
                            fixed (char* chRef = &(sourceChunk.m_ChunkChars[num4]))
                            {
                                this.ReplaceInPlaceAtChunk(ref chunk, ref indexInChunk, chRef, num5 - num4);
                            }
                        }
                        else
                        {
                            indexInChunk += num5 - num4;
                        }
                        goto Label_0044;
                    }
                    if (count < 0)
                    {
                        this.Remove(chunk.m_ChunkOffset + indexInChunk, -count, out chunk, out indexInChunk);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization, SecurityCritical]
        internal extern unsafe void ReplaceBufferAnsiInternal(sbyte* newBuffer, int newLength);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization, SecurityCritical]
        internal extern unsafe void ReplaceBufferInternal(char* newBuffer, int newLength);
        [SecuritySafeCritical]
        private unsafe void ReplaceInPlaceAtChunk(ref StringBuilder chunk, ref int indexInChunk, char* value, int count)
        {
            if (count == 0)
            {
                return;
            }
            while (true)
            {
                int num = chunk.m_ChunkLength - indexInChunk;
                int num2 = Math.Min(num, count);
                ThreadSafeCopy(value, chunk.m_ChunkChars, indexInChunk, num2);
                indexInChunk += num2;
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    indexInChunk = 0;
                }
                count -= num2;
                if (count == 0)
                {
                    return;
                }
                value += num2;
            }
        }

        private bool StartsWith(StringBuilder chunk, int indexInChunk, int count, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (count == 0)
                {
                    return false;
                }
                if (indexInChunk >= chunk.m_ChunkLength)
                {
                    chunk = this.Next(chunk);
                    if (chunk == null)
                    {
                        return false;
                    }
                    indexInChunk = 0;
                }
                if (value[i] != chunk.m_ChunkChars[indexInChunk])
                {
                    return false;
                }
                indexInChunk++;
                count--;
            }
            return true;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("m_MaxCapacity", this.m_MaxCapacity);
            info.AddValue("Capacity", this.Capacity);
            info.AddValue("m_StringValue", this.ToString());
            info.AddValue("m_currentThread", 0);
        }

        [SecuritySafeCritical]
        private static unsafe void ThreadSafeCopy(char* sourcePtr, char[] destination, int destinationIndex, int count)
        {
            if (count > 0)
            {
                if ((destinationIndex > destination.Length) || ((destinationIndex + count) > destination.Length))
                {
                    throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                fixed (char* chRef = &(destination[destinationIndex]))
                {
                    string.wstrcpy(chRef, sourcePtr, count);
                }
            }
        }

        [SecuritySafeCritical]
        private static unsafe void ThreadSafeCopy(char[] source, int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (count > 0)
            {
                if ((sourceIndex > source.Length) || ((sourceIndex + count) > source.Length))
                {
                    throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                }
                fixed (char* chRef = &(source[sourceIndex]))
                {
                    ThreadSafeCopy(chRef, destination, destinationIndex, count);
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe string ToString()
        {
            string str = string.FastAllocateString(this.Length);
            StringBuilder chunkPrevious = this;
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                do
                {
                    if (chunkPrevious.m_ChunkLength > 0)
                    {
                        char[] chunkChars = chunkPrevious.m_ChunkChars;
                        int chunkOffset = chunkPrevious.m_ChunkOffset;
                        int chunkLength = chunkPrevious.m_ChunkLength;
                        if ((((ulong) (chunkLength + chunkOffset)) > str.Length) || (chunkLength > chunkChars.Length))
                        {
                            throw new ArgumentOutOfRangeException("chunkLength", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                        }
                        fixed (char* chRef = chunkChars)
                        {
                            string.wstrcpy(chPtr + chunkOffset, chRef, chunkLength);
                        }
                    }
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                }
                while (chunkPrevious != null);
            }
            return str;
        }

        [SecuritySafeCritical]
        public unsafe string ToString(int startIndex, int length)
        {
            int num = this.Length;
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (startIndex > num)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLargerThanLength"));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (startIndex > (num - length))
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
            }
            StringBuilder chunkPrevious = this;
            int num2 = startIndex + length;
            string str = string.FastAllocateString(length);
            int num3 = length;
            fixed (char* str2 = ((char*) str))
            {
                char* chPtr = str2;
                while (num3 > 0)
                {
                    int chunkLength = num2 - chunkPrevious.m_ChunkOffset;
                    if (chunkLength >= 0)
                    {
                        if (chunkLength > chunkPrevious.m_ChunkLength)
                        {
                            chunkLength = chunkPrevious.m_ChunkLength;
                        }
                        int num5 = num3;
                        int charCount = num5;
                        int index = chunkLength - num5;
                        if (index < 0)
                        {
                            charCount += index;
                            index = 0;
                        }
                        num3 -= charCount;
                        if (charCount > 0)
                        {
                            char[] chunkChars = chunkPrevious.m_ChunkChars;
                            if ((((ulong) (charCount + num3)) > length) || ((charCount + index) > chunkChars.Length))
                            {
                                throw new ArgumentOutOfRangeException("chunkCount", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                            }
                            fixed (char* chRef = &(chunkChars[index]))
                            {
                                string.wstrcpy(chPtr + num3, chRef, charCount);
                            }
                        }
                    }
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                }
            }
            return str;
        }

        [Conditional("_DEBUG")]
        private void VerifyClassInvariant()
        {
            StringBuilder builder = this;
            while (true)
            {
                StringBuilder chunkPrevious = builder.m_ChunkPrevious;
                if (chunkPrevious == null)
                {
                    return;
                }
                builder = chunkPrevious;
            }
        }

        public int Capacity
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.m_ChunkChars.Length + this.m_ChunkOffset);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NegativeCapacity"));
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_Capacity"));
                }
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
                }
                if (this.Capacity != value)
                {
                    int num = value - this.m_ChunkOffset;
                    char[] destinationArray = new char[num];
                    Array.Copy(this.m_ChunkChars, destinationArray, this.m_ChunkLength);
                    this.m_ChunkChars = destinationArray;
                }
            }
        }

        public char this[int index]
        {
            get
            {
                StringBuilder chunkPrevious = this;
                while (true)
                {
                    int num = index - chunkPrevious.m_ChunkOffset;
                    if (num >= 0)
                    {
                        if (num >= chunkPrevious.m_ChunkLength)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        return chunkPrevious.m_ChunkChars[num];
                    }
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                    if (chunkPrevious == null)
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
            }
            set
            {
                int num;
                StringBuilder chunkPrevious = this;
            Label_0002:
                num = index - chunkPrevious.m_ChunkOffset;
                if (num >= 0)
                {
                    if (num >= chunkPrevious.m_ChunkLength)
                    {
                        throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                    }
                    chunkPrevious.m_ChunkChars[num] = value;
                }
                else
                {
                    chunkPrevious = chunkPrevious.m_ChunkPrevious;
                    if (chunkPrevious == null)
                    {
                        throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_Index"));
                    }
                    goto Label_0002;
                }
            }
        }

        public int Length
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.m_ChunkOffset + this.m_ChunkLength);
            }
            [SecuritySafeCritical]
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
                }
                if (value > this.MaxCapacity)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_SmallCapacity"));
                }
                int capacity = this.Capacity;
                if ((value == 0) && (this.m_ChunkPrevious == null))
                {
                    this.m_ChunkLength = 0;
                    this.m_ChunkOffset = 0;
                }
                else
                {
                    int repeatCount = value - this.Length;
                    if (repeatCount > 0)
                    {
                        this.Append('\0', repeatCount);
                    }
                    else
                    {
                        StringBuilder builder = this.FindChunkForIndex(value);
                        if (builder != this)
                        {
                            int num3 = capacity - builder.m_ChunkOffset;
                            char[] destinationArray = new char[num3];
                            Array.Copy(builder.m_ChunkChars, destinationArray, builder.m_ChunkLength);
                            this.m_ChunkChars = destinationArray;
                            this.m_ChunkPrevious = builder.m_ChunkPrevious;
                            this.m_ChunkOffset = builder.m_ChunkOffset;
                        }
                        this.m_ChunkLength = value - builder.m_ChunkOffset;
                    }
                }
            }
        }

        public int MaxCapacity
        {
            get
            {
                return this.m_MaxCapacity;
            }
        }
    }
}

