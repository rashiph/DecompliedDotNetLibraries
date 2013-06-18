namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class RelationOpcode : Opcode
    {
        protected RelationOperator op;

        internal RelationOpcode(RelationOperator op) : this(OpcodeID.Relation, op)
        {
        }

        protected RelationOpcode(OpcodeID id, RelationOperator op) : base(id)
        {
            this.op = op;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.op == ((RelationOpcode) op).op));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            Value[] values = context.Values;
            while (topArg.basePtr <= topArg.endPtr)
            {
                values[secondArg.basePtr].Update(context, values[secondArg.basePtr].CompareTo(ref values[topArg.basePtr], this.op));
                topArg.basePtr++;
                secondArg.basePtr++;
            }
            context.PopFrame();
            return base.next;
        }
    }
}

