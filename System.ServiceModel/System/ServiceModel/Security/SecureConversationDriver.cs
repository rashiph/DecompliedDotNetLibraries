namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal abstract class SecureConversationDriver
    {
        protected SecureConversationDriver()
        {
        }

        public abstract UniqueId GetSecurityContextTokenId(XmlDictionaryReader reader);
        public abstract bool IsAtSecurityContextToken(XmlDictionaryReader reader);

        public abstract XmlDictionaryString BadContextTokenFaultCode { get; }

        public virtual XmlDictionaryString CloseAction
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationDriverVersionDoesNotSupportSession")));
            }
        }

        public virtual XmlDictionaryString CloseResponseAction
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationDriverVersionDoesNotSupportSession")));
            }
        }

        public virtual bool IsSessionSupported
        {
            get
            {
                return false;
            }
        }

        public abstract XmlDictionaryString IssueAction { get; }

        public abstract XmlDictionaryString IssueResponseAction { get; }

        public abstract XmlDictionaryString Namespace { get; }

        public virtual XmlDictionaryString RenewAction
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationDriverVersionDoesNotSupportSession")));
            }
        }

        public abstract XmlDictionaryString RenewNeededFaultCode { get; }

        public virtual XmlDictionaryString RenewResponseAction
        {
            get
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecureConversationDriverVersionDoesNotSupportSession")));
            }
        }

        public abstract string TokenTypeUri { get; }
    }
}

