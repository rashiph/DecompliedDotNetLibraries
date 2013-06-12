namespace System.Diagnostics.Eventing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    internal class EventProviderDataStream : IDisposable
    {
        internal Action<EventWrittenEventArgs> m_Callback;
        internal bool[] m_EventEnabled;
        internal bool m_ManifestSent;
        internal EventProviderDataStream m_MasterStream;
        internal EventProviderDataStream m_Next;
        private static EventHandler<EventProviderCreatedEventArgs> s_EventProviderCreated;
        internal static object s_Lock;
        private static List<WeakReference> s_providers;
        internal static EventProviderDataStream s_Streams;

        public static  event EventHandler<EventProviderCreatedEventArgs> EventProviderCreated
        {
            add
            {
                List<EventProviderBase> list = new List<EventProviderBase>();
                lock (EventProviderStreamsLock)
                {
                    s_EventProviderCreated = (EventHandler<EventProviderCreatedEventArgs>) Delegate.Combine(s_EventProviderCreated, value);
                    foreach (WeakReference reference in s_providers)
                    {
                        EventProviderBase target = reference.Target as EventProviderBase;
                        if (target != null)
                        {
                            list.Add(target);
                        }
                    }
                }
                foreach (EventProviderBase base3 in list)
                {
                    EventProviderCreatedEventArgs e = new EventProviderCreatedEventArgs {
                        Provider = base3
                    };
                    value(null, e);
                }
            }
            remove
            {
                s_EventProviderCreated = (EventHandler<EventProviderCreatedEventArgs>) Delegate.Remove(s_EventProviderCreated, value);
            }
        }

        internal EventProviderDataStream(Action<EventWrittenEventArgs> callBack, EventProviderDataStream next, bool[] eventEnabled, EventProviderDataStream masterStream)
        {
            this.m_Callback = callBack;
            this.m_Next = next;
            this.m_EventEnabled = eventEnabled;
            this.m_MasterStream = masterStream;
        }

        internal static void AddProvider(EventProviderBase newProvider)
        {
            lock (EventProviderStreamsLock)
            {
                if (s_providers == null)
                {
                    s_providers = new List<WeakReference>(2);
                }
                int count = -1;
                if ((s_providers.Count % 0x40) == 0x3f)
                {
                    for (int i = 0; i < s_providers.Count; i++)
                    {
                        WeakReference reference = s_providers[i];
                        if (!reference.IsAlive)
                        {
                            count = i;
                            reference.Target = newProvider;
                            break;
                        }
                    }
                }
                if (count < 0)
                {
                    count = s_providers.Count;
                    s_providers.Add(new WeakReference(newProvider));
                }
                newProvider.m_id = count;
                for (EventProviderDataStream stream = s_Streams; stream != null; stream = stream.m_Next)
                {
                    newProvider.m_OutputStreams = new EventProviderDataStream(stream.m_Callback, newProvider.m_OutputStreams, new bool[newProvider.EventIdLimit], stream);
                }
                EventHandler<EventProviderCreatedEventArgs> handler = s_EventProviderCreated;
                if (handler != null)
                {
                    EventProviderCreatedEventArgs e = new EventProviderCreatedEventArgs {
                        Provider = newProvider
                    };
                    handler(null, e);
                }
            }
        }

        public static int ProviderId(EventProviderBase provider)
        {
            return provider.m_id;
        }

        public static EventProviderDataStream Register(Action<EventWrittenEventArgs> callBack)
        {
            lock (EventProviderStreamsLock)
            {
                EventProviderDataStream masterStream = new EventProviderDataStream(callBack, s_Streams, null, null);
                s_Streams = masterStream;
                foreach (WeakReference reference in s_providers)
                {
                    EventProviderBase target = reference.Target as EventProviderBase;
                    if (target != null)
                    {
                        target.m_OutputStreams = new EventProviderDataStream(callBack, target.m_OutputStreams, new bool[target.EventIdLimit], masterStream);
                    }
                }
                return masterStream;
            }
        }

        private void RemoveStreamFromProviders(EventProviderDataStream streamToRemove)
        {
            foreach (WeakReference reference in s_providers)
            {
                EventProviderBase target = reference.Target as EventProviderBase;
                if (target != null)
                {
                    if (target.m_OutputStreams.m_MasterStream == streamToRemove)
                    {
                        target.m_OutputStreams = target.m_OutputStreams.m_Next;
                    }
                    else
                    {
                        EventProviderDataStream outputStreams = target.m_OutputStreams;
                        while (true)
                        {
                            EventProviderDataStream next = outputStreams.m_Next;
                            if (next == null)
                            {
                                break;
                            }
                            if (next.m_MasterStream == streamToRemove)
                            {
                                outputStreams.m_Next = next.m_Next;
                                break;
                            }
                            outputStreams = next;
                        }
                    }
                }
            }
        }

        public void SendCommand(Guid providerGuid, bool enable, EventLevel level)
        {
            this.SendCommand(providerGuid, enable, level, EventKeywords.None, ControllerCommand.Update, null);
        }

        public void SendCommand(Guid providerGuid, bool enable, EventLevel level, EventKeywords matchAnyKeyword, ControllerCommand command, IDictionary<string, string> commandArguments)
        {
            List<EventProviderBase> list = new List<EventProviderBase>();
            lock (EventProviderStreamsLock)
            {
                foreach (WeakReference reference in s_providers)
                {
                    EventProviderBase target = reference.Target as EventProviderBase;
                    if ((target != null) && ((target.Guid == providerGuid) || (providerGuid == Guid.Empty)))
                    {
                        list.Add(target);
                    }
                }
            }
            foreach (EventProviderBase base3 in list)
            {
                base3.SendCommand(this, enable, level, matchAnyKeyword, command, commandArguments);
            }
        }

        void IDisposable.Dispose()
        {
            this.UnRegister();
        }

        public void UnRegister()
        {
            lock (EventProviderStreamsLock)
            {
                if (s_Streams != null)
                {
                    if (this == s_Streams)
                    {
                        s_Streams = this.m_Next;
                    }
                    else
                    {
                        EventProviderDataStream stream = s_Streams;
                        while (true)
                        {
                            EventProviderDataStream next = stream.m_Next;
                            if (next == null)
                            {
                                break;
                            }
                            if (next == this)
                            {
                                stream.m_Next = next.m_Next;
                                this.RemoveStreamFromProviders(next);
                                break;
                            }
                            stream = next;
                        }
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private static void Validate()
        {
        }

        internal static object EventProviderStreamsLock
        {
            get
            {
                if (s_Lock == null)
                {
                    Interlocked.CompareExchange(ref s_Lock, new object(), null);
                }
                return s_Lock;
            }
        }
    }
}

