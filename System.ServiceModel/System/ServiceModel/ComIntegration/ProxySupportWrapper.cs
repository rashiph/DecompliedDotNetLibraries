namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class ProxySupportWrapper
    {
        private static readonly Guid ClsidProxyInstanceProvider = new Guid("(BF0514FB-6912-4659-AD69-B727E5B7ADD4)");
        private const string fileName = "ServiceMonikerSupport.dll";
        private const string functionName = "DllGetClassObject";
        private DelegateDllGetClassObject getCODelegate = null;
        private SafeLibraryHandle monikerSupportLibrary = null;

        internal ProxySupportWrapper()
        {
        }

        ~ProxySupportWrapper()
        {
            if (this.monikerSupportLibrary != null)
            {
                this.monikerSupportLibrary.Close();
                this.monikerSupportLibrary = null;
            }
        }

        internal IProxyProvider GetProxyProvider()
        {
            if (this.monikerSupportLibrary == null)
            {
                lock (this)
                {
                    if (this.monikerSupportLibrary == null)
                    {
                        this.getCODelegate = null;
                        using (RegistryHandle handle = RegistryHandle.GetCorrectBitnessHKLMSubkey(IntPtr.Size == 8, @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client"))
                        {
                            string libFilename = handle.GetStringValue("InstallPath").TrimEnd(new char[1]) + @"\ServiceMonikerSupport.dll";
                            this.monikerSupportLibrary = UnsafeNativeMethods.LoadLibrary(libFilename);
                            this.monikerSupportLibrary.DoNotFreeLibraryOnRelease();
                            if (this.monikerSupportLibrary.IsInvalid)
                            {
                                this.monikerSupportLibrary.SetHandleAsInvalid();
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ServiceMonikerSupportLoadFailed(libFilename));
                            }
                        }
                    }
                }
            }
            if (this.getCODelegate == null)
            {
                lock (this)
                {
                    if (this.getCODelegate == null)
                    {
                        try
                        {
                            IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(this.monikerSupportLibrary, "DllGetClassObject");
                            this.getCODelegate = (DelegateDllGetClassObject) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DelegateDllGetClassObject));
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ComPlusProxyProviderException(System.ServiceModel.SR.GetString("FailedProxyProviderCreation"), exception));
                        }
                    }
                }
            }
            IClassFactory ppv = null;
            IProxyProvider provider = null;
            try
            {
                this.getCODelegate(ClsidProxyInstanceProvider, typeof(IClassFactory).GUID, ref ppv);
                provider = ppv.CreateInstance(null, typeof(IProxyProvider).GUID) as IProxyProvider;
                Thread.MemoryBarrier();
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ComPlusProxyProviderException(System.ServiceModel.SR.GetString("FailedProxyProviderCreation"), exception2));
            }
            finally
            {
                if (ppv != null)
                {
                    Marshal.ReleaseComObject(ppv);
                    ppv = null;
                }
            }
            return provider;
        }

        internal delegate int DelegateDllGetClassObject([In, MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [In, MarshalAs(UnmanagedType.LPStruct)] Guid iid, ref IClassFactory ppv);
    }
}

