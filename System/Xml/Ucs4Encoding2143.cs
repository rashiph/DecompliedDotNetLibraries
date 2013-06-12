namespace System.Xml
{
    using System;

    internal class Ucs4Encoding2143 : Ucs4Encoding
    {
        public Ucs4Encoding2143()
        {
            base.ucs4Decoder = new Ucs4Decoder2143();
        }

        public override byte[] GetPreamble()
        {
            byte[] buffer = new byte[4];
            buffer[2] = 0xff;
            buffer[3] = 0xfe;
            return buffer;
        }

        public override string EncodingName
        {
            get
            {
                return "ucs-4 (order 2143)";
            }
        }
    }
}

