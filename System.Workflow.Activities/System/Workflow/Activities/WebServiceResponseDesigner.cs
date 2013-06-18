namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(WebServiceResponseDesignerTheme))]
    internal sealed class WebServiceResponseDesigner : ActivityDesigner
    {
        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if ((e.Member != null) && (e.Member.Name == "InputActivityName"))
            {
                (e.Activity as WebServiceOutputActivity).ParameterBindings.Clear();
                TypeDescriptor.Refresh(e.Activity);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (((ITypeProvider) base.GetService(typeof(ITypeProvider))) == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            (base.Activity as WebServiceOutputActivity).GetParameterPropertyDescriptors(properties);
        }
    }
}

