namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited=true, AllowMultiple=true)]
    public sealed class KnownTypeAttribute : Attribute
    {
        private string methodName;
        private System.Type type;

        private KnownTypeAttribute()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public KnownTypeAttribute(string methodName)
        {
            this.methodName = methodName;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public KnownTypeAttribute(System.Type type)
        {
            this.type = type;
        }

        public string MethodName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.methodName;
            }
        }

        public System.Type Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.type;
            }
        }
    }
}

