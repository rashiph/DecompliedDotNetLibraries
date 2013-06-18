namespace System.EnterpriseServices.Internal
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal class HomePage
    {
        public void Create(string FilePath, string VirtualRoot, string PageName, string DiscoRef)
        {
            try
            {
                if (!FilePath.EndsWith("/", StringComparison.Ordinal) && !FilePath.EndsWith(@"\", StringComparison.Ordinal))
                {
                    FilePath = FilePath + @"\";
                }
                if (!File.Exists(FilePath + PageName))
                {
                    new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration).Demand();
                    string str = FilePath + "web.config";
                    string str2 = "<%@ Import Namespace=\"System.Collections\" %>\r\n";
                    str2 = ((((((((((((((((((((((((((((((((((((((((((((((((((str2 + "<%@ Import Namespace=\"System.IO\" %>\r\n") + "<%@ Import Namespace=\"System.Xml.Serialization\" %>\r\n" + "<%@ Import Namespace=\"System.Xml\" %>\r\n") + "<%@ Import Namespace=\"System.Xml.Schema\" %>\r\n" + "<%@ Import Namespace=\"System.Web.Services.Description\" %>\r\n") + "<%@ Import Namespace=\"System\" %>\r\n" + "<%@ Import Namespace=\"System.Globalization\" %>\r\n") + "<%@ Import Namespace=\"System.Resources\" %>\r\n" + "<%@ Import Namespace=\"System.Diagnostics\" %>\r\n") + "<html>\r\n" + "<script language=\"C#\" runat=\"server\">\r\n") + "    string soapNs = \"http://schemas.xmlsoap.org/soap/envelope/\";\r\n" + "    string soapEncNs = \"http://schemas.xmlsoap.org/soap/encoding/\";\r\n") + "    string urtNs = \"urn:schemas-microsoft-com:urt-types\";\r\n" + "    string wsdlNs = \"http://schemas.xmlsoap.org/wsdl/\";\r\n") + "    string VRoot = \"" + VirtualRoot + "\";\r\n") + "    string ServiceName() { return VRoot; }\r\n" + "\r\n") + "   XmlNode GetNextNamedSiblingNode(XmlNode inNode, string name)\r\n" + "    {\r\n") + "       if (inNode == null ) return inNode;\r\n" + "      if (inNode.Name == name) return inNode;\r\n") + "       XmlNode newNode = inNode.NextSibling;\r\n" + "       if (newNode == null) return newNode;\r\n") + "       if (newNode.Name == name ) return newNode;\r\n" + "       bool found = false;\r\n") + "       while (!found)\r\n" + "       {\r\n") + "           XmlNode oldNode = newNode;\r\n" + "           newNode = oldNode.NextSibling;\r\n") + "           if (null == newNode || newNode == oldNode)\r\n" + "           {\r\n") + "               newNode = null;\r\n" + "               break;\r\n") + "           }\r\n" + "           if (newNode.Name == name) found = true;\r\n") + "       }\r\n" + "       return newNode;\r\n") + "   }\r\n" + "\r\n") + "   string GetNodes()\r\n" + "   {\r\n") + "       string retval = \"\";\r\n" + "       XmlDocument configXml = new XmlDocument();\r\n") + "      configXml.Load(@\"" + str + "\");\r\n") + "       XmlNode node= configXml.DocumentElement;\r\n" + "        node = GetNextNamedSiblingNode(node,\"configuration\");\r\n") + "        node = GetNextNamedSiblingNode(node.FirstChild, \"system.runtime.remoting\");\r\n" + "        node = GetNextNamedSiblingNode(node.FirstChild, \"application\");\r\n") + "        node = GetNextNamedSiblingNode(node.FirstChild, \"service\");\r\n" + "        node = GetNextNamedSiblingNode(node.FirstChild, \"wellknown\");\r\n") + "       while (node != null)\r\n" + "       {\r\n") + "           XmlNode attribType = node.Attributes.GetNamedItem(\"objectUri\");\r\n" + "           retval += \"<a href=\" + attribType.Value + \"?WSDL>\" + attribType.Value +\"?WSDL</a><br><br>\";\r\n") + "           node = GetNextNamedSiblingNode(node.NextSibling, \"wellknown\");\r\n" + "       }\r\n") + "        return retval;\r\n" + "    }\r\n") + "\r\n" + "</script>\r\n") + "<title><% = ServiceName() %></title>\r\n" + "<head>\r\n") + "<link type='text/xml' rel='alternate' href='" + DiscoRef + "' />\r\n") + "\r\n") + "   <style type=\"text/css\">\r\n" + " \r\n") + "       BODY { color: #000000; background-color: white; font-family: \"Verdana\"; margin-left: 0px; margin-top: 0px; }\r\n" + "       #content { margin-left: 30px; font-size: .70em; padding-bottom: 2em; }\r\n") + "       A:link { color: #336699; font-weight: bold; text-decoration: underline; }\r\n" + "       A:visited { color: #6699cc; font-weight: bold; text-decoration: underline; }\r\n") + "       A:active { color: #336699; font-weight: bold; text-decoration: underline; }\r\n" + "       A:hover { color: cc3300; font-weight: bold; text-decoration: underline; }\r\n") + "       P { color: #000000; margin-top: 0px; margin-bottom: 12px; font-family: \"Verdana\"; }\r\n" + "       pre { background-color: #e5e5cc; padding: 5px; font-family: \"Courier New\"; font-size: x-small; margin-top: -5px; border: 1px #f0f0e0 solid; }\r\n") + "       td { color: #000000; font-family: verdana; font-size: .7em; }\r\n" + "       h2 { font-size: 1.5em; font-weight: bold; margin-top: 25px; margin-bottom: 10px; border-top: 1px solid #003366; margin-left: -15px; color: #003366; }\r\n") + "       h3 { font-size: 1.1em; color: #000000; margin-left: -15px; margin-top: 10px; margin-bottom: 10px; }\r\n" + "       ul, ol { margin-top: 10px; margin-left: 20px; }\r\n") + "       li { margin-top: 10px; color: #000000; }\r\n" + "       font.value { color: darkblue; font: bold; }\r\n") + "       font.key { color: darkgreen; font: bold; }\r\n" + "       .heading1 { color: #ffffff; font-family: \"Tahoma\"; font-size: 26px; font-weight: normal; background-color: #003366; margin-top: 0px; margin-bottom: 0px; margin-left: 0px; padding-top: 10px; padding-bottom: 3px; padding-left: 15px; width: 105%; }\r\n") + "       .button { background-color: #dcdcdc; font-family: \"Verdana\"; font-size: 1em; border-top: #cccccc 1px solid; border-bottom: #666666 1px solid; border-left: #cccccc 1px solid; border-right: #666666 1px solid; }\r\n" + "       .frmheader { color: #000000; background: #dcdcdc; font-family: \"Verdana\"; font-size: .7em; font-weight: normal; border-bottom: 1px solid #dcdcdc; padding-top: 2px; padding-bottom: 2px; }\r\n") + "       .frmtext { font-family: \"Verdana\"; font-size: .7em; margin-top: 8px; margin-bottom: 0px; margin-left: 32px; }\r\n" + "       .frmInput { font-family: \"Verdana\"; font-size: 1em; }\r\n") + "       .intro { margin-left: -15px; }\r\n" + " \r\n") + "    </style>\r\n" + "\r\n") + "</head>\r\n" + "<body>\r\n") + "<p class=\"heading1\"><% = ServiceName() %></p><br>\r\n" + "<% = GetNodes() %>\r\n") + "</body>\r\n" + "</html>\r\n";
                    FileStream stream = new FileStream(FilePath + PageName, FileMode.Create);
                    StreamWriter writer = new StreamWriter(stream);
                    writer.Write(str2);
                    writer.Close();
                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
            }
        }
    }
}

