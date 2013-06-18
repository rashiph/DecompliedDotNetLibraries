namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class DataSourceDescriptorCollection : CollectionBase
    {
        public int Add(DataSourceDescriptor value)
        {
            return base.List.Add(value);
        }

        public bool Contains(DataSourceDescriptor value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(DataSourceDescriptor[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(DataSourceDescriptor value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, DataSourceDescriptor value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(DataSourceDescriptor value)
        {
            base.List.Remove(value);
        }

        public DataSourceDescriptor this[int index]
        {
            get
            {
                return (DataSourceDescriptor) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

