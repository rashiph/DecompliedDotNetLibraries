namespace System.Xml
{
    using System;

    internal class Ucs4Encoding3412 : Ucs4Encoding
    {
        public Ucs4Encoding3412()
        {
            base.ucs4Decoder = new Ucs4Decoder3412();
        }

        public override byte[] GetPreamble()
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0xfe;
            buffer[1] = 0xff;
            return buffer;
        }

        public override string EncodingName
        {
            get
            {
                return "ucs-4 (order 3412)";
            }
        }
    }
}

