namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.IO;
    using System.Xml;

    public class NameValueFileSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            object obj2 = parent;
            XmlNode node = section.Attributes.RemoveNamedItem("file");
            obj2 = NameValueSectionHandler.CreateStatic(obj2, section);
            if ((node == null) || (node.Value.Length == 0))
            {
                return obj2;
            }
            string str = null;
            str = node.Value;
            IConfigErrorInfo info = node as IConfigErrorInfo;
            if (info == null)
            {
                return null;
            }
            string path = Path.Combine(Path.GetDirectoryName(info.Filename), str);
            if (!File.Exists(path))
            {
                return obj2;
            }
            ConfigXmlDocument document = new ConfigXmlDocument();
            try
            {
                document.Load(path);
            }
            catch (XmlException exception)
            {
                throw new ConfigurationErrorsException(exception.Message, exception, path, exception.LineNumber);
            }
            if (section.Name != document.DocumentElement.Name)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Config_name_value_file_section_file_invalid_root", new object[] { section.Name }), document.DocumentElement);
            }
            return NameValueSectionHandler.CreateStatic(obj2, document.DocumentElement);
        }
    }
}

