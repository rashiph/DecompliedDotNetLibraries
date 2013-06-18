namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions;

    [Guid("89a86e7b-c229-4008-9baa-2f5c8411d7e0")]
    public sealed class RegistrationHelper : MarshalByRefObject, IRegistrationHelper, IThunkInstallation
    {
        public void InstallAssembly(string assembly, ref string application, ref string tlb, InstallationFlags installFlags)
        {
            this.InstallAssembly(assembly, ref application, null, ref tlb, installFlags);
        }

        public void InstallAssembly(string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags)
        {
            RegistrationConfig regConfig = new RegistrationConfig {
                AssemblyFile = assembly,
                Application = application,
                Partition = partition,
                TypeLibrary = tlb,
                InstallationFlags = installFlags
            };
            this.InstallAssemblyFromConfig(ref regConfig);
            application = regConfig.Application;
            tlb = regConfig.TypeLibrary;
        }

        public void InstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
        {
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                RegistrationThreadWrapper wrapper = new RegistrationThreadWrapper(this, regConfig);
                Thread thread = new Thread(new ThreadStart(wrapper.InstallThread));
                thread.Start();
                thread.Join();
                wrapper.PropInstallResult();
            }
            else
            {
                TransactionOptions transactionOptions = new TransactionOptions {
                    Timeout = TimeSpan.FromSeconds(0.0),
                    IsolationLevel = IsolationLevel.Serializable
                };
                CatalogSync obSync = new CatalogSync();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, EnterpriseServicesInteropOption.Full))
                {
                    new RegistrationDriver().InstallAssembly(regConfig, obSync);
                    scope.Complete();
                }
                obSync.Wait();
            }
        }

        void IThunkInstallation.DefaultInstall(string asm)
        {
            string application = null;
            string tlb = null;
            this.InstallAssembly(asm, ref application, ref tlb, InstallationFlags.ReconfigureExistingApplication | InstallationFlags.FindOrCreateTargetApplication);
        }

        public void UninstallAssembly(string assembly, string application)
        {
            this.UninstallAssembly(assembly, application, null);
        }

        public void UninstallAssembly(string assembly, string application, string partition)
        {
            RegistrationConfig regConfig = new RegistrationConfig {
                AssemblyFile = assembly,
                Application = application,
                Partition = partition
            };
            this.UninstallAssemblyFromConfig(ref regConfig);
        }

        public void UninstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig)
        {
            SecurityPermission permission = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
            permission.Demand();
            permission.Assert();
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                RegistrationThreadWrapper wrapper = new RegistrationThreadWrapper(this, regConfig);
                Thread thread = new Thread(new ThreadStart(wrapper.UninstallThread));
                thread.Start();
                thread.Join();
                wrapper.PropUninstallResult();
            }
            else
            {
                TransactionOptions transactionOptions = new TransactionOptions {
                    Timeout = TimeSpan.FromMinutes(0.0),
                    IsolationLevel = IsolationLevel.Serializable
                };
                CatalogSync obSync = new CatalogSync();
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, EnterpriseServicesInteropOption.Full))
                {
                    new RegistrationDriver().UninstallAssembly(regConfig, obSync);
                    scope.Complete();
                }
                obSync.Wait();
            }
        }
    }
}

