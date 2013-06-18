namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Configuration;

    [ConfigurationCollection(typeof(SecurityIdentifierElement))]
    public sealed class SecurityIdentifierElementCollection : ServiceModelConfigurationElementCollection<SecurityIdentifierElement>
    {
        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            SecurityIdentifierElement element2 = (SecurityIdentifierElement) element;
            return element2.SecurityIdentifier.Value;
        }

        internal void SetDefaultIdentifiers()
        {
            if (Iis7Helper.IisVersion >= 7)
            {
                base.Add(new SecurityIdentifierElement(new SecurityIdentifier("S-1-5-32-568")));
            }
            base.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null)));
            base.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null)));
            base.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null)));
            base.Add(new SecurityIdentifierElement(new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null)));
        }
    }
}

