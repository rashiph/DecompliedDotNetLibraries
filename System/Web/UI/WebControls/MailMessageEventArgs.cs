namespace System.Web.UI.WebControls
{
    using System;
    using System.Net.Mail;

    public class MailMessageEventArgs : LoginCancelEventArgs
    {
        private MailMessage _message;

        public MailMessageEventArgs(MailMessage message)
        {
            this._message = message;
        }

        public MailMessage Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

