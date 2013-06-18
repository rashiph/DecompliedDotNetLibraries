namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Globalization;
    using System.IO;

    internal static class SoapClientConfig
    {
        internal static bool Write(string destinationDirectory, string fullUrl, string assemblyName, string typeName, string progId, string authentication)
        {
            string str3 = (("<configuration>\r\n" + "  <system.runtime.remoting>\r\n" + "    <application>\r\n") + "      <client url=\"" + fullUrl + "\">\r\n") + "        ";
            string str = (str3 + "<activated type=\"" + typeName + ", " + assemblyName + "\"/>\r\n") + "      </client>\r\n";
            if (authentication.ToLower(CultureInfo.InvariantCulture) == "windows")
            {
                str = (str + "      <channels>\r\n") + "        <channel ref=\"http\" useDefaultCredentials=\"true\" />\r\n" + "      </channels>\r\n";
            }
            str = (str + "    </application>\r\n") + "  </system.runtime.remoting>\r\n" + "</configuration>\r\n";
            string path = destinationDirectory;
            if ((path.Length > 0) && !path.EndsWith(@"\", StringComparison.Ordinal))
            {
                path = path + @"\";
            }
            path = path + typeName + ".config";
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
    }
}

