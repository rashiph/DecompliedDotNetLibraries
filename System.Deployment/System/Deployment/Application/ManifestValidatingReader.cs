namespace System.Deployment.Application
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    internal static class ManifestValidatingReader
    {
        private static string[] _manifestSchemas = new string[] { "manifest.2.0.0.15-pre.adaptive.xsd" };
        private static XmlSchemaSet _manifestSchemaSet = null;
        private static object _manifestSchemaSetLock = new object();

        public static XmlReader Create(Stream stream)
        {
            return Create(stream, ManifestSchemaSet);
        }

        private static XmlReader Create(Stream stream, XmlSchemaSet schemaSet)
        {
            XmlReaderSettings settings = new XmlReaderSettings {
                Schemas = schemaSet,
                ValidationType = ValidationType.Schema
            };
            XmlFilteredReader reader = new XmlFilteredReader(stream);
            return XmlReader.Create(reader, settings);
        }

        private static XmlSchemaSet MakeSchemaSet(string[] schemas)
        {
            XmlSchemaSet set = new XmlSchemaSet();
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            set.XmlResolver = new ResourceResolver(executingAssembly);
            for (int i = 0; i < schemas.Length; i++)
            {
                using (Stream stream = executingAssembly.GetManifestResourceStream(schemas[i]))
                {
                    set.Add(null, new XmlTextReader(stream));
                }
            }
            return set;
        }

        private static XmlSchemaSet ManifestSchemaSet
        {
            get
            {
                if (_manifestSchemaSet == null)
                {
                    lock (_manifestSchemaSetLock)
                    {
                        if (_manifestSchemaSet == null)
                        {
                            _manifestSchemaSet = MakeSchemaSet(_manifestSchemas);
                        }
                    }
                }
                return _manifestSchemaSet;
            }
        }

        private class ResourceResolver : XmlUrlResolver
        {
            private Assembly _assembly;
            private const string Prefix = "df://resources/";

            public ResourceResolver(Assembly assembly)
            {
                this._assembly = assembly;
            }

            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                if (!absoluteUri.AbsoluteUri.StartsWith("df://resources/", StringComparison.Ordinal))
                {
                    return base.GetEntity(absoluteUri, role, ofObjectToReturn);
                }
                if ((ofObjectToReturn != null) && (ofObjectToReturn != typeof(Stream)))
                {
                    throw new XmlException(Resources.GetString("Ex_OnlyStreamTypeSupported"));
                }
                if (absoluteUri.ToString() == "df://resources/-//W3C//DTD XMLSCHEMA 200102//EN")
                {
                    return this._assembly.GetManifestResourceStream("XMLSchema.dtd");
                }
                if (absoluteUri.ToString() == "df://resources/xs-datatypes")
                {
                    return this._assembly.GetManifestResourceStream("datatypes.dtd");
                }
                string name = absoluteUri.AbsoluteUri.Remove(0, "df://resources/".Length);
                return this._assembly.GetManifestResourceStream(name);
            }

            public override Uri ResolveUri(Uri baseUri, string relativeUri)
            {
                if (((baseUri != null) && !(baseUri.ToString() == string.Empty)) && (!baseUri.IsAbsoluteUri || !baseUri.AbsoluteUri.StartsWith("df://resources/", StringComparison.Ordinal)))
                {
                    return base.ResolveUri(baseUri, relativeUri);
                }
                return new Uri("df://resources/" + relativeUri);
            }
        }

        private class XmlFilteredReader : XmlTextReader
        {
            private static StringCollection KnownNamespaces = new StringCollection();

            static XmlFilteredReader()
            {
                KnownNamespaces.Add("urn:schemas-microsoft-com:asm.v1");
                KnownNamespaces.Add("urn:schemas-microsoft-com:asm.v2");
                KnownNamespaces.Add("http://www.w3.org/2000/09/xmldsig#");
            }

            public XmlFilteredReader(Stream stream) : base(stream)
            {
            }

            public override bool Read()
            {
                bool flag = base.Read();
                if ((base.NodeType == XmlNodeType.Element) && !KnownNamespaces.Contains(base.NamespaceURI))
                {
                    base.Skip();
                }
                return flag;
            }
        }
    }
}

