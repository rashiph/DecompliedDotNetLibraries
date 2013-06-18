namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using System.Xml.XPath;

    internal class NodeSequence
    {
        private int count;
        internal static NodeSequence Empty = new NodeSequence(0);
        private NodeSequenceItem[] items;
        private NodeSequence next;
        private ProcessingContext ownerContext;
        private int position;
        internal int refCount;
        private int sizePosition;
        private static readonly QueryNodeComparer staticQueryNodeComparerInstance = new QueryNodeComparer();

        internal NodeSequence() : this(8, null)
        {
        }

        internal NodeSequence(int capacity) : this(capacity, null)
        {
        }

        internal NodeSequence(int capacity, ProcessingContext ownerContext)
        {
            this.items = new NodeSequenceItem[capacity];
            this.ownerContext = ownerContext;
        }

        internal void Add(QueryNode node)
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.position++;
            this.items[this.count++].Set(node, this.position, this.sizePosition);
        }

        internal void Add(SeekableXPathNavigator node)
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.position++;
            this.items[this.count++].Set(node, this.position, this.sizePosition);
        }

        internal void Add(XPathNodeIterator iter)
        {
            while (iter.MoveNext())
            {
                SeekableXPathNavigator current = iter.Current as SeekableXPathNavigator;
                if (current == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, System.ServiceModel.SR.GetString("QueryMustBeSeekable")));
                }
                this.Add(current);
            }
        }

        internal void Add(ref NodeSequenceItem item)
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.position++;
            this.items[this.count++].Set(ref item, this.position, this.sizePosition);
        }

        internal void AddCopy(ref NodeSequenceItem item)
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.items[this.count++] = item;
        }

        internal void AddCopy(ref NodeSequenceItem item, int size)
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.items[this.count] = item;
            this.items[this.count++].Size = size;
        }

        internal bool CanReuse(ProcessingContext context)
        {
            return (((this.count == 1) && (this.ownerContext == context)) && (this.refCount == 1));
        }

        internal void Clear()
        {
            this.count = 0;
        }

        internal bool Compare(double val, RelationOperator op)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i].Compare(val, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(NodeSequence sequence, RelationOperator op)
        {
            for (int i = 0; i < sequence.count; i++)
            {
                if (this.Compare(ref sequence.items[i], op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(string val, RelationOperator op)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i].Compare(val, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(ref NodeSequenceItem item, RelationOperator op)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i].Compare(ref item, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Equals(double val)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i].Equals(val))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Equals(string val)
        {
            for (int i = 0; i < this.count; i++)
            {
                if (this.items[i].Equals(val))
                {
                    return true;
                }
            }
            return false;
        }

        internal static int GetContextSize(NodeSequence sequence, int itemIndex)
        {
            int size = sequence.items[itemIndex].Size;
            if (size <= 0)
            {
                return sequence.items[-size].Size;
            }
            return size;
        }

        private void Grow(int newSize)
        {
            NodeSequenceItem[] destinationArray = new NodeSequenceItem[newSize];
            if (this.items != null)
            {
                Array.Copy(this.items, destinationArray, this.items.Length);
            }
            this.items = destinationArray;
        }

        internal void Merge()
        {
            this.Merge(true);
        }

        internal void Merge(bool renumber)
        {
            if ((this.count != 0) && renumber)
            {
                this.RenumberItems();
            }
        }

        private void RenumberItems()
        {
            if (this.count > 0)
            {
                for (int i = 0; i < this.count; i++)
                {
                    this.items[i].SetPositionAndSize(i + 1, this.count);
                }
                this.items[this.count - 1].Flags = (NodeSequenceItemFlags) ((byte) (this.items[this.count - 1].Flags | NodeSequenceItemFlags.NodesetLast));
            }
        }

        internal void Reset(NodeSequence nextSeq)
        {
            this.count = 0;
            this.refCount = 0;
            this.next = nextSeq;
        }

        internal void StartNodeset()
        {
            this.position = 0;
            this.sizePosition = -this.count;
        }

        internal void StopNodeset()
        {
            switch (this.position)
            {
                case 0:
                    break;

                case 1:
                    this.items[-this.sizePosition].SetSizeAndLast();
                    break;

                default:
                {
                    int index = -this.sizePosition;
                    this.items[index].Size = this.position;
                    this.items[(index + this.position) - 1].Last = true;
                    return;
                }
            }
        }

        internal string StringValue()
        {
            if (this.count > 0)
            {
                return this.items[0].StringValue();
            }
            return string.Empty;
        }

        internal NodeSequence Union(ProcessingContext context, NodeSequence otherSeq)
        {
            NodeSequence sequence = context.CreateSequence();
            SortedBuffer<QueryNode, QueryNodeComparer> buffer = new SortedBuffer<QueryNode, QueryNodeComparer>(staticQueryNodeComparerInstance);
            for (int i = 0; i < this.count; i++)
            {
                buffer.Add(this.items[i].Node);
            }
            for (int j = 0; j < otherSeq.count; j++)
            {
                buffer.Add(otherSeq.items[j].Node);
            }
            for (int k = 0; k < buffer.Count; k++)
            {
                sequence.Add(buffer[k]);
            }
            sequence.RenumberItems();
            return sequence;
        }

        internal int Count
        {
            get
            {
                return this.count;
            }
        }

        internal bool IsNotEmpty
        {
            get
            {
                return (this.count > 0);
            }
        }

        internal NodeSequenceItem this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

        internal NodeSequenceItem[] Items
        {
            get
            {
                return this.items;
            }
        }

        internal string LocalName
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].LocalName;
                }
                return string.Empty;
            }
        }

        internal string Name
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].Name;
                }
                return string.Empty;
            }
        }

        internal string Namespace
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].Namespace;
                }
                return string.Empty;
            }
        }

        internal NodeSequence Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal ProcessingContext OwnerContext
        {
            get
            {
                return this.ownerContext;
            }
            set
            {
                this.ownerContext = value;
            }
        }
    }
}

