namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Xml;

    internal sealed class XmlDocumentSurrogate : ISerializationSurrogate
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal XmlDocumentSurrogate()
        {
        }

        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            XmlDocument document = obj as XmlDocument;
            if (document == null)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidArgumentValue"), "obj");
            }
            info.AddValue("innerXml", document.InnerXml);
            info.SetType(typeof(XmlDocumentReference));
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        [Serializable]
        private sealed class XmlDocumentReference : IObjectReference
        {
            private string innerXml = string.Empty;

            object IObjectReference.GetRealObject(StreamingContext context)
            {
                XmlDocument document = new XmlDocument();
                if (!string.IsNullOrEmpty(this.innerXml))
                {
                    document.InnerXml = this.innerXml;
                }
                return document;
            }
        }
    }
}

