namespace System.Xml
{
    using System;

    internal class Ucs4Encoding1234 : Ucs4Encoding
    {
        public Ucs4Encoding1234()
        {
            base.ucs4Decoder = new Ucs4Decoder1234();
        }

        public override byte[] GetPreamble()
        {
            byte[] buffer = new byte[4];
            buffer[2] = 0xfe;
            buffer[3] = 0xff;
            return buffer;
        }

        public override string EncodingName
        {
            get
            {
                return "ucs-4 (Bigendian)";
            }
        }
    }
}

