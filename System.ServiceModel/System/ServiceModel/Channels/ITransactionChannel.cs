namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;

    internal interface ITransactionChannel
    {
        void ReadIssuedTokens(Message message, MessageDirection direction);
        void ReadTransactionDataFromMessage(Message message, MessageDirection direction);
        void WriteIssuedTokens(Message message, MessageDirection direction);
        void WriteTransactionDataToMessage(Message message, MessageDirection direction);
    }
}

