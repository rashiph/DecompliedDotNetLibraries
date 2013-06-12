namespace System.Xml.Xsl
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class XsltCompileException : XsltException
    {
        public XsltCompileException()
        {
        }

        public XsltCompileException(string message) : base(message)
        {
        }

        protected XsltCompileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XsltCompileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public XsltCompileException(Exception inner, string sourceUri, int lineNumber, int linePosition) : base((lineNumber != 0) ? "Xslt_CompileError" : "Xslt_CompileError2", new string[] { sourceUri, lineNumber.ToString(CultureInfo.InvariantCulture), linePosition.ToString(CultureInfo.InvariantCulture) }, sourceUri, lineNumber, linePosition, inner)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}

