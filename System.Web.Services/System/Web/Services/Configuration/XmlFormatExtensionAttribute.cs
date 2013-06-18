namespace System.Web.Services.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class XmlFormatExtensionAttribute : Attribute
    {
        private string name;
        private string ns;
        private Type[] types;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlFormatExtensionAttribute()
        {
        }

        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1) : this(elementName, ns, new Type[] { extensionPoint1 })
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlFormatExtensionAttribute(string elementName, string ns, Type[] extensionPoints)
        {
            this.name = elementName;
            this.ns = ns;
            this.types = extensionPoints;
        }

        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2 })
        {
        }

        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2, extensionPoint3 })
        {
        }

        public XmlFormatExtensionAttribute(string elementName, string ns, Type extensionPoint1, Type extensionPoint2, Type extensionPoint3, Type extensionPoint4) : this(elementName, ns, new Type[] { extensionPoint1, extensionPoint2, extensionPoint3, extensionPoint4 })
        {
        }

        public string ElementName
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

        public Type[] ExtensionPoints
        {
            get
            {
                if (this.types != null)
                {
                    return this.types;
                }
                return new Type[0];
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.types = value;
            }
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
    }
}

