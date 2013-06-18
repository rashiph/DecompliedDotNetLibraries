namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public class OneWayBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (importer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("importer");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            XmlElement element = PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "OneWay", "http://schemas.microsoft.com/ws/2005/05/routing/policy", true);
            if (element != null)
            {
                OneWayBindingElement item = new OneWayBindingElement();
                context.BindingElements.Add(item);
                for (int i = 0; i < element.ChildNodes.Count; i++)
                {
                    System.Xml.XmlNode node = element.ChildNodes[i];
                    if (((node != null) && (node.NodeType == XmlNodeType.Element)) && ((node.NamespaceURI == "http://schemas.microsoft.com/ws/2005/05/routing/policy") && (node.LocalName == "PacketRoutable")))
                    {
                        item.PacketRoutable = true;
                        return;
                    }
                }
            }
            else if (WsdlImporter.WSAddressingHelper.DetermineSupportedAddressingMode(importer, context) == SupportedAddressingMode.NonAnonymous)
            {
                context.BindingElements.Add(new OneWayBindingElement());
            }
        }
    }
}

