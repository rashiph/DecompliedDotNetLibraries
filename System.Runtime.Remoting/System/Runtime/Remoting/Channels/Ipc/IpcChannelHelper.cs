namespace System.Runtime.Remoting.Channels.Ipc
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;

    internal static class IpcChannelHelper
    {
        private const string _ipc = "ipc://";

        internal static string ParseURL(string url, out string objectURI)
        {
            int index;
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            objectURI = null;
            if (StartsWithIpc(url))
            {
                index = "ipc://".Length;
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

        internal static bool StartsWithIpc(string url)
        {
            return StringHelper.StartsWithAsciiIgnoreCasePrefixLower(url, "ipc://");
        }
    }
}

