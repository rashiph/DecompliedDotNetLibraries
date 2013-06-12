namespace System.Net.Sockets
{
    using System;
    using System.Net;

    public class SendPacketsElement
    {
        internal byte[] m_Buffer;
        internal int m_Count;
        internal string m_FilePath;
        internal UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags m_Flags;
        internal int m_Offset;

        private SendPacketsElement()
        {
        }

        public SendPacketsElement(string filepath) : this(filepath, null, 0, 0, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File)
        {
        }

        public SendPacketsElement(byte[] buffer) : this(null, buffer, 0, buffer.Length, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory)
        {
        }

        public SendPacketsElement(string filepath, int offset, int count) : this(filepath, null, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File)
        {
        }

        public SendPacketsElement(byte[] buffer, int offset, int count) : this(null, buffer, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory)
        {
        }

        public SendPacketsElement(string filepath, int offset, int count, bool endOfPacket) : this(filepath, null, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket | UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.File)
        {
        }

        public SendPacketsElement(byte[] buffer, int offset, int count, bool endOfPacket) : this(null, buffer, offset, count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket | UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.Memory)
        {
        }

        private SendPacketsElement(string filepath, byte[] buffer, int offset, int count, UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags flags)
        {
            this.m_FilePath = filepath;
            this.m_Buffer = buffer;
            this.m_Offset = offset;
            this.m_Count = count;
            this.m_Flags = flags;
        }

        public byte[] Buffer
        {
            get
            {
                return this.m_Buffer;
            }
        }

        public int Count
        {
            get
            {
                return this.m_Count;
            }
        }

        public bool EndOfPacket
        {
            get
            {
                return ((this.m_Flags & UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.EndOfPacket) != UnsafeNclNativeMethods.OSSOCK.TransmitPacketsElementFlags.None);
            }
        }

        public string FilePath
        {
            get
            {
                return this.m_FilePath;
            }
        }

        public int Offset
        {
            get
            {
                return this.m_Offset;
            }
        }
    }
}

