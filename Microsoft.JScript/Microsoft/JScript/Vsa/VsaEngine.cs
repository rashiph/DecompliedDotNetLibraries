namespace Microsoft.JScript.Vsa
{
    using Microsoft.JScript;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Resources;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Xml;

    [Guid("B71E484D-93ED-4b56-BFB9-CEED5134822B"), ComVisible(true), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public sealed class VsaEngine : BaseVsaEngine, IEngine2, IRedirectOutput
    {
        private static TypeReferences _reflectionOnlyTypeRefs;
        internal bool alwaysGenerateIL;
        private static Hashtable assemblyReferencesTable = null;
        private bool autoRef;
        private SimpleHashtable cachedTypeLookups;
        internal int classCounter;
        private Microsoft.JScript.CompilerGlobals compilerGlobals;
        private static readonly Version CurrentProjectVersion = new Version("1.0");
        private string debugDirectory;
        private Hashtable Defines;
        internal bool doCRS;
        internal bool doFast;
        internal bool doPrint;
        internal bool doSaveAfterCompile;
        private bool doWarnAsError;
        private static string engineVersion = GetVersionString();
        private CultureInfo errorCultureInfo;
        internal static bool executeForJSEE = false;
        private static volatile VsaEngine exeEngine;
        internal bool genStartupClass;
        private Microsoft.JScript.Globals globals;
        internal VsaScriptScope globalScope;
        private ArrayList implicitAssemblies;
        private SimpleHashtable implicitAssemblyCache;
        internal bool isCLSCompliant;
        private bool isCompilerSet;
        private string libpath;
        private string[] libpathList;
        private ICollection managedResources;
        private int numberOfErrors;
        private int nWarningLevel;
        private ArrayList packages;
        internal PEFileKinds PEFileKind;
        private string PEFileName;
        internal PortableExecutableKinds PEKindFlags;
        internal ImageFileMachine PEMachineArchitecture;
        private RNGCryptoServiceProvider randomNumberGenerator;
        private byte[] rawPDB;
        private byte[] rawPE;
        internal LoaderAPI ReferenceLoaderAPI;
        private static Module reflectionOnlyJScriptModule = null;
        private static Module reflectionOnlyVsaModule = null;
        internal Thread runningThread;
        private Assembly runtimeAssembly;
        private string runtimeDirectory;
        private ArrayList scopes;
        private string tempDirectory;
        private Hashtable typenameTable;
        private Version versionInfo;
        internal bool versionSafe;
        private string win32resource;

        public VsaEngine() : this(true)
        {
        }

        public VsaEngine(bool fast) : base("JScript", engineVersion, true)
        {
            this.alwaysGenerateIL = false;
            this.autoRef = false;
            this.doCRS = false;
            this.doFast = fast;
            base.genDebugInfo = false;
            this.genStartupClass = true;
            this.doPrint = false;
            this.doWarnAsError = false;
            this.nWarningLevel = 4;
            this.isCLSCompliant = false;
            this.versionSafe = false;
            this.PEFileName = null;
            this.PEFileKind = PEFileKinds.Dll;
            this.PEKindFlags = PortableExecutableKinds.ILOnly;
            this.PEMachineArchitecture = ImageFileMachine.I386;
            this.ReferenceLoaderAPI = LoaderAPI.LoadFrom;
            this.errorCultureInfo = null;
            this.libpath = null;
            this.libpathList = null;
            this.globalScope = null;
            base.vsaItems = new VsaItems(this);
            this.packages = null;
            this.scopes = null;
            this.classCounter = 0;
            this.implicitAssemblies = null;
            this.implicitAssemblyCache = null;
            this.cachedTypeLookups = null;
            base.isEngineRunning = false;
            base.isEngineCompiled = false;
            this.isCompilerSet = false;
            base.isClosed = false;
            this.runningThread = null;
            this.compilerGlobals = null;
            this.globals = null;
            this.runtimeDirectory = null;
            Microsoft.JScript.Globals.contextEngine = this;
            this.runtimeAssembly = null;
            this.typenameTable = null;
        }

        private VsaEngine(Assembly runtimeAssembly) : this(true)
        {
            this.runtimeAssembly = runtimeAssembly;
        }

        private void AddChildAndValue(XmlDocument doc, XmlElement parent, string name, string value)
        {
            XmlElement elem = doc.CreateElement(name);
            this.CreateAttribute(doc, elem, "Value", value);
            parent.AppendChild(elem);
        }

        internal void AddPackage(PackageScope pscope)
        {
            if (this.packages == null)
            {
                this.packages = new ArrayList(8);
            }
            IEnumerator enumerator = this.packages.GetEnumerator();
            while (enumerator.MoveNext())
            {
                PackageScope current = (PackageScope) enumerator.Current;
                if (current.name.Equals(pscope.name))
                {
                    current.owner.MergeWith(pscope.owner);
                    return;
                }
            }
            this.packages.Add(pscope);
        }

        private void AddReferences()
        {
            if (assemblyReferencesTable == null)
            {
                Hashtable table = new Hashtable();
                assemblyReferencesTable = Hashtable.Synchronized(table);
            }
            string[] strArray = assemblyReferencesTable[this.runtimeAssembly.FullName] as string[];
            if (strArray != null)
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    VsaReference reference = (VsaReference) base.vsaItems.CreateItem(strArray[i], JSVsaItemType.Reference, JSVsaItemFlag.None);
                    reference.AssemblyName = strArray[i];
                }
            }
            else
            {
                object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.runtimeAssembly, typeof(ReferenceAttribute), false);
                string[] strArray2 = new string[objArray.Length];
                for (int j = 0; j < objArray.Length; j++)
                {
                    string name = ((ReferenceAttribute) objArray[j]).reference;
                    VsaReference reference2 = (VsaReference) base.vsaItems.CreateItem(name, JSVsaItemType.Reference, JSVsaItemFlag.None);
                    reference2.AssemblyName = name;
                    strArray2[j] = name;
                }
                assemblyReferencesTable[this.runtimeAssembly.FullName] = strArray2;
            }
        }

        internal void CheckForErrors()
        {
            if (!base.isClosed && !base.isEngineCompiled)
            {
                this.SetUpCompilerEnvironment();
                this.Globals.ScopeStack.Push(this.GetGlobalScope().GetObject());
                try
                {
                    foreach (object obj2 in base.vsaItems)
                    {
                        if (obj2 is VsaReference)
                        {
                            ((VsaReference) obj2).Compile();
                        }
                    }
                    if (base.vsaItems.Count > 0)
                    {
                        this.SetEnclosingContext(new WrappedNamespace("", this));
                    }
                    foreach (object obj3 in base.vsaItems)
                    {
                        if (!(obj3 is VsaReference))
                        {
                            ((VsaItem) obj3).CheckForErrors();
                        }
                    }
                    if (this.globalScope != null)
                    {
                        this.globalScope.CheckForErrors();
                    }
                }
                finally
                {
                    this.Globals.ScopeStack.Pop();
                }
            }
            this.globalScope = null;
        }

        internal static bool CheckIdentifierForCLSCompliance(string name)
        {
            if (name[0] == '_')
            {
                return false;
            }
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '$')
                {
                    return false;
                }
            }
            return true;
        }

        internal void CheckTypeNameForCLSCompliance(string name, string fullname, Context context)
        {
            if (this.isCLSCompliant)
            {
                if (name[0] == '_')
                {
                    context.HandleError(JSError.NonCLSCompliantType);
                }
                else if (!CheckIdentifierForCLSCompliance(fullname))
                {
                    context.HandleError(JSError.NonCLSCompliantType);
                }
                else
                {
                    if (this.typenameTable == null)
                    {
                        this.typenameTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                    }
                    if (this.typenameTable[fullname] == null)
                    {
                        this.typenameTable[fullname] = fullname;
                    }
                    else
                    {
                        context.HandleError(JSError.NonCLSCompliantType);
                    }
                }
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public IJSVsaEngine Clone(AppDomain domain)
        {
            throw new NotImplementedException();
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public bool CompileEmpty()
        {
            bool flag;
            base.TryObtainLock();
            try
            {
                flag = this.DoCompile();
            }
            finally
            {
                base.ReleaseLock();
            }
            return flag;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void ConnectEvents()
        {
        }

        private void CreateAttribute(XmlDocument doc, XmlElement elem, string name, string value)
        {
            XmlAttribute newAttr = doc.CreateAttribute(name);
            elem.SetAttributeNode(newAttr);
            elem.SetAttribute(name, value);
        }

        public static VsaEngine CreateEngine()
        {
            if (exeEngine == null)
            {
                VsaEngine engine = new VsaEngine(true);
                engine.InitVsaEngine("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa", new DefaultVsaSite());
                exeEngine = engine;
            }
            return exeEngine;
        }

        public static GlobalScope CreateEngineAndGetGlobalScope(bool fast, string[] assemblyNames)
        {
            VsaEngine engine = new VsaEngine(fast);
            engine.InitVsaEngine("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa", new DefaultVsaSite());
            engine.doPrint = true;
            engine.SetEnclosingContext(new WrappedNamespace("", engine));
            foreach (string str in assemblyNames)
            {
                VsaReference reference = (VsaReference) engine.vsaItems.CreateItem(str, JSVsaItemType.Reference, JSVsaItemFlag.None);
                reference.AssemblyName = str;
            }
            exeEngine = engine;
            GlobalScope scope = (GlobalScope) engine.GetGlobalScope().GetObject();
            scope.globalObject = engine.Globals.globalObject;
            return scope;
        }

        public static GlobalScope CreateEngineAndGetGlobalScopeWithType(bool fast, string[] assemblyNames, RuntimeTypeHandle callingTypeHandle)
        {
            return CreateEngineAndGetGlobalScopeWithTypeAndRootNamespace(fast, assemblyNames, callingTypeHandle, null);
        }

        public static GlobalScope CreateEngineAndGetGlobalScopeWithTypeAndRootNamespace(bool fast, string[] assemblyNames, RuntimeTypeHandle callingTypeHandle, string rootNamespace)
        {
            VsaEngine engine = new VsaEngine(fast);
            engine.InitVsaEngine("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa", new DefaultVsaSite());
            engine.doPrint = true;
            engine.SetEnclosingContext(new WrappedNamespace("", engine));
            if (rootNamespace != null)
            {
                engine.SetEnclosingContext(new WrappedNamespace(rootNamespace, engine));
            }
            foreach (string str in assemblyNames)
            {
                VsaReference reference = (VsaReference) engine.vsaItems.CreateItem(str, JSVsaItemType.Reference, JSVsaItemFlag.None);
                reference.AssemblyName = str;
            }
            Assembly assembly = Type.GetTypeFromHandle(callingTypeHandle).Assembly;
            System.Runtime.Remoting.Messaging.CallContext.SetData("JScript:" + assembly.FullName, engine);
            GlobalScope scope = (GlobalScope) engine.GetGlobalScope().GetObject();
            scope.globalObject = engine.Globals.globalObject;
            return scope;
        }

        internal static VsaEngine CreateEngineForDebugger()
        {
            VsaEngine engine = new VsaEngine(true);
            engine.InitVsaEngine("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa", new DefaultVsaSite());
            GlobalScope scope = (GlobalScope) engine.GetGlobalScope().GetObject();
            scope.globalObject = engine.Globals.globalObject;
            return engine;
        }

        public static VsaEngine CreateEngineWithType(RuntimeTypeHandle callingTypeHandle)
        {
            Assembly runtimeAssembly = Type.GetTypeFromHandle(callingTypeHandle).Assembly;
            object data = System.Runtime.Remoting.Messaging.CallContext.GetData("JScript:" + runtimeAssembly.FullName);
            if (data != null)
            {
                VsaEngine engine = data as VsaEngine;
                if (engine != null)
                {
                    return engine;
                }
            }
            VsaEngine engine2 = new VsaEngine(runtimeAssembly);
            engine2.InitVsaEngine("JScript.Vsa.VsaEngine://Microsoft.JScript.VsaEngine.Vsa", new DefaultVsaSite());
            GlobalScope scope = (GlobalScope) engine2.GetGlobalScope().GetObject();
            scope.globalObject = engine2.Globals.globalObject;
            int num = 0;
            Type type = null;
            do
            {
                string name = "JScript " + num.ToString(CultureInfo.InvariantCulture);
                type = runtimeAssembly.GetType(name, false);
                if (type != null)
                {
                    engine2.SetEnclosingContext(new WrappedNamespace("", engine2));
                    ConstructorInfo constructor = type.GetConstructor(new Type[] { typeof(GlobalScope) });
                    MethodInfo method = type.GetMethod("Global Code");
                    try
                    {
                        object obj3 = constructor.Invoke(new object[] { scope });
                        method.Invoke(obj3, new object[0]);
                    }
                    catch (SecurityException)
                    {
                        break;
                    }
                }
                num++;
            }
            while (type != null);
            if (data == null)
            {
                System.Runtime.Remoting.Messaging.CallContext.SetData("JScript:" + runtimeAssembly.FullName, engine2);
            }
            return engine2;
        }

        private void CreateEntryPointIL(ILGenerator il, FieldInfo site)
        {
            this.CreateEntryPointIL(il, site, null);
        }

        private void CreateEntryPointIL(ILGenerator il, FieldInfo site, TypeBuilder startupClass)
        {
            LocalBuilder local = il.DeclareLocal(Typeob.GlobalScope);
            if (this.doFast)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
            SimpleHashtable hashtable = new SimpleHashtable((uint) base.vsaItems.Count);
            ArrayList list = new ArrayList();
            foreach (object obj2 in base.vsaItems)
            {
                if (obj2 is VsaReference)
                {
                    string fullName = ((VsaReference) obj2).Assembly.GetName().FullName;
                    if (hashtable[fullName] == null)
                    {
                        list.Add(fullName);
                        hashtable[fullName] = obj2;
                    }
                }
            }
            if (this.implicitAssemblies != null)
            {
                foreach (object obj3 in this.implicitAssemblies)
                {
                    Assembly assembly = obj3 as Assembly;
                    if (assembly != null)
                    {
                        string str2 = assembly.GetName().FullName;
                        if (hashtable[str2] == null)
                        {
                            list.Add(str2);
                            hashtable[str2] = obj3;
                        }
                    }
                }
            }
            ConstantWrapper.TranslateToILInt(il, list.Count);
            il.Emit(OpCodes.Newarr, Typeob.String);
            int num = 0;
            foreach (string str3 in list)
            {
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, num++);
                il.Emit(OpCodes.Ldstr, str3);
                il.Emit(OpCodes.Stelem_Ref);
            }
            if (startupClass != null)
            {
                il.Emit(OpCodes.Ldtoken, startupClass);
                if (base.rootNamespace != null)
                {
                    il.Emit(OpCodes.Ldstr, base.rootNamespace);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
                MethodInfo method = Typeob.VsaEngine.GetMethod("CreateEngineAndGetGlobalScopeWithTypeAndRootNamespace");
                il.Emit(OpCodes.Call, method);
            }
            else
            {
                MethodInfo meth = Typeob.VsaEngine.GetMethod("CreateEngineAndGetGlobalScope");
                il.Emit(OpCodes.Call, meth);
            }
            il.Emit(OpCodes.Stloc, local);
            if (site != null)
            {
                this.CreateHostCallbackIL(il, site);
            }
            bool genDebugInfo = base.genDebugInfo;
            bool flag2 = false;
            foreach (object obj4 in base.vsaItems)
            {
                Type compiledType = ((VsaItem) obj4).GetCompiledType();
                if (null != compiledType)
                {
                    ConstructorInfo constructor = compiledType.GetConstructor(new Type[] { Typeob.GlobalScope });
                    MethodInfo entryPoint = compiledType.GetMethod("Global Code");
                    if (genDebugInfo)
                    {
                        this.CompilerGlobals.module.SetUserEntryPoint(entryPoint);
                        genDebugInfo = false;
                    }
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Newobj, constructor);
                    if (!flag2 && (obj4 is VsaStaticCode))
                    {
                        LocalBuilder builder2 = il.DeclareLocal(compiledType);
                        il.Emit(OpCodes.Stloc, builder2);
                        il.Emit(OpCodes.Ldloc, local);
                        il.Emit(OpCodes.Ldfld, Microsoft.JScript.CompilerGlobals.engineField);
                        il.Emit(OpCodes.Ldloc, builder2);
                        il.Emit(OpCodes.Call, Microsoft.JScript.CompilerGlobals.pushScriptObjectMethod);
                        il.Emit(OpCodes.Ldloc, builder2);
                        flag2 = true;
                    }
                    il.Emit(OpCodes.Call, entryPoint);
                    il.Emit(OpCodes.Pop);
                }
            }
            if (flag2)
            {
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ldfld, Microsoft.JScript.CompilerGlobals.engineField);
                il.Emit(OpCodes.Call, Microsoft.JScript.CompilerGlobals.popScriptObjectMethod);
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ret);
        }

        private void CreateHostCallbackIL(ILGenerator il, FieldInfo site)
        {
            MethodInfo method = site.FieldType.GetMethod("GetGlobalInstance");
            site.FieldType.GetMethod("GetEventSourceInstance");
            foreach (object obj2 in base.vsaItems)
            {
                if (obj2 is VsaHostObject)
                {
                    VsaHostObject obj3 = (VsaHostObject) obj2;
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, site);
                    il.Emit(OpCodes.Ldstr, obj3.Name);
                    il.Emit(OpCodes.Callvirt, method);
                    Type fieldType = obj3.Field.FieldType;
                    il.Emit(OpCodes.Ldtoken, fieldType);
                    il.Emit(OpCodes.Call, Microsoft.JScript.CompilerGlobals.getTypeFromHandleMethod);
                    ConstantWrapper.TranslateToILInt(il, 0);
                    il.Emit(OpCodes.Call, Microsoft.JScript.CompilerGlobals.coerceTMethod);
                    if (fieldType.IsValueType)
                    {
                        Microsoft.JScript.Convert.EmitUnbox(il, fieldType, Type.GetTypeCode(fieldType));
                    }
                    else
                    {
                        il.Emit(OpCodes.Castclass, fieldType);
                    }
                    il.Emit(OpCodes.Stsfld, obj3.Field);
                }
            }
        }

        private void CreateMain()
        {
            TypeBuilder builder = this.CompilerGlobals.module.DefineType("JScript Main", TypeAttributes.Public);
            MethodBuilder entryMethod = builder.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public, Typeob.Void, new Type[] { Typeob.ArrayOfString });
            entryMethod.SetCustomAttribute(Typeob.STAThreadAttribute.GetConstructor(Type.EmptyTypes), new byte[0]);
            ILGenerator iLGenerator = entryMethod.GetILGenerator();
            this.CreateEntryPointIL(iLGenerator, null);
            builder.CreateType();
            this.CompilerGlobals.assemblyBuilder.SetEntryPoint(entryMethod, this.PEFileKind);
        }

        private void CreateShutdownIL(ILGenerator il)
        {
            foreach (object obj2 in base.vsaItems)
            {
                if (obj2 is VsaHostObject)
                {
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stsfld, ((VsaHostObject) obj2).Field);
                }
            }
            il.Emit(OpCodes.Ret);
        }

        private void CreateStartupClass()
        {
            TypeBuilder startupClass = this.CompilerGlobals.module.DefineType(base.rootNamespace + "._Startup", TypeAttributes.Public, Typeob.BaseVsaStartup);
            FieldInfo field = Typeob.BaseVsaStartup.GetField("site", BindingFlags.NonPublic | BindingFlags.Instance);
            this.CreateEntryPointIL(startupClass.DefineMethod("Startup", MethodAttributes.Virtual | MethodAttributes.Public, Typeob.Void, Type.EmptyTypes).GetILGenerator(), field, startupClass);
            this.CreateShutdownIL(startupClass.DefineMethod("Shutdown", MethodAttributes.Virtual | MethodAttributes.Public, Typeob.Void, Type.EmptyTypes).GetILGenerator());
            startupClass.CreateType();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void DisconnectEvents()
        {
        }

        protected override void DoClose()
        {
            ((VsaItems) base.vsaItems).Close();
            if (this.globalScope != null)
            {
                this.globalScope.Close();
            }
            base.vsaItems = null;
            base.engineSite = null;
            this.globalScope = null;
            this.runningThread = null;
            this.compilerGlobals = null;
            this.globals = null;
            ScriptStream.Out = Console.Out;
            ScriptStream.Error = Console.Error;
            this.rawPE = null;
            this.rawPDB = null;
            base.isClosed = true;
            if ((this.tempDirectory != null) && Directory.Exists(this.tempDirectory))
            {
                Directory.Delete(this.tempDirectory);
            }
        }

        protected override bool DoCompile()
        {
            if (!base.isClosed && !base.isEngineCompiled)
            {
                this.SetUpCompilerEnvironment();
                if (this.PEFileName == null)
                {
                    this.PEFileName = this.GenerateRandomPEFileName();
                }
                this.SaveSourceForDebugging();
                this.numberOfErrors = 0;
                base.isEngineCompiled = true;
                this.Globals.ScopeStack.Push(this.GetGlobalScope().GetObject());
                try
                {
                    foreach (object obj2 in base.vsaItems)
                    {
                        if (obj2 is VsaReference)
                        {
                            ((VsaReference) obj2).Compile();
                        }
                    }
                    if (base.vsaItems.Count > 0)
                    {
                        this.SetEnclosingContext(new WrappedNamespace("", this));
                    }
                    foreach (object obj3 in base.vsaItems)
                    {
                        if (obj3 is VsaHostObject)
                        {
                            ((VsaHostObject) obj3).Compile();
                        }
                    }
                    foreach (object obj4 in base.vsaItems)
                    {
                        if (obj4 is VsaStaticCode)
                        {
                            ((VsaStaticCode) obj4).Parse();
                        }
                    }
                    foreach (object obj5 in base.vsaItems)
                    {
                        if (obj5 is VsaStaticCode)
                        {
                            ((VsaStaticCode) obj5).ProcessAssemblyAttributeLists();
                        }
                    }
                    foreach (object obj6 in base.vsaItems)
                    {
                        if (obj6 is VsaStaticCode)
                        {
                            ((VsaStaticCode) obj6).PartiallyEvaluate();
                        }
                    }
                    foreach (object obj7 in base.vsaItems)
                    {
                        if (obj7 is VsaStaticCode)
                        {
                            ((VsaStaticCode) obj7).TranslateToIL();
                        }
                    }
                    foreach (object obj8 in base.vsaItems)
                    {
                        if (obj8 is VsaStaticCode)
                        {
                            ((VsaStaticCode) obj8).GetCompiledType();
                        }
                    }
                    if (this.globalScope != null)
                    {
                        this.globalScope.Compile();
                    }
                }
                catch (JScriptException exception)
                {
                    this.OnCompilerError(exception);
                }
                catch (FileLoadException exception2)
                {
                    JScriptException se = new JScriptException(JSError.ImplicitlyReferencedAssemblyNotFound) {
                        value = exception2.FileName
                    };
                    this.OnCompilerError(se);
                    base.isEngineCompiled = false;
                }
                catch (EndOfFile)
                {
                }
                catch
                {
                    base.isEngineCompiled = false;
                    throw;
                }
                finally
                {
                    this.Globals.ScopeStack.Pop();
                }
                if (base.isEngineCompiled)
                {
                    base.isEngineCompiled = (this.numberOfErrors == 0) || this.alwaysGenerateIL;
                }
            }
            if (this.win32resource != null)
            {
                this.CompilerGlobals.assemblyBuilder.DefineUnmanagedResource(this.win32resource);
            }
            else if (this.compilerGlobals != null)
            {
                this.compilerGlobals.assemblyBuilder.DefineVersionInfoResource();
            }
            if (this.managedResources != null)
            {
                foreach (ResInfo info in this.managedResources)
                {
                    if (info.isLinked)
                    {
                        this.CompilerGlobals.assemblyBuilder.AddResourceFile(info.name, Path.GetFileName(info.filename), info.isPublic ? ResourceAttributes.Public : ResourceAttributes.Private);
                    }
                    else
                    {
                        try
                        {
                            using (ResourceReader reader = new ResourceReader(info.filename))
                            {
                                IResourceWriter writer = this.CompilerGlobals.module.DefineResource(info.name, info.filename, info.isPublic ? ResourceAttributes.Public : ResourceAttributes.Private);
                                foreach (DictionaryEntry entry in reader)
                                {
                                    writer.AddResource((string) entry.Key, entry.Value);
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                            JScriptException exception4 = new JScriptException(JSError.InvalidResource) {
                                value = info.filename
                            };
                            this.OnCompilerError(exception4);
                            base.isEngineCompiled = false;
                            return false;
                        }
                    }
                }
            }
            if (base.isEngineCompiled)
            {
                this.EmitReferences();
            }
            if (base.isEngineCompiled)
            {
                if (this.doSaveAfterCompile)
                {
                    if (this.PEFileKind != PEFileKinds.Dll)
                    {
                        this.CreateMain();
                    }
                    try
                    {
                        this.compilerGlobals.assemblyBuilder.Save(Path.GetFileName(this.PEFileName), this.PEKindFlags, this.PEMachineArchitecture);
                        goto Label_052D;
                    }
                    catch (Exception exception5)
                    {
                        throw new JSVsaException(JSVsaError.SaveCompiledStateFailed, exception5.Message, exception5);
                    }
                }
                if (this.genStartupClass)
                {
                    this.CreateStartupClass();
                }
            }
        Label_052D:
            return base.isEngineCompiled;
        }

        protected override void DoLoadSourceState(IJSVsaPersistSite site)
        {
            string xml = site.LoadElement(null);
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                XmlElement documentElement = document.DocumentElement;
                if (this.LoadProjectVersion(documentElement) == CurrentProjectVersion)
                {
                    this.LoadVsaEngineState(documentElement);
                    base.isEngineDirty = false;
                }
            }
            catch (Exception exception)
            {
                throw new JSVsaException(JSVsaError.UnknownError, exception.ToString(), exception);
            }
        }

        protected override void DoSaveCompiledState(out byte[] pe, out byte[] pdb)
        {
            pe = null;
            pdb = null;
            if (this.rawPE == null)
            {
                try
                {
                    if (!Directory.Exists(this.tempDirectory))
                    {
                        Directory.CreateDirectory(this.tempDirectory);
                    }
                    this.compilerGlobals.assemblyBuilder.Save(Path.GetFileName(this.PEFileName), this.PEKindFlags, this.PEMachineArchitecture);
                    string path = Path.ChangeExtension(this.PEFileName, ".pdb");
                    try
                    {
                        FileStream stream = new FileStream(this.PEFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        try
                        {
                            this.rawPE = new byte[(int) stream.Length];
                            stream.Read(this.rawPE, 0, this.rawPE.Length);
                        }
                        finally
                        {
                            stream.Close();
                        }
                        if (File.Exists(path))
                        {
                            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                            try
                            {
                                this.rawPDB = new byte[(int) stream.Length];
                                stream.Read(this.rawPDB, 0, this.rawPDB.Length);
                            }
                            finally
                            {
                                stream.Close();
                            }
                        }
                    }
                    finally
                    {
                        File.Delete(this.PEFileName);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new JSVsaException(JSVsaError.SaveCompiledStateFailed, exception.ToString(), exception);
                }
            }
            pe = this.rawPE;
            pdb = this.rawPDB;
        }

        protected override void DoSaveSourceState(IJSVsaPersistSite site)
        {
            XmlDocument project = new XmlDocument();
            project.LoadXml("<project></project>");
            XmlElement documentElement = project.DocumentElement;
            this.SaveProjectVersion(project, documentElement);
            this.SaveVsaEngineState(project, documentElement);
            site.SaveElement(null, project.OuterXml);
            this.SaveSourceForDebugging();
            base.isEngineDirty = false;
        }

        private void EmitReferences()
        {
            SimpleHashtable hashtable = new SimpleHashtable((uint) (base.vsaItems.Count + ((this.implicitAssemblies == null) ? 0 : this.implicitAssemblies.Count)));
            foreach (object obj2 in base.vsaItems)
            {
                if (obj2 is VsaReference)
                {
                    string fullName = ((VsaReference) obj2).Assembly.GetName().FullName;
                    if (hashtable[fullName] == null)
                    {
                        CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(Microsoft.JScript.CompilerGlobals.referenceAttributeConstructor, new object[] { fullName });
                        this.CompilerGlobals.assemblyBuilder.SetCustomAttribute(customBuilder);
                        hashtable[fullName] = obj2;
                    }
                }
            }
            if (this.implicitAssemblies != null)
            {
                foreach (object obj3 in this.implicitAssemblies)
                {
                    Assembly assembly = obj3 as Assembly;
                    if (assembly != null)
                    {
                        string str2 = assembly.GetName().FullName;
                        if (hashtable[str2] == null)
                        {
                            CustomAttributeBuilder builder2 = new CustomAttributeBuilder(Microsoft.JScript.CompilerGlobals.referenceAttributeConstructor, new object[] { str2 });
                            this.CompilerGlobals.assemblyBuilder.SetCustomAttribute(builder2);
                            hashtable[str2] = obj3;
                        }
                    }
                }
            }
        }

        internal void EnsureReflectionOnlyModulesLoaded()
        {
            if (reflectionOnlyVsaModule == null)
            {
                reflectionOnlyVsaModule = Assembly.ReflectionOnlyLoadFrom(typeof(IJSVsaEngine).Assembly.Location).GetModule("Microsoft.JScript.Vsa.dll");
                reflectionOnlyJScriptModule = Assembly.ReflectionOnlyLoadFrom(typeof(VsaEngine).Assembly.Location).GetModule("Microsoft.JScript.dll");
            }
        }

        internal string FindAssembly(string name)
        {
            string path = name;
            if (Path.GetFileName(name) == name)
            {
                if (File.Exists(name))
                {
                    path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + name;
                }
                else
                {
                    string str2 = this.RuntimeDirectory + Path.DirectorySeparatorChar + name;
                    if (File.Exists(str2))
                    {
                        path = str2;
                    }
                    else
                    {
                        foreach (string str3 in this.LibpathList)
                        {
                            if (str3.Length > 0)
                            {
                                str2 = str3 + Path.DirectorySeparatorChar + name;
                                if (File.Exists(str2))
                                {
                                    path = str2;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!File.Exists(path))
            {
                return null;
            }
            return path;
        }

        private string GenerateRandomPEFileName()
        {
            if (this.randomNumberGenerator == null)
            {
                this.randomNumberGenerator = new RNGCryptoServiceProvider();
            }
            byte[] data = new byte[6];
            this.randomNumberGenerator.GetBytes(data);
            string str = System.Convert.ToBase64String(data).Replace('/', '-').Replace('+', '_');
            if (this.tempDirectory == null)
            {
                this.tempDirectory = Path.GetTempPath() + str;
            }
            string str2 = str + ((this.PEFileKind == PEFileKinds.Dll) ? ".dll" : ".exe");
            return (this.tempDirectory + Path.DirectorySeparatorChar + str2);
        }

        [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
        public Assembly GetAssembly()
        {
            Assembly assemblyBuilder;
            base.TryObtainLock();
            try
            {
                if (this.PEFileName != null)
                {
                    return Assembly.LoadFrom(this.PEFileName);
                }
                assemblyBuilder = this.compilerGlobals.assemblyBuilder;
            }
            finally
            {
                base.ReleaseLock();
            }
            return assemblyBuilder;
        }

        internal ClassScope GetClass(string className)
        {
            if (this.packages != null)
            {
                int num = 0;
                int count = this.packages.Count;
                while (num < count)
                {
                    object memberValue = ((PackageScope) this.packages[num]).GetMemberValue(className, 1);
                    if (!(memberValue is Microsoft.JScript.Missing))
                    {
                        return (ClassScope) memberValue;
                    }
                    num++;
                }
            }
            return null;
        }

        protected override object GetCustomOption(string name)
        {
            if (string.Compare(name, "CLSCompliant", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.isCLSCompliant;
            }
            if (string.Compare(name, "fast", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.doFast;
            }
            if (string.Compare(name, "output", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.PEFileName;
            }
            if (string.Compare(name, "PEFileKind", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.PEFileKind;
            }
            if (string.Compare(name, "PortableExecutableKind", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.PEKindFlags;
            }
            if (string.Compare(name, "ImageFileMachine", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.PEMachineArchitecture;
            }
            if (string.Compare(name, "ReferenceLoaderAPI", StringComparison.OrdinalIgnoreCase) == 0)
            {
                switch (this.ReferenceLoaderAPI)
                {
                    case LoaderAPI.LoadFrom:
                        return "LoadFrom";

                    case LoaderAPI.LoadFile:
                        return "LoadFile";

                    case LoaderAPI.ReflectionOnlyLoadFrom:
                        return "ReflectionOnlyLoadFrom";
                }
                throw new JSVsaException(JSVsaError.OptionNotSupported);
            }
            if (string.Compare(name, "print", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.doPrint;
            }
            if (string.Compare(name, "UseContextRelativeStatics", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.doCRS;
            }
            if (string.Compare(name, "optimize", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
            if (string.Compare(name, "define", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
            if (string.Compare(name, "defines", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.Defines;
            }
            if (string.Compare(name, "ee", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return executeForJSEE;
            }
            if (string.Compare(name, "version", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.versionInfo;
            }
            if (string.Compare(name, "VersionSafe", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.versionSafe;
            }
            if (string.Compare(name, "warnaserror", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.doWarnAsError;
            }
            if (string.Compare(name, "WarningLevel", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.nWarningLevel;
            }
            if (string.Compare(name, "win32resource", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.win32resource;
            }
            if (string.Compare(name, "managedResources", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.managedResources;
            }
            if (string.Compare(name, "alwaysGenerateIL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.alwaysGenerateIL;
            }
            if (string.Compare(name, "DebugDirectory", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this.debugDirectory;
            }
            if (string.Compare(name, "AutoRef", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new JSVsaException(JSVsaError.OptionNotSupported);
            }
            return this.autoRef;
        }

        public IVsaScriptScope GetGlobalScope()
        {
            if (this.globalScope == null)
            {
                this.globalScope = new VsaScriptScope(this, "Global", null);
                GlobalScope scope = (GlobalScope) this.globalScope.GetObject();
                scope.globalObject = this.Globals.globalObject;
                scope.fast = this.doFast;
                scope.isKnownAtCompileTime = this.doFast;
            }
            return this.globalScope;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public IJSVsaItem GetItem(string itemName)
        {
            return base.vsaItems[itemName];
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public IJSVsaItem GetItemAtIndex(int index)
        {
            return base.vsaItems[index];
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public int GetItemCount()
        {
            return base.vsaItems.Count;
        }

        public GlobalScope GetMainScope()
        {
            ScriptObject parent = this.ScriptObjectStackTop();
            while ((parent != null) && !(parent is GlobalScope))
            {
                parent = parent.GetParent();
            }
            return (GlobalScope) parent;
        }

        public Module GetModule()
        {
            if (this.PEFileName != null)
            {
                return this.GetAssembly().GetModules()[0];
            }
            return this.CompilerGlobals.module;
        }

        public ArrayConstructor GetOriginalArrayConstructor()
        {
            return this.Globals.globalObject.originalArray;
        }

        public ObjectConstructor GetOriginalObjectConstructor()
        {
            return this.Globals.globalObject.originalObject;
        }

        public RegExpConstructor GetOriginalRegExpConstructor()
        {
            return this.Globals.globalObject.originalRegExp;
        }

        private JSScanner GetScannerInstance(string name)
        {
            char[] anyOf = new char[] { 
                '\t', '\n', '\v', '\f', '\r', ' ', '\x00a0', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', 
                ' ', ' ', '​', '　', '﻿'
             };
            if ((name == null) || (name.IndexOfAny(anyOf) > -1))
            {
                return null;
            }
            VsaItem sourceItem = new VsaStaticCode(this, "itemName", JSVsaItemFlag.None);
            Context sourceContext = new Context(new DocumentContext(sourceItem), name) {
                errorReported = -1
            };
            JSScanner scanner = new JSScanner();
            scanner.SetSource(sourceContext);
            return scanner;
        }

        internal int GetStaticCodeBlockCount()
        {
            return ((VsaItems) base.vsaItems).staticCodeBlockCount;
        }

        internal Type GetType(string typeName)
        {
            if (this.cachedTypeLookups == null)
            {
                this.cachedTypeLookups = new SimpleHashtable(0x3e8);
            }
            object obj2 = this.cachedTypeLookups[typeName];
            if (obj2 != null)
            {
                return (obj2 as Type);
            }
            int num = 0;
            int count = this.Scopes.Count;
            while (num < count)
            {
                GlobalScope scope = (GlobalScope) this.scopes[num];
                Type type = Microsoft.JScript.Globals.TypeRefs.ToReferenceContext(scope.GetType()).Assembly.GetType(typeName, false);
                if (type != null)
                {
                    this.cachedTypeLookups[typeName] = type;
                    return type;
                }
                num++;
            }
            if (this.runtimeAssembly != null)
            {
                this.AddReferences();
                this.runtimeAssembly = null;
            }
            int num3 = 0;
            int num4 = base.vsaItems.Count;
            while (num3 < num4)
            {
                object obj3 = base.vsaItems[num3];
                if (obj3 is VsaReference)
                {
                    Type type2 = ((VsaReference) obj3).GetType(typeName);
                    if (type2 != null)
                    {
                        this.cachedTypeLookups[typeName] = type2;
                        return type2;
                    }
                }
                num3++;
            }
            if (this.implicitAssemblies == null)
            {
                this.cachedTypeLookups[typeName] = false;
                return null;
            }
            int num5 = 0;
            int num6 = this.implicitAssemblies.Count;
            while (num5 < num6)
            {
                Type target = ((Assembly) this.implicitAssemblies[num5]).GetType(typeName, false);
                if (target != null)
                {
                    if (target.IsPublic && !Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(RequiredAttributeAttribute), true))
                    {
                        this.cachedTypeLookups[typeName] = target;
                        return target;
                    }
                    target = null;
                }
                num5++;
            }
            this.cachedTypeLookups[typeName] = false;
            return null;
        }

        private static string GetVersionString()
        {
            object[] objArray = new object[7];
            objArray[0] = 10;
            objArray[1] = ".";
            int num = 0;
            objArray[2] = num.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
            objArray[3] = ".";
            objArray[4] = 0.ToString(CultureInfo.InvariantCulture);
            objArray[5] = ".";
            int num3 = 0x766f;
            objArray[6] = num3.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0');
            return string.Concat(objArray);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void InitVsaEngine(string rootMoniker, IJSVsaSite site)
        {
            this.genStartupClass = false;
            base.engineMoniker = rootMoniker;
            base.engineSite = site;
            base.isEngineInitialized = true;
            base.rootNamespace = "JScript.DefaultNamespace";
            base.isEngineDirty = true;
            base.isEngineCompiled = false;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void Interrupt()
        {
            if (this.runningThread != null)
            {
                this.runningThread.Abort();
                this.runningThread = null;
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public override bool IsValidIdentifier(string ident)
        {
            JSScanner scannerInstance = this.GetScannerInstance(ident);
            if (scannerInstance == null)
            {
                return false;
            }
            if (scannerInstance.PeekToken() != JSToken.Identifier)
            {
                return false;
            }
            scannerInstance.GetNextToken();
            if (scannerInstance.PeekToken() != JSToken.EndOfFile)
            {
                return false;
            }
            return true;
        }

        protected override bool IsValidNamespaceName(string name)
        {
            JSScanner scannerInstance = this.GetScannerInstance(name);
            if (scannerInstance != null)
            {
                while (scannerInstance.PeekToken() == JSToken.Identifier)
                {
                    scannerInstance.GetNextToken();
                    if (scannerInstance.PeekToken() == JSToken.EndOfFile)
                    {
                        return true;
                    }
                    if (scannerInstance.PeekToken() != JSToken.AccessField)
                    {
                        return false;
                    }
                    scannerInstance.GetNextToken();
                }
            }
            return false;
        }

        protected override Assembly LoadCompiledState()
        {
            byte[] buffer;
            byte[] buffer2;
            if (!base.genDebugInfo)
            {
                Evidence compilationEvidence = this.CompilerGlobals.compilationEvidence;
                Evidence executionEvidence = base.executionEvidence;
                if (((compilationEvidence == null) && (executionEvidence == null)) || ((compilationEvidence != null) && compilationEvidence.Equals(executionEvidence)))
                {
                    return this.compilerGlobals.assemblyBuilder;
                }
            }
            this.DoSaveCompiledState(out buffer, out buffer2);
            return Assembly.Load(buffer, buffer2, base.executionEvidence);
        }

        private void LoadCustomOptions(XmlElement parent)
        {
            XmlElement element = parent["Options"];
            this.doFast = bool.Parse(element.GetAttribute("fast"));
            this.doPrint = bool.Parse(element.GetAttribute("print"));
            this.doCRS = bool.Parse(element.GetAttribute("UseContextRelativeStatics"));
            this.versionSafe = bool.Parse(element.GetAttribute("VersionSafe"));
            this.libpath = element.GetAttribute("libpath");
            this.doWarnAsError = bool.Parse(element.GetAttribute("warnaserror"));
            this.nWarningLevel = int.Parse(element.GetAttribute("WarningLevel"), CultureInfo.InvariantCulture);
            if (element.HasAttribute("win32resource"))
            {
                this.win32resource = element.GetAttribute("win32resource");
            }
            this.LoadUserDefines(element);
            this.LoadManagedResources(element);
        }

        private void LoadManagedResources(XmlElement parent)
        {
            XmlElement element = parent["ManagedResources"];
            XmlNodeList childNodes = element.ChildNodes;
            if (childNodes.Count > 0)
            {
                this.managedResources = new ArrayList(childNodes.Count);
                foreach (XmlElement element2 in childNodes)
                {
                    string attribute = element2.GetAttribute("Name");
                    string filename = element2.GetAttribute("FileName");
                    bool isPublic = bool.Parse(element2.GetAttribute("Public"));
                    bool isLinked = bool.Parse(element2.GetAttribute("Linked"));
                    ((ArrayList) this.managedResources).Add(new ResInfo(filename, attribute, isPublic, isLinked));
                }
            }
        }

        private Version LoadProjectVersion(XmlElement root)
        {
            return new Version(root["ProjectVersion"].GetAttribute("Version"));
        }

        private void LoadUserDefines(XmlElement parent)
        {
            XmlElement element = parent["Defines"];
            foreach (XmlElement element2 in element.ChildNodes)
            {
                this.Defines[element2.Name] = element2.GetAttribute("Value");
            }
        }

        private void LoadVsaEngineState(XmlElement parent)
        {
            XmlElement element = parent["VsaEngine"];
            base.applicationPath = element.GetAttribute("ApplicationBase");
            base.genDebugInfo = bool.Parse(element.GetAttribute("GenerateDebugInfo"));
            base.scriptLanguage = element.GetAttribute("Language");
            base.LCID = int.Parse(element.GetAttribute("LCID"), CultureInfo.InvariantCulture);
            base.Name = element.GetAttribute("Name");
            base.rootNamespace = element.GetAttribute("RootNamespace");
            base.assemblyVersion = element.GetAttribute("Version");
            this.LoadCustomOptions(element);
            this.LoadVsaItems(element);
        }

        private void LoadVsaItems(XmlElement parent)
        {
            XmlNodeList childNodes = parent["VsaItems"].ChildNodes;
            string strB = JSVsaItemType.Reference.ToString();
            string str3 = JSVsaItemType.AppGlobal.ToString();
            string str4 = JSVsaItemType.Code.ToString();
            foreach (XmlElement element in childNodes)
            {
                IJSVsaItem item;
                string attribute = element.GetAttribute("Name");
                string strA = element.GetAttribute("ItemType");
                if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    item = base.vsaItems.CreateItem(attribute, JSVsaItemType.Reference, JSVsaItemFlag.None);
                    ((IJSVsaReferenceItem) item).AssemblyName = element.GetAttribute("AssemblyName");
                }
                else if (string.Compare(strA, str3, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    item = base.vsaItems.CreateItem(attribute, JSVsaItemType.AppGlobal, JSVsaItemFlag.None);
                    ((IJSVsaGlobalItem) item).ExposeMembers = bool.Parse(element.GetAttribute("ExposeMembers"));
                    ((IJSVsaGlobalItem) item).TypeString = element.GetAttribute("TypeString");
                }
                else
                {
                    if (string.Compare(strA, str4, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        throw new JSVsaException(JSVsaError.LoadElementFailed);
                    }
                    item = base.vsaItems.CreateItem(attribute, JSVsaItemType.Code, JSVsaItemFlag.None);
                    XmlCDataSection firstChild = (XmlCDataSection) element.FirstChild;
                    string str6 = firstChild.Value.Replace(" >", ">");
                    ((IJSVsaCodeItem) item).SourceText = str6;
                }
                foreach (XmlElement element2 in element["Options"].ChildNodes)
                {
                    item.SetOption(element2.Name, element2.GetAttribute("Value"));
                }
                ((VsaItem) item).IsDirty = false;
            }
        }

        internal bool OnCompilerError(JScriptException se)
        {
            if ((se.Severity == 0) || (this.doWarnAsError && (se.Severity <= this.nWarningLevel)))
            {
                this.numberOfErrors++;
            }
            bool flag = base.engineSite.OnCompilerError(se);
            if (!flag)
            {
                base.isEngineCompiled = false;
            }
            return flag;
        }

        public ScriptObject PopScriptObject()
        {
            return (ScriptObject) this.Globals.ScopeStack.Pop();
        }

        public void PushScriptObject(ScriptObject obj)
        {
            this.Globals.ScopeStack.Push(obj);
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void RegisterEventSource(string name)
        {
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public override void Reset()
        {
            if (this.genStartupClass)
            {
                base.Reset();
            }
            else
            {
                this.ResetCompiledState();
            }
        }

        protected override void ResetCompiledState()
        {
            if (this.globalScope != null)
            {
                this.globalScope.Reset();
                this.globalScope = null;
            }
            this.classCounter = 0;
            base.haveCompiledState = false;
            base.failedCompilation = true;
            base.compiledRootNamespace = null;
            base.startupClass = null;
            this.compilerGlobals = null;
            this.globals = null;
            foreach (object obj2 in base.vsaItems)
            {
                ((VsaItem) obj2).Reset();
            }
            this.implicitAssemblies = null;
            this.implicitAssemblyCache = null;
            this.cachedTypeLookups = null;
            base.isEngineCompiled = false;
            base.isEngineRunning = false;
            this.isCompilerSet = false;
            this.packages = null;
            if (!this.doSaveAfterCompile)
            {
                this.PEFileName = null;
            }
            this.rawPE = null;
            this.rawPDB = null;
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void Restart()
        {
            base.TryObtainLock();
            try
            {
                ((VsaItems) base.vsaItems).Close();
                if (this.globalScope != null)
                {
                    this.globalScope.Close();
                }
                this.globalScope = null;
                base.vsaItems = new VsaItems(this);
                base.isEngineRunning = false;
                base.isEngineCompiled = false;
                this.isCompilerSet = false;
                base.isClosed = false;
                this.runningThread = null;
                this.globals = null;
            }
            finally
            {
                base.ReleaseLock();
            }
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void Run(AppDomain domain)
        {
            throw new NotImplementedException();
        }

        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public void RunEmpty()
        {
            base.TryObtainLock();
            try
            {
                base.Preconditions(BaseVsaEngine.Pre.SiteSet | BaseVsaEngine.Pre.RootMonikerSet | BaseVsaEngine.Pre.EngineNotClosed);
                base.isEngineRunning = true;
                this.runningThread = Thread.CurrentThread;
                if (this.globalScope != null)
                {
                    this.globalScope.Run();
                }
                foreach (object obj2 in base.vsaItems)
                {
                    ((VsaItem) obj2).Run();
                }
            }
            finally
            {
                this.runningThread = null;
                base.ReleaseLock();
            }
        }

        private void SaveCustomOptions(XmlDocument project, XmlElement parent)
        {
            XmlElement elem = project.CreateElement("Options");
            this.CreateAttribute(project, elem, "fast", this.doFast.ToString());
            this.CreateAttribute(project, elem, "print", this.doPrint.ToString());
            this.CreateAttribute(project, elem, "UseContextRelativeStatics", this.doCRS.ToString());
            this.CreateAttribute(project, elem, "VersionSafe", this.versionSafe.ToString());
            this.CreateAttribute(project, elem, "libpath", this.libpath);
            this.CreateAttribute(project, elem, "warnaserror", this.doWarnAsError.ToString());
            this.CreateAttribute(project, elem, "WarningLevel", this.nWarningLevel.ToString(CultureInfo.InvariantCulture));
            if (this.win32resource != null)
            {
                this.CreateAttribute(project, elem, "win32resource", this.win32resource);
            }
            this.SaveUserDefines(project, elem);
            this.SaveManagedResources(project, elem);
            parent.AppendChild(elem);
        }

        private void SaveManagedResources(XmlDocument project, XmlElement parent)
        {
            XmlElement newChild = project.CreateElement("ManagedResources");
            if (this.managedResources != null)
            {
                foreach (ResInfo info in this.managedResources)
                {
                    XmlElement elem = project.CreateElement(info.name);
                    this.CreateAttribute(project, elem, "File", info.filename);
                    this.CreateAttribute(project, elem, "Public", info.isPublic.ToString());
                    this.CreateAttribute(project, elem, "Linked", info.isLinked.ToString());
                    newChild.AppendChild(elem);
                }
            }
            parent.AppendChild(newChild);
        }

        private void SaveProjectVersion(XmlDocument project, XmlElement root)
        {
            XmlElement elem = project.CreateElement("ProjectVersion");
            this.CreateAttribute(project, elem, "Version", CurrentProjectVersion.ToString());
            root.AppendChild(elem);
        }

        private void SaveSourceForDebugging()
        {
            if ((base.GenerateDebugInfo && (this.debugDirectory != null)) && base.isEngineDirty)
            {
                foreach (VsaItem item in base.vsaItems)
                {
                    if (item is VsaStaticCode)
                    {
                        string path = this.debugDirectory + item.Name + ".js";
                        try
                        {
                            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                            {
                                using (StreamWriter writer = new StreamWriter(stream))
                                {
                                    writer.Write(((VsaStaticCode) item).SourceText);
                                }
                                item.SetOption("codebase", path);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        private void SaveUserDefines(XmlDocument project, XmlElement parent)
        {
            XmlElement element = project.CreateElement("Defines");
            if (this.Defines != null)
            {
                foreach (string str in this.Defines.Keys)
                {
                    this.AddChildAndValue(project, element, str, (string) this.Defines[str]);
                }
            }
            parent.AppendChild(element);
        }

        private void SaveVsaEngineState(XmlDocument project, XmlElement parent)
        {
            XmlElement elem = project.CreateElement("VsaEngine");
            this.CreateAttribute(project, elem, "ApplicationBase", base.applicationPath);
            this.CreateAttribute(project, elem, "GenerateDebugInfo", this.genDebugInfo.ToString());
            this.CreateAttribute(project, elem, "Language", base.scriptLanguage);
            this.CreateAttribute(project, elem, "LCID", this.errorLocale.ToString(CultureInfo.InvariantCulture));
            this.CreateAttribute(project, elem, "Name", base.engineName);
            this.CreateAttribute(project, elem, "RootNamespace", base.rootNamespace);
            this.CreateAttribute(project, elem, "Version", base.assemblyVersion);
            this.SaveCustomOptions(project, elem);
            this.SaveVsaItems(project, elem);
            parent.AppendChild(elem);
        }

        private void SaveVsaItems(XmlDocument project, XmlElement parent)
        {
            XmlElement newChild = project.CreateElement("VsaItems");
            foreach (IJSVsaItem item in base.vsaItems)
            {
                XmlElement elem = project.CreateElement("IJSVsaItem");
                this.CreateAttribute(project, elem, "Name", item.Name);
                this.CreateAttribute(project, elem, "ItemType", item.ItemType.ToString(CultureInfo.InvariantCulture));
                XmlElement element3 = project.CreateElement("Options");
                if (item is VsaHostObject)
                {
                    this.CreateAttribute(project, elem, "TypeString", ((VsaHostObject) item).TypeString);
                    this.CreateAttribute(project, elem, "ExposeMembers", ((VsaHostObject) item).ExposeMembers.ToString(CultureInfo.InvariantCulture));
                }
                else if (item is VsaReference)
                {
                    this.CreateAttribute(project, elem, "AssemblyName", ((VsaReference) item).AssemblyName);
                }
                else
                {
                    if (!(item is VsaStaticCode))
                    {
                        throw new JSVsaException(JSVsaError.SaveElementFailed);
                    }
                    string data = ((VsaStaticCode) item).SourceText.Replace(">", " >");
                    XmlCDataSection section = project.CreateCDataSection(data);
                    elem.AppendChild(section);
                    string option = (string) item.GetOption("codebase");
                    if (option != null)
                    {
                        this.AddChildAndValue(project, element3, "codebase", option);
                    }
                }
                ((VsaItem) item).IsDirty = false;
                elem.AppendChild(element3);
                newChild.AppendChild(elem);
            }
            parent.AppendChild(newChild);
        }

        public ScriptObject ScriptObjectStackTop()
        {
            return this.Globals.ScopeStack.Peek();
        }

        protected override void SetCustomOption(string name, object value)
        {
            try
            {
                if (string.Compare(name, "CLSCompliant", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.isCLSCompliant = (bool) value;
                }
                else if (string.Compare(name, "fast", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.doFast = (bool) value;
                }
                else if (string.Compare(name, "output", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (value is string)
                    {
                        this.PEFileName = (string) value;
                        this.doSaveAfterCompile = true;
                    }
                }
                else if (string.Compare(name, "PEFileKind", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.PEFileKind = (PEFileKinds) value;
                }
                else if (string.Compare(name, "PortableExecutableKind", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.PEKindFlags = (PortableExecutableKinds) value;
                }
                else if (string.Compare(name, "ImageFileMachine", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.PEMachineArchitecture = (ImageFileMachine) value;
                }
                else if (string.Compare(name, "ReferenceLoaderAPI", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    string strA = (string) value;
                    if (string.Compare(strA, "LoadFrom", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        if (string.Compare(strA, "LoadFile", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (string.Compare(strA, "ReflectionOnlyLoadFrom", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                throw new JSVsaException(JSVsaError.OptionInvalid);
                            }
                            this.ReferenceLoaderAPI = LoaderAPI.ReflectionOnlyLoadFrom;
                        }
                        else
                        {
                            this.ReferenceLoaderAPI = LoaderAPI.LoadFile;
                        }
                    }
                    else
                    {
                        this.ReferenceLoaderAPI = LoaderAPI.LoadFrom;
                    }
                }
                else if (string.Compare(name, "print", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.doPrint = (bool) value;
                }
                else if (string.Compare(name, "UseContextRelativeStatics", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.doCRS = (bool) value;
                }
                else if ((string.Compare(name, "optimize", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(name, "define", StringComparison.OrdinalIgnoreCase) != 0))
                {
                    if (string.Compare(name, "defines", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.Defines = (Hashtable) value;
                    }
                    else if (string.Compare(name, "ee", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        executeForJSEE = (bool) value;
                    }
                    else if (string.Compare(name, "version", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.versionInfo = (Version) value;
                    }
                    else if (string.Compare(name, "VersionSafe", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.versionSafe = (bool) value;
                    }
                    else if (string.Compare(name, "libpath", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.libpath = (string) value;
                    }
                    else if (string.Compare(name, "warnaserror", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.doWarnAsError = (bool) value;
                    }
                    else if (string.Compare(name, "WarningLevel", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.nWarningLevel = (int) value;
                    }
                    else if (string.Compare(name, "win32resource", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.win32resource = (string) value;
                    }
                    else if (string.Compare(name, "managedResources", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.managedResources = (ICollection) value;
                    }
                    else if (string.Compare(name, "alwaysGenerateIL", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.alwaysGenerateIL = (bool) value;
                    }
                    else if (string.Compare(name, "DebugDirectory", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (value == null)
                        {
                            this.debugDirectory = null;
                        }
                        else
                        {
                            string path = value as string;
                            if (path == null)
                            {
                                throw new JSVsaException(JSVsaError.OptionInvalid);
                            }
                            try
                            {
                                path = Path.GetFullPath(path + Path.DirectorySeparatorChar);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }
                            }
                            catch (Exception exception)
                            {
                                throw new JSVsaException(JSVsaError.OptionInvalid, "", exception);
                            }
                            this.debugDirectory = path;
                        }
                    }
                    else
                    {
                        if (string.Compare(name, "AutoRef", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new JSVsaException(JSVsaError.OptionNotSupported);
                        }
                        this.autoRef = (bool) value;
                    }
                }
            }
            catch (JSVsaException)
            {
                throw;
            }
            catch
            {
                throw new JSVsaException(JSVsaError.OptionInvalid);
            }
        }

        internal void SetEnclosingContext(ScriptObject ob)
        {
            ScriptObject parent = this.Globals.ScopeStack.Peek();
            while (parent.GetParent() != null)
            {
                parent = parent.GetParent();
            }
            parent.SetParent(ob);
        }

        public void SetOutputStream(IMessageReceiver output)
        {
            COMCharStream stream = new COMCharStream(output);
            StreamWriter writer = new StreamWriter(stream, Encoding.Default) {
                AutoFlush = true
            };
            ScriptStream.Out = writer;
            ScriptStream.Error = writer;
        }

        internal void SetUpCompilerEnvironment()
        {
            if (!this.isCompilerSet)
            {
                Microsoft.JScript.Globals.TypeRefs = this.TypeRefs;
                this.globals = this.Globals;
                this.isCompilerSet = true;
            }
        }

        internal void TryToAddImplicitAssemblyReference(string name)
        {
            if (this.autoRef)
            {
                string str;
                if (this.implicitAssemblyCache == null)
                {
                    this.implicitAssemblyCache = new SimpleHashtable(50);
                    this.implicitAssemblyCache[Path.GetFileNameWithoutExtension(this.PEFileName).ToLowerInvariant()] = true;
                    foreach (object obj2 in base.vsaItems)
                    {
                        VsaReference reference = obj2 as VsaReference;
                        if ((reference != null) && (reference.AssemblyName != null))
                        {
                            str = Path.GetFileName(reference.AssemblyName).ToLowerInvariant();
                            if (str.EndsWith(".dll", StringComparison.Ordinal))
                            {
                                str = str.Substring(0, str.Length - 4);
                            }
                            this.implicitAssemblyCache[str] = true;
                        }
                    }
                    this.implicitAssemblyCache = this.implicitAssemblyCache;
                }
                str = name.ToLowerInvariant();
                if (this.implicitAssemblyCache[str] == null)
                {
                    this.implicitAssemblyCache[str] = true;
                    try
                    {
                        VsaReference reference2 = new VsaReference(this, name + ".dll");
                        if (reference2.Compile(false))
                        {
                            ArrayList implicitAssemblies = this.implicitAssemblies;
                            if (implicitAssemblies == null)
                            {
                                implicitAssemblies = new ArrayList();
                                this.implicitAssemblies = implicitAssemblies;
                            }
                            implicitAssemblies.Add(reference2.Assembly);
                        }
                    }
                    catch (JSVsaException)
                    {
                    }
                }
            }
        }

        protected override void ValidateRootMoniker(string rootMoniker)
        {
            if (this.genStartupClass)
            {
                base.ValidateRootMoniker(rootMoniker);
            }
            else if ((rootMoniker == null) || (rootMoniker.Length == 0))
            {
                throw new JSVsaException(JSVsaError.RootMonikerInvalid);
            }
        }

        internal Microsoft.JScript.CompilerGlobals CompilerGlobals
        {
            get
            {
                if (this.compilerGlobals == null)
                {
                    this.compilerGlobals = new Microsoft.JScript.CompilerGlobals(this, base.Name, this.PEFileName, this.PEFileKind, this.doSaveAfterCompile || this.genStartupClass, !this.doSaveAfterCompile || this.genStartupClass, base.genDebugInfo, this.isCLSCompliant, this.versionInfo, this.globals);
                }
                return this.compilerGlobals;
            }
        }

        internal CultureInfo ErrorCultureInfo
        {
            get
            {
                if ((this.errorCultureInfo == null) || (this.errorCultureInfo.LCID != base.errorLocale))
                {
                    this.errorCultureInfo = new CultureInfo(base.errorLocale);
                }
                return this.errorCultureInfo;
            }
        }

        internal Microsoft.JScript.Globals Globals
        {
            get
            {
                if (this.globals == null)
                {
                    this.globals = new Microsoft.JScript.Globals(this.doFast, this);
                }
                return this.globals;
            }
        }

        internal bool HasErrors
        {
            get
            {
                return (this.numberOfErrors != 0);
            }
        }

        internal Module JScriptModule
        {
            get
            {
                if (this.ReferenceLoaderAPI != LoaderAPI.ReflectionOnlyLoadFrom)
                {
                    return typeof(VsaEngine).Module;
                }
                this.EnsureReflectionOnlyModulesLoaded();
                return reflectionOnlyJScriptModule;
            }
        }

        public Microsoft.JScript.LenientGlobalObject LenientGlobalObject
        {
            get
            {
                return (Microsoft.JScript.LenientGlobalObject) this.Globals.globalObject;
            }
        }

        internal string[] LibpathList
        {
            get
            {
                if (this.libpathList == null)
                {
                    if (this.libpath == null)
                    {
                        this.libpathList = new string[] { typeof(object).Module.Assembly.Location };
                    }
                    else
                    {
                        this.libpathList = this.libpath.Split(new char[] { Path.PathSeparator });
                    }
                }
                return this.libpathList;
            }
        }

        internal string RuntimeDirectory
        {
            get
            {
                if (this.runtimeDirectory == null)
                {
                    string fullyQualifiedName = typeof(object).Module.FullyQualifiedName;
                    this.runtimeDirectory = Path.GetDirectoryName(fullyQualifiedName);
                }
                return this.runtimeDirectory;
            }
        }

        internal ArrayList Scopes
        {
            get
            {
                if (this.scopes == null)
                {
                    this.scopes = new ArrayList(8);
                }
                return this.scopes;
            }
        }

        private TypeReferences TypeRefs
        {
            get
            {
                if (LoaderAPI.ReflectionOnlyLoadFrom == this.ReferenceLoaderAPI)
                {
                    TypeReferences references = _reflectionOnlyTypeRefs;
                    if (references == null)
                    {
                        references = _reflectionOnlyTypeRefs = new TypeReferences(this.JScriptModule);
                    }
                    return references;
                }
                return Runtime.TypeRefs;
            }
        }

        internal Module VsaModule
        {
            get
            {
                if (this.ReferenceLoaderAPI != LoaderAPI.ReflectionOnlyLoadFrom)
                {
                    return typeof(IJSVsaEngine).Module;
                }
                this.EnsureReflectionOnlyModulesLoaded();
                return reflectionOnlyVsaModule;
            }
        }
    }
}

