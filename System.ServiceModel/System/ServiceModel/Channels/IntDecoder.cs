namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IntDecoder
    {
        private const int LastIndex = 4;
        private int value;
        private short index;
        private bool isValueDecoded;
        public int Value
        {
            get
            {
                if (!this.isValueDecoded)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.value;
            }
        }
        public bool IsValueDecoded
        {
            get
            {
                return this.isValueDecoded;
            }
        }
        public void Reset()
        {
            this.index = 0;
            this.value = 0;
            this.isValueDecoded = false;
        }

        public int Decode(byte[] buffer, int offset, int size)
        {
            DecoderHelper.ValidateSize(size);
            if (this.isValueDecoded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
            }
            int num = 0;
            while (num < size)
            {
                int num2 = buffer[offset];
                this.value |= (num2 & 0x7f) << (this.index * 7);
                num++;
                if ((this.index == 4) && ((num2 & 0xf8) != 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("FramingSizeTooLarge")));
                }
                this.index = (short) (this.index + 1);
                if ((num2 & 0x80) == 0)
                {
                    this.isValueDecoded = true;
                    return num;
                }
                offset++;
            }
            return num;
        }
    }
}

