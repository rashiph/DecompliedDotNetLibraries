namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;

    internal static class SafeSecurityHelper
    {
        private static Dictionary<object, AssemblyName> _assemblies;
        private static bool _isGCCallbackPending;
        internal const string IMAGE = "image";
        private static object syncObject = new object();

        private static void CleanupCollectedAssemblies(object state)
        {
            bool flag = false;
            List<object> list = null;
            lock (syncObject)
            {
                foreach (object obj2 in _assemblies.Keys)
                {
                    WeakReference reference = obj2 as WeakReference;
                    if (reference != null)
                    {
                        if (reference.IsAlive)
                        {
                            flag = true;
                        }
                        else
                        {
                            if (list == null)
                            {
                                list = new List<object>();
                            }
                            list.Add(obj2);
                        }
                    }
                }
                if (list != null)
                {
                    foreach (object obj3 in list)
                    {
                        _assemblies.Remove(obj3);
                    }
                }
                if (flag)
                {
                    GCNotificationToken.RegisterCallback(new WaitCallback(SafeSecurityHelper.CleanupCollectedAssemblies), null);
                }
                else
                {
                    _isGCCallbackPending = false;
                }
            }
        }

        private static AssemblyName GetAssemblyName(Assembly assembly)
        {
            object key = assembly.IsDynamic ? ((object) new WeakRefKey(assembly)) : ((object) assembly);
            lock (syncObject)
            {
                AssemblyName name;
                if (_assemblies == null)
                {
                    _assemblies = new Dictionary<object, AssemblyName>();
                }
                else if (_assemblies.TryGetValue(key, out name))
                {
                    return name;
                }
                name = new AssemblyName(assembly.FullName);
                _assemblies.Add(key, name);
                if (assembly.IsDynamic && !_isGCCallbackPending)
                {
                    GCNotificationToken.RegisterCallback(new WaitCallback(SafeSecurityHelper.CleanupCollectedAssemblies), null);
                    _isGCCallbackPending = true;
                }
                return name;
            }
        }

        internal static Assembly GetLoadedAssembly(AssemblyName assemblyName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Version version = assemblyName.Version;
            CultureInfo cultureInfo = assemblyName.CultureInfo;
            byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
            for (int i = assemblies.Length - 1; i >= 0; i--)
            {
                AssemblyName name = GetAssemblyName(assemblies[i]);
                Version version2 = name.Version;
                CultureInfo info2 = name.CultureInfo;
                byte[] curKeyToken = name.GetPublicKeyToken();
                if ((((string.Compare(name.Name, assemblyName.Name, true, TypeConverterHelper.InvariantEnglishUS) == 0) && ((version == null) || version.Equals(version2))) && ((cultureInfo == null) || cultureInfo.Equals(info2))) && ((publicKeyToken == null) || IsSameKeyToken(publicKeyToken, curKeyToken)))
                {
                    return assemblies[i];
                }
            }
            return null;
        }

        internal static bool IsSameKeyToken(byte[] reqKeyToken, byte[] curKeyToken)
        {
            bool flag = false;
            if ((reqKeyToken == null) && (curKeyToken == null))
            {
                return true;
            }
            if (((reqKeyToken != null) && (curKeyToken != null)) && (reqKeyToken.Length == curKeyToken.Length))
            {
                flag = true;
                for (int i = 0; i < reqKeyToken.Length; i++)
                {
                    if (reqKeyToken[i] != curKeyToken[i])
                    {
                        return false;
                    }
                }
            }
            return flag;
        }
    }
}

