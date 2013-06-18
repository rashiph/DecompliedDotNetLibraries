namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class TransactionFlowBindingElementImporter : IPolicyImportExtension
    {
        private TransactionFlowBindingElement EnsureBindingElement(PolicyConversionContext context)
        {
            TransactionFlowBindingElement item = context.BindingElements.Find<TransactionFlowBindingElement>();
            if (item == null)
            {
                item = new TransactionFlowBindingElement(false);
                context.BindingElements.Add(item);
            }
            return item;
        }

        private TransactionFlowOption GetOption(XmlElement elem, bool useLegacyNs)
        {
            TransactionFlowOption mandatory;
            try
            {
                if (IsRealOptionalTrue(elem) || (useLegacyNs && IsLegacyOptionalTrue(elem)))
                {
                    return TransactionFlowOption.Allowed;
                }
                mandatory = TransactionFlowOption.Mandatory;
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedBooleanAttribute", new object[] { "Optional", exception.Message })));
            }
            return mandatory;
        }

        private static bool IsLegacyOptionalTrue(XmlElement elem)
        {
            return XmlUtil.IsTrue(elem.GetAttribute("Optional", "http://schemas.xmlsoap.org/ws/2002/12/policy"));
        }

        private static bool IsRealOptionalTrue(XmlElement elem)
        {
            string attribute = elem.GetAttribute("Optional", "http://schemas.xmlsoap.org/ws/2004/09/policy");
            string booleanValue = elem.GetAttribute("Optional", "http://www.w3.org/ns/ws-policy");
            if (!XmlUtil.IsTrue(attribute))
            {
                return XmlUtil.IsTrue(booleanValue);
            }
            return true;
        }

        void IPolicyImportExtension.ImportPolicy(MetadataImporter importer, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            bool everyoneAgrees = true;
            bool flag2 = true;
            TransactionFlowOption notAllowed = TransactionFlowOption.NotAllowed;
            TransactionProtocol transactionProtocol = TransactionFlowDefaults.TransactionProtocol;
            bool anOperationCares = false;
            bool flag4 = false;
            XmlElement item = null;
            XmlElement element2 = null;
            foreach (OperationDescription description in context.Contract.Operations)
            {
                ICollection<XmlElement> operationBindingAssertions = context.GetOperationBindingAssertions(description);
                foreach (XmlElement element3 in operationBindingAssertions)
                {
                    if ((element3.NamespaceURI == "http://schemas.microsoft.com/ws/2006/02/tx/oletx") && (element3.LocalName == "OleTxAssertion"))
                    {
                        item = element3;
                        TransactionFlowOption option = this.GetOption(element3, true);
                        this.UpdateTransactionFlowAtribute(description, option);
                        TrackAgreement(ref everyoneAgrees, option, ref notAllowed, ref anOperationCares);
                        TrackAgreementTransactionProtocol(ref flag2, TransactionProtocol.OleTransactions, ref transactionProtocol, ref flag4);
                    }
                    else if ((element3.NamespaceURI == "http://schemas.xmlsoap.org/ws/2004/10/wsat") && (element3.LocalName == "ATAssertion"))
                    {
                        element2 = element3;
                        TransactionFlowOption txFlow = this.GetOption(element3, true);
                        this.UpdateTransactionFlowAtribute(description, txFlow);
                        TrackAgreement(ref everyoneAgrees, txFlow, ref notAllowed, ref anOperationCares);
                        TrackAgreementTransactionProtocol(ref flag2, TransactionProtocol.WSAtomicTransactionOctober2004, ref transactionProtocol, ref flag4);
                    }
                    else if ((element3.NamespaceURI == "http://docs.oasis-open.org/ws-tx/wsat/2006/06") && (element3.LocalName == "ATAssertion"))
                    {
                        element2 = element3;
                        TransactionFlowOption option4 = this.GetOption(element3, false);
                        this.UpdateTransactionFlowAtribute(description, option4);
                        TrackAgreement(ref everyoneAgrees, option4, ref notAllowed, ref anOperationCares);
                        TrackAgreementTransactionProtocol(ref flag2, TransactionProtocol.WSAtomicTransaction11, ref transactionProtocol, ref flag4);
                    }
                }
                if (item != null)
                {
                    operationBindingAssertions.Remove(item);
                }
                if (element2 != null)
                {
                    operationBindingAssertions.Remove(element2);
                }
            }
            if (anOperationCares)
            {
                TransactionFlowBindingElement element4 = this.EnsureBindingElement(context);
                element4.Transactions = true;
                if (flag4 && flag2)
                {
                    element4.TransactionProtocol = transactionProtocol;
                }
                else if (flag4)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxCannotHaveDifferentTransactionProtocolsInOneBinding")));
                }
            }
        }

        private static void TrackAgreement(ref bool everyoneAgrees, TransactionFlowOption option, ref TransactionFlowOption agreedOption, ref bool anOperationCares)
        {
            if (!anOperationCares)
            {
                agreedOption = option;
                anOperationCares = true;
            }
            else if (option != agreedOption)
            {
                everyoneAgrees = false;
            }
        }

        private static void TrackAgreementTransactionProtocol(ref bool everyoneAgrees, TransactionProtocol option, ref TransactionProtocol agreedOption, ref bool anOperationCares)
        {
            if (!anOperationCares)
            {
                agreedOption = option;
                anOperationCares = true;
            }
            else if (option != agreedOption)
            {
                everyoneAgrees = false;
            }
        }

        private void UpdateTransactionFlowAtribute(OperationDescription operation, TransactionFlowOption txFlow)
        {
            operation.Behaviors.Remove<TransactionFlowAttribute>();
            operation.Behaviors.Add(new TransactionFlowAttribute(txFlow));
        }
    }
}

