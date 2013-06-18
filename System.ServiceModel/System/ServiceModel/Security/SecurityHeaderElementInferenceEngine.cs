namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal abstract class SecurityHeaderElementInferenceEngine
    {
        protected SecurityHeaderElementInferenceEngine()
        {
        }

        public abstract void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader);
        public static SecurityHeaderElementInferenceEngine GetInferenceEngine(SecurityHeaderLayout layout)
        {
            SecurityHeaderLayoutHelper.Validate(layout);
            switch (layout)
            {
                case SecurityHeaderLayout.Strict:
                    return StrictModeSecurityHeaderElementInferenceEngine.Instance;

                case SecurityHeaderLayout.Lax:
                    return LaxModeSecurityHeaderElementInferenceEngine.Instance;

                case SecurityHeaderLayout.LaxTimestampFirst:
                    return LaxTimestampFirstModeSecurityHeaderElementInferenceEngine.Instance;

                case SecurityHeaderLayout.LaxTimestampLast:
                    return LaxTimestampLastModeSecurityHeaderElementInferenceEngine.Instance;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("layout"));
        }

        public abstract void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode);
    }
}

