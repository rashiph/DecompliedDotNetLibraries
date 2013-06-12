namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class UnionExpr : Query
    {
        private bool advance1;
        private bool advance2;
        private XPathNavigator currentNode;
        private XPathNavigator nextNode;
        internal Query qy1;
        internal Query qy2;

        private UnionExpr(UnionExpr other) : base(other)
        {
            this.qy1 = Query.Clone(other.qy1);
            this.qy2 = Query.Clone(other.qy2);
            this.advance1 = other.advance1;
            this.advance2 = other.advance2;
            this.currentNode = Query.Clone(other.currentNode);
            this.nextNode = Query.Clone(other.nextNode);
        }

        public UnionExpr(Query query1, Query query2)
        {
            this.qy1 = query1;
            this.qy2 = query2;
            this.advance1 = true;
            this.advance2 = true;
        }

        public override XPathNavigator Advance()
        {
            XPathNavigator nextNode;
            XPathNavigator navigator2;
            XmlNodeOrder before = XmlNodeOrder.Before;
            if (this.advance1)
            {
                nextNode = this.qy1.Advance();
            }
            else
            {
                nextNode = this.nextNode;
            }
            if (this.advance2)
            {
                navigator2 = this.qy2.Advance();
            }
            else
            {
                navigator2 = this.nextNode;
            }
            if ((nextNode != null) && (navigator2 != null))
            {
                before = Query.CompareNodes(nextNode, navigator2);
            }
            else
            {
                if (navigator2 == null)
                {
                    this.advance1 = true;
                    this.advance2 = false;
                    this.currentNode = nextNode;
                    this.nextNode = null;
                    return nextNode;
                }
                this.advance1 = false;
                this.advance2 = true;
                this.currentNode = navigator2;
                this.nextNode = null;
                return navigator2;
            }
            switch (before)
            {
                case XmlNodeOrder.Before:
                    return this.ProcessBeforePosition(nextNode, navigator2);

                case XmlNodeOrder.After:
                    return this.ProcessAfterPosition(nextNode, navigator2);
            }
            return this.ProcessSamePosition(nextNode);
        }

        public override XPathNodeIterator Clone()
        {
            return new UnionExpr(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            this.qy1.Evaluate(context);
            this.qy2.Evaluate(context);
            this.advance1 = true;
            this.advance2 = true;
            this.nextNode = null;
            base.ResetCount();
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator xsltContext)
        {
            if (xsltContext == null)
            {
                return null;
            }
            XPathNavigator navigator = this.qy1.MatchNode(xsltContext);
            if (navigator != null)
            {
                return navigator;
            }
            return this.qy2.MatchNode(xsltContext);
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            if (this.qy1 != null)
            {
                this.qy1.PrintQuery(w);
            }
            if (this.qy2 != null)
            {
                this.qy2.PrintQuery(w);
            }
            w.WriteEndElement();
        }

        private XPathNavigator ProcessAfterPosition(XPathNavigator res1, XPathNavigator res2)
        {
            this.nextNode = res1;
            this.advance1 = false;
            this.advance2 = true;
            this.currentNode = res2;
            return res2;
        }

        private XPathNavigator ProcessBeforePosition(XPathNavigator res1, XPathNavigator res2)
        {
            this.nextNode = res2;
            this.advance2 = false;
            this.advance1 = true;
            this.currentNode = res1;
            return res1;
        }

        private XPathNavigator ProcessSamePosition(XPathNavigator result)
        {
            this.currentNode = result;
            this.advance1 = this.advance2 = true;
            return result;
        }

        public override void Reset()
        {
            this.qy1.Reset();
            this.qy2.Reset();
            this.advance1 = true;
            this.advance2 = true;
            this.nextNode = null;
        }

        public override void SetXsltContext(XsltContext xsltContext)
        {
            this.qy1.SetXsltContext(xsltContext);
            this.qy2.SetXsltContext(xsltContext);
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.currentNode;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }
    }
}

