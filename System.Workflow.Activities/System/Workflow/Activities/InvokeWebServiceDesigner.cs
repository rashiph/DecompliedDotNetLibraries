namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(InvokeWebServiceDesignerTheme))]
    internal sealed class InvokeWebServiceDesigner : ActivityDesigner
    {
        private string url;

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (e.Member != null)
            {
                if ((e.Member.Name == "ProxyClass") && (base.Activity.Site != null))
                {
                    InvokeWebServiceActivity activity = e.Activity as InvokeWebServiceActivity;
                    System.Workflow.Activities.Common.PropertyDescriptorUtils.SetPropertyValue(base.Activity.Site, TypeDescriptor.GetProperties(base.Activity)["MethodName"], base.Activity, string.Empty);
                    IExtendedUIService service = (IExtendedUIService) base.Activity.Site.GetService(typeof(IExtendedUIService));
                    if (service == null)
                    {
                        throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IExtendedUIService).FullName }));
                    }
                    if (activity.ProxyClass == null)
                    {
                        this.url = null;
                    }
                    else
                    {
                        Uri urlForProxyClass = service.GetUrlForProxyClass(activity.ProxyClass);
                        this.url = (urlForProxyClass != null) ? urlForProxyClass.ToString() : string.Empty;
                    }
                }
                if (((e.Member.Name == "MethodName") || (e.Member.Name == "TargetWorkflow")) && (e.Activity is InvokeWebServiceActivity))
                {
                    (e.Activity as InvokeWebServiceActivity).ParameterBindings.Clear();
                }
                if ((e.Member.Name == "ProxyClass") || (e.Member.Name == "MethodName"))
                {
                    TypeDescriptor.Refresh(e.Activity);
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (properties["URL"] == null)
            {
                properties["URL"] = new WebServiceUrlPropertyDescriptor(base.Activity.Site, TypeDescriptor.CreateProperty(base.GetType(), "URL", typeof(string), new Attribute[] { DesignOnlyAttribute.Yes, MergablePropertyAttribute.No }));
            }
            if (((ITypeProvider) base.GetService(typeof(ITypeProvider))) == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            (base.Activity as InvokeWebServiceActivity).GetParameterPropertyDescriptors(properties);
        }

        [RefreshProperties(RefreshProperties.All), SRCategory("Activity"), SRDescription("URLDescr"), Editor(typeof(WebServicePickerEditor), typeof(UITypeEditor))]
        public string URL
        {
            get
            {
                if (this.url == null)
                {
                    InvokeWebServiceActivity activity = base.Activity as InvokeWebServiceActivity;
                    IExtendedUIService service = (IExtendedUIService) base.Activity.Site.GetService(typeof(IExtendedUIService));
                    if ((service != null) && (activity.ProxyClass != null))
                    {
                        Uri urlForProxyClass = service.GetUrlForProxyClass(activity.ProxyClass);
                        this.url = (urlForProxyClass != null) ? urlForProxyClass.ToString() : string.Empty;
                    }
                }
                return this.url;
            }
            set
            {
                if (this.url != value)
                {
                    this.url = value;
                    IExtendedUIService service = (IExtendedUIService) base.Activity.Site.GetService(typeof(IExtendedUIService));
                    if (service == null)
                    {
                        throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IExtendedUIService).FullName }));
                    }
                    DesignerTransaction transaction = null;
                    IDesignerHost host = base.Activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null)
                    {
                        transaction = host.CreateTransaction(SR.GetString("ChangingVariable"));
                    }
                    try
                    {
                        System.Workflow.Activities.Common.PropertyDescriptorUtils.SetPropertyValue(base.Activity.Site, TypeDescriptor.GetProperties(base.Activity)["ProxyClass"], base.Activity, string.IsNullOrEmpty(this.url) ? null : service.GetProxyClassForUrl(new Uri(this.url)));
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            ((IDisposable) transaction).Dispose();
                        }
                    }
                }
            }
        }
    }
}

