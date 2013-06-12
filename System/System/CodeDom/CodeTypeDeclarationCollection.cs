namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeTypeDeclarationCollection : CollectionBase
    {
        public CodeTypeDeclarationCollection()
        {
        }

        public CodeTypeDeclarationCollection(CodeTypeDeclarationCollection value)
        {
            this.AddRange(value);
        }

        public CodeTypeDeclarationCollection(CodeTypeDeclaration[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeTypeDeclaration value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CodeTypeDeclaration[] value)
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

        public void AddRange(CodeTypeDeclarationCollection value)
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

        public bool Contains(CodeTypeDeclaration value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeTypeDeclaration[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeTypeDeclaration value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeTypeDeclaration value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeTypeDeclaration value)
        {
            base.List.Remove(value);
        }

        public CodeTypeDeclaration this[int index]
        {
            get
            {
                return (CodeTypeDeclaration) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

