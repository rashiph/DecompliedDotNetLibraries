namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public struct CngProperty : IEquatable<CngProperty>
    {
        private string m_name;
        private CngPropertyOptions m_propertyOptions;
        private byte[] m_value;
        private int? m_hashCode;
        public CngProperty(string name, byte[] value, CngPropertyOptions options)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.m_name = name;
            this.m_propertyOptions = options;
            this.m_hashCode = null;
            if (value != null)
            {
                this.m_value = value.Clone() as byte[];
            }
            else
            {
                this.m_value = null;
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
        }
        public CngPropertyOptions Options
        {
            get
            {
                return this.m_propertyOptions;
            }
        }
        internal byte[] Value
        {
            get
            {
                return this.m_value;
            }
        }
        public byte[] GetValue()
        {
            byte[] buffer = null;
            if (this.m_value != null)
            {
                buffer = this.m_value.Clone() as byte[];
            }
            return buffer;
        }

        public static bool operator ==(CngProperty left, CngProperty right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CngProperty left, CngProperty right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return (((obj != null) && (obj is CngProperty)) && this.Equals((CngProperty) obj));
        }

        public bool Equals(CngProperty other)
        {
            if (!string.Equals(this.Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }
            if (this.Options != other.Options)
            {
                return false;
            }
            if (this.m_value == null)
            {
                return (other.m_value == null);
            }
            if (other.m_value == null)
            {
                return false;
            }
            if (this.m_value.Length != other.m_value.Length)
            {
                return false;
            }
            for (int i = 0; i < this.m_value.Length; i++)
            {
                if (this.m_value[i] != other.m_value[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (!this.m_hashCode.HasValue)
            {
                int num = this.Name.GetHashCode() ^ this.Options.GetHashCode();
                if (this.m_value != null)
                {
                    for (int i = 0; i < this.m_value.Length; i++)
                    {
                        int num3 = this.m_value[i] << ((i % 4) * 8);
                        num ^= num3;
                    }
                }
                this.m_hashCode = new int?(num);
            }
            return this.m_hashCode.Value;
        }
    }
}

