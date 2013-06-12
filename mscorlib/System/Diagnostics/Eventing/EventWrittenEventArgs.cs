namespace System.Diagnostics.Eventing
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class EventWrittenEventArgs : EventArgs
    {
        private EventProviderBase m_providerBase;

        internal EventWrittenEventArgs(EventProviderBase providerBase)
        {
            this.m_providerBase = providerBase;
        }

        public EventDescriptorInternal Descriptor
        {
            get
            {
                return this.m_providerBase.m_eventData[this.EventId].Descriptor;
            }
        }

        public int EventId { get; internal set; }

        public EventLevel Level
        {
            get
            {
                if (this.EventId >= this.m_providerBase.m_eventData.Length)
                {
                    return EventLevel.LogAlways;
                }
                return (EventLevel) this.m_providerBase.m_eventData[this.EventId].Descriptor.Level;
            }
        }

        public string Message
        {
            get
            {
                string format = null;
                if (this.EventId < this.m_providerBase.m_eventData.Length)
                {
                    format = this.m_providerBase.m_eventData[this.EventId].Message;
                }
                if (format != null)
                {
                    return string.Format(CultureInfo.InvariantCulture, format, this.Payload);
                }
                if (((this.EventId == 0) && (this.Payload.Length == 1)) && (this.Payload[0].GetType() == typeof(string)))
                {
                    return this.Payload[0].ToString();
                }
                return null;
            }
        }

        public object[] Payload { get; internal set; }

        public EventProviderBase Provider
        {
            get
            {
                return this.m_providerBase;
            }
        }
    }
}

