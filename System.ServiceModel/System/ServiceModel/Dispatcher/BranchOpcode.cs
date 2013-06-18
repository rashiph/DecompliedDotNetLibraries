namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class BranchOpcode : Opcode
    {
        private OpcodeList branches;

        internal BranchOpcode() : this(OpcodeID.Branch)
        {
        }

        internal BranchOpcode(OpcodeID id) : base(id)
        {
            base.flags |= OpcodeFlags.Branch;
            this.branches = new OpcodeList(2);
        }

        internal override void Add(Opcode opcode)
        {
            for (int i = 0; i < this.branches.Count; i++)
            {
                if (this.branches[i].IsEquivalentForAdd(opcode))
                {
                    this.branches[i].Add(opcode);
                    return;
                }
            }
            this.AddBranch(opcode);
        }

        internal override void AddBranch(Opcode opcode)
        {
            this.branches.Add(opcode);
            opcode.Prev = this;
            if (this.IsInConditional())
            {
                this.LinkToConditional(opcode);
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            for (int i = 0; i < this.branches.Count; i++)
            {
                this.branches[i].CollectXPathFilters(filters);
            }
        }

        internal override void DelinkFromConditional(Opcode child)
        {
            if (base.prev != null)
            {
                base.prev.DelinkFromConditional(child);
            }
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            QueryProcessor processor = context.Processor;
            SeekableXPathNavigator contextNode = processor.ContextNode;
            int counterMarker = processor.CounterMarker;
            long currentPosition = contextNode.CurrentPosition;
            int num3 = 0;
            int count = this.branches.Count;
            try
            {
                Opcode opcode;
                if (context.StacksInUse)
                {
                    if (--count > 0)
                    {
                        BranchContext context2 = new BranchContext(context);
                        while (num3 < count)
                        {
                            opcode = this.branches[num3];
                            if ((opcode.Flags & OpcodeFlags.Fx) != OpcodeFlags.None)
                            {
                                opcode.Eval(context);
                            }
                            else
                            {
                                ProcessingContext context3 = context2.Create();
                                while (opcode != null)
                                {
                                    opcode = opcode.Eval(context3);
                                }
                            }
                            contextNode.CurrentPosition = currentPosition;
                            processor.CounterMarker = counterMarker;
                            num3++;
                        }
                        context2.Release();
                    }
                    opcode = this.branches[num3];
                    while (opcode != null)
                    {
                        opcode = opcode.Eval(context);
                    }
                }
                else
                {
                    int nodeCount = context.NodeCount;
                    while (num3 < count)
                    {
                        for (opcode = this.branches[num3]; opcode != null; opcode = opcode.Eval(context))
                        {
                        }
                        context.ClearContext();
                        context.NodeCount = nodeCount;
                        contextNode.CurrentPosition = currentPosition;
                        processor.CounterMarker = counterMarker;
                        num3++;
                    }
                }
            }
            catch (XPathNavigatorException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception.Process(this.branches[num3]));
            }
            catch (NavigatorInvalidBodyAccessException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2.Process(this.branches[num3]));
            }
            processor.CounterMarker = counterMarker;
            return base.next;
        }

        internal override bool IsInConditional()
        {
            if (base.prev != null)
            {
                return base.prev.IsInConditional();
            }
            return true;
        }

        internal override void LinkToConditional(Opcode child)
        {
            if (base.prev != null)
            {
                base.prev.LinkToConditional(child);
            }
        }

        internal override Opcode Locate(Opcode opcode)
        {
            int num = 0;
            int count = this.branches.Count;
            while (num < count)
            {
                Opcode opcode2 = this.branches[num];
                if (opcode2.TestFlag(OpcodeFlags.Branch))
                {
                    Opcode opcode3 = opcode2.Locate(opcode);
                    if (opcode3 != null)
                    {
                        return opcode3;
                    }
                }
                else if (opcode2.Equals(opcode))
                {
                    return opcode2;
                }
                num++;
            }
            return null;
        }

        internal override void Remove()
        {
            if (this.branches.Count == 0)
            {
                base.Remove();
            }
        }

        internal override void RemoveChild(Opcode opcode)
        {
            if (this.IsInConditional())
            {
                this.DelinkFromConditional(opcode);
            }
            this.branches.Remove(opcode);
            this.branches.Trim();
        }

        internal override void Replace(Opcode replace, Opcode with)
        {
            int index = this.branches.IndexOf(replace);
            if (index >= 0)
            {
                replace.Prev = null;
                this.branches[index] = with;
                with.Prev = this;
            }
        }

        internal override void Trim()
        {
            this.branches.Trim();
            for (int i = 0; i < this.branches.Count; i++)
            {
                this.branches[i].Trim();
            }
        }

        internal OpcodeList Branches
        {
            get
            {
                return this.branches;
            }
        }
    }
}

