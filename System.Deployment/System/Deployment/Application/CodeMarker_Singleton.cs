namespace System.Deployment.Application
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Threading;

    internal static class CodeMarker_Singleton
    {
        private static CodeMarkers codemarkers = null;
        private static object syncObject = new object();

        public static CodeMarkers Instance
        {
            get
            {
                if (codemarkers == null)
                {
                    lock (syncObject)
                    {
                        if (codemarkers == null)
                        {
                            CodeMarkers instance = CodeMarkers.Instance;
                            instance.InitPerformanceDll(CodeMarkerApp.CLICKONCEPERF, @"Software\Microsoft\VisualStudio\8.0");
                            Thread.MemoryBarrier();
                            codemarkers = instance;
                        }
                    }
                }
                return codemarkers;
            }
        }
    }
}

