namespace System.Drawing.Imaging
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    public sealed class MetaHeader
    {
        private short type;
        private short headerSize;
        private short version;
        private int size;
        private short noObjects;
        private int maxRecord;
        private short noParameters;
        public short Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
        public short HeaderSize
        {
            get
            {
                return this.headerSize;
            }
            set
            {
                this.headerSize = value;
            }
        }
        public short Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }
        public int Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
        public short NoObjects
        {
            get
            {
                return this.noObjects;
            }
            set
            {
                this.noObjects = value;
            }
        }
        public int MaxRecord
        {
            get
            {
                return this.maxRecord;
            }
            set
            {
                this.maxRecord = value;
            }
        }
        public short NoParameters
        {
            get
            {
                return this.noParameters;
            }
            set
            {
                this.noParameters = value;
            }
        }
    }
}

