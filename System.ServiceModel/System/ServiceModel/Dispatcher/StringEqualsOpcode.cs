namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class StringEqualsOpcode : LiteralRelationOpcode
    {
        private string literal;

        internal StringEqualsOpcode(string literal) : base(OpcodeID.StringEquals)
        {
            this.literal = literal;
        }

        internal override void Add(Opcode op)
        {
            StringEqualsOpcode opcode = op as StringEqualsOpcode;
            if (opcode == null)
            {
                base.Add(op);
            }
            else
            {
                StringEqualsBranchOpcode with = new StringEqualsBranchOpcode();
                base.prev.Replace(this, with);
                with.Add(this);
                with.Add(opcode);
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                StringEqualsOpcode opcode = (StringEqualsOpcode) op;
                return (opcode.literal == this.literal);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            Value[] values = context.Values;
            StackFrame topArg = context.TopArg;
            if (1 == topArg.Count)
            {
                values[topArg.basePtr].Update(context, values[topArg.basePtr].Equals(this.literal));
            }
            else
            {
                for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
                {
                    values[i].Update(context, values[i].Equals(this.literal));
                }
            }
            return base.next;
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

