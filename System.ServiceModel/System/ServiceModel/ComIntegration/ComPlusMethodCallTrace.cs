namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal static class ComPlusMethodCallTrace
    {
        private static readonly Guid IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
        private static readonly Guid IID_IObjectContextInfo = new Guid("75B52DDB-E8ED-11d1-93AD-00AA00BA3258");

        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity, Guid iid, int instanceID, bool traceContextTransaction)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMethodCallSchema schema = null;
                Guid empty = Guid.Empty;
                if (traceContextTransaction)
                {
                    IComThreadingInfo info2 = (IComThreadingInfo) SafeNativeMethods.CoGetObjectContext(IID_IComThreadingInfo);
                    if (info2 != null)
                    {
                        IObjectContextInfo info3 = info2 as IObjectContextInfo;
                        if ((info3 != null) && info3.IsInTransaction())
                        {
                            info3.GetTransactionId(out empty);
                        }
                    }
                    if (empty != Guid.Empty)
                    {
                        schema = new ComPlusMethodCallContextTxSchema(from, info.AppID, info.Clsid, iid, action, instanceID, Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity, empty);
                    }
                }
                else
                {
                    schema = new ComPlusMethodCallSchema(from, info.AppID, info.Clsid, iid, action, instanceID, Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity);
                }
                if (schema != null)
                {
                    TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
                }
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity, Guid iid, int instanceID, Guid guidIncomingTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMethodCallNewTxSchema schema = new ComPlusMethodCallNewTxSchema(from, info.AppID, info.Clsid, iid, action, instanceID, Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity, guidIncomingTransactionID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }

        public static void Trace(TraceEventType type, int traceCode, string description, ServiceInfo info, Uri from, string action, string callerIdentity, Guid iid, int instanceID, Guid incomingTransactionID, Guid currentTransactionID)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                ComPlusMethodCallTxMismatchSchema schema = new ComPlusMethodCallTxMismatchSchema(from, info.AppID, info.Clsid, iid, action, instanceID, Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId(), callerIdentity, incomingTransactionID, currentTransactionID);
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

