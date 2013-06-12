namespace System.Security.Policy
{
    using System;
    using System.Collections;

    internal sealed class CodeGroupStack
    {
        private ArrayList m_array = new ArrayList();

        internal CodeGroupStack()
        {
        }

        internal bool IsEmpty()
        {
            return (this.m_array.Count == 0);
        }

        internal CodeGroupStackFrame Pop()
        {
            if (this.IsEmpty())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_EmptyStack"));
            }
            int count = this.m_array.Count;
            CodeGroupStackFrame frame = (CodeGroupStackFrame) this.m_array[count - 1];
            this.m_array.RemoveAt(count - 1);
            return frame;
        }

        internal void Push(CodeGroupStackFrame element)
        {
            this.m_array.Add(element);
        }
    }
}

