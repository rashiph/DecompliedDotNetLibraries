namespace Microsoft.SqlServer.Server
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false, Inherited=false)]
    public class SqlFacetAttribute : Attribute
    {
        private bool m_IsFixedLength;
        private bool m_IsNullable;
        private int m_MaxSize;
        private int m_Precision;
        private int m_Scale;

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

        public bool IsNullable
        {
            get
            {
                return this.m_IsNullable;
            }
            set
            {
                this.m_IsNullable = value;
            }
        }

        public int MaxSize
        {
            get
            {
                return this.m_MaxSize;
            }
            set
            {
                this.m_MaxSize = value;
            }
        }

        public int Precision
        {
            get
            {
                return this.m_Precision;
            }
            set
            {
                this.m_Precision = value;
            }
        }

        public int Scale
        {
            get
            {
                return this.m_Scale;
            }
            set
            {
                this.m_Scale = value;
            }
        }
    }
}

