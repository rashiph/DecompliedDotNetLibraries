namespace Microsoft.JScript
{
    using System;
    using System.Reflection;

    public sealed class MemberInfoList
    {
        internal int count = 0;
        private MemberInfo[] list = new MemberInfo[0x10];

        internal MemberInfoList()
        {
        }

        internal void Add(MemberInfo elem)
        {
            int index = this.count++;
            if (this.list.Length == index)
            {
                this.Grow();
            }
            this.list[index] = elem;
        }

        internal void AddRange(MemberInfo[] elems)
        {
            foreach (MemberInfo info in elems)
            {
                this.Add(info);
            }
        }

        private void Grow()
        {
            MemberInfo[] list = this.list;
            int length = list.Length;
            MemberInfo[] infoArray2 = this.list = new MemberInfo[length + 0x10];
            for (int i = 0; i < length; i++)
            {
                infoArray2[i] = list[i];
            }
        }

        internal MemberInfo[] ToArray()
        {
            int count = this.count;
            MemberInfo[] infoArray = new MemberInfo[count];
            MemberInfo[] list = this.list;
            for (int i = 0; i < count; i++)
            {
                infoArray[i] = list[i];
            }
            return infoArray;
        }

        internal MemberInfo this[int i]
        {
            get
            {
                return this.list[i];
            }
            set
            {
                this.list[i] = value;
            }
        }
    }
}

