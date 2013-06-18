namespace System.EnterpriseServices
{
    using System;
    using System.Reflection;

    internal static class ReflectionCache
    {
        private static Cachetable Cache = new Cachetable();

        public static MemberInfo ConvertToClassMI(Type t, MemberInfo mi)
        {
            Type reflectedType = mi.ReflectedType;
            if (!reflectedType.IsInterface)
            {
                return mi;
            }
            Cachetable cachetable = (Cachetable) Cache.Get(t);
            if (cachetable != null)
            {
                MemberInfo info = (MemberInfo) cachetable.Get(mi);
                if (info != null)
                {
                    return info;
                }
            }
            MethodInfo info2 = (MethodInfo) mi;
            MethodInfo nv = null;
            InterfaceMapping interfaceMap = t.GetInterfaceMap(reflectedType);
            if (interfaceMap.TargetMethods == null)
            {
                throw new InvalidCastException();
            }
            for (int i = 0; i < interfaceMap.TargetMethods.Length; i++)
            {
                if (interfaceMap.InterfaceMethods[i] == info2)
                {
                    nv = interfaceMap.TargetMethods[i];
                    break;
                }
            }
            if (cachetable == null)
            {
                cachetable = (Cachetable) Cache.Set(t, new Cachetable());
            }
            cachetable.Reset(mi, nv);
            return nv;
        }

        public static MemberInfo ConvertToInterfaceMI(MemberInfo mi)
        {
            MemberInfo info = (MemberInfo) Cache.Get(mi);
            if (info != null)
            {
                return info;
            }
            MethodInfo info2 = mi as MethodInfo;
            if (info2 == null)
            {
                return null;
            }
            MethodInfo nv = null;
            Type reflectedType = info2.ReflectedType;
            if (reflectedType.IsInterface)
            {
                nv = info2;
            }
            else
            {
                Type[] interfaces = reflectedType.GetInterfaces();
                if (interfaces == null)
                {
                    return null;
                }
                for (int i = 0; i < interfaces.Length; i++)
                {
                    InterfaceMapping interfaceMap = reflectedType.GetInterfaceMap(interfaces[i]);
                    if (interfaceMap.TargetMethods != null)
                    {
                        for (int j = 0; j < interfaceMap.TargetMethods.Length; j++)
                        {
                            if (interfaceMap.TargetMethods[j] == info2)
                            {
                                nv = interfaceMap.InterfaceMethods[j];
                                break;
                            }
                        }
                        if (nv != null)
                        {
                            break;
                        }
                    }
                }
            }
            Cache.Reset(mi, nv);
            return nv;
        }
    }
}

