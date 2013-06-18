namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class ActivityValidatorAttribute : Attribute
    {
        private string validatorTypeName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityValidatorAttribute(string validatorTypeName)
        {
            this.validatorTypeName = validatorTypeName;
        }

        public ActivityValidatorAttribute(Type validatorType)
        {
            if (validatorType != null)
            {
                this.validatorTypeName = validatorType.AssemblyQualifiedName;
            }
        }

        public string ValidatorTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validatorTypeName;
            }
        }
    }
}

