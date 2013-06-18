namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class MathOpcode : Opcode
    {
        private MathOperator mathOp;

        internal MathOpcode(OpcodeID id, MathOperator op) : base(id)
        {
            this.mathOp = op;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.mathOp == ((MathOpcode) op).mathOp));
        }
    }
}

