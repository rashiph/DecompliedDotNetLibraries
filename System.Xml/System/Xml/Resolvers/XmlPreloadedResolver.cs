namespace System.Xml.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public class XmlPreloadedResolver : XmlResolver
    {
        private XmlResolver fallbackResolver;
        private Dictionary<Uri, PreloadedData> mappings;
        private XmlKnownDtds preloadedDtds;
        private static XmlKnownDtdData[] Rss091_Dtd = new XmlKnownDtdData[] { new XmlKnownDtdData("-//Netscape Communications//DTD RSS 0.91//EN", "http://my.netscape.com/publish/formats/rss-0.91.dtd", "rss-0.91.dtd") };
        private static XmlKnownDtdData[] Xhtml10_Dtd = new XmlKnownDtdData[] { new XmlKnownDtdData("-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", "xhtml1-strict.dtd"), new XmlKnownDtdData("-//W3C//DTD XHTML 1.0 Transitional//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", "xhtml1-transitional.dtd"), new XmlKnownDtdData("-//W3C//DTD XHTML 1.0 Frameset//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd", "xhtml1-frameset.dtd"), new XmlKnownDtdData("-//W3C//ENTITIES Latin 1 for XHTML//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml-lat1.ent", "xhtml-lat1.ent"), new XmlKnownDtdData("-//W3C//ENTITIES Symbols for XHTML//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml-symbol.ent", "xhtml-symbol.ent"), new XmlKnownDtdData("-//W3C//ENTITIES Special for XHTML//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml-special.ent", "xhtml-special.ent") };

        public XmlPreloadedResolver() : this((XmlResolver) null)
        {
        }

        public XmlPreloadedResolver(XmlKnownDtds preloadedDtds) : this(null, preloadedDtds, null)
        {
        }

        public XmlPreloadedResolver(XmlResolver fallbackResolver) : this(fallbackResolver, XmlKnownDtds.All, null)
        {
        }

        public XmlPreloadedResolver(XmlResolver fallbackResolver, XmlKnownDtds preloadedDtds) : this(fallbackResolver, preloadedDtds, null)
        {
        }

        public XmlPreloadedResolver(XmlResolver fallbackResolver, XmlKnownDtds preloadedDtds, IEqualityComparer<Uri> uriComparer)
        {
            this.fallbackResolver = fallbackResolver;
            this.mappings = new Dictionary<Uri, PreloadedData>(0x10, uriComparer);
            this.preloadedDtds = preloadedDtds;
            if (preloadedDtds != XmlKnownDtds.None)
            {
                if ((preloadedDtds & XmlKnownDtds.Xhtml10) != XmlKnownDtds.None)
                {
                    this.AddKnownDtd(Xhtml10_Dtd);
                }
                if ((preloadedDtds & XmlKnownDtds.Rss091) != XmlKnownDtds.None)
                {
                    this.AddKnownDtd(Rss091_Dtd);
                }
            }
        }

        public void Add(Uri uri, byte[] value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Add(uri, new ByteArrayChunk(value, 0, value.Length));
        }

        public void Add(Uri uri, Stream value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (value.CanSeek)
            {
                int length = (int) value.Length;
                byte[] buffer = new byte[length];
                value.Read(buffer, 0, length);
                this.Add(uri, new ByteArrayChunk(buffer));
            }
            else
            {
                int num2;
                MemoryStream stream = new MemoryStream();
                byte[] buffer2 = new byte[0x1000];
                while ((num2 = value.Read(buffer2, 0, buffer2.Length)) > 0)
                {
                    stream.Write(buffer2, 0, num2);
                }
                int position = (int) stream.Position;
                byte[] destinationArray = new byte[position];
                Array.Copy(stream.GetBuffer(), destinationArray, position);
                this.Add(uri, new ByteArrayChunk(destinationArray));
            }
        }

        public void Add(Uri uri, string value)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Add(uri, new StringData(value));
        }

        private void Add(Uri uri, PreloadedData data)
        {
            if (this.mappings.ContainsKey(uri))
            {
                this.mappings[uri] = data;
            }
            else
            {
                this.mappings.Add(uri, data);
            }
        }

        public void Add(Uri uri, byte[] value, int offset, int count)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((value.Length - offset) < count)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            this.Add(uri, new ByteArrayChunk(value, offset, count));
        }

        private void AddKnownDtd(XmlKnownDtdData[] dtdSet)
        {
            for (int i = 0; i < dtdSet.Length; i++)
            {
                XmlKnownDtdData data = dtdSet[i];
                this.mappings.Add(new Uri(data.publicId, UriKind.RelativeOrAbsolute), data);
                this.mappings.Add(new Uri(data.systemId, UriKind.RelativeOrAbsolute), data);
            }
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            PreloadedData data;
            if (absoluteUri == null)
            {
                throw new ArgumentNullException("absoluteUri");
            }
            if (!this.mappings.TryGetValue(absoluteUri, out data))
            {
                if (this.fallbackResolver == null)
                {
                    throw new XmlException(Res.GetString("Xml_CannotResolveUrl", new object[] { absoluteUri.ToString() }));
                }
                return this.fallbackResolver.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            if (((ofObjectToReturn == null) || (ofObjectToReturn == typeof(Stream))) || (ofObjectToReturn == typeof(object)))
            {
                return data.AsStream();
            }
            if (ofObjectToReturn != typeof(TextReader))
            {
                throw new XmlException(Res.GetString("Xml_UnsupportedClass"));
            }
            return data.AsTextReader();
        }

        public void Remove(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            this.mappings.Remove(uri);
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if ((relativeUri != null) && relativeUri.StartsWith("-//", StringComparison.CurrentCulture))
            {
                if (((this.preloadedDtds & XmlKnownDtds.Xhtml10) != XmlKnownDtds.None) && relativeUri.StartsWith("-//W3C//", StringComparison.CurrentCulture))
                {
                    for (int i = 0; i < Xhtml10_Dtd.Length; i++)
                    {
                        if (relativeUri == Xhtml10_Dtd[i].publicId)
                        {
                            return new Uri(relativeUri, UriKind.Relative);
                        }
                    }
                }
                if (((this.preloadedDtds & XmlKnownDtds.Rss091) != XmlKnownDtds.None) && (relativeUri == Rss091_Dtd[0].publicId))
                {
                    return new Uri(relativeUri, UriKind.Relative);
                }
            }
            return base.ResolveUri(baseUri, relativeUri);
        }

        public override bool SupportsType(Uri absoluteUri, Type type)
        {
            PreloadedData data;
            if (absoluteUri == null)
            {
                throw new ArgumentNullException("absoluteUri");
            }
            if (this.mappings.TryGetValue(absoluteUri, out data))
            {
                return data.SupportsType(type);
            }
            if (this.fallbackResolver != null)
            {
                return this.fallbackResolver.SupportsType(absoluteUri, type);
            }
            return base.SupportsType(absoluteUri, type);
        }

        public override ICredentials Credentials
        {
            set
            {
                if (this.fallbackResolver != null)
                {
                    this.fallbackResolver.Credentials = value;
                }
            }
        }

        public IEnumerable<Uri> PreloadedUris
        {
            get
            {
                return this.mappings.Keys;
            }
        }

        private class ByteArrayChunk : XmlPreloadedResolver.PreloadedData
        {
            private byte[] array;
            private int length;
            private int offset;

            internal ByteArrayChunk(byte[] array) : this(array, 0, array.Length)
            {
            }

            internal ByteArrayChunk(byte[] array, int offset, int length)
            {
                this.array = array;
                this.offset = offset;
                this.length = length;
            }

            internal override Stream AsStream()
            {
                return new MemoryStream(this.array, this.offset, this.length);
            }
        }

        private abstract class PreloadedData
        {
            protected PreloadedData()
            {
            }

            internal abstract Stream AsStream();
            internal virtual TextReader AsTextReader()
            {
                throw new XmlException(Res.GetString("Xml_UnsupportedClass"));
            }

            internal virtual bool SupportsType(Type type)
            {
                if ((type != null) && !(type == typeof(Stream)))
                {
                    return false;
                }
                return true;
            }
        }

        private class StringData : XmlPreloadedResolver.PreloadedData
        {
            private string str;

            internal StringData(string str)
            {
                this.str = str;
            }

            internal override Stream AsStream()
            {
                return new MemoryStream(Encoding.Unicode.GetBytes(this.str));
            }

            internal override TextReader AsTextReader()
            {
                return new StringReader(this.str);
            }

            internal override bool SupportsType(Type type)
            {
                return ((type == typeof(TextReader)) || base.SupportsType(type));
            }
        }

        private class XmlKnownDtdData : XmlPreloadedResolver.PreloadedData
        {
            internal string publicId;
            private string resourceName;
            internal string systemId;

            internal XmlKnownDtdData(string publicId, string systemId, string resourceName)
            {
                this.publicId = publicId;
                this.systemId = systemId;
                this.resourceName = resourceName;
            }

            internal override Stream AsStream()
            {
                return Assembly.GetExecutingAssembly().GetManifestResourceStream(this.resourceName);
            }
        }
    }
}

