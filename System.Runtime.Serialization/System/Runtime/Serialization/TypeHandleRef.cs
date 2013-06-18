namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    internal class TypeHandleRef
    {
        private RuntimeTypeHandle value;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypeHandleRef()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypeHandleRef(RuntimeTypeHandle value)
        {
            this.value = value;
        }

        public RuntimeTypeHandle Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.value = value;
            }
        }
    }
}

