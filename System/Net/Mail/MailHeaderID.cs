namespace System.Net.Mail
{
    using System;

    internal enum MailHeaderID
    {
        Bcc = 0,
        Cc = 1,
        Comments = 2,
        ContentDescription = 3,
        ContentDisposition = 4,
        ContentID = 5,
        ContentLocation = 6,
        ContentTransferEncoding = 7,
        ContentType = 8,
        Date = 9,
        From = 10,
        Importance = 11,
        InReplyTo = 12,
        Keywords = 13,
        Max = 14,
        MessageID = 15,
        MimeVersion = 0x10,
        Priority = 0x11,
        References = 0x12,
        ReplyTo = 0x13,
        ResentBcc = 20,
        ResentCc = 0x15,
        ResentDate = 0x16,
        ResentFrom = 0x17,
        ResentMessageID = 0x18,
        ResentSender = 0x19,
        ResentTo = 0x1a,
        Sender = 0x1b,
        Subject = 0x1c,
        To = 0x1d,
        Unknown = -1,
        XPriority = 30,
        XReceiver = 0x1f,
        XSender = 0x20,
        ZMaxEnumValue = 0x20
    }
}

