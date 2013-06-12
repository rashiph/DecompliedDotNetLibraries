namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class XmlSchemaObjectTable
    {
        private List<XmlSchemaObjectEntry> entries = new List<XmlSchemaObjectEntry>();
        private Dictionary<XmlQualifiedName, XmlSchemaObject> table = new Dictionary<XmlQualifiedName, XmlSchemaObject>();

        internal XmlSchemaObjectTable()
        {
        }

        internal void Add(XmlQualifiedName name, XmlSchemaObject value)
        {
            this.table.Add(name, value);
            this.entries.Add(new XmlSchemaObjectEntry(name, value));
        }

        internal void Clear()
        {
            this.table.Clear();
            this.entries.Clear();
        }

        public bool Contains(XmlQualifiedName name)
        {
            return this.table.ContainsKey(name);
        }

        private int FindIndexByValue(XmlSchemaObject xso)
        {
            for (int i = 0; i < this.entries.Count; i++)
            {
                if (this.entries[i].xso == xso)
                {
                    return i;
                }
            }
            return -1;
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return new XSODictionaryEnumerator(this.entries, this.table.Count, EnumeratorType.DictionaryEntry);
        }

        internal void Insert(XmlQualifiedName name, XmlSchemaObject value)
        {
            XmlSchemaObject obj2 = null;
            if (this.table.TryGetValue(name, out obj2))
            {
                this.table[name] = value;
                int num = this.FindIndexByValue(obj2);
                this.entries[num] = new XmlSchemaObjectEntry(name, value);
            }
            else
            {
                this.Add(name, value);
            }
        }

        internal void Remove(XmlQualifiedName name)
        {
            XmlSchemaObject obj2;
            if (this.table.TryGetValue(name, out obj2))
            {
                this.table.Remove(name);
                int index = this.FindIndexByValue(obj2);
                this.entries.RemoveAt(index);
            }
        }

        internal void Replace(XmlQualifiedName name, XmlSchemaObject value)
        {
            XmlSchemaObject obj2;
            if (this.table.TryGetValue(name, out obj2))
            {
                this.table[name] = value;
                int num = this.FindIndexByValue(obj2);
                this.entries[num] = new XmlSchemaObjectEntry(name, value);
            }
        }

        public int Count
        {
            get
            {
                return this.table.Count;
            }
        }

        public XmlSchemaObject this[XmlQualifiedName name]
        {
            get
            {
                XmlSchemaObject obj2;
                if (this.table.TryGetValue(name, out obj2))
                {
                    return obj2;
                }
                return null;
            }
        }

        public ICollection Names
        {
            get
            {
                return new NamesCollection(this.entries, this.table.Count);
            }
        }

        public ICollection Values
        {
            get
            {
                return new ValuesCollection(this.entries, this.table.Count);
            }
        }

        internal enum EnumeratorType
        {
            Keys,
            Values,
            DictionaryEntry
        }

        internal class NamesCollection : ICollection, IEnumerable
        {
            private List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries;
            private int size;

            internal NamesCollection(List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries, int size)
            {
                this.entries = entries;
                this.size = size;
            }

            public void CopyTo(Array array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                for (int i = 0; i < this.size; i++)
                {
                    array.SetValue(this.entries[i].qname, arrayIndex++);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new XmlSchemaObjectTable.XSOEnumerator(this.entries, this.size, XmlSchemaObjectTable.EnumeratorType.Keys);
            }

            public int Count
            {
                get
                {
                    return this.size;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return ((ICollection) this.entries).IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return ((ICollection) this.entries).SyncRoot;
                }
            }
        }

        internal class ValuesCollection : ICollection, IEnumerable
        {
            private List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries;
            private int size;

            internal ValuesCollection(List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries, int size)
            {
                this.entries = entries;
                this.size = size;
            }

            public void CopyTo(Array array, int arrayIndex)
            {
                if (array == null)
                {
                    throw new ArgumentNullException("array");
                }
                if (arrayIndex < 0)
                {
                    throw new ArgumentOutOfRangeException("arrayIndex");
                }
                for (int i = 0; i < this.size; i++)
                {
                    array.SetValue(this.entries[i].xso, arrayIndex++);
                }
            }

            public IEnumerator GetEnumerator()
            {
                return new XmlSchemaObjectTable.XSOEnumerator(this.entries, this.size, XmlSchemaObjectTable.EnumeratorType.Values);
            }

            public int Count
            {
                get
                {
                    return this.size;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return ((ICollection) this.entries).IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return ((ICollection) this.entries).SyncRoot;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XmlSchemaObjectEntry
        {
            internal XmlQualifiedName qname;
            internal XmlSchemaObject xso;
            public XmlSchemaObjectEntry(XmlQualifiedName name, XmlSchemaObject value)
            {
                this.qname = name;
                this.xso = value;
            }

            public XmlSchemaObject IsMatch(string localName, string ns)
            {
                if ((localName == this.qname.Name) && (ns == this.qname.Namespace))
                {
                    return this.xso;
                }
                return null;
            }

            public void Reset()
            {
                this.qname = null;
                this.xso = null;
            }
        }

        internal class XSODictionaryEnumerator : XmlSchemaObjectTable.XSOEnumerator, IDictionaryEnumerator, IEnumerator
        {
            internal XSODictionaryEnumerator(List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries, int size, XmlSchemaObjectTable.EnumeratorType enumType) : base(entries, size, enumType)
            {
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (base.currentIndex == -1)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                    }
                    if (base.currentIndex >= base.size)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumFinished", new object[] { string.Empty }));
                    }
                    return new DictionaryEntry(base.currentKey, base.currentValue);
                }
            }

            public object Key
            {
                get
                {
                    if (base.currentIndex == -1)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                    }
                    if (base.currentIndex >= base.size)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumFinished", new object[] { string.Empty }));
                    }
                    return base.currentKey;
                }
            }

            public object Value
            {
                get
                {
                    if (base.currentIndex == -1)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                    }
                    if (base.currentIndex >= base.size)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumFinished", new object[] { string.Empty }));
                    }
                    return base.currentValue;
                }
            }
        }

        internal class XSOEnumerator : IEnumerator
        {
            protected int currentIndex;
            protected XmlQualifiedName currentKey;
            protected XmlSchemaObject currentValue;
            private List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries;
            private XmlSchemaObjectTable.EnumeratorType enumType;
            protected int size;

            internal XSOEnumerator(List<XmlSchemaObjectTable.XmlSchemaObjectEntry> entries, int size, XmlSchemaObjectTable.EnumeratorType enumType)
            {
                this.entries = entries;
                this.size = size;
                this.enumType = enumType;
                this.currentIndex = -1;
            }

            public bool MoveNext()
            {
                if (this.currentIndex >= (this.size - 1))
                {
                    this.currentValue = null;
                    this.currentKey = null;
                    return false;
                }
                this.currentIndex++;
                this.currentValue = this.entries[this.currentIndex].xso;
                this.currentKey = this.entries[this.currentIndex].qname;
                return true;
            }

            public void Reset()
            {
                this.currentIndex = -1;
                this.currentValue = null;
                this.currentKey = null;
            }

            public object Current
            {
                get
                {
                    if (this.currentIndex == -1)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumNotStarted", new object[] { string.Empty }));
                    }
                    if (this.currentIndex >= this.size)
                    {
                        throw new InvalidOperationException(Res.GetString("Sch_EnumFinished", new object[] { string.Empty }));
                    }
                    switch (this.enumType)
                    {
                        case XmlSchemaObjectTable.EnumeratorType.Keys:
                            return this.currentKey;

                        case XmlSchemaObjectTable.EnumeratorType.Values:
                            return this.currentValue;

                        case XmlSchemaObjectTable.EnumeratorType.DictionaryEntry:
                            return new DictionaryEntry(this.currentKey, this.currentValue);
                    }
                    return null;
                }
            }
        }
    }
}

