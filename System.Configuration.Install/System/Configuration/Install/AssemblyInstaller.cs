namespace System.Configuration.Install
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Xml;

    public class AssemblyInstaller : Installer
    {
        private System.Reflection.Assembly assembly;
        private string[] commandLine;
        private static bool helpPrinted;
        private bool initialized;
        private bool useNewContext;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AssemblyInstaller()
        {
        }

        public AssemblyInstaller(System.Reflection.Assembly assembly, string[] commandLine)
        {
            this.Assembly = assembly;
            this.commandLine = commandLine;
            this.useNewContext = true;
        }

        public AssemblyInstaller(string fileName, string[] commandLine)
        {
            this.Path = System.IO.Path.GetFullPath(fileName);
            this.commandLine = commandLine;
            this.useNewContext = true;
        }

        public static void CheckIfInstallable(string assemblyName)
        {
            AssemblyInstaller installer = new AssemblyInstaller {
                UseNewContext = false,
                Path = assemblyName,
                CommandLine = new string[0],
                Context = new InstallContext(null, new string[0])
            };
            installer.InitializeFromAssembly();
            if (installer.Installers.Count == 0)
            {
                throw new InvalidOperationException(System.Configuration.Install.Res.GetString("InstallNoPublicInstallers", new object[] { assemblyName }));
            }
        }

        public override void Commit(IDictionary savedState)
        {
            this.PrintStartText(System.Configuration.Install.Res.GetString("InstallActivityCommitting"));
            if (!this.initialized)
            {
                this.InitializeFromAssembly();
            }
            string installStatePath = this.GetInstallStatePath(this.Path);
            FileStream input = new FileStream(installStatePath, FileMode.Open, FileAccess.Read);
            XmlReaderSettings settings = new XmlReaderSettings {
                CheckCharacters = false,
                CloseInput = false
            };
            XmlReader reader = null;
            if (input != null)
            {
                reader = XmlReader.Create(input, settings);
            }
            try
            {
                if (reader != null)
                {
                    NetDataContractSerializer serializer = new NetDataContractSerializer();
                    savedState = (Hashtable) serializer.ReadObject(reader);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (input != null)
                {
                    input.Close();
                }
                if (base.Installers.Count == 0)
                {
                    base.Context.LogMessage(System.Configuration.Install.Res.GetString("RemovingInstallState"));
                    File.Delete(installStatePath);
                }
            }
            base.Commit(savedState);
        }

        private InstallContext CreateAssemblyContext()
        {
            InstallContext context = new InstallContext(System.IO.Path.ChangeExtension(this.Path, ".InstallLog"), this.CommandLine);
            if (base.Context != null)
            {
                context.Parameters["logtoconsole"] = base.Context.Parameters["logtoconsole"];
            }
            context.Parameters["assemblypath"] = this.Path;
            return context;
        }

        private static Type[] GetInstallerTypes(System.Reflection.Assembly assem)
        {
            ArrayList list = new ArrayList();
            Module[] modules = assem.GetModules();
            for (int i = 0; i < modules.Length; i++)
            {
                Type[] types = modules[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if ((typeof(Installer).IsAssignableFrom(types[j]) && !types[j].IsAbstract) && (types[j].IsPublic && ((RunInstallerAttribute) TypeDescriptor.GetAttributes(types[j])[typeof(RunInstallerAttribute)]).RunInstaller))
                    {
                        list.Add(types[j]);
                    }
                }
            }
            return (Type[]) list.ToArray(typeof(Type));
        }

        private string GetInstallStatePath(string assemblyPath)
        {
            string str2 = base.Context.Parameters["InstallStateDir"];
            assemblyPath = System.IO.Path.ChangeExtension(assemblyPath, ".InstallState");
            if (!string.IsNullOrEmpty(str2))
            {
                return System.IO.Path.Combine(str2, System.IO.Path.GetFileName(assemblyPath));
            }
            return assemblyPath;
        }

        private void InitializeFromAssembly()
        {
            Type[] installerTypes = null;
            try
            {
                installerTypes = GetInstallerTypes(this.assembly);
            }
            catch (Exception exception)
            {
                base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallException", new object[] { this.Path }));
                Installer.LogException(exception, base.Context);
                base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallAbort", new object[] { this.Path }));
                throw new InvalidOperationException(System.Configuration.Install.Res.GetString("InstallNoInstallerTypes", new object[] { this.Path }), exception);
            }
            if ((installerTypes == null) || (installerTypes.Length == 0))
            {
                base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallNoPublicInstallers", new object[] { this.Path }));
            }
            else
            {
                for (int i = 0; i < installerTypes.Length; i++)
                {
                    try
                    {
                        Installer installer = (Installer) Activator.CreateInstance(installerTypes[i], BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, new object[0], null);
                        base.Installers.Add(installer);
                    }
                    catch (Exception exception2)
                    {
                        base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallCannotCreateInstance", new object[] { installerTypes[i].FullName }));
                        Installer.LogException(exception2, base.Context);
                        throw new InvalidOperationException(System.Configuration.Install.Res.GetString("InstallCannotCreateInstance", new object[] { installerTypes[i].FullName }), exception2);
                    }
                }
                this.initialized = true;
            }
        }

        public override void Install(IDictionary savedState)
        {
            this.PrintStartText(System.Configuration.Install.Res.GetString("InstallActivityInstalling"));
            if (!this.initialized)
            {
                this.InitializeFromAssembly();
            }
            Hashtable hashtable = new Hashtable();
            savedState = hashtable;
            try
            {
                base.Install(savedState);
            }
            finally
            {
                FileStream output = new FileStream(this.GetInstallStatePath(this.Path), FileMode.Create);
                XmlWriterSettings settings = new XmlWriterSettings {
                    Encoding = Encoding.UTF8,
                    CheckCharacters = false,
                    CloseOutput = false
                };
                XmlWriter writer = XmlWriter.Create(output, settings);
                try
                {
                    new NetDataContractSerializer().WriteObject(writer, savedState);
                }
                finally
                {
                    writer.Close();
                    output.Close();
                }
            }
        }

        private void PrintStartText(string activity)
        {
            if (this.UseNewContext)
            {
                InstallContext context = this.CreateAssemblyContext();
                if (base.Context != null)
                {
                    base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallLogContent", new object[] { this.Path }));
                    base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallFileLocation", new object[] { context.Parameters["logfile"] }));
                }
                base.Context = context;
            }
            base.Context.LogMessage(string.Format(CultureInfo.InvariantCulture, activity, new object[] { this.Path }));
            base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallLogParameters"));
            if (base.Context.Parameters.Count == 0)
            {
                base.Context.LogMessage("   " + System.Configuration.Install.Res.GetString("InstallLogNone"));
            }
            IDictionaryEnumerator enumerator = (IDictionaryEnumerator) base.Context.Parameters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string key = (string) enumerator.Key;
                string str2 = (string) enumerator.Value;
                if (key.Equals("password", StringComparison.InvariantCultureIgnoreCase))
                {
                    str2 = "********";
                }
                base.Context.LogMessage("   " + key + " = " + str2);
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            this.PrintStartText(System.Configuration.Install.Res.GetString("InstallActivityRollingBack"));
            if (!this.initialized)
            {
                this.InitializeFromAssembly();
            }
            string installStatePath = this.GetInstallStatePath(this.Path);
            FileStream input = new FileStream(installStatePath, FileMode.Open, FileAccess.Read);
            XmlReaderSettings settings = new XmlReaderSettings {
                CheckCharacters = false,
                CloseInput = false
            };
            XmlReader reader = null;
            if (input != null)
            {
                reader = XmlReader.Create(input, settings);
            }
            try
            {
                if (reader != null)
                {
                    NetDataContractSerializer serializer = new NetDataContractSerializer();
                    savedState = (Hashtable) serializer.ReadObject(reader);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (input != null)
                {
                    input.Close();
                }
            }
            try
            {
                base.Rollback(savedState);
            }
            finally
            {
                File.Delete(installStatePath);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            this.PrintStartText(System.Configuration.Install.Res.GetString("InstallActivityUninstalling"));
            if (!this.initialized)
            {
                this.InitializeFromAssembly();
            }
            string installStatePath = this.GetInstallStatePath(this.Path);
            if ((installStatePath != null) && File.Exists(installStatePath))
            {
                FileStream input = new FileStream(installStatePath, FileMode.Open, FileAccess.Read);
                XmlReaderSettings settings = new XmlReaderSettings {
                    CheckCharacters = false,
                    CloseInput = false
                };
                XmlReader reader = null;
                if (input != null)
                {
                    reader = XmlReader.Create(input, settings);
                }
                try
                {
                    if (reader != null)
                    {
                        NetDataContractSerializer serializer = new NetDataContractSerializer();
                        savedState = (Hashtable) serializer.ReadObject(reader);
                    }
                }
                catch
                {
                    base.Context.LogMessage(System.Configuration.Install.Res.GetString("InstallSavedStateFileCorruptedWarning", new object[] { this.Path, installStatePath }));
                    savedState = null;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                    if (input != null)
                    {
                        input.Close();
                    }
                }
            }
            else
            {
                savedState = null;
            }
            base.Uninstall(savedState);
            if ((installStatePath != null) && (installStatePath.Length != 0))
            {
                try
                {
                    File.Delete(installStatePath);
                }
                catch
                {
                    throw new InvalidOperationException(System.Configuration.Install.Res.GetString("InstallUnableDeleteFile", new object[] { installStatePath }));
                }
            }
        }

        [System.Configuration.Install.ResDescription("Desc_AssemblyInstaller_Assembly")]
        public System.Reflection.Assembly Assembly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assembly;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.assembly = value;
            }
        }

        [System.Configuration.Install.ResDescription("Desc_AssemblyInstaller_CommandLine")]
        public string[] CommandLine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commandLine;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.commandLine = value;
            }
        }

        public override string HelpText
        {
            get
            {
                if ((this.Path != null) && (this.Path.Length > 0))
                {
                    base.Context = new InstallContext(null, new string[0]);
                    if (!this.initialized)
                    {
                        this.InitializeFromAssembly();
                    }
                }
                if (helpPrinted)
                {
                    return base.HelpText;
                }
                helpPrinted = true;
                return (System.Configuration.Install.Res.GetString("InstallAssemblyHelp") + "\r\n" + base.HelpText);
            }
        }

        [System.Configuration.Install.ResDescription("Desc_AssemblyInstaller_Path")]
        public string Path
        {
            get
            {
                if (this.assembly == null)
                {
                    return null;
                }
                return this.assembly.Location;
            }
            set
            {
                if (value == null)
                {
                    this.assembly = null;
                }
                this.assembly = System.Reflection.Assembly.LoadFrom(value);
            }
        }

        [System.Configuration.Install.ResDescription("Desc_AssemblyInstaller_UseNewContext")]
        public bool UseNewContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.useNewContext;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.useNewContext = value;
            }
        }
    }
}

