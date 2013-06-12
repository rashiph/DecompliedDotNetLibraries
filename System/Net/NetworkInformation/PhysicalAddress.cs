namespace System.Net.NetworkInformation
{
    using System;
    using System.Text;

    public class PhysicalAddress
    {
        private byte[] address;
        private bool changed = true;
        private int hash;
        public static readonly PhysicalAddress None = new PhysicalAddress(new byte[0]);

        public PhysicalAddress(byte[] address)
        {
            this.address = address;
        }

        public override bool Equals(object comparand)
        {
            PhysicalAddress address = comparand as PhysicalAddress;
            if (address == null)
            {
                return false;
            }
            if (this.address.Length != address.address.Length)
            {
                return false;
            }
            for (int i = 0; i < address.address.Length; i++)
            {
                if (this.address[i] != address.address[i])
                {
                    return false;
                }
            }
            return true;
        }

        public byte[] GetAddressBytes()
        {
            byte[] dst = new byte[this.address.Length];
            Buffer.BlockCopy(this.address, 0, dst, 0, this.address.Length);
            return dst;
        }

        public override int GetHashCode()
        {
            if (this.changed)
            {
                this.changed = false;
                this.hash = 0;
                int num2 = this.address.Length & -4;
                int index = 0;
                while (index < num2)
                {
                    this.hash ^= ((this.address[index] | (this.address[index + 1] << 8)) | (this.address[index + 2] << 0x10)) | (this.address[index + 3] << 0x18);
                    index += 4;
                }
                if ((this.address.Length & 3) != 0)
                {
                    int num3 = 0;
                    int num4 = 0;
                    while (index < this.address.Length)
                    {
                        num3 |= this.address[index] << num4;
                        num4 += 8;
                        index++;
                    }
                    this.hash ^= num3;
                }
            }
            return this.hash;
        }

        public static PhysicalAddress Parse(string address)
        {
            int num = 0;
            bool flag = false;
            byte[] buffer = null;
            if (address == null)
            {
                return None;
            }
            if (address.IndexOf('-') >= 0)
            {
                flag = true;
                buffer = new byte[(address.Length + 1) / 3];
            }
            else
            {
                if ((address.Length % 2) > 0)
                {
                    throw new FormatException(SR.GetString("net_bad_mac_address"));
                }
                buffer = new byte[address.Length / 2];
            }
            int index = 0;
            for (int i = 0; i < address.Length; i++)
            {
                int num4 = address[i];
                if ((num4 >= 0x30) && (num4 <= 0x39))
                {
                    num4 -= 0x30;
                }
                else if ((num4 >= 0x41) && (num4 <= 70))
                {
                    num4 -= 0x37;
                }
                else
                {
                    if (num4 != 0x2d)
                    {
                        throw new FormatException(SR.GetString("net_bad_mac_address"));
                    }
                    if (num != 2)
                    {
                        throw new FormatException(SR.GetString("net_bad_mac_address"));
                    }
                    num = 0;
                    continue;
                }
                if (flag && (num >= 2))
                {
                    throw new FormatException(SR.GetString("net_bad_mac_address"));
                }
                if ((num % 2) == 0)
                {
                    buffer[index] = (byte) (num4 << 4);
                }
                else
                {
                    buffer[index++] = (byte) (buffer[index++] | ((byte) num4));
                }
                num++;
            }
            if (num < 2)
            {
                throw new FormatException(SR.GetString("net_bad_mac_address"));
            }
            return new PhysicalAddress(buffer);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num in this.address)
            {
                int num2 = (num >> 4) & 15;
                for (int i = 0; i < 2; i++)
                {
                    if (num2 < 10)
                    {
                        builder.Append((char) (num2 + 0x30));
                    }
                    else
                    {
                        builder.Append((char) (num2 + 0x37));
                    }
                    num2 = num & 15;
                }
            }
            return builder.ToString();
        }
    }
}

