namespace System.Drawing.Imaging
{
    using System;

    public sealed class Encoder
    {
        public static readonly Encoder ChrominanceTable = new Encoder(new System.Guid(-219916836, 0x9b3, 0x4316, new byte[] { 130, 0x60, 0x67, 0x6a, 0xda, 50, 0x48, 0x1c }));
        public static readonly Encoder ColorDepth = new Encoder(new System.Guid(0x66087055, -21146, 0x4c7c, new byte[] { 0x9a, 0x18, 0x38, 0xa2, 0x31, 11, 0x83, 0x37 }));
        public static readonly Encoder Compression = new Encoder(new System.Guid(-526552163, -13100, 0x44ee, new byte[] { 0x8e, 0xba, 0x3f, 0xbf, 0x8b, 0xe4, 0xfc, 0x58 }));
        private System.Guid guid;
        public static readonly Encoder LuminanceTable = new Encoder(new System.Guid(-307020850, 0x266, 0x4a77, new byte[] { 0xb9, 4, 0x27, 0x21, 0x60, 0x99, 0xe7, 0x17 }));
        public static readonly Encoder Quality = new Encoder(new System.Guid(0x1d5be4b5, -1462, 0x452d, new byte[] { 0x9c, 0xdd, 0x5d, 0xb3, 0x51, 5, 0xe7, 0xeb }));
        public static readonly Encoder RenderMethod = new Encoder(new System.Guid(0x6d42c53a, 0x229a, 0x4825, new byte[] { 0x8b, 0xb7, 0x5c, 0x99, 0xe2, 0xb9, 0xa8, 0xb8 }));
        public static readonly Encoder SaveFlag = new Encoder(new System.Guid(0x292266fc, -21440, 0x47bf, new byte[] { 140, 0xfc, 0xa8, 0x5b, 0x89, 0xa6, 0x55, 0xde }));
        public static readonly Encoder ScanMethod = new Encoder(new System.Guid(0x3a4e2661, 0x3109, 0x4e56, new byte[] { 0x85, 0x36, 0x42, 0xc1, 0x56, 0xe7, 220, 250 }));
        public static readonly Encoder Transformation = new Encoder(new System.Guid(-1928416559, -23154, 0x4ea8, new byte[] { 170, 20, 0x10, 0x80, 0x74, 0xb7, 0xb6, 0xf9 }));
        public static readonly Encoder Version = new Encoder(new System.Guid(0x24d18c76, -32438, 0x41a4, new byte[] { 0xbf, 0x53, 0x1c, 0x21, 0x9c, 0xcc, 0xf7, 0x97 }));

        public Encoder(System.Guid guid)
        {
            this.guid = guid;
        }

        public System.Guid Guid
        {
            get
            {
                return this.guid;
            }
        }
    }
}

