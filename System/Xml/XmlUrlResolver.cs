namespace System.Xml
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Security.Permissions;
    using System.Threading;

    public class XmlUrlResolver : XmlResolver
    {
        private RequestCachePolicy _cachePolicy;
        private ICredentials _credentials;
        private IWebProxy _proxy;
        private static object s_DownloadManager;

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (((ofObjectToReturn != null) && !(ofObjectToReturn == typeof(Stream))) && !(ofObjectToReturn == typeof(object)))
            {
                throw new XmlException("Xml_UnsupportedClass", string.Empty);
            }
            return DownloadManager.GetStream(absoluteUri, this._credentials, this._proxy, this._cachePolicy);
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return base.ResolveUri(baseUri, relativeUri);
        }

        public RequestCachePolicy CachePolicy
        {
            set
            {
                this._cachePolicy = value;
            }
        }

        public override ICredentials Credentials
        {
            set
            {
                this._credentials = value;
            }
        }

        private static XmlDownloadManager DownloadManager
        {
            get
            {
                if (s_DownloadManager == null)
                {
                    object obj2 = new XmlDownloadManager();
                    Interlocked.CompareExchange<object>(ref s_DownloadManager, obj2, null);
                }
                return (XmlDownloadManager) s_DownloadManager;
            }
        }

        public IWebProxy Proxy
        {
            set
            {
                this._proxy = value;
            }
        }
    }
}

