namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct QueryNode
    {
        private SeekableXPathNavigator node;
        private long nodePosition;
        internal QueryNode(SeekableXPathNavigator node)
        {
            this.node = node;
            this.nodePosition = node.CurrentPosition;
        }

        internal string LocalName
        {
            get
            {
                return this.node.GetLocalName(this.nodePosition);
            }
        }
        internal string Name
        {
            get
            {
                return this.node.GetName(this.nodePosition);
            }
        }
        internal string Namespace
        {
            get
            {
                return this.node.GetNamespace(this.nodePosition);
            }
        }
        internal SeekableXPathNavigator Node
        {
            get
            {
                return this.node;
            }
        }
        internal long Position
        {
            get
            {
                return this.nodePosition;
            }
        }
        internal string Value
        {
            get
            {
                return this.node.GetValue(this.nodePosition);
            }
        }
        internal SeekableXPathNavigator MoveTo()
        {
            this.node.CurrentPosition = this.nodePosition;
            return this.node;
        }
    }
}

