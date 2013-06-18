namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.EnterpriseServices;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    internal class ComPlusInstanceContextInitializer : IInstanceContextInitializer
    {
        private static readonly Guid DefaultPartitionId = new Guid("41E90F3E-56C1-4633-81C3-6E8BAC8BDD70");
        private static readonly Guid IID_IServiceActivity = new Guid("67532E0C-9E2F-4450-A354-035633944E17");
        private ServiceInfo info;
        private static string manifestFileName = Guid.NewGuid().ToString();
        private static object manifestLock = new object();

        static ComPlusInstanceContextInitializer()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ComPlusInstanceContextInitializer.ResolveAssembly);
        }

        public ComPlusInstanceContextInitializer(ServiceInfo info)
        {
            this.info = info;
            if (this.info.HasUdts())
            {
                string directory = string.Empty;
                lock (manifestLock)
                {
                    try
                    {
                        directory = Path.GetTempPath();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.CannotAccessDirectory(directory));
                    }
                    string path = directory + this.info.AppID.ToString();
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
        }

        public void Initialize(InstanceContext instanceContext, Message message)
        {
            object pIUnknown = null;
            pIUnknown = this.SetupServiceConfig(instanceContext, message);
            IServiceActivity activity = (IServiceActivity) System.ServiceModel.ComIntegration.SafeNativeMethods.CoCreateActivity(pIUnknown, IID_IServiceActivity);
            bool postSynchronous = this.info.ThreadingModel == ThreadingModel.MTA;
            ComPlusSynchronizationContext context = new ComPlusSynchronizationContext(activity, postSynchronous);
            instanceContext.SynchronizationContext = context;
            instanceContext.Closing += new EventHandler(this.OnInstanceContextClosing);
            Marshal.ReleaseComObject(pIUnknown);
        }

        public void OnInstanceContextClosing(object sender, EventArgs args)
        {
            InstanceContext context = (InstanceContext) sender;
            ((ComPlusSynchronizationContext) context.SynchronizationContext).Dispose();
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            int index = args.Name.IndexOf(",", StringComparison.Ordinal);
            if (index == -1)
            {
                return null;
            }
            string guidString = args.Name.Substring(0, index).Trim().ToLowerInvariant();
            return TypeCacheManager.Provider.ResolveAssembly(Fx.CreateGuid(guidString));
        }

        private object SetupServiceConfig(InstanceContext instanceContext, Message message)
        {
            object obj2 = new CServiceConfig();
            IServiceThreadPoolConfig config = (IServiceThreadPoolConfig) obj2;
            switch (this.info.ThreadingModel)
            {
                case ThreadingModel.MTA:
                    config.SelectThreadPool(System.ServiceModel.ComIntegration.ThreadPoolOption.MTA);
                    break;

                case ThreadingModel.STA:
                    config.SelectThreadPool(System.ServiceModel.ComIntegration.ThreadPoolOption.STA);
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.UnexpectedThreadingModel());
            }
            config.SetBindingInfo(System.ServiceModel.ComIntegration.BindingOption.BindingToPoolThread);
            if (this.info.HasUdts())
            {
                IServiceSxsConfig config2 = obj2 as IServiceSxsConfig;
                if (config2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.QFENotPresent());
                }
                lock (manifestLock)
                {
                    string directory = string.Empty;
                    try
                    {
                        directory = Path.GetTempPath();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.CannotAccessDirectory(directory));
                    }
                    string path = directory + this.info.AppID.ToString() + @"\";
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.CannotAccessDirectory(path));
                        }
                        Guid[] assemblies = this.info.Assemblies;
                        ComIntegrationManifestGenerator.GenerateManifestCollectionFile(assemblies, path + manifestFileName + ".manifest", manifestFileName);
                        foreach (Guid guid in assemblies)
                        {
                            System.Type[] types = this.info.GetTypes(guid);
                            if (types.Length > 0)
                            {
                                string assemblyName = guid.ToString();
                                ComIntegrationManifestGenerator.GenerateWin32ManifestFile(types, path + assemblyName + ".manifest", assemblyName);
                            }
                        }
                    }
                    config2.SxsConfig(CSC_SxsConfig.CSC_NewSxs);
                    config2.SxsName(manifestFileName + ".manifest");
                    config2.SxsDirectory(path);
                }
            }
            if (this.info.PartitionId != DefaultPartitionId)
            {
                IServicePartitionConfig config3 = (IServicePartitionConfig) obj2;
                config3.PartitionConfig(System.ServiceModel.ComIntegration.PartitionOption.New);
                config3.PartitionID(this.info.PartitionId);
            }
            IServiceTransactionConfig config4 = (IServiceTransactionConfig) obj2;
            config4.ConfigureTransaction(TransactionConfig.NoTransaction);
            if ((this.info.TransactionOption == TransactionOption.Required) || (this.info.TransactionOption == TransactionOption.Supported))
            {
                Transaction messageTransaction = null;
                messageTransaction = MessageUtil.GetMessageTransaction(message);
                if (messageTransaction != null)
                {
                    System.ServiceModel.ComIntegration.TransactionProxy item = new System.ServiceModel.ComIntegration.TransactionProxy(this.info.AppID, this.info.Clsid);
                    item.SetTransaction(messageTransaction);
                    instanceContext.Extensions.Add(item);
                    IServiceSysTxnConfig config5 = (IServiceSysTxnConfig) config4;
                    IntPtr pITxByot = TransactionProxyBuilder.CreateTransactionProxyTearOff(item);
                    config5.ConfigureBYOTSysTxn(pITxByot);
                    Marshal.Release(pITxByot);
                }
            }
            return obj2;
        }
    }
}

