namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class XmlSchemaObjectCollection : CollectionBase
    {
        private XmlSchemaObject parent;

        public XmlSchemaObjectCollection()
        {
        }

        public XmlSchemaObjectCollection(XmlSchemaObject parent)
        {
            this.parent = parent;
        }

        public int Add(XmlSchemaObject item)
        {
            return base.List.Add(item);
        }

        private void Add(XmlSchemaObjectCollection collToAdd)
        {
            base.InnerList.InsertRange(0, collToAdd);
        }

        internal XmlSchemaObjectCollection Clone()
        {
            XmlSchemaObjectCollection objects = new XmlSchemaObjectCollection();
            objects.Add(this);
            return objects;
        }

        public bool Contains(XmlSchemaObject item)
        {
            return base.List.Contains(item);
        }

        public void CopyTo(XmlSchemaObject[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public XmlSchemaObjectEnumerator GetEnumerator()
        {
            return new XmlSchemaObjectEnumerator(base.InnerList.GetEnumerator());
        }

        public int IndexOf(XmlSchemaObject item)
        {
            return base.List.IndexOf(item);
        }

        public void Insert(int index, XmlSchemaObject item)
        {
            base.List.Insert(index, item);
        }

        protected override void OnClear()
        {
            if (this.parent != null)
            {
                this.parent.OnClear(this);
            }
        }

        protected override void OnInsert(int index, object item)
        {
            if (this.parent != null)
            {
                this.parent.OnAdd(this, item);
            }
        }

        protected override void OnRemove(int index, object item)
        {
            if (this.parent != null)
            {
                this.parent.OnRemove(this, item);
            }
        }

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            if (this.parent != null)
            {
                this.parent.OnRemove(this, oldValue);
                this.parent.OnAdd(this, newValue);
            }
        }

        public void Remove(XmlSchemaObject item)
        {
            base.List.Remove(item);
        }

        public virtual XmlSchemaObject this[int index]
        {
            get
            {
                return (XmlSchemaObject) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

