namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DesignerActionItemCollection : CollectionBase
    {
        public int Add(DesignerActionItem value)
        {
            return base.List.Add(value);
        }

        public bool Contains(DesignerActionItem value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DesignerActionItem[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DesignerActionItem value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DesignerActionItem value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(DesignerActionItem value)
        {
            base.List.Remove(value);
        }

        public DesignerActionItem this[int index]
        {
            get
            {
                return (DesignerActionItem) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

