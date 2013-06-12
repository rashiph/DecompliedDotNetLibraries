namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;

    internal class DynamicAssemblies
    {
        private static ArrayList assembliesInConfig = new ArrayList();
        private static Hashtable assemblyToNameMap;
        private static FileIOPermission fileIOPermission;
        private static Hashtable nameToAssemblyMap;
        private static object s_InternalSyncObject;
        private static Hashtable tableIsTypeDynamic = new Hashtable();

        private DynamicAssemblies()
        {
        }

        internal static void Add(Assembly a)
        {
            if (nameToAssemblyMap == null)
            {
                lock (InternalSyncObject)
                {
                    if (nameToAssemblyMap == null)
                    {
                        nameToAssemblyMap = new Hashtable();
                        assemblyToNameMap = new Hashtable();
                    }
                }
            }
            lock (nameToAssemblyMap)
            {
                if (assemblyToNameMap[a] == null)
                {
                    Assembly assembly = nameToAssemblyMap[a.FullName] as Assembly;
                    string key = null;
                    if (assembly == null)
                    {
                        key = a.FullName;
                    }
                    else if (assembly != a)
                    {
                        key = a.FullName + ", " + nameToAssemblyMap.Count;
                    }
                    if (key != null)
                    {
                        nameToAssemblyMap.Add(key, a);
                        assemblyToNameMap.Add(a, key);
                    }
                }
            }
        }

        internal static Assembly Get(string fullName)
        {
            if (nameToAssemblyMap == null)
            {
                return null;
            }
            return (Assembly) nameToAssemblyMap[fullName];
        }

        internal static string GetName(Assembly a)
        {
            if (assemblyToNameMap == null)
            {
                return null;
            }
            return (string) assemblyToNameMap[a];
        }

        internal static bool IsTypeDynamic(Type type)
        {
            object obj2 = tableIsTypeDynamic[type];
            if (obj2 == null)
            {
                UnrestrictedFileIOPermission.Assert();
                Assembly assembly = type.Assembly;
                bool flag = assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location);
                if (!flag)
                {
                    if (type.IsArray)
                    {
                        flag = IsTypeDynamic(type.GetElementType());
                    }
                    else if (type.IsGenericType)
                    {
                        Type[] genericArguments = type.GetGenericArguments();
                        if (genericArguments != null)
                        {
                            for (int i = 0; i < genericArguments.Length; i++)
                            {
                                Type type2 = genericArguments[i];
                                if ((type2 != null) && !type2.IsGenericParameter)
                                {
                                    flag = IsTypeDynamic(type2);
                                    if (flag)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                tableIsTypeDynamic[type] = obj2 = flag;
            }
            return (bool) obj2;
        }

        internal static bool IsTypeDynamic(Type[] arguments)
        {
            foreach (Type type in arguments)
            {
                if (IsTypeDynamic(type))
                {
                    return true;
                }
            }
            return false;
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        private static FileIOPermission UnrestrictedFileIOPermission
        {
            get
            {
                if (fileIOPermission == null)
                {
                    fileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
                }
                return fileIOPermission;
            }
        }
    }
}

