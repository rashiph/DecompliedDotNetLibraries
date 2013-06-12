namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class WorkItems
    {
        private ArrayList list = new ArrayList();

        internal void Add(ImportStructWorkItem item)
        {
            this.list.Add(item);
        }

        internal bool Contains(StructMapping mapping)
        {
            return (this.IndexOf(mapping) >= 0);
        }

        internal int IndexOf(StructMapping mapping)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Mapping == mapping)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        internal int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        internal ImportStructWorkItem this[int index]
        {
            get
            {
                return (ImportStructWorkItem) this.list[index];
            }
            set
            {
                this.list[index] = value;
            }
        }
    }
}

