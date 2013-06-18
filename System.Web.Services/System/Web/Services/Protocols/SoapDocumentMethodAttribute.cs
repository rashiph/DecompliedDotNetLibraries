namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;
    using System.Web.Services.Description;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SoapDocumentMethodAttribute : Attribute
    {
        private string action;
        private string binding;
        private bool oneWay;
        private string requestName;
        private string requestNamespace;
        private string responseName;
        private string responseNamespace;
        private SoapParameterStyle style;
        private SoapBindingUse use;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapDocumentMethodAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SoapDocumentMethodAttribute(string action)
        {
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

        public SoapParameterStyle ParameterStyle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.style;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.style = value;
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

