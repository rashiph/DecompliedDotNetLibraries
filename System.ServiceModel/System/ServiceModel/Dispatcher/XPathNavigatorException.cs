namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Xml.XPath;

    [Serializable, KnownType(typeof(string[]))]
    public class XPathNavigatorException : XPathException
    {
        public XPathNavigatorException()
        {
        }

        public XPathNavigatorException(string message) : this(message, null)
        {
        }

        protected XPathNavigatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XPathNavigatorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal MessageFilterException Process(Opcode op)
        {
            Collection<MessageFilter> filters = new Collection<MessageFilter>();
            op.CollectXPathFilters(filters);
            return new MessageFilterException(this.Message, base.InnerException, filters);
        }
    }
}

