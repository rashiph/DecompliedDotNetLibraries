namespace System.ServiceModel.Channels
{
    using System;

    internal class ClientSimplexEncoder : SessionEncoder
    {
        public static byte[] ModeBytes = new byte[] { 0, 1, 0, 1, 3 };

        private ClientSimplexEncoder()
        {
        }
    }
}

