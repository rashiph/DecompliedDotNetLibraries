namespace System.Xml
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Net.Cache;

    internal class XmlDownloadManager
    {
        private Hashtable connections;

        private Stream GetNonFileStream(Uri uri, ICredentials credentials, IWebProxy proxy, RequestCachePolicy cachePolicy)
        {
            WebRequest request = WebRequest.Create(uri);
            if (credentials != null)
            {
                request.Credentials = credentials;
            }
            if (proxy != null)
            {
                request.Proxy = proxy;
            }
            if (cachePolicy != null)
            {
                request.CachePolicy = cachePolicy;
            }
            WebResponse response = request.GetResponse();
            HttpWebRequest request2 = request as HttpWebRequest;
            if (request2 != null)
            {
                lock (this)
                {
                    if (this.connections == null)
                    {
                        this.connections = new Hashtable();
                    }
                    OpenedHost host = (OpenedHost) this.connections[request2.Address.Host];
                    if (host == null)
                    {
                        host = new OpenedHost();
                    }
                    if (host.nonCachedConnectionsCount < (request2.ServicePoint.ConnectionLimit - 1))
                    {
                        if (host.nonCachedConnectionsCount == 0)
                        {
                            this.connections.Add(request2.Address.Host, host);
                        }
                        host.nonCachedConnectionsCount++;
                        return new XmlRegisteredNonCachedStream(response.GetResponseStream(), this, request2.Address.Host);
                    }
                    return new XmlCachedStream(response.ResponseUri, response.GetResponseStream());
                }
            }
            return response.GetResponseStream();
        }

        internal Stream GetStream(Uri uri, ICredentials credentials, IWebProxy proxy, RequestCachePolicy cachePolicy)
        {
            if (uri.Scheme == "file")
            {
                return new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1);
            }
            return this.GetNonFileStream(uri, credentials, proxy, cachePolicy);
        }

        internal void Remove(string host)
        {
            lock (this)
            {
                OpenedHost host2 = (OpenedHost) this.connections[host];
                if ((host2 != null) && (--host2.nonCachedConnectionsCount == 0))
                {
                    this.connections.Remove(host);
                }
            }
        }
    }
}

