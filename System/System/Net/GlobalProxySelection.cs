namespace System.Net
{
    using System;

    [Obsolete("This class has been deprecated. Please use WebRequest.DefaultWebProxy instead to access and set the global default proxy. Use 'null' instead of GetEmptyWebProxy. http://go.microsoft.com/fwlink/?linkid=14202")]
    public class GlobalProxySelection
    {
        public static IWebProxy GetEmptyWebProxy()
        {
            return new EmptyWebProxy();
        }

        public static IWebProxy Select
        {
            get
            {
                IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                if (defaultWebProxy == null)
                {
                    return GetEmptyWebProxy();
                }
                WebRequest.WebProxyWrapper wrapper = defaultWebProxy as WebRequest.WebProxyWrapper;
                if (wrapper != null)
                {
                    return wrapper.WebProxy;
                }
                return defaultWebProxy;
            }
            set
            {
                WebRequest.DefaultWebProxy = value;
            }
        }
    }
}

