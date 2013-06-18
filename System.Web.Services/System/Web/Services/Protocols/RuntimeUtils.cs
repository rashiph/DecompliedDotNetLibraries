namespace System.Web.Services.Protocols
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web.Services;
    using System.Web.Services.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    internal class RuntimeUtils
    {
        private RuntimeUtils()
        {
        }

        internal static string ElementString(XmlElement element)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            writer.Write("<");
            writer.Write(element.Name);
            if ((element.NamespaceURI != null) && (element.NamespaceURI.Length > 0))
            {
                writer.Write(" xmlns");
                if ((element.Prefix != null) && (element.Prefix.Length > 0))
                {
                    writer.Write(":");
                    writer.Write(element.Prefix);
                }
                writer.Write("='");
                writer.Write(element.NamespaceURI);
                writer.Write("'");
            }
            writer.Write(">..</");
            writer.Write(element.Name);
            writer.Write(">");
            return writer.ToString();
        }

        internal static XmlDeserializationEvents GetDeserializationEvents()
        {
            return new XmlDeserializationEvents { OnUnknownElement = new XmlElementEventHandler(RuntimeUtils.OnUnknownElement), OnUnknownAttribute = new XmlAttributeEventHandler(RuntimeUtils.OnUnknownAttribute) };
        }

        internal static bool IsKnownNamespace(string ns)
        {
            if (ns != "http://www.w3.org/2001/XMLSchema-instance")
            {
                if (ns == "http://www.w3.org/XML/1998/namespace")
                {
                    return true;
                }
                if ((ns == "http://schemas.xmlsoap.org/soap/encoding/") || (ns == "http://schemas.xmlsoap.org/soap/envelope/"))
                {
                    return true;
                }
                if ((!(ns == "http://www.w3.org/2003/05/soap-envelope") && !(ns == "http://www.w3.org/2003/05/soap-encoding")) && !(ns == "http://www.w3.org/2003/05/soap-rpc"))
                {
                    return false;
                }
            }
            return true;
        }

        private static void OnUnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            if ((e.Attr != null) && !IsKnownNamespace(e.Attr.NamespaceURI))
            {
                Tracing.OnUnknownAttribute(sender, e);
                if (e.ExpectedAttributes == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownAttribute", new object[] { e.Attr.Name, e.Attr.Value }));
                }
                if (e.ExpectedAttributes.Length == 0)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownAttribute2", new object[] { e.Attr.Name, e.Attr.Value }));
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownAttribute3", new object[] { e.Attr.Name, e.Attr.Value, e.ExpectedAttributes }));
            }
        }

        internal static void OnUnknownElement(object sender, XmlElementEventArgs e)
        {
            if (e.Element != null)
            {
                string str = ElementString(e.Element);
                Tracing.OnUnknownElement(sender, e);
                if (e.ExpectedElements == null)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownElement", new object[] { str }));
                }
                if (e.ExpectedElements.Length == 0)
                {
                    throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownElement1", new object[] { str }));
                }
                throw new InvalidOperationException(System.Web.Services.Res.GetString("WebUnknownElement2", new object[] { str, e.ExpectedElements }));
            }
        }
    }
}

