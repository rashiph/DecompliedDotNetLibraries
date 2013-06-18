namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal abstract class Opcode
    {
        protected OpcodeFlags flags;
        protected Opcode next;
        private OpcodeID opcodeID;
        protected Opcode prev;

        internal Opcode(OpcodeID id)
        {
            this.opcodeID = id;
            this.flags = OpcodeFlags.Single;
        }

        internal virtual void Add(Opcode op)
        {
            this.prev.AddBranch(op);
        }

        internal virtual void AddBranch(Opcode opcode)
        {
            Opcode next = this.next;
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                this.DelinkFromConditional(next);
            }
            BranchOpcode op = new BranchOpcode();
            this.next = null;
            this.Attach(op);
            if (next != null)
            {
                op.Add(next);
            }
            op.Add(opcode);
        }

        internal void Attach(Opcode op)
        {
            this.next = op;
            op.prev = this;
        }

        internal virtual void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.next != null)
            {
                this.next.CollectXPathFilters(filters);
            }
        }

        internal virtual void DelinkFromConditional(Opcode child)
        {
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                ((QueryConditionalBranchOpcode) this.prev).RemoveAlwaysBranch(child);
            }
        }

        internal Opcode DetachChild()
        {
            Opcode next = this.next;
            if ((next != null) && this.IsInConditional())
            {
                this.DelinkFromConditional(next);
            }
            this.next = null;
            next.prev = null;
            return next;
        }

        internal void DetachFromParent()
        {
            Opcode prev = this.prev;
            if (prev != null)
            {
                prev.DetachChild();
            }
        }

        internal virtual bool Equals(Opcode op)
        {
            return (op.ID == this.ID);
        }

        internal virtual Opcode Eval(ProcessingContext context)
        {
            return this.next;
        }

        internal virtual Opcode Eval(NodeSequence sequence, SeekableXPathNavigator node)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
        }

        internal virtual Opcode EvalSpecial(ProcessingContext context)
        {
            return this.Eval(context);
        }

        internal virtual bool IsEquivalentForAdd(Opcode opcode)
        {
            return (this.ID == opcode.ID);
        }

        internal virtual bool IsInConditional()
        {
            return this.TestFlag(OpcodeFlags.InConditional);
        }

        internal bool IsMultipleResult()
        {
            return ((this.flags & (OpcodeFlags.Result | OpcodeFlags.Multiple)) == (OpcodeFlags.Result | OpcodeFlags.Multiple));
        }

        internal bool IsReachableFromConditional()
        {
            return ((this.prev != null) && this.prev.IsInConditional());
        }

        internal virtual void LinkToConditional(Opcode child)
        {
            if (this.TestFlag(OpcodeFlags.InConditional))
            {
                ((QueryConditionalBranchOpcode) this.prev).AddAlwaysBranch(this, child);
            }
        }

        internal virtual Opcode Locate(Opcode opcode)
        {
            if ((this.next != null) && this.next.Equals(opcode))
            {
                return this.next;
            }
            return null;
        }

        internal virtual void Remove()
        {
            if (this.next == null)
            {
                Opcode prev = this.prev;
                if (prev != null)
                {
                    prev.RemoveChild(this);
                    prev.Remove();
                }
            }
        }

        internal virtual void RemoveChild(Opcode opcode)
        {
            if (this.IsInConditional())
            {
                this.DelinkFromConditional(opcode);
            }
            opcode.prev = null;
            this.next = null;
            opcode.Flags |= OpcodeFlags.Deleted;
        }

        internal virtual void Replace(Opcode replace, Opcode with)
        {
            if (this.next == replace)
            {
                bool flag = this.IsInConditional();
                if (flag)
                {
                    this.DelinkFromConditional(this.next);
                }
                this.next.prev = null;
                this.next = with;
                with.prev = this;
                if (flag)
                {
                    this.LinkToConditional(with);
                }
            }
        }

        internal bool TestFlag(OpcodeFlags flag)
        {
            return (OpcodeFlags.None != (this.flags & flag));
        }

        internal virtual void Trim()
        {
            if (this.next != null)
            {
                this.next.Trim();
            }
        }

        internal OpcodeFlags Flags
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

        internal OpcodeID ID
        {
            get
            {
                return this.opcodeID;
            }
        }

        internal Opcode Next
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

        internal Opcode Prev
        {
            get
            {
                return this.prev;
            }
            set
            {
                this.prev = value;
            }
        }
    }
}

