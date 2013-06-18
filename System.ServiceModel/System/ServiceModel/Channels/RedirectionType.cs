namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [Serializable]
    public sealed class RedirectionType
    {
        private static RedirectionType cache = new RedirectionType(InternalRedirectionType.Cache);
        private int? hashCode;
        private InternalRedirectionType internalType;
        private static RedirectionType resource = new RedirectionType(InternalRedirectionType.Resource);
        private string toString;
        private static RedirectionType useIntermediary = new RedirectionType(InternalRedirectionType.UseIntermediary);

        private RedirectionType()
        {
            this.hashCode = null;
        }

        private RedirectionType(InternalRedirectionType type)
        {
            this.hashCode = null;
            this.Namespace = "http://schemas.microsoft.com/ws/2008/06/redirect";
            this.internalType = type;
            switch (type)
            {
                case InternalRedirectionType.Cache:
                    this.Value = "Cache";
                    return;

                case InternalRedirectionType.UseIntermediary:
                    this.Value = "UseIntermediary";
                    return;

                case InternalRedirectionType.Resource:
                    this.Value = "Resource";
                    return;
            }
        }

        private RedirectionType(string value, string ns)
        {
            this.hashCode = null;
            this.Value = value;
            this.Namespace = ns;
            this.internalType = InternalRedirectionType.Unknown;
        }

        public static RedirectionType Create(string type, string ns)
        {
            if (type == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("type");
            }
            if (type.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("type", System.ServiceModel.SR.GetString("ParameterCannotBeEmpty"));
            }
            return new RedirectionType(type, ns);
        }

        private void DetectType()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, "http://schemas.microsoft.com/ws/2008/06/redirect"))
            {
                if (string.Equals(this.Value, "Cache", StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.Cache;
                }
                else if (string.Equals(this.Value, "Resource", StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.Resource;
                }
                else if (string.Equals(this.Value, "UseIntermediary", StringComparison.Ordinal))
                {
                    this.internalType = InternalRedirectionType.UseIntermediary;
                }
                else
                {
                    this.internalType = InternalRedirectionType.Custom;
                }
            }
            else
            {
                this.internalType = InternalRedirectionType.Custom;
            }
        }

        public override bool Equals(object obj)
        {
            bool flag = base.Equals(obj);
            if (!flag)
            {
                flag = (obj as RedirectionType) == this;
            }
            return flag;
        }

        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                this.hashCode = new int?(RedirectionUtility.ComputeHashCode(this.Value, this.Namespace));
            }
            return this.hashCode.Value;
        }

        public static bool operator ==(RedirectionType left, RedirectionType right)
        {
            bool flag = false;
            if ((left == null) && (right == null))
            {
                return true;
            }
            if ((left == null) || (right == null))
            {
                return flag;
            }
            return ((left.InternalType == right.InternalType) || RedirectionUtility.IsNamespaceAndValueMatch(left.Value, left.Namespace, right.Value, right.Namespace));
        }

        public static bool operator !=(RedirectionType left, RedirectionType right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (this.toString == null)
            {
                if (this.Namespace != null)
                {
                    this.toString = System.ServiceModel.SR.GetString("RedirectionInfoStringFormatWithNamespace", new object[] { this.Value, this.Namespace });
                }
                else
                {
                    this.toString = System.ServiceModel.SR.GetString("RedirectionInfoStringFormatNoNamespace", new object[] { this.Value });
                }
            }
            return this.toString;
        }

        public static RedirectionType Cache
        {
            get
            {
                return cache;
            }
        }

        internal InternalRedirectionType InternalType
        {
            get
            {
                if (this.internalType == InternalRedirectionType.Unknown)
                {
                    this.DetectType();
                }
                return this.internalType;
            }
        }

        public string Namespace { get; private set; }

        public static RedirectionType Resource
        {
            get
            {
                return resource;
            }
        }

        public static RedirectionType UseIntermediary
        {
            get
            {
                return useIntermediary;
            }
        }

        public string Value { get; private set; }

        internal enum InternalRedirectionType
        {
            Unknown,
            Custom,
            Cache,
            UseIntermediary,
            Resource
        }
    }
}

