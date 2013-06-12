namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    public sealed class TemplatedMailWebEventProvider : MailWebEventProvider, IInternalWebEventProvider
    {
        private bool _detailedTemplateErrors;
        private int _nonBufferNotificationSequence;
        private string _templateUrl;
        internal const string CurrentEventsName = "_TWCurEvt";

        internal TemplatedMailWebEventProvider()
        {
        }

        private void GenerateMessageBody(MailMessage msg, WebBaseEventCollection events, DateTime lastNotificationUtc, int discardedSinceLastNotification, int eventsInBuffer, int notificationSequence, EventNotificationType notificationType, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence, out bool fatalError)
        {
            StringWriter writer = new StringWriter(CultureInfo.InstalledUICulture);
            MailEventNotificationInfo data = new MailEventNotificationInfo(msg, events, lastNotificationUtc, discardedSinceLastNotification, eventsInBuffer, notificationSequence, notificationType, eventsInNotification, eventsRemaining, messagesInNotification, eventsLostDueToMessageLimit, messageSequence);
            CallContext.SetData("_TWCurEvt", data);
            try
            {
                TemplatedMailErrorFormatterGenerator errorFormatterGenerator = new TemplatedMailErrorFormatterGenerator(events.Count + eventsRemaining, this._detailedTemplateErrors);
                HttpServerUtility.ExecuteLocalRequestAndCaptureResponse(this._templateUrl, writer, errorFormatterGenerator);
                fatalError = errorFormatterGenerator.ErrorFormatterCalled;
                if (fatalError)
                {
                    msg.Subject = HttpUtility.HtmlEncode(System.Web.SR.GetString("WebEvent_event_email_subject_template_error", new object[] { notificationSequence.ToString(CultureInfo.InstalledUICulture), messageSequence.ToString(CultureInfo.InstalledUICulture), base.SubjectPrefix }));
                }
                msg.Body = writer.ToString();
                msg.IsBodyHtml = true;
            }
            finally
            {
                CallContext.FreeNamedDataSlot("_TWCurEvt");
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            ProviderUtil.GetAndRemoveStringAttribute(config, "template", name, ref this._templateUrl);
            if (this._templateUrl == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Provider_missing_attribute", new object[] { "template", name }));
            }
            this._templateUrl = this._templateUrl.Trim();
            if (this._templateUrl.Length == 0)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_attribute", new object[] { "template", name, this._templateUrl }));
            }
            if (!System.Web.Util.UrlPath.IsRelativeUrl(this._templateUrl))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_mail_template_provider_attribute", new object[] { "template", name, this._templateUrl }));
            }
            this._templateUrl = System.Web.Util.UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, this._templateUrl);
            if (!HttpRuntime.IsPathWithinAppRoot(this._templateUrl))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_mail_template_provider_attribute", new object[] { "template", name, this._templateUrl }));
            }
            ProviderUtil.GetAndRemoveBooleanAttribute(config, "detailedTemplateErrors", name, ref this._detailedTemplateErrors);
            base.Initialize(name, config);
        }

        internal override void SendMessage(WebBaseEvent eventRaised)
        {
            bool flag;
            WebBaseEventCollection events = new WebBaseEventCollection(eventRaised);
            this.SendMessageInternal(events, DateTime.MinValue, 0, 0, Interlocked.Increment(ref this._nonBufferNotificationSequence), EventNotificationType.Unbuffered, 1, 0, 1, 0, 1, out flag);
        }

        internal override void SendMessage(WebBaseEventCollection events, WebEventBufferFlushInfo flushInfo, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence, int eventsSent, out bool fatalError)
        {
            this.SendMessageInternal(events, flushInfo.LastNotificationUtc, flushInfo.EventsDiscardedSinceLastNotification, flushInfo.EventsInBuffer, flushInfo.NotificationSequence, flushInfo.NotificationType, eventsInNotification, eventsRemaining, messagesInNotification, eventsLostDueToMessageLimit, messageSequence, out fatalError);
        }

        private void SendMessageInternal(WebBaseEventCollection events, DateTime lastNotificationUtc, int discardedSinceLastNotification, int eventsInBuffer, int notificationSequence, EventNotificationType notificationType, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence, out bool fatalError)
        {
            using (MailMessage message = base.GetMessage())
            {
                message.Subject = base.GenerateSubject(notificationSequence, messageSequence, events, events.Count);
                this.GenerateMessageBody(message, events, lastNotificationUtc, discardedSinceLastNotification, eventsInBuffer, notificationSequence, notificationType, eventsInNotification, eventsRemaining, messagesInNotification, eventsLostDueToMessageLimit, messageSequence, out fatalError);
                base.SendMail(message);
            }
        }

        public static MailEventNotificationInfo CurrentNotification
        {
            get
            {
                return (MailEventNotificationInfo) CallContext.GetData("_TWCurEvt");
            }
        }

        private class TemplatedMailErrorFormatterGenerator : ErrorFormatterGenerator
        {
            private bool _errorFormatterCalled;
            private int _eventsRemaining;
            private bool _showDetails;

            internal TemplatedMailErrorFormatterGenerator(int eventsRemaining, bool showDetails)
            {
                this._eventsRemaining = eventsRemaining;
                this._showDetails = showDetails;
            }

            internal override ErrorFormatter GetErrorFormatter(Exception e)
            {
                Exception innerException = e.InnerException;
                this._errorFormatterCalled = true;
                while (innerException != null)
                {
                    if (innerException is HttpCompileException)
                    {
                        return new TemplatedMailCompileErrorFormatter((HttpCompileException) innerException, this._eventsRemaining, this._showDetails);
                    }
                    innerException = innerException.InnerException;
                }
                return new TemplatedMailRuntimeErrorFormatter(e, this._eventsRemaining, this._showDetails);
            }

            internal bool ErrorFormatterCalled
            {
                get
                {
                    return this._errorFormatterCalled;
                }
            }
        }
    }
}

