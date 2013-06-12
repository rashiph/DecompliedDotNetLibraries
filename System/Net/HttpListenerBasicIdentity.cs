namespace System.Net
{
    using System;
    using System.Security.Principal;

    public class HttpListenerBasicIdentity : GenericIdentity
    {
        private string m_Password;

        public HttpListenerBasicIdentity(string username, string password) : base(username, "Basic")
        {
            this.m_Password = password;
        }

        public virtual string Password
        {
            get
            {
                return this.m_Password;
            }
        }
    }
}

