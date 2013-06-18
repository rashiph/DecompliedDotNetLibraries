namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal abstract class MultipleResultOpcode : ResultOpcode
    {
        protected QueryBuffer<object> results;

        internal MultipleResultOpcode(OpcodeID id) : base(id)
        {
            base.flags |= OpcodeFlags.Multiple;
            this.results = new QueryBuffer<object>(1);
        }

        internal override void Add(Opcode op)
        {
            MultipleResultOpcode opcode = op as MultipleResultOpcode;
            if (opcode != null)
            {
                this.results.Add(ref opcode.results);
                this.results.TrimToCount();
            }
            else
            {
                base.Add(op);
            }
        }

        public void AddItem(object item)
        {
            this.results.Add(item);
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.results.Count; i++)
            {
                XPathMessageFilter item = this.results[i] as XPathMessageFilter;
                if (item != null)
                {
                    filters.Add(item);
                }
            }
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && object.ReferenceEquals(this, op));
        }

        internal override void Remove()
        {
            if (this.results.Count == 0)
            {
                base.Remove();
            }
        }

        public void RemoveItem(object item)
        {
            this.results.Remove(item);
            this.Remove();
        }

        internal override void Trim()
        {
            this.results.TrimToCount();
        }
    }
}

