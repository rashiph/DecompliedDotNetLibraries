namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.Xml;

    [ConfigurationCollection(typeof(XmlElementElement), AddItemName="xmlElement", CollectionType=ConfigurationElementCollectionType.BasicMap)]
    public sealed class XmlElementElementCollection : ServiceModelConfigurationElementCollection<XmlElementElement>
    {
        public XmlElementElementCollection() : base(ConfigurationElementCollectionType.BasicMap, "xmlElement")
        {
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((XmlElementElement) element).XmlElement.OuterXml;
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            XmlDocument document = new XmlDocument();
            base.Add(new XmlElementElement((XmlElement) document.ReadNode(reader)));
            return true;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement != null)
            {
                XmlElementElementCollection elements = (XmlElementElementCollection) sourceElement;
                XmlElementElementCollection elements2 = (XmlElementElementCollection) parentElement;
                for (int i = 0; i < elements.Count; i++)
                {
                    XmlElementElement element = elements[i];
                    if ((elements2 == null) || !elements2.ContainsKey(this.GetElementKey(element)))
                    {
                        XmlElementElement element2 = new XmlElementElement();
                        element2.ResetInternal(element);
                        base.Add(element2);
                    }
                }
            }
        }
    }
}

