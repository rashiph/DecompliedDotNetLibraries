namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Serialization;

    internal static class BindValidatorHelper
    {
        internal static Type GetActivityType(IServiceProvider serviceProvider, Activity refActivity)
        {
            Type type = null;
            string str = refActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            if ((refActivity.Site != null) && !string.IsNullOrEmpty(str))
            {
                ITypeProvider service = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if ((service != null) && !string.IsNullOrEmpty(str))
                {
                    type = service.GetType(str, false);
                }
                return type;
            }
            return refActivity.GetType();
        }
    }
}

