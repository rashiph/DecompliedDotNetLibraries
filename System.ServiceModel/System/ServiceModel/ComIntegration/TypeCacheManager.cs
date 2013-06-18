namespace System.ServiceModel.ComIntegration
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal class TypeCacheManager : ITypeCacheManager
    {
        private Dictionary<Guid, Assembly> assemblyTable = new Dictionary<Guid, Assembly>();
        private object assemblyTableLock = new object();
        private static Guid clrAssemblyCustomID = new Guid("90883F05-3D28-11D2-8F17-00A0C9A6186D");
        internal static ITypeCacheManager instance;
        private static object instanceLock = new object();
        private Dictionary<Guid, Type> typeTable = new Dictionary<Guid, Type>();
        private object typeTableLock = new object();

        internal TypeCacheManager()
        {
        }

        public void FindOrCreateType(Guid iid, out Type interfaceType, bool noAssemblyGeneration, bool isServer)
        {
            lock (this.typeTableLock)
            {
                this.typeTable.TryGetValue(iid, out interfaceType);
                if (interfaceType == null)
                {
                    Type type = null;
                    foreach (Type type2 in this.ResolveAssemblyFromIID(iid, noAssemblyGeneration, isServer).GetTypes())
                    {
                        if (type2.GUID == iid)
                        {
                            if (type2.IsInterface && this.NoCoClassAttributeOnType(type2))
                            {
                                interfaceType = type2;
                                break;
                            }
                            if (type2.IsInterface && !this.NoCoClassAttributeOnType(type2))
                            {
                                type = type2;
                            }
                        }
                    }
                    if ((interfaceType == null) && (type != null))
                    {
                        interfaceType = type;
                    }
                    else if (interfaceType == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InterfaceNotFoundInAssembly")));
                    }
                    this.typeTable[iid] = interfaceType;
                }
            }
        }

        private ITypeLib2 GettypeLibrary(Guid typeLibraryID, string version)
        {
            object obj2;
            ushort major = 0;
            ushort minor = 0;
            this.ParseVersion(version, out major, out minor);
            int errorCode = System.ServiceModel.ComIntegration.SafeNativeMethods.LoadRegTypeLib(ref typeLibraryID, major, minor, 0, out obj2);
            if ((errorCode != 0) || (obj2 == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("FailedToLoadTypeLibrary"), errorCode));
            }
            return (obj2 as ITypeLib2);
        }

        private Guid GettypeLibraryIDFromIID(Guid iid, bool isServer, out string version)
        {
            RegistryKey key = null;
            Guid guid2;
            try
            {
                Guid guid;
                string name = null;
                if (isServer)
                {
                    name = @"software\classes\interface\{" + iid.ToString() + @"}\typelib";
                    key = Registry.LocalMachine.OpenSubKey(name, false);
                }
                else
                {
                    name = @"interface\{" + iid.ToString() + @"}\typelib";
                    key = Registry.ClassesRoot.OpenSubKey(name, false);
                }
                if (key == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InterfaceNotRegistered")));
                }
                string str2 = key.GetValue("").ToString();
                if (string.IsNullOrEmpty(str2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NoTypeLibraryFoundForInterface")));
                }
                version = key.GetValue("Version").ToString();
                if (string.IsNullOrEmpty(version))
                {
                    version = "1.0";
                }
                if (!DiagnosticUtility.Utility.TryCreateGuid(str2, out guid))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BadInterfaceRegistration")));
                }
                guid2 = guid;
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
            }
            return guid2;
        }

        private bool NoCoClassAttributeOnType(ICustomAttributeProvider attrProvider)
        {
            return (ServiceReflector.GetCustomAttributes(attrProvider, typeof(CoClassAttribute), false).Length == 0);
        }

        private void ParseVersion(string version, out ushort major, out ushort minor)
        {
            major = 0;
            minor = 0;
            if (!string.IsNullOrEmpty(version))
            {
                int index = version.IndexOf(".", StringComparison.Ordinal);
                try
                {
                    if (index == -1)
                    {
                        major = Convert.ToUInt16(version, NumberFormatInfo.InvariantInfo);
                        minor = 0;
                    }
                    else
                    {
                        major = Convert.ToUInt16(version.Substring(0, index), NumberFormatInfo.InvariantInfo);
                        string str = version.Substring(index + 1);
                        int length = version.IndexOf(".", StringComparison.Ordinal);
                        if (length != -1)
                        {
                            str = str.Substring(0, length);
                        }
                        minor = Convert.ToUInt16(str, NumberFormatInfo.InvariantInfo);
                    }
                }
                catch (FormatException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BadInterfaceVersion")));
                }
                catch (OverflowException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BadInterfaceVersion")));
                }
            }
        }

        private Assembly ResolveAssemblyFromIID(Guid iid, bool noAssemblyGeneration, bool isServer)
        {
            string str;
            Guid typeLibraryID = this.GettypeLibraryIDFromIID(iid, isServer, out str);
            return this.ResolveAssemblyFromTypeLibID(iid, typeLibraryID, str, noAssemblyGeneration);
        }

        private Assembly ResolveAssemblyFromTypeLibID(Guid iid, Guid typeLibraryID, string version, bool noAssemblyGeneration)
        {
            Assembly assembly;
            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, 0x5000d, "TraceCodeComIntegrationTLBImportStarting", iid, typeLibraryID);
            bool flag = false;
            ITypeLib2 typeLibrary = null;
            try
            {
                lock (this.assemblyTableLock)
                {
                    this.assemblyTable.TryGetValue(typeLibraryID, out assembly);
                    if (assembly == null)
                    {
                        typeLibrary = this.GettypeLibrary(typeLibraryID, version);
                        object pVarVal = null;
                        typeLibrary.GetCustData(ref clrAssemblyCustomID, out pVarVal);
                        if (pVarVal == null)
                        {
                            flag = true;
                        }
                        string str = pVarVal as string;
                        if (string.IsNullOrEmpty(str))
                        {
                            flag = true;
                        }
                        if (noAssemblyGeneration && flag)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NativeTypeLibraryNotAllowed", new object[] { typeLibraryID })));
                        }
                        if (!flag)
                        {
                            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, 0x5000e, "TraceCodeComIntegrationTLBImportFromAssembly", iid, typeLibraryID, str);
                            assembly = Assembly.Load(str);
                        }
                        else
                        {
                            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, 0x5000f, "TraceCodeComIntegrationTLBImportFromTypelib", iid, typeLibraryID);
                            assembly = TypeLibraryHelper.GenerateAssemblyFromNativeTypeLibrary(iid, typeLibraryID, typeLibrary);
                        }
                        this.assemblyTable[typeLibraryID] = assembly;
                    }
                }
            }
            catch (Exception exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610728), new string[] { iid.ToString(), typeLibraryID.ToString(), exception.ToString() });
                throw;
            }
            finally
            {
                if (typeLibrary != null)
                {
                    Marshal.ReleaseComObject(typeLibrary);
                }
            }
            if (null == assembly)
            {
                throw Fx.AssertAndThrow("Assembly should not be null");
            }
            ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, 0x50011, "TraceCodeComIntegrationTLBImportFinished", iid, typeLibraryID);
            return assembly;
        }

        public static Type ResolveClsidToType(Guid clsid)
        {
            string name = @"software\classes\clsid\{" + clsid.ToString() + @"}\InprocServer32";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(name, false))
            {
                if (key != null)
                {
                    using (RegistryKey key2 = key.OpenSubKey(typeof(TypeCacheManager).Assembly.ImageRuntimeVersion))
                    {
                        string str2 = null;
                        if (key2 == null)
                        {
                            name = null;
                            foreach (string str3 in key.GetSubKeyNames())
                            {
                                name = str3;
                                if (!string.IsNullOrEmpty(name))
                                {
                                    using (RegistryKey key3 = key.OpenSubKey(name))
                                    {
                                        str2 = (string) key3.GetValue("Assembly");
                                        if (!string.IsNullOrEmpty(str2))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            str2 = (string) key2.GetValue("Assembly");
                        }
                        if (!string.IsNullOrEmpty(str2))
                        {
                            foreach (Type type in Assembly.Load(str2).GetTypes())
                            {
                                if (type.IsClass && (type.GUID == clsid))
                                {
                                    return type;
                                }
                            }
                        }
                        return null;
                    }
                }
            }
            using (RegistryHandle handle = RegistryHandle.GetBitnessHKCR(IntPtr.Size != 8))
            {
                if (handle != null)
                {
                    using (RegistryHandle handle2 = handle.OpenSubKey(@"CLSID\{" + clsid.ToString() + @"}\InprocServer32"))
                    {
                        using (RegistryHandle handle3 = handle2.OpenSubKey(typeof(TypeCacheManager).Assembly.ImageRuntimeVersion))
                        {
                            string stringValue = null;
                            if (handle3 == null)
                            {
                                name = null;
                                foreach (string str in handle2.GetSubKeyNames())
                                {
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        using (RegistryHandle handle4 = handle2.OpenSubKey(str))
                                        {
                                            stringValue = handle4.GetStringValue("Assembly");
                                            if (!string.IsNullOrEmpty(stringValue))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                stringValue = handle3.GetStringValue("Assembly");
                            }
                            if (!string.IsNullOrEmpty(stringValue))
                            {
                                foreach (Type type2 in Assembly.Load(stringValue).GetTypes())
                                {
                                    if (type2.IsClass && (type2.GUID == clsid))
                                    {
                                        return type2;
                                    }
                                }
                            }
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        void ITypeCacheManager.FindOrCreateType(Guid typeLibId, string typeLibVersion, Guid typeDefId, out Type userDefinedType, bool noAssemblyGeneration)
        {
            lock (this.typeTableLock)
            {
                this.typeTable.TryGetValue(typeDefId, out userDefinedType);
                if (userDefinedType == null)
                {
                    foreach (Type type in this.ResolveAssemblyFromTypeLibID(Guid.Empty, typeLibId, typeLibVersion, noAssemblyGeneration).GetTypes())
                    {
                        if ((type.GUID == typeDefId) && type.IsValueType)
                        {
                            userDefinedType = type;
                            break;
                        }
                    }
                    if (userDefinedType == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UdtNotFoundInAssembly", new object[] { typeDefId })));
                    }
                    this.typeTable[typeDefId] = userDefinedType;
                }
            }
        }

        void ITypeCacheManager.FindOrCreateType(Type serverType, Guid iid, out Type interfaceType, bool noAssemblyGeneration, bool isServer)
        {
            interfaceType = null;
            if (serverType == null)
            {
                this.FindOrCreateType(iid, out interfaceType, noAssemblyGeneration, isServer);
            }
            else
            {
                if (!serverType.IsClass)
                {
                    throw Fx.AssertAndThrow("This should be a class");
                }
                foreach (Type type in serverType.GetInterfaces())
                {
                    if (type.GUID == iid)
                    {
                        interfaceType = type;
                        break;
                    }
                }
                if (interfaceType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InterfaceNotFoundInAssembly")));
                }
            }
        }

        Assembly ITypeCacheManager.ResolveAssembly(Guid assembly)
        {
            Assembly assembly2 = null;
            lock (this.assemblyTableLock)
            {
                this.assemblyTable.TryGetValue(assembly, out assembly2);
            }
            return assembly2;
        }

        internal Type VerifyType(Guid iid)
        {
            Type type;
            this.FindOrCreateType(iid, out type, false, true);
            return type;
        }

        public static ITypeCacheManager Provider
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        ITypeCacheManager manager = new TypeCacheManager();
                        Thread.MemoryBarrier();
                        instance = manager;
                    }
                }
                return instance;
            }
        }

        private enum RegKind
        {
            Default,
            Register,
            None
        }
    }
}

