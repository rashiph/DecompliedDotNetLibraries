namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Diagnostics;

    internal static class EtwTrace
    {
        private const int MaxSupportedStringSize = 0xffce;
        private static EtwTraceProvider provider;
        private static object syncRoot = new object();
        private static Guid WsatProviderGuid = new Guid("7f3fe630-462b-47c5-ab07-67ca84934abd");
        private static Guid WsatTraceGuid = new Guid("{eb6517d4-090c-48ab-825e-adad366406a2}");

        private static Guid GetActivityId()
        {
            object activityId = System.Diagnostics.Trace.CorrelationManager.ActivityId;
            if (activityId != null)
            {
                return (Guid) activityId;
            }
            return Guid.Empty;
        }

        internal static void Trace(string xml, TraceType type, int eventId)
        {
            TraceInternal(GetActivityId(), xml, type, eventId);
        }

        private static unsafe uint TraceInternal(Guid guid, string xml, TraceType type, int eventId)
        {
            uint maxValue = uint.MaxValue;
            if ((Provider != null) && Provider.ShouldTrace)
            {
                int num2 = (((xml.Length + 1) * 2) < 0xffce) ? ((xml.Length + 1) * 2) : 0xffce;
                Mof3Event event2 = new Mof3Event();
                event2.Header.Guid = WsatTraceGuid;
                event2.Header.Type = (byte) type;
                event2.Header.ClientContext = 0;
                event2.Header.Flags = 0x120000;
                event2.Header.BufferSize = 0x60;
                event2.Mof2.Length = (uint) num2;
                event2.Mof1.Length = 0x10;
                event2.Mof1.Data = (IntPtr) &guid;
                event2.Mof3.Length = 4;
                event2.Mof3.Data = (IntPtr) &eventId;
                fixed (char* str = ((char*) xml))
                {
                    char* chPtr = str;
                    event2.Mof2.Data = (IntPtr) chPtr;
                    if (Provider != null)
                    {
                        maxValue = provider.Trace((MofEvent*) &event2);
                    }
                }
            }
            return maxValue;
        }

        internal static uint TraceTransfer(Guid relatedId)
        {
            return TraceTransfer(GetActivityId(), relatedId);
        }

        private static unsafe uint TraceTransfer(Guid activityId, Guid relatedId)
        {
            uint maxValue = uint.MaxValue;
            if ((Provider != null) && Provider.ShouldTrace)
            {
                Guid2Event event2 = new Guid2Event();
                event2.Header.Guid = WsatTraceGuid;
                event2.Header.Type = 5;
                event2.Header.ClientContext = 0;
                event2.Header.Flags = 0x20000;
                event2.Header.BufferSize = 80;
                event2.Guid1 = activityId;
                event2.Guid2 = relatedId;
                if (Provider != null)
                {
                    maxValue = provider.Trace((MofEvent*) &event2);
                }
            }
            return maxValue;
        }

        internal static EtwTraceProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    lock (syncRoot)
                    {
                        if (provider == null)
                        {
                            provider = new EtwTraceProvider(WsatProviderGuid, WsatTraceGuid);
                        }
                    }
                }
                return provider;
            }
        }
    }
}

