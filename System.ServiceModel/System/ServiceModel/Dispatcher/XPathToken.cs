namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathToken
    {
        private string name;
        private double number;
        private string prefix;
        private XPathTokenID tokenID = XPathTokenID.Unknown;

        internal XPathToken()
        {
        }

        internal void Clear()
        {
            this.number = double.NaN;
            this.prefix = string.Empty;
            this.name = string.Empty;
            this.tokenID = XPathTokenID.Unknown;
        }

        internal void Set(XPathTokenID id)
        {
            this.Clear();
            this.tokenID = id;
        }

        internal void Set(XPathTokenID id, double number)
        {
            this.Set(id);
            this.number = number;
        }

        internal void Set(XPathTokenID id, XPathParser.QName qname)
        {
            this.Set(id, qname.Name);
            this.prefix = qname.Prefix;
        }

        internal void Set(XPathTokenID id, string name)
        {
            this.Clear();
            this.tokenID = id;
            this.name = name;
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal double Number
        {
            get
            {
                return this.number;
            }
        }

        internal string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        internal XPathTokenID TokenID
        {
            get
            {
                return this.tokenID;
            }
        }
    }
}

