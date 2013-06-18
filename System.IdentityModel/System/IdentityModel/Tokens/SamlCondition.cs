namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Xml;

    public abstract class SamlCondition
    {
        protected SamlCondition()
        {
        }

        public abstract void MakeReadOnly();
        public abstract void ReadXml(XmlDictionaryReader reader, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer, SecurityTokenResolver outOfBandTokenResolver);
        public abstract void WriteXml(XmlDictionaryWriter writer, SamlSerializer samlSerializer, SecurityTokenSerializer keyInfoSerializer);

        public abstract bool IsReadOnly { get; }
    }
}

