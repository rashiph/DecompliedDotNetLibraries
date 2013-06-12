namespace System.Net.NetworkInformation
{
    using System;
    using System.ComponentModel;

    public class PingCompletedEventArgs : AsyncCompletedEventArgs
    {
        private PingReply reply;

        internal PingCompletedEventArgs(PingReply reply, Exception error, bool cancelled, object userToken) : base(error, cancelled, userToken)
        {
            this.reply = reply;
        }

        public PingReply Reply
        {
            get
            {
                return this.reply;
            }
        }
    }
}

