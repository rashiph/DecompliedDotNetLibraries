namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal static class ComPlusActivityTrace
    {
        internal static readonly Guid IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
        internal static readonly Guid IID_IObjectContextInfo = new Guid("75B52DDB-E8ED-11d1-93AD-00AA00BA3258");

        public static void Trace(TraceEventType type, int traceCode, string description)
        {
            if (DiagnosticUtility.ShouldTrace(type))
            {
                Guid empty = Guid.Empty;
                Guid guid = Guid.Empty;
                IComThreadingInfo info = (IComThreadingInfo) SafeNativeMethods.CoGetObjectContext(IID_IComThreadingInfo);
                if (info != null)
                {
                    info.GetCurrentLogicalThreadId(out empty);
                    IObjectContextInfo info2 = info as IObjectContextInfo;
                    if (info2 != null)
                    {
                        info2.GetActivityId(out guid);
                    }
                }
                ComPlusActivitySchema schema = new ComPlusActivitySchema(guid, empty, Thread.CurrentThread.ManagedThreadId, SafeNativeMethods.GetCurrentThreadId());
                TraceUtility.TraceEvent(type, traceCode, System.ServiceModel.SR.GetString(description), (TraceRecord) schema);
            }
        }
    }
}

