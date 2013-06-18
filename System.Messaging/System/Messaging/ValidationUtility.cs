namespace System.Messaging
{
    using System;

    internal static class ValidationUtility
    {
        public static bool ValidateAccessControlEntryType(AccessControlEntryType value)
        {
            return ((value >= AccessControlEntryType.Allow) && (value <= AccessControlEntryType.Revoke));
        }

        public static bool ValidateCryptographicProviderType(CryptographicProviderType value)
        {
            return ((value >= CryptographicProviderType.None) && (value <= CryptographicProviderType.SttIss));
        }

        public static bool ValidateEncryptionAlgorithm(EncryptionAlgorithm value)
        {
            if ((value != EncryptionAlgorithm.None) && (value != EncryptionAlgorithm.Rc2))
            {
                return (value == EncryptionAlgorithm.Rc4);
            }
            return true;
        }

        public static bool ValidateEncryptionRequired(EncryptionRequired value)
        {
            return ((value >= EncryptionRequired.None) && (value <= EncryptionRequired.Body));
        }

        public static bool ValidateHashAlgorithm(HashAlgorithm value)
        {
            if ((((value != HashAlgorithm.None) && (value != HashAlgorithm.Md2)) && ((value != HashAlgorithm.Md4) && (value != HashAlgorithm.Md5))) && (value != HashAlgorithm.Sha))
            {
                return (value == HashAlgorithm.Mac);
            }
            return true;
        }

        public static bool ValidateMessageLookupAction(MessageLookupAction value)
        {
            if (((value != MessageLookupAction.Current) && (value != MessageLookupAction.Next)) && ((value != MessageLookupAction.Previous) && (value != MessageLookupAction.First)))
            {
                return (value == MessageLookupAction.Last);
            }
            return true;
        }

        public static bool ValidateMessagePriority(MessagePriority value)
        {
            return ((value >= MessagePriority.Lowest) && (value <= MessagePriority.Highest));
        }

        public static bool ValidateMessageQueueTransactionType(MessageQueueTransactionType value)
        {
            if ((value != MessageQueueTransactionType.None) && (value != MessageQueueTransactionType.Automatic))
            {
                return (value == MessageQueueTransactionType.Single);
            }
            return true;
        }

        public static bool ValidateQueueAccessMode(QueueAccessMode value)
        {
            if ((((value != QueueAccessMode.Send) && (value != QueueAccessMode.Peek)) && ((value != QueueAccessMode.Receive) && (value != QueueAccessMode.PeekAndAdmin))) && (value != QueueAccessMode.ReceiveAndAdmin))
            {
                return (value == QueueAccessMode.SendAndReceive);
            }
            return true;
        }

        public static bool ValidateTrusteeType(TrusteeType trustee)
        {
            return ((trustee >= TrusteeType.Unknown) && (trustee <= TrusteeType.Computer));
        }
    }
}

