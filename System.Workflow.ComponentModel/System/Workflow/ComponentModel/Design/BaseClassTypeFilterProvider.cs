namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class BaseClassTypeFilterProvider : ITypeFilterProvider
    {
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BaseClassTypeFilterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool flag = false;
            if (((TypeProvider.IsAssignable(typeof(Activity), type) && type.IsPublic) && (!type.IsSealed && !type.IsAbstract)) && !(type is DesignTimeType))
            {
                flag = true;
            }
            return flag;
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString("CustomActivityBaseClassTypeFilterProviderDesc");
            }
        }
    }
}

