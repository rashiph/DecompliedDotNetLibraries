namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Windows.Forms;

    public class AxImporter
    {
        private Hashtable copiedAssems;
        private ArrayList genAssems;
        private ArrayList generatedSources;
        internal Options options;
        private Hashtable rcwCache;
        private ArrayList refAssems;
        private ArrayList tlbAttrs;
        internal string typeLibName;

        public AxImporter(Options options)
        {
            this.options = options;
        }

        private void AddDependentAssemblies(Assembly assem, string assemPath)
        {
            foreach (AssemblyName name in assem.GetReferencedAssemblies())
            {
                if (!string.Equals(name.Name, "mscorlib", StringComparison.OrdinalIgnoreCase))
                {
                    string comReference = this.GetComReference(name);
                    if (comReference == null)
                    {
                        Assembly assembly = null;
                        try
                        {
                            assembly = Assembly.ReflectionOnlyLoad(name.FullName);
                        }
                        catch (FileNotFoundException)
                        {
                            if (name.CodeBase != null)
                            {
                                throw;
                            }
                            assembly = Assembly.ReflectionOnlyLoad(AssemblyName.GetAssemblyName(Path.Combine(Path.GetDirectoryName(assemPath), name.Name + ".dll")).FullName);
                        }
                        comReference = assembly.EscapedCodeBase;
                        if (comReference != null)
                        {
                            comReference = this.GetLocalPath(comReference);
                        }
                    }
                    this.AddReferencedAssembly(comReference);
                }
            }
        }

        private void AddGeneratedAssembly(string assem)
        {
            if (this.genAssems == null)
            {
                this.genAssems = new ArrayList();
            }
            this.genAssems.Add(assem);
        }

        internal void AddRCW(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib, Assembly assem)
        {
            if (this.rcwCache == null)
            {
                this.rcwCache = new Hashtable();
            }
            IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
            typeLib.GetLibAttr(out invalidIntPtr);
            try
            {
                if (invalidIntPtr != System.Design.NativeMethods.InvalidIntPtr)
                {
                    System.Runtime.InteropServices.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.TYPELIBATTR));
                    this.rcwCache.Add(typelibattr.guid, assem);
                }
            }
            finally
            {
                typeLib.ReleaseTLibAttr(invalidIntPtr);
            }
        }

        private void AddReferencedAssembly(string assem)
        {
            if (this.refAssems == null)
            {
                this.refAssems = new ArrayList();
            }
            this.refAssems.Add(assem);
        }

        private void AddTypeLibAttr(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib)
        {
            if (this.tlbAttrs == null)
            {
                this.tlbAttrs = new ArrayList();
            }
            IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
            typeLib.GetLibAttr(out invalidIntPtr);
            if (invalidIntPtr != System.Design.NativeMethods.InvalidIntPtr)
            {
                System.Runtime.InteropServices.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.TYPELIBATTR));
                this.tlbAttrs.Add(typelibattr);
                typeLib.ReleaseTLibAttr(invalidIntPtr);
            }
        }

        internal Assembly FindRCW(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib)
        {
            if (this.rcwCache != null)
            {
                IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
                typeLib.GetLibAttr(out invalidIntPtr);
                try
                {
                    if (invalidIntPtr != System.Design.NativeMethods.InvalidIntPtr)
                    {
                        System.Runtime.InteropServices.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.TYPELIBATTR));
                        return (Assembly) this.rcwCache[typelibattr.guid];
                    }
                }
                finally
                {
                    typeLib.ReleaseTLibAttr(invalidIntPtr);
                }
            }
            return null;
        }

        internal string GenerateFromActiveXClsid(Guid clsid)
        {
            string name = @"CLSID\{" + clsid.ToString() + "}";
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(name);
            if (key == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("AXNotRegistered", new object[] { name.ToString() }));
            }
            System.Runtime.InteropServices.ComTypes.ITypeLib o = null;
            Guid empty = Guid.Empty;
            RegistryKey key2 = key.OpenSubKey("TypeLib");
            if (key2 != null)
            {
                RegistryKey key3 = key.OpenSubKey("Version");
                short majorVersion = -1;
                short minorVersion = -1;
                string s = (string) key3.GetValue("");
                int index = s.IndexOf('.');
                if (index == -1)
                {
                    majorVersion = short.Parse(s, CultureInfo.InvariantCulture);
                    minorVersion = 0;
                }
                else
                {
                    majorVersion = short.Parse(s.Substring(0, index), CultureInfo.InvariantCulture);
                    minorVersion = short.Parse(s.Substring(index + 1, (s.Length - index) - 1), CultureInfo.InvariantCulture);
                }
                key3.Close();
                object obj2 = key2.GetValue("");
                empty = new Guid((string) obj2);
                key2.Close();
                try
                {
                    o = System.Design.NativeMethods.LoadRegTypeLib(ref empty, majorVersion, minorVersion, Application.CurrentCulture.LCID);
                }
                catch (Exception)
                {
                }
            }
            if (o == null)
            {
                RegistryKey key4 = key.OpenSubKey("InprocServer32");
                if (key4 != null)
                {
                    string typelib = (string) key4.GetValue("");
                    key4.Close();
                    o = System.Design.NativeMethods.LoadTypeLib(typelib);
                }
            }
            key.Close();
            if (o != null)
            {
                try
                {
                    return this.GenerateFromTypeLibrary((UCOMITypeLib) o, clsid);
                }
                finally
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            throw new ArgumentException(System.Design.SR.GetString("AXNotRegistered", new object[] { name.ToString() }));
        }

        public string GenerateFromFile(FileInfo file)
        {
            string str;
            this.typeLibName = file.FullName;
            System.Runtime.InteropServices.ComTypes.ITypeLib o = null;
            o = System.Design.NativeMethods.LoadTypeLib(this.typeLibName);
            if (o == null)
            {
                throw new Exception(System.Design.SR.GetString("AXCannotLoadTypeLib", new object[] { this.typeLibName }));
            }
            try
            {
                str = this.GenerateFromTypeLibrary((UCOMITypeLib) o);
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return str;
        }

        public string GenerateFromTypeLibrary(UCOMITypeLib typeLib)
        {
            bool flag = false;
            int typeInfoCount = ((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib).GetTypeInfoCount();
            for (int i = 0; i < typeInfoCount; i++)
            {
                IntPtr zero;
                System.Runtime.InteropServices.ComTypes.ITypeInfo info;
                ((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib).GetTypeInfo(i, out info);
                info.GetTypeAttr(out zero);
                System.Runtime.InteropServices.ComTypes.TYPEATTR typeattr = (System.Runtime.InteropServices.ComTypes.TYPEATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPEATTR));
                if (typeattr.typekind == System.Runtime.InteropServices.ComTypes.TYPEKIND.TKIND_COCLASS)
                {
                    string name = @"CLSID\{" + typeattr.guid.ToString() + @"}\Control";
                    if (Registry.ClassesRoot.OpenSubKey(name) != null)
                    {
                        flag = true;
                    }
                }
                info.ReleaseTypeAttr(zero);
                zero = IntPtr.Zero;
                Marshal.ReleaseComObject(info);
                info = null;
            }
            if (flag)
            {
                return this.GenerateFromTypeLibrary(typeLib, Guid.Empty);
            }
            string message = System.Design.SR.GetString("AXNoActiveXControls", new object[] { (this.typeLibName != null) ? this.typeLibName : Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib) });
            if (this.options.msBuildErrors)
            {
                message = "AxImp: error aximp000: " + message;
            }
            throw new Exception(message);
        }

        public string GenerateFromTypeLibrary(UCOMITypeLib typeLib, Guid clsid)
        {
            string fileName = null;
            string axTypeFromAssembly = null;
            Assembly assem = null;
            fileName = this.GetAxReference((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
            if ((fileName != null) && (clsid != Guid.Empty))
            {
                axTypeFromAssembly = this.GetAxTypeFromAssembly(fileName, clsid);
            }
            if (fileName == null)
            {
                string typeLibName = Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                string asmFileName = Path.Combine(this.options.outputDirectory, typeLibName + ".dll");
                this.AddReferencedAssembly(this.GetManagedReference("System.Windows.Forms"));
                this.AddReferencedAssembly(this.GetManagedReference("System.Drawing"));
                this.AddReferencedAssembly(this.GetManagedReference("System"));
                string comReference = this.GetComReference((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                if (comReference != null)
                {
                    this.AddReferencedAssembly(comReference);
                    assem = this.GetCopiedAssembly(comReference, false, false);
                    this.AddDependentAssemblies(assem, comReference);
                }
                else
                {
                    TypeLibConverter tlbConverter = new TypeLibConverter();
                    assem = this.GetPrimaryInteropAssembly((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, tlbConverter);
                    if (assem != null)
                    {
                        comReference = this.GetLocalPath(assem.EscapedCodeBase);
                        this.AddDependentAssemblies(assem, comReference);
                    }
                    else
                    {
                        AssemblyBuilder asmBldr = tlbConverter.ConvertTypeLibToAssembly((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, asmFileName, TypeLibImporterFlags.None, new ImporterCallback(this), this.options.publicKey, this.options.keyPair, null, null);
                        if (comReference == null)
                        {
                            comReference = this.SaveAssemblyBuilder((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, asmBldr, asmFileName);
                            assem = asmBldr;
                        }
                    }
                }
                int num = 0;
                string[] refAssemblies = new string[this.refAssems.Count];
                foreach (string str6 in this.refAssems)
                {
                    string str7 = str6;
                    str7 = str7.Replace("%20", " ");
                    refAssemblies[num++] = str7;
                }
                if (axTypeFromAssembly == null)
                {
                    string fileOfTypeLib = GetFileOfTypeLib((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                    DateTime tlbTimeStamp = (fileOfTypeLib == null) ? DateTime.Now : File.GetLastWriteTime(fileOfTypeLib);
                    ResolveEventHandler handler = new ResolveEventHandler(this.OnAssemblyResolve);
                    AppDomain.CurrentDomain.AssemblyResolve += handler;
                    AppDomain.CurrentDomain.TypeResolve += new ResolveEventHandler(this.OnTypeResolve);
                    try
                    {
                        if (this.options.genSources)
                        {
                            AxWrapperGen.GeneratedSources = new ArrayList();
                        }
                        if (this.options.outputName == null)
                        {
                            this.options.outputName = "Ax" + typeLibName + ".dll";
                        }
                        axTypeFromAssembly = AxWrapperGen.GenerateWrappers(this, clsid, assem, refAssemblies, tlbTimeStamp, out fileName);
                        if (this.options.genSources)
                        {
                            this.generatedSources = AxWrapperGen.GeneratedSources;
                        }
                    }
                    finally
                    {
                        AppDomain.CurrentDomain.AssemblyResolve -= handler;
                        AppDomain.CurrentDomain.TypeResolve -= new ResolveEventHandler(this.OnTypeResolve);
                    }
                    if (axTypeFromAssembly == null)
                    {
                        string message = System.Design.SR.GetString("AXNoActiveXControls", new object[] { (this.typeLibName != null) ? this.typeLibName : typeLibName });
                        if (this.options.msBuildErrors)
                        {
                            message = "AxImp: error aximp000: " + message;
                        }
                        throw new Exception(message);
                    }
                }
                if (axTypeFromAssembly != null)
                {
                    this.AddReferencedAssembly(fileName);
                    this.AddTypeLibAttr((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                    this.AddGeneratedAssembly(fileName);
                }
            }
            return axTypeFromAssembly;
        }

        private string GetAxReference(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib)
        {
            if (this.options.references == null)
            {
                return null;
            }
            return this.options.references.ResolveActiveXReference((UCOMITypeLib) typeLib);
        }

        private string GetAxTypeFromAssembly(string fileName, Guid clsid)
        {
            foreach (System.Type type in this.GetCopiedAssembly(fileName, true, false).GetTypes())
            {
                if (typeof(AxHost).IsAssignableFrom(type))
                {
                    CustomAttributeTypedArgument argument = AxWrapperGen.GetAttributeData(type, typeof(AxHost.ClsidAttribute))[0].ConstructorArguments[0];
                    if (argument.Value.ToString() == ("{" + clsid.ToString() + "}"))
                    {
                        return type.FullName;
                    }
                }
            }
            return null;
        }

        private string GetComReference(AssemblyName name)
        {
            if (this.options.references == null)
            {
                return name.EscapedCodeBase;
            }
            return this.options.references.ResolveComReference(name);
        }

        private string GetComReference(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib)
        {
            if (this.options.references == null)
            {
                return null;
            }
            return this.options.references.ResolveComReference((UCOMITypeLib) typeLib);
        }

        private Assembly GetCopiedAssembly(string fileName, bool loadPdb, bool isPIA)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            Assembly loadedAssembly = null;
            string key = fileName.ToUpper(CultureInfo.InvariantCulture);
            if (this.copiedAssems == null)
            {
                this.copiedAssems = new Hashtable();
            }
            else if (this.copiedAssems.Contains(key))
            {
                return (Assembly) this.copiedAssems[key];
            }
            if (!isPIA)
            {
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int length = (int) stream.Length;
                byte[] buffer = new byte[length];
                stream.Read(buffer, 0, length);
                stream.Close();
                byte[] buffer2 = null;
                if (loadPdb)
                {
                    string path = Path.ChangeExtension(fileName, "pdb");
                    if (File.Exists(path))
                    {
                        stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        length = (int) stream.Length;
                        buffer2 = new byte[length];
                        stream.Read(buffer2, 0, length);
                        stream.Close();
                    }
                }
                loadedAssembly = this.GetLoadedAssembly(fileName, true);
                if (loadedAssembly == null)
                {
                    loadedAssembly = Assembly.ReflectionOnlyLoad(buffer);
                }
            }
            else
            {
                loadedAssembly = Assembly.ReflectionOnlyLoadFrom(fileName);
            }
            this.copiedAssems.Add(key, loadedAssembly);
            return loadedAssembly;
        }

        private static string GetFileOfTypeLib(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib)
        {
            IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
            typeLib.GetLibAttr(out invalidIntPtr);
            if (invalidIntPtr != System.Design.NativeMethods.InvalidIntPtr)
            {
                System.Runtime.InteropServices.TYPELIBATTR tlibattr = (System.Runtime.InteropServices.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.TYPELIBATTR));
                try
                {
                    return GetFileOfTypeLib(ref tlibattr);
                }
                finally
                {
                    typeLib.ReleaseTLibAttr(invalidIntPtr);
                }
            }
            return null;
        }

        public static string GetFileOfTypeLib(ref System.Runtime.InteropServices.TYPELIBATTR tlibattr)
        {
            string path = null;
            path = System.Design.NativeMethods.QueryPathOfRegTypeLib(ref tlibattr.guid, tlibattr.wMajorVerNum, tlibattr.wMinorVerNum, tlibattr.lcid);
            if (path.Length <= 0)
            {
                return path;
            }
            int index = path.IndexOf('\0');
            if (index > -1)
            {
                path = path.Substring(0, index);
            }
            if (File.Exists(path))
            {
                return path;
            }
            int length = path.LastIndexOf(Path.DirectorySeparatorChar);
            if (length != -1)
            {
                bool flag = true;
                for (int i = length + 1; i < path.Length; i++)
                {
                    if ((path[i] != '\0') && !char.IsDigit(path[i]))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    path = path.Substring(0, length);
                    if (!File.Exists(path))
                    {
                        path = null;
                    }
                    return path;
                }
            }
            return null;
        }

        private Assembly GetLoadedAssembly(string filePath, bool reflectionOnly)
        {
            Assembly[] assemblyArray = reflectionOnly ? AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies() : AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblyArray)
            {
                if (assembly.Location.Equals(filePath, StringComparison.InvariantCultureIgnoreCase))
                {
                    return assembly;
                }
            }
            return null;
        }

        private string GetLocalPath(string fileName)
        {
            Uri uri = new Uri(fileName);
            return (uri.LocalPath + uri.Fragment);
        }

        private string GetManagedReference(string assemName)
        {
            if (this.options.references == null)
            {
                return (assemName + ".dll");
            }
            return this.options.references.ResolveManagedReference(assemName);
        }

        internal Assembly GetPrimaryInteropAssembly(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib, TypeLibConverter tlbConverter)
        {
            Assembly assem = this.FindRCW(typeLib);
            if (assem == null)
            {
                IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
                typeLib.GetLibAttr(out invalidIntPtr);
                if (!(invalidIntPtr != System.Design.NativeMethods.InvalidIntPtr))
                {
                    return assem;
                }
                System.Runtime.InteropServices.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.TYPELIBATTR));
                string asmName = null;
                string asmCodeBase = null;
                try
                {
                    tlbConverter.GetPrimaryInteropAssembly(typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, typelibattr.lcid, out asmName, out asmCodeBase);
                    if ((asmName != null) && (asmCodeBase == null))
                    {
                        try
                        {
                            assem = Assembly.ReflectionOnlyLoad(asmName);
                            asmCodeBase = this.GetLocalPath(assem.EscapedCodeBase);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else if (asmCodeBase != null)
                    {
                        asmCodeBase = this.GetLocalPath(asmCodeBase);
                        assem = Assembly.ReflectionOnlyLoadFrom(asmCodeBase);
                    }
                    if (assem != null)
                    {
                        this.AddRCW(typeLib, assem);
                        this.AddReferencedAssembly(asmCodeBase);
                    }
                }
                finally
                {
                    typeLib.ReleaseTLibAttr(invalidIntPtr);
                }
            }
            return assem;
        }

        private string GetReferencedAssembly(string assemName)
        {
            if ((this.refAssems != null) && (this.refAssems.Count > 0))
            {
                foreach (string str in this.refAssems)
                {
                    if (string.Equals(str, assemName, StringComparison.OrdinalIgnoreCase))
                    {
                        return str;
                    }
                }
            }
            return null;
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            string name = e.Name;
            if (this.rcwCache != null)
            {
                foreach (Assembly assembly in this.rcwCache.Values)
                {
                    if (assembly.FullName == name)
                    {
                        return assembly;
                    }
                }
            }
            Assembly assembly2 = null;
            if (this.copiedAssems == null)
            {
                this.copiedAssems = new Hashtable();
            }
            else
            {
                assembly2 = (Assembly) this.copiedAssems[name];
                if (assembly2 != null)
                {
                    return assembly2;
                }
            }
            if ((this.refAssems != null) && (this.refAssems.Count != 0))
            {
                foreach (string str2 in this.refAssems)
                {
                    Assembly assembly3 = this.GetCopiedAssembly(str2, false, false);
                    if ((assembly3 != null) && (assembly3.FullName == name))
                    {
                        return assembly3;
                    }
                }
            }
            return null;
        }

        private Assembly OnTypeResolve(object sender, ResolveEventArgs e)
        {
            try
            {
                string name = e.Name;
                if ((this.refAssems == null) || (this.refAssems.Count == 0))
                {
                    return null;
                }
                foreach (string str2 in this.refAssems)
                {
                    Assembly assembly = this.GetCopiedAssembly(str2, false, false);
                    if ((assembly != null) && (assembly.GetType(name, false) != null))
                    {
                        return assembly;
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private string SaveAssemblyBuilder(System.Runtime.InteropServices.ComTypes.ITypeLib typeLib, AssemblyBuilder asmBldr, string rcwName)
        {
            string assem = null;
            FileInfo info = new FileInfo(rcwName);
            string name = info.Name;
            if (info.Exists)
            {
                if (!this.options.overwriteRCW)
                {
                    goto Label_00D2;
                }
                if ((this.typeLibName != null) && string.Equals(this.typeLibName, info.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception(System.Design.SR.GetString("AXCannotOverwriteFile", new object[] { info.FullName }));
                }
                if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    throw new Exception(System.Design.SR.GetString("AXReadOnlyFile", new object[] { info.FullName }));
                }
                try
                {
                    info.Delete();
                    asmBldr.Save(name);
                    goto Label_00D2;
                }
                catch (Exception)
                {
                    throw new Exception(System.Design.SR.GetString("AXCannotOverwriteFile", new object[] { info.FullName }));
                }
            }
            asmBldr.Save(name);
        Label_00D2:
            assem = info.FullName;
            this.AddReferencedAssembly(assem);
            this.AddTypeLibAttr(typeLib);
            this.AddGeneratedAssembly(assem);
            return assem;
        }

        public string[] GeneratedAssemblies
        {
            get
            {
                if ((this.genAssems == null) || (this.genAssems.Count <= 0))
                {
                    return new string[0];
                }
                string[] strArray = new string[this.genAssems.Count];
                for (int i = 0; i < this.genAssems.Count; i++)
                {
                    strArray[i] = (string) this.genAssems[i];
                }
                return strArray;
            }
        }

        public string[] GeneratedSources
        {
            get
            {
                if (!this.options.genSources)
                {
                    return null;
                }
                string[] strArray = new string[this.generatedSources.Count];
                for (int i = 0; i < this.generatedSources.Count; i++)
                {
                    strArray[i] = (string) this.generatedSources[i];
                }
                return strArray;
            }
        }

        public System.Runtime.InteropServices.TYPELIBATTR[] GeneratedTypeLibAttributes
        {
            get
            {
                if (this.tlbAttrs == null)
                {
                    return new System.Runtime.InteropServices.TYPELIBATTR[0];
                }
                System.Runtime.InteropServices.TYPELIBATTR[] typelibattrArray = new System.Runtime.InteropServices.TYPELIBATTR[this.tlbAttrs.Count];
                for (int i = 0; i < this.tlbAttrs.Count; i++)
                {
                    typelibattrArray[i] = (System.Runtime.InteropServices.TYPELIBATTR) this.tlbAttrs[i];
                }
                return typelibattrArray;
            }
        }

        private class ImporterCallback : ITypeLibImporterNotifySink
        {
            private AxImporter importer;
            private AxImporter.Options options;

            public ImporterCallback(AxImporter importer)
            {
                this.importer = importer;
                this.options = importer.options;
            }

            void ITypeLibImporterNotifySink.ReportEvent(ImporterEventKind EventKind, int EventCode, string EventMsg)
            {
            }

            Assembly ITypeLibImporterNotifySink.ResolveRef(object typeLib)
            {
                Assembly assembly2;
                try
                {
                    string comReference = this.importer.GetComReference((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                    if (comReference != null)
                    {
                        this.importer.AddReferencedAssembly(comReference);
                    }
                    Assembly primaryInteropAssembly = this.importer.FindRCW((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib);
                    if (primaryInteropAssembly != null)
                    {
                        assembly2 = primaryInteropAssembly;
                    }
                    else
                    {
                        try
                        {
                            string assemName = Path.Combine(this.options.outputDirectory, Marshal.GetTypeLibName((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib) + ".dll");
                            if (this.importer.GetReferencedAssembly(assemName) != null)
                            {
                                return this.importer.GetCopiedAssembly(assemName, false, false);
                            }
                            TypeLibConverter tlbConverter = new TypeLibConverter();
                            primaryInteropAssembly = this.importer.GetPrimaryInteropAssembly((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, tlbConverter);
                            if (primaryInteropAssembly != null)
                            {
                                return primaryInteropAssembly;
                            }
                            AssemblyBuilder asmBldr = tlbConverter.ConvertTypeLibToAssembly(typeLib, assemName, TypeLibImporterFlags.None, new AxImporter.ImporterCallback(this.importer), this.options.publicKey, this.options.keyPair, null, null);
                            if (comReference == null)
                            {
                                this.importer.SaveAssemblyBuilder((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, asmBldr, assemName);
                                this.importer.AddRCW((System.Runtime.InteropServices.ComTypes.ITypeLib) typeLib, asmBldr);
                                return asmBldr;
                            }
                            assembly2 = this.importer.GetCopiedAssembly(comReference, false, false);
                        }
                        catch
                        {
                            assembly2 = null;
                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(typeLib);
                }
                return assembly2;
            }
        }

        public interface IReferenceResolver
        {
            string ResolveActiveXReference(UCOMITypeLib typeLib);
            string ResolveComReference(AssemblyName name);
            string ResolveComReference(UCOMITypeLib typeLib);
            string ResolveManagedReference(string assemName);
        }

        public sealed class Options
        {
            public bool delaySign;
            public bool genSources;
            public string keyContainer;
            public string keyFile;
            public StrongNameKeyPair keyPair;
            public bool msBuildErrors;
            public bool noLogo;
            public string outputDirectory;
            public string outputName;
            public bool overwriteRCW;
            public byte[] publicKey;
            public AxImporter.IReferenceResolver references;
            public bool silentMode;
            public bool verboseMode;
        }
    }
}

