namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class SqlUserDefinedTypeAttribute : Attribute
    {
        private string m_fName;
        private Microsoft.SqlServer.Server.Format m_format;
        private bool m_IsByteOrdered;
        private bool m_IsFixedLength;
        private int m_MaxByteSize;
        private string m_ValidationMethodName;
        internal const int YukonMaxByteSizeValue = 0x1f40;

        public SqlUserDefinedTypeAttribute(Microsoft.SqlServer.Server.Format format)
        {
            switch (format)
            {
                case Microsoft.SqlServer.Server.Format.Unknown:
                    throw ADP.NotSupportedUserDefinedTypeSerializationFormat(format, "format");

                case Microsoft.SqlServer.Server.Format.Native:
                case Microsoft.SqlServer.Server.Format.UserDefined:
                    this.m_format = format;
                    return;
            }
            throw ADP.InvalidUserDefinedTypeSerializationFormat(format);
        }

        public Microsoft.SqlServer.Server.Format Format
        {
            get
            {
                return this.m_format;
            }
        }

        public bool IsByteOrdered
        {
            get
            {
                return this.m_IsByteOrdered;
            }
            set
            {
                this.m_IsByteOrdered = value;
            }
        }

        public bool IsFixedLength
        {
            get
            {
                return this.m_IsFixedLength;
            }
            set
            {
                this.m_IsFixedLength = value;
            }
        }

        public int MaxByteSize
        {
            get
            {
                return this.m_MaxByteSize;
            }
            set
            {
                if (value < -1)
                {
                    throw ADP.ArgumentOutOfRange("MaxByteSize");
                }
                this.m_MaxByteSize = value;
            }
        }

        public string Name
        {
            get
            {
                return this.m_fName;
            }
            set
            {
                this.m_fName = value;
            }
        }

        public string ValidationMethodName
        {
            get
            {
                return this.m_ValidationMethodName;
            }
            set
            {
                this.m_ValidationMethodName = value;
            }
        }
    }
}

