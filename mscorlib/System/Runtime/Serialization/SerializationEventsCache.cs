namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;

    internal static class SerializationEventsCache
    {
        private static Hashtable cache = new Hashtable();

        internal static SerializationEvents GetSerializationEventsForType(Type t)
        {
            SerializationEvents events = (SerializationEvents) cache[t];
            if (events == null)
            {
                lock (cache.SyncRoot)
                {
                    events = (SerializationEvents) cache[t];
                    if (events == null)
                    {
                        events = new SerializationEvents(t);
                        cache[t] = events;
                    }
                }
            }
            return events;
        }
    }
}

