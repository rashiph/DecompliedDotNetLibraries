namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    public class BindingElementCollection : Collection<BindingElement>
    {
        public BindingElementCollection()
        {
        }

        public BindingElementCollection(BindingElement[] elements)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            for (int i = 0; i < elements.Length; i++)
            {
                base.Add(elements[i]);
            }
        }

        internal BindingElementCollection(BindingElementCollection elements)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            for (int i = 0; i < elements.Count; i++)
            {
                base.Add(elements[i]);
            }
        }

        public BindingElementCollection(IEnumerable<BindingElement> elements)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            foreach (BindingElement element in elements)
            {
                base.Add(element);
            }
        }

        public void AddRange(params BindingElement[] elements)
        {
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            for (int i = 0; i < elements.Length; i++)
            {
                base.Add(elements[i]);
            }
        }

        public BindingElementCollection Clone()
        {
            BindingElementCollection elements = new BindingElementCollection();
            for (int i = 0; i < base.Count; i++)
            {
                elements.Add(base[i].Clone());
            }
            return elements;
        }

        public bool Contains(System.Type bindingElementType)
        {
            if (bindingElementType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElementType");
            }
            for (int i = 0; i < base.Count; i++)
            {
                if (bindingElementType.IsInstanceOfType(base[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public T Find<T>()
        {
            return this.Find<T>(false);
        }

        private T Find<T>(bool remove)
        {
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i] is T)
                {
                    T local = base[i];
                    if (remove)
                    {
                        base.RemoveAt(i);
                    }
                    return local;
                }
            }
            return default(T);
        }

        public Collection<T> FindAll<T>()
        {
            return this.FindAll<T>(false);
        }

        private Collection<T> FindAll<T>(bool remove)
        {
            Collection<T> collection = new Collection<T>();
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i] is T)
                {
                    T item = base[i];
                    if (remove)
                    {
                        base.RemoveAt(i);
                        i--;
                    }
                    collection.Add(item);
                }
            }
            return collection;
        }

        protected override void InsertItem(int index, BindingElement item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        public T Remove<T>()
        {
            return this.Find<T>(true);
        }

        public Collection<T> RemoveAll<T>()
        {
            return this.FindAll<T>(true);
        }

        protected override void SetItem(int index, BindingElement item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

