namespace System.Web.Services.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;

    internal static class Tracing
    {
        private static bool appDomainShutdown;
        private static TraceSource asmxTraceSource;
        private static object internalSyncObject;
        private const string TraceSourceAsmx = "System.Web.Services.Asmx";
        private static bool tracingEnabled = true;
        private static bool tracingInitialized;

        private static void AppDomainUnloadEvent(object sender, EventArgs e)
        {
            Close();
            appDomainShutdown = true;
        }

        private static void Close()
        {
            if (asmxTraceSource != null)
            {
                asmxTraceSource.Close();
            }
        }

        internal static List<string> Details(HttpRequest request)
        {
            if (request == null)
            {
                return null;
            }
            List<string> list = null;
            list = new List<string> {
                Res.GetString("TraceUserHostAddress", new object[] { request.UserHostAddress })
            };
            string str = (request.UserHostAddress == request.UserHostName) ? GetHostByAddress(request.UserHostAddress) : request.UserHostName;
            if (!string.IsNullOrEmpty(str))
            {
                list.Add(Res.GetString("TraceUserHostName", new object[] { str }));
            }
            list.Add(Res.GetString("TraceUrl", new object[] { request.HttpMethod, request.Url }));
            if (request.UrlReferrer != null)
            {
                list.Add(Res.GetString("TraceUrlReferrer", new object[] { request.UrlReferrer }));
            }
            return list;
        }

        internal static void Enter(string callId, TraceMethod caller)
        {
            Enter(callId, caller, null, null);
        }

        internal static void Enter(string callId, TraceMethod caller, List<string> details)
        {
            Enter(callId, caller, null, details);
        }

        internal static void Enter(string callId, TraceMethod caller, TraceMethod callDetails)
        {
            Enter(callId, caller, callDetails, null);
        }

        internal static void Enter(string callId, TraceMethod caller, TraceMethod callDetails, List<string> details)
        {
            if (ValidateSettings(Asmx, TraceEventType.Information))
            {
                string str = (callDetails == null) ? Res.GetString("TraceCallEnter", new object[] { callId, caller }) : Res.GetString("TraceCallEnterDetails", new object[] { callId, caller, callDetails });
                if ((details != null) && (details.Count > 0))
                {
                    StringBuilder builder = new StringBuilder(str);
                    foreach (string str2 in details)
                    {
                        builder.Append(Environment.NewLine);
                        builder.Append("    ");
                        builder.Append(str2);
                    }
                    str = builder.ToString();
                }
                TraceEvent(TraceEventType.Information, str);
            }
        }

        internal static Exception ExceptionCatch(TraceMethod method, Exception e)
        {
            return ExceptionCatch(TraceEventType.Error, method, e);
        }

        internal static Exception ExceptionCatch(TraceEventType eventType, TraceMethod method, Exception e)
        {
            if (ValidateSettings(Asmx, eventType))
            {
                TraceEvent(eventType, Res.GetString("TraceExceptionCought", new object[] { method, e.GetType(), e.Message }));
                StackTrace(eventType, e);
            }
            return e;
        }

        internal static Exception ExceptionCatch(TraceEventType eventType, object target, string method, Exception e)
        {
            if (ValidateSettings(Asmx, eventType))
            {
                TraceEvent(eventType, Res.GetString("TraceExceptionCought", new object[] { TraceMethod.MethodId(target, method), e.GetType(), e.Message }));
                StackTrace(eventType, e);
            }
            return e;
        }

        internal static Exception ExceptionIgnore(TraceEventType eventType, TraceMethod method, Exception e)
        {
            if (ValidateSettings(Asmx, eventType))
            {
                TraceEvent(eventType, Res.GetString("TraceExceptionIgnored", new object[] { method, e.GetType(), e.Message }));
                StackTrace(eventType, e);
            }
            return e;
        }

        internal static Exception ExceptionThrow(TraceMethod method, Exception e)
        {
            return ExceptionThrow(TraceEventType.Error, method, e);
        }

        internal static Exception ExceptionThrow(TraceEventType eventType, TraceMethod method, Exception e)
        {
            if (ValidateSettings(Asmx, eventType))
            {
                TraceEvent(eventType, Res.GetString("TraceExceptionThrown", new object[] { method.ToString(), e.GetType(), e.Message }));
                StackTrace(eventType, e);
            }
            return e;
        }

        internal static void Exit(string callId, TraceMethod caller)
        {
            if (ValidateSettings(Asmx, TraceEventType.Information))
            {
                TraceEvent(TraceEventType.Information, Res.GetString("TraceCallExit", new object[] { callId, caller }));
            }
        }

        internal static XmlDeserializationEvents GetDeserializationEvents()
        {
            return new XmlDeserializationEvents { OnUnknownElement = new XmlElementEventHandler(Tracing.OnUnknownElement), OnUnknownAttribute = new XmlAttributeEventHandler(Tracing.OnUnknownAttribute) };
        }

        private static string GetHostByAddress(string ipAddress)
        {
            try
            {
                return Dns.GetHostByAddress(ipAddress).HostName;
            }
            catch
            {
                return null;
            }
        }

        internal static void Information(string format, params object[] args)
        {
            if (ValidateSettings(Asmx, TraceEventType.Information))
            {
                TraceEvent(TraceEventType.Information, Res.GetString(format, args));
            }
        }

        private static void InitializeLogging()
        {
            lock (InternalSyncObject)
            {
                if (!tracingInitialized)
                {
                    bool flag = false;
                    asmxTraceSource = new TraceSource("System.Web.Services.Asmx");
                    if (asmxTraceSource.Switch.ShouldTrace(TraceEventType.Critical))
                    {
                        flag = true;
                        AppDomain currentDomain = AppDomain.CurrentDomain;
                        currentDomain.UnhandledException += new UnhandledExceptionEventHandler(Tracing.UnhandledExceptionHandler);
                        currentDomain.DomainUnload += new EventHandler(Tracing.AppDomainUnloadEvent);
                        currentDomain.ProcessExit += new EventHandler(Tracing.ProcessExitEvent);
                    }
                    tracingEnabled = flag;
                    tracingInitialized = true;
                }
            }
        }

        internal static void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            if ((ValidateSettings(Asmx, TraceEventType.Warning) && (e.Attr != null)) && !RuntimeUtils.IsKnownNamespace(e.Attr.NamespaceURI))
            {
                string name = (e.ExpectedAttributes == null) ? "WebUnknownAttribute" : ((e.ExpectedAttributes.Length == 0) ? "WebUnknownAttribute2" : "WebUnknownAttribute3");
                TraceEvent(TraceEventType.Warning, Res.GetString(name, new object[] { e.Attr.Name, e.Attr.Value, e.ExpectedAttributes }));
            }
        }

        internal static void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            if (ValidateSettings(Asmx, TraceEventType.Warning) && (e.Element != null))
            {
                string str = RuntimeUtils.ElementString(e.Element);
                string name = (e.ExpectedElements == null) ? "WebUnknownElement" : ((e.ExpectedElements.Length == 0) ? "WebUnknownElement1" : "WebUnknownElement2");
                TraceEvent(TraceEventType.Warning, Res.GetString(name, new object[] { str, e.ExpectedElements }));
            }
        }

        private static void ProcessExitEvent(object sender, EventArgs e)
        {
            Close();
            appDomainShutdown = true;
        }

        private static void StackTrace(TraceEventType eventType, Exception e)
        {
            if (IsVerbose && !string.IsNullOrEmpty(e.StackTrace))
            {
                TraceEvent(eventType, Res.GetString("TraceExceptionDetails", new object[] { e.ToString() }));
            }
        }

        private static void TraceEvent(TraceEventType eventType, string format)
        {
            Asmx.TraceEvent(eventType, 0, format);
        }

        internal static string TraceId(string id)
        {
            return Res.GetString(id);
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exceptionObject = (Exception) args.ExceptionObject;
            ExceptionCatch(TraceEventType.Error, sender, "UnhandledExceptionHandler", exceptionObject);
        }

        private static bool ValidateSettings(TraceSource traceSource, TraceEventType traceLevel)
        {
            if (!tracingEnabled)
            {
                return false;
            }
            if (!tracingInitialized)
            {
                InitializeLogging();
            }
            if ((traceSource == null) || !traceSource.Switch.ShouldTrace(traceLevel))
            {
                return false;
            }
            if (appDomainShutdown)
            {
                return false;
            }
            return true;
        }

        internal static TraceSource Asmx
        {
            get
            {
                if (!tracingInitialized)
                {
                    InitializeLogging();
                }
                if (!tracingEnabled)
                {
                    return null;
                }
                return asmxTraceSource;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (internalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref internalSyncObject, obj2, null);
                }
                return internalSyncObject;
            }
        }

        internal static bool IsVerbose
        {
            get
            {
                return ValidateSettings(Asmx, TraceEventType.Verbose);
            }
        }

        internal static bool On
        {
            get
            {
                if (!tracingInitialized)
                {
                    InitializeLogging();
                }
                return tracingEnabled;
            }
        }
    }
}

