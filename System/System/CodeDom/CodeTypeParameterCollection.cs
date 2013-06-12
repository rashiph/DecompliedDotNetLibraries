namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeParameterCollection : CollectionBase
    {
        public CodeTypeParameterCollection()
        {
        }

        public CodeTypeParameterCollection(CodeTypeParameterCollection value)
        {
            this.AddRange(value);
        }

        public CodeTypeParameterCollection(CodeTypeParameter[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeTypeParameter value)
        {
            return base.List.Add(value);
        }

        public void Add(string value)
        {
            this.Add(new CodeTypeParameter(value));
        }

        public void AddRange(CodeTypeParameter[] value)
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

        public void AddRange(CodeTypeParameterCollection value)
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

        public bool Contains(CodeTypeParameter value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeTypeParameter[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeTypeParameter value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeTypeParameter value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeTypeParameter value)
        {
            base.List.Remove(value);
        }

        public CodeTypeParameter this[int index]
        {
            get
            {
                return (CodeTypeParameter) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

