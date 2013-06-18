namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Xml.XPath;

    internal class NodeSelectCriteria
    {
        protected QueryAxis axis;
        protected NodeQName qname;
        protected NodeQNameType qnameType;
        protected QueryNodeType type;

        internal NodeSelectCriteria(QueryAxisType axis, NodeQName qname, QueryNodeType nodeType)
        {
            this.axis = QueryDataModel.GetAxis(axis);
            this.qname = qname;
            this.qnameType = qname.GetQNameType();
            this.type = nodeType;
        }

        public bool Equals(NodeSelectCriteria criteria)
        {
            return (((this.axis.Type == criteria.axis.Type) && (this.type == criteria.type)) && this.qname.Equals(criteria.qname));
        }

        internal bool MatchQName(SeekableXPathNavigator node)
        {
            switch (((NodeQNameType) ((byte) (this.qnameType & NodeQNameType.Standard))))
            {
                case NodeQNameType.Name:
                    if (node.NamespaceURI.Length != 0)
                    {
                        return false;
                    }
                    return this.qname.EqualsName(node.LocalName);

                case NodeQNameType.Standard:
                {
                    string localName = node.LocalName;
                    if ((this.qname.name.Length != localName.Length) || !(this.qname.name == localName))
                    {
                        return false;
                    }
                    localName = node.NamespaceURI;
                    if (this.qname.ns.Length != localName.Length)
                    {
                        return false;
                    }
                    return (this.qname.ns == localName);
                }
            }
            if (this.qnameType == NodeQNameType.Empty)
            {
                return true;
            }
            NodeQNameType type2 = (NodeQNameType) ((byte) (this.qnameType & NodeQNameType.Wildcard));
            if (type2 != NodeQNameType.NameWildcard)
            {
                return (type2 == NodeQNameType.Wildcard);
            }
            return this.qname.EqualsNamespace(node.NamespaceURI);
        }

        internal bool MatchType(SeekableXPathNavigator node)
        {
            QueryNodeType root;
            switch (node.NodeType)
            {
                case XPathNodeType.Root:
                    root = QueryNodeType.Root;
                    break;

                case XPathNodeType.Element:
                    root = QueryNodeType.Element;
                    break;

                case XPathNodeType.Attribute:
                    root = QueryNodeType.Attribute;
                    break;

                case XPathNodeType.Text:
                case XPathNodeType.SignificantWhitespace:
                case XPathNodeType.Whitespace:
                    root = QueryNodeType.Text;
                    break;

                case XPathNodeType.ProcessingInstruction:
                    root = QueryNodeType.Processing;
                    break;

                case XPathNodeType.Comment:
                    root = QueryNodeType.Comment;
                    break;

                default:
                    return false;
            }
            return (root == ((byte) (this.type & root)));
        }

        internal void Select(SeekableXPathNavigator contextNode, NodeSequence destSequence)
        {
            switch (this.type)
            {
                case QueryNodeType.Root:
                    contextNode.MoveToRoot();
                    destSequence.Add(contextNode);
                    return;

                case QueryNodeType.Attribute:
                    if (contextNode.MoveToFirstAttribute())
                    {
                        do
                        {
                            if (this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                                if (((byte) (this.qnameType & NodeQNameType.Wildcard)) == 0)
                                {
                                    return;
                                }
                            }
                        }
                        while (contextNode.MoveToNextAttribute());
                    }
                    return;

                case QueryNodeType.Element:
                    if (QueryAxisType.Descendant != this.axis.Type)
                    {
                        if (QueryAxisType.DescendantOrSelf == this.axis.Type)
                        {
                            destSequence.Add(contextNode);
                            this.SelectDescendants(contextNode, destSequence);
                            return;
                        }
                        if (contextNode.MoveToFirstChild())
                        {
                            do
                            {
                                if ((XPathNodeType.Element == contextNode.NodeType) && this.MatchQName(contextNode))
                                {
                                    destSequence.Add(contextNode);
                                }
                            }
                            while (contextNode.MoveToNext());
                        }
                        return;
                    }
                    this.SelectDescendants(contextNode, destSequence);
                    return;

                case QueryNodeType.Text:
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            if (this.MatchType(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    return;

                case QueryNodeType.ChildNodes:
                    if (QueryAxisType.Descendant == this.axis.Type)
                    {
                        this.SelectDescendants(contextNode, destSequence);
                        return;
                    }
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    return;
            }
            if (QueryAxisType.Self == this.axis.Type)
            {
                if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                {
                    destSequence.Add(contextNode);
                }
            }
            else if (QueryAxisType.Descendant == this.axis.Type)
            {
                this.SelectDescendants(contextNode, destSequence);
            }
            else if (QueryAxisType.DescendantOrSelf == this.axis.Type)
            {
                destSequence.Add(contextNode);
                this.SelectDescendants(contextNode, destSequence);
            }
            else if (QueryAxisType.Child == this.axis.Type)
            {
                if (contextNode.MoveToFirstChild())
                {
                    do
                    {
                        if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                        {
                            destSequence.Add(contextNode);
                        }
                    }
                    while (contextNode.MoveToNext());
                }
            }
            else
            {
                if (QueryAxisType.Attribute != this.axis.Type)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
                }
                if (contextNode.MoveToFirstAttribute())
                {
                    do
                    {
                        if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                        {
                            destSequence.Add(contextNode);
                            if (((byte) (this.qnameType & NodeQNameType.Wildcard)) == 0)
                            {
                                return;
                            }
                        }
                    }
                    while (contextNode.MoveToNextAttribute());
                }
            }
        }

        internal Opcode Select(SeekableXPathNavigator contextNode, NodeSequence destSequence, SelectOpcode next)
        {
            Opcode opcode = next.Next;
            switch (this.type)
            {
                case QueryNodeType.Root:
                    contextNode.MoveToRoot();
                    return next.Eval(destSequence, contextNode);

                case QueryNodeType.Element:
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            if ((XPathNodeType.Element == contextNode.NodeType) && this.MatchQName(contextNode))
                            {
                                long currentPosition = contextNode.CurrentPosition;
                                opcode = next.Eval(destSequence, contextNode);
                                contextNode.CurrentPosition = currentPosition;
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    return opcode;

                case QueryNodeType.ChildNodes:
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    return opcode;
            }
            if (QueryAxisType.Self != this.axis.Type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
            }
            if (this.MatchType(contextNode) && this.MatchQName(contextNode))
            {
                long num = contextNode.CurrentPosition;
                opcode = next.Eval(destSequence, contextNode);
                contextNode.CurrentPosition = num;
            }
            return opcode;
        }

        private void SelectDescendants(SeekableXPathNavigator contextNode, NodeSequence destSequence)
        {
            int num = 1;
            if (contextNode.MoveToFirstChild())
            {
                while (num > 0)
                {
                    if (this.MatchQName(contextNode))
                    {
                        destSequence.Add(contextNode);
                    }
                    if (contextNode.MoveToFirstChild())
                    {
                        num++;
                    }
                    else if (!contextNode.MoveToNext())
                    {
                        while (num > 0)
                        {
                            contextNode.MoveToParent();
                            num--;
                            if (contextNode.MoveToNext())
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal QueryAxis Axis
        {
            get
            {
                return this.axis;
            }
        }

        internal bool IsCompressable
        {
            get
            {
                if (QueryAxisType.Self != this.axis.Type)
                {
                    return (QueryAxisType.Child == this.axis.Type);
                }
                return true;
            }
        }

        internal NodeQName QName
        {
            get
            {
                return this.qname;
            }
        }

        internal QueryNodeType Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

