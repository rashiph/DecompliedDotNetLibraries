namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;

    public class RegisterAssembly : AppDomainIsolatedTaskExtension, ITypeLibExporterNotifySink
    {
        private ITaskItem[] assemblies;
        private ITaskItem assemblyListFile;
        private bool createCodeBase;
        private bool typeLibExportFailed;
        private ITaskItem[] typeLibFiles;

        public override bool Execute()
        {
            if ((this.TypeLibFiles != null) && (this.TypeLibFiles.Length != this.Assemblies.Length))
            {
                base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.Assemblies.Length, this.TypeLibFiles.Length, "Assemblies", "TypeLibFiles" });
                return false;
            }
            if (this.TypeLibFiles == null)
            {
                this.TypeLibFiles = new TaskItem[this.Assemblies.Length];
            }
            AssemblyRegistrationCache cache = null;
            if ((this.AssemblyListFile != null) && (this.AssemblyListFile.ItemSpec.Length > 0))
            {
                cache = (AssemblyRegistrationCache) StateFileBase.DeserializeCache(this.AssemblyListFile.ItemSpec, base.Log, typeof(AssemblyRegistrationCache));
                if (cache == null)
                {
                    cache = new AssemblyRegistrationCache();
                }
            }
            bool flag = true;
            try
            {
                for (int i = 0; i < this.Assemblies.Length; i++)
                {
                    try
                    {
                        string itemSpec;
                        if ((this.TypeLibFiles[i] != null) && (this.TypeLibFiles[i].ItemSpec.Length > 0))
                        {
                            itemSpec = this.TypeLibFiles[i].ItemSpec;
                        }
                        else
                        {
                            itemSpec = Path.ChangeExtension(this.Assemblies[i].ItemSpec, ".tlb");
                            this.TypeLibFiles[i] = new TaskItem(itemSpec);
                        }
                        if (!this.Register(this.Assemblies[i].ItemSpec, itemSpec))
                        {
                            flag = false;
                        }
                        else if (cache != null)
                        {
                            cache.AddEntry(this.Assemblies[i].ItemSpec, itemSpec);
                        }
                    }
                    catch (ArgumentException exception)
                    {
                        base.Log.LogErrorWithCodeFromResources("General.InvalidAssemblyName", new object[] { this.Assemblies[i], exception.Message });
                        flag = false;
                    }
                }
            }
            finally
            {
                if (cache != null)
                {
                    cache.SerializeCache(this.AssemblyListFile.ItemSpec, base.Log);
                }
            }
            return flag;
        }

        private bool ExportTypeLib(Assembly asm, string typeLibFileName)
        {
            this.typeLibExportFailed = false;
            ITypeLib o = null;
            try
            {
                ITypeLibConverter converter = new TypeLibConverter();
                o = (ITypeLib) converter.ConvertAssemblyToTypeLib(asm, typeLibFileName, TypeLibExporterFlags.None, this);
                if ((o == null) || this.typeLibExportFailed)
                {
                    return false;
                }
                ((UCOMICreateITypeLib) o).SaveAllChanges();
            }
            finally
            {
                if (o != null)
                {
                    Marshal.ReleaseComObject(o);
                }
            }
            return !this.typeLibExportFailed;
        }

        private bool Register(string assemblyPath, string typeLibPath)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(typeLibPath, "typeLibPath");
            base.Log.LogMessageFromResources(MessageImportance.Low, "RegisterAssembly.RegisteringAssembly", new object[] { assemblyPath });
            if (!File.Exists(assemblyPath))
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.RegisterAsmFileDoesNotExist", new object[] { assemblyPath });
                return false;
            }
            ITypeLib pTypeLib = null;
            try
            {
                Assembly assembly = Assembly.UnsafeLoadFrom(assemblyPath);
                RegistrationServices services = new RegistrationServices();
                if (!services.RegisterAssembly(assembly, this.CreateCodeBase ? AssemblyRegistrationFlags.SetCodeBase : AssemblyRegistrationFlags.None))
                {
                    base.Log.LogWarningWithCodeFromResources("RegisterAssembly.NoValidTypes", new object[] { assemblyPath });
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "RegisterAssembly.RegisteringTypeLib", new object[] { typeLibPath });
                if (!File.Exists(typeLibPath) || (File.GetLastWriteTime(typeLibPath) < File.GetLastWriteTime(assemblyPath)))
                {
                    try
                    {
                        if (!this.ExportTypeLib(assembly, typeLibPath))
                        {
                            return false;
                        }
                        goto Label_0138;
                    }
                    catch (COMException exception)
                    {
                        base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantExportTypeLib", new object[] { assemblyPath, exception.Message });
                        return false;
                    }
                }
                base.Log.LogMessageFromResources(MessageImportance.Low, "RegisterAssembly.TypeLibUpToDate", new object[] { typeLibPath });
            Label_0138:
                try
                {
                    pTypeLib = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadTypeLibEx(typeLibPath, 2);
                    Microsoft.Build.Tasks.NativeMethods.RegisterTypeLib(pTypeLib, typeLibPath, null);
                }
                catch (COMException exception2)
                {
                    base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterTypeLib", new object[] { typeLibPath, exception2.Message });
                    return false;
                }
            }
            catch (ArgumentNullException exception3)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception3.Message });
                return false;
            }
            catch (InvalidOperationException exception4)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception4.Message });
                return false;
            }
            catch (TargetInvocationException exception5)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception5.Message });
                return false;
            }
            catch (IOException exception6)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception6.Message });
                return false;
            }
            catch (TypeLoadException exception7)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception7.Message });
                return false;
            }
            catch (UnauthorizedAccessException exception8)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.UnauthorizedAccess", new object[] { assemblyPath, exception8.Message });
                return false;
            }
            catch (BadImageFormatException)
            {
                base.Log.LogErrorWithCodeFromResources("General.InvalidAssembly", new object[] { assemblyPath });
                return false;
            }
            catch (SecurityException exception9)
            {
                base.Log.LogErrorWithCodeFromResources("RegisterAssembly.CantRegisterAssembly", new object[] { assemblyPath, exception9.Message });
                return false;
            }
            finally
            {
                if (pTypeLib != null)
                {
                    Marshal.ReleaseComObject(pTypeLib);
                }
            }
            return true;
        }

        public void ReportEvent(ExporterEventKind kind, int code, string msg)
        {
            if (kind == ExporterEventKind.ERROR_REFTOINVALIDASSEMBLY)
            {
                base.Log.LogError(msg, new object[0]);
                this.typeLibExportFailed = true;
            }
            else if (kind == ExporterEventKind.NOTIF_CONVERTWARNING)
            {
                base.Log.LogWarning(msg, new object[0]);
            }
            else if (kind == ExporterEventKind.NOTIF_TYPECONVERTED)
            {
                base.Log.LogMessage(MessageImportance.Low, msg, new object[0]);
            }
            else
            {
                base.Log.LogMessage(MessageImportance.Low, msg, new object[0]);
            }
        }

        public object ResolveRef(Assembly assemblyToResolve)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(assemblyToResolve, "assemblyToResolve");
            base.Log.LogErrorWithCodeFromResources("RegisterAssembly.AssemblyNotRegisteredForComInterop", new object[] { assemblyToResolve.GetName().FullName });
            this.typeLibExportFailed = true;
            return null;
        }

        [Required]
        public ITaskItem[] Assemblies
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.assemblies, "assemblies");
                return this.assemblies;
            }
            set
            {
                this.assemblies = value;
            }
        }

        public ITaskItem AssemblyListFile
        {
            get
            {
                return this.assemblyListFile;
            }
            set
            {
                this.assemblyListFile = value;
            }
        }

        public bool CreateCodeBase
        {
            get
            {
                return this.createCodeBase;
            }
            set
            {
                this.createCodeBase = value;
            }
        }

        [Output]
        public ITaskItem[] TypeLibFiles
        {
            get
            {
                return this.typeLibFiles;
            }
            set
            {
                this.typeLibFiles = value;
            }
        }
    }
}

