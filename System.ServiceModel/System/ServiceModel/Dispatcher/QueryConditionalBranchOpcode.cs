namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    internal class QueryConditionalBranchOpcode : Opcode
    {
        private QueryBranchTable alwaysBranches;
        private QueryBranchIndex branchIndex;
        private int nextID;

        internal QueryConditionalBranchOpcode(OpcodeID id, QueryBranchIndex branchIndex) : base(id)
        {
            base.flags |= OpcodeFlags.Branch;
            this.branchIndex = branchIndex;
            this.nextID = 0;
        }

        internal override void Add(Opcode opcode)
        {
            LiteralRelationOpcode opcode2 = this.ValidateOpcode(opcode);
            if (opcode2 == null)
            {
                base.Add(opcode);
            }
            else
            {
                QueryBranch literalBranch = this.branchIndex[opcode2.Literal];
                if (literalBranch == null)
                {
                    this.nextID++;
                    literalBranch = new QueryBranch(opcode2, this.nextID);
                    opcode2.Prev = this;
                    this.branchIndex[opcode2.Literal] = literalBranch;
                }
                else
                {
                    literalBranch.Branch.Next.Add(opcode2.Next);
                }
                opcode2.Flags |= OpcodeFlags.InConditional;
                this.AddAlwaysBranch(literalBranch, opcode2.Next);
            }
        }

        internal void AddAlwaysBranch(LiteralRelationOpcode literal, Opcode next)
        {
            QueryBranch literalBranch = this.branchIndex[literal.Literal];
            this.AddAlwaysBranch(literalBranch, next);
        }

        internal void AddAlwaysBranch(Opcode literal, Opcode next)
        {
            LiteralRelationOpcode opcode = this.ValidateOpcode(literal);
            if (opcode != null)
            {
                this.AddAlwaysBranch(opcode, next);
            }
        }

        private void AddAlwaysBranch(QueryBranch literalBranch, Opcode next)
        {
            if (OpcodeID.Branch == next.ID)
            {
                BranchOpcode opcode = (BranchOpcode) next;
                OpcodeList branches = opcode.Branches;
                for (int i = 0; i < branches.Count; i++)
                {
                    Opcode opcode2 = branches[i];
                    if (this.IsAlwaysBranch(opcode2))
                    {
                        this.AlwaysBranches.AddInOrder(new QueryBranch(opcode2, literalBranch.ID));
                    }
                    else
                    {
                        opcode2.Flags |= OpcodeFlags.NoContextCopy;
                    }
                }
            }
            else if (this.IsAlwaysBranch(next))
            {
                this.AlwaysBranches.AddInOrder(new QueryBranch(next, literalBranch.ID));
            }
            else
            {
                next.Flags |= OpcodeFlags.NoContextCopy;
            }
        }

        internal virtual void CollectMatches(int valIndex, ref Value val, QueryBranchResultSet results)
        {
            this.branchIndex.Match(valIndex, ref val, results);
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            if (this.alwaysBranches != null)
            {
                for (int i = 0; i < this.alwaysBranches.Count; i++)
                {
                    this.alwaysBranches[i].Branch.CollectXPathFilters(filters);
                }
            }
            this.branchIndex.CollectXPathFilters(filters);
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            int count = topArg.Count;
            if (count > 0)
            {
                QueryBranchResultSet resultTable = context.Processor.CreateResultSet();
                BranchMatcher matcher = new BranchMatcher(count, resultTable);
                for (int i = 0; i < count; i++)
                {
                    this.CollectMatches(i, ref context.Values[topArg[i]], resultTable);
                }
                context.PopFrame();
                if (resultTable.Count > 1)
                {
                    resultTable.Sort();
                }
                if ((this.alwaysBranches != null) && (this.alwaysBranches.Count > 0))
                {
                    matcher.InvokeNonMatches(context, this.alwaysBranches);
                }
                matcher.InvokeMatches(context);
                matcher.Release(context);
            }
            else
            {
                context.PopFrame();
            }
            return base.next;
        }

        internal QueryBranch GetBranch(Opcode op)
        {
            if (op.TestFlag(OpcodeFlags.Literal))
            {
                LiteralRelationOpcode opcode = this.ValidateOpcode(op);
                if (opcode != null)
                {
                    QueryBranch branch = this.branchIndex[opcode.Literal];
                    if ((branch != null) && (branch.Branch.ID == op.ID))
                    {
                        return branch;
                    }
                }
            }
            return null;
        }

        private bool IsAlwaysBranch(Opcode next)
        {
            JumpIfOpcode opcode = next as JumpIfOpcode;
            if (opcode != null)
            {
                if (opcode.Test)
                {
                    Opcode opcode3;
                    Opcode jump = opcode.Jump;
                    if (jump == null)
                    {
                        return false;
                    }
                    if (jump.TestFlag(OpcodeFlags.Branch))
                    {
                        OpcodeList branches = ((BranchOpcode) jump).Branches;
                        for (int i = 0; i < branches.Count; i++)
                        {
                            opcode3 = branches[i].Next;
                            if ((opcode3 != null) && !opcode3.TestFlag(OpcodeFlags.Result))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    opcode3 = opcode.Jump.Next;
                    if ((opcode3 != null) && opcode3.TestFlag(OpcodeFlags.Result))
                    {
                        return false;
                    }
                }
                return true;
            }
            if (OpcodeID.BlockEnd == next.ID)
            {
                return !next.Next.TestFlag(OpcodeFlags.Result);
            }
            return !next.TestFlag(OpcodeFlags.Result);
        }

        internal override bool IsEquivalentForAdd(Opcode opcode)
        {
            return ((this.ValidateOpcode(opcode) != null) || base.IsEquivalentForAdd(opcode));
        }

        internal override Opcode Locate(Opcode opcode)
        {
            QueryBranch branch = this.GetBranch(opcode);
            if (branch != null)
            {
                return branch.Branch;
            }
            return null;
        }

        internal override void Remove()
        {
            if ((this.branchIndex == null) || (this.branchIndex.Count == 0))
            {
                base.Remove();
            }
        }

        internal void RemoveAlwaysBranch(Opcode opcode)
        {
            if (this.alwaysBranches != null)
            {
                if (OpcodeID.Branch == opcode.ID)
                {
                    OpcodeList branches = ((BranchOpcode) opcode).Branches;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        this.alwaysBranches.Remove(branches[i]);
                    }
                }
                else
                {
                    this.alwaysBranches.Remove(opcode);
                }
                if (this.alwaysBranches.Count == 0)
                {
                    this.alwaysBranches = null;
                }
            }
        }

        internal override void RemoveChild(Opcode opcode)
        {
            LiteralRelationOpcode opcode2 = this.ValidateOpcode(opcode);
            QueryBranch branch = this.branchIndex[opcode2.Literal];
            this.branchIndex.Remove(opcode2.Literal);
            Opcode opcode1 = branch.Branch;
            opcode1.Flags &= ~OpcodeFlags.NoContextCopy;
            if (this.alwaysBranches != null)
            {
                int index = this.alwaysBranches.IndexOfID(branch.ID);
                if (index >= 0)
                {
                    this.alwaysBranches.RemoveAt(index);
                    if (this.alwaysBranches.Count == 0)
                    {
                        this.alwaysBranches = null;
                    }
                }
            }
        }

        internal override void Replace(Opcode replace, Opcode with)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new NotImplementedException(System.ServiceModel.SR.GetString("FilterUnexpectedError")));
        }

        internal override void Trim()
        {
            if (this.alwaysBranches != null)
            {
                this.alwaysBranches.Trim();
            }
            this.branchIndex.Trim();
        }

        internal virtual LiteralRelationOpcode ValidateOpcode(Opcode opcode)
        {
            return (opcode as LiteralRelationOpcode);
        }

        internal QueryBranchTable AlwaysBranches
        {
            get
            {
                if (this.alwaysBranches == null)
                {
                    this.alwaysBranches = new QueryBranchTable();
                }
                return this.alwaysBranches;
            }
        }
    }
}

