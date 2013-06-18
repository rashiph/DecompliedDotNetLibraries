namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal abstract class SessionEncoder
    {
        public static byte[] EndBytes = new byte[] { 7 };
        public const int MaxMessageFrameSize = 6;
        public static byte[] PreambleEndBytes = new byte[] { 12 };

        protected SessionEncoder()
        {
        }

        public static int CalcStartSize(EncodedVia via, EncodedContentType contentType)
        {
            return (via.EncodedBytes.Length + contentType.EncodedBytes.Length);
        }

        public static ArraySegment<byte> EncodeMessageFrame(ArraySegment<byte> messageFrame)
        {
            int num = 1 + IntEncoder.GetEncodedSize(messageFrame.Count);
            int offset = messageFrame.Offset - num;
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("messageFrame.Offset", messageFrame.Offset, System.ServiceModel.SR.GetString("SpaceNeededExceedsMessageFrameOffset", new object[] { num })));
            }
            byte[] array = messageFrame.Array;
            array[offset++] = 6;
            IntEncoder.Encode(messageFrame.Count, array, offset);
            return new ArraySegment<byte>(array, messageFrame.Offset - num, messageFrame.Count + num);
        }

        public static void EncodeStart(byte[] buffer, int offset, EncodedVia via, EncodedContentType contentType)
        {
            Buffer.BlockCopy(via.EncodedBytes, 0, buffer, offset, via.EncodedBytes.Length);
            Buffer.BlockCopy(contentType.EncodedBytes, 0, buffer, offset + via.EncodedBytes.Length, contentType.EncodedBytes.Length);
        }
    }
}

