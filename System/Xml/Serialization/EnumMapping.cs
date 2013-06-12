namespace System.Xml.Serialization
{
    using System;

    internal class EnumMapping : PrimitiveMapping
    {
        private ConstantMapping[] constants;
        private bool isFlags;

        internal ConstantMapping[] Constants
        {
            get
            {
                return this.constants;
            }
            set
            {
                this.constants = value;
            }
        }

        internal bool IsFlags
        {
            get
            {
                return this.isFlags;
            }
            set
            {
                this.isFlags = value;
            }
        }
    }
}

