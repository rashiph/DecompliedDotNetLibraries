namespace System.ServiceModel.Security
{
    using System;
    using System.Xml;

    internal sealed class StrictModeSecurityHeaderElementInferenceEngine : SecurityHeaderElementInferenceEngine
    {
        private static StrictModeSecurityHeaderElementInferenceEngine instance = new StrictModeSecurityHeaderElementInferenceEngine();

        private StrictModeSecurityHeaderElementInferenceEngine()
        {
        }

        public override void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader)
        {
            securityHeader.ExecuteFullPass(reader);
        }

        public override void MarkElements(ReceiveSecurityHeaderElementManager elementManager, bool messageSecurityMode)
        {
            bool flag = false;
            for (int i = 0; i < elementManager.Count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                elementManager.GetElementEntry(i, out entry);
                if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.Signature)
                {
                    if (!messageSecurityMode || flag)
                    {
                        elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Endorsing);
                    }
                    else
                    {
                        elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Primary);
                        flag = true;
                    }
                }
            }
        }

        internal static StrictModeSecurityHeaderElementInferenceEngine Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

