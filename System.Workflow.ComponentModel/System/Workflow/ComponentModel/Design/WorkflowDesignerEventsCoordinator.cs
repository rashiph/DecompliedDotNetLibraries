namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WorkflowDesignerEventsCoordinator : IDisposable
    {
        private EventHandler refreshDesignerActionsHandler;
        private EventHandler refreshTasksHandler;
        private EventHandler refreshTypesHandler;
        private IDesignerLoaderHost serviceProvider;
        private bool typeSystemTypesChanged;

        public WorkflowDesignerEventsCoordinator(IDesignerLoaderHost serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.serviceProvider.LoadComplete += new EventHandler(this.OnDesignerReloaded);
            IDesignerEventService service = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if (service != null)
            {
                service.ActiveDesignerChanged += new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
            }
            ITypeProvider provider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider != null)
            {
                provider.TypesChanged += new EventHandler(this.OnTypeSystemTypesChanged);
            }
            ISelectionService service2 = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service2 != null)
            {
                service2.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            IComponentChangeService service3 = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service3 != null)
            {
                service3.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            IPropertyValueUIService service4 = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (service4 != null)
            {
                service4.AddPropertyValueUIHandler(new PropertyValueUIHandler(this.OnPropertyGridAdornments));
            }
        }

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
        {
            if ((e.NewDesigner == this.serviceProvider.GetService(typeof(IDesignerHost))) && this.typeSystemTypesChanged)
            {
                this.RefreshTypes();
            }
            else
            {
                this.RefreshTasks();
            }
        }

        private void OnBindProperty(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
        {
            BindUITypeEditor.EditValue(context);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs eventArgs)
        {
            this.RefreshDesignerActions();
        }

        private void OnDesignerReloaded(object sender, EventArgs e)
        {
            bool flag = this.refreshTypesHandler != null;
            bool flag2 = this.refreshDesignerActionsHandler != null;
            bool flag3 = this.refreshTasksHandler != null;
            this.refreshTypesHandler = null;
            this.refreshDesignerActionsHandler = null;
            this.refreshTasksHandler = null;
            if ((flag || flag3) || flag2)
            {
                this.RefreshTypes();
                this.RefreshDesignerActions();
            }
        }

        private void OnPropertyGridAdornments(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            IComponent reference = null;
            IReferenceService service = this.serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (service != null)
            {
                reference = service.GetComponent(context.Instance);
            }
            string str = string.Empty;
            DefaultPropertyAttribute attribute = propDesc.Attributes[typeof(DefaultPropertyAttribute)] as DefaultPropertyAttribute;
            if (((attribute != null) && (attribute.Name != null)) && (attribute.Name.Length > 0))
            {
                str = propDesc.Name + "." + attribute.Name;
            }
            if (reference != null)
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(reference as Activity);
                if (designer != null)
                {
                    if ((!designer.IsLocked && ActivityBindPropertyDescriptor.IsBindableProperty(propDesc)) && !propDesc.IsReadOnly)
                    {
                        valueUIItemList.Add(new PropertyValueUIItem(DR.GetImage("Bind"), new PropertyValueUIItemInvokeHandler(this.OnBindProperty), DR.GetString("BindProperty", new object[0])));
                    }
                    string name = service.GetName(reference);
                    string str3 = service.GetName(context.Instance);
                    str3 = (str3.Length > name.Length) ? (str3.Substring(name.Length + 1, (str3.Length - name.Length) - 1) + "." + propDesc.Name) : string.Empty;
                    foreach (DesignerAction action in designer.DesignerActions)
                    {
                        string propertyName = action.PropertyName;
                        if (((propertyName != null) && (propertyName.Length != 0)) && (((propertyName == propDesc.Name) || (propertyName == str3)) || (propertyName == str)))
                        {
                            PropertyValueUIItemHandler handler = new PropertyValueUIItemHandler(action);
                            valueUIItemList.Add(new PropertyValueUIItem(action.Image, new PropertyValueUIItemInvokeHandler(handler.OnFixPropertyError), action.Text));
                            break;
                        }
                    }
                }
            }
        }

        private void OnRefreshDesignerActions(object sender, EventArgs e)
        {
            WorkflowView view = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.refreshDesignerActionsHandler != null)
            {
                if (view != null)
                {
                    view.Idle -= this.refreshDesignerActionsHandler;
                }
                this.refreshDesignerActionsHandler = null;
            }
            DesignerHelpers.RefreshDesignerActions(this.serviceProvider);
            IPropertyValueUIService service = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (service != null)
            {
                service.NotifyPropertyValueUIItemsChanged();
            }
            this.RefreshTasks();
        }

        private void OnRefreshTasks(object sender, EventArgs e)
        {
            WorkflowView view = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (this.refreshTasksHandler != null)
            {
                if (view != null)
                {
                    view.Idle -= this.refreshTasksHandler;
                }
                this.refreshTasksHandler = null;
            }
            ISelectionService service = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            IExtendedUIService service2 = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if ((service != null) && (service2 != null))
            {
                service2.RemoveDesignerActions();
                IDesignerEventService service3 = (IDesignerEventService) this.serviceProvider.GetService(typeof(IDesignerEventService));
                if ((service3 != null) && (service3.ActiveDesigner == this.serviceProvider.GetService(typeof(IDesignerHost))))
                {
                    foreach (object obj2 in service.GetSelectedComponents())
                    {
                        ActivityDesigner associatedDesigner = null;
                        if (obj2 is HitTestInfo)
                        {
                            associatedDesigner = ((HitTestInfo) obj2).AssociatedDesigner;
                        }
                        else if (obj2 is Activity)
                        {
                            associatedDesigner = ActivityDesigner.GetDesigner(obj2 as Activity);
                        }
                        if (associatedDesigner != null)
                        {
                            service2.AddDesignerActions(new List<DesignerAction>(associatedDesigner.DesignerActions).ToArray());
                        }
                    }
                }
            }
            if (view != null)
            {
                view.Invalidate();
            }
        }

        private void OnRefreshTypes(object sender, EventArgs e)
        {
            ITypeProvider typeProvider;
            if (this.refreshTypesHandler != null)
            {
                WorkflowView view = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (view != null)
                {
                    view.Idle -= this.refreshTypesHandler;
                }
                this.refreshTypesHandler = null;
            }
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            Activity seedActivity = (host != null) ? (host.RootComponent as Activity) : null;
            if (seedActivity != null)
            {
                typeProvider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null)
                {
                    Walker walker = new Walker();
                    walker.FoundProperty += delegate (Walker w, WalkerEventArgs args) {
                        if (((args.CurrentValue != null) && (args.CurrentProperty != null)) && ((args.CurrentProperty.PropertyType == typeof(Type)) && (args.CurrentValue is Type)))
                        {
                            Type type = typeProvider.GetType(((Type) args.CurrentValue).FullName);
                            if (type != null)
                            {
                                args.CurrentProperty.SetValue(args.CurrentPropertyOwner, type, null);
                                if (args.CurrentActivity != null)
                                {
                                    TypeDescriptor.Refresh(args.CurrentActivity);
                                }
                            }
                        }
                        else if (((args.CurrentProperty == null) && (args.CurrentValue is DependencyObject)) && !(args.CurrentValue is Activity))
                        {
                            walker.WalkProperties(args.CurrentActivity, args.CurrentValue);
                        }
                    };
                    walker.FoundActivity += delegate (Walker w, WalkerEventArgs args) {
                        if (args.CurrentActivity != null)
                        {
                            TypeDescriptor.Refresh(args.CurrentActivity);
                            ActivityDesigner designer = ActivityDesigner.GetDesigner(args.CurrentActivity);
                            if (designer != null)
                            {
                                designer.RefreshDesignerActions();
                            }
                            InvokeWorkflowDesigner designer2 = designer as InvokeWorkflowDesigner;
                            if (designer2 != null)
                            {
                                designer2.RefreshTargetWorkflowType();
                            }
                        }
                    };
                    walker.Walk(seedActivity);
                }
                IPropertyValueUIService service = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
                if (service != null)
                {
                    service.NotifyPropertyValueUIItemsChanged();
                }
                this.RefreshTasks();
                this.RefreshDesignerActions();
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (service != null)
            {
                service.Invalidate();
            }
            this.RefreshTasks();
        }

        private void OnTypeSystemTypesChanged(object sender, EventArgs e)
        {
            this.typeSystemTypesChanged = true;
            IDesignerEventService service = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if ((service != null) && (service.ActiveDesigner == this.serviceProvider.GetService(typeof(IDesignerHost))))
            {
                this.RefreshTypes();
            }
        }

        private void RefreshDesignerActions()
        {
            if (this.refreshDesignerActionsHandler == null)
            {
                WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    this.refreshDesignerActionsHandler = new EventHandler(this.OnRefreshDesignerActions);
                    service.Idle += this.refreshDesignerActionsHandler;
                }
            }
        }

        private void RefreshTasks()
        {
            if (this.refreshTasksHandler == null)
            {
                WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    this.refreshTasksHandler = new EventHandler(this.OnRefreshTasks);
                    service.Idle += this.refreshTasksHandler;
                }
            }
        }

        private void RefreshTypes()
        {
            if ((this.refreshTypesHandler == null) && this.typeSystemTypesChanged)
            {
                WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    this.refreshTypesHandler = new EventHandler(this.OnRefreshTypes);
                    service.Idle += this.refreshTypesHandler;
                }
            }
            this.typeSystemTypesChanged = false;
        }

        void IDisposable.Dispose()
        {
            WorkflowView view = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (view != null)
            {
                if (this.refreshTypesHandler != null)
                {
                    view.Idle -= this.refreshTypesHandler;
                }
                if (this.refreshDesignerActionsHandler != null)
                {
                    view.Idle -= this.refreshDesignerActionsHandler;
                }
                if (this.refreshTasksHandler != null)
                {
                    view.Idle -= this.refreshTasksHandler;
                }
            }
            this.refreshTypesHandler = null;
            this.refreshDesignerActionsHandler = null;
            this.refreshTasksHandler = null;
            IExtendedUIService service = this.serviceProvider.GetService(typeof(IExtendedUIService)) as IExtendedUIService;
            if (service != null)
            {
                service.RemoveDesignerActions();
            }
            IPropertyValueUIService service2 = this.serviceProvider.GetService(typeof(IPropertyValueUIService)) as IPropertyValueUIService;
            if (service2 != null)
            {
                service2.RemovePropertyValueUIHandler(new PropertyValueUIHandler(this.OnPropertyGridAdornments));
            }
            IComponentChangeService service3 = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service3 != null)
            {
                service3.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            ISelectionService service4 = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service4 != null)
            {
                service4.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
            }
            ITypeProvider provider = this.serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider != null)
            {
                provider.TypesChanged -= new EventHandler(this.OnTypeSystemTypesChanged);
            }
            IDesignerEventService service5 = this.serviceProvider.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
            if (service5 != null)
            {
                service5.ActiveDesignerChanged -= new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
            }
            this.serviceProvider.LoadComplete -= new EventHandler(this.OnDesignerReloaded);
        }

        private class PropertyValueUIItemHandler
        {
            private DesignerAction action;

            internal PropertyValueUIItemHandler(DesignerAction action)
            {
                this.action = action;
            }

            internal void OnFixPropertyError(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
            {
                this.action.Invoke();
            }
        }
    }
}

