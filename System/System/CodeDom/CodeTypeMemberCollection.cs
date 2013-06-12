namespace System.CodeDom
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable, ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public class CodeTypeMemberCollection : CollectionBase
    {
        public CodeTypeMemberCollection()
        {
        }

        public CodeTypeMemberCollection(CodeTypeMemberCollection value)
        {
            this.AddRange(value);
        }

        public CodeTypeMemberCollection(CodeTypeMember[] value)
        {
            this.AddRange(value);
        }

        public int Add(CodeTypeMember value)
        {
            return base.List.Add(value);
        }

        public void AddRange(CodeTypeMember[] value)
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

        public void AddRange(CodeTypeMemberCollection value)
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

        public bool Contains(CodeTypeMember value)
        {
            return base.List.Contains(value);
        }

        public void CopyTo(CodeTypeMember[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public int IndexOf(CodeTypeMember value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, CodeTypeMember value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(CodeTypeMember value)
        {
            base.List.Remove(value);
        }

        public CodeTypeMember this[int index]
        {
            get
            {
                return (CodeTypeMember) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }
    }
}

