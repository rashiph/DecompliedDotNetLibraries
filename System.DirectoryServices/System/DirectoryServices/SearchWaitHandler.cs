namespace System.DirectoryServices
{
    using System;
    using System.Configuration;
    using System.Xml;

    internal class SearchWaitHandler : IConfigurationSectionHandler
    {
        public virtual object Create(object parent, object configContext, XmlNode section)
        {
            bool flag = false;
            bool flag2 = false;
            foreach (XmlNode node in section.ChildNodes)
            {
                string str;
                if (((str = node.Name) != null) && (str == "DirectorySearcher"))
                {
                    if (flag)
                    {
                        throw new ConfigurationErrorsException(System.DirectoryServices.Res.GetString("ConfigSectionsUnique", new object[] { "DirectorySearcher" }));
                    }
                    System.DirectoryServices.HandlerBase.RemoveBooleanAttribute(node, "waitForPagedSearchData", ref flag2);
                    flag = true;
                }
            }
            return flag2;
        }
    }
}

