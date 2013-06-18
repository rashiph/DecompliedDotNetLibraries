namespace System.ServiceModel.Activation.Diagnostics
{
    using System;

    internal static class TraceCode
    {
        public const int Activation = 0x90000;
        public const int WebHostCompilation = 0x90004;
        public const int WebHostDebugRequest = 0x90005;
        public const int WebHostFailedToActivateService = 0x90003;
        public const int WebHostFailedToCompile = 0x90001;
        public const int WebHostNoCBTSupport = 0x90008;
        public const int WebHostProtocolMisconfigured = 0x90006;
        public const int WebHostServiceActivated = 0x90002;
        public const int WebHostServiceCloseFailed = 0x90007;
    }
}

