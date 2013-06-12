namespace System.Drawing.Imaging
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class WmfPlaceableFileHeader
    {
        private int key = -1698247209;
        private short hmf;
        private short bboxLeft;
        private short bboxTop;
        private short bboxRight;
        private short bboxBottom;
        private short inch;
        private int reserved;
        private short checksum;
        public int Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }
        public short Hmf
        {
            get
            {
                return this.hmf;
            }
            set
            {
                this.hmf = value;
            }
        }
        public short BboxLeft
        {
            get
            {
                return this.bboxLeft;
            }
            set
            {
                this.bboxLeft = value;
            }
        }
        public short BboxTop
        {
            get
            {
                return this.bboxTop;
            }
            set
            {
                this.bboxTop = value;
            }
        }
        public short BboxRight
        {
            get
            {
                return this.bboxRight;
            }
            set
            {
                this.bboxRight = value;
            }
        }
        public short BboxBottom
        {
            get
            {
                return this.bboxBottom;
            }
            set
            {
                this.bboxBottom = value;
            }
        }
        public short Inch
        {
            get
            {
                return this.inch;
            }
            set
            {
                this.inch = value;
            }
        }
        public int Reserved
        {
            get
            {
                return this.reserved;
            }
            set
            {
                this.reserved = value;
            }
        }
        public short Checksum
        {
            get
            {
                return this.checksum;
            }
            set
            {
                this.checksum = value;
            }
        }
    }
}

