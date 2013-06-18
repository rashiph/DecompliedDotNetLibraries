namespace System.Web.Services.Protocols
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public sealed class SoapHeaderAttribute : Attribute
    {
        private SoapHeaderDirection direction = SoapHeaderDirection.In;
        private string memberName;
        private bool required = true;

        public SoapHeaderAttribute(string memberName)
        {
            this.memberName = memberName;
        }

        public SoapHeaderDirection Direction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.direction;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.direction = value;
            }
        }

        public string MemberName
        {
            get
            {
                if (this.memberName != null)
                {
                    return this.memberName;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.memberName = value;
            }
        }

        [Obsolete("This property will be removed from a future version. The presence of a particular header in a SOAP message is no longer enforced", false)]
        public bool Required
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.required;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.required = value;
            }
        }
    }
}

