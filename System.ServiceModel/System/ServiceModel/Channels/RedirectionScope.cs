namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [Serializable]
    public class RedirectionScope
    {
        private static RedirectionScope endpoint = new RedirectionScope(InternalRedirectionScope.Endpoint);
        private int? hashCode;
        private InternalRedirectionScope internalScope;
        private static RedirectionScope message = new RedirectionScope(InternalRedirectionScope.Message);
        private static RedirectionScope session = new RedirectionScope(InternalRedirectionScope.Session);
        private string toString;

        private RedirectionScope()
        {
        }

        private RedirectionScope(InternalRedirectionScope scope)
        {
            this.Namespace = "http://schemas.microsoft.com/ws/2008/06/redirect";
            this.internalScope = scope;
            switch (scope)
            {
                case InternalRedirectionScope.Message:
                    this.Value = "Message";
                    return;

                case InternalRedirectionScope.Session:
                    this.Value = "Session";
                    return;

                case InternalRedirectionScope.Endpoint:
                    this.Value = "Endpoint";
                    return;
            }
        }

        private RedirectionScope(string value, string ns)
        {
            this.Value = value;
            this.Namespace = ns;
            this.internalScope = InternalRedirectionScope.Unknown;
        }

        public static RedirectionScope Create(string scope, string ns)
        {
            if (scope == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("scope");
            }
            if (scope.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("scope", System.ServiceModel.SR.GetString("ParameterCannotBeEmpty"));
            }
            return new RedirectionScope(scope, ns);
        }

        private void DetectScope()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, "http://schemas.microsoft.com/ws/2008/06/redirect"))
            {
                if (string.Equals(this.Value, "Message", StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Message;
                }
                else if (string.Equals(this.Value, "Session", StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Session;
                }
                else if (string.Equals(this.Value, "Endpoint", StringComparison.Ordinal))
                {
                    this.internalScope = InternalRedirectionScope.Endpoint;
                }
                else
                {
                    this.internalScope = InternalRedirectionScope.Custom;
                }
            }
            else
            {
                this.internalScope = InternalRedirectionScope.Custom;
            }
        }

        public override bool Equals(object obj)
        {
            bool flag = base.Equals(obj);
            if (!flag)
            {
                flag = (obj as RedirectionScope) == this;
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

        public static bool operator ==(RedirectionScope left, RedirectionScope right)
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
            return ((left.InternalScope == right.InternalScope) || RedirectionUtility.IsNamespaceAndValueMatch(left.Value, left.Namespace, right.Value, right.Namespace));
        }

        public static bool operator !=(RedirectionScope left, RedirectionScope right)
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

        public static RedirectionScope Endpoint
        {
            get
            {
                return endpoint;
            }
        }

        internal InternalRedirectionScope InternalScope
        {
            get
            {
                if (this.internalScope == InternalRedirectionScope.Unknown)
                {
                    this.DetectScope();
                }
                return this.internalScope;
            }
        }

        public static RedirectionScope Message
        {
            get
            {
                return message;
            }
        }

        public string Namespace { get; private set; }

        public static RedirectionScope Session
        {
            get
            {
                return session;
            }
        }

        public string Value { get; private set; }

        internal enum InternalRedirectionScope
        {
            Unknown,
            Custom,
            Message,
            Session,
            Endpoint
        }
    }
}

