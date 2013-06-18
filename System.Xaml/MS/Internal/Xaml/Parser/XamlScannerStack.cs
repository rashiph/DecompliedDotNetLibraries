namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Xaml;

    internal class XamlScannerStack
    {
        private Stack<XamlScannerFrame> _stack = new Stack<XamlScannerFrame>();

        public XamlScannerStack()
        {
            this._stack.Push(new XamlScannerFrame(null, null));
        }

        public void Pop()
        {
            this._stack.Pop();
        }

        public void Push(XamlType type, string ns)
        {
            bool currentXmlSpacePreserve = this.CurrentXmlSpacePreserve;
            this._stack.Push(new XamlScannerFrame(type, ns));
            this.CurrentXmlSpacePreserve = currentXmlSpacePreserve;
        }

        public bool CurrentlyInContent
        {
            get
            {
                return ((this._stack.Count != 0) && this._stack.Peek().InContent);
            }
            set
            {
                this._stack.Peek().InContent = value;
            }
        }

        public XamlMember CurrentProperty
        {
            get
            {
                if (this._stack.Count != 0)
                {
                    return this._stack.Peek().XamlProperty;
                }
                return null;
            }
            set
            {
                this._stack.Peek().XamlProperty = value;
            }
        }

        public XamlType CurrentType
        {
            get
            {
                if (this._stack.Count != 0)
                {
                    return this._stack.Peek().XamlType;
                }
                return null;
            }
        }

        public string CurrentTypeNamespace
        {
            get
            {
                if (this._stack.Count != 0)
                {
                    return this._stack.Peek().TypeNamespace;
                }
                return null;
            }
        }

        public bool CurrentXmlSpacePreserve
        {
            get
            {
                return ((this._stack.Count != 0) && this._stack.Peek().XmlSpacePreserve);
            }
            set
            {
                this._stack.Peek().XmlSpacePreserve = value;
            }
        }

        public int Depth
        {
            get
            {
                return (this._stack.Count - 1);
            }
        }
    }
}

