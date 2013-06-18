namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal sealed class AttributeList
    {
        private SerStack nameA = new SerStack("AttributeName");
        private SerStack valueA = new SerStack("AttributeValue");

        internal void Clear()
        {
            this.nameA.Clear();
            this.valueA.Clear();
        }

        [Conditional("SER_LOGGING")]
        internal void Dump()
        {
        }

        internal void Get(int index, out string name, out string value)
        {
            name = (string) this.nameA.Next();
            value = (string) this.valueA.Next();
        }

        internal void Put(string name, string value)
        {
            this.nameA.Push(name);
            this.valueA.Push(value);
        }

        internal int Count
        {
            get
            {
                return this.nameA.Count();
            }
        }
    }
}

