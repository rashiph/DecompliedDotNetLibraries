namespace System.Runtime.InteropServices
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), Guid("475E398F-8AFA-43a7-A3BE-F4EF8D6787C9")]
    public class RegistrationServices : IRegistrationServices
    {
        private static Guid s_ManagedCategoryGuid = new Guid("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
        private const string strClsIdRootName = "CLSID";
        private const string strComponentCategorySubKey = "Component Categories";
        private const string strDocStringPrefix = "";
        private const string strImplementedCategoriesSubKey = "Implemented Categories";
        private const string strManagedCategoryDescription = ".NET Category";
        private const string strManagedCategoryGuid = "{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}";
        private const string strManagedTypeThreadingModel = "Both";
        private const string strMsCorEEFileName = "mscoree.dll";
        private const string strRecordRootName = "Record";
        private const string strTlbRootName = "TypeLib";

        [SecurityCritical]
        private void CallUserDefinedRegistrationMethod(Type type, bool bRegister)
        {
            bool flag = false;
            Type attributeType = null;
            if (bRegister)
            {
                attributeType = typeof(ComRegisterFunctionAttribute);
            }
            else
            {
                attributeType = typeof(ComUnregisterFunctionAttribute);
            }
            for (Type type3 = type; !flag && (type3 != null); type3 = type3.BaseType)
            {
                MethodInfo[] methods = type3.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                int length = methods.Length;
                for (int i = 0; i < length; i++)
                {
                    MethodInfo info = methods[i];
                    if (info.GetCustomAttributes(attributeType, true).Length != 0)
                    {
                        if (!info.IsStatic)
                        {
                            if (bRegister)
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NonStaticComRegFunction", new object[] { info.Name, type3.Name }));
                            }
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NonStaticComUnRegFunction", new object[] { info.Name, type3.Name }));
                        }
                        ParameterInfo[] parameters = info.GetParameters();
                        if (((info.ReturnType != typeof(void)) || (parameters == null)) || ((parameters.Length != 1) || ((parameters[0].ParameterType != typeof(string)) && (parameters[0].ParameterType != typeof(Type)))))
                        {
                            if (bRegister)
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InvalidComRegFunctionSig", new object[] { info.Name, type3.Name }));
                            }
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InvalidComUnRegFunctionSig", new object[] { info.Name, type3.Name }));
                        }
                        if (flag)
                        {
                            if (bRegister)
                            {
                                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MultipleComRegFunctions", new object[] { type3.Name }));
                            }
                            throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MultipleComUnRegFunctions", new object[] { type3.Name }));
                        }
                        object[] objArray = new object[1];
                        if (parameters[0].ParameterType == typeof(string))
                        {
                            objArray[0] = @"HKEY_CLASSES_ROOT\CLSID\{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
                        }
                        else
                        {
                            objArray[0] = type;
                        }
                        info.Invoke(null, objArray);
                        flag = true;
                    }
                }
            }
        }

        [DllImport("ole32.dll", CharSet=CharSet.Auto, PreserveSig=false)]
        private static extern void CoRevokeClassObject(int cookie);
        private void EnsureManagedCategoryExists()
        {
            if (!ManagedCategoryExists())
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("Component Categories"))
                {
                    using (RegistryKey key2 = key.CreateSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}"))
                    {
                        key2.SetValue("0", ".NET Category");
                    }
                }
            }
        }

        private Type GetBaseComImportType(Type type)
        {
            while ((type != null) && !type.IsImport)
            {
                type = type.BaseType;
            }
            return type;
        }

        public virtual Guid GetManagedCategoryGuid()
        {
            return s_ManagedCategoryGuid;
        }

        [SecurityCritical]
        public virtual string GetProgIdForType(Type type)
        {
            return Marshal.GenerateProgIdForType(type);
        }

        [SecurityCritical]
        public virtual Type[] GetRegistrableTypesInAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (!(assembly is RuntimeAssembly))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");
            }
            Type[] exportedTypes = assembly.GetExportedTypes();
            int length = exportedTypes.Length;
            ArrayList list = new ArrayList();
            for (int i = 0; i < length; i++)
            {
                Type type = exportedTypes[i];
                if (this.TypeRequiresRegistration(type))
                {
                    list.Add(type);
                }
            }
            Type[] array = new Type[list.Count];
            list.CopyTo(array);
            return array;
        }

        private bool IsRegisteredAsValueType(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }
            return true;
        }

        private static bool ManagedCategoryExists()
        {
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("Component Categories", RegistryKeyPermissionCheck.ReadSubTree))
            {
                if (key == null)
                {
                    return false;
                }
                using (RegistryKey key2 = key.OpenSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}", RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (key2 == null)
                    {
                        return false;
                    }
                    object obj2 = key2.GetValue("0");
                    if ((obj2 == null) || (obj2.GetType() != typeof(string)))
                    {
                        return false;
                    }
                    string str = (string) obj2;
                    if (str != ".NET Category")
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        public virtual bool RegisterAssembly(Assembly assembly, AssemblyRegistrationFlags flags)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (assembly.ReflectionOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsmLoadedForReflectionOnly"));
            }
            RuntimeAssembly assembly2 = assembly as RuntimeAssembly;
            if (assembly2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            string fullName = assembly.FullName;
            if (fullName == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoAsmName"));
            }
            string strAsmCodeBase = null;
            if ((flags & AssemblyRegistrationFlags.SetCodeBase) != AssemblyRegistrationFlags.None)
            {
                strAsmCodeBase = assembly2.GetCodeBase(false);
                if (strAsmCodeBase == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NoAsmCodeBase"));
                }
            }
            Type[] registrableTypesInAssembly = this.GetRegistrableTypesInAssembly(assembly);
            int length = registrableTypesInAssembly.Length;
            string strAsmVersion = assembly2.GetVersion().ToString();
            string imageRuntimeVersion = assembly.ImageRuntimeVersion;
            for (int i = 0; i < length; i++)
            {
                if (this.IsRegisteredAsValueType(registrableTypesInAssembly[i]))
                {
                    this.RegisterValueType(registrableTypesInAssembly[i], fullName, strAsmVersion, strAsmCodeBase, imageRuntimeVersion);
                }
                else if (this.TypeRepresentsComType(registrableTypesInAssembly[i]))
                {
                    this.RegisterComImportedType(registrableTypesInAssembly[i], fullName, strAsmVersion, strAsmCodeBase, imageRuntimeVersion);
                }
                else
                {
                    this.RegisterManagedType(registrableTypesInAssembly[i], fullName, strAsmVersion, strAsmCodeBase, imageRuntimeVersion);
                }
                this.CallUserDefinedRegistrationMethod(registrableTypesInAssembly[i], true);
            }
            object[] customAttributes = assembly.GetCustomAttributes(typeof(PrimaryInteropAssemblyAttribute), false);
            int num3 = customAttributes.Length;
            for (int j = 0; j < num3; j++)
            {
                this.RegisterPrimaryInteropAssembly(assembly2, strAsmCodeBase, (PrimaryInteropAssemblyAttribute) customAttributes[j]);
            }
            if ((registrableTypesInAssembly.Length <= 0) && (num3 <= 0))
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        private void RegisterComImportedType(Type type, string strAsmName, string strAsmVersion, string strAsmCodeBase, string strRuntimeVersion)
        {
            string subkey = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("CLSID"))
            {
                using (RegistryKey key2 = key.CreateSubKey(subkey))
                {
                    using (RegistryKey key3 = key2.CreateSubKey("InprocServer32"))
                    {
                        key3.SetValue("Class", type.FullName);
                        key3.SetValue("Assembly", strAsmName);
                        key3.SetValue("RuntimeVersion", strRuntimeVersion);
                        if (strAsmCodeBase != null)
                        {
                            key3.SetValue("CodeBase", strAsmCodeBase);
                        }
                        using (RegistryKey key4 = key3.CreateSubKey(strAsmVersion))
                        {
                            key4.SetValue("Class", type.FullName);
                            key4.SetValue("Assembly", strAsmName);
                            key4.SetValue("RuntimeVersion", strRuntimeVersion);
                            if (strAsmCodeBase != null)
                            {
                                key4.SetValue("CodeBase", strAsmCodeBase);
                            }
                        }
                    }
                }
            }
        }

        [SecurityCritical]
        private void RegisterManagedType(Type type, string strAsmName, string strAsmVersion, string strAsmCodeBase, string strRuntimeVersion)
        {
            string str = type.FullName ?? "";
            string str2 = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            string progIdForType = this.GetProgIdForType(type);
            if (progIdForType != string.Empty)
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progIdForType))
                {
                    key.SetValue("", str);
                    using (RegistryKey key2 = key.CreateSubKey("CLSID"))
                    {
                        key2.SetValue("", str2);
                    }
                }
            }
            using (RegistryKey key3 = Registry.ClassesRoot.CreateSubKey("CLSID"))
            {
                using (RegistryKey key4 = key3.CreateSubKey(str2))
                {
                    key4.SetValue("", str);
                    using (RegistryKey key5 = key4.CreateSubKey("InprocServer32"))
                    {
                        key5.SetValue("", "mscoree.dll");
                        key5.SetValue("ThreadingModel", "Both");
                        key5.SetValue("Class", type.FullName);
                        key5.SetValue("Assembly", strAsmName);
                        key5.SetValue("RuntimeVersion", strRuntimeVersion);
                        if (strAsmCodeBase != null)
                        {
                            key5.SetValue("CodeBase", strAsmCodeBase);
                        }
                        using (RegistryKey key6 = key5.CreateSubKey(strAsmVersion))
                        {
                            key6.SetValue("Class", type.FullName);
                            key6.SetValue("Assembly", strAsmName);
                            key6.SetValue("RuntimeVersion", strRuntimeVersion);
                            if (strAsmCodeBase != null)
                            {
                                key6.SetValue("CodeBase", strAsmCodeBase);
                            }
                        }
                        if (progIdForType != string.Empty)
                        {
                            using (RegistryKey key7 = key4.CreateSubKey("ProgId"))
                            {
                                key7.SetValue("", progIdForType);
                            }
                        }
                    }
                    using (RegistryKey key8 = key4.CreateSubKey("Implemented Categories"))
                    {
                        using (key8.CreateSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}"))
                        {
                        }
                    }
                }
            }
            this.EnsureManagedCategoryExists();
        }

        [SecurityCritical]
        private void RegisterPrimaryInteropAssembly(RuntimeAssembly assembly, string strAsmCodeBase, PrimaryInteropAssemblyAttribute attr)
        {
            if (assembly.GetPublicKey().Length == 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_PIAMustBeStrongNamed"));
            }
            string subkey = "{" + Marshal.GetTypeLibGuidForAssembly(assembly).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            string str2 = attr.MajorVersion.ToString("x", CultureInfo.InvariantCulture) + "." + attr.MinorVersion.ToString("x", CultureInfo.InvariantCulture);
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("TypeLib"))
            {
                using (RegistryKey key2 = key.CreateSubKey(subkey))
                {
                    using (RegistryKey key3 = key2.CreateSubKey(str2))
                    {
                        key3.SetValue("PrimaryInteropAssemblyName", assembly.FullName);
                        if (strAsmCodeBase != null)
                        {
                            key3.SetValue("PrimaryInteropAssemblyCodeBase", strAsmCodeBase);
                        }
                    }
                }
            }
        }

        [SecurityCritical]
        public virtual void RegisterTypeForComClients(Type type, ref Guid g)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            if (!this.TypeRequiresRegistration(type))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustBeComCreatable"), "type");
            }
            RegisterTypeForComClientsNative(type, ref g);
        }

        [ComVisible(false), SecurityCritical]
        public virtual int RegisterTypeForComClients(Type type, RegistrationClassContext classContext, RegistrationConnectionType flags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!(type is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"), "type");
            }
            if (!this.TypeRequiresRegistration(type))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TypeMustBeComCreatable"), "type");
            }
            return RegisterTypeForComClientsExNative(type, classContext, flags);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int RegisterTypeForComClientsExNative(Type t, RegistrationClassContext clsContext, RegistrationConnectionType flags);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void RegisterTypeForComClientsNative(Type type, ref Guid g);
        [SecurityCritical]
        private void RegisterValueType(Type type, string strAsmName, string strAsmVersion, string strAsmCodeBase, string strRuntimeVersion)
        {
            string subkey = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey("Record"))
            {
                using (RegistryKey key2 = key.CreateSubKey(subkey))
                {
                    using (RegistryKey key3 = key2.CreateSubKey(strAsmVersion))
                    {
                        key3.SetValue("Class", type.FullName);
                        key3.SetValue("Assembly", strAsmName);
                        key3.SetValue("RuntimeVersion", strRuntimeVersion);
                        if (strAsmCodeBase != null)
                        {
                            key3.SetValue("CodeBase", strAsmCodeBase);
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public virtual bool TypeRepresentsComType(Type type)
        {
            if (!type.IsCOMObject)
            {
                return false;
            }
            if (type.IsImport)
            {
                return true;
            }
            Type baseComImportType = this.GetBaseComImportType(type);
            return (Marshal.GenerateGuidForType(type) == Marshal.GenerateGuidForType(baseComImportType));
        }

        [SecurityCritical]
        public virtual bool TypeRequiresRegistration(Type type)
        {
            return TypeRequiresRegistrationHelper(type);
        }

        [SecurityCritical]
        internal static bool TypeRequiresRegistrationHelper(Type type)
        {
            if (!type.IsClass && !type.IsValueType)
            {
                return false;
            }
            if (type.IsAbstract)
            {
                return false;
            }
            if (!type.IsValueType && (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) == null))
            {
                return false;
            }
            return Marshal.IsTypeVisibleFromCom(type);
        }

        [SecurityCritical]
        public virtual bool UnregisterAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (assembly.ReflectionOnly)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AsmLoadedForReflectionOnly"));
            }
            RuntimeAssembly assembly2 = assembly as RuntimeAssembly;
            if (assembly2 == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            bool flag = true;
            Type[] registrableTypesInAssembly = this.GetRegistrableTypesInAssembly(assembly);
            int length = registrableTypesInAssembly.Length;
            string strAsmVersion = assembly2.GetVersion().ToString();
            for (int i = 0; i < length; i++)
            {
                this.CallUserDefinedRegistrationMethod(registrableTypesInAssembly[i], false);
                if (this.IsRegisteredAsValueType(registrableTypesInAssembly[i]))
                {
                    if (!this.UnregisterValueType(registrableTypesInAssembly[i], strAsmVersion))
                    {
                        flag = false;
                    }
                }
                else if (this.TypeRepresentsComType(registrableTypesInAssembly[i]))
                {
                    if (!this.UnregisterComImportedType(registrableTypesInAssembly[i], strAsmVersion))
                    {
                        flag = false;
                    }
                }
                else if (!this.UnregisterManagedType(registrableTypesInAssembly[i], strAsmVersion))
                {
                    flag = false;
                }
            }
            object[] customAttributes = assembly.GetCustomAttributes(typeof(PrimaryInteropAssemblyAttribute), false);
            int num3 = customAttributes.Length;
            if (flag)
            {
                for (int j = 0; j < num3; j++)
                {
                    this.UnregisterPrimaryInteropAssembly(assembly, (PrimaryInteropAssemblyAttribute) customAttributes[j]);
                }
            }
            if ((registrableTypesInAssembly.Length <= 0) && (num3 <= 0))
            {
                return false;
            }
            return true;
        }

        [SecurityCritical]
        private bool UnregisterComImportedType(Type type, string strAsmVersion)
        {
            bool flag = true;
            string name = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID", true))
            {
                if (key == null)
                {
                    return flag;
                }
                using (RegistryKey key2 = key.OpenSubKey(name, true))
                {
                    if (key2 != null)
                    {
                        using (RegistryKey key3 = key2.OpenSubKey("InprocServer32", true))
                        {
                            if (key3 != null)
                            {
                                key3.DeleteValue("Assembly", false);
                                key3.DeleteValue("Class", false);
                                key3.DeleteValue("RuntimeVersion", false);
                                key3.DeleteValue("CodeBase", false);
                                using (RegistryKey key4 = key3.OpenSubKey(strAsmVersion, true))
                                {
                                    if (key4 != null)
                                    {
                                        key4.DeleteValue("Assembly", false);
                                        key4.DeleteValue("Class", false);
                                        key4.DeleteValue("RuntimeVersion", false);
                                        key4.DeleteValue("CodeBase", false);
                                        if ((key4.SubKeyCount == 0) && (key4.ValueCount == 0))
                                        {
                                            key3.DeleteSubKey(strAsmVersion);
                                        }
                                    }
                                }
                                if (key3.SubKeyCount != 0)
                                {
                                    flag = false;
                                }
                                if ((key3.SubKeyCount == 0) && (key3.ValueCount == 0))
                                {
                                    key2.DeleteSubKey("InprocServer32");
                                }
                            }
                        }
                        if ((key2.SubKeyCount == 0) && (key2.ValueCount == 0))
                        {
                            key.DeleteSubKey(name);
                        }
                    }
                }
                if ((key.SubKeyCount == 0) && (key.ValueCount == 0))
                {
                    Registry.ClassesRoot.DeleteSubKey("CLSID");
                }
            }
            return flag;
        }

        [SecurityCritical]
        private bool UnregisterManagedType(Type type, string strAsmVersion)
        {
            bool flag = true;
            string name = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            string progIdForType = this.GetProgIdForType(type);
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID", true))
            {
                if (key != null)
                {
                    using (RegistryKey key2 = key.OpenSubKey(name, true))
                    {
                        if (key2 != null)
                        {
                            using (RegistryKey key3 = key2.OpenSubKey("InprocServer32", true))
                            {
                                if (key3 != null)
                                {
                                    using (RegistryKey key4 = key3.OpenSubKey(strAsmVersion, true))
                                    {
                                        if (key4 != null)
                                        {
                                            key4.DeleteValue("Assembly", false);
                                            key4.DeleteValue("Class", false);
                                            key4.DeleteValue("RuntimeVersion", false);
                                            key4.DeleteValue("CodeBase", false);
                                            if ((key4.SubKeyCount == 0) && (key4.ValueCount == 0))
                                            {
                                                key3.DeleteSubKey(strAsmVersion);
                                            }
                                        }
                                    }
                                    if (key3.SubKeyCount != 0)
                                    {
                                        flag = false;
                                    }
                                    if (flag)
                                    {
                                        key3.DeleteValue("", false);
                                        key3.DeleteValue("ThreadingModel", false);
                                    }
                                    key3.DeleteValue("Assembly", false);
                                    key3.DeleteValue("Class", false);
                                    key3.DeleteValue("RuntimeVersion", false);
                                    key3.DeleteValue("CodeBase", false);
                                    if ((key3.SubKeyCount == 0) && (key3.ValueCount == 0))
                                    {
                                        key2.DeleteSubKey("InprocServer32");
                                    }
                                }
                            }
                            if (flag)
                            {
                                key2.DeleteValue("", false);
                                if (progIdForType != string.Empty)
                                {
                                    using (RegistryKey key5 = key2.OpenSubKey("ProgId", true))
                                    {
                                        if (key5 != null)
                                        {
                                            key5.DeleteValue("", false);
                                            if ((key5.SubKeyCount == 0) && (key5.ValueCount == 0))
                                            {
                                                key2.DeleteSubKey("ProgId");
                                            }
                                        }
                                    }
                                }
                                using (RegistryKey key6 = key2.OpenSubKey("Implemented Categories", true))
                                {
                                    if (key6 != null)
                                    {
                                        using (RegistryKey key7 = key6.OpenSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}", true))
                                        {
                                            if (((key7 != null) && (key7.SubKeyCount == 0)) && (key7.ValueCount == 0))
                                            {
                                                key6.DeleteSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
                                            }
                                        }
                                        if ((key6.SubKeyCount == 0) && (key6.ValueCount == 0))
                                        {
                                            key2.DeleteSubKey("Implemented Categories");
                                        }
                                    }
                                }
                            }
                            if ((key2.SubKeyCount == 0) && (key2.ValueCount == 0))
                            {
                                key.DeleteSubKey(name);
                            }
                        }
                    }
                    if ((key.SubKeyCount == 0) && (key.ValueCount == 0))
                    {
                        Registry.ClassesRoot.DeleteSubKey("CLSID");
                    }
                }
                if (!flag || !(progIdForType != string.Empty))
                {
                    return flag;
                }
                using (RegistryKey key8 = Registry.ClassesRoot.OpenSubKey(progIdForType, true))
                {
                    if (key8 != null)
                    {
                        key8.DeleteValue("", false);
                        using (RegistryKey key9 = key8.OpenSubKey("CLSID", true))
                        {
                            if (key9 != null)
                            {
                                key9.DeleteValue("", false);
                                if ((key9.SubKeyCount == 0) && (key9.ValueCount == 0))
                                {
                                    key8.DeleteSubKey("CLSID");
                                }
                            }
                        }
                        if ((key8.SubKeyCount == 0) && (key8.ValueCount == 0))
                        {
                            Registry.ClassesRoot.DeleteSubKey(progIdForType);
                        }
                    }
                    return flag;
                }
            }
        }

        [SecurityCritical]
        private void UnregisterPrimaryInteropAssembly(Assembly assembly, PrimaryInteropAssemblyAttribute attr)
        {
            string name = "{" + Marshal.GetTypeLibGuidForAssembly(assembly).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            string str2 = attr.MajorVersion.ToString("x", CultureInfo.InvariantCulture) + "." + attr.MinorVersion.ToString("x", CultureInfo.InvariantCulture);
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("TypeLib", true))
            {
                if (key != null)
                {
                    using (RegistryKey key2 = key.OpenSubKey(name, true))
                    {
                        if (key2 != null)
                        {
                            using (RegistryKey key3 = key2.OpenSubKey(str2, true))
                            {
                                if (key3 != null)
                                {
                                    key3.DeleteValue("PrimaryInteropAssemblyName", false);
                                    key3.DeleteValue("PrimaryInteropAssemblyCodeBase", false);
                                    if ((key3.SubKeyCount == 0) && (key3.ValueCount == 0))
                                    {
                                        key2.DeleteSubKey(str2);
                                    }
                                }
                            }
                            if ((key2.SubKeyCount == 0) && (key2.ValueCount == 0))
                            {
                                key.DeleteSubKey(name);
                            }
                        }
                    }
                    if ((key.SubKeyCount == 0) && (key.ValueCount == 0))
                    {
                        Registry.ClassesRoot.DeleteSubKey("TypeLib");
                    }
                }
            }
        }

        [SecurityCritical, ComVisible(false)]
        public virtual void UnregisterTypeForComClients(int cookie)
        {
            CoRevokeClassObject(cookie);
        }

        [SecurityCritical]
        private bool UnregisterValueType(Type type, string strAsmVersion)
        {
            bool flag = true;
            string name = "{" + Marshal.GenerateGuidForType(type).ToString().ToUpper(CultureInfo.InvariantCulture) + "}";
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey("Record", true))
            {
                if (key == null)
                {
                    return flag;
                }
                using (RegistryKey key2 = key.OpenSubKey(name, true))
                {
                    if (key2 != null)
                    {
                        using (RegistryKey key3 = key2.OpenSubKey(strAsmVersion, true))
                        {
                            if (key3 != null)
                            {
                                key3.DeleteValue("Assembly", false);
                                key3.DeleteValue("Class", false);
                                key3.DeleteValue("CodeBase", false);
                                key3.DeleteValue("RuntimeVersion", false);
                                if ((key3.SubKeyCount == 0) && (key3.ValueCount == 0))
                                {
                                    key2.DeleteSubKey(strAsmVersion);
                                }
                            }
                        }
                        if (key2.SubKeyCount != 0)
                        {
                            flag = false;
                        }
                        if ((key2.SubKeyCount == 0) && (key2.ValueCount == 0))
                        {
                            key.DeleteSubKey(name);
                        }
                    }
                }
                if ((key.SubKeyCount == 0) && (key.ValueCount == 0))
                {
                    Registry.ClassesRoot.DeleteSubKey("Record");
                }
            }
            return flag;
        }
    }
}

