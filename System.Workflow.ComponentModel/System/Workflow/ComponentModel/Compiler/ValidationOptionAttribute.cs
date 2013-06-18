namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public sealed class ValidationOptionAttribute : Attribute
    {
        private System.Workflow.ComponentModel.Compiler.ValidationOption validationOption;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ValidationOptionAttribute(System.Workflow.ComponentModel.Compiler.ValidationOption validationOption)
        {
            this.validationOption = validationOption;
        }

        public System.Workflow.ComponentModel.Compiler.ValidationOption ValidationOption
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.validationOption;
            }
        }
    }
}

