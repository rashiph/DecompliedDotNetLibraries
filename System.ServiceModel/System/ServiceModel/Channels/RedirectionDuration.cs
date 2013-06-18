namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [Serializable]
    public class RedirectionDuration
    {
        private int? hashCode;
        private InternalRedirectionDuration internalDuration;
        private static RedirectionDuration permanent = new RedirectionDuration(InternalRedirectionDuration.Permanent);
        private static RedirectionDuration temporary = new RedirectionDuration(InternalRedirectionDuration.Temporary);
        private string toString;

        private RedirectionDuration()
        {
            this.hashCode = null;
        }

        private RedirectionDuration(InternalRedirectionDuration duration)
        {
            this.hashCode = null;
            this.Namespace = "http://schemas.microsoft.com/ws/2008/06/redirect";
            this.internalDuration = duration;
            switch (duration)
            {
                case InternalRedirectionDuration.Temporary:
                    this.Value = "Temporary";
                    return;

                case InternalRedirectionDuration.Permanent:
                    this.Value = "Permanent";
                    return;
            }
        }

        private RedirectionDuration(string duration, string ns)
        {
            this.hashCode = null;
            this.Value = duration;
            this.Namespace = ns;
            this.internalDuration = InternalRedirectionDuration.Unknown;
        }

        public static RedirectionDuration Create(string duration, string ns)
        {
            if (duration == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("duration");
            }
            if (duration.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("duration", System.ServiceModel.SR.GetString("ParameterCannotBeEmpty"));
            }
            return new RedirectionDuration(duration, ns);
        }

        private void DetectDuration()
        {
            if (RedirectionUtility.IsNamespaceMatch(this.Namespace, "http://schemas.microsoft.com/ws/2008/06/redirect"))
            {
                if (string.Equals(this.Value, "Temporary", StringComparison.Ordinal))
                {
                    this.internalDuration = InternalRedirectionDuration.Temporary;
                }
                else if (string.Equals(this.Value, "Permanent", StringComparison.Ordinal))
                {
                    this.internalDuration = InternalRedirectionDuration.Permanent;
                }
                else
                {
                    this.internalDuration = InternalRedirectionDuration.Custom;
                }
            }
            else
            {
                this.internalDuration = InternalRedirectionDuration.Custom;
            }
        }

        public override bool Equals(object obj)
        {
            bool flag = base.Equals(obj);
            if (!flag)
            {
                flag = (obj as RedirectionDuration) == this;
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

        public static bool operator ==(RedirectionDuration left, RedirectionDuration right)
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
            return ((left.InternalDuration == right.InternalDuration) || RedirectionUtility.IsNamespaceAndValueMatch(left.Value, left.Namespace, right.Value, right.Namespace));
        }

        public static bool operator !=(RedirectionDuration left, RedirectionDuration right)
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

        internal InternalRedirectionDuration InternalDuration
        {
            get
            {
                if (this.internalDuration == InternalRedirectionDuration.Unknown)
                {
                    this.DetectDuration();
                }
                return this.internalDuration;
            }
        }

        public string Namespace { get; private set; }

        public static RedirectionDuration Permanent
        {
            get
            {
                return permanent;
            }
        }

        public static RedirectionDuration Temporary
        {
            get
            {
                return temporary;
            }
        }

        public string Value { get; private set; }

        internal enum InternalRedirectionDuration
        {
            Unknown,
            Custom,
            Temporary,
            Permanent
        }
    }
}

