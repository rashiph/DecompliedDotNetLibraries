namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Threading;

    public class UnregisterAssembly : AppDomainIsolatedTaskExtension
    {
        private ITaskItem[] assemblies;
        private ITaskItem assemblyListFile;
        private ITaskItem[] typeLibFiles;
        private static Mutex unregisteringLock = new Mutex(false, "MSBUILD_V_3_5_UNREGISTER_LOCK");
        private const string unregisteringLockName = "MSBUILD_V_3_5_UNREGISTER_LOCK";

        public override bool Execute()
        {
            AssemblyRegistrationCache cache = null;
            if (this.AssemblyListFile != null)
            {
                cache = (AssemblyRegistrationCache) StateFileBase.DeserializeCache(this.AssemblyListFile.ItemSpec, base.Log, typeof(AssemblyRegistrationCache));
                if (cache == null)
                {
                    StateFileBase.DeleteFile(this.AssemblyListFile.ItemSpec, base.Log);
                    return true;
                }
            }
            else
            {
                if (this.Assemblies == null)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.AssemblyPathOrStateFileIsRequired", new object[] { base.GetType().Name });
                    return false;
                }
                if ((this.TypeLibFiles != null) && (this.TypeLibFiles.Length != this.Assemblies.Length))
                {
                    base.Log.LogErrorWithCodeFromResources("General.TwoVectorsMustHaveSameLength", new object[] { this.Assemblies.Length, this.TypeLibFiles.Length, "Assemblies", "TypeLibFiles" });
                    return false;
                }
                cache = new AssemblyRegistrationCache();
                for (int i = 0; i < this.Assemblies.Length; i++)
                {
                    if (((this.TypeLibFiles != null) && (this.TypeLibFiles[i] != null)) && (this.TypeLibFiles[i].ItemSpec.Length > 0))
                    {
                        cache.AddEntry(this.Assemblies[i].ItemSpec, this.TypeLibFiles[i].ItemSpec);
                    }
                    else
                    {
                        cache.AddEntry(this.Assemblies[i].ItemSpec, Path.ChangeExtension(this.Assemblies[i].ItemSpec, ".tlb"));
                    }
                }
            }
            bool flag = true;
            try
            {
                for (int j = 0; j < cache.Count; j++)
                {
                    string assemblyPath = null;
                    string typeLibraryPath = null;
                    cache.GetEntry(j, out assemblyPath, out typeLibraryPath);
                    try
                    {
                        if (!this.Unregister(assemblyPath, typeLibraryPath))
                        {
                            flag = false;
                        }
                    }
                    catch (ArgumentException exception)
                    {
                        base.Log.LogErrorWithCodeFromResources("General.InvalidAssemblyName", new object[] { assemblyPath, exception.Message });
                        flag = false;
                    }
                }
            }
            finally
            {
                if (this.AssemblyListFile != null)
                {
                    StateFileBase.DeleteFile(this.AssemblyListFile.ItemSpec, base.Log);
                }
            }
            return flag;
        }

        private bool Unregister(string assemblyPath, string typeLibPath)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(typeLibPath, "typeLibPath");
            base.Log.LogMessageFromResources(MessageImportance.Low, "UnregisterAssembly.UnregisteringAssembly", new object[] { assemblyPath });
            if (File.Exists(assemblyPath))
            {
                try
                {
                    Assembly assembly = Assembly.UnsafeLoadFrom(assemblyPath);
                    RegistrationServices services = new RegistrationServices();
                    try
                    {
                        unregisteringLock.WaitOne();
                        if (!services.UnregisterAssembly(assembly))
                        {
                            base.Log.LogWarningWithCodeFromResources("UnregisterAssembly.NoValidTypes", new object[] { assemblyPath });
                        }
                    }
                    finally
                    {
                        unregisteringLock.ReleaseMutex();
                    }
                    goto Label_0237;
                }
                catch (ArgumentNullException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.CantUnregisterAssembly", new object[] { assemblyPath, exception.Message });
                    return false;
                }
                catch (InvalidOperationException exception2)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.CantUnregisterAssembly", new object[] { assemblyPath, exception2.Message });
                    return false;
                }
                catch (TargetInvocationException exception3)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.CantUnregisterAssembly", new object[] { assemblyPath, exception3.Message });
                    return false;
                }
                catch (IOException exception4)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.CantUnregisterAssembly", new object[] { assemblyPath, exception4.Message });
                    return false;
                }
                catch (TypeLoadException exception5)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.CantUnregisterAssembly", new object[] { assemblyPath, exception5.Message });
                    return false;
                }
                catch (UnauthorizedAccessException exception6)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.UnauthorizedAccess", new object[] { assemblyPath, exception6.Message });
                    return false;
                }
                catch (BadImageFormatException)
                {
                    base.Log.LogErrorWithCodeFromResources("General.InvalidAssembly", new object[] { assemblyPath });
                    return false;
                }
                catch (SecurityException exception7)
                {
                    base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.UnauthorizedAccess", new object[] { assemblyPath, exception7.Message });
                    return false;
                }
            }
            base.Log.LogWarningWithCodeFromResources("UnregisterAssembly.UnregisterAsmFileDoesNotExist", new object[] { assemblyPath });
        Label_0237:;
            base.Log.LogMessageFromResources(MessageImportance.Low, "UnregisterAssembly.UnregisteringTypeLib", new object[] { typeLibPath });
            if (File.Exists(typeLibPath))
            {
                try
                {
                    ITypeLib o = (ITypeLib) Microsoft.Build.Tasks.NativeMethods.LoadTypeLibEx(typeLibPath, 2);
                    IntPtr zero = IntPtr.Zero;
                    try
                    {
                        o.GetLibAttr(out zero);
                        if (zero != IntPtr.Zero)
                        {
                            System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                            Microsoft.Build.Tasks.NativeMethods.UnregisterTypeLib(ref typelibattr.guid, typelibattr.wMajorVerNum, typelibattr.wMinorVerNum, typelibattr.lcid, typelibattr.syskind);
                        }
                    }
                    finally
                    {
                        o.ReleaseTLibAttr(zero);
                        Marshal.ReleaseComObject(o);
                    }
                    goto Label_036E;
                }
                catch (COMException exception8)
                {
                    if (exception8.ErrorCode != -2147319780)
                    {
                        if (exception8.ErrorCode != -2147312566)
                        {
                            throw;
                        }
                        base.Log.LogErrorWithCodeFromResources("UnregisterAssembly.UnregisterTlbCantLoadFile", new object[] { typeLibPath });
                        return false;
                    }
                    base.Log.LogWarningWithCodeFromResources("UnregisterAssembly.UnregisterTlbFileNotRegistered", new object[] { typeLibPath });
                    goto Label_036E;
                }
            }
            base.Log.LogMessageFromResources(MessageImportance.Low, "UnregisterAssembly.UnregisterTlbFileDoesNotExist", new object[] { typeLibPath });
        Label_036E:
            return true;
        }

        public ITaskItem[] Assemblies
        {
            get
            {
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

