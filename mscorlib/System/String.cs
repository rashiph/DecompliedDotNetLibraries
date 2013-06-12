namespace System
{
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class String : IComparable, ICloneable, IConvertible, IComparable<string>, IEnumerable<char>, IEnumerable, IEquatable<string>
    {
        private const int alignConst = 3;
        private const int charPtrAlignConst = 1;
        public static readonly string Empty = "";
        [NonSerialized, ForceTokenStabilization]
        private char m_firstChar;
        [NonSerialized]
        private int m_stringLength;
        private const int TrimBoth = 2;
        private const int TrimHead = 0;
        private const int TrimTail = 1;

        [MethodImpl(MethodImplOptions.InternalCall), CLSCompliant(false), SecurityCritical]
        public extern unsafe String(char* value);
        [MethodImpl(MethodImplOptions.InternalCall), CLSCompliant(false), SecurityCritical]
        public extern unsafe String(sbyte* value);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern String(char[] value);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern String(char c, int count);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, CLSCompliant(false)]
        public extern unsafe String(char* value, int startIndex, int length);
        [MethodImpl(MethodImplOptions.InternalCall), CLSCompliant(false), SecurityCritical]
        public extern unsafe String(sbyte* value, int startIndex, int length);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern String(char[] value, int startIndex, int length);
        [MethodImpl(MethodImplOptions.InternalCall), CLSCompliant(false), SecurityCritical]
        public extern unsafe String(sbyte* value, int startIndex, int length, Encoding enc);
        public object Clone()
        {
            return this;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Compare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Compare(string strA, string strB, bool ignoreCase)
        {
            if (ignoreCase)
            {
                return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public static int Compare(string strA, string strB, StringComparison comparisonType)
        {
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if (strA == strB)
            {
                return 0;
            }
            if (strA == null)
            {
                return -1;
            }
            if (strB == null)
            {
                return 1;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CompareOrdinalHelper(strA, strB);

                case StringComparison.OrdinalIgnoreCase:
                    if (!strA.IsAscii() || !strB.IsAscii())
                    {
                        return TextInfo.CompareOrdinalIgnoreCase(strA, strB);
                    }
                    return CompareOrdinalIgnoreCaseHelper(strA, strB);
            }
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_StringComparison"));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Compare(string strA, string strB, bool ignoreCase, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            if (ignoreCase)
            {
                return culture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase);
            }
            return culture.CompareInfo.Compare(strA, strB, CompareOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int Compare(string strA, string strB, CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            return culture.CompareInfo.Compare(strA, strB, options);
        }

        public static int Compare(string strA, int indexA, string strB, int indexB, int length)
        {
            int num = length;
            int num2 = length;
            if ((strA != null) && ((strA.Length - indexA) < num))
            {
                num = strA.Length - indexA;
            }
            if ((strB != null) && ((strB.Length - indexB) < num2))
            {
                num2 = strB.Length - indexB;
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
        }

        public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase)
        {
            int num = length;
            int num2 = length;
            if ((strA != null) && ((strA.Length - indexA) < num))
            {
                num = strA.Length - indexA;
            }
            if ((strB != null) && ((strB.Length - indexB) < num2))
            {
                num2 = strB.Length - indexB;
            }
            if (ignoreCase)
            {
                return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
        }

        [SecuritySafeCritical]
        public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
        {
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if ((strA == null) || (strB == null))
            {
                if (strA == strB)
                {
                    return 0;
                }
                if (strA != null)
                {
                    return 1;
                }
                return -1;
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (indexA < 0)
            {
                throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (indexB < 0)
            {
                throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((strA.Length - indexA) < 0)
            {
                throw new ArgumentOutOfRangeException("indexA", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((strB.Length - indexB) < 0)
            {
                throw new ArgumentOutOfRangeException("indexB", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((length == 0) || ((strA == strB) && (indexA == indexB)))
            {
                return 0;
            }
            int num = length;
            int num2 = length;
            if ((strA != null) && ((strA.Length - indexA) < num))
            {
                num = strA.Length - indexA;
            }
            if ((strB != null) && ((strB.Length - indexB) < num2))
            {
                num2 = strB.Length - indexB;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);

                case StringComparison.OrdinalIgnoreCase:
                    return TextInfo.CompareOrdinalIgnoreCaseEx(strA, indexA, strB, indexB, num, num2);
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"));
        }

        public static int Compare(string strA, int indexA, string strB, int indexB, int length, bool ignoreCase, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            int num = length;
            int num2 = length;
            if ((strA != null) && ((strA.Length - indexA) < num))
            {
                num = strA.Length - indexA;
            }
            if ((strB != null) && ((strB.Length - indexB) < num2))
            {
                num2 = strB.Length - indexB;
            }
            if (ignoreCase)
            {
                return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.IgnoreCase);
            }
            return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, CompareOptions.None);
        }

        public static int Compare(string strA, int indexA, string strB, int indexB, int length, CultureInfo culture, CompareOptions options)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            int num = length;
            int num2 = length;
            if ((strA != null) && ((strA.Length - indexA) < num))
            {
                num = strA.Length - indexA;
            }
            if ((strB != null) && ((strB.Length - indexB) < num2))
            {
                num2 = strB.Length - indexB;
            }
            return culture.CompareInfo.Compare(strA, indexA, num, strB, indexB, num2, options);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int CompareOrdinal(string strA, string strB)
        {
            if (strA == strB)
            {
                return 0;
            }
            if (strA == null)
            {
                return -1;
            }
            if (strB == null)
            {
                return 1;
            }
            return CompareOrdinalHelper(strA, strB);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
        {
            if ((strA != null) && (strB != null))
            {
                return nativeCompareOrdinalEx(strA, indexA, strB, indexB, length);
            }
            if (strA == strB)
            {
                return 0;
            }
            if (strA != null)
            {
                return 1;
            }
            return -1;
        }

        [SecuritySafeCritical]
        private static unsafe int CompareOrdinalHelper(string strA, string strB)
        {
            int num = Math.Min(strA.Length, strB.Length);
            int num2 = -1;
            fixed (char* chRef = &strA.m_firstChar)
            {
                fixed (char* chRef2 = &strB.m_firstChar)
                {
                    char* chPtr = chRef;
                    char* chPtr2 = chRef2;
                    while (num >= 10)
                    {
                        if (*(((int*) chPtr)) != *(((int*) chPtr2)))
                        {
                            num2 = 0;
                            break;
                        }
                        if (*(((int*) (chPtr + 2))) != *(((int*) (chPtr2 + 2))))
                        {
                            num2 = 2;
                            break;
                        }
                        if (*(((int*) (chPtr + 4))) != *(((int*) (chPtr2 + 4))))
                        {
                            num2 = 4;
                            break;
                        }
                        if (*(((int*) (chPtr + 6))) != *(((int*) (chPtr2 + 6))))
                        {
                            num2 = 6;
                            break;
                        }
                        if (*(((int*) (chPtr + 8))) != *(((int*) (chPtr2 + 8))))
                        {
                            num2 = 8;
                            break;
                        }
                        chPtr += 10;
                        chPtr2 += 10;
                        num -= 10;
                    }
                    if (num2 == -1)
                    {
                        goto Label_00F1;
                    }
                    chPtr += num2;
                    chPtr2 += num2;
                    int num3 = chPtr[0] - chPtr2[0];
                    if (num3 != 0)
                    {
                        return num3;
                    }
                    return (chPtr[1] - chPtr2[1]);
                Label_00D7:
                    if (*(((int*) chPtr)) != *(((int*) chPtr2)))
                    {
                        goto Label_00F5;
                    }
                    chPtr += 2;
                    chPtr2 += 2;
                    num -= 2;
                Label_00F1:
                    if (num > 0)
                    {
                        goto Label_00D7;
                    }
                Label_00F5:
                    if (num > 0)
                    {
                        int num4 = chPtr[0] - chPtr2[0];
                        if (num4 != 0)
                        {
                            return num4;
                        }
                        return (chPtr[1] - chPtr2[1]);
                    }
                    return (strA.Length - strB.Length);
                }
            }
        }

        [SecuritySafeCritical]
        private static unsafe int CompareOrdinalIgnoreCaseHelper(string strA, string strB)
        {
            int num = Math.Min(strA.Length, strB.Length);
            fixed (char* chRef = &strA.m_firstChar)
            {
                fixed (char* chRef2 = &strB.m_firstChar)
                {
                    char* chPtr = chRef;
                    char* chPtr2 = chRef2;
                    while (num != 0)
                    {
                        int num2 = chPtr[0];
                        int num3 = chPtr2[0];
                        if ((num2 - 0x61) <= 0x19)
                        {
                            num2 -= 0x20;
                        }
                        if ((num3 - 0x61) <= 0x19)
                        {
                            num3 -= 0x20;
                        }
                        if (num2 != num3)
                        {
                            return (num2 - num3);
                        }
                        chPtr++;
                        chPtr2++;
                        num--;
                    }
                    return (strA.Length - strB.Length);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int CompareTo(object value)
        {
            if (value == null)
            {
                return 1;
            }
            if (!(value is string))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeString"));
            }
            return Compare(this, (string) value, StringComparison.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int CompareTo(string strB)
        {
            if (strB == null)
            {
                return 1;
            }
            return CultureInfo.CurrentCulture.CompareInfo.Compare(this, strB, CompareOptions.None);
        }

        public static string Concat(object arg0)
        {
            if (arg0 == null)
            {
                return Empty;
            }
            return arg0.ToString();
        }

        public static string Concat(params object[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            string[] values = new string[args.Length];
            int totalLength = 0;
            for (int i = 0; i < args.Length; i++)
            {
                object obj2 = args[i];
                values[i] = (obj2 == null) ? Empty : obj2.ToString();
                if (values[i] == null)
                {
                    values[i] = Empty;
                }
                totalLength += values[i].Length;
                if (totalLength < 0)
                {
                    throw new OutOfMemoryException();
                }
            }
            return ConcatArray(values, totalLength);
        }

        [ComVisible(false)]
        public static string Concat(IEnumerable<string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            StringBuilder builder = new StringBuilder();
            using (IEnumerator<string> enumerator = values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        builder.Append(enumerator.Current);
                    }
                }
            }
            return builder.ToString();
        }

        public static string Concat(params string[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            int totalLength = 0;
            string[] strArray = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                string str = values[i];
                strArray[i] = (str == null) ? Empty : str;
                totalLength += strArray[i].Length;
                if (totalLength < 0)
                {
                    throw new OutOfMemoryException();
                }
            }
            return ConcatArray(strArray, totalLength);
        }

        [ComVisible(false)]
        public static string Concat<T>(IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            StringBuilder builder = new StringBuilder();
            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current != null)
                    {
                        string str = enumerator.Current.ToString();
                        if (str != null)
                        {
                            builder.Append(str);
                        }
                    }
                }
            }
            return builder.ToString();
        }

        public static string Concat(object arg0, object arg1)
        {
            if (arg0 == null)
            {
                arg0 = Empty;
            }
            if (arg1 == null)
            {
                arg1 = Empty;
            }
            return (arg0.ToString() + arg1.ToString());
        }

        [SecuritySafeCritical]
        public static string Concat(string str0, string str1)
        {
            if (IsNullOrEmpty(str0))
            {
                if (IsNullOrEmpty(str1))
                {
                    return Empty;
                }
                return str1;
            }
            if (IsNullOrEmpty(str1))
            {
                return str0;
            }
            int length = str0.Length;
            string dest = FastAllocateString(length + str1.Length);
            FillStringChecked(dest, 0, str0);
            FillStringChecked(dest, length, str1);
            return dest;
        }

        public static string Concat(object arg0, object arg1, object arg2)
        {
            if (arg0 == null)
            {
                arg0 = Empty;
            }
            if (arg1 == null)
            {
                arg1 = Empty;
            }
            if (arg2 == null)
            {
                arg2 = Empty;
            }
            return (arg0.ToString() + arg1.ToString() + arg2.ToString());
        }

        [SecuritySafeCritical]
        public static string Concat(string str0, string str1, string str2)
        {
            if (((str0 == null) && (str1 == null)) && (str2 == null))
            {
                return Empty;
            }
            if (str0 == null)
            {
                str0 = Empty;
            }
            if (str1 == null)
            {
                str1 = Empty;
            }
            if (str2 == null)
            {
                str2 = Empty;
            }
            int length = (str0.Length + str1.Length) + str2.Length;
            string dest = FastAllocateString(length);
            FillStringChecked(dest, 0, str0);
            FillStringChecked(dest, str0.Length, str1);
            FillStringChecked(dest, str0.Length + str1.Length, str2);
            return dest;
        }

        [CLSCompliant(false), SecuritySafeCritical]
        public static string Concat(object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            ArgIterator iterator = new ArgIterator(__arglist);
            int num = iterator.GetRemainingCount() + 4;
            object[] objArray = new object[num];
            objArray[0] = arg0;
            objArray[1] = arg1;
            objArray[2] = arg2;
            objArray[3] = arg3;
            for (int i = 4; i < num; i++)
            {
                objArray[i] = TypedReference.ToObject(iterator.GetNextArg());
            }
            return Concat(objArray);
        }

        [SecuritySafeCritical]
        public static string Concat(string str0, string str1, string str2, string str3)
        {
            if (((str0 == null) && (str1 == null)) && ((str2 == null) && (str3 == null)))
            {
                return Empty;
            }
            if (str0 == null)
            {
                str0 = Empty;
            }
            if (str1 == null)
            {
                str1 = Empty;
            }
            if (str2 == null)
            {
                str2 = Empty;
            }
            if (str3 == null)
            {
                str3 = Empty;
            }
            int length = ((str0.Length + str1.Length) + str2.Length) + str3.Length;
            string dest = FastAllocateString(length);
            FillStringChecked(dest, 0, str0);
            FillStringChecked(dest, str0.Length, str1);
            FillStringChecked(dest, str0.Length + str1.Length, str2);
            FillStringChecked(dest, (str0.Length + str1.Length) + str2.Length, str3);
            return dest;
        }

        [SecuritySafeCritical]
        private static string ConcatArray(string[] values, int totalLength)
        {
            string dest = FastAllocateString(totalLength);
            int destPos = 0;
            for (int i = 0; i < values.Length; i++)
            {
                FillStringChecked(dest, destPos, values[i]);
                destPos += values[i].Length;
            }
            return dest;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool Contains(string value)
        {
            return (this.IndexOf(value, StringComparison.Ordinal) >= 0);
        }

        [SecuritySafeCritical]
        internal unsafe byte[] ConvertToAnsi(int iMaxDBCSCharByteSize, bool fBestFit, bool fThrowOnUnmappableChar, out int cbLength)
        {
            int num;
            byte[] buffer2;
            int cbDestBuffer = (this.Length + 3) * iMaxDBCSCharByteSize;
            byte[] buffer = new byte[cbDestBuffer];
            uint flags = fBestFit ? 0 : 0x400;
            uint num4 = 0;
            if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
            {
                numRef = null;
                goto Label_003D;
            }
            fixed (byte* numRef = buffer2)
            {
            Label_003D:
                fixed (char* chRef = &this.m_firstChar)
                {
                    num = Win32Native.WideCharToMultiByte(0, flags, chRef, this.Length, numRef, cbDestBuffer, IntPtr.Zero, fThrowOnUnmappableChar ? new IntPtr((void*) &num4) : IntPtr.Zero);
                }
            }
            if (num4 != 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Interop_Marshal_Unmappable_Char"));
            }
            cbLength = num;
            buffer[num] = 0;
            return buffer;
        }

        [SecuritySafeCritical]
        public static unsafe string Copy(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            int length = str.Length;
            string str2 = FastAllocateString(length);
            fixed (char* chRef = &str2.m_firstChar)
            {
                fixed (char* chRef2 = &str.m_firstChar)
                {
                    wstrcpyPtrAligned(chRef, chRef2, length);
                }
            }
            return str2;
        }

        [SecuritySafeCritical]
        public unsafe void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
            }
            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (count > (this.Length - sourceIndex))
            {
                throw new ArgumentOutOfRangeException("sourceIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
            }
            if ((destinationIndex > (destination.Length - count)) || (destinationIndex < 0))
            {
                throw new ArgumentOutOfRangeException("destinationIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
            }
            if (count > 0)
            {
                fixed (char* chRef = &this.m_firstChar)
                {
                    fixed (char* chRef2 = destination)
                    {
                        wstrcpy(chRef2 + destinationIndex, chRef + sourceIndex, count);
                        chRef = null;
                    }
                }
            }
        }

        [SecurityCritical]
        private static unsafe string CreateString(sbyte* value, int startIndex, int length, Encoding enc)
        {
            if (enc == null)
            {
                return new string(value, startIndex, length);
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if ((value + startIndex) < value)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
            }
            byte[] dest = new byte[length];
            try
            {
                Buffer.memcpy((byte*) value, startIndex, dest, 0, length);
            }
            catch (NullReferenceException)
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
            }
            return enc.GetString(dest);
        }

        [SecurityCritical]
        internal static unsafe string CreateStringFromEncoding(byte* bytes, int byteLength, Encoding encoding)
        {
            int length = encoding.GetCharCount(bytes, byteLength, null);
            if (length == 0)
            {
                return Empty;
            }
            string str = FastAllocateString(length);
            fixed (char* chRef = &str.m_firstChar)
            {
                encoding.GetChars(bytes, byteLength, chRef, length, null);
            }
            return str;
        }

        [SecurityCritical]
        private string CreateTrimmedString(int start, int end)
        {
            int length = (end - start) + 1;
            if (length == this.Length)
            {
                return this;
            }
            if (length == 0)
            {
                return Empty;
            }
            return this.InternalSubString(start, length, false);
        }

        [SecuritySafeCritical]
        private unsafe string CtorCharArray(char[] value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return Empty;
            }
            string str = FastAllocateString(value.Length);
            fixed (char* str2 = ((char*) str))
            {
                char* dmem = str2;
                fixed (char* chRef = value)
                {
                    wstrcpyPtrAligned(dmem, chRef, value.Length);
                    str2 = null;
                }
                return str;
            }
        }

        [SecuritySafeCritical]
        private unsafe string CtorCharArrayStartLength(char[] value, int startIndex, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (startIndex > (value.Length - length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (length <= 0)
            {
                return Empty;
            }
            string str = FastAllocateString(length);
            fixed (char* str2 = ((char*) str))
            {
                char* dmem = str2;
                fixed (char* chRef = value)
                {
                    wstrcpy(dmem, chRef + startIndex, length);
                    str2 = null;
                }
                return str;
            }
        }

        [SecuritySafeCritical]
        private unsafe string CtorCharCount(char c, int count)
        {
            if (count > 0)
            {
                string str = FastAllocateString(count);
                fixed (char* str2 = ((char*) str))
                {
                    char* chPtr2 = str2;
                    while (((((uint) chPtr2) & 3) != 0) && (count > 0))
                    {
                        chPtr2++;
                        chPtr2[0] = c;
                        count--;
                    }
                    uint num = (c << 0x10) | c;
                    if (count >= 4)
                    {
                        count -= 4;
                        do
                        {
                            *((int*) chPtr2) = num;
                            *((int*) (chPtr2 + 2)) = num;
                            chPtr2 += 4;
                            count -= 4;
                        }
                        while (count >= 0);
                    }
                    if ((count & 2) != 0)
                    {
                        *((int*) chPtr2) = num;
                        chPtr2 += 2;
                    }
                    if ((count & 1) != 0)
                    {
                        chPtr2[0] = c;
                    }
                }
                return str;
            }
            if (count != 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegNum", new object[] { "count" }));
            }
            return Empty;
        }

        [SecurityCritical]
        private unsafe string CtorCharPtr(char* ptr)
        {
            string str2;
            if (ptr == null)
            {
                return Empty;
            }
            if (ptr < 0xfa00)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBeStringPtrNotAtom"));
            }
            try
            {
                int length = wcslen(ptr);
                string str = FastAllocateString(length);
                fixed (char* str3 = ((char*) str))
                {
                    char* dmem = str3;
                    wstrcpy(dmem, ptr, length);
                }
                str2 = str;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
            }
            return str2;
        }

        [ForceTokenStabilization, SecurityCritical]
        private unsafe string CtorCharPtrStartLength(char* ptr, int startIndex, int length)
        {
            string str2;
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            char* smem = ptr + startIndex;
            if (smem < ptr)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
            }
            string str = FastAllocateString(length);
            try
            {
                fixed (char* str3 = ((char*) str))
                {
                    char* dmem = str3;
                    wstrcpy(dmem, smem, length);
                }
                str2 = str;
            }
            catch (NullReferenceException)
            {
                throw new ArgumentOutOfRangeException("ptr", Environment.GetResourceString("ArgumentOutOfRange_PartialWCHAR"));
            }
            return str2;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal bool EndsWith(char value)
        {
            int length = this.Length;
            return ((length != 0) && (this[length - 1] == value));
        }

        public bool EndsWith(string value)
        {
            return this.EndsWith(value, StringComparison.CurrentCulture);
        }

        [ComVisible(false), SecuritySafeCritical]
        public bool EndsWith(string value, StringComparison comparisonType)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if (this == value)
            {
                return true;
            }
            if (value.Length == 0)
            {
                return true;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsSuffix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    if (this.Length < value.Length)
                    {
                        return false;
                    }
                    return (nativeCompareOrdinalEx(this, this.Length - value.Length, value, 0, value.Length) == 0);

                case StringComparison.OrdinalIgnoreCase:
                    if (this.Length < value.Length)
                    {
                        return false;
                    }
                    return (TextInfo.CompareOrdinalIgnoreCaseEx(this, this.Length - value.Length, value, 0, value.Length, value.Length) == 0);
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
        {
            CultureInfo currentCulture;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this == value)
            {
                return true;
            }
            if (culture == null)
            {
                currentCulture = CultureInfo.CurrentCulture;
            }
            else
            {
                currentCulture = culture;
            }
            return currentCulture.CompareInfo.IsSuffix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public override bool Equals(object obj)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            string strB = obj as string;
            if (strB == null)
            {
                return false;
            }
            return (object.ReferenceEquals(this, obj) || EqualsHelper(this, strB));
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool Equals(string value)
        {
            if (this == null)
            {
                throw new NullReferenceException();
            }
            if (value == null)
            {
                return false;
            }
            return (object.ReferenceEquals(this, value) || EqualsHelper(this, value));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool Equals(string a, string b)
        {
            return ((a == b) || (((a != null) && (b != null)) && EqualsHelper(a, b)));
        }

        [SecuritySafeCritical]
        public bool Equals(string value, StringComparison comparisonType)
        {
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if (this == value)
            {
                return true;
            }
            if (value == null)
            {
                return false;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0);

                case StringComparison.CurrentCultureIgnoreCase:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.InvariantCulture:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.None) == 0);

                case StringComparison.InvariantCultureIgnoreCase:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(this, value, CompareOptions.IgnoreCase) == 0);

                case StringComparison.Ordinal:
                    return EqualsHelper(this, value);

                case StringComparison.OrdinalIgnoreCase:
                    if (this.Length == value.Length)
                    {
                        if (this.IsAscii() && value.IsAscii())
                        {
                            return (CompareOrdinalIgnoreCaseHelper(this, value) == 0);
                        }
                        return (TextInfo.CompareOrdinalIgnoreCase(this, value) == 0);
                    }
                    return false;
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        [SecuritySafeCritical]
        public static bool Equals(string a, string b, StringComparison comparisonType)
        {
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if (a == b)
            {
                return true;
            }
            if ((a == null) || (b == null))
            {
                return false;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0);

                case StringComparison.CurrentCultureIgnoreCase:
                    return (CultureInfo.CurrentCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0);

                case StringComparison.InvariantCulture:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.None) == 0);

                case StringComparison.InvariantCultureIgnoreCase:
                    return (CultureInfo.InvariantCulture.CompareInfo.Compare(a, b, CompareOptions.IgnoreCase) == 0);

                case StringComparison.Ordinal:
                    return EqualsHelper(a, b);

                case StringComparison.OrdinalIgnoreCase:
                    if (a.Length == b.Length)
                    {
                        if (a.IsAscii() && b.IsAscii())
                        {
                            return (CompareOrdinalIgnoreCaseHelper(a, b) == 0);
                        }
                        return (TextInfo.CompareOrdinalIgnoreCase(a, b) == 0);
                    }
                    return false;
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        private static unsafe bool EqualsHelper(string strA, string strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            fixed (char* chRef = &strA.m_firstChar)
            {
                fixed (char* chRef2 = &strB.m_firstChar)
                {
                    char* chPtr = chRef;
                    char* chPtr2 = chRef2;
                    while (length >= 10)
                    {
                        if ((((*(((int*) chPtr)) != *(((int*) chPtr2))) || (*(((int*) (chPtr + 2))) != *(((int*) (chPtr2 + 2))))) || ((*(((int*) (chPtr + 4))) != *(((int*) (chPtr2 + 4)))) || (*(((int*) (chPtr + 6))) != *(((int*) (chPtr2 + 6)))))) || (*(((int*) (chPtr + 8))) != *(((int*) (chPtr2 + 8)))))
                        {
                            break;
                        }
                        chPtr += 10;
                        chPtr2 += 10;
                        length -= 10;
                    }
                    while (length > 0)
                    {
                        if (*(((int*) chPtr)) != *(((int*) chPtr2)))
                        {
                            break;
                        }
                        chPtr += 2;
                        chPtr2 += 2;
                        length -= 2;
                    }
                    return (length <= 0);
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern string FastAllocateString(int length);
        [SecuritySafeCritical]
        private static unsafe void FillStringChecked(string dest, int destPos, string src)
        {
            if (src.Length > (dest.Length - destPos))
            {
                throw new IndexOutOfRangeException();
            }
            fixed (char* chRef = &dest.m_firstChar)
            {
                fixed (char* chRef2 = &src.m_firstChar)
                {
                    wstrcpy(chRef + destPos, chRef2, src.Length);
                }
            }
        }

        public static string Format(string format, object arg0)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            return Format(null, format, new object[] { arg0 });
        }

        public static string Format(string format, params object[] args)
        {
            if ((format == null) || (args == null))
            {
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            return Format(null, format, args);
        }

        [SecuritySafeCritical]
        public static string Format(IFormatProvider provider, string format, params object[] args)
        {
            if ((format == null) || (args == null))
            {
                throw new ArgumentNullException((format == null) ? "format" : "args");
            }
            StringBuilder builder = new StringBuilder(format.Length + (args.Length * 8));
            builder.AppendFormat(provider, format, args);
            return builder.ToString();
        }

        public static string Format(string format, object arg0, object arg1)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            return Format(null, format, new object[] { arg0, arg1 });
        }

        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            return Format(null, format, new object[] { arg0, arg1, arg2 });
        }

        public CharEnumerator GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        public override unsafe int GetHashCode()
        {
            fixed (char* str = ((char*) this))
            {
                char* chPtr = str;
                int num = 0x15051505;
                int num2 = num;
                int* numPtr = (int*) chPtr;
                for (int i = this.Length; i > 0; i -= 4)
                {
                    num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }
                    num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
                    numPtr += 2;
                }
                return (num + (num2 * 0x5d588b65));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public TypeCode GetTypeCode()
        {
            return TypeCode.String;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(char value)
        {
            return this.IndexOf(value, 0, this.Length);
        }

        public int IndexOf(string value)
        {
            return this.IndexOf(value, StringComparison.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(char value, int startIndex)
        {
            return this.IndexOf(value, startIndex, this.Length - startIndex);
        }

        public int IndexOf(string value, int startIndex)
        {
            return this.IndexOf(value, startIndex, StringComparison.CurrentCulture);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(string value, StringComparison comparisonType)
        {
            return this.IndexOf(value, 0, this.Length, comparisonType);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int IndexOf(char value, int startIndex, int count);
        public int IndexOf(string value, int startIndex, int count)
        {
            if ((startIndex < 0) || (startIndex > this.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (count > (this.Length - startIndex)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            return this.IndexOf(value, startIndex, count, StringComparison.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            return this.IndexOf(value, startIndex, this.Length - startIndex, comparisonType);
        }

        public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex < 0) || (startIndex > this.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if ((count < 0) || (startIndex > (this.Length - count)))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.IndexOf(this, value, startIndex, count, CompareOptions.Ordinal);

                case StringComparison.OrdinalIgnoreCase:
                    return TextInfo.IndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOfAny(char[] anyOf)
        {
            return this.IndexOfAny(anyOf, 0, this.Length);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return this.IndexOfAny(anyOf, startIndex, this.Length - startIndex);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int IndexOfAny(char[] anyOf, int startIndex, int count);
        [SecuritySafeCritical]
        public string Insert(int startIndex, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((startIndex < 0) || (startIndex > this.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex");
            }
            return this.InsertInternal(startIndex, value);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern string InsertInternal(int startIndex, string value);
        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static string Intern(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return Thread.GetDomain().GetOrInternString(str);
        }

        [SecurityCritical, ForceTokenStabilization]
        internal static unsafe void InternalCopy(string src, IntPtr dest, int len)
        {
            if (len != 0)
            {
                fixed (char* chRef = &src.m_firstChar)
                {
                    byte* numPtr = (byte*) chRef;
                    byte* numPtr2 = (byte*) dest.ToPointer();
                    Buffer.memcpyimpl(numPtr, numPtr2, len);
                }
            }
        }

        private string[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
        {
            int startIndex = 0;
            int index = 0;
            count--;
            int num3 = (numReplaces < count) ? numReplaces : count;
            string[] strArray = new string[num3 + 1];
            for (int i = 0; (i < num3) && (startIndex < this.Length); i++)
            {
                strArray[index++] = this.Substring(startIndex, sepList[i] - startIndex);
                startIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
            }
            if ((startIndex < this.Length) && (num3 >= 0))
            {
                strArray[index] = this.Substring(startIndex);
                return strArray;
            }
            if (index == num3)
            {
                strArray[index] = Empty;
            }
            return strArray;
        }

        private string[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
        {
            int num = (numReplaces < count) ? (numReplaces + 1) : count;
            string[] strArray = new string[num];
            int startIndex = 0;
            int num3 = 0;
            for (int i = 0; (i < numReplaces) && (startIndex < this.Length); i++)
            {
                if ((sepList[i] - startIndex) > 0)
                {
                    strArray[num3++] = this.Substring(startIndex, sepList[i] - startIndex);
                }
                startIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
                if (num3 == (count - 1))
                {
                    while ((i < (numReplaces - 1)) && (startIndex == sepList[++i]))
                    {
                        startIndex += (lengthList == null) ? 1 : lengthList[i];
                    }
                    break;
                }
            }
            if (startIndex < this.Length)
            {
                strArray[num3++] = this.Substring(startIndex);
            }
            string[] strArray2 = strArray;
            if (num3 != num)
            {
                strArray2 = new string[num3];
                for (int j = 0; j < num3; j++)
                {
                    strArray2[j] = strArray[j];
                }
            }
            return strArray2;
        }

        [SecurityCritical]
        private unsafe string InternalSubString(int startIndex, int length, bool fAlwaysCopy)
        {
            if (((startIndex == 0) && (length == this.Length)) && !fAlwaysCopy)
            {
                return this;
            }
            string str = FastAllocateString(length);
            fixed (char* chRef = &str.m_firstChar)
            {
                fixed (char* chRef2 = &this.m_firstChar)
                {
                    wstrcpy(chRef, chRef2 + startIndex, length);
                }
            }
            return str;
        }

        [SecurityCritical]
        internal string InternalSubStringWithChecks(int startIndex, int length, bool fAlwaysCopy)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (startIndex > this.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLargerThanLength"));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_NegativeLength"));
            }
            if (startIndex > (this.Length - length))
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_IndexLength"));
            }
            if (length == 0)
            {
                return Empty;
            }
            return this.InternalSubString(startIndex, length, fAlwaysCopy);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern bool IsAscii();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern bool IsFastSort();
        [SecuritySafeCritical]
        public static string IsInterned(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            return Thread.GetDomain().IsStringInterned(str);
        }

        public bool IsNormalized()
        {
            return this.IsNormalized(NormalizationForm.FormC);
        }

        [SecuritySafeCritical]
        public bool IsNormalized(NormalizationForm normalizationForm)
        {
            if (!this.IsFastSort() || (((normalizationForm != NormalizationForm.FormC) && (normalizationForm != NormalizationForm.FormKC)) && ((normalizationForm != NormalizationForm.FormD) && (normalizationForm != NormalizationForm.FormKD))))
            {
                return Normalization.IsNormalized(this, normalizationForm);
            }
            return true;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool IsNullOrEmpty(string value)
        {
            if (value != null)
            {
                return (value.Length == 0);
            }
            return true;
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [ComVisible(false)]
        public static string Join(string separator, params object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if ((values.Length == 0) || (values[0] == null))
            {
                return Empty;
            }
            if (separator == null)
            {
                separator = Empty;
            }
            StringBuilder builder = new StringBuilder();
            string str = values[0].ToString();
            if (str != null)
            {
                builder.Append(str);
            }
            for (int i = 1; i < values.Length; i++)
            {
                builder.Append(separator);
                if (values[i] != null)
                {
                    str = values[i].ToString();
                    if (str != null)
                    {
                        builder.Append(str);
                    }
                }
            }
            return builder.ToString();
        }

        public static string Join(string separator, params string[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return Join(separator, value, 0, value.Length);
        }

        [ComVisible(false)]
        public static string Join(string separator, IEnumerable<string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (separator == null)
            {
                separator = Empty;
            }
            using (IEnumerator<string> enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return Empty;
                }
                StringBuilder builder = new StringBuilder();
                if (enumerator.Current != null)
                {
                    builder.Append(enumerator.Current);
                }
                while (enumerator.MoveNext())
                {
                    builder.Append(separator);
                    if (enumerator.Current != null)
                    {
                        builder.Append(enumerator.Current);
                    }
                }
                return builder.ToString();
            }
        }

        [ComVisible(false)]
        public static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (separator == null)
            {
                separator = Empty;
            }
            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return Empty;
                }
                StringBuilder builder = new StringBuilder();
                if (enumerator.Current != null)
                {
                    string str = enumerator.Current.ToString();
                    if (str != null)
                    {
                        builder.Append(str);
                    }
                }
                while (enumerator.MoveNext())
                {
                    builder.Append(separator);
                    if (enumerator.Current != null)
                    {
                        string str2 = enumerator.Current.ToString();
                        if (str2 != null)
                        {
                            builder.Append(str2);
                        }
                    }
                }
                return builder.ToString();
            }
        }

        [SecuritySafeCritical]
        public static unsafe string Join(string separator, string[] value, int startIndex, int count)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
            }
            if (startIndex > (value.Length - count))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (separator == null)
            {
                separator = Empty;
            }
            if (count == 0)
            {
                return Empty;
            }
            int length = 0;
            int num2 = (startIndex + count) - 1;
            for (int i = startIndex; i <= num2; i++)
            {
                if (value[i] != null)
                {
                    length += value[i].Length;
                }
            }
            length += (count - 1) * separator.Length;
            if ((length < 0) || ((length + 1) < 0))
            {
                throw new OutOfMemoryException();
            }
            if (length == 0)
            {
                return Empty;
            }
            string str = FastAllocateString(length);
            fixed (char* chRef = &str.m_firstChar)
            {
                UnSafeCharBuffer buffer = new UnSafeCharBuffer(chRef, length);
                buffer.AppendString(value[startIndex]);
                for (int j = startIndex + 1; j <= num2; j++)
                {
                    buffer.AppendString(separator);
                    buffer.AppendString(value[j]);
                }
            }
            return str;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public int LastIndexOf(char value)
        {
            return this.LastIndexOf(value, this.Length - 1, this.Length);
        }

        [SecuritySafeCritical]
        public int LastIndexOf(string value)
        {
            return this.LastIndexOf(value, this.Length - 1, this.Length, StringComparison.CurrentCulture);
        }

        public int LastIndexOf(char value, int startIndex)
        {
            return this.LastIndexOf(value, startIndex, startIndex + 1);
        }

        public int LastIndexOf(string value, int startIndex)
        {
            return this.LastIndexOf(value, startIndex, startIndex + 1, StringComparison.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), SecuritySafeCritical]
        public int LastIndexOf(string value, StringComparison comparisonType)
        {
            return this.LastIndexOf(value, this.Length - 1, this.Length, comparisonType);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int LastIndexOf(char value, int startIndex, int count);
        public int LastIndexOf(string value, int startIndex, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            return this.LastIndexOf(value, startIndex, count, StringComparison.CurrentCulture);
        }

        public int LastIndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            return this.LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
        }

        public int LastIndexOf(string value, int startIndex, int count, StringComparison comparisonType)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((this.Length == 0) && ((startIndex == -1) || (startIndex == 0)))
            {
                if (value.Length != 0)
                {
                    return -1;
                }
                return 0;
            }
            if ((startIndex < 0) || (startIndex > this.Length))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (startIndex == this.Length)
            {
                startIndex--;
                if (count > 0)
                {
                    count--;
                }
                if (((value.Length == 0) && (count >= 0)) && (((startIndex - count) + 1) >= 0))
                {
                    return startIndex;
                }
            }
            if ((count < 0) || (((startIndex - count) + 1) < 0))
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_Count"));
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(this, value, startIndex, count, CompareOptions.Ordinal);

                case StringComparison.OrdinalIgnoreCase:
                    return TextInfo.LastIndexOfStringOrdinalIgnoreCase(this, value, startIndex, count);
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        public int LastIndexOfAny(char[] anyOf)
        {
            return this.LastIndexOfAny(anyOf, this.Length - 1, this.Length);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            return this.LastIndexOfAny(anyOf, startIndex, startIndex + 1);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public extern int LastIndexOfAny(char[] anyOf, int startIndex, int count);
        [SecuritySafeCritical]
        private unsafe int MakeSeparatorList(char[] separator, ref int[] sepList)
        {
            int num = 0;
            if ((separator == null) || (separator.Length == 0))
            {
                fixed (char* chRef = &this.m_firstChar)
                {
                    for (int i = 0; (i < this.Length) && (num < sepList.Length); i++)
                    {
                        if (char.IsWhiteSpace(chRef[i]))
                        {
                            sepList[num++] = i;
                        }
                    }
                }
                return num;
            }
            int length = sepList.Length;
            int num4 = separator.Length;
            fixed (char* chRef2 = &this.m_firstChar)
            {
                fixed (char* chRef3 = separator)
                {
                    for (int j = 0; (j < this.Length) && (num < length); j++)
                    {
                        char* chPtr = chRef3;
                        int num6 = 0;
                        while (num6 < num4)
                        {
                            if (chRef2[j] == chPtr[0])
                            {
                                sepList[num++] = j;
                                break;
                            }
                            num6++;
                            chPtr++;
                        }
                    }
                    chRef2 = null;
                }
                return num;
            }
        }

        [SecuritySafeCritical]
        private unsafe int MakeSeparatorList(string[] separators, ref int[] sepList, ref int[] lengthList)
        {
            int index = 0;
            int length = sepList.Length;
            fixed (char* chRef = &this.m_firstChar)
            {
                for (int i = 0; (i < this.Length) && (index < length); i++)
                {
                    for (int j = 0; j < separators.Length; j++)
                    {
                        string str = separators[j];
                        if (!IsNullOrEmpty(str))
                        {
                            int num5 = str.Length;
                            if (((chRef[i] == str[0]) && (num5 <= (this.Length - i))) && ((num5 == 1) || (CompareOrdinal(this, i, str, 0, num5) == 0)))
                            {
                                sepList[index] = i;
                                lengthList[index] = num5;
                                index++;
                                i += num5 - 1;
                                break;
                            }
                        }
                    }
                }
            }
            return index;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int nativeCompareOrdinalEx(string strA, int indexA, string strB, int indexB, int count);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern unsafe int nativeCompareOrdinalIgnoreCaseWC(string strA, char* strBChars);
        public string Normalize()
        {
            return this.Normalize(NormalizationForm.FormC);
        }

        [SecuritySafeCritical]
        public string Normalize(NormalizationForm normalizationForm)
        {
            if (!this.IsAscii() || (((normalizationForm != NormalizationForm.FormC) && (normalizationForm != NormalizationForm.FormKC)) && ((normalizationForm != NormalizationForm.FormD) && (normalizationForm != NormalizationForm.FormKD))))
            {
                return Normalization.Normalize(this, normalizationForm);
            }
            return this;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(string a, string b)
        {
            return Equals(a, b);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(string a, string b)
        {
            return !Equals(a, b);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern string PadHelper(int totalWidth, char paddingChar, bool isRightPadded);
        public string PadLeft(int totalWidth)
        {
            return this.PadHelper(totalWidth, ' ', false);
        }

        public string PadLeft(int totalWidth, char paddingChar)
        {
            return this.PadHelper(totalWidth, paddingChar, false);
        }

        public string PadRight(int totalWidth)
        {
            return this.PadHelper(totalWidth, ' ', true);
        }

        public string PadRight(int totalWidth, char paddingChar)
        {
            return this.PadHelper(totalWidth, paddingChar, true);
        }

        public string Remove(int startIndex)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            if (startIndex >= this.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndexLessThanLength"));
            }
            return this.Substring(0, startIndex);
        }

        [SecuritySafeCritical]
        public string Remove(int startIndex, int count)
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            return this.RemoveInternal(startIndex, count);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern string RemoveInternal(int startIndex, int count);
        [SecuritySafeCritical]
        public string Replace(char oldChar, char newChar)
        {
            return this.ReplaceInternal(oldChar, newChar);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string Replace(string oldValue, string newValue)
        {
            if (oldValue == null)
            {
                throw new ArgumentNullException("oldValue");
            }
            return this.ReplaceInternal(oldValue, newValue);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern string ReplaceInternal(char oldChar, char newChar);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private extern string ReplaceInternal(string oldValue, string newValue);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern void SetTrailByte(byte data);
        [SecuritySafeCritical]
        internal static unsafe string SmallCharToUpper(string strIn)
        {
            int length = strIn.Length;
            string str = FastAllocateString(length);
            fixed (char* chRef = &strIn.m_firstChar)
            {
                fixed (char* chRef2 = &str.m_firstChar)
                {
                    for (int i = 0; i < length; i++)
                    {
                        int num3 = chRef[i];
                        if ((num3 - 0x61) <= 0x19)
                        {
                            num3 -= 0x20;
                        }
                        chRef2[i] = (char) num3;
                    }
                }
            }
            return str;
        }

        public string[] Split(params char[] separator)
        {
            return this.SplitInternal(separator, 0x7fffffff, StringSplitOptions.None);
        }

        public string[] Split(char[] separator, int count)
        {
            return this.SplitInternal(separator, count, StringSplitOptions.None);
        }

        [ComVisible(false)]
        public string[] Split(char[] separator, StringSplitOptions options)
        {
            return this.SplitInternal(separator, 0x7fffffff, options);
        }

        [ComVisible(false)]
        public string[] Split(string[] separator, StringSplitOptions options)
        {
            return this.Split(separator, 0x7fffffff, options);
        }

        [ComVisible(false)]
        public string[] Split(char[] separator, int count, StringSplitOptions options)
        {
            return this.SplitInternal(separator, count, options);
        }

        [ComVisible(false)]
        public string[] Split(string[] separator, int count, StringSplitOptions options)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
            }
            if ((options < StringSplitOptions.None) || (options > StringSplitOptions.RemoveEmptyEntries))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { (int) options }));
            }
            bool flag = options == StringSplitOptions.RemoveEmptyEntries;
            if ((separator == null) || (separator.Length == 0))
            {
                return this.SplitInternal(null, count, options);
            }
            if ((count == 0) || (flag && (this.Length == 0)))
            {
                return new string[0];
            }
            int[] sepList = new int[this.Length];
            int[] lengthList = new int[this.Length];
            int numReplaces = this.MakeSeparatorList(separator, ref sepList, ref lengthList);
            if ((numReplaces == 0) || (count == 1))
            {
                return new string[] { this };
            }
            if (flag)
            {
                return this.InternalSplitOmitEmptyEntries(sepList, lengthList, numReplaces, count);
            }
            return this.InternalSplitKeepEmptyEntries(sepList, lengthList, numReplaces, count);
        }

        [ComVisible(false)]
        internal string[] SplitInternal(char[] separator, int count, StringSplitOptions options)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NegativeCount"));
            }
            if ((options < StringSplitOptions.None) || (options > StringSplitOptions.RemoveEmptyEntries))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EnumIllegalVal", new object[] { options }));
            }
            bool flag = options == StringSplitOptions.RemoveEmptyEntries;
            if ((count == 0) || (flag && (this.Length == 0)))
            {
                return new string[0];
            }
            int[] sepList = new int[this.Length];
            int numReplaces = this.MakeSeparatorList(separator, ref sepList);
            if ((numReplaces == 0) || (count == 1))
            {
                return new string[] { this };
            }
            if (flag)
            {
                return this.InternalSplitOmitEmptyEntries(sepList, null, numReplaces, count);
            }
            return this.InternalSplitKeepEmptyEntries(sepList, null, numReplaces, count);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public bool StartsWith(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.StartsWith(value, StringComparison.CurrentCulture);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public bool StartsWith(string value, StringComparison comparisonType)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
            }
            if (this == value)
            {
                return true;
            }
            if (value.Length == 0)
            {
                return true;
            }
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);

                case StringComparison.CurrentCultureIgnoreCase:
                    return CultureInfo.CurrentCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.InvariantCulture:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.None);

                case StringComparison.InvariantCultureIgnoreCase:
                    return CultureInfo.InvariantCulture.CompareInfo.IsPrefix(this, value, CompareOptions.IgnoreCase);

                case StringComparison.Ordinal:
                    return ((this.Length >= value.Length) && (nativeCompareOrdinalEx(this, 0, value, 0, value.Length) == 0));

                case StringComparison.OrdinalIgnoreCase:
                    return ((this.Length >= value.Length) && (TextInfo.CompareOrdinalIgnoreCaseEx(this, 0, value, 0, value.Length, value.Length) == 0));
            }
            throw new ArgumentException(Environment.GetResourceString("NotSupported_StringComparison"), "comparisonType");
        }

        public bool StartsWith(string value, bool ignoreCase, CultureInfo culture)
        {
            CultureInfo currentCulture;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this == value)
            {
                return true;
            }
            if (culture == null)
            {
                currentCulture = CultureInfo.CurrentCulture;
            }
            else
            {
                currentCulture = culture;
            }
            return currentCulture.CompareInfo.IsPrefix(this, value, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string Substring(int startIndex)
        {
            return this.Substring(startIndex, this.Length - startIndex);
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string Substring(int startIndex, int length)
        {
            return this.InternalSubStringWithChecks(startIndex, length, false);
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CharEnumerator(this);
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this, provider);
        }

        [SecuritySafeCritical]
        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this, provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this, provider);
        }

        [SecuritySafeCritical]
        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(this, provider);
        }

        [SecuritySafeCritical]
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(this, provider);
        }

        [SecuritySafeCritical]
        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this, provider);
        }

        [SecuritySafeCritical]
        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this, provider);
        }

        [SecuritySafeCritical]
        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this, provider);
        }

        [SecuritySafeCritical]
        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this, provider);
        }

        [SecuritySafeCritical]
        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this, provider);
        }

        [SecuritySafeCritical]
        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this, provider);
        }

        object IConvertible.ToType(Type type, IFormatProvider provider)
        {
            return Convert.DefaultToType(this, type, provider);
        }

        [SecuritySafeCritical]
        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this, provider);
        }

        [SecuritySafeCritical]
        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this, provider);
        }

        [SecuritySafeCritical]
        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this, provider);
        }

        [SecuritySafeCritical]
        public unsafe char[] ToCharArray()
        {
            int length = this.Length;
            char[] chArray = new char[length];
            if (length <= 0)
            {
                return chArray;
            }
            fixed (char* chRef = &this.m_firstChar)
            {
                fixed (char* chRef2 = chArray)
                {
                    wstrcpyPtrAligned(chRef2, chRef, length);
                    chRef = null;
                }
                return chArray;
            }
        }

        [SecuritySafeCritical]
        public unsafe char[] ToCharArray(int startIndex, int length)
        {
            if (((startIndex < 0) || (startIndex > this.Length)) || (startIndex > (this.Length - length)))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            char[] chArray = new char[length];
            if (length <= 0)
            {
                return chArray;
            }
            fixed (char* chRef = &this.m_firstChar)
            {
                fixed (char* chRef2 = chArray)
                {
                    wstrcpy(chRef2, chRef + startIndex, length);
                    chRef = null;
                }
                return chArray;
            }
        }

        public string ToLower()
        {
            return this.ToLower(CultureInfo.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToLower(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            return culture.TextInfo.ToLower(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToLowerInvariant()
        {
            return this.ToLower(CultureInfo.InvariantCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override string ToString()
        {
            return this;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToString(IFormatProvider provider)
        {
            return this;
        }

        public string ToUpper()
        {
            return this.ToUpper(CultureInfo.CurrentCulture);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToUpper(CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            return culture.TextInfo.ToUpper(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string ToUpperInvariant()
        {
            return this.ToUpper(CultureInfo.InvariantCulture);
        }

        public string Trim()
        {
            return this.TrimHelper(2);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string Trim(params char[] trimChars)
        {
            if ((trimChars != null) && (trimChars.Length != 0))
            {
                return this.TrimHelper(trimChars, 2);
            }
            return this.TrimHelper(2);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string TrimEnd(params char[] trimChars)
        {
            if ((trimChars != null) && (trimChars.Length != 0))
            {
                return this.TrimHelper(trimChars, 1);
            }
            return this.TrimHelper(1);
        }

        [SecuritySafeCritical]
        private string TrimHelper(int trimType)
        {
            int end = this.Length - 1;
            int start = 0;
            if (trimType != 1)
            {
                start = 0;
                while (start < this.Length)
                {
                    if (!char.IsWhiteSpace(this[start]))
                    {
                        break;
                    }
                    start++;
                }
            }
            if (trimType != 0)
            {
                end = this.Length - 1;
                while (end >= start)
                {
                    if (!char.IsWhiteSpace(this[end]))
                    {
                        break;
                    }
                    end--;
                }
            }
            return this.CreateTrimmedString(start, end);
        }

        [SecuritySafeCritical]
        private string TrimHelper(char[] trimChars, int trimType)
        {
            int end = this.Length - 1;
            int start = 0;
            if (trimType != 1)
            {
                start = 0;
                while (start < this.Length)
                {
                    int index = 0;
                    char ch = this[start];
                    index = 0;
                    while (index < trimChars.Length)
                    {
                        if (trimChars[index] == ch)
                        {
                            break;
                        }
                        index++;
                    }
                    if (index == trimChars.Length)
                    {
                        break;
                    }
                    start++;
                }
            }
            if (trimType != 0)
            {
                end = this.Length - 1;
                while (end >= start)
                {
                    int num4 = 0;
                    char ch2 = this[end];
                    num4 = 0;
                    while (num4 < trimChars.Length)
                    {
                        if (trimChars[num4] == ch2)
                        {
                            break;
                        }
                        num4++;
                    }
                    if (num4 == trimChars.Length)
                    {
                        break;
                    }
                    end--;
                }
            }
            return this.CreateTrimmedString(start, end);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public string TrimStart(params char[] trimChars)
        {
            if ((trimChars != null) && (trimChars.Length != 0))
            {
                return this.TrimHelper(trimChars, 0);
            }
            return this.TrimHelper(0);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal extern bool TryGetTrailByte(out byte data);
        [ForceTokenStabilization, SecurityCritical]
        private static unsafe int wcslen(char* ptr)
        {
            char* chPtr = ptr;
            while (((((uint) chPtr) & 3) != 0) && (chPtr[0] != '\0'))
            {
                chPtr++;
            }
            if (chPtr[0] != '\0')
            {
                while (((chPtr[0] & chPtr[1]) != 0) || ((chPtr[0] != '\0') && (chPtr[1] != '\0')))
                {
                    chPtr += 2;
                }
            }
            while (chPtr[0] != '\0')
            {
                chPtr++;
            }
            return (int) ((long) ((chPtr - ptr) / 2));
        }

        [SecurityCritical]
        internal static unsafe void wstrcpy(char* dmem, char* smem, int charCount)
        {
            if (charCount > 0)
            {
                if ((((int) dmem) & 2) != 0)
                {
                    dmem[0] = smem[0];
                    dmem++;
                    smem++;
                    charCount--;
                }
                while (charCount >= 8)
                {
                    *((int*) dmem) = *((uint*) smem);
                    *((int*) (dmem + 2)) = *((uint*) (smem + 2));
                    *((int*) (dmem + 4)) = *((uint*) (smem + 4));
                    *((int*) (dmem + 6)) = *((uint*) (smem + 6));
                    dmem += 8;
                    smem += 8;
                    charCount -= 8;
                }
                if ((charCount & 4) != 0)
                {
                    *((int*) dmem) = *((uint*) smem);
                    *((int*) (dmem + 2)) = *((uint*) (smem + 2));
                    dmem += 4;
                    smem += 4;
                }
                if ((charCount & 2) != 0)
                {
                    *((int*) dmem) = *((uint*) smem);
                    dmem += 2;
                    smem += 2;
                }
                if ((charCount & 1) != 0)
                {
                    dmem[0] = smem[0];
                }
            }
        }

        [SecurityCritical]
        private static unsafe void wstrcpyPtrAligned(char* dmem, char* smem, int charCount)
        {
            while (charCount >= 8)
            {
                *((int*) dmem) = *((uint*) smem);
                *((int*) (dmem + 2)) = *((uint*) (smem + 2));
                *((int*) (dmem + 4)) = *((uint*) (smem + 4));
                *((int*) (dmem + 6)) = *((uint*) (smem + 6));
                dmem += 8;
                smem += 8;
                charCount -= 8;
            }
            if ((charCount & 4) != 0)
            {
                *((int*) dmem) = *((uint*) smem);
                *((int*) (dmem + 2)) = *((uint*) (smem + 2));
                dmem += 4;
                smem += 4;
            }
            if ((charCount & 2) != 0)
            {
                *((int*) dmem) = *((uint*) smem);
                dmem += 2;
                smem += 2;
            }
            if ((charCount & 1) != 0)
            {
                dmem[0] = smem[0];
            }
        }

        public char this[int index] { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] get; }

        internal char FirstChar
        {
            get
            {
                return this.m_firstChar;
            }
        }

        public int Length { [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical] get; }
    }
}

