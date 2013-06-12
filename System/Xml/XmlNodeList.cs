namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Reflection;

    public abstract class XmlNodeList : IEnumerable
    {
        protected XmlNodeList()
        {
        }

        public abstract IEnumerator GetEnumerator();
        public abstract XmlNode Item(int index);

        public abstract int Count { get; }

        public virtual XmlNode this[int i]
        {
            get
            {
                return this.Item(i);
            }
        }
    }
}

