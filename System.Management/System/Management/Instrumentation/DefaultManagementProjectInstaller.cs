namespace System.Management.Instrumentation
{
    using System;
    using System.Configuration.Install;

    public class DefaultManagementProjectInstaller : Installer
    {
        public DefaultManagementProjectInstaller()
        {
            ManagementInstaller installer = new ManagementInstaller();
            base.Installers.Add(installer);
        }
    }
}

