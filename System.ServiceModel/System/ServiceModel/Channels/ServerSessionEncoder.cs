namespace System.ServiceModel.Channels
{
    using System;

    internal abstract class ServerSessionEncoder : SessionEncoder
    {
        public static byte[] AckResponseBytes = new byte[] { 11 };
        public static byte[] UpgradeResponseBytes = new byte[] { 10 };

        protected ServerSessionEncoder()
        {
        }
    }
}

