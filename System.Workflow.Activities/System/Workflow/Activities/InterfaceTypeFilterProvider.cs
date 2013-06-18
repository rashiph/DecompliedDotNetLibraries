namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel.Design;

    internal sealed class InterfaceTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InterfaceTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanFilterType(Type type, bool throwOnError)
        {
            if (throwOnError && !type.IsInterface)
            {
                throw new Exception(SR.GetString("Error_InterfaceTypeNotInterface", new object[] { "InterfaceType" }));
            }
            return type.IsInterface;
        }

        public string FilterDescription
        {
            get
            {
                return SR.GetString("InterfaceTypeFilterDescription");
            }
        }
    }
}

