namespace System.Xml
{
    using System;

    internal class Ucs4Encoding4321 : Ucs4Encoding
    {
        public Ucs4Encoding4321()
        {
            base.ucs4Decoder = new Ucs4Decoder4321();
        }

        public override byte[] GetPreamble()
        {
            byte[] buffer = new byte[4];
            buffer[0] = 0xff;
            buffer[1] = 0xfe;
            return buffer;
        }

        public override string EncodingName
        {
            get
            {
                return "ucs-4";
            }
        }
    }
}

