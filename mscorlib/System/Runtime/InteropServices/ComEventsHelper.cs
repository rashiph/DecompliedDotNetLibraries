namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Remoting;
    using System.Security;

    public static class ComEventsHelper
    {
        [SecurityCritical]
        public static void Combine(object rcw, Guid iid, int dispid, Delegate d)
        {
            rcw = UnwrapIfTransparentProxy(rcw);
            lock (rcw)
            {
                ComEventsInfo info = ComEventsInfo.FromObject(rcw);
                ComEventsSink sink = info.FindSink(ref iid);
                if (sink == null)
                {
                    sink = info.AddSink(ref iid);
                }
                ComEventsMethod method = sink.FindMethod(dispid);
                if (method == null)
                {
                    method = sink.AddMethod(dispid);
                }
                method.AddDelegate(d);
            }
        }

        [SecurityCritical]
        public static Delegate Remove(object rcw, Guid iid, int dispid, Delegate d)
        {
            rcw = UnwrapIfTransparentProxy(rcw);
            lock (rcw)
            {
                ComEventsInfo info = ComEventsInfo.Find(rcw);
                if (info == null)
                {
                    return null;
                }
                ComEventsSink sink = info.FindSink(ref iid);
                if (sink == null)
                {
                    return null;
                }
                ComEventsMethod method = sink.FindMethod(dispid);
                if (method == null)
                {
                    return null;
                }
                method.RemoveDelegate(d);
                if (method.Empty)
                {
                    method = sink.RemoveMethod(method);
                }
                if (method == null)
                {
                    sink = info.RemoveSink(sink);
                }
                if (sink == null)
                {
                    Marshal.SetComObjectData(rcw, typeof(ComEventsInfo), null);
                    GC.SuppressFinalize(info);
                }
                return d;
            }
        }

        [SecurityCritical]
        internal static object UnwrapIfTransparentProxy(object rcw)
        {
            if (RemotingServices.IsTransparentProxy(rcw))
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(rcw);
                try
                {
                    rcw = Marshal.GetObjectForIUnknown(iUnknownForObject);
                }
                finally
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            return rcw;
        }
    }
}

