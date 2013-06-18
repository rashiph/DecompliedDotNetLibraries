namespace System.Runtime.Remoting.Services
{
    using System.ComponentModel;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Security.Principal;
    using System.Web;
    using System.Web.SessionState;

    public class RemotingService : Component
    {
        public HttpApplicationState Application
        {
            get
            {
                return this.Context.Application;
            }
        }

        public HttpContext Context
        {
            get
            {
                HttpContext current = HttpContext.Current;
                if (current == null)
                {
                    throw new RemotingException(CoreChannel.GetResourceString("Remoting_HttpContextNotAvailable"));
                }
                return current;
            }
        }

        public HttpServerUtility Server
        {
            get
            {
                return this.Context.Server;
            }
        }

        public HttpSessionState Session
        {
            get
            {
                return this.Context.Session;
            }
        }

        public IPrincipal User
        {
            get
            {
                return this.Context.User;
            }
        }
    }
}

