namespace System.Web.Services.Protocols
{
    using System;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class SoapHeaderMapping
    {
        internal bool custom;
        internal SoapHeaderDirection direction;
        internal Type headerType;
        internal System.Reflection.MemberInfo memberInfo;
        internal bool repeats;

        internal SoapHeaderMapping()
        {
        }

        public bool Custom
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.custom;
            }
        }

        public SoapHeaderDirection Direction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.direction;
            }
        }

        public Type HeaderType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.headerType;
            }
        }

        public System.Reflection.MemberInfo MemberInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.memberInfo;
            }
        }

        public bool Repeats
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.repeats;
            }
        }
    }
}

