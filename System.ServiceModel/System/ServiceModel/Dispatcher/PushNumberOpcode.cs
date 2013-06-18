namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class PushNumberOpcode : Opcode
    {
        private double literal;

        internal PushNumberOpcode(double literal) : base(OpcodeID.PushDouble)
        {
            this.literal = literal;
            base.flags |= OpcodeFlags.Literal;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.literal == ((PushNumberOpcode) op).literal));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            context.PushFrame();
            int iterationCount = context.IterationCount;
            if (iterationCount > 0)
            {
                context.Push(this.literal, iterationCount);
            }
            return base.next;
        }
    }
}

