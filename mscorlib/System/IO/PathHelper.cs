namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Reflection;
    using System.Security;
    using System.Text;

    internal class PathHelper
    {
        private unsafe char* m_arrayPtr;
        private int m_capacity;
        private int m_length;
        private int m_maxPath;
        private StringBuilder m_sb;
        private bool useStackAlloc;

        [SecurityCritical]
        internal unsafe PathHelper(char* charArrayPtr, int length)
        {
            this.m_arrayPtr = charArrayPtr;
            this.m_capacity = length;
            this.m_maxPath = Path.MaxPath;
            this.useStackAlloc = true;
        }

        internal PathHelper(int capacity, int maxPath)
        {
            this.m_sb = new StringBuilder(capacity);
            this.m_capacity = capacity;
            this.m_maxPath = maxPath;
        }

        [SecurityCritical]
        internal unsafe void Append(char value)
        {
            if ((this.Length + 1) >= this.m_capacity)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            if (this.useStackAlloc)
            {
                this.m_arrayPtr[this.Length] = value;
                this.m_length++;
            }
            else
            {
                this.m_sb.Append(value);
            }
        }

        [SecurityCritical]
        internal unsafe void Fixup(int lenSavedName, int lastSlash)
        {
            if (this.useStackAlloc)
            {
                char* pDest = (char*) stackalloc byte[(((IntPtr) lenSavedName) * 2)];
                Buffer.memcpy(this.m_arrayPtr, lastSlash + 1, pDest, 0, lenSavedName);
                this.Length = lastSlash;
                this.NullTerminate();
                this.TryExpandShortFileName();
                this.Append(Path.DirectorySeparatorChar);
                if ((this.Length + lenSavedName) >= Path.MaxPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                Buffer.memcpy(pDest, 0, this.m_arrayPtr, this.Length, lenSavedName);
                this.Length += lenSavedName;
            }
            else
            {
                string str = this.m_sb.ToString(lastSlash + 1, lenSavedName);
                this.Length = lastSlash;
                this.TryExpandShortFileName();
                this.Append(Path.DirectorySeparatorChar);
                if ((this.Length + lenSavedName) >= this.m_maxPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                this.m_sb.Append(str);
            }
        }

        [SecurityCritical]
        internal unsafe int GetFullPathName()
        {
            if (this.useStackAlloc)
            {
                char* chPtr = (char*) stackalloc byte[(((IntPtr) (Path.MaxPath + 1)) * 2)];
                int num = Win32Native.GetFullPathName(this.m_arrayPtr, Path.MaxPath + 1, chPtr, IntPtr.Zero);
                if (num > Path.MaxPath)
                {
                    chPtr = (char*) stackalloc byte[(((IntPtr) num) * 2)];
                    num = Win32Native.GetFullPathName(this.m_arrayPtr, num, chPtr, IntPtr.Zero);
                }
                if (num >= Path.MaxPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                if ((num == 0) && (this.m_arrayPtr[0] != '\0'))
                {
                    __Error.WinIOError();
                }
                else if (num < Path.MaxPath)
                {
                    chPtr[num] = '\0';
                }
                Buffer.memcpy(chPtr, 0, this.m_arrayPtr, 0, num);
                this.Length = num;
                return num;
            }
            StringBuilder buffer = new StringBuilder(this.m_capacity + 1);
            int numBufferChars = Win32Native.GetFullPathName(this.m_sb.ToString(), this.m_capacity + 1, buffer, IntPtr.Zero);
            if (numBufferChars > this.m_maxPath)
            {
                buffer.Length = numBufferChars;
                numBufferChars = Win32Native.GetFullPathName(this.m_sb.ToString(), numBufferChars, buffer, IntPtr.Zero);
            }
            if (numBufferChars >= this.m_maxPath)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            if ((numBufferChars == 0) && (this.m_sb[0] != '\0'))
            {
                if (this.Length >= this.m_maxPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                __Error.WinIOError();
            }
            this.m_sb = buffer;
            return numBufferChars;
        }

        private StringBuilder GetStringBuilder()
        {
            return this.m_sb;
        }

        [SecurityCritical]
        private unsafe void NullTerminate()
        {
            this.m_arrayPtr[this.m_length] = '\0';
        }

        [SecurityCritical]
        internal unsafe bool OrdinalStartsWith(string compareTo, bool ignoreCase)
        {
            if (this.Length < compareTo.Length)
            {
                return false;
            }
            if (this.useStackAlloc)
            {
                this.NullTerminate();
                if (ignoreCase)
                {
                    string str = new string(this.m_arrayPtr, 0, compareTo.Length);
                    return compareTo.Equals(str, StringComparison.OrdinalIgnoreCase);
                }
                for (int i = 0; i < compareTo.Length; i++)
                {
                    if (this.m_arrayPtr[i] != compareTo[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            if (ignoreCase)
            {
                return this.m_sb.ToString().StartsWith(compareTo, StringComparison.OrdinalIgnoreCase);
            }
            return this.m_sb.ToString().StartsWith(compareTo, StringComparison.Ordinal);
        }

        [SecuritySafeCritical]
        public override unsafe string ToString()
        {
            if (this.useStackAlloc)
            {
                return new string(this.m_arrayPtr, 0, this.Length);
            }
            return this.m_sb.ToString();
        }

        [SecurityCritical]
        internal unsafe bool TryExpandShortFileName()
        {
            if (this.useStackAlloc)
            {
                this.NullTerminate();
                char* chPtr = this.UnsafeGetArrayPtr();
                char* longPathBuffer = (char*) stackalloc byte[(((IntPtr) (Path.MaxPath + 1)) * 2)];
                int len = Win32Native.GetLongPathName(chPtr, longPathBuffer, Path.MaxPath);
                if (len >= Path.MaxPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                if (len == 0)
                {
                    return false;
                }
                Buffer.memcpy(longPathBuffer, 0, chPtr, 0, len);
                this.Length = len;
                this.NullTerminate();
                return true;
            }
            StringBuilder stringBuilder = this.GetStringBuilder();
            string str = stringBuilder.ToString();
            string path = str;
            bool flag = false;
            if (path.Length > Path.MaxPath)
            {
                path = Path.AddLongPathPrefix(path);
                flag = true;
            }
            stringBuilder.Capacity = this.m_capacity;
            stringBuilder.Length = 0;
            int num2 = Win32Native.GetLongPathName(path, stringBuilder, this.m_capacity);
            if (num2 == 0)
            {
                stringBuilder.Length = 0;
                stringBuilder.Append(str);
                return false;
            }
            if (flag)
            {
                num2 -= 4;
            }
            if (num2 >= this.m_maxPath)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            stringBuilder = Path.RemoveLongPathPrefix(stringBuilder);
            this.Length = stringBuilder.Length;
            return true;
        }

        [SecurityCritical]
        private unsafe char* UnsafeGetArrayPtr()
        {
            return this.m_arrayPtr;
        }

        internal int Capacity
        {
            get
            {
                return this.m_capacity;
            }
        }

        internal char this[int index]
        {
            [SecurityCritical]
            get
            {
                if (this.useStackAlloc)
                {
                    return this.m_arrayPtr[index];
                }
                return this.m_sb[index];
            }
            [SecurityCritical]
            set
            {
                if (this.useStackAlloc)
                {
                    this.m_arrayPtr[index] = value;
                }
                else
                {
                    this.m_sb[index] = value;
                }
            }
        }

        internal int Length
        {
            get
            {
                if (this.useStackAlloc)
                {
                    return this.m_length;
                }
                return this.m_sb.Length;
            }
            set
            {
                if (this.useStackAlloc)
                {
                    this.m_length = value;
                }
                else
                {
                    this.m_sb.Length = value;
                }
            }
        }
    }
}

