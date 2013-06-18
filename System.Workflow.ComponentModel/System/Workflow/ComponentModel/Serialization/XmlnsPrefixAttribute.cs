namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        private string prefix;
        private string xmlNamespace;

        public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            this.xmlNamespace = xmlNamespace;
            this.prefix = prefix;
        }

        public string Prefix
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.prefix;
            }
        }

        public string XmlNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.xmlNamespace;
            }
        }
    }
}

