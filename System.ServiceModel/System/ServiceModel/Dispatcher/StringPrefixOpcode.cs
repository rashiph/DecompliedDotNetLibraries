namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class StringPrefixOpcode : LiteralRelationOpcode
    {
        private string literal;

        internal StringPrefixOpcode(string literal) : base(OpcodeID.StringPrefix)
        {
            this.literal = literal;
        }

        internal override void Add(Opcode op)
        {
            StringPrefixOpcode opcode = op as StringPrefixOpcode;
            if (opcode == null)
            {
                base.Add(op);
            }
            else
            {
                StringPrefixBranchOpcode with = new StringPrefixBranchOpcode();
                base.prev.Replace(this, with);
                with.Add(this);
                with.Add(opcode);
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                StringPrefixOpcode opcode = (StringPrefixOpcode) op;
                return (opcode.literal == this.literal);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            if (1 == topArg.Count)
            {
                context.Values[topArg.basePtr].Boolean = context.Values[topArg.basePtr].String.StartsWith(this.literal, StringComparison.Ordinal);
            }
            else
            {
                for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
                {
                    context.Values[i].Boolean = context.Values[i].String.StartsWith(this.literal, StringComparison.Ordinal);
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

