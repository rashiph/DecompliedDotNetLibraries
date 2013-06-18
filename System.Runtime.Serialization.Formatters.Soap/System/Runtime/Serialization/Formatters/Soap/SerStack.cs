namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;

    internal sealed class SerStack
    {
        internal int next;
        internal object[] objects = new object[10];
        internal string stackId;
        internal int top = -1;

        internal SerStack(string stackId)
        {
            this.stackId = stackId;
        }

        internal void Clear()
        {
            this.top = -1;
            this.next = 0;
        }

        internal int Count()
        {
            return (this.top + 1);
        }

        [Conditional("SER_LOGGING")]
        internal void Dump()
        {
            for (int i = 0; i < this.Count(); i++)
            {
                object obj1 = this.objects[i];
            }
        }

        internal object GetItem(int index)
        {
            return this.objects[index];
        }

        internal void IncreaseCapacity()
        {
            int num = this.objects.Length * 2;
            object[] destinationArray = new object[num];
            Array.Copy(this.objects, 0, destinationArray, 0, this.objects.Length);
            this.objects = destinationArray;
        }

        internal bool IsEmpty()
        {
            if (this.top > 0)
            {
                return false;
            }
            return true;
        }

        internal object Next()
        {
            if (this.next > this.top)
            {
                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_StackRange"), new object[] { this.stackId }));
            }
            return this.objects[this.next++];
        }

        internal object Peek()
        {
            if (this.top < 0)
            {
                return null;
            }
            return this.objects[this.top];
        }

        internal object PeekPeek()
        {
            if (this.top < 1)
            {
                return null;
            }
            return this.objects[this.top - 1];
        }

        internal object Pop()
        {
            if (this.top < 0)
            {
                return null;
            }
            object obj2 = this.objects[this.top];
            this.objects[this.top--] = null;
            return obj2;
        }

        internal void Push(object obj)
        {
            if (this.top == (this.objects.Length - 1))
            {
                this.IncreaseCapacity();
            }
            this.objects[++this.top] = obj;
        }

        internal void Reverse()
        {
            Array.Reverse(this.objects, 0, this.Count());
        }
    }
}

