namespace System.Drawing.Drawing2D
{
    using System;

    public sealed class RegionData
    {
        private byte[] data;

        internal RegionData(byte[] data)
        {
            this.data = data;
        }

        public byte[] Data
        {
            get
            {
                return this.data;
            }
            set
            {
                this.data = value;
            }
        }
    }
}

