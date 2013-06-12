namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct LockCookie
    {
        private int _dwFlags;
        private int _dwWriterSeqNum;
        private int _wReaderAndWriterLevel;
        private int _dwThreadID;
        public override int GetHashCode()
        {
            return (((this._dwFlags + this._dwWriterSeqNum) + this._wReaderAndWriterLevel) + this._dwThreadID);
        }

        public override bool Equals(object obj)
        {
            return ((obj is LockCookie) && this.Equals((LockCookie) obj));
        }

        public bool Equals(LockCookie obj)
        {
            return ((((obj._dwFlags == this._dwFlags) && (obj._dwWriterSeqNum == this._dwWriterSeqNum)) && (obj._wReaderAndWriterLevel == this._wReaderAndWriterLevel)) && (obj._dwThreadID == this._dwThreadID));
        }

        public static bool operator ==(LockCookie a, LockCookie b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LockCookie a, LockCookie b)
        {
            return !(a == b);
        }
    }
}

