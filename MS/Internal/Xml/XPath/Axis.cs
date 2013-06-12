namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class Axis : AstNode
    {
        protected bool abbrAxis;
        private AxisType axisType;
        private AstNode input;
        private string name;
        private XPathNodeType nodeType;
        private string prefix;
        private string urn;

        public Axis(AxisType axisType, AstNode input) : this(axisType, input, string.Empty, string.Empty, XPathNodeType.All)
        {
            this.abbrAxis = true;
        }

        public Axis(AxisType axisType, AstNode input, string prefix, string name, XPathNodeType nodetype)
        {
            this.urn = string.Empty;
            this.axisType = axisType;
            this.input = input;
            this.prefix = prefix;
            this.name = name;
            this.nodeType = nodetype;
        }

        public bool AbbrAxis
        {
            get
            {
                return this.abbrAxis;
            }
        }

        public AstNode Input
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public XPathNodeType NodeType
        {
            get
            {
                return this.nodeType;
            }
        }

        public string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }

        public override AstNode.AstType Type
        {
            get
            {
                return AstNode.AstType.Axis;
            }
        }

        public AxisType TypeOfAxis
        {
            get
            {
                return this.axisType;
            }
        }

        public string Urn
        {
            get
            {
                return this.urn;
            }
            set
            {
                this.urn = value;
            }
        }

        public enum AxisType
        {
            Ancestor,
            AncestorOrSelf,
            Attribute,
            Child,
            Descendant,
            DescendantOrSelf,
            Following,
            FollowingSibling,
            Namespace,
            Parent,
            Preceding,
            PrecedingSibling,
            Self,
            None
        }
    }
}

