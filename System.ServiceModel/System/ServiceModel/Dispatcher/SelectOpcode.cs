namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class SelectOpcode : Opcode
    {
        protected NodeSelectCriteria criteria;

        internal SelectOpcode(NodeSelectCriteria criteria) : this(OpcodeID.Select, criteria)
        {
        }

        internal SelectOpcode(OpcodeID id, NodeSelectCriteria criteria) : this(id, criteria, OpcodeFlags.None)
        {
        }

        internal SelectOpcode(OpcodeID id, NodeSelectCriteria criteria, OpcodeFlags flags) : base(id)
        {
            this.criteria = criteria;
            base.flags |= flags | OpcodeFlags.Select;
            if (criteria.IsCompressable && ((base.flags & OpcodeFlags.InitialSelect) == OpcodeFlags.None))
            {
                base.flags |= OpcodeFlags.CompressableSelect;
            }
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && this.criteria.Equals(((SelectOpcode) op).criteria));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topSequenceArg = context.TopSequenceArg;
            SeekableXPathNavigator contextNode = null;
            Value[] sequences = context.Sequences;
            for (int i = topSequenceArg.basePtr; i <= topSequenceArg.endPtr; i++)
            {
                NodeSequence sequence = sequences[i].Sequence;
                int count = sequence.Count;
                if (count == 0)
                {
                    context.ReplaceSequenceAt(i, NodeSequence.Empty);
                    context.ReleaseSequence(sequence);
                }
                else
                {
                    NodeSequenceItem[] items = sequence.Items;
                    if (sequence.CanReuse(context))
                    {
                        contextNode = items[0].GetNavigator();
                        sequence.Clear();
                        sequence.StartNodeset();
                        this.criteria.Select(contextNode, sequence);
                        sequence.StopNodeset();
                    }
                    else
                    {
                        NodeSequence destSequence = null;
                        for (int j = 0; j < count; j++)
                        {
                            contextNode = items[j].GetNavigator();
                            if (destSequence == null)
                            {
                                destSequence = context.CreateSequence();
                            }
                            destSequence.StartNodeset();
                            this.criteria.Select(contextNode, destSequence);
                            destSequence.StopNodeset();
                        }
                        context.ReplaceSequenceAt(i, (destSequence != null) ? destSequence : NodeSequence.Empty);
                        context.ReleaseSequence(sequence);
                    }
                }
            }
            return base.next;
        }

        internal override Opcode Eval(NodeSequence sequence, SeekableXPathNavigator node)
        {
            if ((base.next != null) && ((base.next.Flags & OpcodeFlags.CompressableSelect) != OpcodeFlags.None))
            {
                return this.criteria.Select(node, sequence, (SelectOpcode) base.next);
            }
            sequence.StartNodeset();
            this.criteria.Select(node, sequence);
            sequence.StopNodeset();
            return base.next;
        }

        internal NodeSelectCriteria Criteria
        {
            get
            {
                return this.criteria;
            }
        }
    }
}

