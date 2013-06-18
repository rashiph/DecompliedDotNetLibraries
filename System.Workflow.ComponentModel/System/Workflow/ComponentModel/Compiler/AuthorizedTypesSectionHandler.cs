namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class AuthorizedTypesSectionHandler : IConfigurationSectionHandler
    {
        private const string TargetFxVersionAttribute = "version";

        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            Dictionary<string, IList<AuthorizedType>> dictionary = new Dictionary<string, IList<AuthorizedType>>();
            XmlSerializer serializer = new XmlSerializer(typeof(AuthorizedType));
            foreach (XmlNode node in section.ChildNodes)
            {
                XmlAttribute namedItem = node.Attributes.GetNamedItem("version") as XmlAttribute;
                if (namedItem != null)
                {
                    string str = namedItem.Value;
                    if (!string.IsNullOrEmpty(str))
                    {
                        IList<AuthorizedType> list;
                        if (!dictionary.TryGetValue(str, out list))
                        {
                            list = new List<AuthorizedType>();
                            dictionary.Add(str, list);
                        }
                        foreach (XmlNode node2 in node.ChildNodes)
                        {
                            AuthorizedType item = serializer.Deserialize(new XmlNodeReader(node2)) as AuthorizedType;
                            if (item != null)
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }
    }
}

