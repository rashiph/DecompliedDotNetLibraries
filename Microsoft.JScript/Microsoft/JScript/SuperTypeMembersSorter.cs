namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class SuperTypeMembersSorter
    {
        private int count = 0;
        private SimpleHashtable members = new SimpleHashtable(0x40);
        private ArrayList names = new ArrayList();

        internal SuperTypeMembersSorter()
        {
        }

        internal void Add(MemberInfo[] members)
        {
            foreach (MemberInfo info in members)
            {
                this.Add(info);
            }
        }

        internal void Add(MemberInfo member)
        {
            this.count++;
            string name = member.Name;
            object obj2 = this.members[name];
            if (obj2 == null)
            {
                this.members[name] = member;
                this.names.Add(name);
            }
            else if (obj2 is MemberInfo)
            {
                ArrayList list = new ArrayList(8);
                list.Add(obj2);
                list.Add(member);
                this.members[name] = list;
            }
            else
            {
                ((ArrayList) obj2).Add(member);
            }
        }

        internal object[] GetMembers()
        {
            object[] objArray = new object[this.count];
            int num = 0;
            foreach (object obj2 in this.names)
            {
                object obj3 = this.members[obj2];
                if (obj3 is MemberInfo)
                {
                    objArray[num++] = obj3;
                }
                else
                {
                    foreach (object obj4 in (ArrayList) obj3)
                    {
                        objArray[num++] = obj4;
                    }
                }
            }
            return objArray;
        }
    }
}

