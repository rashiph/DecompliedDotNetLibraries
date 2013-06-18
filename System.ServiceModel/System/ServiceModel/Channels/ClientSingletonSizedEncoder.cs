namespace System.ServiceModel.Channels
{
    using System;

    internal static class ClientSingletonSizedEncoder
    {
        public static byte[] ModeBytes = new byte[] { 0, 1, 0, 1, 4 };

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

