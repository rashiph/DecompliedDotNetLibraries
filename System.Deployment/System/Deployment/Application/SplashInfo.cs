namespace System.Deployment.Application
{
    using System;
    using System.Threading;

    internal class SplashInfo
    {
        public bool cancelled;
        public bool initializedAsWait = true;
        public ManualResetEvent pieceReady = new ManualResetEvent(true);
    }
}

