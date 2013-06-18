namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;

    internal static class PeerStrings
    {
        public const string CacheMiss = "CacheMiss";
        public const string ConnectAction = "http://schemas.microsoft.com/net/2006/05/peer/Connect";
        public const string DisconnectAction = "http://schemas.microsoft.com/net/2006/05/peer/Disconnect";
        public const string FloodAction = "http://schemas.microsoft.com/net/2006/05/peer/Flood";
        public const string HopCountElementName = "Hops";
        public const string HopCountElementNamespace = "http://schemas.microsoft.com/net/2006/05/peer/HopCount";
        public const string InternalFloodAction = "http://schemas.microsoft.com/net/2006/05/peer/IntFlood";
        public const string KnownServiceUriPrefix = "PeerChannelEndpoints";
        public const string LinkUtilityAction = "http://schemas.microsoft.com/net/2006/05/peer/LinkUtility";
        public const string MessageId = "MessageID";
        public const string MessageVerified = "MessageVerified";
        public const string Namespace = "http://schemas.microsoft.com/net/2006/05/peer";
        public const string PeerCustomResolver = "PeerCustomResolver";
        public const string PeerProperty = "PeerProperty";
        public const string PingAction = "http://schemas.microsoft.com/net/2006/05/peer/Ping";
        public static Dictionary<string, string> protocolActions = new Dictionary<string, string>();
        public const string RefuseAction = "http://schemas.microsoft.com/net/2006/05/peer/Refuse";
        public const string RequestSecurityTokenAction = "RequestSecurityToken";
        public const string RequestSecurityTokenResponseAction = "RequestSecurityTokenResponse";
        public const string Scheme = "net.p2p";
        public const string ServiceContractName = "PeerService";
        public const string SkipLocalChannels = "SkipLocalChannels";
        public const string Via = "PeerVia";
        public const string WelcomeAction = "http://schemas.microsoft.com/net/2006/05/peer/Welcome";

        static PeerStrings()
        {
            PopulateProtocolActions();
        }

        public static string FindAction(string action)
        {
            string str = null;
            protocolActions.TryGetValue(action, out str);
            return str;
        }

        private static void PopulateProtocolActions()
        {
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/Connect", "Connect");
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/Welcome", "Welcome");
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/Refuse", "Refuse");
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/Disconnect", "Disconnect");
            protocolActions.Add("RequestSecurityToken", "ProcessRequestSecurityToken");
            protocolActions.Add("RequestSecurityTokenResponse", "RequestSecurityTokenResponse");
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/LinkUtility", "LinkUtility");
            protocolActions.Add("http://www.w3.org/2005/08/addressing/fault", "Fault");
            protocolActions.Add("http://schemas.microsoft.com/net/2006/05/peer/Ping", "Ping");
        }
    }
}

