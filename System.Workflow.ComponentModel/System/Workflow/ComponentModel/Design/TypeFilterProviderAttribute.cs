namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple=false)]
    public sealed class TypeFilterProviderAttribute : Attribute
    {
        private string typeName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TypeFilterProviderAttribute(string typeName)
        {
            this.typeName = typeName;
        }

        public TypeFilterProviderAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.typeName = type.AssemblyQualifiedName;
        }

        public string TypeFilterProviderTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.typeName;
            }
        }
    }
}

