namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;

    internal class NameTable : INameScope
    {
        private Hashtable table = new Hashtable();

        internal void Add(XmlQualifiedName qname, object value)
        {
            this.Add(qname.Name, qname.Namespace, value);
        }

        internal void Add(string name, string ns, object value)
        {
            NameKey key = new NameKey(name, ns);
            this.table.Add(key, value);
        }

        internal Array ToArray(Type type)
        {
            Array array = Array.CreateInstance(type, this.table.Count);
            this.table.Values.CopyTo(array, 0);
            return array;
        }

        internal object this[XmlQualifiedName qname]
        {
            get
            {
                return this.table[new NameKey(qname.Name, qname.Namespace)];
            }
            set
            {
                this.table[new NameKey(qname.Name, qname.Namespace)] = value;
            }
        }

        internal object this[string name, string ns]
        {
            get
            {
                return this.table[new NameKey(name, ns)];
            }
            set
            {
                this.table[new NameKey(name, ns)] = value;
            }
        }

        object INameScope.this[string name, string ns]
        {
            get
            {
                return this.table[new NameKey(name, ns)];
            }
            set
            {
                this.table[new NameKey(name, ns)] = value;
            }
        }

        internal ICollection Values
        {
            get
            {
                return this.table.Values;
            }
        }
    }
}

