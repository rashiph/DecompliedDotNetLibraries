namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed class UseManagedPresentationBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            if ((PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "RequireFederatedIdentityProvisioning", "http://schemas.xmlsoap.org/ws/2005/05/identity", true) != null) && (policyContext.BindingElements.Find<UseManagedPresentationBindingElement>() == null))
            {
                UseManagedPresentationBindingElement item = new UseManagedPresentationBindingElement();
                policyContext.BindingElements.Add(item);
            }
        }
    }
}

