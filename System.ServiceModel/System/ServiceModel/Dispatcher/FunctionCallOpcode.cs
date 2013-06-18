namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class FunctionCallOpcode : Opcode
    {
        private QueryFunction function;

        internal FunctionCallOpcode(QueryFunction function) : base(OpcodeID.Function)
        {
            this.function = function;
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                FunctionCallOpcode opcode = (FunctionCallOpcode) op;
                return opcode.function.Equals(this.function);
            }
            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            this.function.Eval(context);
            return base.next;
        }
    }
}

