namespace System.Web.Management
{
    using System;

    public sealed class WebEventBufferFlushInfo
    {
        private WebBaseEventCollection _events;
        private int _eventsDiscardedSinceLastNotification;
        private int _eventsInBuffer;
        private DateTime _lastNotification;
        private int _notificationSequence;
        private EventNotificationType _notificationType;

        internal WebEventBufferFlushInfo(WebBaseEventCollection events, EventNotificationType notificationType, int notificationSequence, DateTime lastNotification, int eventsDiscardedSinceLastNotification, int eventsInBuffer)
        {
            this._events = events;
            this._notificationType = notificationType;
            this._notificationSequence = notificationSequence;
            this._lastNotification = lastNotification;
            this._eventsDiscardedSinceLastNotification = eventsDiscardedSinceLastNotification;
            this._eventsInBuffer = eventsInBuffer;
        }

        public WebBaseEventCollection Events
        {
            get
            {
                return this._events;
            }
        }

        public int EventsDiscardedSinceLastNotification
        {
            get
            {
                return this._eventsDiscardedSinceLastNotification;
            }
        }

        public int EventsInBuffer
        {
            get
            {
                return this._eventsInBuffer;
            }
        }

        public DateTime LastNotificationUtc
        {
            get
            {
                return this._lastNotification;
            }
        }

        public int NotificationSequence
        {
            get
            {
                return this._notificationSequence;
            }
        }

        public EventNotificationType NotificationType
        {
            get
            {
                return this._notificationType;
            }
        }
    }
}

