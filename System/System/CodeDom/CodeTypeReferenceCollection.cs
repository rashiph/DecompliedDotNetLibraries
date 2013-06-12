namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeReferenceCollection : CollectionBase
    {
        public CodeTypeReferenceCollection()
        {
        }

        public CodeTypeReferenceCollection(CodeTypeReferenceCollection value)
        {
            this.AddRange(value);
        }

        public CodeTypeReferenceCollection(CodeTypeReference[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeTypeReference value)
        {
            return base.List.Add(value);
        }

        public void Add(string value)
        {
            this.Add(new CodeTypeReference(value));
        }

        public void Add(Type value)
        {
            this.Add(new CodeTypeReference(value));
        }

        public void AddRange(CodeTypeReference[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(CodeTypeReferenceCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int count = value.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(CodeTypeReference value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeTypeReference[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeTypeReference value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeTypeReference value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeTypeReference value)
        {
            base.List.Remove(value);
        }

        public CodeTypeReference this[int index]
        {
            get
            {
                return (CodeTypeReference) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

