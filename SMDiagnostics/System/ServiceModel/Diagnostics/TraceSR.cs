namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class TraceSR
    {
        internal const string ActivityBoundary = "ActivityBoundary";
        internal const string GenericCallbackException = "GenericCallbackException";
        private static TraceSR loader;
        private ResourceManager resources;
        internal const string ThrowingException = "ThrowingException";
        internal const string TraceCodeAppDomainUnload = "TraceCodeAppDomainUnload";
        internal const string TraceCodeEventLog = "TraceCodeEventLog";
        internal const string TraceCodeTraceTruncatedQuotaExceeded = "TraceCodeTraceTruncatedQuotaExceeded";
        internal const string TraceHandledException = "TraceHandledException";
        internal const string UnhandledException = "UnhandledException";
        internal const string WriteCharsInvalidContent = "WriteCharsInvalidContent";

        internal TraceSR()
        {
            this.resources = new ResourceManager("SMDiagnostics", base.GetType().Assembly);
        }

        private static TraceSR GetLoader()
        {
            if (loader == null)
            {
                TraceSR esr = new TraceSR();
                Interlocked.CompareExchange<TraceSR>(ref loader, esr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            TraceSR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            TraceSR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            TraceSR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

