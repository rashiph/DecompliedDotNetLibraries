namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class NumberRelationOpcode : LiteralRelationOpcode
    {
        private double literal;
        private RelationOperator op;

        internal NumberRelationOpcode(double literal, RelationOperator op) : this(OpcodeID.NumberRelation, literal, op)
        {
        }

        protected NumberRelationOpcode(OpcodeID id, double literal, RelationOperator op) : base(id)
        {
            this.literal = literal;
            this.op = op;
        }

        internal override bool Equals(Opcode opcode)
        {
            if (!base.Equals(opcode))
            {
                return false;
            }
            NumberRelationOpcode opcode2 = (NumberRelationOpcode) opcode;
            return ((opcode2.op == this.op) && (opcode2.literal == this.literal));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame topArg = context.TopArg;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                values[i].Update(context, values[i].CompareTo(this.literal, this.op));
            }
            return base.next;
        }

        internal Interval ToInterval()
        {
            return new Interval(this.literal, this.op);
        }

        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }
    }
}

