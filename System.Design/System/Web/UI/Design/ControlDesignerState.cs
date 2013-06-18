namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public sealed class ControlDesignerState
    {
        private IComponent _component;
        private IDictionary _designerState;

        internal ControlDesignerState(IComponent component)
        {
            this._component = component;
        }

        public object this[string key]
        {
            get
            {
                if (this._designerState == null)
                {
                    if ((this._component != null) && (this._component.Site != null))
                    {
                        IComponentDesignerStateService service = (IComponentDesignerStateService) this._component.Site.GetService(typeof(IComponentDesignerStateService));
                        if (service != null)
                        {
                            return service.GetState(this._component, key);
                        }
                    }
                    this._designerState = new Hashtable();
                }
                return this._designerState[key];
            }
            set
            {
                if (this._designerState == null)
                {
                    if ((this._component != null) && (this._component.Site != null))
                    {
                        IComponentDesignerStateService service = (IComponentDesignerStateService) this._component.Site.GetService(typeof(IComponentDesignerStateService));
                        if (service != null)
                        {
                            service.SetState(this._component, key, value);
                            return;
                        }
                    }
                    this._designerState = new Hashtable();
                }
                this._designerState[key] = value;
            }
        }
    }
}

