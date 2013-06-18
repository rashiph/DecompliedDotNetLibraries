namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Xml.Linq;

    internal static class ContractValidationHelper
    {
        public static string GetErrorMessageEndpointName(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                return System.ServiceModel.Activities.SR.NotSpecified;
            }
            return endpointName;
        }

        public static string GetErrorMessageEndpointServiceContractName(XName serviceContractName)
        {
            if (serviceContractName == null)
            {
                return System.ServiceModel.Activities.SR.NotSpecified;
            }
            return serviceContractName.LocalName;
        }

        public static string GetErrorMessageOperationName(string operationName)
        {
            if (string.IsNullOrEmpty(operationName))
            {
                return System.ServiceModel.Activities.SR.NotSpecified;
            }
            return operationName;
        }

        private static void ValidateReceiveParametersWithReceiveParameters(ReceiveParametersContent receiveParameters1, ReceiveParametersContent receiveParameters2, string receiveOperationName)
        {
            int length = receiveParameters1.ArgumentNames.Length;
            if (length != receiveParameters2.ArgumentNames.Length)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceiveParametersWithSameNameButDifferentParameterCount(receiveOperationName)));
            }
            for (int i = 0; i < length; i++)
            {
                if (receiveParameters1.ArgumentNames[i] != receiveParameters2.ArgumentNames[i])
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceiveParametersWithSameNameButDifferentParameterName(receiveOperationName)));
                }
                if (receiveParameters1.ArgumentTypes[i] != receiveParameters2.ArgumentTypes[i])
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceiveParametersWithSameNameButDifferentParameterType(receiveOperationName)));
                }
            }
        }

        public static void ValidateReceiveWithReceive(Receive receive1, Receive receive2)
        {
            string operationName = receive1.OperationName;
            if (receive1.Action != receive2.Action)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceivesWithSameNameButDifferentAction(operationName)));
            }
            if ((receive1.InternalContent is ReceiveMessageContent) && (receive2.InternalContent is ReceiveMessageContent))
            {
                ReceiveMessageContent internalContent = receive1.InternalContent as ReceiveMessageContent;
                ReceiveMessageContent content2 = receive2.InternalContent as ReceiveMessageContent;
                ValidateReceiveWithReceive(internalContent, content2, operationName);
            }
            else
            {
                if (!(receive1.InternalContent is ReceiveParametersContent) || !(receive2.InternalContent is ReceiveParametersContent))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.ReceiveAndReceiveParametersHaveSameName(operationName)));
                }
                ReceiveParametersContent content3 = receive1.InternalContent as ReceiveParametersContent;
                ReceiveParametersContent content4 = receive2.InternalContent as ReceiveParametersContent;
                ValidateReceiveParametersWithReceiveParameters(content3, content4, operationName);
            }
            if (receive1.HasReply && receive2.HasReply)
            {
                ValidateSendReplyWithSendReply(receive1.FollowingReplies[0], receive2.FollowingReplies[0]);
            }
            else if ((receive1.HasReply || receive1.HasFault) != (receive2.HasReply || receive2.HasFault))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceivesWithSameNameButDifferentIsOneWay(operationName)));
            }
            if ((receive1.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope != receive2.InternalReceive.AdditionalData.IsInsideTransactedReceiveScope) || (receive1.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree != receive2.InternalReceive.AdditionalData.IsFirstReceiveOfTransactedReceiveScopeTree))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceivesWithSameNameButDifferentTxProperties(operationName)));
            }
        }

        private static void ValidateReceiveWithReceive(ReceiveMessageContent receive1, ReceiveMessageContent receive2, string receiveOperationName)
        {
            if (receive1.InternalDeclaredMessageType != receive2.InternalDeclaredMessageType)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoReceivesWithSameNameButDifferentValueType(receiveOperationName)));
            }
        }

        public static void ValidateSendReplyWithSendReply(SendReply sendReply1, SendReply sendReply2)
        {
            string operationName = sendReply1.Request.OperationName;
            if (sendReply1.Action != sendReply2.Action)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoSendRepliesWithSameNameButDifferentAction(operationName)));
            }
            if ((sendReply1.InternalContent is SendMessageContent) && (sendReply2.InternalContent is SendMessageContent))
            {
                SendMessageContent internalContent = sendReply1.InternalContent as SendMessageContent;
                SendMessageContent content2 = sendReply2.InternalContent as SendMessageContent;
                if (internalContent.InternalDeclaredMessageType != content2.InternalDeclaredMessageType)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoSendRepliesWithSameNameButDifferentValueType(operationName)));
                }
            }
            else
            {
                if (!(sendReply1.InternalContent is SendParametersContent) || !(sendReply2.InternalContent is SendParametersContent))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.ReceivePairedWithSendReplyAndSendReplyParameters(operationName)));
                }
                SendParametersContent content3 = sendReply1.InternalContent as SendParametersContent;
                SendParametersContent content4 = sendReply2.InternalContent as SendParametersContent;
                int length = content3.ArgumentNames.Length;
                if (length != content4.ArgumentNames.Length)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoSendReplyParametersWithSameNameButDifferentParameterCount(operationName)));
                }
                for (int i = 0; i < length; i++)
                {
                    if (content3.ArgumentNames[i] != content4.ArgumentNames[i])
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoSendReplyParametersWithSameNameButDifferentParameterName(operationName)));
                    }
                    if (content3.ArgumentTypes[i] != content4.ArgumentTypes[i])
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.TwoSendReplyParametersWithSameNameButDifferentParameterType(operationName)));
                    }
                }
            }
        }
    }
}

