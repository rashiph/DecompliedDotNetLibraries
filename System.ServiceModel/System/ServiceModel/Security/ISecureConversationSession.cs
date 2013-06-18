namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    public interface ISecureConversationSession : ISecuritySession, ISession
    {
        bool TryReadSessionTokenIdentifier(XmlReader reader);
        void WriteSessionTokenIdentifier(XmlDictionaryWriter writer);
    }
}

