namespace System.Web.Services
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true)]
    public sealed class WebServiceBindingAttribute : System.Attribute
    {
        private WsiProfiles claims;
        private bool emitClaims;
        private string location;
        private string name;
        private string ns;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceBindingAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceBindingAttribute(string name)
        {
            this.name = name;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceBindingAttribute(string name, string ns)
        {
            this.name = name;
            this.ns = ns;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceBindingAttribute(string name, string ns, string location)
        {
            this.name = name;
            this.ns = ns;
            this.location = location;
        }

        public WsiProfiles ConformsTo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.claims;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.claims = value;
            }
        }

        public bool EmitConformanceClaims
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.emitClaims;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.emitClaims = value;
            }
        }

        public string Location
        {
            get
            {
                if (this.location != null)
                {
                    return this.location;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.location = value;
            }
        }

        public string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                if (this.ns != null)
                {
                    return this.ns;
                }
                return string.Empty;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ns = value;
            }
        }
    }
}

