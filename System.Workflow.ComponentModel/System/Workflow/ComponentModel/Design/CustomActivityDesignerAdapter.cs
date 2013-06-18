namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class CustomActivityDesignerAdapter : IDisposable
    {
        private EventHandler ensureChildHierarchyHandler;
        private IServiceProvider serviceProvider;

        public CustomActivityDesignerAdapter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentAdding += new ComponentEventHandler(this.OnComponentAdding);
                service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
            }
        }

        private static void EnsureDefaultChildHierarchy(IDesignerHost designerHost)
        {
            CompositeActivity rootComponent = designerHost.RootComponent as CompositeActivity;
            if ((rootComponent != null) && (rootComponent.Activities.Count == 0))
            {
                object[] customAttributes = rootComponent.GetType().GetCustomAttributes(typeof(ToolboxItemAttribute), false);
                ToolboxItemAttribute attribute = ((customAttributes != null) && (customAttributes.GetLength(0) > 0)) ? (customAttributes[0] as ToolboxItemAttribute) : null;
                if ((attribute != null) && (attribute.ToolboxItemType != null))
                {
                    IComponent[] componentArray = (Activator.CreateInstance(attribute.ToolboxItemType, new object[] { rootComponent.GetType() }) as ToolboxItem).CreateComponents();
                    CompositeActivity activity2 = null;
                    foreach (IComponent component in componentArray)
                    {
                        if (component.GetType() == rootComponent.GetType())
                        {
                            activity2 = component as CompositeActivity;
                            break;
                        }
                    }
                    if ((activity2 != null) && (activity2.Activities.Count > 0))
                    {
                        IIdentifierCreationService service = designerHost.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                        if (service != null)
                        {
                            Activity[] childActivities = activity2.Activities.ToArray();
                            activity2.Activities.Clear();
                            service.EnsureUniqueIdentifiers(rootComponent, childActivities);
                            foreach (Activity activity3 in childActivities)
                            {
                                rootComponent.Activities.Add(activity3);
                            }
                            foreach (Activity activity4 in childActivities)
                            {
                                WorkflowDesignerLoader.AddActivityToDesigner(designerHost, activity4);
                            }
                        }
                    }
                }
            }
        }

        private static DesignerAttribute GetDesignerAttribute(object component, System.Type designerBaseType)
        {
            foreach (Attribute attribute in TypeDescriptor.GetAttributes(component))
            {
                DesignerAttribute attribute2 = attribute as DesignerAttribute;
                if ((attribute2 != null) && (attribute2.DesignerBaseTypeName == designerBaseType.AssemblyQualifiedName))
                {
                    return attribute2;
                }
            }
            return null;
        }

        private void OnComponentAdded(object sender, ComponentEventArgs eventArgs)
        {
            IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                if (service.RootComponent == eventArgs.Component)
                {
                    Activity rootComponent = service.RootComponent as Activity;
                    if (rootComponent != null)
                    {
                        if ((rootComponent is CompositeActivity) && (this.ensureChildHierarchyHandler == null))
                        {
                            this.ensureChildHierarchyHandler = new EventHandler(this.OnEnsureChildHierarchy);
                            Application.Idle += this.ensureChildHierarchyHandler;
                        }
                        rootComponent.UserData[UserDataKeys.CustomActivity] = false;
                    }
                }
                else if (eventArgs.Component is Activity)
                {
                    if ((eventArgs.Component is CompositeActivity) && Helpers.IsCustomActivity(eventArgs.Component as CompositeActivity))
                    {
                        (eventArgs.Component as Activity).UserData[UserDataKeys.CustomActivity] = true;
                    }
                    else
                    {
                        (eventArgs.Component as Activity).UserData[UserDataKeys.CustomActivity] = false;
                    }
                }
            }
        }

        private void OnComponentAdding(object sender, ComponentEventArgs eventArgs)
        {
            IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if ((service != null) && (service.RootComponent == null))
            {
                Activity component = eventArgs.Component as Activity;
                if ((component != null) && (GetDesignerAttribute(component, typeof(IRootDesigner)).DesignerTypeName == typeof(ActivityDesigner).AssemblyQualifiedName))
                {
                    DesignerAttribute designerAttribute = GetDesignerAttribute(component, typeof(IDesigner));
                    if (designerAttribute != null)
                    {
                        TypeDescriptor.AddAttributes(component, new Attribute[] { new DesignerAttribute(designerAttribute.DesignerTypeName, typeof(IRootDesigner)) });
                    }
                }
            }
        }

        private void OnEnsureChildHierarchy(object sender, EventArgs e)
        {
            if (this.ensureChildHierarchyHandler != null)
            {
                Application.Idle -= this.ensureChildHierarchyHandler;
                this.ensureChildHierarchyHandler = null;
                IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    EnsureDefaultChildHierarchy(service);
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (this.ensureChildHierarchyHandler != null)
            {
                Application.Idle -= this.ensureChildHierarchyHandler;
                this.ensureChildHierarchyHandler = null;
            }
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service != null)
            {
                service.ComponentAdding -= new ComponentEventHandler(this.OnComponentAdding);
                service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
            }
        }
    }
}

