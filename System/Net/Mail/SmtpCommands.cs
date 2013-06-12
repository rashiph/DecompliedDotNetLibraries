namespace System.Net.Mail
{
    using System;

    internal static class SmtpCommands
    {
        internal static readonly byte[] Auth = Encoding.ASCII.GetBytes("AUTH ");
        internal static readonly byte[] CRLF = Encoding.ASCII.GetBytes("\r\n");
        internal static readonly byte[] Data = Encoding.ASCII.GetBytes("DATA\r\n");
        internal static readonly byte[] DataStop = Encoding.ASCII.GetBytes("\r\n.\r\n");
        internal static readonly byte[] EHello = Encoding.ASCII.GetBytes("EHLO ");
        internal static readonly byte[] Expand = Encoding.ASCII.GetBytes("EXPN ");
        internal static readonly byte[] Hello = Encoding.ASCII.GetBytes("HELO ");
        internal static readonly byte[] Help = Encoding.ASCII.GetBytes("HELP");
        internal static readonly byte[] Mail = Encoding.ASCII.GetBytes("MAIL FROM:");
        internal static readonly byte[] Noop = Encoding.ASCII.GetBytes("NOOP\r\n");
        internal static readonly byte[] Quit = Encoding.ASCII.GetBytes("QUIT\r\n");
        internal static readonly byte[] Recipient = Encoding.ASCII.GetBytes("RCPT TO:");
        internal static readonly byte[] Reset = Encoding.ASCII.GetBytes("RSET\r\n");
        internal static readonly byte[] Send = Encoding.ASCII.GetBytes("SEND FROM:");
        internal static readonly byte[] SendAndMail = Encoding.ASCII.GetBytes("SAML FROM:");
        internal static readonly byte[] SendOrMail = Encoding.ASCII.GetBytes("SOML FROM:");
        internal static readonly byte[] StartTls = Encoding.ASCII.GetBytes("STARTTLS");
        internal static readonly byte[] Turn = Encoding.ASCII.GetBytes("TURN\r\n");
        internal static readonly byte[] Verify = Encoding.ASCII.GetBytes("VRFY ");
    }
}

