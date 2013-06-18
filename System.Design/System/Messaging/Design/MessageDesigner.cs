namespace System.Messaging.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class MessageDesigner : ComponentDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            RuntimeComponentFilter.FilterProperties(properties, new string[] { 
                "EncryptionAlgorithm", "BodyType", "DigitalSignature", "UseJournalQueue", "SenderCertificate", "ConnectorType", "TransactionStatusQueue", "UseDeadLetterQueue", "UseTracing", "UseAuthentication", "TimeToReachQueue", "HashAlgorithm", "Priority", "BodyStream", "DestinationSymmetricKey", "AppSpecific", 
                "ResponseQueue", "AuthenticationProviderName", "Recoverable", "UseEncryption", "AttachSenderId", "CorrelationId", "AdministrationQueue", "AuthenticationProviderType", "TimeToBeReceived", "AcknowledgeType", "Label", "Extension"
             }, null);
        }
    }
}

