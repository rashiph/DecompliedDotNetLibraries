namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Policy;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal static class XmlUtil
    {
        private static readonly Evidence evidence = XmlSecureResolver.CreateEvidenceForUrl(Util.GetAssemblyPath());
        private static readonly ResourceResolver resolver = new ResourceResolver();

        public static XmlElement CloneElementToDocument(XmlElement element, XmlDocument document, string namespaceURI)
        {
            XmlElement element2 = document.CreateElement(element.Name, namespaceURI);
            foreach (XmlAttribute attribute in element.Attributes)
            {
                XmlAttribute attribute2 = document.CreateAttribute(attribute.Name);
                attribute2.Value = attribute.Value;
                element2.Attributes.Append(attribute2);
            }
            foreach (XmlNode node in element.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement newChild = CloneElementToDocument((XmlElement) node, document, namespaceURI);
                    element2.AppendChild(newChild);
                }
                else if (node.NodeType == XmlNodeType.Comment)
                {
                    XmlComment comment = document.CreateComment(((XmlComment) node).Data);
                    element2.AppendChild(comment);
                }
            }
            return element2;
        }

        public static string GetQName(XmlTextReader r, XmlNamespaceManager nsmgr)
        {
            string str = !string.IsNullOrEmpty(r.Prefix) ? r.Prefix : nsmgr.LookupPrefix(r.NamespaceURI);
            if (!string.IsNullOrEmpty(str))
            {
                return (str + ":" + r.LocalName);
            }
            return r.LocalName;
        }

        public static string TrimPrefix(string s)
        {
            int index = s.IndexOf(':');
            if (index < 0)
            {
                return s;
            }
            return s.Substring(index + 1);
        }

        public static Stream XslTransform(string resource, Stream input, params DictionaryEntry[] entries)
        {
            int tickCount = Environment.TickCount;
            Stream embeddedResourceStream = Util.GetEmbeddedResourceStream(resource);
            int num2 = Environment.TickCount;
            XPathDocument stylesheet = new XPathDocument(embeddedResourceStream);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "new XPathDocument(1) t={1}", new object[] { resource, Environment.TickCount - num2 }));
            int num3 = Environment.TickCount;
            System.Xml.Xsl.XslTransform transform = new System.Xml.Xsl.XslTransform();
            transform.Load(stylesheet, resolver, evidence);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "XslTransform.Load t={1}", new object[] { resource, Environment.TickCount - num3 }));
            MemoryStream output = new MemoryStream();
            Util.CopyStream(input, output);
            int num4 = Environment.TickCount;
            XPathDocument document2 = new XPathDocument(output);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "new XPathDocument(2) t={1}", new object[] { resource, Environment.TickCount - num4 }));
            XsltArgumentList args = null;
            if (entries.Length > 0)
            {
                args = new XsltArgumentList();
                foreach (DictionaryEntry entry in entries)
                {
                    string name = entry.Key.ToString();
                    object parameter = entry.Value.ToString();
                    args.AddParam(name, "", parameter);
                    Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "arg: key='{0}' value='{1}'", new object[] { name, parameter.ToString() }));
                }
            }
            MemoryStream w = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(w, Encoding.UTF8);
            writer.WriteStartDocument();
            int num1 = Environment.TickCount;
            transform.Transform((IXPathNavigable) document2, args, (XmlWriter) writer, resolver);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "XslTransform.Transform t={1}", new object[] { resource, Environment.TickCount - num4 }));
            writer.WriteEndDocument();
            writer.Flush();
            w.Position = 0L;
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "XslTransform(\"{0}\") t={1}", new object[] { resource, Environment.TickCount - tickCount }));
            return w;
        }

        private class ResourceResolver : XmlUrlResolver
        {
            public override object GetEntity(Uri uri, string role, Type t)
            {
                string path = uri.Segments[uri.Segments.Length - 1];
                Stream manifestResourceStream = null;
                if (!uri.LocalPath.StartsWith(Path.GetTempPath(), StringComparison.Ordinal))
                {
                    manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { typeof(Util).Namespace, path }));
                    if (manifestResourceStream != null)
                    {
                        return manifestResourceStream;
                    }
                    try
                    {
                        manifestResourceStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    if (manifestResourceStream != null)
                    {
                        return manifestResourceStream;
                    }
                }
                try
                {
                    manifestResourceStream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (DirectoryNotFoundException)
                {
                }
                catch (FileNotFoundException)
                {
                }
                if (manifestResourceStream != null)
                {
                    return manifestResourceStream;
                }
                return null;
            }
        }
    }
}

