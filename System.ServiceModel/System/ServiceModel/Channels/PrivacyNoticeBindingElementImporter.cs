namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class PrivacyNoticeBindingElementImporter : IPolicyImportExtension
    {
        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            if (policyContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("policyContext");
            }
            XmlElement element = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(), "PrivacyNotice", "http://schemas.xmlsoap.org/ws/2005/05/identity", true);
            if (element != null)
            {
                PrivacyNoticeBindingElement item = policyContext.BindingElements.Find<PrivacyNoticeBindingElement>();
                if (item == null)
                {
                    item = new PrivacyNoticeBindingElement();
                    policyContext.BindingElements.Add(item);
                }
                item.Url = new Uri(element.InnerText);
                string attribute = element.GetAttribute("Version", "http://schemas.xmlsoap.org/ws/2005/05/identity");
                if (string.IsNullOrEmpty(attribute))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotImportPrivacyNoticeElementWithoutVersionAttribute")));
                }
                int result = 0;
                if (!int.TryParse(attribute, out result))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PrivacyNoticeElementVersionAttributeInvalid")));
                }
                item.Version = result;
            }
        }
    }
}

