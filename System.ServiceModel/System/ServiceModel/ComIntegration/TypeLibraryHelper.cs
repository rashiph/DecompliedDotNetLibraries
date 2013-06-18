namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.ServiceModel;
    using System.Threading;

    internal sealed class TypeLibraryHelper
    {
        private static TypeLibraryHelper instance;
        private static int instanceCount = 0;
        private static object instanceLock = new object();
        private Dictionary<Guid, Assembly> TypelibraryAssembly = new Dictionary<Guid, Assembly>();
        private TypeLibConverter TypelibraryConverter = new TypeLibConverter();

        private Assembly GenerateAssemblyFromNativeTypeLibInternal(Guid iid, Guid typeLibraryID, ITypeLib typeLibrary)
        {
            Assembly assembly = null;
            try
            {
                lock (this)
                {
                    this.TypelibraryAssembly.TryGetValue(typeLibraryID, out assembly);
                    if (assembly == null)
                    {
                        int num;
                        string str4;
                        string asmFileName = "";
                        string strDocString = "";
                        string strHelpFile = "";
                        typeLibrary.GetDocumentation(-1, out str4, out strDocString, out num, out strHelpFile);
                        if (string.IsNullOrEmpty(str4))
                        {
                            throw Fx.AssertAndThrowFatal("Assembly cannot be null");
                        }
                        asmFileName = str4 + this.GetRandomName() + ".dll";
                        assembly = this.TypelibraryConverter.ConvertTypeLibToAssembly(typeLibrary, asmFileName, TypeLibImporterFlags.SerializableValueClasses, new ConversionEventHandler(iid, typeLibraryID), null, null, str4, null);
                        this.TypelibraryAssembly[typeLibraryID] = assembly;
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FailedToConvertTypelibraryToAssembly")));
            }
            if (assembly == null)
            {
                throw Fx.AssertAndThrowFatal("Assembly cannot be null");
            }
            return assembly;
        }

        internal static Assembly GenerateAssemblyFromNativeTypeLibrary(Guid iid, Guid typeLibraryID, ITypeLib typeLibrary)
        {
            Assembly assembly;
            TypeLibraryHelper helperInstance = GetHelperInstance();
            try
            {
                assembly = helperInstance.GenerateAssemblyFromNativeTypeLibInternal(iid, typeLibraryID, typeLibrary);
            }
            finally
            {
                ReleaseHelperInstance();
            }
            return assembly;
        }

        private static TypeLibraryHelper GetHelperInstance()
        {
            lock (instanceLock)
            {
                if (instance == null)
                {
                    TypeLibraryHelper helper = new TypeLibraryHelper();
                    Thread.MemoryBarrier();
                    instance = helper;
                }
            }
            Interlocked.Increment(ref instanceCount);
            return instance;
        }

        private string GetRandomName()
        {
            return Guid.NewGuid().ToString().Replace('-', '_');
        }

        private static void ReleaseHelperInstance()
        {
            if (Interlocked.Decrement(ref instanceCount) == 0)
            {
                instance = null;
            }
        }

        internal class ConversionEventHandler : ITypeLibImporterNotifySink
        {
            private Guid iid;
            private Guid typeLibraryID;

            public ConversionEventHandler(Guid iid, Guid typeLibraryID)
            {
                this.iid = iid;
                this.typeLibraryID = typeLibraryID;
            }

            void ITypeLibImporterNotifySink.ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
            {
                ComPlusTLBImportTrace.Trace(TraceEventType.Verbose, 0x50010, "TraceCodeComIntegrationTLBImportConverterEvent", this.iid, this.typeLibraryID, eventKind, eventCode, eventMsg);
            }

            Assembly ITypeLibImporterNotifySink.ResolveRef(object typeLib)
            {
                Assembly assembly;
                ITypeLib lib = typeLib as ITypeLib;
                IntPtr zero = IntPtr.Zero;
                try
                {
                    lib.GetLibAttr(out zero);
                    System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                    assembly = TypeLibraryHelper.GenerateAssemblyFromNativeTypeLibrary(this.iid, typelibattr.guid, typeLib as ITypeLib);
                }
                finally
                {
                    if ((zero != IntPtr.Zero) && (lib != null))
                    {
                        lib.ReleaseTLibAttr(zero);
                    }
                }
                return assembly;
            }
        }
    }
}

