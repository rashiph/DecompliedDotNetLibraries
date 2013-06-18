namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    internal static class ContextBindingElementPolicy
    {
        private static XmlDocument document;
        private const string EncryptAndSignName = "EncryptAndSign";
        private const string HttpNamespace = "http://schemas.xmlsoap.org/soap/http";
        private const string HttpUseCookieName = "HttpUseCookie";
        private const string IncludeContextName = "IncludeContext";
        private const string NoneName = "None";
        private const string ProtectionLevelName = "ProtectionLevel";
        private const string SignName = "Sign";
        private const string WscNamespace = "http://schemas.microsoft.com/ws/2006/05/context";

        private static bool ContainOnlyWhitespaceChild(XmlElement parent)
        {
            if (parent.ChildNodes.Count != 0)
            {
                foreach (System.Xml.XmlNode node in parent.ChildNodes)
                {
                    if (!(node is XmlWhitespace))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static void ExportRequireContextAssertion(ContextBindingElement bindingElement, PolicyAssertionCollection assertions)
        {
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            if (bindingElement.ContextExchangeMechanism != ContextExchangeMechanism.ContextSoapHeader)
            {
                XmlElement item = Document.CreateElement(null, "HttpUseCookie", "http://schemas.xmlsoap.org/soap/http");
                assertions.Add(item);
            }
            else
            {
                XmlElement element = Document.CreateElement(null, "IncludeContext", "http://schemas.microsoft.com/ws/2006/05/context");
                System.Xml.XmlAttribute node = Document.CreateAttribute("ProtectionLevel");
                switch (bindingElement.ProtectionLevel)
                {
                    case ProtectionLevel.Sign:
                        node.Value = "Sign";
                        break;

                    case ProtectionLevel.EncryptAndSign:
                        node.Value = "EncryptAndSign";
                        break;

                    default:
                        node.Value = "None";
                        break;
                }
                element.Attributes.Append(node);
                assertions.Add(element);
            }
        }

        public static bool TryGetHttpUseCookieAssertion(ICollection<XmlElement> assertions, out XmlElement httpUseCookieAssertion)
        {
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            httpUseCookieAssertion = null;
            foreach (XmlElement element in assertions)
            {
                if (((element.LocalName == "HttpUseCookie") && (element.NamespaceURI == "http://schemas.xmlsoap.org/soap/http")) && (element.ChildNodes.Count == 0))
                {
                    httpUseCookieAssertion = element;
                    break;
                }
            }
            return (httpUseCookieAssertion != null);
        }

        public static bool TryImportRequireContextAssertion(PolicyAssertionCollection assertions, out ContextBindingElement bindingElement)
        {
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            bindingElement = null;
            foreach (XmlElement element in assertions)
            {
                if (((element.LocalName == "IncludeContext") && (element.NamespaceURI == "http://schemas.microsoft.com/ws/2006/05/context")) && ContainOnlyWhitespaceChild(element))
                {
                    string attribute = element.GetAttribute("ProtectionLevel");
                    if ("EncryptAndSign".Equals(attribute, StringComparison.Ordinal))
                    {
                        bindingElement = new ContextBindingElement(ProtectionLevel.EncryptAndSign);
                    }
                    else if ("Sign".Equals(attribute, StringComparison.Ordinal))
                    {
                        bindingElement = new ContextBindingElement(ProtectionLevel.Sign);
                    }
                    else if ("None".Equals(attribute, StringComparison.Ordinal))
                    {
                        bindingElement = new ContextBindingElement(ProtectionLevel.None);
                    }
                    if (bindingElement != null)
                    {
                        assertions.Remove(element);
                        return true;
                    }
                }
            }
            return false;
        }

        private static XmlDocument Document
        {
            get
            {
                if (document == null)
                {
                    document = new XmlDocument();
                }
                return document;
            }
        }
    }
}

