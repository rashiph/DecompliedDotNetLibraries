namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    public class ClientRemotingConfig
    {
        private const string indent = "  ";

        public static bool Write(string DestinationDirectory, string VRoot, string BaseUrl, string AssemblyName, string TypeName, string ProgId, string Mode, string Transport)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Demand();
                string str = "<configuration>\r\n";
                str = str + "  <system.runtime.remoting>\r\n" + "    <application>\r\n";
                string str2 = BaseUrl;
                if ((str2.Length > 0) && !str2.EndsWith("/", StringComparison.Ordinal))
                {
                    str2 = str2 + "/";
                }
                str2 = str2 + VRoot;
                str = str + "      <client url=\"" + str2 + "\">\r\n";
                if ((Mode.Length <= 0) || ("WELLKNOWNOBJECT" == Mode.ToUpper(CultureInfo.InvariantCulture)))
                {
                    string str4 = str + "        ";
                    str = str4 + "<wellknown type=\"" + TypeName + ", " + AssemblyName + "\" url=\"" + str2;
                    if (!str2.EndsWith("/", StringComparison.Ordinal))
                    {
                        str = str + "/";
                    }
                    str = str + ProgId + ".soap\" />\r\n";
                }
                else
                {
                    string str5 = str + "        ";
                    str = str5 + "<activated type=\"" + TypeName + ", " + AssemblyName + "\"/>\r\n";
                }
                str = (str + "      </client>\r\n" + "    </application>\r\n") + "  </system.runtime.remoting>\r\n" + "</configuration>\r\n";
                string path = DestinationDirectory;
                if ((path.Length > 0) && !path.EndsWith(@"\", StringComparison.Ordinal))
                {
                    path = path + @"\";
                }
                path = path + TypeName + ".config";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                FileStream stream = new FileStream(path, FileMode.Create);
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(str);
                writer.Close();
                stream.Close();
                return true;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                return false;
            }
        }
    }
}

