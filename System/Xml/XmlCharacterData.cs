namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Text;
    using System.Xml.XPath;

    public abstract class XmlCharacterData : XmlLinkedNode
    {
        private string data;

        protected internal XmlCharacterData(string data, XmlDocument doc) : base(doc)
        {
            this.data = data;
        }

        public virtual void AppendData(string strData)
        {
            XmlNode parentNode = this.ParentNode;
            int capacity = (this.data != null) ? this.data.Length : 0;
            if (strData != null)
            {
                capacity += strData.Length;
            }
            string newValue = new StringBuilder(capacity).Append(this.data).Append(strData).ToString();
            XmlNodeChangedEventArgs args = this.GetEventArgs(this, parentNode, parentNode, this.data, newValue, XmlNodeChangedAction.Change);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            this.data = newValue;
            if (args != null)
            {
                this.AfterEvent(args);
            }
        }

        internal bool CheckOnData(string data)
        {
            return XmlCharType.Instance.IsOnlyWhitespace(data);
        }

        internal bool DecideXPNodeTypeForTextNodes(XmlNode node, ref XPathNodeType xnt)
        {
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        xnt = XPathNodeType.Text;
                        return false;

                    case XmlNodeType.EntityReference:
                        if (this.DecideXPNodeTypeForTextNodes(node.FirstChild, ref xnt))
                        {
                            break;
                        }
                        return false;

                    case XmlNodeType.Whitespace:
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        xnt = XPathNodeType.SignificantWhitespace;
                        break;

                    default:
                        return false;
                }
                node = node.NextSibling;
            }
            return true;
        }

        public virtual void DeleteData(int offset, int count)
        {
            int num = (this.data != null) ? this.data.Length : 0;
            if ((num > 0) && (num < (offset + count)))
            {
                count = Math.Max(num - offset, 0);
            }
            string newValue = new StringBuilder(this.data).Remove(offset, count).ToString();
            XmlNode parentNode = this.ParentNode;
            XmlNodeChangedEventArgs args = this.GetEventArgs(this, parentNode, parentNode, this.data, newValue, XmlNodeChangedAction.Change);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            this.data = newValue;
            if (args != null)
            {
                this.AfterEvent(args);
            }
        }

        public virtual void InsertData(int offset, string strData)
        {
            XmlNode parentNode = this.ParentNode;
            int capacity = (this.data != null) ? this.data.Length : 0;
            if (strData != null)
            {
                capacity += strData.Length;
            }
            string newValue = new StringBuilder(capacity).Append(this.data).Insert(offset, strData).ToString();
            XmlNodeChangedEventArgs args = this.GetEventArgs(this, parentNode, parentNode, this.data, newValue, XmlNodeChangedAction.Change);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            this.data = newValue;
            if (args != null)
            {
                this.AfterEvent(args);
            }
        }

        public virtual void ReplaceData(int offset, int count, string strData)
        {
            int num = (this.data != null) ? this.data.Length : 0;
            if ((num > 0) && (num < (offset + count)))
            {
                count = Math.Max(num - offset, 0);
            }
            string newValue = new StringBuilder(this.data).Remove(offset, count).Insert(offset, strData).ToString();
            XmlNode parentNode = this.ParentNode;
            XmlNodeChangedEventArgs args = this.GetEventArgs(this, parentNode, parentNode, this.data, newValue, XmlNodeChangedAction.Change);
            if (args != null)
            {
                this.BeforeEvent(args);
            }
            this.data = newValue;
            if (args != null)
            {
                this.AfterEvent(args);
            }
        }

        public virtual string Substring(int offset, int count)
        {
            int num = (this.data != null) ? this.data.Length : 0;
            if (num <= 0)
            {
                return string.Empty;
            }
            if (num < (offset + count))
            {
                count = num - offset;
            }
            return this.data.Substring(offset, count);
        }

        public virtual string Data
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.data != null)
                {
                    return this.data;
                }
                return string.Empty;
            }
            set
            {
                XmlNode parentNode = this.ParentNode;
                XmlNodeChangedEventArgs args = this.GetEventArgs(this, parentNode, parentNode, this.data, value, XmlNodeChangedAction.Change);
                if (args != null)
                {
                    this.BeforeEvent(args);
                }
                this.data = value;
                if (args != null)
                {
                    this.AfterEvent(args);
                }
            }
        }

        public override string InnerText
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = value;
            }
        }

        public virtual int Length
        {
            get
            {
                if (this.data != null)
                {
                    return this.data.Length;
                }
                return 0;
            }
        }

        public override string Value
        {
            get
            {
                return this.Data;
            }
            set
            {
                this.Data = value;
            }
        }
    }
}

