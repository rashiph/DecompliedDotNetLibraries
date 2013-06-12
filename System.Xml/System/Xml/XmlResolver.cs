namespace System.Xml
{
    using System;
    using System.IO;
    using System.Net;

    public abstract class XmlResolver
    {
        protected XmlResolver()
        {
        }

        public abstract object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn);
        public virtual Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if ((baseUri == null) || (!baseUri.IsAbsoluteUri && (baseUri.OriginalString.Length == 0)))
            {
                Uri uri = new Uri(relativeUri, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri && (uri.OriginalString.Length > 0))
                {
                    uri = new Uri(Path.GetFullPath(relativeUri));
                }
                return uri;
            }
            if ((relativeUri == null) || (relativeUri.Length == 0))
            {
                return baseUri;
            }
            if (!baseUri.IsAbsoluteUri)
            {
                throw new NotSupportedException(Res.GetString("Xml_RelativeUriNotSupported"));
            }
            return new Uri(baseUri, relativeUri);
        }

        public virtual bool SupportsType(Uri absoluteUri, Type type)
        {
            if (absoluteUri == null)
            {
                throw new ArgumentNullException("absoluteUri");
            }
            if ((type != null) && !(type == typeof(Stream)))
            {
                return false;
            }
            return true;
        }

        public virtual ICredentials Credentials
        {
            set
            {
            }
        }
    }
}

