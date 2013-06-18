namespace System.Workflow.ComponentModel.Compiler
{
    using Microsoft.Build.Tasks;
    using System;
    using System.IO;
    using System.Xml;

    internal static class TasksHelper
    {
        internal static string GetXomlManifestName(string fileName, string linkFileName, string rootNamespace, Stream binaryStream)
        {
            string str = string.Empty;
            string name = linkFileName;
            if ((name == null) || (name.Length == 0))
            {
                name = fileName;
            }
            System.Workflow.ComponentModel.Compiler.Culture.ItemCultureInfo itemCultureInfo = System.Workflow.ComponentModel.Compiler.Culture.GetItemCultureInfo(name);
            if (binaryStream != null)
            {
                string str3 = null;
                try
                {
                    XmlTextReader reader = new XmlTextReader(binaryStream);
                    if ((reader.MoveToContent() == XmlNodeType.Element) && reader.MoveToAttribute("Class", "http://schemas.microsoft.com/winfx/2006/xaml"))
                    {
                        str3 = reader.Value;
                    }
                }
                catch
                {
                }
                if ((str3 != null) && (str3.Length > 0))
                {
                    str = str3;
                    if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                    {
                        str = str + "." + itemCultureInfo.culture;
                    }
                }
            }
            if (str.Length == 0)
            {
                if (!string.IsNullOrEmpty(rootNamespace))
                {
                    str = rootNamespace + ".";
                }
                string str4 = CreateManifestResourceName.MakeValidEverettIdentifier(Path.GetDirectoryName(itemCultureInfo.cultureNeutralFilename));
                if (string.Compare(Path.GetExtension(itemCultureInfo.cultureNeutralFilename), ".resx", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    str = (str + Path.Combine(str4, Path.GetFileNameWithoutExtension(itemCultureInfo.cultureNeutralFilename))).Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
                    if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                    {
                        str = str + "." + itemCultureInfo.culture;
                    }
                    return str;
                }
                str = (str + Path.Combine(str4, Path.GetFileName(itemCultureInfo.cultureNeutralFilename))).Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
                if ((itemCultureInfo.culture != null) && (itemCultureInfo.culture.Length > 0))
                {
                    str = itemCultureInfo.culture + Path.DirectorySeparatorChar + str;
                }
            }
            return str;
        }
    }
}

