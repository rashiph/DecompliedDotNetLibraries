namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    public sealed class BindValidationContext
    {
        private AccessTypes access;
        private Type targetType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BindValidationContext(Type targetType) : this(targetType, AccessTypes.Read)
        {
        }

        public BindValidationContext(Type targetType, AccessTypes access)
        {
            this.access = AccessTypes.Read;
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            this.targetType = targetType;
            this.access = access;
        }

        public AccessTypes Access
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.access;
            }
        }

        public Type TargetType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetType;
            }
        }
    }
}

