namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Security;

    public sealed class SerializationObjectManager
    {
        private StreamingContext m_context;
        private Hashtable m_objectSeenTable = new Hashtable();
        private SerializationEventHandler m_onSerializedHandler;

        public SerializationObjectManager(StreamingContext context)
        {
            this.m_context = context;
            this.m_objectSeenTable = new Hashtable();
        }

        private void AddOnSerialized(object obj)
        {
            this.m_onSerializedHandler = SerializationEventsCache.GetSerializationEventsForType(obj.GetType()).AddOnSerialized(obj, this.m_onSerializedHandler);
        }

        public void RaiseOnSerializedEvent()
        {
            if (this.m_onSerializedHandler != null)
            {
                this.m_onSerializedHandler(this.m_context);
            }
        }

        [SecurityCritical]
        public void RegisterObject(object obj)
        {
            SerializationEvents serializationEventsForType = SerializationEventsCache.GetSerializationEventsForType(obj.GetType());
            if (serializationEventsForType.HasOnSerializingEvents && (this.m_objectSeenTable[obj] == null))
            {
                this.m_objectSeenTable[obj] = true;
                serializationEventsForType.InvokeOnSerializing(obj, this.m_context);
                this.AddOnSerialized(obj);
            }
        }
    }
}

