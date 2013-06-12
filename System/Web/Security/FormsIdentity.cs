namespace System.Web.Security
{
    using System;
    using System.Security.Principal;

    [Serializable]
    public class FormsIdentity : IIdentity
    {
        private FormsAuthenticationTicket _Ticket;

        public FormsIdentity(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            this._Ticket = ticket;
        }

        public string AuthenticationType
        {
            get
            {
                return "Forms";
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return true;
            }
        }

        public string Name
        {
            get
            {
                return this._Ticket.Name;
            }
        }

        public FormsAuthenticationTicket Ticket
        {
            get
            {
                return this._Ticket;
            }
        }
    }
}

