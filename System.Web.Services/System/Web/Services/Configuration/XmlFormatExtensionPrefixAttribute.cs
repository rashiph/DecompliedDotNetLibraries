namespace System.Web.Services.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class XmlFormatExtensionPrefixAttribute : Attribute
    {
        private string ns;
        private string prefix;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlFormatExtensionPrefixAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlFormatExtensionPrefixAttribute(string prefix, string ns)
        {
            this.prefix = prefix;
            this.ns = ns;
        }

        public string Namespace
        {
            get
            {
                if (this.ns != null)
                {
                    return this.ns;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ns = value;
            }
        }

        public string Prefix
        {
            get
            {
                if (this.prefix != null)
                {
                    return this.prefix;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.prefix = value;
            }
        }
    }
}

