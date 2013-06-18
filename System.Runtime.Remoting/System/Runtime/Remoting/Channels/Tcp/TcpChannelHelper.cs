namespace System.Runtime.Remoting.Channels.Tcp
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;

    internal static class TcpChannelHelper
    {
        private const string _tcp = "tcp://";

        internal static string ParseURL(string url, out string objectURI)
        {
            int index;
            objectURI = null;
            if (StringHelper.StartsWithAsciiIgnoreCasePrefixLower(url, "tcp://"))
            {
                index = "tcp://".Length;
            }
            else
            {
                return null;
            }
            index = url.IndexOf('/', index);
            if (-1 == index)
            {
                return url;
            }
            string str = url.Substring(0, index);
            objectURI = url.Substring(index);
            return str;
        }
    }
}

