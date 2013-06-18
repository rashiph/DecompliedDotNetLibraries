namespace System.Web.Services.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class XmlFormatExtensionPointAttribute : Attribute
    {
        private bool allowElements = true;
        private string name;

        public XmlFormatExtensionPointAttribute(string memberName)
        {
            this.name = memberName;
        }

        public bool AllowElements
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.allowElements;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.allowElements = value;
            }
        }

        public string MemberName
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }
    }
}

