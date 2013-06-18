namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple=true), CLSCompliant(false)]
    public sealed class AttributeInfoAttribute : Attribute
    {
        private System.Workflow.ComponentModel.Compiler.AttributeInfo attributeInfo;

        internal AttributeInfoAttribute(System.Workflow.ComponentModel.Compiler.AttributeInfo attributeInfo)
        {
            if (attributeInfo == null)
            {
                throw new ArgumentNullException("attributeInfo");
            }
            this.attributeInfo = attributeInfo;
        }

        internal static AttributeInfoAttribute CreateAttributeInfoAttribute(Type attributeType, string[] argumentNames, object[] argumentValues)
        {
            return new AttributeInfoAttribute(new System.Workflow.ComponentModel.Compiler.AttributeInfo(attributeType, argumentNames, argumentValues));
        }

        public System.Workflow.ComponentModel.Compiler.AttributeInfo AttributeInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.attributeInfo;
            }
        }
    }
}

