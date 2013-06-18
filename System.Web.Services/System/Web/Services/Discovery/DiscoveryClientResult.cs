namespace System.Web.Services.Discovery
{
    using System;
    using System.Runtime;
    using System.Xml.Serialization;

    public sealed class DiscoveryClientResult
    {
        private string filename;
        private string referenceTypeName;
        private string url;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DiscoveryClientResult()
        {
        }

        public DiscoveryClientResult(Type referenceType, string url, string filename)
        {
            this.referenceTypeName = (referenceType == null) ? string.Empty : referenceType.FullName;
            this.url = url;
            this.filename = filename;
        }

        [XmlAttribute("filename")]
        public string Filename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.filename;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.filename = value;
            }
        }

        [XmlAttribute("referenceType")]
        public string ReferenceTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.referenceTypeName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.referenceTypeName = value;
            }
        }

        [XmlAttribute("url")]
        public string Url
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.url;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.url = value;
            }
        }
    }
}

