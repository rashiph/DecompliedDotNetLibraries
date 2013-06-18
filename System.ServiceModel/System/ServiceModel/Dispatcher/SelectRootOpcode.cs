namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class SelectRootOpcode : Opcode
    {
        internal SelectRootOpcode() : base(OpcodeID.SelectRoot)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            int iterationCount = context.IterationCount;
            Opcode next = base.next;
            context.PushSequenceFrame();
            NodeSequence empty = context.CreateSequence();
            if ((base.next != null) && ((base.next.Flags & OpcodeFlags.CompressableSelect) != OpcodeFlags.None))
            {
                SeekableXPathNavigator contextNode = context.Processor.ContextNode;
                contextNode.MoveToRoot();
                next = base.next.Eval(empty, contextNode);
                while ((next != null) && ((next.Flags & OpcodeFlags.CompressableSelect) != OpcodeFlags.None))
                {
                    next = next.Next;
                }
            }
            else
            {
                empty.StartNodeset();
                SeekableXPathNavigator node = context.Processor.ContextNode;
                node.MoveToRoot();
                empty.Add(node);
                empty.StopNodeset();
            }
            if (empty.Count == 0)
            {
                context.ReleaseSequence(empty);
                empty = NodeSequence.Empty;
            }
            for (int i = 0; i < iterationCount; i++)
            {
                context.PushSequence(empty);
            }
            if (iterationCount > 1)
            {
                empty.refCount += iterationCount - 1;
            }
            return next;
        }
    }
}

