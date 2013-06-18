namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Collections;
    using System.EnterpriseServices;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security.Permissions;

    [Guid("ecabafd1-7f19-11d2-978e-0000f8757e2a")]
    public class ClrObjectFactory : IClrObjectFactory
    {
        private static Hashtable _htTypes = new Hashtable();

        public object CreateFromAssembly(string AssemblyName, string TypeName, string Mode)
        {
            object obj3;
            try
            {
                Assembly assembly;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                if (AssemblyName.StartsWith("System.EnterpriseServices", StringComparison.Ordinal))
                {
                    return null;
                }
                string path = Publish.GetClientPhysicalPath(false) + TypeName + ".config";
                if (File.Exists(path))
                {
                    lock (_htTypes)
                    {
                        if (!_htTypes.ContainsKey(path))
                        {
                            RemotingConfiguration.Configure(path, false);
                            _htTypes.Add(path, path);
                        }
                        goto Label_0092;
                    }
                }
                throw new COMException(Resource.FormatString("Err_ClassNotReg"), -2147221164);
            Label_0092:
                assembly = Assembly.Load(AssemblyName);
                if (null == assembly)
                {
                    throw new COMException(Resource.FormatString("Err_ClassNotReg"), -2147221164);
                }
                object obj2 = assembly.CreateInstance(TypeName);
                if (obj2 == null)
                {
                    throw new COMException(Resource.FormatString("Err_ClassNotReg"), -2147221164);
                }
                obj3 = obj2;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
            return obj3;
        }

        public object CreateFromMailbox(string Mailbox, string Mode)
        {
            string s = Resource.FormatString("Soap_SmtpNotImplemented");
            ComSoapPublishError.Report(s);
            throw new COMException(s);
        }

        public object CreateFromVroot(string VrootUrl, string Mode)
        {
            string wsdlUrl = VrootUrl + "?wsdl";
            return this.CreateFromWsdl(wsdlUrl, Mode);
        }

        public object CreateFromWsdl(string WsdlUrl, string Mode)
        {
            object obj3;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                string clientPhysicalPath = Publish.GetClientPhysicalPath(true);
                string typeName = "";
                string str3 = this.Url2File(WsdlUrl);
                if ((str3.Length + clientPhysicalPath.Length) > 250)
                {
                    str3 = str3.Remove(0, (str3.Length + clientPhysicalPath.Length) - 250);
                }
                string fileName = str3 + ".dll";
                if (!File.Exists(clientPhysicalPath + fileName))
                {
                    new GenAssemblyFromWsdl().Run(WsdlUrl, fileName, clientPhysicalPath);
                }
                Assembly assembly = Assembly.LoadFrom(clientPhysicalPath + fileName);
                Type[] types = assembly.GetTypes();
                for (long i = 0L; i < types.GetLength(0); i += 1L)
                {
                    if (types[(int) ((IntPtr) i)].IsClass)
                    {
                        typeName = types[(int) ((IntPtr) i)].ToString();
                    }
                }
                obj3 = assembly.CreateInstance(typeName);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
            return obj3;
        }

        private string Url2File(string InUrl)
        {
            string str = InUrl;
            return str.Replace("/", "0").Replace(":", "1").Replace("?", "2").Replace(@"\", "3").Replace(".", "4").Replace("\"", "5").Replace("'", "6").Replace(" ", "7").Replace(";", "8").Replace("=", "9").Replace("|", "A").Replace("<", "[").Replace(">", "]");
        }
    }
}

