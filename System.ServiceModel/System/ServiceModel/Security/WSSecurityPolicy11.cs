namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class WSSecurityPolicy11 : WSSecurityPolicy
    {
        public const string WsspNamespace = "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";

        public override XmlElement CreateWsspHttpsTokenAssertion(MetadataExporter exporter, HttpsTransportBindingElement httpsBinding)
        {
            XmlElement element = this.CreateWsspAssertion("HttpsToken");
            element.SetAttribute("RequireClientCertificate", httpsBinding.RequireClientCertificate ? "true" : "false");
            return element;
        }

        public override XmlElement CreateWsspMustNotSendCancelAssertion(bool requireCancel)
        {
            if (!requireCancel)
            {
                return this.CreateMsspAssertion("MustNotSendCancel");
            }
            return null;
        }

        public override XmlElement CreateWsspTrustAssertion(MetadataExporter exporter, SecurityKeyEntropyMode keyEntropyMode)
        {
            return base.CreateWsspTrustAssertion("Trust10", exporter, keyEntropyMode);
        }

        public override MessageSecurityVersion GetSupportedMessageSecurityVersion(SecurityVersion version)
        {
            if (version != SecurityVersion.WSSecurity10)
            {
                return MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
            }
            return MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10;
        }

        public override bool IsSecurityVersionSupported(MessageSecurityVersion version)
        {
            if ((version != MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10) && (version != MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11))
            {
                return (version == MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10);
            }
            return true;
        }

        public override bool TryImportWsspHttpsTokenAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, HttpsTransportBindingElement httpsBinding)
        {
            XmlElement element;
            if (assertions == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assertions");
            }
            if (this.TryImportWsspAssertion(assertions, "HttpsToken", out element))
            {
                bool flag = true;
                string attribute = element.GetAttribute("RequireClientCertificate");
                try
                {
                    httpsBinding.RequireClientCertificate = XmlUtil.IsTrue(attribute);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (exception is NullReferenceException)
                    {
                        throw;
                    }
                    importer.Errors.Add(new MetadataConversionError(System.ServiceModel.SR.GetString("UnsupportedBooleanAttribute", new object[] { "RequireClientCertificate", exception.Message }), false));
                    flag = false;
                }
                return flag;
            }
            return false;
        }

        public override bool TryImportWsspMustNotSendCancelAssertion(ICollection<XmlElement> assertions, out bool requireCancellation)
        {
            requireCancellation = !this.TryImportMsspAssertion(assertions, "MustNotSendCancel");
            return true;
        }

        public override bool TryImportWsspTrustAssertion(MetadataImporter importer, ICollection<XmlElement> assertions, SecurityBindingElement binding, out XmlElement assertion)
        {
            return base.TryImportWsspTrustAssertion("Trust10", importer, assertions, binding, out assertion);
        }

        public override System.ServiceModel.Security.TrustDriver TrustDriver
        {
            get
            {
                return new WSTrustFeb2005.DriverFeb2005(new SecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, WSSecurityTokenSerializer.DefaultInstance));
            }
        }

        public override string WsspNamespaceUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";
            }
        }
    }
}

