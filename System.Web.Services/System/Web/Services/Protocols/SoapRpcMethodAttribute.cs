namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Web.Services.Description;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SoapRpcMethodAttribute : Attribute
    {
        private string action;
        private string binding;
        private bool oneWay;
        private string requestName;
        private string requestNamespace;
        private string responseName;
        private string responseNamespace;
        private SoapBindingUse use;

        public SoapRpcMethodAttribute()
        {
            this.use = SoapBindingUse.Encoded;
        }

        public SoapRpcMethodAttribute(string action)
        {
            this.use = SoapBindingUse.Encoded;
            this.action = action;
        }

        public string Action
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.action;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.action = value;
            }
        }

        public string Binding
        {
            get
            {
                if (this.binding != null)
                {
                    return this.binding;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.binding = value;
            }
        }

        public bool OneWay
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.oneWay;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.oneWay = value;
            }
        }

        public string RequestElementName
        {
            get
            {
                if (this.requestName != null)
                {
                    return this.requestName;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.requestName = value;
            }
        }

        public string RequestNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.requestNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.requestNamespace = value;
            }
        }

        public string ResponseElementName
        {
            get
            {
                if (this.responseName != null)
                {
                    return this.responseName;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.responseName = value;
            }
        }

        public string ResponseNamespace
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.responseNamespace;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.responseNamespace = value;
            }
        }

        [ComVisible(false)]
        public SoapBindingUse Use
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.use;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.use = value;
            }
        }
    }
}

