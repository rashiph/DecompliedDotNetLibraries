namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Text;

    internal class ConcatFunction : QueryFunction
    {
        private int argCount;

        internal ConcatFunction(int argCount) : base("concat", ValueDataType.String, MakeTypes(argCount))
        {
            this.argCount = argCount;
        }

        internal override bool Equals(QueryFunction function)
        {
            ConcatFunction function2 = function as ConcatFunction;
            return ((function2 != null) && (this.argCount == function2.argCount));
        }

        internal override void Eval(ProcessingContext context)
        {
            StackFrame[] frameArray = new StackFrame[this.argCount];
            for (int i = 0; i < this.argCount; i++)
            {
                frameArray[i] = context[i];
            }
            StringBuilder builder = new StringBuilder();
            while (frameArray[0].basePtr <= frameArray[0].endPtr)
            {
                builder.Length = 0;
                for (int k = 0; k < this.argCount; k++)
                {
                    builder.Append(context.PeekString(frameArray[k].basePtr));
                }
                context.SetValue(context, frameArray[this.argCount - 1].basePtr, builder.ToString());
                for (int m = 0; m < this.argCount; m++)
                {
                    frameArray[m].basePtr++;
                }
            }
            for (int j = 0; j < (this.argCount - 1); j++)
            {
                context.PopFrame();
            }
        }

        internal static ValueDataType[] MakeTypes(int size)
        {
            ValueDataType[] typeArray = new ValueDataType[size];
            for (int i = 0; i < size; i++)
            {
                typeArray[i] = ValueDataType.String;
            }
            return typeArray;
        }
    }
}

