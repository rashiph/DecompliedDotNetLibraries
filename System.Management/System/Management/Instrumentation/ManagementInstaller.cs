namespace System.Management.Instrumentation
{
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;

    public class ManagementInstaller : Installer
    {
        private static bool helpPrinted;
        private string mof;

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
            if (base.Context.Parameters.ContainsKey("mof"))
            {
                string path = base.Context.Parameters["mof"];
                if ((path == null) || (path.Length == 0))
                {
                    path = base.Context.Parameters["assemblypath"];
                    if ((path == null) || (path.Length == 0))
                    {
                        path = "defaultmoffile";
                    }
                    else
                    {
                        path = Path.GetFileName(path);
                    }
                }
                if (path.Length < 4)
                {
                    path = path + ".mof";
                }
                else if (string.Compare(path.Substring(path.Length - 4, 4), ".mof", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    path = path + ".mof";
                }
                base.Context.LogMessage(RC.GetString("MOFFILE_GENERATING") + " " + path);
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.Unicode))
                {
                    writer.WriteLine("//**************************************************************************");
                    writer.WriteLine("//* {0}", path);
                    writer.WriteLine("//**************************************************************************");
                    writer.WriteLine(this.mof);
                }
            }
        }

        public override void Install(IDictionary savedState)
        {
            new FileIOPermission(FileIOPermissionAccess.Read, base.Context.Parameters["assemblypath"]).Demand();
            base.Install(savedState);
            base.Context.LogMessage(RC.GetString("WMISCHEMA_INSTALLATIONSTART"));
            string assemblyFile = base.Context.Parameters["assemblypath"];
            Assembly assembly = Assembly.LoadFrom(assemblyFile);
            SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assembly);
            schemaNaming.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
            if (schemaNaming != null)
            {
                if ((!schemaNaming.IsAssemblyRegistered() || base.Context.Parameters.ContainsKey("force")) || base.Context.Parameters.ContainsKey("f"))
                {
                    base.Context.LogMessage(RC.GetString("REGESTRING_ASSEMBLY") + " " + schemaNaming.DecoupledProviderInstanceName);
                    schemaNaming.RegisterNonAssemblySpecificSchema(base.Context);
                    schemaNaming.RegisterAssemblySpecificSchema();
                }
                this.mof = schemaNaming.Mof;
                base.Context.LogMessage(RC.GetString("WMISCHEMA_INSTALLATIONEND"));
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
        }

        public override string HelpText
        {
            get
            {
                if (helpPrinted)
                {
                    return base.HelpText;
                }
                helpPrinted = true;
                StringBuilder builder = new StringBuilder();
                builder.Append("/MOF=[filename]\r\n");
                builder.Append(" " + RC.GetString("FILETOWRITE_MOF") + "\r\n\r\n");
                builder.Append("/Force or /F\r\n");
                builder.Append(" " + RC.GetString("FORCE_UPDATE"));
                return (builder.ToString() + base.HelpText);
            }
        }
    }
}

