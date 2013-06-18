namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;

    internal static class DebuggingHelper
    {
        public static object BooleanToObject(bool i)
        {
            return i;
        }

        public static object ByteToObject(byte i)
        {
            return i;
        }

        public static object CallConstructor(string typename, object[] arguments, VsaEngine engine)
        {
            if (engine == null)
            {
                engine = VsaEngine.CreateEngine();
            }
            object type = GetType(typename);
            return LateBinding.CallValue(null, type, arguments, true, false, engine);
        }

        public static object CallMethod(string name, object thisob, object[] arguments, VsaEngine engine)
        {
            if (engine == null)
            {
                engine = VsaEngine.CreateEngine();
            }
            LateBinding binding = new LateBinding(name, thisob, true);
            return binding.Call(arguments, false, false, engine);
        }

        public static object CallStaticMethod(string name, string typename, object[] arguments, VsaEngine engine)
        {
            if (engine == null)
            {
                engine = VsaEngine.CreateEngine();
            }
            object type = GetType(typename);
            LateBinding binding = new LateBinding(name, type, true);
            return binding.Call(arguments, false, false, engine);
        }

        public static object[] CreateArray(int length)
        {
            object[] objArray = new object[length];
            for (int i = 0; i < length; i++)
            {
                objArray[i] = new object();
            }
            return objArray;
        }

        public static VsaEngine CreateEngine()
        {
            return VsaEngine.CreateEngineForDebugger();
        }

        public static string[] CreateStringArray(string s)
        {
            return new string[] { s };
        }

        public static object DoubleToObject(double i)
        {
            return i;
        }

        public static object GetClosureInstance(VsaEngine engine)
        {
            if (engine != null)
            {
                StackFrame frame = engine.ScriptObjectStackTop() as StackFrame;
                if (frame != null)
                {
                    return frame.closureInstance;
                }
            }
            return null;
        }

        public static object GetDefaultIndexedPropertyValue(object thisob, object[] arguments, VsaEngine engine, string[] namedParameters)
        {
            if (engine == null)
            {
                engine = VsaEngine.CreateEngine();
            }
            object[] target = null;
            int num = (arguments == null) ? 0 : arguments.Length;
            if (((namedParameters != null) && (namedParameters.Length > 0)) && ((namedParameters[0] == "this") && (num > 0)))
            {
                target = new object[num - 1];
                ArrayObject.Copy(arguments, 1, target, 0, num - 1);
            }
            else
            {
                target = arguments;
            }
            LateBinding binding = new LateBinding(null, thisob, true);
            return binding.Call(target, false, false, engine);
        }

        public static DynamicFieldInfo[] GetExpandoObjectFields(object o, bool hideNamespaces)
        {
            IReflect reflect = o as IReflect;
            if (reflect == null)
            {
                return new DynamicFieldInfo[0];
            }
            try
            {
                FieldInfo[] fields = reflect.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                ArrayList list = new ArrayList();
                foreach (FieldInfo info in fields)
                {
                    bool flag = false;
                    foreach (object obj2 in list)
                    {
                        if (info.Name == ((DynamicFieldInfo) obj2).name)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        object obj3 = info.GetValue(o);
                        if (!hideNamespaces || !(obj3 is Namespace))
                        {
                            list.Add(new DynamicFieldInfo(info.Name, obj3, info.FieldType.Name));
                        }
                    }
                }
                return (DynamicFieldInfo[]) list.ToArray(typeof(DynamicFieldInfo));
            }
            catch
            {
                return new DynamicFieldInfo[0];
            }
        }

        public static DynamicFieldInfo[] GetHashTableFields(SimpleHashtable h)
        {
            DynamicFieldInfo[] infoArray = null;
            try
            {
                int count = h.count;
                infoArray = new DynamicFieldInfo[count];
                IDictionaryEnumerator enumerator = h.GetEnumerator();
                for (int i = 0; (i < count) && enumerator.MoveNext(); i++)
                {
                    infoArray[i] = new DynamicFieldInfo((string) enumerator.Key, enumerator.Value);
                }
            }
            catch
            {
                infoArray = new DynamicFieldInfo[0];
            }
            return infoArray;
        }

        private static Type GetType(string typename)
        {
            string[] strArray = typename.Split(new char[] { '.' });
            if ((strArray == null) || (strArray.Length <= 0))
            {
                return null;
            }
            string str = strArray[0];
            Type typeInCurrentAppDomain = GetTypeInCurrentAppDomain(str);
            int index = 1;
            while ((index < strArray.Length) && (typeInCurrentAppDomain == null))
            {
                str = str + "." + strArray[index];
                typeInCurrentAppDomain = GetTypeInCurrentAppDomain(str);
                index++;
            }
            for (int i = index; (i < strArray.Length) && (typeInCurrentAppDomain != null); i++)
            {
                typeInCurrentAppDomain = typeInCurrentAppDomain.GetNestedType(strArray[i], BindingFlags.NonPublic | BindingFlags.Public);
            }
            return typeInCurrentAppDomain;
        }

        private static Type GetTypeInCurrentAppDomain(string typename)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!(assembly is AssemblyBuilder))
                {
                    Type type = assembly.GetType(typename);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        public static object Int16ToObject(short i)
        {
            return i;
        }

        public static object Int32ToObject(int i)
        {
            return i;
        }

        public static object Int64ToObject(long i)
        {
            return i;
        }

        public static object InvokeCOMObject(string name, object obj, object[] arguments, BindingFlags invokeAttr)
        {
            return obj.GetType().InvokeMember(name, invokeAttr, JSBinder.ob, obj, arguments, null, null, null);
        }

        public static object InvokeMethodInfo(MethodInfo m, object[] arguments, bool construct, object thisob, VsaEngine engine)
        {
            if (engine == null)
            {
                engine = VsaEngine.CreateEngine();
            }
            return LateBinding.CallOneOfTheMembers(new MemberInfo[] { m }, arguments, construct, thisob, JSBinder.ob, null, null, engine);
        }

        public static void Print(string message, VsaEngine engine)
        {
            if ((engine != null) && engine.doPrint)
            {
                ScriptStream.Out.Write(message);
            }
        }

        public static object SByteToObject(sbyte i)
        {
            return i;
        }

        public static void SetDefaultIndexedPropertyValue(object thisob, object[] arguments, VsaEngine engine, string[] namedParameters)
        {
            object obj2 = null;
            object[] target = null;
            int length = arguments.Length;
            if (length > 0)
            {
                obj2 = arguments[length - 1];
            }
            int i = 0;
            int n = length - 1;
            if (((namedParameters != null) && (namedParameters.Length > 0)) && (namedParameters[0] == "this"))
            {
                n--;
                i = 1;
            }
            target = new object[n];
            ArrayObject.Copy(arguments, i, target, 0, n);
            new LateBinding(null, thisob, true).SetIndexedPropertyValue(target, obj2);
        }

        public static void SetIndexedPropertyValue(string name, object thisob, object[] arguments, object value, VsaEngine engine)
        {
            new LateBinding(name, thisob, true).SetIndexedPropertyValue(arguments, value);
        }

        public static void SetStaticIndexedPropertyValue(string name, string typename, object[] arguments, object value, VsaEngine engine)
        {
            object type = GetType(typename);
            new LateBinding(name, type, true).SetIndexedPropertyValue(arguments, value);
        }

        public static object SingleToObject(float i)
        {
            return i;
        }

        public static object StringToObject(string s)
        {
            return s;
        }

        public static object ToNativeArray(string elementTypename, object arrayObject)
        {
            Type elementType = GetType(elementTypename);
            if (elementType == null)
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            ArrayObject obj2 = arrayObject as ArrayObject;
            if (obj2 == null)
            {
                throw new JScriptException(JSError.TypeMismatch);
            }
            return obj2.ToNativeArray(elementType);
        }

        public static object UInt16ToObject(ushort i)
        {
            return i;
        }

        public static object UInt32ToObject(uint i)
        {
            return i;
        }

        public static object UInt64ToObject(ulong i)
        {
            return i;
        }
    }
}

