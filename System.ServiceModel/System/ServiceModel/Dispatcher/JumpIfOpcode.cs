namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class JumpIfOpcode : JumpOpcode
    {
        protected bool test;

        internal JumpIfOpcode(Opcode jump, bool test) : this(OpcodeID.JumpIfNot, jump, test)
        {
        }

        protected JumpIfOpcode(OpcodeID id, Opcode jump, bool test) : base(id, jump)
        {
            this.test = test;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.test == ((JumpIfOpcode) op).test));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                if (this.test == context.Values[i].Boolean)
                {
                    return base.next;
                }
            }
            return base.Jump;
        }

        internal bool Test
        {
            get
            {
                return this.test;
            }
        }
    }
}

