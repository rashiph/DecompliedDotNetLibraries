namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;

    internal class MatchMultipleResultOpcode : MultipleResultOpcode
    {
        internal MatchMultipleResultOpcode() : base(OpcodeID.MatchMultipleResult)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topArg = context.TopArg;
            bool flag = false;
            if (1 == topArg.Count)
            {
                flag = context.Values[topArg.basePtr].ToBoolean();
            }
            else
            {
                context.Processor.Result = false;
                for (int i = topArg.basePtr; i <= topArg.endPtr; i++)
                {
                    if (context.Values[i].ToBoolean())
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                ICollection<MessageFilter> matchSet = context.Processor.MatchSet;
                int num2 = 0;
                int count = this.results.Count;
                while (num2 < count)
                {
                    matchSet.Add((MessageFilter) this.results[num2]);
                    num2++;
                }
            }
            context.PopFrame();
            return base.next;
        }
    }
}

