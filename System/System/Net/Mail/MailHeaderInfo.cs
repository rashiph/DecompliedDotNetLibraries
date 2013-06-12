namespace System.Net.Mail
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class MailHeaderInfo
    {
        private static readonly Dictionary<string, int> m_HeaderDictionary = new Dictionary<string, int>(0x21, StringComparer.OrdinalIgnoreCase);
        private static readonly HeaderInfo[] m_HeaderInfo = new HeaderInfo[] { 
            new HeaderInfo(MailHeaderID.Bcc, "Bcc", true, false), new HeaderInfo(MailHeaderID.Cc, "Cc", true, false), new HeaderInfo(MailHeaderID.Comments, "Comments", false, true), new HeaderInfo(MailHeaderID.ContentDescription, "Content-Description", true, true), new HeaderInfo(MailHeaderID.ContentDisposition, "Content-Disposition", true, true), new HeaderInfo(MailHeaderID.ContentID, "Content-ID", true, false), new HeaderInfo(MailHeaderID.ContentLocation, "Content-Location", true, false), new HeaderInfo(MailHeaderID.ContentTransferEncoding, "Content-Transfer-Encoding", true, false), new HeaderInfo(MailHeaderID.ContentType, "Content-Type", true, false), new HeaderInfo(MailHeaderID.Date, "Date", true, false), new HeaderInfo(MailHeaderID.From, "From", true, false), new HeaderInfo(MailHeaderID.Importance, "Importance", true, false), new HeaderInfo(MailHeaderID.InReplyTo, "In-Reply-To", true, true), new HeaderInfo(MailHeaderID.Keywords, "Keywords", false, true), new HeaderInfo(MailHeaderID.Max, "Max", false, true), new HeaderInfo(MailHeaderID.MessageID, "Message-ID", true, true), 
            new HeaderInfo(MailHeaderID.MimeVersion, "MIME-Version", true, false), new HeaderInfo(MailHeaderID.Priority, "Priority", true, false), new HeaderInfo(MailHeaderID.References, "References", true, true), new HeaderInfo(MailHeaderID.ReplyTo, "Reply-To", true, false), new HeaderInfo(MailHeaderID.ResentBcc, "Resent-Bcc", false, true), new HeaderInfo(MailHeaderID.ResentCc, "Resent-Cc", false, true), new HeaderInfo(MailHeaderID.ResentDate, "Resent-Date", false, true), new HeaderInfo(MailHeaderID.ResentFrom, "Resent-From", false, true), new HeaderInfo(MailHeaderID.ResentMessageID, "Resent-Message-ID", false, true), new HeaderInfo(MailHeaderID.ResentSender, "Resent-Sender", false, true), new HeaderInfo(MailHeaderID.ResentTo, "Resent-To", false, true), new HeaderInfo(MailHeaderID.Sender, "Sender", true, false), new HeaderInfo(MailHeaderID.Subject, "Subject", true, false), new HeaderInfo(MailHeaderID.To, "To", true, false), new HeaderInfo(MailHeaderID.XPriority, "X-Priority", true, false), new HeaderInfo(MailHeaderID.XReceiver, "X-Receiver", false, true), 
            new HeaderInfo(MailHeaderID.XSender, "X-Sender", true, true)
         };

        static MailHeaderInfo()
        {
            for (int i = 0; i < m_HeaderInfo.Length; i++)
            {
                m_HeaderDictionary.Add(m_HeaderInfo[i].NormalizedName, i);
            }
        }

        internal static MailHeaderID GetID(string name)
        {
            int num;
            if (m_HeaderDictionary.TryGetValue(name, out num))
            {
                return (MailHeaderID) num;
            }
            return MailHeaderID.Unknown;
        }

        internal static string GetString(MailHeaderID id)
        {
            MailHeaderID rid = id;
            if ((rid != MailHeaderID.Unknown) && (rid != (MailHeaderID.XSender | MailHeaderID.Cc)))
            {
                return m_HeaderInfo[(int) id].NormalizedName;
            }
            return null;
        }

        internal static bool IsMatch(string name, MailHeaderID header)
        {
            int num;
            return (m_HeaderDictionary.TryGetValue(name, out num) && (num == header));
        }

        internal static bool IsSingleton(string name)
        {
            int num;
            return (m_HeaderDictionary.TryGetValue(name, out num) && m_HeaderInfo[num].IsSingleton);
        }

        internal static bool IsUserSettable(string name)
        {
            int num;
            if (m_HeaderDictionary.TryGetValue(name, out num))
            {
                return m_HeaderInfo[num].IsUserSettable;
            }
            return true;
        }

        internal static bool IsWellKnown(string name)
        {
            int num;
            return m_HeaderDictionary.TryGetValue(name, out num);
        }

        internal static string NormalizeCase(string name)
        {
            int num;
            if (m_HeaderDictionary.TryGetValue(name, out num))
            {
                return m_HeaderInfo[num].NormalizedName;
            }
            return name;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HeaderInfo
        {
            public readonly string NormalizedName;
            public readonly bool IsSingleton;
            public readonly MailHeaderID ID;
            public readonly bool IsUserSettable;
            public HeaderInfo(MailHeaderID id, string name, bool isSingleton, bool isUserSettable)
            {
                this.ID = id;
                this.NormalizedName = name;
                this.IsSingleton = isSingleton;
                this.IsUserSettable = isUserSettable;
            }
        }
    }
}

