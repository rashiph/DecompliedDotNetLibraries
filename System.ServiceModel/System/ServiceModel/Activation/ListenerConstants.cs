namespace System.ServiceModel.Activation
{
    using System;

    internal static class ListenerConstants
    {
        public const int DefaultListenBackLog = 10;
        public const int DefaultMaxPendingAccepts = 2;
        public const int DefaultMaxPendingConnections = 100;
        public const bool DefaultPerformanceCountersEnabled = true;
        public const string DefaultReceiveTimeoutString = "00:00:10";
        public const bool DefaultTeredoEnabled = false;
        public const string GlobalPrefix = @"Global\";
        public const int MaxRetries = 5;
        public const int MaxUriSize = 0x800;
        public const string MsmqActivationServiceName = "NetMsmqActivator";
        public const string NamedPipeActivationServiceName = "NetPipeActivator";
        public const string NamedPipeSharedMemoryName = "NetPipeActivator/endpoint";
        public static readonly TimeSpan RegistrationCloseTimeout = TimeSpan.FromSeconds(2.0);
        public const int RegistrationMaxConcurrentSessions = 0x7fffffff;
        public const int RegistrationMaxReceivedMessageSize = 0x2710;
        public static readonly TimeSpan ServiceStartTimeout = TimeSpan.FromSeconds(10.0);
        public const int ServiceStopTimeout = 0x7530;
        public const int SharedConnectionBufferSize = 0x9c4;
        public const int SharedMaxContentTypeSize = 0x100;
        public const int SharedMaxDrainSize = 0x10000;
        public static readonly TimeSpan SharedSendTimeout = ServiceDefaults.SendTimeout;
        public const string TcpActivationServiceName = "NetTcpActivator";
        public const string TcpPortSharingServiceName = "NetTcpPortSharing";
        public const string TcpSharedMemoryName = "NetTcpPortSharing/endpoint";
        public static readonly TimeSpan WasConnectTimeout = TimeSpan.FromSeconds(120.0);
    }
}

