namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal abstract class SingletonEncoder
    {
        public static byte[] EndBytes;
        public static byte[] EnvelopeEndBytes = new byte[1];
        public static byte[] EnvelopeEndFramingEndBytes;
        public static byte[] EnvelopeStartBytes = new byte[] { 5 };

        static SingletonEncoder()
        {
            byte[] buffer3 = new byte[2];
            buffer3[1] = 7;
            EnvelopeEndFramingEndBytes = buffer3;
            EndBytes = new byte[] { 7 };
        }

        protected SingletonEncoder()
        {
        }

        public static ArraySegment<byte> EncodeMessageFrame(ArraySegment<byte> messageFrame)
        {
            int encodedSize = IntEncoder.GetEncodedSize(messageFrame.Count);
            int offset = messageFrame.Offset - encodedSize;
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageFrame.Offset", messageFrame.Offset, System.ServiceModel.SR.GetString("SpaceNeededExceedsMessageFrameOffset", new object[] { encodedSize })));
            }
            byte[] array = messageFrame.Array;
            IntEncoder.Encode(messageFrame.Count, array, offset);
            return new ArraySegment<byte>(array, offset, messageFrame.Count + encodedSize);
        }
    }
}

