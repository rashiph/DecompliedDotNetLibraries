namespace System.EnterpriseServices
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Permissions;

    internal class RegistrationDriver
    {
        private ICatalogCollection _appColl;
        private Hashtable _cache;
        private ICatalog _cat;
        private InstallationFlags _installFlags;

        internal bool AfterSaveChanges(ICustomAttributeProvider t, ICatalogObject obj, ICatalogCollection coll, string prefix, Hashtable cache)
        {
            bool flag = false;
            object[] customAttributes = t.GetCustomAttributes(true);
            cache[prefix] = obj;
            cache[prefix + "Type"] = t;
            cache[prefix + "Collection"] = coll;
            cache["CurrentTarget"] = prefix;
            foreach (object obj2 in customAttributes)
            {
                if (obj2 is IConfigurationAttribute)
                {
                    IConfigurationAttribute attribute = (IConfigurationAttribute) obj2;
                    if (attribute.IsValidTarget(prefix) && attribute.AfterSaveChanges(cache))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private void ApplicationDefaults(ICatalogObject obj, ICatalogCollection coll)
        {
            obj.SetValue("Activation", ActivationOption.Library);
            obj.SetValue("AccessChecksLevel", AccessChecksLevelOption.Application);
            obj.SetValue("ApplicationAccessChecksEnabled", true);
            obj.SetValue("Authentication", AuthenticationOption.Packet);
            obj.SetValue("CRMEnabled", false);
            obj.SetValue("EventsEnabled", true);
            obj.SetValue("ImpersonationLevel", ImpersonationLevelOption.Impersonate);
            obj.SetValue("QueuingEnabled", false);
            obj.SetValue("QueueListenerEnabled", false);
            obj.SetValue("SoapActivated", false);
            obj.SetValue("QCListenerMaxThreads", 0);
        }

        internal bool AssemblyHasStrongName(Assembly asm)
        {
            return (asm.GetName().GetPublicKeyToken().Length > 0);
        }

        private static RegistrationErrorInfo[] BuildErrorInfoChain(ICatalogCollection coll)
        {
            RegistrationErrorInfo[] infoArray2;
            try
            {
                Populate(coll);
                int num = coll.Count();
                RegistrationErrorInfo[] infoArray = null;
                if (num > 0)
                {
                    infoArray = new RegistrationErrorInfo[num];
                    for (int i = 0; i < num; i++)
                    {
                        string majorRef = null;
                        string minorRef = null;
                        int errorCode = 0;
                        ICatalogObject obj2 = (ICatalogObject) coll.Item(i);
                        string name = (string) obj2.GetValue("Name");
                        errorCode = (int) obj2.GetValue("ErrorCode");
                        majorRef = (string) obj2.GetValue("MajorRef");
                        minorRef = (string) obj2.GetValue("MinorRef");
                        infoArray[i] = new RegistrationErrorInfo(majorRef, minorRef, name, errorCode);
                    }
                }
                infoArray2 = infoArray;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_ErrCollectionErr"), exception);
            }
            return infoArray2;
        }

        public void CheckAssemblySCValidity(Assembly asm)
        {
            Type[] types = null;
            bool flag = true;
            ArrayList list = null;
            RegistrationServices services = new RegistrationServices();
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }
            foreach (Type type in types)
            {
                if (((null != type) && type.IsClass) && type.IsSubclassOf(typeof(ServicedComponent)))
                {
                    if (!services.TypeRequiresRegistration(type) && !type.IsAbstract)
                    {
                        flag = false;
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        RegistrationErrorInfo info = new RegistrationErrorInfo(null, null, type.ToString(), -2147467259);
                        list.Add(info);
                    }
                    ClassInterfaceType classInterfaceType = ServicedComponentInfo.GetClassInterfaceType(type);
                    foreach (MethodInfo info2 in type.GetMethods())
                    {
                        if (ReflectionCache.ConvertToInterfaceMI(info2) == null)
                        {
                            if (ServicedComponentInfo.HasSpecialMethodAttributes(info2))
                            {
                                this.ReportWarning(Resource.FormatString("Reg_NoClassInterfaceSecure", type.FullName, info2.Name));
                            }
                            if ((classInterfaceType == ClassInterfaceType.AutoDispatch) && ServicedComponentInfo.IsMethodAutoDone(info2))
                            {
                                this.ReportWarning(Resource.FormatString("Reg_NoClassInterface", type.FullName, info2.Name));
                            }
                        }
                    }
                }
            }
            if (!flag)
            {
                RegistrationErrorInfo[] errorInfo = (RegistrationErrorInfo[]) list.ToArray(typeof(RegistrationErrorInfo));
                throw new RegistrationException(Resource.FormatString("Reg_InvalidServicedComponents"), errorInfo);
            }
        }

        public void CheckForAppSecurityAttribute(Assembly asm)
        {
            if (asm.GetCustomAttributes(typeof(ApplicationAccessControlAttribute), true).Length <= 0)
            {
                this.ReportWarning(Resource.FormatString("Reg_NoApplicationSecurity"));
            }
        }

        internal void ClassicRegistration(Assembly asm)
        {
            RegistryPermission permission = new RegistryPermission(PermissionState.Unrestricted);
            permission.Demand();
            permission.Assert();
            try
            {
                new RegistrationServices().RegisterAssembly(asm, AssemblyRegistrationFlags.SetCodeBase);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_AssemblyRegErr", asm), exception);
            }
        }

        internal void ClassicUnregistration(Assembly asm)
        {
            try
            {
                new RegistrationServices().UnregisterAssembly(asm);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_AssemblyUnregErr", asm), exception);
            }
        }

        private void CleanupDriver()
        {
            this._cat = null;
            this._cache = null;
            this._appColl = null;
        }

        internal void ConfigureCollection(ICatalogCollection coll, IConfigCallback cb)
        {
            bool flag = false;
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            foreach (object obj2 in cb)
            {
                object a = cb.FindObject(coll, obj2);
                cb.ConfigureDefaults(a, obj2);
            }
            SaveChanges(coll);
            flag = false;
            foreach (object obj4 in cb)
            {
                object obj5 = cb.FindObject(coll, obj4);
                if (cb.Configure(obj5, obj4))
                {
                    flag = true;
                }
            }
            SaveChanges(coll);
            flag = false;
            foreach (object obj6 in cb)
            {
                object obj7 = cb.FindObject(coll, obj6);
                if (cb.AfterSaveChanges(obj7, obj6))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                SaveChanges(coll);
            }
            cb.ConfigureSubCollections(coll);
        }

        private void ConfigureComponents(ApplicationSpec spec)
        {
            ICatalogCollection coll = null;
            try
            {
                ICatalogObject obj2 = this.FindApplication(this._appColl, spec);
                if (obj2 == null)
                {
                    throw new RegistrationException(Resource.FormatString("Reg_AppNotFoundErr", spec));
                }
                this._cache["Application"] = obj2;
                this._cache["ApplicationType"] = spec.Assembly;
                this._cache["ApplicationCollection"] = this._appColl;
                coll = (ICatalogCollection) this._appColl.GetCollection(CollectionName.Components, obj2.Key());
                this.ConfigureCollection(coll, new ComponentConfigCallback(coll, spec, this._cache, this, this._installFlags));
            }
            catch (RegistrationException)
            {
                throw;
            }
            catch (COMException exception)
            {
                throw this.WrapCOMException(coll, exception, Resource.FormatString("Reg_ConfigErr"));
            }
            catch (Exception exception2)
            {
                if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_ConfigUnkErr"), exception2);
            }
        }

        internal bool ConfigureObject(ICustomAttributeProvider t, ICatalogObject obj, ICatalogCollection coll, string prefix, Hashtable cache)
        {
            bool flag = false;
            object[] customAttributes = t.GetCustomAttributes(true);
            cache[prefix] = obj;
            cache[prefix + "Type"] = t;
            cache[prefix + "Collection"] = coll;
            cache["CurrentTarget"] = prefix;
            foreach (object obj2 in customAttributes)
            {
                if (obj2 is IConfigurationAttribute)
                {
                    try
                    {
                        IConfigurationAttribute attribute = (IConfigurationAttribute) obj2;
                        if (attribute.IsValidTarget(prefix) && attribute.Apply(cache))
                        {
                            flag = true;
                        }
                    }
                    catch (Exception exception)
                    {
                        if ((exception is NullReferenceException) || (exception is SEHException))
                        {
                            throw;
                        }
                        throw new RegistrationException(Resource.FormatString("Reg_ComponentAttrErr", obj.Name(), obj2), exception);
                    }
                }
            }
            return flag;
        }

        private ICatalogObject CreateApplication(ApplicationSpec spec, bool checkExistence)
        {
            if (checkExistence && (this.FindApplication(this._appColl, spec) != null))
            {
                throw new RegistrationException(Resource.FormatString("Reg_AppExistsErr", spec));
            }
            ICatalogObject obj3 = (ICatalogObject) this._appColl.Add();
            this.CheckForAppSecurityAttribute(spec.Assembly);
            this.ApplicationDefaults(obj3, this._appColl);
            obj3.SetValue("Name", spec.Name);
            if (spec.ID != null)
            {
                obj3.SetValue("ID", spec.ID);
            }
            if (spec.AppRootDir != null)
            {
                obj3.SetValue("ApplicationDirectory", spec.AppRootDir);
            }
            SaveChanges(this._appColl);
            this.ConfigureObject(spec.Assembly, obj3, this._appColl, "Application", this._cache);
            spec.Name = (string) obj3.GetValue("Name");
            SaveChanges(this._appColl);
            return obj3;
        }

        private ICatalogObject FindApplication(ICatalogCollection apps, ApplicationSpec spec)
        {
            for (int i = 0; i < apps.Count(); i++)
            {
                ICatalogObject obj2 = (ICatalogObject) apps.Item(i);
                if (spec.Matches(obj2))
                {
                    return obj2;
                }
            }
            return null;
        }

        private int FindIndexOf(Guid[] arr, Guid key)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == key)
                {
                    return i;
                }
            }
            return -1;
        }

        private int FindIndexOf(string[] arr, string key)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == key)
                {
                    return i;
                }
            }
            return -1;
        }

        private ICatalogObject FindOrCreateApplication(ApplicationSpec spec, bool configure)
        {
            ICatalogObject obj2 = this.FindApplication(this._appColl, spec);
            if (obj2 == null)
            {
                return this.CreateApplication(spec, false);
            }
            if (configure)
            {
                this.CheckForAppSecurityAttribute(spec.Assembly);
                this.ApplicationDefaults(obj2, this._appColl);
                obj2.SetValue("Name", spec.Name);
                obj2.SetValue("ApplicationDirectory", (spec.AppRootDir == null) ? "" : spec.AppRootDir);
                this.ConfigureObject(spec.Assembly, obj2, this._appColl, "Application", this._cache);
                spec.Name = (string) obj2.GetValue("Name");
                SaveChanges(this._appColl);
            }
            return obj2;
        }

        internal static object GenerateTypeLibrary(Assembly asm, string tlb, Report report)
        {
            object obj3;
            try
            {
                TypeLibConverter converter = new TypeLibConverter();
                RegistrationExporterNotifySink notifySink = new RegistrationExporterNotifySink(tlb, report);
                object obj2 = converter.ConvertAssemblyToTypeLib(asm, tlb, TypeLibExporterFlags.OnlyReferenceRegistered, notifySink);
                ((ICreateTypeLib) obj2).SaveAllChanges();
                RegisterTypeLibrary(tlb);
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_TypeLibGenErr", tlb, asm), exception);
            }
            return obj3;
        }

        public void InstallAssembly(RegistrationConfig regConfig, object obSync)
        {
            Assembly asm = null;
            ApplicationSpec spec = null;
            CatalogSync sync = null;
            bool flag = false;
            bool flag2 = false;
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            try
            {
                permission.Demand();
                permission.Assert();
                ICatalogObject app = null;
                this.PrepArguments(regConfig);
                asm = this.NewLoadAssembly(regConfig.AssemblyFile);
                spec = new ApplicationSpec(asm, regConfig);
                if (spec.ConfigurableTypes == null)
                {
                    regConfig.Application = null;
                    regConfig.TypeLibrary = null;
                }
                else
                {
                    if (obSync != null)
                    {
                        if (!(obSync is CatalogSync))
                        {
                            throw new ArgumentException(Resource.FormatString("Err_obSync"));
                        }
                        sync = (CatalogSync) obSync;
                    }
                    this.PrepDriver(ref spec);
                    string message = string.Empty;
                    if (!this.ValidateBitness(spec, out message))
                    {
                        throw new RegistrationException(message);
                    }
                    if ((regConfig.InstallationFlags & InstallationFlags.Register) != InstallationFlags.Default)
                    {
                        flag = !this.IsAssemblyRegistered(spec);
                        this.ClassicRegistration(spec.Assembly);
                        if ((regConfig.InstallationFlags & InstallationFlags.ExpectExistingTypeLib) != InstallationFlags.Default)
                        {
                            RegisterTypeLibrary(spec.TypeLib);
                        }
                        else
                        {
                            flag2 = true;
                            GenerateTypeLibrary(spec.Assembly, spec.TypeLib, new Report(this.ReportWarning));
                        }
                    }
                    if (((regConfig.InstallationFlags & InstallationFlags.Install) != InstallationFlags.Default) && (spec.ConfigurableTypes != null))
                    {
                        if ((regConfig.InstallationFlags & InstallationFlags.CreateTargetApplication) != InstallationFlags.Default)
                        {
                            app = this.CreateApplication(spec, true);
                        }
                        else if ((regConfig.InstallationFlags & InstallationFlags.FindOrCreateTargetApplication) != InstallationFlags.Default)
                        {
                            app = this.FindOrCreateApplication(spec, (regConfig.InstallationFlags & InstallationFlags.ReconfigureExistingApplication) != InstallationFlags.Default);
                        }
                        this.InstallTypeLibrary(spec);
                        if (sync != null)
                        {
                            sync.Set();
                        }
                    }
                    if (((regConfig.InstallationFlags & InstallationFlags.Configure) != InstallationFlags.Default) && (spec.ConfigurableTypes != null))
                    {
                        this.ConfigureComponents(spec);
                        if (sync != null)
                        {
                            sync.Set();
                        }
                    }
                    if (app != null)
                    {
                        this.PostProcessApplication(app, spec);
                    }
                    this.CleanupDriver();
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                if (((exception is SecurityException) || (exception is UnauthorizedAccessException)) || ((exception.InnerException != null) && ((exception.InnerException is SecurityException) || (exception.InnerException is UnauthorizedAccessException))))
                {
                    exception = new RegistrationException(Resource.FormatString("Reg_Unauthorized"), exception);
                }
                if (flag && (null != asm))
                {
                    try
                    {
                        this.ClassicUnregistration(asm);
                    }
                    catch (Exception exception2)
                    {
                        if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                        {
                            throw;
                        }
                    }
                }
                if (flag2 && (null != asm))
                {
                    try
                    {
                        this.UnregisterTypeLib(asm);
                    }
                    catch (Exception exception3)
                    {
                        if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                        {
                            throw;
                        }
                    }
                }
                throw exception;
            }
        }

        private void InstallTypeLibrary(ApplicationSpec spec)
        {
            try
            {
                object[] fileNames = new object[] { spec.TypeLib };
                Type[] normalTypes = spec.NormalTypes;
                if (normalTypes != null)
                {
                    if ((normalTypes == null) || (normalTypes.Length == 0))
                    {
                        throw new RegistrationException(Resource.FormatString("Reg_NoConfigTypesErr"));
                    }
                    object[] cLSIDS = new object[normalTypes.Length];
                    for (int i = 0; i < normalTypes.Length; i++)
                    {
                        cLSIDS[i] = "{" + Marshal.GenerateGuidForType(normalTypes[i]).ToString() + "}";
                    }
                    this._cat.InstallMultipleComponents(spec.DefinitiveName, ref fileNames, ref cLSIDS);
                }
                normalTypes = spec.EventTypes;
                if (normalTypes != null)
                {
                    if ((normalTypes == null) || (normalTypes.Length == 0))
                    {
                        throw new RegistrationException(Resource.FormatString("Reg_NoConfigTypesErr"));
                    }
                    object[] objArray3 = new object[normalTypes.Length];
                    for (int j = 0; j < normalTypes.Length; j++)
                    {
                        objArray3[j] = "{" + Marshal.GenerateGuidForType(normalTypes[j]).ToString() + "}";
                    }
                    this._cat.InstallMultipleEventClasses(spec.DefinitiveName, ref fileNames, ref objArray3);
                }
            }
            catch (COMException exception)
            {
                throw this.WrapCOMException(null, exception, Resource.FormatString("Reg_TypeLibInstallErr", spec.TypeLib, spec.Name));
            }
        }

        private object InvokeMemberHelper(Type type, string name, BindingFlags invokeAttr, Binder binder, object target, object[] args)
        {
            object obj2;
            try
            {
                obj2 = type.InvokeMember(name, invokeAttr, binder, target, args, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        internal bool IsAssemblyRegistered(ApplicationSpec spec)
        {
            bool flag = false;
            if ((spec == null) || (spec.ConfigurableTypes == null))
            {
                return false;
            }
            RegistryKey key = Registry.ClassesRoot.OpenSubKey("CLSID");
            if (key == null)
            {
                throw new RegistrationException(Resource.FormatString("Reg_RegistryErr"));
            }
            foreach (Type type in spec.ConfigurableTypes)
            {
                string name = "{" + Marshal.GenerateGuidForType(type).ToString() + "}";
                RegistryKey key2 = null;
                RegistryKey key3 = null;
                try
                {
                    key2 = key.OpenSubKey(name);
                    if (key2 != null)
                    {
                        key3 = key2.OpenSubKey("InprocServer32");
                        if (((key3 != null) && (key3.GetValue("Assembly") != null)) && (key3.GetValue("Class") != null))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (key3 != null)
                    {
                        key3.Close();
                    }
                    if (key2 != null)
                    {
                        key2.Close();
                    }
                }
            }
            key.Close();
            return flag;
        }

        internal Assembly LoadAssembly(string assembly)
        {
            assembly = Path.GetFullPath(assembly).ToLower(CultureInfo.InvariantCulture);
            bool flag = false;
            string currentDirectory = null;
            string directoryName = Path.GetDirectoryName(assembly);
            currentDirectory = Environment.CurrentDirectory;
            if (currentDirectory != directoryName)
            {
                Environment.CurrentDirectory = directoryName;
                flag = true;
            }
            Assembly assembly2 = null;
            try
            {
                assembly2 = Assembly.LoadFrom(assembly);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_AssemblyLoadErr", assembly), exception);
            }
            if (flag)
            {
                Environment.CurrentDirectory = currentDirectory;
            }
            if (assembly2 == null)
            {
                throw new RegistrationException(Resource.FormatString("Reg_AssemblyLoadErr", assembly));
            }
            if (assembly2.GetName().Name == "System.EnterpriseServices")
            {
                throw new RegistrationException(Resource.FormatString("RegSvcs_NoBootstrap"));
            }
            return assembly2;
        }

        internal Assembly NewLoadAssembly(string assembly)
        {
            Assembly assembly2;
            if (!File.Exists(assembly))
            {
                assembly2 = Assembly.Load(assembly);
                this.CheckAssemblySCValidity(assembly2);
                return assembly2;
            }
            assembly2 = this.LoadAssembly(assembly);
            this.CheckAssemblySCValidity(assembly2);
            if (!this.AssemblyHasStrongName(assembly2))
            {
                throw new RegistrationException(Resource.FormatString("Reg_NoStrongName", assembly));
            }
            return assembly2;
        }

        internal static void Populate(ICatalogCollection coll)
        {
            try
            {
                coll.Populate();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2146368511)
                {
                    throw;
                }
            }
        }

        private void PostProcessApplication(ICatalogObject app, ApplicationSpec spec)
        {
            try
            {
                if (this.AfterSaveChanges(spec.Assembly, app, this._appColl, "Application", this._cache))
                {
                    SaveChanges(this._appColl);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_ConfigUnkErr"), exception);
            }
        }

        private void PrepArguments(RegistrationConfig regConfig)
        {
            if ((regConfig.AssemblyFile == null) || (regConfig.AssemblyFile.Length == 0))
            {
                throw new RegistrationException(Resource.FormatString("Reg_ArgumentAssembly"));
            }
            if (((regConfig.InstallationFlags & InstallationFlags.ExpectExistingTypeLib) != InstallationFlags.Default) && ((regConfig.TypeLibrary == null) || (regConfig.TypeLibrary.Length == 0)))
            {
                throw new RegistrationException(Resource.FormatString("Reg_ExpectExisting"));
            }
            if (((regConfig.InstallationFlags & InstallationFlags.CreateTargetApplication) != InstallationFlags.Default) && ((regConfig.InstallationFlags & InstallationFlags.FindOrCreateTargetApplication) != InstallationFlags.Default))
            {
                throw new RegistrationException(Resource.FormatString("Reg_CreateFlagErr"));
            }
            if ((((regConfig.InstallationFlags & InstallationFlags.Register) == InstallationFlags.Default) && ((regConfig.InstallationFlags & InstallationFlags.Install) == InstallationFlags.Default)) && ((regConfig.InstallationFlags & InstallationFlags.Configure) == InstallationFlags.Default))
            {
                regConfig.InstallationFlags |= InstallationFlags.Configure | InstallationFlags.Install | InstallationFlags.Register;
            }
            this._installFlags = regConfig.InstallationFlags;
            if ((regConfig.Partition != null) && (regConfig.Partition.Length != 0))
            {
                string strB = "Base Application Partition";
                string str2 = "{41E90F3E-56C1-4633-81C3-6E8BAC8BDD70}";
                if ((string.Compare(regConfig.Partition, str2, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(regConfig.Partition, strB, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    regConfig.Partition = null;
                }
            }
            if ((regConfig.ApplicationRootDirectory != null) && !Directory.Exists(regConfig.ApplicationRootDirectory))
            {
                throw new RegistrationException(Resource.FormatString("Reg_BadAppRootDir"));
            }
        }

        private void PrepDriver(ref ApplicationSpec spec)
        {
            try
            {
                this._cat = (ICatalog) new xCatalog();
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_CatalogErr"), exception);
            }
            if (((spec.Partition == null) || (spec.Partition.Length == 0)) && (spec.ID != null))
            {
                try
                {
                    Type type = this._cat.GetType();
                    try
                    {
                        spec.Partition = (string) this.InvokeMemberHelper(type, "GetAppPartitionId", BindingFlags.InvokeMethod, null, this._cat, new object[] { spec.ID });
                    }
                    catch (COMException exception2)
                    {
                        if (-2147352570 == exception2.ErrorCode)
                        {
                            spec.Partition = (string) this.InvokeMemberHelper(type, "GetPartitionID", BindingFlags.InvokeMethod, null, this._cat, new object[] { spec.ID });
                        }
                    }
                }
                catch (Exception exception3)
                {
                    if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                    {
                        throw;
                    }
                }
            }
            if ((spec.Partition != null) && (spec.Partition.Length != 0))
            {
                try
                {
                    Type type2 = this._cat.GetType();
                    try
                    {
                        this.InvokeMemberHelper(type2, "SetApplicationPartition", BindingFlags.InvokeMethod, null, this._cat, new object[] { spec.Partition });
                    }
                    catch (COMException exception4)
                    {
                        if (-2147352570 != exception4.ErrorCode)
                        {
                            throw;
                        }
                        this.InvokeMemberHelper(type2, "CurrentPartition", BindingFlags.SetProperty, null, this._cat, new object[] { spec.Partition });
                    }
                }
                catch (Exception exception5)
                {
                    if ((exception5 is NullReferenceException) || (exception5 is SEHException))
                    {
                        throw;
                    }
                    throw new RegistrationException(Resource.FormatString("Reg_PartitionErr", spec.Partition), exception5);
                }
            }
            try
            {
                this._appColl = (ICatalogCollection) this._cat.GetCollection(CollectionName.Applications);
                Populate(this._appColl);
            }
            catch (Exception exception6)
            {
                if ((exception6 is NullReferenceException) || (exception6 is SEHException))
                {
                    throw;
                }
                throw new RegistrationException(Resource.FormatString("Reg_CatalogErr"), exception6);
            }
            this._cache = new Hashtable();
        }

        private static void RegisterTypeLibrary(string tlb)
        {
            IntPtr zero = IntPtr.Zero;
            tlb = Path.GetFullPath(tlb);
            int errorCode = System.EnterpriseServices.Util.LoadTypeLibEx(tlb, 1, out zero);
            if ((errorCode < 0) || (zero == IntPtr.Zero))
            {
                Exception exceptionForHR = Marshal.GetExceptionForHR(errorCode);
                throw new RegistrationException(Resource.FormatString("Reg_TypeLibRegErr", tlb), exceptionForHR);
            }
            errorCode = System.EnterpriseServices.Util.RegisterTypeLib(zero, tlb, Path.GetDirectoryName(tlb));
            if ((errorCode < 0) || (zero == IntPtr.Zero))
            {
                Exception inner = Marshal.GetExceptionForHR(errorCode);
                throw new RegistrationException(Resource.FormatString("Reg_TypeLibRegErr", tlb), inner);
            }
            Marshal.Release(zero);
        }

        internal void ReportWarning(string msg)
        {
            if ((this._installFlags & InstallationFlags.ReportWarningsToConsole) != InstallationFlags.Default)
            {
                Console.WriteLine(msg);
            }
        }

        internal static void SaveChanges(ICatalogCollection coll)
        {
            coll.SaveChanges();
        }

        public void UninstallAssembly(RegistrationConfig regConfig, object obSync)
        {
            CatalogSync sync = null;
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            if (obSync != null)
            {
                if (!(obSync is CatalogSync))
                {
                    throw new ArgumentException(Resource.FormatString("Err_obSync"));
                }
                sync = (CatalogSync) obSync;
            }
            Assembly asm = this.NewLoadAssembly(regConfig.AssemblyFile);
            ApplicationSpec spec = new ApplicationSpec(asm, regConfig);
            if (spec.ConfigurableTypes != null)
            {
                this.PrepDriver(ref spec);
                if (spec.ConfigurableTypes != null)
                {
                    ICatalogObject obj2 = this.FindApplication(this._appColl, spec);
                    if (obj2 == null)
                    {
                        throw new RegistrationException(Resource.FormatString("Reg_AppNotFoundErr", spec));
                    }
                    ICatalogCollection collection = (ICatalogCollection) this._appColl.GetCollection(CollectionName.Components, obj2.Key());
                    string[] arr = new string[spec.ConfigurableTypes.Length];
                    int index = 0;
                    foreach (Type type in spec.ConfigurableTypes)
                    {
                        arr[index] = Marshal.GenerateGuidForType(type).ToString();
                        index++;
                    }
                    Populate(collection);
                    bool flag = true;
                    int lIndex = 0;
                    while (lIndex < collection.Count())
                    {
                        ICatalogObject obj3 = (ICatalogObject) collection.Item(lIndex);
                        string g = (string) obj3.Key();
                        g = new Guid(g).ToString();
                        if (this.FindIndexOf(arr, g) != -1)
                        {
                            collection.Remove(lIndex);
                            if (sync != null)
                            {
                                sync.Set();
                            }
                        }
                        else
                        {
                            lIndex++;
                            flag = false;
                        }
                    }
                    SaveChanges(collection);
                    if (flag)
                    {
                        for (int i = 0; i < this._appColl.Count(); i++)
                        {
                            ICatalogObject obj4 = (ICatalogObject) this._appColl.Item(i);
                            if (obj4.Key().Equals(obj2.Key()))
                            {
                                this._appColl.Remove(i);
                                if (sync != null)
                                {
                                    sync.Set();
                                }
                                break;
                            }
                        }
                        SaveChanges(this._appColl);
                    }
                }
                this.UnregisterAssembly(asm, spec);
                this.CleanupDriver();
            }
        }

        internal void UnregisterAssembly(Assembly asm, ApplicationSpec spec)
        {
            bool flag = true;
            if ((null != asm) && ((spec != null) && (spec.ConfigurableTypes != null)))
            {
                foreach (Type type in spec.ConfigurableTypes)
                {
                    string str = "{" + Marshal.GenerateGuidForType(type).ToString() + "}";
                    try
                    {
                        int num = 0;
                        Type type2 = this._cat.GetType();
                        try
                        {
                            object[] args = new object[5];
                            args[0] = str;
                            num = (int) this.InvokeMemberHelper(type2, "GetComponentVersions", BindingFlags.InvokeMethod, null, this._cat, args);
                        }
                        catch (COMException exception)
                        {
                            if (-2147352570 != exception.ErrorCode)
                            {
                                throw;
                            }
                            num = (int) this.InvokeMemberHelper(type2, "GetComponentVersionCount", BindingFlags.InvokeMethod, null, this._cat, new object[] { str });
                        }
                        if (num > 0)
                        {
                            flag = false;
                            break;
                        }
                    }
                    catch (COMException exception2)
                    {
                        if (-2147221164 != exception2.ErrorCode)
                        {
                            throw;
                        }
                    }
                }
                if (flag)
                {
                    this.ClassicUnregistration(asm);
                    try
                    {
                        this.UnregisterTypeLib(asm);
                    }
                    catch (Exception exception3)
                    {
                        if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                        {
                            throw;
                        }
                    }
                }
            }
        }

        internal void UnregisterTypeLib(Assembly asm)
        {
            IntPtr zero = IntPtr.Zero;
            object pptlib = null;
            ITypeLib o = null;
            try
            {
                Guid typeLibGuidForAssembly = Marshal.GetTypeLibGuidForAssembly(asm);
                Version version = asm.GetName().Version;
                if ((version.Major == 0) && (version.Minor == 0))
                {
                    version = new Version(1, 0);
                }
                if (System.EnterpriseServices.Util.LoadRegTypeLib(typeLibGuidForAssembly, (short) version.Major, (short) version.Minor, 0, out pptlib) == 0)
                {
                    o = (ITypeLib) pptlib;
                    o.GetLibAttr(out zero);
                    System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                    System.EnterpriseServices.Util.UnRegisterTypeLib(typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, typelibattr.lcid, typelibattr.syskind);
                }
            }
            finally
            {
                if ((o != null) && (zero != IntPtr.Zero))
                {
                    o.ReleaseTLibAttr(zero);
                }
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
        }

        private bool ValidateBitness(ApplicationSpec spec, out string message)
        {
            bool flag = false;
            bool flag2 = true;
            message = string.Empty;
            if (Wow64Helper.IsWow64Supported())
            {
                flag = Wow64Helper.IsWow64Process();
                ICatalogObject obj2 = this.FindApplication(this._appColl, spec);
                if (obj2 == null)
                {
                    return flag2;
                }
                ICatalogCollection collection = (ICatalogCollection) this._appColl.GetCollection(CollectionName.Components, obj2.Key());
                Populate(collection);
                int num = collection.Count();
                if (num <= 0)
                {
                    return flag2;
                }
                Guid[] arr = new Guid[spec.ConfigurableTypes.Length];
                for (int i = 0; i < spec.ConfigurableTypes.Length; i++)
                {
                    arr[i] = Marshal.GenerateGuidForType(spec.ConfigurableTypes[i]);
                }
                for (int j = 0; j < num; j++)
                {
                    ICatalogObject obj3 = (ICatalogObject) collection.Item(j);
                    string g = (string) obj3.Key();
                    Guid key = new Guid(g);
                    if (this.FindIndexOf(arr, key) != -1)
                    {
                        int num4 = (int) obj3.GetValue("Bitness");
                        if (flag && (num4 == 2))
                        {
                            message = Resource.FormatString("Reg_Already64bit");
                            return false;
                        }
                        if (!flag && (num4 == 1))
                        {
                            message = Resource.FormatString("Reg_Already32bit");
                            return false;
                        }
                    }
                }
            }
            return flag2;
        }

        private RegistrationException WrapCOMException(ICatalogCollection coll, COMException e, string msg)
        {
            RegistrationErrorInfo[] errorInfo = null;
            if (e.ErrorCode == -2146368511)
            {
                ICatalogCollection collection = null;
                if (coll == null)
                {
                    collection = (ICatalogCollection) this._cat.GetCollection("ErrorInfo");
                }
                else
                {
                    collection = (ICatalogCollection) coll.GetCollection("ErrorInfo", "");
                }
                if (collection != null)
                {
                    errorInfo = BuildErrorInfoChain(collection);
                }
            }
            return new RegistrationException(msg, errorInfo);
        }
    }
}

