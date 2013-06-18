namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    internal sealed class NamePropertyDescriptor : DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal NamePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc) : base(serviceProvider, actualPropDesc)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            Activity activity = component as Activity;
            if (activity != null)
            {
                IIdentifierCreationService service = activity.Site.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IIdentifierCreationService).FullName }));
                }
                string identifier = value as string;
                service.ValidateIdentifier(activity, identifier);
                bool ignoreCase = CompilerHelpers.GetSupportedLanguage(activity.Site) == SupportedLanguages.VB;
                Type dataSourceClass = Helpers.GetDataSourceClass(Helpers.GetRootActivity(activity), activity.Site);
                if ((dataSourceClass != null) && (ActivityBindPropertyDescriptor.FindMatchingMember(identifier, dataSourceClass, ignoreCase) != null))
                {
                    throw new ArgumentException(SR.GetString("Error_ActivityNameExist", new object[] { identifier }));
                }
                IMemberCreationService service2 = activity.Site.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                if (service2 == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IMemberCreationService).FullName }));
                }
                IDesignerHost host = activity.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                }
                string newClassName = identifier;
                int num = host.RootComponentClassName.LastIndexOf('.');
                if (num > 0)
                {
                    newClassName = host.RootComponentClassName.Substring(0, num + 1) + identifier;
                }
                service2.UpdateTypeName(((Activity) host.RootComponent).GetValue(WorkflowMarkupSerializer.XClassProperty) as string, newClassName);
                ((Activity) host.RootComponent).SetValue(WorkflowMarkupSerializer.XClassProperty, newClassName);
                base.SetValue(component, value);
                DesignerHelpers.UpdateSiteName((Activity) host.RootComponent, identifier);
            }
        }

        public override string Category
        {
            get
            {
                return SR.GetString("Activity");
            }
        }

        public override string Description
        {
            get
            {
                return SR.GetString("RootActivityNameDesc");
            }
        }
    }
}

