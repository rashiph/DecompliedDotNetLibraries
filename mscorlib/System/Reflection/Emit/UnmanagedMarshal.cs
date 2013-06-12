namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable, Obsolete("An alternate API is available: Emit the MarshalAs custom attribute instead. http://go.microsoft.com/fwlink/?linkid=14202"), ComVisible(true), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class UnmanagedMarshal
    {
        internal UnmanagedType m_baseType;
        internal Guid m_guid;
        internal int m_numElem;
        internal UnmanagedType m_unmanagedType;

        private UnmanagedMarshal(UnmanagedType unmanagedType, Guid guid, int numElem, UnmanagedType type)
        {
            this.m_unmanagedType = unmanagedType;
            this.m_guid = guid;
            this.m_numElem = numElem;
            this.m_baseType = type;
        }

        public static UnmanagedMarshal DefineByValArray(int elemCount)
        {
            return new UnmanagedMarshal(UnmanagedType.ByValArray, Guid.Empty, elemCount, (UnmanagedType) 0);
        }

        public static UnmanagedMarshal DefineByValTStr(int elemCount)
        {
            return new UnmanagedMarshal(UnmanagedType.ByValTStr, Guid.Empty, elemCount, (UnmanagedType) 0);
        }

        public static UnmanagedMarshal DefineLPArray(UnmanagedType elemType)
        {
            return new UnmanagedMarshal(UnmanagedType.LPArray, Guid.Empty, 0, elemType);
        }

        public static UnmanagedMarshal DefineSafeArray(UnmanagedType elemType)
        {
            return new UnmanagedMarshal(UnmanagedType.SafeArray, Guid.Empty, 0, elemType);
        }

        public static UnmanagedMarshal DefineUnmanagedMarshal(UnmanagedType unmanagedType)
        {
            if (((unmanagedType == UnmanagedType.ByValTStr) || (unmanagedType == UnmanagedType.SafeArray)) || (((unmanagedType == UnmanagedType.CustomMarshaler) || (unmanagedType == UnmanagedType.ByValArray)) || (unmanagedType == UnmanagedType.LPArray)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotASimpleNativeType"));
            }
            return new UnmanagedMarshal(unmanagedType, Guid.Empty, 0, (UnmanagedType) 0);
        }

        internal byte[] InternalGetBytes()
        {
            byte[] buffer;
            if ((this.m_unmanagedType == UnmanagedType.SafeArray) || (this.m_unmanagedType == UnmanagedType.LPArray))
            {
                int num = 2;
                buffer = new byte[num];
                buffer[0] = (byte) this.m_unmanagedType;
                buffer[1] = (byte) this.m_baseType;
                return buffer;
            }
            if ((this.m_unmanagedType == UnmanagedType.ByValArray) || (this.m_unmanagedType == UnmanagedType.ByValTStr))
            {
                int num2;
                int num3 = 0;
                if (this.m_numElem <= 0x7f)
                {
                    num2 = 1;
                }
                else if (this.m_numElem <= 0x3fff)
                {
                    num2 = 2;
                }
                else
                {
                    num2 = 4;
                }
                num2++;
                buffer = new byte[num2];
                buffer[num3++] = (byte) this.m_unmanagedType;
                if (this.m_numElem <= 0x7f)
                {
                    buffer[num3++] = (byte) (this.m_numElem & 0xff);
                    return buffer;
                }
                if (this.m_numElem <= 0x3fff)
                {
                    buffer[num3++] = (byte) ((this.m_numElem >> 8) | 0x80);
                    buffer[num3++] = (byte) (this.m_numElem & 0xff);
                    return buffer;
                }
                if (this.m_numElem <= 0x1fffffff)
                {
                    buffer[num3++] = (byte) ((this.m_numElem >> 0x18) | 0xc0);
                    buffer[num3++] = (byte) ((this.m_numElem >> 0x10) & 0xff);
                    buffer[num3++] = (byte) ((this.m_numElem >> 8) & 0xff);
                    buffer[num3++] = (byte) (this.m_numElem & 0xff);
                }
                return buffer;
            }
            return new byte[] { ((byte) this.m_unmanagedType) };
        }

        public UnmanagedType BaseType
        {
            get
            {
                if ((this.m_unmanagedType != UnmanagedType.LPArray) && (this.m_unmanagedType != UnmanagedType.SafeArray))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NoNestedMarshal"));
                }
                return this.m_baseType;
            }
        }

        public int ElementCount
        {
            get
            {
                if ((this.m_unmanagedType != UnmanagedType.ByValArray) && (this.m_unmanagedType != UnmanagedType.ByValTStr))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NoUnmanagedElementCount"));
                }
                return this.m_numElem;
            }
        }

        public UnmanagedType GetUnmanagedType
        {
            get
            {
                return this.m_unmanagedType;
            }
        }

        public Guid IIDGuid
        {
            get
            {
                if (this.m_unmanagedType != UnmanagedType.CustomMarshaler)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_NotACustomMarshaler"));
                }
                return this.m_guid;
            }
        }
    }
}

