namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;

    internal sealed class LaxTimestampFirstModeSecurityHeaderElementInferenceEngine : LaxModeSecurityHeaderElementInferenceEngine
    {
        private static LaxTimestampFirstModeSecurityHeaderElementInferenceEngine instance = new LaxTimestampFirstModeSecurityHeaderElementInferenceEngine();

        private LaxTimestampFirstModeSecurityHeaderElementInferenceEngine()
        {
        }

        public override void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode)
        {
            for (int i = 1; i < elementManager.Count; i++)
            {
                if (elementManager.GetElementCategory(i) == ReceiveSecurityHeaderElementCategory.Timestamp)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimestampMustOccurFirstInSecurityHeaderLayout")));
                }
            }
            base.MarkElements(elementManager, messageSecurityMode);
        }

        internal static LaxTimestampFirstModeSecurityHeaderElementInferenceEngine Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

