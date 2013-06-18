namespace System.ServiceModel.Description
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    [DebuggerDisplay("Name={name}, Namespace={ns}, Type={Type}, Index={index}}")]
    public class MessagePartDescription
    {
        private ICustomAttributeProvider additionalAttributesProvider;
        private string baseType;
        private bool hasProtectionLevel;
        private int index;
        private System.Reflection.MemberInfo memberInfo;
        private bool multiple;
        private System.ServiceModel.Description.XmlName name;
        private string ns;
        private System.Net.Security.ProtectionLevel protectionLevel;
        private int serializationPosition;
        private System.Type type;
        private string uniquePartName;

        internal MessagePartDescription(MessagePartDescription other)
        {
            this.name = other.name;
            this.ns = other.ns;
            this.index = other.index;
            this.type = other.type;
            this.serializationPosition = other.serializationPosition;
            this.hasProtectionLevel = other.hasProtectionLevel;
            this.protectionLevel = other.protectionLevel;
            this.memberInfo = other.memberInfo;
            this.multiple = other.multiple;
            this.additionalAttributesProvider = other.additionalAttributesProvider;
            this.baseType = other.baseType;
            this.uniquePartName = other.uniquePartName;
        }

        public MessagePartDescription(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name", System.ServiceModel.SR.GetString("SFxParameterNameCannotBeNull"));
            }
            this.name = new System.ServiceModel.Description.XmlName(name, true);
            if (!string.IsNullOrEmpty(ns))
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }
            this.ns = ns;
        }

        internal virtual MessagePartDescription Clone()
        {
            return new MessagePartDescription(this);
        }

        internal void ResetProtectionLevel()
        {
            this.protectionLevel = System.Net.Security.ProtectionLevel.None;
            this.hasProtectionLevel = false;
        }

        internal ICustomAttributeProvider AdditionalAttributesProvider
        {
            get
            {
                return (this.additionalAttributesProvider ?? this.memberInfo);
            }
            set
            {
                this.additionalAttributesProvider = value;
            }
        }

        internal string BaseType
        {
            get
            {
                return this.baseType;
            }
            set
            {
                this.baseType = value;
            }
        }

        internal string CodeName
        {
            get
            {
                return this.name.DecodedName;
            }
        }

        public bool HasProtectionLevel
        {
            get
            {
                return this.hasProtectionLevel;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
            set
            {
                this.index = value;
            }
        }

        public System.Reflection.MemberInfo MemberInfo
        {
            get
            {
                return this.memberInfo;
            }
            set
            {
                this.memberInfo = value;
            }
        }

        [DefaultValue(false)]
        public bool Multiple
        {
            get
            {
                return this.multiple;
            }
            set
            {
                this.multiple = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name.EncodedName;
            }
        }

        public string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public System.Net.Security.ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
                this.hasProtectionLevel = true;
            }
        }

        internal int SerializationPosition
        {
            get
            {
                return this.serializationPosition;
            }
            set
            {
                this.serializationPosition = value;
            }
        }

        public System.Type Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        internal string UniquePartName
        {
            get
            {
                return this.uniquePartName;
            }
            set
            {
                this.uniquePartName = value;
            }
        }

        internal System.ServiceModel.Description.XmlName XmlName
        {
            get
            {
                return this.name;
            }
        }
    }
}

