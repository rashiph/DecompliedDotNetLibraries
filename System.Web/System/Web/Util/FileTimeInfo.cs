namespace System.Web.Util
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FileTimeInfo
    {
        internal long LastWriteTime;
        internal long Size;
        internal static readonly FileTimeInfo MinValue;
        internal FileTimeInfo(long lastWriteTime, long size)
        {
            this.LastWriteTime = lastWriteTime;
            this.Size = size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FileTimeInfo))
            {
                return false;
            }
            FileTimeInfo info = (FileTimeInfo) obj;
            return ((this.LastWriteTime == info.LastWriteTime) && (this.Size == info.Size));
        }

        public static bool operator ==(FileTimeInfo value1, FileTimeInfo value2)
        {
            return ((value1.LastWriteTime == value2.LastWriteTime) && (value1.Size == value2.Size));
        }

        public static bool operator !=(FileTimeInfo value1, FileTimeInfo value2)
        {
            return !(value1 == value2);
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.CombineHashCodes(this.LastWriteTime.GetHashCode(), this.Size.GetHashCode());
        }

        static FileTimeInfo()
        {
            MinValue = new FileTimeInfo(0L, 0L);
        }
    }
}

