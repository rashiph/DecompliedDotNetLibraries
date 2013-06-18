namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;

    internal sealed class MessageSecurityTokenVersion : SecurityTokenVersion
    {
        private const string bsp10ns = "http://ws-i.org/profiles/basic-security/core/1.0";
        private bool emitBspRequiredAttributes;
        private System.ServiceModel.Security.SecureConversationVersion secureConversationVersion;
        private System.ServiceModel.Security.SecurityVersion securityVersion;
        private ReadOnlyCollection<string> supportedSpecs;
        private string toString;
        private System.ServiceModel.Security.TrustVersion trustVersion;
        private static MessageSecurityTokenVersion wss10bsp10 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity10, System.ServiceModel.Security.TrustVersion.WSTrustFeb2005, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversationFeb2005, "WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10", true, new string[] { XD.SecurityJan2004Dictionary.Namespace.Value, XD.TrustFeb2005Dictionary.Namespace.Value, XD.SecureConversationFeb2005Dictionary.Namespace.Value, "http://ws-i.org/profiles/basic-security/core/1.0" });
        private static MessageSecurityTokenVersion wss10oasisdec2005bsp10 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity10, System.ServiceModel.Security.TrustVersion.WSTrust13, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversation13, "WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10", true, new string[] { XD.SecurityXXX2005Dictionary.Namespace.Value, DXD.TrustDec2005Dictionary.Namespace.Value, DXD.SecureConversationDec2005Dictionary.Namespace.Value });
        private static MessageSecurityTokenVersion wss11 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity11, System.ServiceModel.Security.TrustVersion.WSTrustFeb2005, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversationFeb2005, "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005", false, new string[] { XD.SecurityXXX2005Dictionary.Namespace.Value, XD.TrustFeb2005Dictionary.Namespace.Value, XD.SecureConversationFeb2005Dictionary.Namespace.Value });
        private static MessageSecurityTokenVersion wss11bsp10 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity11, System.ServiceModel.Security.TrustVersion.WSTrustFeb2005, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversationFeb2005, "WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10", true, new string[] { XD.SecurityXXX2005Dictionary.Namespace.Value, XD.TrustFeb2005Dictionary.Namespace.Value, XD.SecureConversationFeb2005Dictionary.Namespace.Value, "http://ws-i.org/profiles/basic-security/core/1.0" });
        private static MessageSecurityTokenVersion wss11oasisdec2005 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity11, System.ServiceModel.Security.TrustVersion.WSTrust13, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversation13, "WSSecurity11WSTrust13WSSecureConversation13", false, new string[] { XD.SecurityJan2004Dictionary.Namespace.Value, DXD.TrustDec2005Dictionary.Namespace.Value, DXD.SecureConversationDec2005Dictionary.Namespace.Value });
        private static MessageSecurityTokenVersion wss11oasisdec2005bsp10 = new MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion.WSSecurity11, System.ServiceModel.Security.TrustVersion.WSTrust13, System.ServiceModel.Security.SecureConversationVersion.WSSecureConversation13, "WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10", true, new string[] { XD.SecurityXXX2005Dictionary.Namespace.Value, DXD.TrustDec2005Dictionary.Namespace.Value, DXD.SecureConversationDec2005Dictionary.Namespace.Value });

        private MessageSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion securityVersion, System.ServiceModel.Security.TrustVersion trustVersion, System.ServiceModel.Security.SecureConversationVersion secureConversationVersion, string toString, bool emitBspRequiredAttributes, params string[] supportedSpecs)
        {
            this.emitBspRequiredAttributes = emitBspRequiredAttributes;
            this.supportedSpecs = new ReadOnlyCollection<string>(supportedSpecs);
            this.toString = toString;
            this.securityVersion = securityVersion;
            this.trustVersion = trustVersion;
            this.secureConversationVersion = secureConversationVersion;
        }

        public override ReadOnlyCollection<string> GetSecuritySpecifications()
        {
            return this.supportedSpecs;
        }

        public static MessageSecurityTokenVersion GetSecurityTokenVersion(System.ServiceModel.Security.SecurityVersion version, bool emitBspAttributes)
        {
            if (version == System.ServiceModel.Security.SecurityVersion.WSSecurity10)
            {
                if (!emitBspAttributes)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                return WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
            }
            if (version != System.ServiceModel.Security.SecurityVersion.WSSecurity11)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            if (emitBspAttributes)
            {
                return WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10;
            }
            return WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005;
        }

        public override string ToString()
        {
            return this.toString;
        }

        public bool EmitBspRequiredAttributes
        {
            get
            {
                return this.emitBspRequiredAttributes;
            }
        }

        public System.ServiceModel.Security.SecureConversationVersion SecureConversationVersion
        {
            get
            {
                return this.secureConversationVersion;
            }
        }

        public System.ServiceModel.Security.SecurityVersion SecurityVersion
        {
            get
            {
                return this.securityVersion;
            }
        }

        public System.ServiceModel.Security.TrustVersion TrustVersion
        {
            get
            {
                return this.trustVersion;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10
        {
            get
            {
                return wss10oasisdec2005bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10
        {
            get
            {
                return wss10bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrust13WSSecureConversation13
        {
            get
            {
                return wss11oasisdec2005;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10
        {
            get
            {
                return wss11oasisdec2005bsp10;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005
        {
            get
            {
                return wss11;
            }
        }

        public static MessageSecurityTokenVersion WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10
        {
            get
            {
                return wss11bsp10;
            }
        }
    }
}

