namespace System.ServiceModel.Channels
{
    using System;

    internal class PeerPropertyNames
    {
        public static readonly string Certificate = "Credentials.Peer.Certificate";
        public static readonly string Credentials = "SecurityCredentialsManager";
        public static readonly string MessageSenderAuthentication = "Credentials.Peer.MessageSenderAuthentication";
        public static readonly string Password = "Credentials.Peer.MeshPassword";
        public static readonly string PeerAuthentication = "Credentials.Peer.PeerAuthentication";
    }
}

