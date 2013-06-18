namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal abstract class SingleFxEngineResultOpcode : ResultOpcode
    {
        protected object item;
        protected XPathExpression xpath;

        internal SingleFxEngineResultOpcode(OpcodeID id) : base(id)
        {
            base.flags |= OpcodeFlags.Fx;
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            MessageFilter item = this.item as MessageFilter;
            if (item != null)
            {
                filters.Add(item);
            }
        }

        internal override bool Equals(Opcode op)
        {
            return false;
        }

        protected object Evaluate(XPathNavigator nav)
        {
            SeekableMessageNavigator navigator = nav as SeekableMessageNavigator;
            if (navigator != null)
            {
                navigator.Atomize();
            }
            if (XPathResultType.NodeSet == this.xpath.ReturnType)
            {
                return nav.Select(this.xpath);
            }
            return nav.Evaluate(this.xpath);
        }

        internal object Item
        {
            set
            {
                this.item = value;
            }
        }

        internal XPathExpression XPath
        {
            set
            {
                this.xpath = value;
            }
        }
    }
}

