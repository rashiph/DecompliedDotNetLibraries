namespace System.ServiceModel.Channels
{
    using System;

    internal class ServerSingletonEncoder : SingletonEncoder
    {
        public static byte[] AckResponseBytes = new byte[] { 11 };
        public static byte[] UpgradeResponseBytes = new byte[] { 10 };

        private ServerSingletonEncoder()
        {
        }
    }
}

