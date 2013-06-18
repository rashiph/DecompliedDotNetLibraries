namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Guid("3b0398c9-7812-4007-85cb-18c771f2206f")]
    public class ComManagedImportUtil : IComManagedImportUtil
    {
        public void GetComponentInfo(string assemblyPath, out string numComponents, out string componentInfo)
        {
            RegistrationServices services = new RegistrationServices();
            Assembly assembly = this.LoadAssembly(assemblyPath);
            Type[] registrableTypesInAssembly = services.GetRegistrableTypesInAssembly(assembly);
            int num = 0;
            string str = "";
            foreach (Type type in registrableTypesInAssembly)
            {
                if (type.IsClass && type.IsSubclassOf(typeof(ServicedComponent)))
                {
                    num++;
                    string str2 = Marshal.GenerateGuidForType(type).ToString();
                    string str3 = Marshal.GenerateProgIdForType(type);
                    if ((str2.Length == 0) || (str3.Length == 0))
                    {
                        throw new COMException();
                    }
                    string str4 = str;
                    str = str4 + str3 + ",{" + str2 + "},";
                }
            }
            numComponents = num.ToString(CultureInfo.InvariantCulture);
            componentInfo = str;
        }

        public void InstallAssembly(string asmpath, string parname, string appname)
        {
            try
            {
                string tlb = null;
                InstallationFlags installFlags = InstallationFlags.Default;
                new RegistrationHelper().InstallAssembly(asmpath, ref appname, parname, ref tlb, installFlags);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                EventLog.WriteEntry(Resource.FormatString("Reg_InstallTitle"), Resource.FormatString("Reg_FailInstall", asmpath, appname) + "\n\n" + exception.ToString(), EventLogEntryType.Error);
                throw;
            }
        }

        private Assembly LoadAssembly(string assemblyFile)
        {
            string path = Path.GetFullPath(assemblyFile).ToLower(CultureInfo.InvariantCulture);
            bool flag = false;
            string directoryName = Path.GetDirectoryName(path);
            string currentDirectory = Environment.CurrentDirectory;
            if (currentDirectory != directoryName)
            {
                Environment.CurrentDirectory = directoryName;
                flag = true;
            }
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(path);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
            }
            if (flag)
            {
                Environment.CurrentDirectory = currentDirectory;
            }
            return assembly;
        }
    }
}

