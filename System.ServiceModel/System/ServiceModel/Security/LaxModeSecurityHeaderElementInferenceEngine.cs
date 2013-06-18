namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.ServiceModel;
    using System.Xml;

    internal class LaxModeSecurityHeaderElementInferenceEngine : SecurityHeaderElementInferenceEngine
    {
        private static LaxModeSecurityHeaderElementInferenceEngine instance = new LaxModeSecurityHeaderElementInferenceEngine();

        protected LaxModeSecurityHeaderElementInferenceEngine()
        {
        }

        public override void ExecuteProcessingPasses(ReceiveSecurityHeader securityHeader, XmlDictionaryReader reader)
        {
            securityHeader.ExecuteReadingPass(reader);
            securityHeader.ExecuteDerivedKeyTokenStubPass(false);
            securityHeader.ExecuteSubheaderDecryptionPass();
            securityHeader.ExecuteDerivedKeyTokenStubPass(true);
            this.MarkElements(securityHeader.ElementManager, securityHeader.RequireMessageProtection);
            securityHeader.ExecuteSignatureEncryptionProcessingPass();
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
                    if (!messageSecurityMode)
                    {
                        elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Endorsing);
                        continue;
                    }
                    SignedXml element = (SignedXml) entry.element;
                    StandardSignedInfo signedInfo = (StandardSignedInfo) element.Signature.SignedInfo;
                    bool flag2 = false;
                    if (signedInfo.ReferenceCount == 1)
                    {
                        string uri = signedInfo[0].Uri;
                        if (((uri == null) || (uri.Length <= 1)) || (uri[0] != '#'))
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToResolveReferenceUriForSignature", new object[] { uri })));
                        }
                        string str2 = uri.Substring(1);
                        for (int j = 0; j < elementManager.Count; j++)
                        {
                            ReceiveSecurityHeaderEntry entry2;
                            elementManager.GetElementEntry(j, out entry2);
                            if (((j != i) && (entry2.elementCategory == ReceiveSecurityHeaderElementCategory.Signature)) && (entry2.id == str2))
                            {
                                flag2 = true;
                                break;
                            }
                        }
                    }
                    if (flag2)
                    {
                        elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Endorsing);
                    }
                    else
                    {
                        if (flag)
                        {
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("AtMostOnePrimarySignatureInReceiveSecurityHeader")));
                        }
                        flag = true;
                        elementManager.SetBindingMode(i, ReceiveSecurityHeaderBindingModes.Primary);
                    }
                }
            }
        }

        internal static LaxModeSecurityHeaderElementInferenceEngine Instance
        {
            get
            {
                return instance;
            }
        }
    }
}

