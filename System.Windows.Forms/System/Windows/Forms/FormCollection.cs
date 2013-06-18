namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class FormCollection : ReadOnlyCollectionBase
    {
        internal static object CollectionSyncRoot = new object();

        internal void Add(Form form)
        {
            lock (CollectionSyncRoot)
            {
                base.InnerList.Add(form);
            }
        }

        internal bool Contains(Form form)
        {
            lock (CollectionSyncRoot)
            {
                return base.InnerList.Contains(form);
            }
        }

        internal void Remove(Form form)
        {
            lock (CollectionSyncRoot)
            {
                base.InnerList.Remove(form);
            }
        }

        public virtual Form this[string name]
        {
            get
            {
                if (name != null)
                {
                    lock (CollectionSyncRoot)
                    {
                        foreach (Form form in base.InnerList)
                        {
                            if (string.Equals(form.Name, name, StringComparison.OrdinalIgnoreCase))
                            {
                                return form;
                            }
                        }
                    }
                }
                return null;
            }
        }

        public virtual Form this[int index]
        {
            get
            {
                lock (CollectionSyncRoot)
                {
                    return (Form) base.InnerList[index];
                }
            }
        }
    }
}

