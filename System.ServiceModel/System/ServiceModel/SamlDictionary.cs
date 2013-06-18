namespace System.ServiceModel
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

        public SamlDictionary(ServiceModelDictionary dictionary)
        {
            this.Access = dictionary.CreateString("Access", 0xfb);
            this.AccessDecision = dictionary.CreateString("AccessDecision", 0xfc);
            this.Action = dictionary.CreateString("Action", 5);
            this.Advice = dictionary.CreateString("Advice", 0xfd);
            this.Assertion = dictionary.CreateString("Assertion", 0xb3);
            this.AssertionId = dictionary.CreateString("AssertionID", 0xfe);
            this.AssertionIdReference = dictionary.CreateString("AssertionIDReference", 0xff);
            this.Attribute = dictionary.CreateString("Attribute", 0x100);
            this.AttributeName = dictionary.CreateString("AttributeName", 0x101);
            this.AttributeNamespace = dictionary.CreateString("AttributeNamespace", 0x102);
            this.AttributeStatement = dictionary.CreateString("AttributeStatement", 0x103);
            this.AttributeValue = dictionary.CreateString("AttributeValue", 260);
            this.Audience = dictionary.CreateString("Audience", 0x105);
            this.AudienceRestrictionCondition = dictionary.CreateString("AudienceRestrictionCondition", 0x106);
            this.AuthenticationInstant = dictionary.CreateString("AuthenticationInstant", 0x107);
            this.AuthenticationMethod = dictionary.CreateString("AuthenticationMethod", 0x108);
            this.AuthenticationStatement = dictionary.CreateString("AuthenticationStatement", 0x109);
            this.AuthorityBinding = dictionary.CreateString("AuthorityBinding", 0x10a);
            this.AuthorityKind = dictionary.CreateString("AuthorityKind", 0x10b);
            this.AuthorizationDecisionStatement = dictionary.CreateString("AuthorizationDecisionStatement", 0x10c);
            this.Binding = dictionary.CreateString("Binding", 0x10d);
            this.Condition = dictionary.CreateString("Condition", 270);
            this.Conditions = dictionary.CreateString("Conditions", 0x10f);
            this.Decision = dictionary.CreateString("Decision", 0x110);
            this.DoNotCacheCondition = dictionary.CreateString("DoNotCacheCondition", 0x111);
            this.Evidence = dictionary.CreateString("Evidence", 0x112);
            this.IssueInstant = dictionary.CreateString("IssueInstant", 0x113);
            this.Issuer = dictionary.CreateString("Issuer", 0x114);
            this.Location = dictionary.CreateString("Location", 0x115);
            this.MajorVersion = dictionary.CreateString("MajorVersion", 0x116);
            this.MinorVersion = dictionary.CreateString("MinorVersion", 0x117);
            this.Namespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:assertion", 180);
            this.NameIdentifier = dictionary.CreateString("NameIdentifier", 280);
            this.NameIdentifierFormat = dictionary.CreateString("Format", 0x119);
            this.NameIdentifierNameQualifier = dictionary.CreateString("NameQualifier", 0x11a);
            this.ActionNamespaceAttribute = dictionary.CreateString("Namespace", 0x11b);
            this.NotBefore = dictionary.CreateString("NotBefore", 0x11c);
            this.NotOnOrAfter = dictionary.CreateString("NotOnOrAfter", 0x11d);
            this.PreferredPrefix = dictionary.CreateString("saml", 0x11e);
            this.Statement = dictionary.CreateString("Statement", 0x11f);
            this.Subject = dictionary.CreateString("Subject", 0x120);
            this.SubjectConfirmation = dictionary.CreateString("SubjectConfirmation", 0x121);
            this.SubjectConfirmationData = dictionary.CreateString("SubjectConfirmationData", 290);
            this.SubjectConfirmationMethod = dictionary.CreateString("ConfirmationMethod", 0x123);
            this.HolderOfKey = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:cm:holder-of-key", 0x124);
            this.SenderVouches = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:cm:sender-vouches", 0x125);
            this.SubjectLocality = dictionary.CreateString("SubjectLocality", 0x126);
            this.SubjectLocalityDNSAddress = dictionary.CreateString("DNSAddress", 0x127);
            this.SubjectLocalityIPAddress = dictionary.CreateString("IPAddress", 0x128);
            this.SubjectStatement = dictionary.CreateString("SubjectStatement", 0x129);
            this.UnspecifiedAuthenticationMethod = dictionary.CreateString("urn:oasis:names:tc:SAML:1.0:am:unspecified", 0x12a);
            this.NamespaceAttributePrefix = dictionary.CreateString("xmlns", 0x12b);
            this.Resource = dictionary.CreateString("Resource", 300);
            this.UserName = dictionary.CreateString("UserName", 0x12d);
            this.UserNameNamespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.1:nameid-format:WindowsDomainQualifiedName", 0x12e);
            this.EmailName = dictionary.CreateString("EmailName", 0x12f);
            this.EmailNamespace = dictionary.CreateString("urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress", 0x130);
        }
    }
}

