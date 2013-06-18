namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [Obsolete("The RegistrationHelperTx class has been deprecated."), Transaction(TransactionOption.RequiresNew), Guid("c89ac250-e18a-4fc7-abd5-b8897b6a78a5")]
    public sealed class RegistrationHelperTx : ServicedComponent
    {
        private static Guid _appid;
        private static Guid _appidNoWow64 = new Guid("1e246775-2281-484f-8ad4-044c15b86eb7");
        private static Guid _appidWow64 = new Guid("57926702-ab7c-402b-abce-e262da1dd7c9");
        private static string _appname;
        private static string _appnameNoWow64 = ".NET Utilities";
        private static string _appnameWow64 = ".NET Utilities (32 bit)";

        static RegistrationHelperTx()
        {
            if (Wow64Helper.IsWow64Process())
            {
                _appid = _appidWow64;
                _appname = _appnameWow64;
            }
            else
            {
                _appid = _appidNoWow64;
                _appname = _appnameNoWow64;
            }
        }

        protected internal override void Activate()
        {
        }

        private static void ConfigureComponent(ICatalogCollection coll, ICatalogObject obj)
        {
            obj.SetValue("Transaction", TransactionOption.RequiresNew);
            obj.SetValue("ComponentTransactionTimeoutEnabled", true);
            obj.SetValue("ComponentTransactionTimeout", 0);
            coll.SaveChanges();
        }

        protected internal override void Deactivate()
        {
        }

        private static ICatalogObject FindApplication(ICatalogCollection coll, Guid appid, ref int idx)
        {
            int num = coll.Count();
            for (int i = 0; i < num; i++)
            {
                ICatalogObject obj2 = (ICatalogObject) coll.Item(i);
                Guid guid = new Guid((string) obj2.GetValue("ID"));
                if (guid == appid)
                {
                    idx = i;
                    return obj2;
                }
            }
            return null;
        }

        private static ICatalogObject FindComponent(ICatalogCollection coll, Guid clsid, ref int idx)
        {
            RegistrationDriver.Populate(coll);
            int num = coll.Count();
            for (int i = 0; i < num; i++)
            {
                ICatalogObject obj2 = (ICatalogObject) coll.Item(i);
                Guid guid = new Guid((string) obj2.GetValue("CLSID"));
                if (guid == clsid)
                {
                    idx = i;
                    return obj2;
                }
            }
            return null;
        }

        public void InstallAssembly(string assembly, ref string application, ref string tlb, InstallationFlags installFlags, object sync)
        {
            this.InstallAssembly(assembly, ref application, null, ref tlb, installFlags, sync);
        }

        public void InstallAssembly(string assembly, ref string application, string partition, ref string tlb, InstallationFlags installFlags, object sync)
        {
            RegistrationConfig regConfig = new RegistrationConfig {
                AssemblyFile = assembly,
                Application = application,
                Partition = partition,
                TypeLibrary = tlb,
                InstallationFlags = installFlags
            };
            this.InstallAssemblyFromConfig(ref regConfig, sync);
            application = regConfig.AssemblyFile;
            tlb = regConfig.TypeLibrary;
        }

        public void InstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig, object sync)
        {
            bool flag = false;
            try
            {
                new RegistrationDriver().InstallAssembly(regConfig, sync);
                ContextUtil.SetComplete();
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    ContextUtil.SetAbort();
                }
            }
        }

        [ComRegisterFunction]
        internal static void InstallUtilityApplication(Type t)
        {
        }

        public bool IsInTransaction()
        {
            return ContextUtil.IsInTransaction;
        }

        public void UninstallAssembly(string assembly, string application, object sync)
        {
            this.UninstallAssembly(assembly, application, null, sync);
        }

        public void UninstallAssembly(string assembly, string application, string partition, object sync)
        {
            RegistrationConfig regConfig = new RegistrationConfig {
                AssemblyFile = assembly,
                Application = application,
                Partition = partition
            };
            this.UninstallAssemblyFromConfig(ref regConfig, sync);
        }

        public void UninstallAssemblyFromConfig([MarshalAs(UnmanagedType.IUnknown)] ref RegistrationConfig regConfig, object sync)
        {
            bool flag = false;
            try
            {
                new RegistrationDriver().UninstallAssembly(regConfig, sync);
                ContextUtil.SetComplete();
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    ContextUtil.SetAbort();
                }
            }
        }

        [ComUnregisterFunction]
        internal static void UninstallUtilityApplication(Type t)
        {
        }
    }
}

