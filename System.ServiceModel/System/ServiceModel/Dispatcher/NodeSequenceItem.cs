namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NodeSequenceItem
    {
        private NodeSequenceItemFlags flags;
        private QueryNode node;
        private int position;
        private int size;
        internal NodeSequenceItemFlags Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }
        internal bool Last
        {
            get
            {
                return (0 != ((byte) (NodeSequenceItemFlags.NodesetLast & this.flags)));
            }
            set
            {
                if (value)
                {
                    this.flags = (NodeSequenceItemFlags) ((byte) (this.flags | NodeSequenceItemFlags.NodesetLast));
                }
                else
                {
                    this.flags = (NodeSequenceItemFlags) ((byte) (this.flags & ((NodeSequenceItemFlags) 0xfe)));
                }
            }
        }
        internal string LocalName
        {
            get
            {
                return this.node.LocalName;
            }
        }
        internal string Name
        {
            get
            {
                return this.node.Name;
            }
        }
        internal string Namespace
        {
            get
            {
                return this.node.Namespace;
            }
        }
        internal QueryNode Node
        {
            get
            {
                return this.node;
            }
        }
        internal int Position
        {
            get
            {
                return this.position;
            }
        }
        internal int Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }
        internal bool Compare(double dblVal, RelationOperator op)
        {
            return QueryValueModel.Compare(this.NumberValue(), dblVal, op);
        }

        internal bool Compare(string strVal, RelationOperator op)
        {
            return QueryValueModel.Compare(this.StringValue(), strVal, op);
        }

        internal bool Compare(ref NodeSequenceItem item, RelationOperator op)
        {
            return QueryValueModel.Compare(this.StringValue(), item.StringValue(), op);
        }

        internal bool Equals(string literal)
        {
            return QueryValueModel.Equals(this.StringValue(), literal);
        }

        internal bool Equals(double literal)
        {
            return (this.NumberValue() == literal);
        }

        internal SeekableXPathNavigator GetNavigator()
        {
            return this.node.MoveTo();
        }

        internal long GetNavigatorPosition()
        {
            return this.node.Position;
        }

        internal double NumberValue()
        {
            return QueryValueModel.Double(this.StringValue());
        }

        internal void Set(SeekableXPathNavigator node, int position, int size)
        {
            this.node = new QueryNode(node);
            this.position = position;
            this.size = size;
            this.flags = NodeSequenceItemFlags.None;
        }

        internal void Set(QueryNode node, int position, int size)
        {
            this.node = node;
            this.position = position;
            this.size = size;
            this.flags = NodeSequenceItemFlags.None;
        }

        internal void Set(ref NodeSequenceItem item, int position, int size)
        {
            this.node = item.node;
            this.position = position;
            this.size = size;
            this.flags = item.flags;
        }

        internal void SetPositionAndSize(int position, int size)
        {
            this.position = position;
            this.size = size;
            this.flags = (NodeSequenceItemFlags) ((byte) (this.flags & ((NodeSequenceItemFlags) 0xfe)));
        }

        internal void SetSizeAndLast()
        {
            this.size = 1;
            this.flags = (NodeSequenceItemFlags) ((byte) (this.flags | NodeSequenceItemFlags.NodesetLast));
        }

        internal string StringValue()
        {
            return this.node.Value;
        }
    }
}

