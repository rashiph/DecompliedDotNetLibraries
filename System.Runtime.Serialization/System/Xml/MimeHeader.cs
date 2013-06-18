namespace System.Xml
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    internal class MimeHeader
    {
        private string name;
        private string value;

        public MimeHeader(string name, string value)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }
            this.name = name;
            this.value = value;
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
        }
    }
}

