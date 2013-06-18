namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public abstract class DsmlDocument
    {
        internal string dsmlRequestID;

        protected DsmlDocument()
        {
        }

        public abstract XmlDocument ToXml();
    }
}

