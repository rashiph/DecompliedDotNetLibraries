namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class ApplyBooleanOpcode : JumpIfOpcode
    {
        internal ApplyBooleanOpcode(Opcode jump, bool test) : this(OpcodeID.ApplyBoolean, jump, test)
        {
        }

        protected ApplyBooleanOpcode(OpcodeID id, Opcode jump, bool test) : base(id, jump, test)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            int num = this.UpdateResultMask(context);
            context.PopFrame();
            if (num == 0)
            {
                return base.Jump;
            }
            return base.next;
        }

        protected int UpdateResultMask(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            StackFrame secondArg = context.SecondArg;
            Value[] values = context.Values;
            int num = 0;
            int basePtr = secondArg.basePtr;
            int index = topArg.basePtr;
            while (basePtr <= secondArg.endPtr)
            {
                if (base.test == values[basePtr].Boolean)
                {
                    bool boolean = values[index].Boolean;
                    if (base.test == boolean)
                    {
                        num++;
                    }
                    values[basePtr].Boolean = boolean;
                    index++;
                }
                basePtr++;
            }
            return num;
        }
    }
}

