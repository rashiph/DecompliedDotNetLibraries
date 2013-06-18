namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net.Mail;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Util;

    public sealed class SimpleMailWebEventProvider : MailWebEventProvider, IInternalWebEventProvider
    {
        private string _bodyFooter;
        private string _bodyHeader;
        private int _maxEventLength = 0x2000;
        private int _nonBufferNotificationSequence;
        private string _separator = "---------------\n";
        private const int DefaultMaxEventLength = 0x2000;
        private const int MessageIdDiscard = 100;
        private const int MessageIdEventsToDrop = 0x65;
        private static string s_header_app_info = System.Web.SR.GetString("MailWebEventProvider_Application_Info");
        private static string s_header_events = System.Web.SR.GetString("MailWebEventProvider_Events");
        private static string s_header_summary = System.Web.SR.GetString("MailWebEventProvider_Summary");
        private static string s_header_warnings = System.Web.SR.GetString("MailWebEventProvider_Warnings");

        internal SimpleMailWebEventProvider()
        {
        }

        private void GenerateApplicationInformation(StringBuilder sb)
        {
            sb.Append(s_header_app_info);
            sb.Append("\n");
            sb.Append(this._separator);
            sb.Append(WebBaseEvent.ApplicationInformation.ToString());
            sb.Append("\n\n");
        }

        private string GenerateBody(WebBaseEventCollection events, int begin, DateTime lastFlush, int discardedSinceLastFlush, int eventsInBuffer, int messageSequence, int eventsInNotification, int eventsLostDueToMessageLimit)
        {
            StringBuilder sb = new StringBuilder();
            int count = events.Count;
            if (this._bodyHeader != null)
            {
                sb.Append(this._bodyHeader);
            }
            this.GenerateWarnings(sb, lastFlush, discardedSinceLastFlush, messageSequence, eventsLostDueToMessageLimit);
            this.GenerateSummary(sb, begin, (begin + count) - 1, eventsInNotification, eventsInBuffer);
            this.GenerateApplicationInformation(sb);
            for (int i = 0; i < count; i++)
            {
                string str = events[i].ToString(false, true);
                if ((this._maxEventLength != 0x7fffffff) && (str.Length > this._maxEventLength))
                {
                    str = str.Substring(0, this._maxEventLength);
                }
                if (i == 0)
                {
                    sb.Append(s_header_events);
                    sb.Append("\n");
                    sb.Append(this._separator);
                }
                sb.Append(str);
                sb.Append("\n");
                sb.Append(this._separator);
            }
            if (this._bodyFooter != null)
            {
                sb.Append(this._bodyFooter);
            }
            return sb.ToString();
        }

        private void GenerateSummary(StringBuilder sb, int firstEvent, int lastEvent, int eventsInNotif, int eventsInBuffer)
        {
            if (base.UseBuffering)
            {
                sb.Append(s_header_summary);
                sb.Append("\n");
                sb.Append(this._separator);
                firstEvent++;
                lastEvent++;
                sb.Append(System.Web.SR.GetString("MailWebEventProvider_summary_body", new object[] { firstEvent.ToString(CultureInfo.InstalledUICulture), lastEvent.ToString(CultureInfo.InstalledUICulture), eventsInNotif.ToString(CultureInfo.InstalledUICulture), eventsInBuffer.ToString(CultureInfo.InstalledUICulture) }));
                sb.Append("\n\n");
                sb.Append("\n");
            }
        }

        private void GenerateWarnings(StringBuilder sb, DateTime lastFlush, int discardedSinceLastFlush, int seq, int eventsToDrop)
        {
            if (base.UseBuffering)
            {
                bool flag = false;
                bool flag2 = false;
                if ((discardedSinceLastFlush != 0) && (seq == 1))
                {
                    sb.Append(s_header_warnings);
                    sb.Append("\n");
                    sb.Append(this._separator);
                    flag = true;
                    object[] args = new object[] { 100.ToString(CultureInfo.InstalledUICulture), discardedSinceLastFlush.ToString(CultureInfo.InstalledUICulture), lastFlush.ToString("r", CultureInfo.InstalledUICulture) };
                    sb.Append(System.Web.SR.GetString("MailWebEventProvider_discard_warning", args));
                    sb.Append("\n\n");
                    flag2 = true;
                }
                if (eventsToDrop > 0)
                {
                    if (!flag)
                    {
                        sb.Append(s_header_warnings);
                        sb.Append("\n");
                        sb.Append(this._separator);
                        flag = true;
                    }
                    object[] objArray2 = new object[] { 0x65.ToString(CultureInfo.InstalledUICulture), eventsToDrop.ToString(CultureInfo.InstalledUICulture) };
                    sb.Append(System.Web.SR.GetString("MailWebEventProvider_events_drop_warning", objArray2));
                    sb.Append("\n\n");
                    flag2 = true;
                }
                if (flag2)
                {
                    sb.Append("\n");
                }
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            string val = null;
            ProviderUtil.GetAndRemoveStringAttribute(config, "bodyHeader", name, ref this._bodyHeader);
            if (this._bodyHeader != null)
            {
                this._bodyHeader = this._bodyHeader + "\n";
            }
            ProviderUtil.GetAndRemoveStringAttribute(config, "bodyFooter", name, ref this._bodyFooter);
            if (this._bodyFooter != null)
            {
                this._bodyFooter = this._bodyFooter + "\n";
            }
            ProviderUtil.GetAndRemoveStringAttribute(config, "separator", name, ref val);
            if (val != null)
            {
                this._separator = val + "\n";
            }
            ProviderUtil.GetAndRemovePositiveOrInfiniteAttribute(config, "maxEventLength", name, ref this._maxEventLength);
            base.Initialize(name, config);
        }

        internal override void SendMessage(WebBaseEvent eventRaised)
        {
            WebBaseEventCollection events = new WebBaseEventCollection(eventRaised);
            this.SendMessageInternal(events, Interlocked.Increment(ref this._nonBufferNotificationSequence), 0, DateTime.MinValue, 0, 0, 1, 1, 1, 0);
        }

        internal override void SendMessage(WebBaseEventCollection events, WebEventBufferFlushInfo flushInfo, int eventsInNotification, int eventsRemaining, int messagesInNotification, int eventsLostDueToMessageLimit, int messageSequence, int eventsSent, out bool fatalError)
        {
            this.SendMessageInternal(events, flushInfo.NotificationSequence, eventsSent, flushInfo.LastNotificationUtc, flushInfo.EventsDiscardedSinceLastNotification, flushInfo.EventsInBuffer, messageSequence, messagesInNotification, eventsInNotification, eventsLostDueToMessageLimit);
            fatalError = false;
        }

        private void SendMessageInternal(WebBaseEventCollection events, int notificationSequence, int begin, DateTime lastFlush, int discardedSinceLastFlush, int eventsInBuffer, int messageSequence, int messagesInNotification, int eventsInNotification, int eventsLostDueToMessageLimit)
        {
            using (MailMessage message = base.GetMessage())
            {
                if (messageSequence != messagesInNotification)
                {
                    eventsLostDueToMessageLimit = 0;
                }
                message.Body = this.GenerateBody(events, begin, lastFlush, discardedSinceLastFlush, eventsInBuffer, messageSequence, eventsInNotification, eventsLostDueToMessageLimit);
                message.Subject = base.GenerateSubject(notificationSequence, messageSequence, events, events.Count);
                base.SendMail(message);
            }
        }
    }
}

