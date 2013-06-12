namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class FilterQuery : BaseAxisQuery
    {
        private Query cond;
        private bool noPosition;

        private FilterQuery(FilterQuery other) : base((BaseAxisQuery) other)
        {
            this.cond = Query.Clone(other.cond);
            this.noPosition = other.noPosition;
        }

        public FilterQuery(Query qyParent, Query cond, bool noPosition) : base(qyParent)
        {
            this.cond = cond;
            this.noPosition = noPosition;
        }

        public override XPathNavigator Advance()
        {
            while ((base.currentNode = base.qyInput.Advance()) != null)
            {
                if (this.EvaluatePredicate())
                {
                    base.position++;
                    return base.currentNode;
                }
            }
            return null;
        }

        public override XPathNodeIterator Clone()
        {
            return new FilterQuery(this);
        }

        internal bool EvaluatePredicate()
        {
            object obj2 = this.cond.Evaluate(base.qyInput);
            if (obj2 is XPathNodeIterator)
            {
                return (this.cond.Advance() != null);
            }
            if (obj2 is string)
            {
                return (((string) obj2).Length != 0);
            }
            if (obj2 is double)
            {
                return (((double) obj2) == base.qyInput.CurrentPosition);
            }
            if (obj2 is bool)
            {
                return (bool) obj2;
            }
            return true;
        }

        public override XPathNavigator MatchNode(XPathNavigator current)
        {
            XPathNavigator navigator;
            XPathNavigator navigator4;
            if (current != null)
            {
                navigator = base.qyInput.MatchNode(current);
                if (navigator == null)
                {
                    goto Label_01B9;
                }
                switch (this.cond.StaticType)
                {
                    case XPathResultType.Number:
                    {
                        OperandQuery cond = this.cond as OperandQuery;
                        if (cond != null)
                        {
                            double val = (double) cond.val;
                            ChildrenQuery qyInput = base.qyInput as ChildrenQuery;
                            if (qyInput != null)
                            {
                                XPathNavigator e = current.Clone();
                                e.MoveToParent();
                                int num2 = 0;
                                e.MoveToFirstChild();
                                do
                                {
                                    if (qyInput.matches(e))
                                    {
                                        num2++;
                                        if (current.IsSamePosition(e))
                                        {
                                            if (val != num2)
                                            {
                                                return null;
                                            }
                                            return navigator;
                                        }
                                    }
                                }
                                while (e.MoveToNext());
                                return null;
                            }
                            AttributeQuery query3 = base.qyInput as AttributeQuery;
                            if (query3 != null)
                            {
                                XPathNavigator navigator3 = current.Clone();
                                navigator3.MoveToParent();
                                int num3 = 0;
                                navigator3.MoveToFirstAttribute();
                                do
                                {
                                    if (query3.matches(navigator3))
                                    {
                                        num3++;
                                        if (current.IsSamePosition(navigator3))
                                        {
                                            if (val != num3)
                                            {
                                                return null;
                                            }
                                            return navigator;
                                        }
                                    }
                                }
                                while (navigator3.MoveToNextAttribute());
                                return null;
                            }
                        }
                        goto Label_0192;
                    }
                    case XPathResultType.String:
                        if (!this.noPosition)
                        {
                            goto Label_0192;
                        }
                        if (((string) this.cond.Evaluate(new XPathSingletonIterator(current, true))).Length != 0)
                        {
                            return navigator;
                        }
                        return null;

                    case XPathResultType.Boolean:
                        if (!this.noPosition)
                        {
                            goto Label_0192;
                        }
                        if ((bool) this.cond.Evaluate(new XPathSingletonIterator(current, true)))
                        {
                            return navigator;
                        }
                        return null;

                    case XPathResultType.NodeSet:
                        this.cond.Evaluate(new XPathSingletonIterator(current, true));
                        if (this.cond.Advance() != null)
                        {
                            return navigator;
                        }
                        return null;

                    case ((XPathResultType) 4):
                        return navigator;
                }
            }
            return null;
        Label_0192:
            this.Evaluate(new XPathSingletonIterator(navigator, true));
            while ((navigator4 = this.Advance()) != null)
            {
                if (navigator4.IsSamePosition(current))
                {
                    return navigator;
                }
            }
        Label_01B9:
            return null;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            if (!this.noPosition)
            {
                w.WriteAttributeString("position", "yes");
            }
            base.qyInput.PrintQuery(w);
            this.cond.PrintQuery(w);
            w.WriteEndElement();
        }

        public override void Reset()
        {
            this.cond.Reset();
            base.Reset();
        }

        public override void SetXsltContext(XsltContext input)
        {
            base.SetXsltContext(input);
            this.cond.SetXsltContext(input);
            if (((this.cond.StaticType != XPathResultType.Number) && (this.cond.StaticType != XPathResultType.Any)) && this.noPosition)
            {
                ReversePositionQuery qyInput = base.qyInput as ReversePositionQuery;
                if (qyInput != null)
                {
                    base.qyInput = qyInput.input;
                }
            }
        }

        public Query Condition
        {
            get
            {
                return this.cond;
            }
        }

        public override QueryProps Properties
        {
            get
            {
                return (QueryProps.Position | (base.qyInput.Properties & (QueryProps.Merge | QueryProps.Reverse)));
            }
        }
    }
}

