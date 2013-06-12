namespace System.Xml
{
    using System;
    using System.Xml.Schema;

    internal interface IValidationEventHandling
    {
        void SendEvent(Exception exception, XmlSeverityType severity);

        object EventHandler { get; }
    }
}

