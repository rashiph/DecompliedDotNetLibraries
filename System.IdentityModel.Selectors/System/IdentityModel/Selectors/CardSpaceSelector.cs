namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    public static class CardSpaceSelector
    {
        private static XmlDictionaryReaderQuotas DefaultQuotas = new XmlDictionaryReaderQuotas();
        internal const int MaxPolicyChainLength = 50;
        private static CardSpaceShim s_cardSpaceShim = new CardSpaceShim();

        static CardSpaceSelector()
        {
            DefaultQuotas.MaxDepth = 0x20;
            DefaultQuotas.MaxStringContentLength = 0x2000;
            DefaultQuotas.MaxArrayLength = 0x1400000;
            DefaultQuotas.MaxBytesPerRead = 0x1000;
            DefaultQuotas.MaxNameTableCharCount = 0x4000;
        }

        private static XmlDictionaryReader CreateReaderWithQuotas(string root)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(root);
            return XmlDictionaryReader.CreateTextReader(bytes, 0, bytes.GetLength(0), null, DefaultQuotas, null);
        }

        internal static CardSpaceShim GetShim()
        {
            s_cardSpaceShim.InitializeIfNecessary();
            return s_cardSpaceShim;
        }

        public static GenericXmlSecurityToken GetToken(CardSpacePolicyElement[] policyChain, SecurityTokenSerializer tokenSerializer)
        {
            InfoCardProofToken proofToken = null;
            InternalRefCountedHandle pCryptoHandle = null;
            RpcGenericXmlToken token3 = new RpcGenericXmlToken();
            SafeTokenHandle securityToken = null;
            int status = 0;
            if ((policyChain == null) || (policyChain.Length == 0))
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("policyChain");
            }
            if (tokenSerializer == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("tokenSerializer");
            }
            if (tokenSerializer == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("tokenSerializer");
            }
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                bool success = false;
                try
                {
                }
                finally
                {
                    try
                    {
                        using (PolicyChain chain = new PolicyChain(policyChain))
                        {
                            status = GetShim().m_csShimGetToken(chain.Length, chain.DoMarshal(), out securityToken, out pCryptoHandle);
                        }
                        if (status == 0)
                        {
                            securityToken.DangerousAddRef(ref success);
                            token3 = (RpcGenericXmlToken) Marshal.PtrToStructure(securityToken.DangerousGetHandle(), typeof(RpcGenericXmlToken));
                        }
                    }
                    finally
                    {
                        if (success)
                        {
                            securityToken.DangerousRelease();
                        }
                    }
                }
                if (status == 0)
                {
                    using (ProofTokenCryptoHandle handle3 = (ProofTokenCryptoHandle) CryptoHandle.Create(pCryptoHandle))
                    {
                        proofToken = handle3.CreateProofToken();
                    }
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(token3.xmlToken);
                    SecurityKeyIdentifierClause internalTokenReference = null;
                    if (token3.internalTokenReference != null)
                    {
                        internalTokenReference = tokenSerializer.ReadKeyIdentifierClause(CreateReaderWithQuotas(token3.internalTokenReference));
                    }
                    SecurityKeyIdentifierClause externalTokenReference = null;
                    if (token3.externalTokenReference != null)
                    {
                        externalTokenReference = tokenSerializer.ReadKeyIdentifierClause(CreateReaderWithQuotas(token3.externalTokenReference));
                    }
                    DateTime effectiveTime = DateTime.FromFileTimeUtc(token3.createDate);
                    return new GenericXmlSecurityToken(document.DocumentElement, proofToken, effectiveTime, DateTime.FromFileTimeUtc(token3.expiryDate), internalTokenReference, externalTokenReference, null);
                }
                ExceptionHelper.ThrowIfCardSpaceException(status);
                throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPIInfocardError")));
            }
            catch
            {
                if (pCryptoHandle != null)
                {
                    pCryptoHandle.Dispose();
                }
                if (proofToken != null)
                {
                    proofToken.Dispose();
                }
                throw;
            }
            finally
            {
                if (securityToken != null)
                {
                    securityToken.Dispose();
                }
            }
            return null;
        }

        public static GenericXmlSecurityToken GetToken(XmlElement endpoint, IEnumerable<XmlElement> policy, XmlElement requiredRemoteTokenIssuer, SecurityTokenSerializer tokenSerializer)
        {
            if (endpoint == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("endpoint");
            }
            if (policy == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("policy");
            }
            if (tokenSerializer == null)
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("tokenSerializer");
            }
            Collection<XmlElement> parameters = new Collection<XmlElement>();
            foreach (XmlElement element in policy)
            {
                parameters.Add(element);
            }
            return GetToken(new CardSpacePolicyElement[] { new CardSpacePolicyElement(endpoint, requiredRemoteTokenIssuer, parameters, null, 0, false) }, tokenSerializer);
        }

        public static void Import(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw InfoCardTrace.ThrowHelperArgumentNull("fileName");
            }
            int status = GetShim().m_csShimImportInformationCard(fileName);
            if (status != 0)
            {
                ExceptionHelper.ThrowIfCardSpaceException(status);
                throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPIInfocardError")));
            }
        }

        public static void Manage()
        {
            int status = GetShim().m_csShimManageCardSpace();
            if (status != 0)
            {
                ExceptionHelper.ThrowIfCardSpaceException(status);
                throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPIInfocardError")));
            }
        }

        internal static string XmlToString(IEnumerable<XmlElement> xml)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XmlElement element in xml)
            {
                if (element == null)
                {
                    throw InfoCardTrace.ThrowHelperError(new ArgumentException(Microsoft.InfoCards.SR.GetString("ClientAPIInvalidPolicy")));
                }
                builder.Append(element.OuterXml);
            }
            return builder.ToString();
        }
    }
}

