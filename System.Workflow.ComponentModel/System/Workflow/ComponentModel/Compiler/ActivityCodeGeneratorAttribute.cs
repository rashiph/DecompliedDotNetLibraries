namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class ActivityCodeGeneratorAttribute : Attribute
    {
        private string codeGeneratorTypeName;

        public ActivityCodeGeneratorAttribute(string codeGeneratorTypeName)
        {
            if (codeGeneratorTypeName == null)
            {
                throw new ArgumentNullException("codeGeneratorTypeName");
            }
            this.codeGeneratorTypeName = codeGeneratorTypeName;
        }

        public ActivityCodeGeneratorAttribute(Type codeGeneratorType)
        {
            if (codeGeneratorType == null)
            {
                throw new ArgumentNullException("codeGeneratorType");
            }
            if (!typeof(ActivityCodeGenerator).IsAssignableFrom(codeGeneratorType))
            {
                throw new ArgumentException(SR.GetString("Error_NotCodeGeneratorType"), "codeGeneratorType");
            }
            if (codeGeneratorType.GetConstructor(new Type[0]) == null)
            {
                throw new ArgumentException(SR.GetString("Error_MissingDefaultConstructor", new object[] { codeGeneratorType.FullName }), "codeGeneratorType");
            }
            this.codeGeneratorTypeName = codeGeneratorType.AssemblyQualifiedName;
        }

        public string CodeGeneratorTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeGeneratorTypeName;
            }
        }
    }
}

