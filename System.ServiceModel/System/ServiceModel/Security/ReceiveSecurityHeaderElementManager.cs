namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal sealed class ReceiveSecurityHeaderElementManager : ISignatureReaderProvider
    {
        private string bodyContentId;
        private string bodyId;
        private int count;
        private ReceiveSecurityHeaderEntry[] elements;
        private readonly string[] headerIds;
        private const int InitialCapacity = 8;
        private string[] predecryptionHeaderIds;
        private readonly ReceiveSecurityHeader securityHeader;

        public ReceiveSecurityHeaderElementManager(ReceiveSecurityHeader securityHeader)
        {
            this.securityHeader = securityHeader;
            this.elements = new ReceiveSecurityHeaderEntry[8];
            if (securityHeader.RequireMessageProtection)
            {
                this.headerIds = new string[securityHeader.ProcessedMessage.Headers.Count];
            }
        }

        public void AppendElement(ReceiveSecurityHeaderElementCategory elementCategory, object element, ReceiveSecurityHeaderBindingModes bindingMode, string id, TokenTracker supportingTokenTracker)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
            }
            this.EnsureCapacityToAdd();
            this.elements[this.count++].SetElement(elementCategory, element, bindingMode, id, false, null, supportingTokenTracker);
        }

        public void AppendEncryptedData(EncryptedData encryptedData)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.EncryptedData, encryptedData, ReceiveSecurityHeaderBindingModes.Unknown, encryptedData.Id, null);
        }

        public void AppendReferenceList(ReferenceList referenceList)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.ReferenceList, referenceList, ReceiveSecurityHeaderBindingModes.Unknown, null, null);
        }

        public void AppendSignature(SignedXml signedXml)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.Signature, signedXml, ReceiveSecurityHeaderBindingModes.Unknown, signedXml.Id, null);
        }

        public void AppendSignatureConfirmation(ISignatureValueSecurityElement signatureConfirmationElement)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.SignatureConfirmation, signatureConfirmationElement, ReceiveSecurityHeaderBindingModes.Unknown, signatureConfirmationElement.Id, null);
        }

        public void AppendTimestamp(SecurityTimestamp timestamp)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.Timestamp, timestamp, ReceiveSecurityHeaderBindingModes.Unknown, timestamp.Id, null);
        }

        public void AppendToken(SecurityToken token, ReceiveSecurityHeaderBindingModes mode, TokenTracker supportingTokenTracker)
        {
            this.AppendElement(ReceiveSecurityHeaderElementCategory.Token, token, mode, token.Id, supportingTokenTracker);
        }

        public void EnsureAllRequiredSecurityHeaderTargetsWereProtected()
        {
            for (int i = 0; i < this.count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.GetElementEntry(i, out entry);
                if (!entry.signed)
                {
                    switch (entry.elementCategory)
                    {
                        case ReceiveSecurityHeaderElementCategory.SignatureConfirmation:
                        case ReceiveSecurityHeaderElementCategory.Timestamp:
                            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredSecurityHeaderElementNotSigned", new object[] { entry.elementCategory, entry.id })));

                        case ReceiveSecurityHeaderElementCategory.Token:
                            goto Label_007C;
                    }
                }
                goto Label_00D4;
            Label_007C:
                switch (entry.bindingMode)
                {
                    case ReceiveSecurityHeaderBindingModes.Signed:
                    case ReceiveSecurityHeaderBindingModes.SignedEndorsing:
                    case ReceiveSecurityHeaderBindingModes.Basic:
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredSecurityTokenNotSigned", new object[] { entry.element, entry.bindingMode })));
                }
            Label_00D4:
                if ((!entry.encrypted && (entry.elementCategory == ReceiveSecurityHeaderElementCategory.Token)) && (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Basic))
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredSecurityTokenNotEncrypted", new object[] { entry.element, entry.bindingMode })));
                }
            }
        }

        private void EnsureCapacityToAdd()
        {
            if (this.count == this.elements.Length)
            {
                ReceiveSecurityHeaderEntry[] destinationArray = new ReceiveSecurityHeaderEntry[this.elements.Length * 2];
                Array.Copy(this.elements, 0, destinationArray, 0, this.count);
                this.elements = destinationArray;
            }
        }

        public object GetElement(int index)
        {
            return this.elements[index].element;
        }

        public T GetElement<T>(int index) where T: class
        {
            return (T) this.elements[index].element;
        }

        public ReceiveSecurityHeaderElementCategory GetElementCategory(int index)
        {
            return this.elements[index].elementCategory;
        }

        public void GetElementEntry(int index, out ReceiveSecurityHeaderEntry element)
        {
            element = this.elements[index];
        }

        public void GetPrimarySignature(out XmlDictionaryReader reader, out string id)
        {
            for (int i = 0; i < this.count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.GetElementEntry(i, out entry);
                if ((entry.elementCategory == ReceiveSecurityHeaderElementCategory.Signature) && (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary))
                {
                    reader = this.GetReader(i, false);
                    id = entry.id;
                    return;
                }
            }
            reader = null;
            id = null;
        }

        internal XmlDictionaryReader GetReader(int index, bool requiresEncryptedFormReader)
        {
            if (!requiresEncryptedFormReader)
            {
                byte[] decryptedBuffer = this.elements[index].decryptedBuffer;
                if (decryptedBuffer != null)
                {
                    return this.securityHeader.CreateDecryptedReader(decryptedBuffer);
                }
            }
            XmlDictionaryReader reader = this.securityHeader.CreateSecurityHeaderReader();
            reader.ReadStartElement();
            for (int i = 0; reader.IsStartElement() && (i < index); i++)
            {
                reader.Skip();
            }
            return reader;
        }

        public XmlDictionaryReader GetSignatureVerificationReader(string id, bool requiresEncryptedFormReaderIfDecrypted)
        {
            for (int i = 0; i < this.count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.GetElementEntry(i, out entry);
                bool requiresEncryptedFormId = entry.encrypted && requiresEncryptedFormReaderIfDecrypted;
                bool flag2 = (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Signed) || (entry.bindingMode == ReceiveSecurityHeaderBindingModes.SignedEndorsing);
                if (entry.MatchesId(id, requiresEncryptedFormId))
                {
                    this.SetSigned(i);
                    return this.GetReader(i, requiresEncryptedFormId);
                }
                if (entry.MatchesId(id, flag2))
                {
                    this.SetSigned(i);
                    return this.GetReader(i, flag2);
                }
            }
            return null;
        }

        private void OnDuplicateId(string id)
        {
            throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("DuplicateIdInMessageToBeVerified", new object[] { id })), this.securityHeader.SecurityVerifiedMessage);
        }

        public void ReplaceHeaderEntry(int index, ReceiveSecurityHeaderEntry element)
        {
            this.elements[index] = element;
        }

        public void SetBindingMode(int index, ReceiveSecurityHeaderBindingModes bindingMode)
        {
            this.elements[index].bindingMode = bindingMode;
        }

        public void SetElement(int index, object element)
        {
            this.elements[index].element = element;
        }

        public void SetElementAfterDecryption(int index, ReceiveSecurityHeaderElementCategory elementCategory, object element, ReceiveSecurityHeaderBindingModes bindingMode, string id, byte[] decryptedBuffer, TokenTracker supportingTokenTracker)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
            }
            this.elements[index].PreserveIdBeforeDecryption();
            this.elements[index].SetElement(elementCategory, element, bindingMode, id, true, decryptedBuffer, supportingTokenTracker);
        }

        public void SetSignatureAfterDecryption(int index, SignedXml signedXml, byte[] decryptedBuffer)
        {
            this.SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.Signature, signedXml, ReceiveSecurityHeaderBindingModes.Unknown, signedXml.Id, decryptedBuffer, null);
        }

        public void SetSignatureConfirmationAfterDecryption(int index, ISignatureValueSecurityElement signatureConfirmationElement, byte[] decryptedBuffer)
        {
            this.SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.SignatureConfirmation, signatureConfirmationElement, ReceiveSecurityHeaderBindingModes.Unknown, signatureConfirmationElement.Id, decryptedBuffer, null);
        }

        private void SetSigned(int index)
        {
            this.elements[index].signed = true;
            if (this.elements[index].supportingTokenTracker != null)
            {
                this.elements[index].supportingTokenTracker.IsSigned = true;
            }
        }

        public void SetTimestampSigned(string id)
        {
            for (int i = 0; i < this.count; i++)
            {
                if ((this.elements[i].elementCategory == ReceiveSecurityHeaderElementCategory.Timestamp) && (this.elements[i].id == id))
                {
                    this.SetSigned(i);
                }
            }
        }

        public void SetTokenAfterDecryption(int index, SecurityToken token, ReceiveSecurityHeaderBindingModes mode, byte[] decryptedBuffer, TokenTracker supportingTokenTracker)
        {
            this.SetElementAfterDecryption(index, ReceiveSecurityHeaderElementCategory.Token, token, mode, token.Id, decryptedBuffer, supportingTokenTracker);
        }

        XmlDictionaryReader ISignatureReaderProvider.GetReader(object callbackContext)
        {
            int index = (int) callbackContext;
            return this.GetReader(index, false);
        }

        private void VerifyIdUniquenessInHeaderIdTable(string id, int headerCount, string[] headerIdTable)
        {
            for (int i = 0; i < headerCount; i++)
            {
                if (headerIdTable[i] == id)
                {
                    this.OnDuplicateId(id);
                }
            }
        }

        private void VerifyIdUniquenessInMessageHeadersAndBody(string id, int headerCount)
        {
            this.VerifyIdUniquenessInHeaderIdTable(id, headerCount, this.headerIds);
            if (this.predecryptionHeaderIds != null)
            {
                this.VerifyIdUniquenessInHeaderIdTable(id, headerCount, this.predecryptionHeaderIds);
            }
            if ((this.bodyId == id) || (this.bodyContentId == id))
            {
                this.OnDuplicateId(id);
            }
        }

        private void VerifyIdUniquenessInSecurityHeader(string id)
        {
            for (int i = 0; i < this.count; i++)
            {
                if ((this.elements[i].id == id) || (this.elements[i].encryptedFormId == id))
                {
                    this.OnDuplicateId(id);
                }
            }
        }

        public void VerifySignatureConfirmationWasFound()
        {
            for (int i = 0; i < this.count; i++)
            {
                ReceiveSecurityHeaderEntry entry;
                this.GetElementEntry(i, out entry);
                if (entry.elementCategory == ReceiveSecurityHeaderElementCategory.SignatureConfirmation)
                {
                    return;
                }
            }
            throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SignatureConfirmationWasExpected")));
        }

        public void VerifyUniquenessAndSetBodyContentId(string id)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
                this.VerifyIdUniquenessInMessageHeadersAndBody(id, this.headerIds.Length);
                this.bodyContentId = id;
            }
        }

        public void VerifyUniquenessAndSetBodyId(string id)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
                this.VerifyIdUniquenessInMessageHeadersAndBody(id, this.headerIds.Length);
                this.bodyId = id;
            }
        }

        public void VerifyUniquenessAndSetDecryptedHeaderId(string id, int headerIndex)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
                this.VerifyIdUniquenessInMessageHeadersAndBody(id, headerIndex);
                if (this.predecryptionHeaderIds == null)
                {
                    this.predecryptionHeaderIds = new string[this.headerIds.Length];
                }
                this.predecryptionHeaderIds[headerIndex] = this.headerIds[headerIndex];
                this.headerIds[headerIndex] = id;
            }
        }

        public void VerifyUniquenessAndSetHeaderId(string id, int headerIndex)
        {
            if (id != null)
            {
                this.VerifyIdUniquenessInSecurityHeader(id);
                this.VerifyIdUniquenessInMessageHeadersAndBody(id, headerIndex);
                this.headerIds[headerIndex] = id;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }
    }
}

