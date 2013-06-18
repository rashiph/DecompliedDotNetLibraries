namespace System.IdentityModel
{
    using System;
    using System.Xml;

    internal class SamlDictionary
    {
        public XmlDictionaryString Access;
        public XmlDictionaryString AccessDecision;
        public XmlDictionaryString Action;
        public XmlDictionaryString ActionNamespaceAttribute;
        public XmlDictionaryString Advice;
        public XmlDictionaryString Assertion;
        public XmlDictionaryString AssertionId;
        public XmlDictionaryString AssertionIdReference;
        public XmlDictionaryString Attribute;
        public XmlDictionaryString AttributeName;
        public XmlDictionaryString AttributeNamespace;
        public XmlDictionaryString AttributeStatement;
        public XmlDictionaryString AttributeValue;
        public XmlDictionaryString Audience;
        public XmlDictionaryString AudienceRestrictionCondition;
        public XmlDictionaryString AuthenticationInstant;
        public XmlDictionaryString AuthenticationMethod;
        public XmlDictionaryString AuthenticationStatement;
        public XmlDictionaryString AuthorityBinding;
        public XmlDictionaryString AuthorityKind;
        public XmlDictionaryString AuthorizationDecisionStatement;
        public XmlDictionaryString Binding;
        public XmlDictionaryString Condition;
        public XmlDictionaryString Conditions;
        public XmlDictionaryString Decision;
        public XmlDictionaryString DoNotCacheCondition;
        public XmlDictionaryString EmailName;
        public XmlDictionaryString EmailNamespace;
        public XmlDictionaryString Evidence;
        public XmlDictionaryString HolderOfKey;
        public XmlDictionaryString IssueInstant;
        public XmlDictionaryString Issuer;
        public XmlDictionaryString Location;
        public XmlDictionaryString MajorVersion;
        public XmlDictionaryString MinorVersion;
        public XmlDictionaryString NameIdentifier;
        public XmlDictionaryString NameIdentifierFormat;
        public XmlDictionaryString NameIdentifierNameQualifier;
        public XmlDictionaryString Namespace;
        public XmlDictionaryString NamespaceAttributePrefix;
        public XmlDictionaryString NotBefore;
        public XmlDictionaryString NotOnOrAfter;
        public XmlDictionaryString PreferredPrefix;
        public XmlDictionaryString Resource;
        public XmlDictionaryString SenderVouches;
        public XmlDictionaryString Statement;
        public XmlDictionaryString Subject;
        public XmlDictionaryString SubjectConfirmation;
        public XmlDictionaryString SubjectConfirmationData;
        public XmlDictionaryString SubjectConfirmationMethod;
        public XmlDictionaryString SubjectLocality;
        public XmlDictionaryString SubjectLocalityDNSAddress;
        public XmlDictionaryString SubjectLocalityIPAddress;
        public XmlDictionaryString SubjectStatement;
        public XmlDictionaryString UnspecifiedAuthenticationMethod;
        public XmlDictionaryString UserName;
        public XmlDictionaryString UserNameNamespace;

        public SamlDictionary(IdentityModelDictionary dictionary)
        {
            this.Access = dictionary.CreateString("Access", 0x18);
            this.AccessDecision = dictionary.CreateString("AccessDecision", 0x19);
            this.Action = dictionary.CreateString("Action", 0x1a);
            this.Advice = dictionary.CreateString("Advice", 0x1b);
            this.Assertion = dictionary.CreateString("Assertion", 0x1c);
            this.AssertionId = dictionary.CreateString("AssertionID", 0x1d);
            this.AssertionIdReference = dictionary.CreateString("AssertionIDReference", 30);
            this.Attribute = dictionary.CreateString("Attribute", 0x1f);
            this.AttributeName = dictionary.CreateString("AttributeName", 0x20);
            this.AttributeNamespace = dictionary.CreateString("AttributeNamespace", 0x21);
            this.AttributeStatement = dictionary.CreateString("AttributeStatement", 0x22);
            this.AttributeValue = dictionary.CreateString("AttributeValue", 0x23);
            this.Audience = dictionary.CreateString("Audience", 0x24);
            this.AudienceRestrictionCondition = dictionary.CreateString("AudienceRestrictionCondition", 0x25);
            this.AuthenticationInstant = dictionary.CreateString("AuthenticationInstant", 0x26);
            this.AuthenticationMethod = dictionary.CreateString("AuthenticationMethod", 0x27);
            this.AuthenticationStatement = dictionary.CreateString("AuthenticationStatement", 40);
            this.AuthorityBinding = dictionary.CreateString("AuthorityBinding", 0x29);
            this.AuthorityKind = dictionary.CreateString("AuthorityKind", 0x2a);
            this.AuthorizationDecisionStatement = dictionary.CreateString("AuthorizationDecisionStatement", 0x2b);
            this.Binding = dictionary.CreateString("Binding", 0x2c);
            this.Condition = dictionary.CreateString("Condition", 0x2d);
            this.Conditions = dictionary.CreateString("Conditions", 0x2e);
            this.Decision = dictionary.CreateString("Decision", 0x2f);
            this.DoNotCacheCondition = dictionary.CreateString("DoNotCacheCondition", 0x30);
            this.Evidence = dictionary.CreateString("Evidence", 0x31);
            this.IssueInstant = dictionary.CreateString("IssueInstant", 50);
            this.Issuer = dictionary.CreateString("Issuer", 0x33);
            this.Location = dictionary.CreateString("Location", 0x34);
            this.MajorVersion = dictionary.CreateString("MajorVersion", 0x35);
            this.MinorVersion = dictionary.CreateString("MinorVersion", 0x36);
            this.Namespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:assertion", 0x37);
            this.NameIdentifier = dictionary.CreateString("NameIdentifier", 0x38);
            this.NameIdentifierFormat = dictionary.CreateString("Format", 0x39);
            this.NameIdentifierNameQualifier = dictionary.CreateString("NameQualifier", 0x3a);
            this.ActionNamespaceAttribute = dictionary.CreateString("Namespace", 0x3b);
            this.NotBefore = dictionary.CreateString("NotBefore", 60);
            this.NotOnOrAfter = dictionary.CreateString("NotOnOrAfter", 0x3d);
            this.PreferredPrefix = dictionary.CreateString("saml", 0x3e);
            this.Statement = dictionary.CreateString("Statement", 0x3f);
            this.Subject = dictionary.CreateString("Subject", 0x40);
            this.SubjectConfirmation = dictionary.CreateString("SubjectConfirmation", 0x41);
            this.SubjectConfirmationData = dictionary.CreateString("SubjectConfirmationData", 0x42);
            this.SubjectConfirmationMethod = dictionary.CreateString("ConfirmationMethod", 0x43);
            this.HolderOfKey = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:cm:holder-of-key", 0x44);
            this.SenderVouches = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:cm:sender-vouches", 0x45);
            this.SubjectLocality = dictionary.CreateString("SubjectLocality", 70);
            this.SubjectLocalityDNSAddress = dictionary.CreateString("DNSAddress", 0x47);
            this.SubjectLocalityIPAddress = dictionary.CreateString("IPAddress", 0x48);
            this.SubjectStatement = dictionary.CreateString("SubjectStatement", 0x49);
            this.UnspecifiedAuthenticationMethod = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:am:unspecified", 0x4a);
            this.NamespaceAttributePrefix = dictionary.CreateString("xmlns", 0x4b);
            this.Resource = dictionary.CreateString("Resource", 0x4c);
            this.UserName = dictionary.CreateString("UserName", 0x4d);
            this.UserNameNamespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName", 0x4e);
            this.EmailName = dictionary.CreateString("EmailName", 0x4f);
            this.EmailNamespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress", 80);
        }

        public SamlDictionary(IXmlDictionary dictionary)
        {
            this.Access = this.LookupDictionaryString(dictionary, "Access");
            this.AccessDecision = this.LookupDictionaryString(dictionary, "AccessDecision");
            this.Action = this.LookupDictionaryString(dictionary, "Action");
            this.Advice = this.LookupDictionaryString(dictionary, "Advice");
            this.Assertion = this.LookupDictionaryString(dictionary, "Assertion");
            this.AssertionId = this.LookupDictionaryString(dictionary, "AssertionID");
            this.AssertionIdReference = this.LookupDictionaryString(dictionary, "AssertionIDReference");
            this.Attribute = this.LookupDictionaryString(dictionary, "Attribute");
            this.AttributeName = this.LookupDictionaryString(dictionary, "AttributeName");
            this.AttributeNamespace = this.LookupDictionaryString(dictionary, "AttributeNamespace");
            this.AttributeStatement = this.LookupDictionaryString(dictionary, "AttributeStatement");
            this.AttributeValue = this.LookupDictionaryString(dictionary, "AttributeValue");
            this.Audience = this.LookupDictionaryString(dictionary, "Audience");
            this.AudienceRestrictionCondition = this.LookupDictionaryString(dictionary, "AudienceRestrictionCondition");
            this.AuthenticationInstant = this.LookupDictionaryString(dictionary, "AuthenticationInstant");
            this.AuthenticationMethod = this.LookupDictionaryString(dictionary, "AuthenticationMethod");
            this.AuthenticationStatement = this.LookupDictionaryString(dictionary, "AuthenticationStatement");
            this.AuthorityBinding = this.LookupDictionaryString(dictionary, "AuthorityBinding");
            this.AuthorityKind = this.LookupDictionaryString(dictionary, "AuthorityKind");
            this.AuthorizationDecisionStatement = this.LookupDictionaryString(dictionary, "AuthorizationDecisionStatement");
            this.Binding = this.LookupDictionaryString(dictionary, "Binding");
            this.Condition = this.LookupDictionaryString(dictionary, "Condition");
            this.Conditions = this.LookupDictionaryString(dictionary, "Conditions");
            this.Decision = this.LookupDictionaryString(dictionary, "Decision");
            this.DoNotCacheCondition = this.LookupDictionaryString(dictionary, "DoNotCacheCondition");
            this.Evidence = this.LookupDictionaryString(dictionary, "Evidence");
            this.IssueInstant = this.LookupDictionaryString(dictionary, "IssueInstant");
            this.Issuer = this.LookupDictionaryString(dictionary, "Issuer");
            this.Location = this.LookupDictionaryString(dictionary, "Location");
            this.MajorVersion = this.LookupDictionaryString(dictionary, "MajorVersion");
            this.MinorVersion = this.LookupDictionaryString(dictionary, "MinorVersion");
            this.Namespace = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.0:assertion");
            this.NameIdentifier = this.LookupDictionaryString(dictionary, "NameIdentifier");
            this.NameIdentifierFormat = this.LookupDictionaryString(dictionary, "Format");
            this.NameIdentifierNameQualifier = this.LookupDictionaryString(dictionary, "NameQualifier");
            this.ActionNamespaceAttribute = this.LookupDictionaryString(dictionary, "Namespace");
            this.NotBefore = this.LookupDictionaryString(dictionary, "NotBefore");
            this.NotOnOrAfter = this.LookupDictionaryString(dictionary, "NotOnOrAfter");
            this.PreferredPrefix = this.LookupDictionaryString(dictionary, "saml");
            this.Statement = this.LookupDictionaryString(dictionary, "Statement");
            this.Subject = this.LookupDictionaryString(dictionary, "Subject");
            this.SubjectConfirmation = this.LookupDictionaryString(dictionary, "SubjectConfirmation");
            this.SubjectConfirmationData = this.LookupDictionaryString(dictionary, "SubjectConfirmationData");
            this.SubjectConfirmationMethod = this.LookupDictionaryString(dictionary, "ConfirmationMethod");
            this.HolderOfKey = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.0:cm:holder-of-key");
            this.SenderVouches = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.0:cm:sender-vouches");
            this.SubjectLocality = this.LookupDictionaryString(dictionary, "SubjectLocality");
            this.SubjectLocalityDNSAddress = this.LookupDictionaryString(dictionary, "DNSAddress");
            this.SubjectLocalityIPAddress = this.LookupDictionaryString(dictionary, "IPAddress");
            this.SubjectStatement = this.LookupDictionaryString(dictionary, "SubjectStatement");
            this.UnspecifiedAuthenticationMethod = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.0:am:unspecified");
            this.NamespaceAttributePrefix = this.LookupDictionaryString(dictionary, "xmlns");
            this.Resource = this.LookupDictionaryString(dictionary, "Resource");
            this.UserName = this.LookupDictionaryString(dictionary, "UserName");
            this.UserNameNamespace = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName");
            this.EmailName = this.LookupDictionaryString(dictionary, "EmailName");
            this.EmailNamespace = this.LookupDictionaryString(dictionary, "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress");
        }

        private XmlDictionaryString LookupDictionaryString(IXmlDictionary dictionary, string value)
        {
            XmlDictionaryString str;
            if (!dictionary.TryLookup(value, out str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.IdentityModel.SR.GetString("XDCannotFindValueInDictionaryString", new object[] { value }));
            }
            return str;
        }
    }
}

