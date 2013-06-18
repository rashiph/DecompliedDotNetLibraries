namespace System.ServiceModel.Channels
{
    using System;

    internal class ClientSingletonEncoder : SingletonEncoder
    {
        public static byte[] ModeBytes = new byte[] { 0, 1, 0, 1, 1 };
        public static byte[] PreambleEndBytes = new byte[] { 12 };

        private ClientSingletonEncoder()
        {
        }

        public static int CalcStartSize(EncodedVia via, EncodedContentType contentType)
        {
            return (via.EncodedBytes.Length + contentType.EncodedBytes.Length);
        }

        public static void EncodeStart(byte[] buffer, int offset, EncodedVia via, EncodedContentType contentType)
        {
            Buffer.BlockCopy(via.EncodedBytes, 0, buffer, offset, via.EncodedBytes.Length);
            Buffer.BlockCopy(contentType.EncodedBytes, 0, buffer, offset + via.EncodedBytes.Length, contentType.EncodedBytes.Length);
        }
    }
}

