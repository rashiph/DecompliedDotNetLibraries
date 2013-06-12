namespace System.Xml.Schema
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class XmlSchemaInferenceException : XmlSchemaException
    {
        public XmlSchemaInferenceException() : base(null)
        {
        }

        public XmlSchemaInferenceException(string message) : base(message, (Exception) null, 0, 0)
        {
        }

        protected XmlSchemaInferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XmlSchemaInferenceException(string message, Exception innerException) : base(message, innerException, 0, 0)
        {
        }

        internal XmlSchemaInferenceException(string res, string[] args) : base(res, args, null, null, 0, 0, null)
        {
        }

        internal XmlSchemaInferenceException(string res, string arg) : base(res, new string[] { arg }, null, null, 0, 0, null)
        {
        }

        internal XmlSchemaInferenceException(string res, int lineNumber, int linePosition) : base(res, null, null, null, lineNumber, linePosition, null)
        {
        }

        public XmlSchemaInferenceException(string message, Exception innerException, int lineNumber, int linePosition) : base(message, innerException, lineNumber, linePosition)
        {
        }

        internal XmlSchemaInferenceException(string res, string sourceUri, int lineNumber, int linePosition) : base(res, null, null, sourceUri, lineNumber, linePosition, null)
        {
        }

        internal XmlSchemaInferenceException(string res, string arg, string sourceUri, int lineNumber, int linePosition) : base(res, new string[] { arg }, null, sourceUri, lineNumber, linePosition, null)
        {
        }

        internal XmlSchemaInferenceException(string res, string[] args, string sourceUri, int lineNumber, int linePosition) : base(res, args, null, sourceUri, lineNumber, linePosition, null)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}

