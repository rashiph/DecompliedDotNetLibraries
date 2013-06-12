namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal abstract class ExtensionQuery : Query
    {
        protected string name;
        protected string prefix;
        private ResetableIterator queryIterator;
        protected XsltContext xsltContext;

        protected ExtensionQuery(ExtensionQuery other) : base(other)
        {
            this.prefix = other.prefix;
            this.name = other.name;
            this.xsltContext = other.xsltContext;
            this.queryIterator = (ResetableIterator) Query.Clone(other.queryIterator);
        }

        public ExtensionQuery(string prefix, string name)
        {
            this.prefix = prefix;
            this.name = name;
        }

        public override XPathNavigator Advance()
        {
            if (this.queryIterator == null)
            {
                throw XPathException.Create("Xp_NodeSetExpected");
            }
            if (this.queryIterator.MoveNext())
            {
                return this.queryIterator.Current;
            }
            return null;
        }

        protected object ProcessResult(object value)
        {
            if (value is string)
            {
                return value;
            }
            if (value is double)
            {
                return value;
            }
            if (value is bool)
            {
                return value;
            }
            if (value is XPathNavigator)
            {
                return value;
            }
            if (value is int)
            {
                return (double) ((int) value);
            }
            if (value == null)
            {
                this.queryIterator = XPathEmptyIterator.Instance;
                return this;
            }
            ResetableIterator iterator = value as ResetableIterator;
            if (iterator != null)
            {
                this.queryIterator = (ResetableIterator) iterator.Clone();
                return this;
            }
            XPathNodeIterator nodeIterator = value as XPathNodeIterator;
            if (nodeIterator != null)
            {
                this.queryIterator = new XPathArrayIterator(nodeIterator);
                return this;
            }
            IXPathNavigable navigable = value as IXPathNavigable;
            if (navigable != null)
            {
                return navigable.CreateNavigator();
            }
            if (value is short)
            {
                return (double) ((short) value);
            }
            if (value is long)
            {
                return (double) ((long) value);
            }
            if (value is uint)
            {
                return (double) ((uint) value);
            }
            if (value is ushort)
            {
                return (double) ((ushort) value);
            }
            if (value is ulong)
            {
                return (double) ((ulong) value);
            }
            if (value is float)
            {
                return (double) ((float) value);
            }
            if (value is decimal)
            {
                return (double) ((decimal) value);
            }
            return value.ToString();
        }

        public override void Reset()
        {
            if (this.queryIterator != null)
            {
                this.queryIterator.Reset();
            }
        }

        public override int Count
        {
            get
            {
                if (this.queryIterator != null)
                {
                    return this.queryIterator.Count;
                }
                return 1;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                if (this.queryIterator == null)
                {
                    throw XPathException.Create("Xp_NodeSetExpected");
                }
                if (this.queryIterator.CurrentPosition == 0)
                {
                    this.Advance();
                }
                return this.queryIterator.Current;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                if (this.queryIterator != null)
                {
                    return this.queryIterator.CurrentPosition;
                }
                return 0;
            }
        }

        protected string QName
        {
            get
            {
                if (this.prefix.Length == 0)
                {
                    return this.name;
                }
                return (this.prefix + ":" + this.name);
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.Any;
            }
        }
    }
}

