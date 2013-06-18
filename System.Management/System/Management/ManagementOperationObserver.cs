namespace System.Management
{
    using System;
    using System.Collections;
    using System.Threading;

    public class ManagementOperationObserver
    {
        private WmiDelegateInvoker delegateInvoker;
        private Hashtable m_sinkCollection = new Hashtable();

        public event CompletedEventHandler Completed;

        public event ObjectPutEventHandler ObjectPut;

        public event ObjectReadyEventHandler ObjectReady;

        public event ProgressEventHandler Progress;

        public ManagementOperationObserver()
        {
            this.delegateInvoker = new WmiDelegateInvoker(this);
        }

        public void Cancel()
        {
            Hashtable hashtable = new Hashtable();
            lock (this.m_sinkCollection)
            {
                IDictionaryEnumerator enumerator = this.m_sinkCollection.GetEnumerator();
                try
                {
                    enumerator.Reset();
                    while (enumerator.MoveNext())
                    {
                        DictionaryEntry current = (DictionaryEntry) enumerator.Current;
                        hashtable.Add(current.Key, current.Value);
                    }
                }
                catch
                {
                }
            }
            try
            {
                IDictionaryEnumerator enumerator2 = hashtable.GetEnumerator();
                enumerator2.Reset();
                while (enumerator2.MoveNext())
                {
                    DictionaryEntry entry2 = (DictionaryEntry) enumerator2.Current;
                    WmiEventSink sink = (WmiEventSink) entry2.Value;
                    try
                    {
                        sink.Cancel();
                        continue;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
            }
        }

        internal void FireCompleted(CompletedEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.Completed, args);
            }
            catch
            {
            }
        }

        internal void FireObjectPut(ObjectPutEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.ObjectPut, args);
            }
            catch
            {
            }
        }

        internal void FireObjectReady(ObjectReadyEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.ObjectReady, args);
            }
            catch
            {
            }
        }

        internal void FireProgress(ProgressEventArgs args)
        {
            try
            {
                this.delegateInvoker.FireEventToDelegates(this.Progress, args);
            }
            catch
            {
            }
        }

        internal WmiGetEventSink GetNewGetSink(ManagementScope scope, object context, ManagementObject managementObject)
        {
            try
            {
                WmiGetEventSink sink = WmiGetEventSink.GetWmiGetEventSink(this, context, scope, managementObject);
                lock (this.m_sinkCollection)
                {
                    this.m_sinkCollection.Add(sink.GetHashCode(), sink);
                }
                return sink;
            }
            catch
            {
                return null;
            }
        }

        internal WmiEventSink GetNewPutSink(ManagementScope scope, object context, string path, string className)
        {
            try
            {
                WmiEventSink sink = WmiEventSink.GetWmiEventSink(this, context, scope, path, className);
                lock (this.m_sinkCollection)
                {
                    this.m_sinkCollection.Add(sink.GetHashCode(), sink);
                }
                return sink;
            }
            catch
            {
                return null;
            }
        }

        internal WmiEventSink GetNewSink(ManagementScope scope, object context)
        {
            try
            {
                WmiEventSink sink = WmiEventSink.GetWmiEventSink(this, context, scope, null, null);
                lock (this.m_sinkCollection)
                {
                    this.m_sinkCollection.Add(sink.GetHashCode(), sink);
                }
                return sink;
            }
            catch
            {
                return null;
            }
        }

        internal void RemoveSink(WmiEventSink eventSink)
        {
            try
            {
                lock (this.m_sinkCollection)
                {
                    this.m_sinkCollection.Remove(eventSink.GetHashCode());
                }
                eventSink.ReleaseStub();
            }
            catch
            {
            }
        }

        internal bool HaveListenersForProgress
        {
            get
            {
                bool flag = false;
                try
                {
                    if (this.Progress != null)
                    {
                        flag = this.Progress.GetInvocationList().Length > 0;
                    }
                }
                catch
                {
                }
                return flag;
            }
        }
    }
}

