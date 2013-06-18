namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class XmlnsDefinitionAttribute : Attribute
    {
        private string assemblyName;
        private string clrNamespace;
        private string xmlNamespace;

        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }
            this.xmlNamespace = xmlNamespace;
            this.clrNamespace = clrNamespace;
        }

        public string AssemblyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.assemblyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.assemblyName = value;
            }
        }

        public string ClrNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.clrNamespace;
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

