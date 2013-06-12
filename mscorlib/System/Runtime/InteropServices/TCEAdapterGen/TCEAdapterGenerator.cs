namespace System.Runtime.InteropServices.TCEAdapterGen
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;

    internal class TCEAdapterGenerator
    {
        private ModuleBuilder m_Module;
        private Hashtable m_SrcItfToSrcItfInfoMap = new Hashtable();
        private static CustomAttributeBuilder s_HiddenCABuilder;
        private static CustomAttributeBuilder s_NoClassItfCABuilder;

        internal static TypeBuilder DefineUniqueType(string strInitFullName, TypeAttributes attrs, Type BaseType, Type[] aInterfaceTypes, ModuleBuilder mb)
        {
            string className = strInitFullName;
            for (int i = 2; mb.GetType(className) != null; i++)
            {
                className = strInitFullName + "_" + i;
            }
            return mb.DefineType(className, attrs, BaseType, aInterfaceTypes);
        }

        internal static MethodInfo[] GetNonPropertyMethods(Type type)
        {
            ArrayList list = new ArrayList(type.GetMethods());
            foreach (PropertyInfo info in type.GetProperties())
            {
                foreach (MethodInfo info2 in info.GetAccessors())
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (((MethodInfo) list[i]) == info2)
                        {
                            list.RemoveAt(i);
                        }
                    }
                }
            }
            MethodInfo[] array = new MethodInfo[list.Count];
            list.CopyTo(array);
            return array;
        }

        internal static MethodInfo[] GetPropertyMethods(Type type)
        {
            type.GetMethods();
            ArrayList list = new ArrayList();
            foreach (PropertyInfo info in type.GetProperties())
            {
                foreach (MethodInfo info2 in info.GetAccessors())
                {
                    list.Add(info2);
                }
            }
            MethodInfo[] array = new MethodInfo[list.Count];
            list.CopyTo(array);
            return array;
        }

        public void Process(ModuleBuilder ModBldr, ArrayList EventItfList)
        {
            this.m_Module = ModBldr;
            int count = EventItfList.Count;
            for (int i = 0; i < count; i++)
            {
                EventItfInfo info = (EventItfInfo) EventItfList[i];
                Type eventItfType = info.GetEventItfType();
                Type srcItfType = info.GetSrcItfType();
                string eventProviderName = info.GetEventProviderName();
                Type sinkHelperType = new EventSinkHelperWriter(this.m_Module, srcItfType, eventItfType).Perform();
                new EventProviderWriter(this.m_Module, eventProviderName, eventItfType, srcItfType, sinkHelperType).Perform();
            }
        }

        internal static void SetClassInterfaceTypeToNone(TypeBuilder tb)
        {
            if (s_NoClassItfCABuilder == null)
            {
                Type[] types = new Type[] { typeof(ClassInterfaceType) };
                ConstructorInfo constructor = typeof(ClassInterfaceAttribute).GetConstructor(types);
                object[] constructorArgs = new object[] { ClassInterfaceType.None };
                s_NoClassItfCABuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            }
            tb.SetCustomAttribute(s_NoClassItfCABuilder);
        }

        internal static void SetHiddenAttribute(TypeBuilder tb)
        {
            if (s_HiddenCABuilder == null)
            {
                Type[] types = new Type[] { typeof(TypeLibTypeFlags) };
                ConstructorInfo constructor = typeof(TypeLibTypeAttribute).GetConstructor(types);
                object[] constructorArgs = new object[] { TypeLibTypeFlags.FHidden };
                s_HiddenCABuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            }
            tb.SetCustomAttribute(s_HiddenCABuilder);
        }
    }
}

