namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Net.Mail;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public abstract class MailWebEventProvider : BufferedWebEventProvider
    {
        private string _bcc;
        private string _cc;
        private string _from;
        private int _maxEventsPerMessage = 50;
        private int _maxMessagesPerNotification = 10;
        private SmtpClient _smtpClient;
        private string _subjectPrefix;
        private string _to;
        internal const int DefaultMaxEventsPerMessage = 50;
        internal const int DefaultMaxMessagesPerNotification = 10;
        internal const int MessageSequenceBase = 1;

        internal MailWebEventProvider()
        {
        }

        [EnvironmentPermission(SecurityAction.Assert, Read="USERNAME"), SmtpPermission(SecurityAction.Assert, Access="Connect")]
        internal static SmtpClient CreateSmtpClientWithAssert()
        {
            return new SmtpClient();
        }

        internal string GenerateSubject(int notificationSequence, int messageSequence, WebBaseEventCollection events, int count)
        {
            WebBaseEvent event2 = events[0];
            if (count == 1)
            {
                return HttpUtility.HtmlEncode(System.Web.SR.GetString("WebEvent_event_email_subject", new string[] { notificationSequence.ToString(CultureInfo.InstalledUICulture), messageSequence.ToString(CultureInfo.InstalledUICulture), this._subjectPrefix, event2.GetType().ToString(), WebBaseEvent.ApplicationInformation.ApplicationVirtualPath }));
            }
            return HttpUtility.HtmlEncode(System.Web.SR.GetString("WebEvent_event_group_email_subject", new string[] { notificationSequence.ToString(CultureInfo.InstalledUICulture), messageSequence.ToString(CultureInfo.InstalledUICulture), this._subjectPrefix, count.ToString(CultureInfo.InstalledUICulture), WebBaseEvent.ApplicationInformation.ApplicationVirtualPath }));
        }

        internal MailMessage GetMessage()
        {
            MailMessage message = new MailMessage(this._from, this._to);
            if (!string.IsNullOrEmpty(this._cc))
            {
                message.CC.Add(new MailAddress(this._cc));
            }
            if (!string.IsNullOrEmpty(this._bcc))
            {
                message.Bcc.Add(new MailAddress(this._bcc));
            }
            return message;
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            ProviderUtil.GetAndRemoveRequiredNonEmptyStringAttribute(config, "from", name, ref this._from);
            ProviderUtil.GetAndRemoveStringAttribute(config, "to", name, ref this._to);
            ProviderUtil.GetAndRemoveStringAttribute(config, "cc", name, ref this._cc);
            ProviderUtil.GetAndRemoveStringAttribute(config, "bcc", name, ref this._bcc);
            if ((string.IsNullOrEmpty(this._to) && string.IsNullOrEmpty(this._cc)) && string.IsNullOrEmpty(this._bcc))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("MailWebEventProvider_no_recipient_error", new object[] { base.GetType().ToString(), name }));
            }
            ProviderUtil.GetAndRemoveStringAttribute(config, "subjectPrefix", name, ref this._subjectPrefix);
            ProviderUtil.GetAndRemoveNonZeroPositiveOrInfiniteAttribute(config, "maxMessagesPerNotification", name, ref this._maxMessagesPerNotification);
            ProviderUtil.GetAndRemoveNonZeroPositiveOrInfiniteAttribute(config, "maxEventsPerMessage", name, ref this._maxEventsPerMessage);
            this._smtpClient = CreateSmtpClientWithAssert();
            base.Initialize(name, config);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (base.UseBuffering)
            {
                base.ProcessEvent(eventRaised);
            }
            else
            {
                this.SendMessage(eventRaised);
            }
        }

        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo)
        {
            int count = flushInfo.Events.Count;
            bool flag = false;
            int messageSequence = 1;
            int eventsLostDueToMessageLimit = 0;
            bool fatalError = false;
            if (count != 0)
            {
                int maxMessagesPerNotification;
                WebBaseEvent[] eventArray = null;
                if (count > this.MaxEventsPerMessage)
                {
                    flag = true;
                    maxMessagesPerNotification = count / this.MaxEventsPerMessage;
                    if (count > (maxMessagesPerNotification * this.MaxEventsPerMessage))
                    {
                        maxMessagesPerNotification++;
                    }
                    if (maxMessagesPerNotification > this.MaxMessagesPerNotification)
                    {
                        eventsLostDueToMessageLimit = count - (this.MaxMessagesPerNotification * this.MaxEventsPerMessage);
                        maxMessagesPerNotification = this.MaxMessagesPerNotification;
                        count -= eventsLostDueToMessageLimit;
                    }
                }
                else
                {
                    maxMessagesPerNotification = 1;
                }
                int eventsSent = 0;
                while (eventsSent < count)
                {
                    WebBaseEventCollection events;
                    if (flag)
                    {
                        int num6 = Math.Min(this.MaxEventsPerMessage, count - eventsSent);
                        if ((eventArray == null) || (eventArray.Length != num6))
                        {
                            eventArray = new WebBaseEvent[num6];
                        }
                        for (int i = 0; i < num6; i++)
                        {
                            eventArray[i] = flushInfo.Events[i + eventsSent];
                        }
                        events = new WebBaseEventCollection(eventArray);
                    }
                    else
                    {
                        events = flushInfo.Events;
                    }
                    this.SendMessage(events, flushInfo, count, count - (eventsSent + events.Count), maxMessagesPerNotification, eventsLostDueToMessageLimit, messageSequence, eventsSent, out fatalError);
                    if (fatalError)
                    {
                        return;
                    }
                    eventsSent += events.Count;
                    messageSequence++;
                }
            }
        }

        [SmtpPermission(SecurityAction.Assert, Access="Connect")]
        internal void SendMail(MailMessage msg)
        {
            try
            {
                this._smtpClient.Send(msg);
            }
            catch (Exception exception)
            {
                throw new HttpException(System.Web.SR.GetString("MailWebEventProvider_cannot_send_mail"), exception);
            }
        }

        internal abstract void SendMessage(WebBaseEvent eventRaised);
        internal abstract void SendMessage(WebBaseEventCollection events, WebEventBufferFlushInfo flushInfo, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence, int eventsSent, out bool fatalError);
        public override void Shutdown()
        {
            this.Flush();
        }

        internal int MaxEventsPerMessage
        {
            get
            {
                return this._maxEventsPerMessage;
            }
        }

        internal int MaxMessagesPerNotification
        {
            get
            {
                return this._maxMessagesPerNotification;
            }
        }

        internal string SubjectPrefix
        {
            get
            {
                return this._subjectPrefix;
            }
        }
    }
}

