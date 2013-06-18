namespace System.Web.Services.Discovery
{
    using System;
    using System.Runtime;
    using System.Xml.Serialization;

    public sealed class ExcludePathInfo
    {
        private string path;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExcludePathInfo()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExcludePathInfo(string path)
        {
            this.path = path;
        }

        [XmlAttribute("path")]
        public string Path
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.path;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.path = value;
            }
        }
    }
}

