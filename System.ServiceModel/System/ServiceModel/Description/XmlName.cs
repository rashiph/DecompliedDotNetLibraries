namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.Xml;

    internal class XmlName
    {
        private string decoded;
        private string encoded;

        internal XmlName(string name) : this(name, false)
        {
        }

        internal XmlName(string name, bool isEncoded)
        {
            if (isEncoded)
            {
                ValidateEncodedName(name, true);
                this.encoded = name;
            }
            else
            {
                this.decoded = name;
            }
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, this))
            {
                return true;
            }
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            System.ServiceModel.Description.XmlName xmlName = obj as System.ServiceModel.Description.XmlName;
            if (xmlName == null)
            {
                return false;
            }
            return this.Matches(xmlName);
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(this.EncodedName))
            {
                return 0;
            }
            return this.EncodedName.GetHashCode();
        }

        internal static bool IsNullOrEmpty(System.ServiceModel.Description.XmlName xmlName)
        {
            if (xmlName != null)
            {
                return xmlName.IsEmpty;
            }
            return true;
        }

        private bool Matches(System.ServiceModel.Description.XmlName xmlName)
        {
            return string.Equals(this.EncodedName, xmlName.EncodedName, StringComparison.Ordinal);
        }

        public static bool operator ==(System.ServiceModel.Description.XmlName a, System.ServiceModel.Description.XmlName b)
        {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }
            return a.Equals(b);
        }

        public static bool operator !=(System.ServiceModel.Description.XmlName a, System.ServiceModel.Description.XmlName b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if ((this.encoded == null) && (this.decoded == null))
            {
                return null;
            }
            if (this.encoded != null)
            {
                return this.encoded;
            }
            return this.decoded;
        }

        private static void ValidateEncodedName(string name, bool allowNull)
        {
            if (!allowNull || (name != null))
            {
                try
                {
                    XmlConvert.VerifyNCName(name);
                }
                catch (XmlException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(exception.Message, "name"));
                }
            }
        }

        internal string DecodedName
        {
            get
            {
                if (this.decoded == null)
                {
                    this.decoded = NamingHelper.CodeName(this.encoded);
                }
                return this.decoded;
            }
        }

        internal string EncodedName
        {
            get
            {
                if (this.encoded == null)
                {
                    this.encoded = NamingHelper.XmlName(this.decoded);
                }
                return this.encoded;
            }
        }

        private bool IsEmpty
        {
            get
            {
                return (string.IsNullOrEmpty(this.encoded) && string.IsNullOrEmpty(this.decoded));
            }
        }
    }
}

