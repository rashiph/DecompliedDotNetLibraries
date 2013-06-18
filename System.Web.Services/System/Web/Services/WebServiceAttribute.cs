namespace System.Web.Services
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class WebServiceAttribute : System.Attribute
    {
        public const string DefaultNamespace = "http://tempuri.org/";
        private string description;
        private string name;
        private string ns = "http://tempuri.org/";

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
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.description = value;
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
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ns;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ns = value;
            }
        }
    }
}

