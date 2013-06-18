namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;

    internal sealed class LaxTimestampLastModeSecurityHeaderElementInferenceEngine : LaxModeSecurityHeaderElementInferenceEngine
    {
        private static LaxTimestampLastModeSecurityHeaderElementInferenceEngine instance = new LaxTimestampLastModeSecurityHeaderElementInferenceEngine();

        private LaxTimestampLastModeSecurityHeaderElementInferenceEngine()
        {
        }

        public override void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode)
        {
            for (int i = 0; i < (elementManager.Count - 1); i++)
            {
                if (elementManager.GetElementCategory(i) == ReceiveSecurityHeaderElementCategory.Timestamp)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TimestampMustOccurLastInSecurityHeaderLayout")));
                }
            }
            base.MarkElements(elementManager, messageSecurityMode);
        }

        internal static LaxTimestampLastModeSecurityHeaderElementInferenceEngine Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

