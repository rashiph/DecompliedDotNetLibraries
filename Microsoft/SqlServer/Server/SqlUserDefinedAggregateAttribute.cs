namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data;
    using System.Data.Common;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class SqlUserDefinedAggregateAttribute : Attribute
    {
        private bool m_fInvariantToDup;
        private bool m_fInvariantToNulls;
        private bool m_fInvariantToOrder = true;
        private string m_fName;
        private bool m_fNullIfEmpty;
        private Microsoft.SqlServer.Server.Format m_format;
        private int m_MaxByteSize;
        public const int MaxByteSizeValue = 0x1f40;

        public SqlUserDefinedAggregateAttribute(Microsoft.SqlServer.Server.Format format)
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

        public bool IsInvariantToDuplicates
        {
            get
            {
                return this.m_fInvariantToDup;
            }
            set
            {
                this.m_fInvariantToDup = value;
            }
        }

        public bool IsInvariantToNulls
        {
            get
            {
                return this.m_fInvariantToNulls;
            }
            set
            {
                this.m_fInvariantToNulls = value;
            }
        }

        public bool IsInvariantToOrder
        {
            get
            {
                return this.m_fInvariantToOrder;
            }
            set
            {
                this.m_fInvariantToOrder = value;
            }
        }

        public bool IsNullIfEmpty
        {
            get
            {
                return this.m_fNullIfEmpty;
            }
            set
            {
                this.m_fNullIfEmpty = value;
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
                if ((value < 0) || (value > 0x1f40))
                {
                    throw ADP.ArgumentOutOfRange(Res.GetString("SQLUDT_MaxByteSizeValue"), "MaxByteSize", value);
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
    }
}

