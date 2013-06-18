namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;

    public sealed class WindowsServiceCredential
    {
        private bool allowAnonymousLogons;
        private bool includeWindowsGroups;
        private bool isReadOnly;

        internal WindowsServiceCredential()
        {
            this.includeWindowsGroups = true;
        }

        internal WindowsServiceCredential(WindowsServiceCredential other)
        {
            this.includeWindowsGroups = true;
            this.allowAnonymousLogons = other.allowAnonymousLogons;
            this.includeWindowsGroups = other.includeWindowsGroups;
            this.isReadOnly = other.isReadOnly;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public bool AllowAnonymousLogons
        {
            get
            {
                return this.allowAnonymousLogons;
            }
            set
            {
                this.ThrowIfImmutable();
                this.allowAnonymousLogons = value;
            }
        }

        public bool IncludeWindowsGroups
        {
            get
            {
                return this.includeWindowsGroups;
            }
            set
            {
                this.ThrowIfImmutable();
                this.includeWindowsGroups = value;
            }
        }
    }
}

