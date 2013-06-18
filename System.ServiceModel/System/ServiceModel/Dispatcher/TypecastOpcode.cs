namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class TypecastOpcode : Opcode
    {
        private ValueDataType newType;

        internal TypecastOpcode(ValueDataType newType) : base(OpcodeID.Cast)
        {
            this.newType = newType;
        }

        internal override bool Equals(Opcode op)
        {
            return (base.Equals(op) && (this.newType == ((TypecastOpcode) op).newType));
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            Value[] values = context.Values;
            for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
            {
                values[i].ConvertTo(context, this.newType);
            }
            return base.next;
        }
    }
}

