namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class TypeReflector : ScriptObject
    {
        private MemberInfo[] defaultMembers;
        private static MemberInfo[] EmptyMembers = new MemberInfo[0];
        internal uint hashCode;
        private object implementsIReflect;
        private SimpleHashtable instanceMembers;
        private object is__ComObject;
        private MemberInfo[][] memberInfos;
        private ArrayList memberLookupTable;
        internal TypeReflector next;
        private SimpleHashtable staticMembers;
        private static TRHashtable Table = new TRHashtable();
        internal Type type;

        internal TypeReflector(Type type) : base(null)
        {
            this.defaultMembers = null;
            ArrayList list = new ArrayList(0x200);
            int num = 0;
            SimpleHashtable hashtable = new SimpleHashtable(0x100);
            foreach (MemberInfo info in type.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static))
            {
                string name = info.Name;
                object obj2 = hashtable[name];
                if (obj2 == null)
                {
                    hashtable[name] = num++;
                    list.Add(info);
                }
                else
                {
                    int num2 = (int) obj2;
                    obj2 = list[num2];
                    MemberInfo elem = obj2 as MemberInfo;
                    if (elem != null)
                    {
                        MemberInfoList list2 = new MemberInfoList();
                        list2.Add(elem);
                        list2.Add(info);
                        list[num2] = list2;
                    }
                    else
                    {
                        ((MemberInfoList) obj2).Add(info);
                    }
                }
            }
            this.staticMembers = hashtable;
            SimpleHashtable hashtable2 = new SimpleHashtable(0x100);
            foreach (MemberInfo info3 in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                string str2 = info3.Name;
                object obj3 = hashtable2[str2];
                if (obj3 == null)
                {
                    hashtable2[str2] = num++;
                    list.Add(info3);
                }
                else
                {
                    int num3 = (int) obj3;
                    obj3 = list[num3];
                    MemberInfo info4 = obj3 as MemberInfo;
                    if (info4 != null)
                    {
                        MemberInfoList list3 = new MemberInfoList();
                        list3.Add(info4);
                        list3.Add(info3);
                        list[num3] = list3;
                    }
                    else
                    {
                        ((MemberInfoList) obj3).Add(info3);
                    }
                }
            }
            this.instanceMembers = hashtable2;
            this.memberLookupTable = list;
            this.memberInfos = new MemberInfo[num][];
            this.type = type;
            this.implementsIReflect = null;
            this.is__ComObject = null;
            this.hashCode = (uint) type.GetHashCode();
            this.next = null;
        }

        internal MemberInfo[] GetDefaultMembers()
        {
            MemberInfo[] defaultMembers = this.defaultMembers;
            if (defaultMembers == null)
            {
                defaultMembers = JSBinder.GetDefaultMembers(this.type);
                if (defaultMembers == null)
                {
                    defaultMembers = new MemberInfo[0];
                }
                WrapMembers(this.defaultMembers = defaultMembers);
            }
            return defaultMembers;
        }

        public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
        {
            bool flag = (bindingAttr & BindingFlags.Instance) != BindingFlags.Default;
            SimpleHashtable hashtable = flag ? this.instanceMembers : this.staticMembers;
            object obj2 = hashtable[name];
            if (obj2 == null)
            {
                if ((bindingAttr & BindingFlags.IgnoreCase) != BindingFlags.Default)
                {
                    obj2 = hashtable.IgnoreCaseGet(name);
                }
                if (obj2 == null)
                {
                    if (flag && ((bindingAttr & BindingFlags.Static) != BindingFlags.Default))
                    {
                        return this.GetMember(name, bindingAttr & ~BindingFlags.Instance);
                    }
                    return EmptyMembers;
                }
            }
            int index = (int) obj2;
            MemberInfo[] infoArray = this.memberInfos[index];
            if (infoArray == null)
            {
                return this.GetNewMemberArray(name, index);
            }
            return infoArray;
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new JScriptException(JSError.InternalError);
        }

        private MemberInfo[] GetNewMemberArray(string name, int index)
        {
            MemberInfo[] members = null;
            object obj2 = this.memberLookupTable[index];
            if (obj2 == null)
            {
                return this.memberInfos[index];
            }
            MemberInfo info = obj2 as MemberInfo;
            if (info != null)
            {
                members = new MemberInfo[] { info };
            }
            else
            {
                members = ((MemberInfoList) obj2).ToArray();
            }
            this.memberInfos[index] = members;
            this.memberLookupTable[index] = null;
            WrapMembers(members);
            return members;
        }

        internal static TypeReflector GetTypeReflectorFor(Type type)
        {
            TypeReflector reflector = Table[type];
            if (reflector == null)
            {
                reflector = new TypeReflector(type);
                lock (Table)
                {
                    TypeReflector reflector2 = Table[type];
                    if (reflector2 != null)
                    {
                        return reflector2;
                    }
                    Table[type] = reflector;
                }
            }
            return reflector;
        }

        internal bool ImplementsIReflect()
        {
            object implementsIReflect = this.implementsIReflect;
            if (implementsIReflect != null)
            {
                return (bool) implementsIReflect;
            }
            bool flag = typeof(IReflect).IsAssignableFrom(this.type);
            this.implementsIReflect = flag;
            return flag;
        }

        internal bool Is__ComObject()
        {
            object obj2 = this.is__ComObject;
            if (obj2 != null)
            {
                return (bool) obj2;
            }
            bool flag = this.type.ToString() == "System.__ComObject";
            this.is__ComObject = flag;
            return flag;
        }

        private static void WrapMembers(MemberInfo[] members)
        {
            int index = 0;
            int length = members.Length;
            while (index < length)
            {
                MemberInfo info = members[index];
                MemberTypes memberType = info.MemberType;
                if (memberType != MemberTypes.Field)
                {
                    if (memberType == MemberTypes.Method)
                    {
                        goto Label_0032;
                    }
                    if (memberType == MemberTypes.Property)
                    {
                        goto Label_0042;
                    }
                }
                else
                {
                    members[index] = new JSFieldInfo((FieldInfo) info);
                }
                goto Label_0050;
            Label_0032:
                members[index] = new JSMethodInfo((MethodInfo) info);
                goto Label_0050;
            Label_0042:
                members[index] = new JSPropertyInfo((PropertyInfo) info);
            Label_0050:
                index++;
            }
        }
    }
}

