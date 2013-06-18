namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public sealed class DesignerActionUIService : IDisposable
    {
        private DesignerActionService designerActionService;
        private IServiceProvider serviceProvider;

        public event DesignerActionUIStateChangeEventHandler DesignerActionUIStateChange;

        internal DesignerActionUIService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            if (serviceProvider != null)
            {
                this.serviceProvider = serviceProvider;
                ((IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost))).AddService(typeof(DesignerActionUIService), this);
                this.designerActionService = serviceProvider.GetService(typeof(DesignerActionService)) as DesignerActionService;
            }
        }

        public void Dispose()
        {
            if (this.serviceProvider != null)
            {
                IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    service.RemoveService(typeof(DesignerActionUIService));
                }
            }
        }

        public void HideUI(IComponent component)
        {
            this.OnDesignerActionUIStateChange(new DesignerActionUIStateChangeEventArgs(component, DesignerActionUIStateChangeType.Hide));
        }

        private void OnDesignerActionUIStateChange(DesignerActionUIStateChangeEventArgs e)
        {
            if (this.designerActionUIStateChangedEventHandler != null)
            {
                this.designerActionUIStateChangedEventHandler(this, e);
            }
        }

        public void Refresh(IComponent component)
        {
            this.OnDesignerActionUIStateChange(new DesignerActionUIStateChangeEventArgs(component, DesignerActionUIStateChangeType.Refresh));
        }

        public bool ShouldAutoShow(IComponent component)
        {
            if (this.serviceProvider != null)
            {
                DesignerOptionService service = this.serviceProvider.GetService(typeof(DesignerOptionService)) as DesignerOptionService;
                if (service != null)
                {
                    PropertyDescriptor descriptor = service.Options.Properties["ObjectBoundSmartTagAutoShow"];
                    if (((descriptor != null) && (descriptor.PropertyType == typeof(bool))) && !((bool) descriptor.GetValue(null)))
                    {
                        return false;
                    }
                }
            }
            if (this.designerActionService != null)
            {
                DesignerActionListCollection componentActions = this.designerActionService.GetComponentActions(component);
                if ((componentActions != null) && (componentActions.Count > 0))
                {
                    for (int i = 0; i < componentActions.Count; i++)
                    {
                        if (componentActions[i].AutoShow)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void ShowUI(IComponent component)
        {
            this.OnDesignerActionUIStateChange(new DesignerActionUIStateChangeEventArgs(component, DesignerActionUIStateChangeType.Show));
        }
    }
}

