namespace System.Web.Services
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebMethodAttribute : System.Attribute
    {
        private bool bufferResponse;
        private bool bufferResponseSpecified;
        private int cacheDuration;
        private bool cacheDurationSpecified;
        private string description;
        private bool descriptionSpecified;
        private bool enableSession;
        private bool enableSessionSpecified;
        private string messageName;
        private bool messageNameSpecified;
        private int transactionOption;
        private bool transactionOptionSpecified;

        public WebMethodAttribute()
        {
            this.enableSession = false;
            this.transactionOption = 0;
            this.cacheDuration = 0;
            this.bufferResponse = true;
        }

        public WebMethodAttribute(bool enableSession) : this()
        {
            this.EnableSession = enableSession;
        }

        public WebMethodAttribute(bool enableSession, System.EnterpriseServices.TransactionOption transactionOption) : this()
        {
            this.EnableSession = enableSession;
            this.transactionOption = (int) transactionOption;
            this.transactionOptionSpecified = true;
        }

        public WebMethodAttribute(bool enableSession, System.EnterpriseServices.TransactionOption transactionOption, int cacheDuration)
        {
            this.EnableSession = enableSession;
            this.transactionOption = (int) transactionOption;
            this.transactionOptionSpecified = true;
            this.CacheDuration = cacheDuration;
            this.BufferResponse = true;
        }

        public WebMethodAttribute(bool enableSession, System.EnterpriseServices.TransactionOption transactionOption, int cacheDuration, bool bufferResponse)
        {
            this.EnableSession = enableSession;
            this.transactionOption = (int) transactionOption;
            this.transactionOptionSpecified = true;
            this.CacheDuration = cacheDuration;
            this.BufferResponse = bufferResponse;
        }

        public bool BufferResponse
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bufferResponse;
            }
            set
            {
                this.bufferResponse = value;
                this.bufferResponseSpecified = true;
            }
        }

        internal bool BufferResponseSpecified
        {
            get
            {
                return this.bufferResponseSpecified;
            }
        }

        public int CacheDuration
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.cacheDuration;
            }
            set
            {
                this.cacheDuration = value;
                this.cacheDurationSpecified = true;
            }
        }

        internal bool CacheDurationSpecified
        {
            get
            {
                return this.cacheDurationSpecified;
            }
        }

        public string Description
        {
            get
            {
                if (this.description != null)
                {
                    return this.description;
                }
                return string.Empty;
            }
            set
            {
                this.description = value;
                this.descriptionSpecified = true;
            }
        }

        internal bool DescriptionSpecified
        {
            get
            {
                return this.descriptionSpecified;
            }
        }

        public bool EnableSession
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enableSession;
            }
            set
            {
                this.enableSession = value;
                this.enableSessionSpecified = true;
            }
        }

        internal bool EnableSessionSpecified
        {
            get
            {
                return this.enableSessionSpecified;
            }
        }

        public string MessageName
        {
            get
            {
                if (this.messageName != null)
                {
                    return this.messageName;
                }
                return string.Empty;
            }
            set
            {
                this.messageName = value;
                this.messageNameSpecified = true;
            }
        }

        internal bool MessageNameSpecified
        {
            get
            {
                return this.messageNameSpecified;
            }
        }

        internal bool TransactionEnabled
        {
            get
            {
                return (this.transactionOption != 0);
            }
        }

        public System.EnterpriseServices.TransactionOption TransactionOption
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return (System.EnterpriseServices.TransactionOption) this.transactionOption;
            }
            set
            {
                this.transactionOption = (int) value;
                this.transactionOptionSpecified = true;
            }
        }

        internal bool TransactionOptionSpecified
        {
            get
            {
                return this.transactionOptionSpecified;
            }
        }
    }
}

