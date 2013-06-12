namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;

    public class SocketAddress
    {
        internal const int IPv4AddressSize = 0x10;
        internal const int IPv6AddressSize = 0x1c;
        internal byte[] m_Buffer;
        private bool m_changed;
        private int m_hash;
        internal int m_Size;
        private const int MaxSize = 0x20;
        private const int WriteableOffset = 2;

        public SocketAddress(AddressFamily family) : this(family, 0x20)
        {
        }

        public SocketAddress(AddressFamily family, int size)
        {
            this.m_changed = true;
            if (size < 2)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            this.m_Size = size;
            this.m_Buffer = new byte[((size / IntPtr.Size) + 2) * IntPtr.Size];
            this.m_Buffer[0] = (byte) family;
            this.m_Buffer[1] = (byte) (((int) family) >> 8);
        }

        internal void CopyAddressSizeIntoBuffer()
        {
            this.m_Buffer[this.m_Buffer.Length - IntPtr.Size] = (byte) this.m_Size;
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 1] = (byte) (this.m_Size >> 8);
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 2] = (byte) (this.m_Size >> 0x10);
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 3] = (byte) (this.m_Size >> 0x18);
        }

        public override bool Equals(object comparand)
        {
            SocketAddress address = comparand as SocketAddress;
            if ((address == null) || (this.Size != address.Size))
            {
                return false;
            }
            for (int i = 0; i < this.Size; i++)
            {
                if (this[i] != address[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal int GetAddressSizeOffset()
        {
            return (this.m_Buffer.Length - IntPtr.Size);
        }

        public override int GetHashCode()
        {
            if (this.m_changed)
            {
                this.m_changed = false;
                this.m_hash = 0;
                int num2 = this.Size & -4;
                int index = 0;
                while (index < num2)
                {
                    this.m_hash ^= ((this.m_Buffer[index] | (this.m_Buffer[index + 1] << 8)) | (this.m_Buffer[index + 2] << 0x10)) | (this.m_Buffer[index + 3] << 0x18);
                    index += 4;
                }
                if ((this.Size & 3) != 0)
                {
                    int num3 = 0;
                    int num4 = 0;
                    while (index < this.Size)
                    {
                        num3 |= this.m_Buffer[index] << num4;
                        num4 += 8;
                        index++;
                    }
                    this.m_hash ^= num3;
                }
            }
            return this.m_hash;
        }

        internal unsafe void SetSize(IntPtr ptr)
        {
            this.m_Size = *((int*) ptr);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 2; i < this.Size; i++)
            {
                if (i > 2)
                {
                    builder.Append(",");
                }
                builder.Append(this[i].ToString(NumberFormatInfo.InvariantInfo));
            }
            return (this.Family.ToString() + ":" + this.Size.ToString(NumberFormatInfo.InvariantInfo) + ":{" + builder.ToString() + "}");
        }

        public AddressFamily Family
        {
            get
            {
                int num = this.m_Buffer[0] | (this.m_Buffer[1] << 8);
                return (AddressFamily) num;
            }
        }

        public byte this[int offset]
        {
            get
            {
                if ((offset < 0) || (offset >= this.Size))
                {
                    throw new IndexOutOfRangeException();
                }
                return this.m_Buffer[offset];
            }
            set
            {
                if ((offset < 0) || (offset >= this.Size))
                {
                    throw new IndexOutOfRangeException();
                }
                if (this.m_Buffer[offset] != value)
                {
                    this.m_changed = true;
                }
                this.m_Buffer[offset] = value;
            }
        }

        public int Size
        {
            get
            {
                return this.m_Size;
            }
        }
    }
}

