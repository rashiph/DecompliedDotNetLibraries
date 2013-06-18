namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel.Design;

    internal class ExternalDataExchangeInterfaceTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalDataExchangeInterfaceTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanFilterType(Type type, bool throwOnError)
        {
            if (type.IsInterface)
            {
                if (type.GetCustomAttributes(typeof(ExternalDataExchangeAttribute), false).Length != 0)
                {
                    return true;
                }
                if (throwOnError)
                {
                    throw new Exception(SR.GetString("Error_InterfaceTypeNeedsExternalDataExchangeAttribute", new object[] { "InterfaceType" }));
                }
            }
            if (throwOnError)
            {
                throw new Exception(SR.GetString("Error_InterfaceTypeNotInterface", new object[] { "InterfaceType" }));
            }
            return false;
        }

        public string FilterDescription
        {
            get
            {
                return SR.GetString("ShowingExternalDataExchangeService");
            }
        }
    }
}

