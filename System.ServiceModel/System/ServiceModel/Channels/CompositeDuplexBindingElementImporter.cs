namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public class CompositeDuplexBindingElementImporter : IPolicyImportExtension
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
            if ((PolicyConversionContext.FindAssertion(context.GetBindingAssertions(), "CompositeDuplex", "http://schemas.microsoft.com/net/2006/06/duplex", true) != null) || (WsdlImporter.WSAddressingHelper.DetermineSupportedAddressingMode(importer, context) == SupportedAddressingMode.NonAnonymous))
            {
                context.BindingElements.Add(new CompositeDuplexBindingElement());
            }
        }
    }
}

