namespace System.ServiceModel.XamlIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;

    [ContentProperty("Namespaces")]
    public class XPathMessageContextMarkupExtension : MarkupExtension
    {
        private static List<string> implicitPrefixes = new List<string>();
        private Dictionary<string, string> namespaces;

        static XPathMessageContextMarkupExtension()
        {
            foreach (string str in XPathMessageContext.defaultNamespaces.Keys)
            {
                implicitPrefixes.Add(str);
            }
            implicitPrefixes.Add("");
            implicitPrefixes.Add("xml");
            implicitPrefixes.Add("xmlns");
        }

        public XPathMessageContextMarkupExtension()
        {
            this.namespaces = new Dictionary<string, string>();
        }

        public XPathMessageContextMarkupExtension(XPathMessageContext context) : this()
        {
            foreach (string str in context)
            {
                if (!implicitPrefixes.Contains(str))
                {
                    this.namespaces.Add(str, context.LookupNamespace(str));
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            XPathMessageContext context = new XPathMessageContext();
            foreach (KeyValuePair<string, string> pair in this.namespaces)
            {
                context.AddNamespace(pair.Key, pair.Value);
            }
            return context;
        }

        public Dictionary<string, string> Namespaces
        {
            get
            {
                return this.namespaces;
            }
        }
    }
}

