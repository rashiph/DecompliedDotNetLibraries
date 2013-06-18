namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class SubExpr
    {
        protected List<SubExpr> children = new List<SubExpr>(2);
        private Opcode ops;
        private SubExpr parent;
        internal int refCount;
        internal bool useSpecial;
        internal int var;

        internal SubExpr(SubExpr parent, Opcode ops, int var)
        {
            this.var = var;
            this.parent = parent;
            this.useSpecial = false;
            if (parent != null)
            {
                this.ops = new InternalSubExprOpcode(parent);
                this.ops.Attach(ops);
                this.useSpecial = (parent is SubExprHeader) && (((SelectOpcode) ops).Criteria.Axis.Type == QueryAxisType.Child);
            }
            else
            {
                this.ops = ops;
            }
        }

        internal SubExprOpcode Add(Opcode opseq, SubExprEliminator elim)
        {
            Opcode firstOp = this.FirstOp;
            Opcode op = opseq;
            while (((firstOp != null) && (op != null)) && firstOp.Equals(op))
            {
                firstOp = firstOp.Next;
                op = op.Next;
            }
            if (op == null)
            {
                if (firstOp == null)
                {
                    return new SubExprOpcode(this);
                }
                return new SubExprOpcode(this.BranchAt(firstOp, elim));
            }
            if (firstOp == null)
            {
                op.DetachFromParent();
                for (int i = 0; i < this.children.Count; i++)
                {
                    if (this.children[i].FirstOp.Equals(op))
                    {
                        return this.children[i].Add(op, elim);
                    }
                }
                SubExpr expr2 = new SubExpr(this, op, elim.NewVarID());
                this.AddChild(expr2);
                return new SubExprOpcode(expr2);
            }
            SubExpr parent = this.BranchAt(firstOp, elim);
            op.DetachFromParent();
            SubExpr expr = new SubExpr(parent, op, elim.NewVarID());
            parent.AddChild(expr);
            return new SubExprOpcode(expr);
        }

        internal virtual void AddChild(SubExpr expr)
        {
            this.children.Add(expr);
        }

        private SubExpr BranchAt(Opcode op, SubExprEliminator elim)
        {
            Opcode firstOp = this.FirstOp;
            if (this.parent != null)
            {
                this.parent.RemoveChild(this);
            }
            else
            {
                elim.Exprs.Remove(this);
            }
            firstOp.DetachFromParent();
            op.DetachFromParent();
            SubExpr expr = new SubExpr(this.parent, firstOp, elim.NewVarID());
            if (this.parent != null)
            {
                this.parent.AddChild(expr);
            }
            else
            {
                elim.Exprs.Add(expr);
            }
            expr.AddChild(this);
            this.parent = expr;
            this.ops = new InternalSubExprOpcode(expr);
            this.ops.Attach(op);
            return expr;
        }

        internal void CleanUp(SubExprEliminator elim)
        {
            if (this.refCount == 0)
            {
                if (this.children.Count == 0)
                {
                    if (this.parent == null)
                    {
                        elim.Exprs.Remove(this);
                    }
                    else
                    {
                        this.parent.RemoveChild(this);
                        this.parent.CleanUp(elim);
                    }
                }
                else if (this.children.Count == 1)
                {
                    SubExpr item = this.children[0];
                    Opcode firstOp = item.FirstOp;
                    firstOp.DetachFromParent();
                    Opcode ops = this.ops;
                    while (ops.Next != null)
                    {
                        ops = ops.Next;
                    }
                    ops.Attach(firstOp);
                    item.ops = this.ops;
                    if (this.parent == null)
                    {
                        elim.Exprs.Remove(this);
                        elim.Exprs.Add(item);
                        item.parent = null;
                    }
                    else
                    {
                        this.parent.RemoveChild(this);
                        this.parent.AddChild(item);
                        item.parent = this.parent;
                    }
                }
            }
        }

        internal void DecRef(SubExprEliminator elim)
        {
            this.refCount--;
            this.CleanUp(elim);
        }

        internal void Eval(ProcessingContext context)
        {
            int count = 0;
            int counterMarker = context.Processor.CounterMarker;
            Opcode ops = this.ops;
            if (!this.useSpecial)
            {
                while (ops != null)
                {
                    ops = ops.Eval(context);
                }
                count = context.Processor.ElapsedCount(counterMarker);
                context.SaveVariable(this.var, count);
            }
            else
            {
                ops.EvalSpecial(context);
                context.LoadVariable(this.var);
            }
        }

        internal virtual void EvalSpecial(ProcessingContext context)
        {
            this.Eval(context);
        }

        internal void IncRef()
        {
            this.refCount++;
        }

        internal virtual void RemoveChild(SubExpr expr)
        {
            this.children.Remove(expr);
        }

        internal void Renumber(SubExprEliminator elim)
        {
            this.var = elim.NewVarID();
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].Renumber(elim);
            }
        }

        internal void Trim()
        {
            this.children.Capacity = this.children.Count;
            this.ops.Trim();
            for (int i = 0; i < this.children.Count; i++)
            {
                this.children[i].Trim();
            }
        }

        internal Opcode FirstOp
        {
            get
            {
                if (this.parent == null)
                {
                    return this.ops;
                }
                return this.ops.Next;
            }
        }

        internal int Variable
        {
            get
            {
                return this.var;
            }
        }
    }
}

