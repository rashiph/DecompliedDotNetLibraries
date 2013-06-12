namespace System.Net.Mail
{
    using System;
    using System.Net;

    internal class SmtpPooledStream : PooledStream
    {
        internal ICredentialsByHost creds;
        internal bool dsnEnabled;
        internal bool previouslyUsed;
        private const int safeBufferLength = 80;

        internal SmtpPooledStream(ConnectionPool connectionPool, TimeSpan lifetime, bool checkLifetime) : base(connectionPool, lifetime, checkLifetime)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "SmtpPooledStream::Dispose #" + ValidationHelper.HashString(this));
            }
            if (disposing && base.NetworkStream.Connected)
            {
                this.Write(SmtpCommands.Quit, 0, SmtpCommands.Quit.Length);
                this.Flush();
                byte[] buffer = new byte[80];
                this.Read(buffer, 0, 80);
            }
            base.Dispose(disposing);
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, "SmtpPooledStream::Dispose #" + ValidationHelper.HashString(this));
            }
        }
    }
}

