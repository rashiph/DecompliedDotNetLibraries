namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.Web.Services.Description;

    internal static class ValidWsdl
    {
        internal static bool Check(MessagePart part, Message message, WsdlWarningHandler warningHandler)
        {
            if (string.IsNullOrEmpty(part.Name))
            {
                string str = System.ServiceModel.SR.GetString("XsdMissingRequiredAttribute1", new object[] { "name" });
                string warning = System.ServiceModel.SR.GetString("IgnoreMessagePart3", new object[] { message.Name, message.ServiceDescription.TargetNamespace, str });
                warningHandler(warning);
                return false;
            }
            return true;
        }

        internal static bool Check(SoapFaultBinding soapFaultBinding, FaultBinding faultBinding, WsdlWarningHandler warningHandler)
        {
            if (string.IsNullOrEmpty(soapFaultBinding.Name))
            {
                string str = System.ServiceModel.SR.GetString("XsdMissingRequiredAttribute1", new object[] { "name" });
                string warning = System.ServiceModel.SR.GetString("IgnoreSoapFaultBinding3", new object[] { faultBinding.OperationBinding.Name, faultBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, str });
                warningHandler(warning);
                return false;
            }
            return true;
        }

        internal static bool Check(SoapHeaderBinding soapHeaderBinding, MessageBinding messageBinding, WsdlWarningHandler warningHandler)
        {
            if ((soapHeaderBinding.Message == null) || soapHeaderBinding.Message.IsEmpty)
            {
                string str = System.ServiceModel.SR.GetString("XsdMissingRequiredAttribute1", new object[] { "message" });
                string warning = System.ServiceModel.SR.GetString("IgnoreSoapHeaderBinding3", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, str });
                warningHandler(warning);
                return false;
            }
            if (string.IsNullOrEmpty(soapHeaderBinding.Part))
            {
                string str3 = System.ServiceModel.SR.GetString("XsdMissingRequiredAttribute1", new object[] { "part" });
                string str4 = System.ServiceModel.SR.GetString("IgnoreSoapHeaderBinding3", new object[] { messageBinding.OperationBinding.Name, messageBinding.OperationBinding.Binding.ServiceDescription.TargetNamespace, str3 });
                warningHandler(str4);
                return false;
            }
            return true;
        }
    }
}

