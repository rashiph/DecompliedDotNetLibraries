namespace System.Runtime.InteropServices
{
    using System;
    using System.Security;

    [SecurityCritical]
    internal class ComEventsInfo
    {
        private object _rcw;
        private ComEventsSink _sinks;

        private ComEventsInfo(object rcw)
        {
            this._rcw = rcw;
        }

        internal ComEventsSink AddSink(ref Guid iid)
        {
            ComEventsSink sink = new ComEventsSink(this._rcw, iid);
            this._sinks = ComEventsSink.Add(this._sinks, sink);
            return this._sinks;
        }

        [SecuritySafeCritical]
        ~ComEventsInfo()
        {
            this._sinks = ComEventsSink.RemoveAll(this._sinks);
        }

        [SecurityCritical]
        internal static ComEventsInfo Find(object rcw)
        {
            return (ComEventsInfo) Marshal.GetComObjectData(rcw, typeof(ComEventsInfo));
        }

        internal ComEventsSink FindSink(ref Guid iid)
        {
            return ComEventsSink.Find(this._sinks, ref iid);
        }

        [SecurityCritical]
        internal static ComEventsInfo FromObject(object rcw)
        {
            ComEventsInfo data = Find(rcw);
            if (data == null)
            {
                data = new ComEventsInfo(rcw);
                Marshal.SetComObjectData(rcw, typeof(ComEventsInfo), data);
            }
            return data;
        }

        [SecurityCritical]
        internal ComEventsSink RemoveSink(ComEventsSink sink)
        {
            this._sinks = ComEventsSink.Remove(this._sinks, sink);
            return this._sinks;
        }
    }
}

