namespace System.Data
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class XmlIgnoreNamespaceReader : XmlNodeReader
    {
        private List<string> namespacesToIgnore;

        internal XmlIgnoreNamespaceReader(XmlDocument xdoc, string[] namespacesToIgnore) : base(xdoc)
        {
            this.namespacesToIgnore = new List<string>(namespacesToIgnore);
        }

        public override bool MoveToFirstAttribute()
        {
            if (!base.MoveToFirstAttribute())
            {
                return false;
            }
            return ((!this.namespacesToIgnore.Contains(this.NamespaceURI) && (!(this.NamespaceURI == "http://www.w3.org/XML/1998/namespace") || !(this.LocalName != "lang"))) || this.MoveToNextAttribute());
        }

        public override bool MoveToNextAttribute()
        {
            bool flag;
            bool flag2;
            do
            {
                flag2 = false;
                flag = false;
                if (base.MoveToNextAttribute())
                {
                    flag2 = true;
                    if (this.namespacesToIgnore.Contains(this.NamespaceURI) || ((this.NamespaceURI == "http://www.w3.org/XML/1998/namespace") && (this.LocalName != "lang")))
                    {
                        flag = true;
                    }
                }
            }
            while (flag);
            return flag2;
        }
    }
}

