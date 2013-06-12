namespace System.Runtime.InteropServices
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Assemblies;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.InteropServices.TCEAdapterGen;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true), Guid("F1C3BF79-C3E4-11d3-88E7-00902754C43A"), ClassInterface(ClassInterfaceType.None)]
    public sealed class TypeLibConverter : ITypeLibConverter
    {
        private const int MAX_NAMESPACE_LENGTH = 0x400;
        private const string s_strTypeLibAssemblyDescPrefix = "Assembly generated from typelib ";
        private const string s_strTypeLibAssemblyTitlePrefix = "TypeLib ";

        [return: MarshalAs(UnmanagedType.Interface)]
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public object ConvertAssemblyToTypeLib(Assembly assembly, string strTypeLibName, TypeLibExporterFlags flags, ITypeLibExporterNotifySink notifySink)
        {
            RuntimeAssembly internalAssembly;
            AssemblyBuilder builder = assembly as AssemblyBuilder;
            if (builder != null)
            {
                internalAssembly = builder.InternalAssembly;
            }
            else
            {
                internalAssembly = assembly as RuntimeAssembly;
            }
            return nConvertAssemblyToTypeLib(internalAssembly, strTypeLibName, flags, notifySink);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public AssemblyBuilder ConvertTypeLibToAssembly([MarshalAs(UnmanagedType.Interface)] object typeLib, string asmFileName, int flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, bool unsafeInterfaces)
        {
            return this.ConvertTypeLibToAssembly(typeLib, asmFileName, unsafeInterfaces ? TypeLibImporterFlags.UnsafeInterfaces : TypeLibImporterFlags.None, notifySink, publicKey, keyPair, null, null);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public AssemblyBuilder ConvertTypeLibToAssembly([MarshalAs(UnmanagedType.Interface)] object typeLib, string asmFileName, TypeLibImporterFlags flags, ITypeLibImporterNotifySink notifySink, byte[] publicKey, StrongNameKeyPair keyPair, string asmNamespace, Version asmVersion)
        {
            if (typeLib == null)
            {
                throw new ArgumentNullException("typeLib");
            }
            if (asmFileName == null)
            {
                throw new ArgumentNullException("asmFileName");
            }
            if (notifySink == null)
            {
                throw new ArgumentNullException("notifySink");
            }
            if (string.Empty.Equals(asmFileName))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileName"), "asmFileName");
            }
            if (asmFileName.Length > 260)
            {
                throw new ArgumentException(Environment.GetResourceString("IO.PathTooLong"), asmFileName);
            }
            if ((((flags & TypeLibImporterFlags.PrimaryInteropAssembly) != TypeLibImporterFlags.None) && (publicKey == null)) && (keyPair == null))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_PIAMustBeStrongNamed"));
            }
            ArrayList eventItfInfoList = null;
            AssemblyNameFlags none = AssemblyNameFlags.None;
            AssemblyName asmName = GetAssemblyNameFromTypelib(typeLib, asmFileName, publicKey, keyPair, asmVersion, none);
            AssemblyBuilder asmBldr = CreateAssemblyForTypeLib(typeLib, asmFileName, asmName, (flags & TypeLibImporterFlags.PrimaryInteropAssembly) != TypeLibImporterFlags.None, (flags & TypeLibImporterFlags.ReflectionOnlyLoading) != TypeLibImporterFlags.None, (flags & TypeLibImporterFlags.NoDefineVersionResource) != TypeLibImporterFlags.None);
            string fileName = Path.GetFileName(asmFileName);
            ModuleBuilder mod = asmBldr.DefineDynamicModule(fileName, fileName);
            if (asmNamespace == null)
            {
                asmNamespace = asmName.Name;
            }
            TypeResolveHandler handler = new TypeResolveHandler(mod, notifySink);
            AppDomain domain = Thread.GetDomain();
            ResolveEventHandler handler2 = new ResolveEventHandler(handler.ResolveEvent);
            ResolveEventHandler handler3 = new ResolveEventHandler(handler.ResolveAsmEvent);
            ResolveEventHandler handler4 = new ResolveEventHandler(handler.ResolveROAsmEvent);
            domain.TypeResolve += handler2;
            domain.AssemblyResolve += handler3;
            domain.ReflectionOnlyAssemblyResolve += handler4;
            nConvertTypeLibToMetadata(typeLib, asmBldr.InternalAssembly, mod.InternalModule, asmNamespace, flags, handler, out eventItfInfoList);
            UpdateComTypesInAssembly(asmBldr, mod);
            if (eventItfInfoList.Count > 0)
            {
                new TCEAdapterGenerator().Process(mod, eventItfInfoList);
            }
            domain.TypeResolve -= handler2;
            domain.AssemblyResolve -= handler3;
            domain.ReflectionOnlyAssemblyResolve -= handler4;
            return asmBldr;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static AssemblyBuilder CreateAssemblyForTypeLib(object typeLib, string asmFileName, AssemblyName asmName, bool bPrimaryInteropAssembly, bool bReflectionOnly, bool bNoDefineVersionResource)
        {
            AssemblyBuilderAccess reflectionOnly;
            AppDomain domain = Thread.GetDomain();
            string directoryName = null;
            if (asmFileName != null)
            {
                directoryName = Path.GetDirectoryName(asmFileName);
                if (string.IsNullOrEmpty(directoryName))
                {
                    directoryName = null;
                }
            }
            if (bReflectionOnly)
            {
                reflectionOnly = AssemblyBuilderAccess.ReflectionOnly;
            }
            else
            {
                reflectionOnly = AssemblyBuilderAccess.RunAndSave;
            }
            List<CustomAttributeBuilder> assemblyAttributes = new List<CustomAttributeBuilder>();
            CustomAttributeBuilder item = new CustomAttributeBuilder(typeof(SecurityRulesAttribute).GetConstructor(new Type[] { typeof(SecurityRuleSet) }), new object[] { SecurityRuleSet.Level2 });
            assemblyAttributes.Add(item);
            AssemblyBuilder asmBldr = domain.DefineDynamicAssembly(asmName, reflectionOnly, directoryName, false, assemblyAttributes);
            SetGuidAttributeOnAssembly(asmBldr, typeLib);
            SetImportedFromTypeLibAttrOnAssembly(asmBldr, typeLib);
            if (bNoDefineVersionResource)
            {
                SetTypeLibVersionAttribute(asmBldr, typeLib);
            }
            else
            {
                SetVersionInformation(asmBldr, typeLib, asmName);
            }
            if (bPrimaryInteropAssembly)
            {
                SetPIAAttributeOnAssembly(asmBldr, typeLib);
            }
            return asmBldr;
        }

        [SecurityCritical]
        internal static AssemblyName GetAssemblyNameFromTypelib(object typeLib, string asmFileName, byte[] publicKey, StrongNameKeyPair keyPair, Version asmVersion, AssemblyNameFlags asmNameFlags)
        {
            string strName = null;
            string strDocString = null;
            int dwHelpContext = 0;
            string strHelpFile = null;
            ITypeLib typeLibrary = (ITypeLib) typeLib;
            typeLibrary.GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            if (asmFileName == null)
            {
                asmFileName = strName;
            }
            else
            {
                string fileName = Path.GetFileName(asmFileName);
                string extension = Path.GetExtension(asmFileName);
                if (!".dll".Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileExtension"));
                }
                asmFileName = fileName.Substring(0, fileName.Length - ".dll".Length);
            }
            if (asmVersion == null)
            {
                int num2;
                int num3;
                Marshal.GetTypeLibVersion(typeLibrary, out num2, out num3);
                asmVersion = new Version(num2, num3, 0, 0);
            }
            AssemblyName name = new AssemblyName();
            name.Init(asmFileName, publicKey, null, asmVersion, null, AssemblyHashAlgorithm.None, AssemblyVersionCompatibility.SameMachine, null, asmNameFlags, keyPair);
            return name;
        }

        [SecuritySafeCritical]
        public bool GetPrimaryInteropAssembly(Guid g, int major, int minor, int lcid, out string asmName, out string asmCodeBase)
        {
            string name = "{" + g.ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            string str2 = major.ToString("x", CultureInfo.InvariantCulture) + "." + minor.ToString("x", CultureInfo.InvariantCulture);
            asmName = null;
            asmCodeBase = null;
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("TypeLib", false))
            {
                if (key != null)
                {
                    using (RegistryKey key2 = key.OpenSubKey(name))
                    {
                        if (key2 != null)
                        {
                            using (RegistryKey key3 = key2.OpenSubKey(str2, false))
                            {
                                if (key3 != null)
                                {
                                    asmName = (string) key3.GetValue("PrimaryInteropAssemblyName");
                                    asmCodeBase = (string) key3.GetValue("PrimaryInteropAssemblyCodeBase");
                                }
                            }
                        }
                    }
                }
            }
            return (asmName != null);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void LoadInMemoryTypeByName(RuntimeModule module, string className);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern object nConvertAssemblyToTypeLib(RuntimeAssembly assembly, string strTypeLibName, TypeLibExporterFlags flags, ITypeLibExporterNotifySink notifySink);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void nConvertTypeLibToMetadata(object typeLib, RuntimeAssembly asmBldr, RuntimeModule modBldr, string nameSpace, TypeLibImporterFlags flags, ITypeLibImporterNotifySink notifySink, out ArrayList eventItfInfoList);
        [SecurityCritical]
        private static void SetGuidAttributeOnAssembly(AssemblyBuilder asmBldr, object typeLib)
        {
            Type[] types = new Type[] { typeof(string) };
            ConstructorInfo constructor = typeof(GuidAttribute).GetConstructor(types);
            object[] constructorArgs = new object[] { Marshal.GetTypeLibGuid((ITypeLib) typeLib).ToString() };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            asmBldr.SetCustomAttribute(customBuilder);
        }

        [SecurityCritical]
        private static void SetImportedFromTypeLibAttrOnAssembly(AssemblyBuilder asmBldr, object typeLib)
        {
            Type[] types = new Type[] { typeof(string) };
            ConstructorInfo constructor = typeof(ImportedFromTypeLibAttribute).GetConstructor(types);
            string typeLibName = Marshal.GetTypeLibName((ITypeLib) typeLib);
            object[] constructorArgs = new object[] { typeLibName };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            asmBldr.SetCustomAttribute(customBuilder);
        }

        [SecurityCritical]
        private static void SetPIAAttributeOnAssembly(AssemblyBuilder asmBldr, object typeLib)
        {
            IntPtr nULL = Win32Native.NULL;
            ITypeLib lib = (ITypeLib) typeLib;
            int wMajorVerNum = 0;
            int wMinorVerNum = 0;
            Type[] types = new Type[] { typeof(int), typeof(int) };
            ConstructorInfo constructor = typeof(PrimaryInteropAssemblyAttribute).GetConstructor(types);
            try
            {
                lib.GetLibAttr(out nULL);
                System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(nULL, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                wMajorVerNum = typelibattr.wMajorVerNum;
                wMinorVerNum = typelibattr.wMinorVerNum;
            }
            finally
            {
                if (nULL != Win32Native.NULL)
                {
                    lib.ReleaseTLibAttr(nULL);
                }
            }
            object[] constructorArgs = new object[] { wMajorVerNum, wMinorVerNum };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            asmBldr.SetCustomAttribute(customBuilder);
        }

        [SecurityCritical]
        private static void SetTypeLibVersionAttribute(AssemblyBuilder asmBldr, object typeLib)
        {
            int num;
            int num2;
            Type[] types = new Type[] { typeof(int), typeof(int) };
            ConstructorInfo constructor = typeof(TypeLibVersionAttribute).GetConstructor(types);
            Marshal.GetTypeLibVersion((ITypeLib) typeLib, out num, out num2);
            object[] constructorArgs = new object[] { num, num2 };
            CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(constructor, constructorArgs);
            asmBldr.SetCustomAttribute(customBuilder);
        }

        [SecurityCritical]
        private static void SetVersionInformation(AssemblyBuilder asmBldr, object typeLib, AssemblyName asmName)
        {
            string strName = null;
            string strDocString = null;
            int dwHelpContext = 0;
            string strHelpFile = null;
            ((ITypeLib) typeLib).GetDocumentation(-1, out strName, out strDocString, out dwHelpContext, out strHelpFile);
            string product = string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("TypeLibConverter_ImportedTypeLibProductName"), new object[] { strName });
            asmBldr.DefineVersionInfoResource(product, asmName.Version.ToString(), null, null, null);
            SetTypeLibVersionAttribute(asmBldr, typeLib);
        }

        private static void UpdateComTypesInAssembly(AssemblyBuilder asmBldr, ModuleBuilder modBldr)
        {
            AssemblyBuilderData assemblyData = asmBldr.m_assemblyData;
            Type[] types = modBldr.GetTypes();
            int length = types.Length;
            for (int i = 0; i < length; i++)
            {
                assemblyData.AddPublicComType(types[i]);
            }
        }

        private class TypeResolveHandler : ITypeLibImporterNotifySink
        {
            private List<RuntimeAssembly> m_AsmList = new List<RuntimeAssembly>();
            private ModuleBuilder m_Module;
            private ITypeLibImporterNotifySink m_UserSink;

            public TypeResolveHandler(ModuleBuilder mod, ITypeLibImporterNotifySink userSink)
            {
                this.m_Module = mod;
                this.m_UserSink = userSink;
            }

            public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
            {
                this.m_UserSink.ReportEvent(eventKind, eventCode, eventMsg);
            }

            public Assembly ResolveAsmEvent(object sender, ResolveEventArgs args)
            {
                foreach (RuntimeAssembly assembly in this.m_AsmList)
                {
                    if (string.Compare(assembly.FullName, args.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return assembly;
                    }
                }
                return null;
            }

            [SecurityCritical]
            public Assembly ResolveEvent(object sender, ResolveEventArgs args)
            {
                try
                {
                    TypeLibConverter.LoadInMemoryTypeByName(this.m_Module.GetNativeHandle(), args.Name);
                    return this.m_Module.Assembly;
                }
                catch (TypeLoadException exception)
                {
                    if (exception.ResourceId != -2146233054)
                    {
                        throw;
                    }
                }
                foreach (RuntimeAssembly assembly in this.m_AsmList)
                {
                    try
                    {
                        assembly.GetType(args.Name, true, false);
                        return assembly;
                    }
                    catch (TypeLoadException exception2)
                    {
                        if (exception2._HResult != -2146233054)
                        {
                            throw;
                        }
                    }
                }
                return null;
            }

            public Assembly ResolveRef(object typeLib)
            {
                Assembly assembly = this.m_UserSink.ResolveRef(typeLib);
                if (assembly == null)
                {
                    throw new ArgumentNullException();
                }
                RuntimeAssembly item = assembly as RuntimeAssembly;
                if (item == null)
                {
                    AssemblyBuilder builder = assembly as AssemblyBuilder;
                    if (builder != null)
                    {
                        item = builder.InternalAssembly;
                    }
                }
                if (item == null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
                }
                this.m_AsmList.Add(item);
                return item;
            }

            public Assembly ResolveROAsmEvent(object sender, ResolveEventArgs args)
            {
                foreach (RuntimeAssembly assembly in this.m_AsmList)
                {
                    if (string.Compare(assembly.FullName, args.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return assembly;
                    }
                }
                return Assembly.ReflectionOnlyLoad(AppDomain.CurrentDomain.ApplyPolicy(args.Name));
            }
        }
    }
}

