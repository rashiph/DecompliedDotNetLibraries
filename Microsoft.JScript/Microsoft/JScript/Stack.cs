namespace Microsoft.JScript
{
    using System;

    internal sealed class Stack
    {
        private object[] elements = new object[0x20];
        private int top = -1;

        internal Stack()
        {
        }

        internal void GuardedPush(object item)
        {
            if (this.top > 500)
            {
                throw new JScriptException(JSError.OutOfStack);
            }
            if (++this.top >= this.elements.Length)
            {
                object[] target = new object[this.elements.Length + 0x20];
                ArrayObject.Copy(this.elements, target, this.elements.Length);
                this.elements = target;
            }
            this.elements[this.top] = item;
        }

        internal ScriptObject Peek()
        {
            if (this.top < 0)
            {
                return null;
            }
            return (ScriptObject) this.elements[this.top];
        }

        internal object Peek(int i)
        {
            return this.elements[this.top - i];
        }

        internal object Pop()
        {
            object obj2 = this.elements[this.top];
            this.elements[this.top--] = null;
            return obj2;
        }

        internal void Push(object item)
        {
            if (++this.top >= this.elements.Length)
            {
                object[] target = new object[this.elements.Length + 0x20];
                ArrayObject.Copy(this.elements, target, this.elements.Length);
                this.elements = target;
            }
            this.elements[this.top] = item;
        }

        internal int Size()
        {
            return (this.top + 1);
        }

        internal void TrimToSize(int i)
        {
            this.top = i - 1;
        }
    }
}

