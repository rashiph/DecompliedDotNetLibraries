namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeAttributeDeclarationCollection : CollectionBase
    {
        public CodeAttributeDeclarationCollection()
        {
        }

        public CodeAttributeDeclarationCollection(CodeAttributeDeclarationCollection value)
        {
            this.AddRange(value);
        }

        public CodeAttributeDeclarationCollection(CodeAttributeDeclaration[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeAttributeDeclaration value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CodeAttributeDeclaration[] value)
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

        public void AddRange(CodeAttributeDeclarationCollection value)
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

        public bool Contains(CodeAttributeDeclaration value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeAttributeDeclaration[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeAttributeDeclaration value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeAttributeDeclaration value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeAttributeDeclaration value)
        {
            base.List.Remove(value);
        }

        public CodeAttributeDeclaration this[int index]
        {
            get
            {
                return (CodeAttributeDeclaration) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

