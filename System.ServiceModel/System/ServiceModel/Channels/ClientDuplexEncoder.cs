namespace System.ServiceModel.Channels
{
    using System;

    internal class ClientDuplexEncoder : SessionEncoder
    {
        public static byte[] ModeBytes = new byte[] { 0, 1, 0, 1, 2 };

        private ClientDuplexEncoder()
        {
        }
    }
}

