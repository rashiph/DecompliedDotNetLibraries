namespace System.Web.Management
{
    using System;
    using System.Net.Mail;

    public sealed class MailEventNotificationInfo
    {
        private int _discardedSinceLastNotification;
        private WebBaseEventCollection _events;
        private int _eventsInBuffer;
        private int _eventsInNotification;
        private int _eventsLostDueToMessageLimit;
        private int _eventsRemaining;
        private DateTime _lastNotificationUtc;
        private int _messageSequence;
        private int _messagesInNotification;
        private MailMessage _msg;
        private int _notificationSequence;
        private EventNotificationType _notificationType;

        internal MailEventNotificationInfo(MailMessage msg, WebBaseEventCollection events, DateTime lastNotificationUtc, int discardedSinceLastNotification, int eventsInBuffer, int notificationSequence, EventNotificationType notificationType, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence)
        {
            this._events = events;
            this._lastNotificationUtc = lastNotificationUtc;
            this._discardedSinceLastNotification = discardedSinceLastNotification;
            this._eventsInBuffer = eventsInBuffer;
            this._notificationSequence = notificationSequence;
            this._notificationType = notificationType;
            this._eventsInNotification = eventsInNotification;
            this._eventsRemaining = eventsRemaining;
            this._messagesInNotification = messagesInNotification;
            this._eventsLostDueToMessageLimit = eventsLostDueToMessageLimit;
            this._messageSequence = messageSequence;
            this._msg = msg;
        }

        public WebBaseEventCollection Events
        {
            get
            {
                return this._events;
            }
        }

        public int EventsDiscardedByBuffer
        {
            get
            {
                return this._discardedSinceLastNotification;
            }
        }

        public int EventsDiscardedDueToMessageLimit
        {
            get
            {
                return this._eventsLostDueToMessageLimit;
            }
        }

        public int EventsInBuffer
        {
            get
            {
                return this._eventsInBuffer;
            }
        }

        public int EventsInNotification
        {
            get
            {
                return this._eventsInNotification;
            }
        }

        public int EventsRemaining
        {
            get
            {
                return this._eventsRemaining;
            }
        }

        public DateTime LastNotificationUtc
        {
            get
            {
                return this._lastNotificationUtc;
            }
        }

        public MailMessage Message
        {
            get
            {
                return this._msg;
            }
        }

        public int MessageSequence
        {
            get
            {
                return this._messageSequence;
            }
        }

        public int MessagesInNotification
        {
            get
            {
                return this._messagesInNotification;
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

