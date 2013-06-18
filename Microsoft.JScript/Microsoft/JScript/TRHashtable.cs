namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    internal sealed class TRHashtable
    {
        private int count = 0;
        private TypeReflector[] table = new TypeReflector[0x1ff];
        private int threshold = 0x100;

        internal TRHashtable()
        {
        }

        private void Rehash()
        {
            TypeReflector[] table = this.table;
            int num = this.threshold = table.Length + 1;
            int num2 = (num * 2) - 1;
            TypeReflector[] reflectorArray2 = this.table = new TypeReflector[num2];
            int index = num - 1;
            while (index-- > 0)
            {
                TypeReflector next = table[index];
                while (next != null)
                {
                    TypeReflector reflector2 = next;
                    next = next.next;
                    int num4 = (int) (reflector2.hashCode % num2);
                    reflector2.next = reflectorArray2[num4];
                    reflectorArray2[num4] = reflector2;
                }
            }
        }

        internal TypeReflector this[Type type]
        {
            get
            {
                int index = type.GetHashCode() % this.table.Length;
                for (TypeReflector reflector = this.table[index]; reflector != null; reflector = reflector.next)
                {
                    if (reflector.type == type)
                    {
                        return reflector;
                    }
                }
                return null;
            }
            set
            {
                if (++this.count >= this.threshold)
                {
                    this.Rehash();
                }
                int index = (int) (value.hashCode % this.table.Length);
                value.next = this.table[index];
                this.table[index] = value;
            }
        }
    }
}

