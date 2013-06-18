namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Windows.Markup;
    using System.Xml;
    using System.Xml.Xsl;

    [ContentProperty("Expression")]
    public class XPathMessageQuery : MessageQuery
    {
        private string expression;
        private XPathQueryMatcher matcher;
        private XmlNamespaceManager namespaces;
        private bool needCompile;
        private object thisLock;

        public XPathMessageQuery() : this(string.Empty, (XmlNamespaceManager) new XPathMessageContext())
        {
        }

        public XPathMessageQuery(string expression) : this(expression, (XmlNamespaceManager) new XPathMessageContext())
        {
        }

        public XPathMessageQuery(string expression, XmlNamespaceManager namespaces)
        {
            if (expression == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("expression");
            }
            this.expression = expression;
            this.namespaces = namespaces;
            this.needCompile = true;
            this.thisLock = new object();
        }

        public XPathMessageQuery(string expression, XsltContext context) : this(expression, (XmlNamespaceManager) context)
        {
        }

        public override MessageQueryCollection CreateMessageQueryCollection()
        {
            return new XPathMessageQueryCollection();
        }

        private void EnsureCompile()
        {
            if (this.needCompile)
            {
                lock (this.thisLock)
                {
                    if (this.needCompile)
                    {
                        this.matcher = new XPathQueryMatcher(false);
                        this.matcher.Compile(this.expression, this.namespaces);
                        this.needCompile = false;
                    }
                }
            }
        }

        public override TResult Evaluate<TResult>(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (((typeof(TResult) != typeof(XPathResult)) && (typeof(TResult) != typeof(string))) && ((typeof(TResult) != typeof(bool)) && (typeof(TResult) != typeof(object))))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult", System.ServiceModel.SR.GetString("UnsupportedMessageQueryResultType", new object[] { typeof(TResult) }));
            }
            this.EnsureCompile();
            return this.matcher.Evaluate<TResult>(message, false).GetSingleResult();
        }

        public override TResult Evaluate<TResult>(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }
            this.EnsureCompile();
            if (((typeof(TResult) != typeof(XPathResult)) && (typeof(TResult) != typeof(string))) && ((typeof(TResult) != typeof(bool)) && (typeof(TResult) != typeof(object))))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult", System.ServiceModel.SR.GetString("UnsupportedMessageQueryResultType", new object[] { typeof(TResult) }));
            }
            this.EnsureCompile();
            return this.matcher.Evaluate<TResult>(buffer).GetSingleResult();
        }

        [DefaultValue("")]
        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.expression = value;
                this.needCompile = true;
            }
        }

        [DefaultValue((string) null)]
        public XmlNamespaceManager Namespaces
        {
            get
            {
                return this.namespaces;
            }
            set
            {
                this.namespaces = value;
                this.needCompile = true;
            }
        }
    }
}

