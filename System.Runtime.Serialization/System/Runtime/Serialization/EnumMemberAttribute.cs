namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Field, Inherited=false, AllowMultiple=false)]
    public sealed class EnumMemberAttribute : Attribute
    {
        private bool isValueSetExplicit;
        private string value;

        internal bool IsValueSetExplicit
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isValueSetExplicit;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                this.isValueSetExplicit = true;
            }
        }
    }
}

