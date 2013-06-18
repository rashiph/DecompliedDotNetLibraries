namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class CollectionEditVerbManager : IWindowsFormsEditorService, ITypeDescriptorContext, IServiceProvider
    {
        private IComponentChangeService _componentChangeSvc;
        private ComponentDesigner _designer;
        private DesignerVerb _editItemsVerb;
        private PropertyDescriptor _targetProperty;

        internal CollectionEditVerbManager(string text, ComponentDesigner designer, PropertyDescriptor prop, bool addToDesignerVerbs)
        {
            this._designer = designer;
            this._targetProperty = prop;
            if (prop == null)
            {
                prop = TypeDescriptor.GetDefaultProperty(designer.Component);
                if ((prop != null) && typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                {
                    this._targetProperty = prop;
                }
            }
            if (text == null)
            {
                text = System.Design.SR.GetString("ToolStripItemCollectionEditorVerb");
            }
            this._editItemsVerb = new DesignerVerb(text, new EventHandler(this.OnEditItems));
            if (addToDesignerVerbs)
            {
                this._designer.Verbs.Add(this._editItemsVerb);
            }
        }

        private void OnEditItems(object sender, EventArgs e)
        {
            DesignerActionUIService service = (DesignerActionUIService) ((IServiceProvider) this).GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.HideUI(this._designer.Component);
            }
            object component = this._targetProperty.GetValue(this._designer.Component);
            if (component != null)
            {
                CollectionEditor editor = TypeDescriptor.GetEditor(component, typeof(UITypeEditor)) as CollectionEditor;
                if (editor != null)
                {
                    editor.EditValue(this, this, component);
                }
            }
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
            this.ChangeService.OnComponentChanged(this._designer.Component, this._targetProperty, null, null);
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            try
            {
                this.ChangeService.OnComponentChanging(this._designer.Component, this._targetProperty);
            }
            catch (CheckoutException exception)
            {
                if (exception != CheckoutException.Canceled)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if ((serviceType == typeof(ITypeDescriptorContext)) || (serviceType == typeof(IWindowsFormsEditorService)))
            {
                return this;
            }
            if (this._designer.Component.Site != null)
            {
                return this._designer.Component.Site.GetService(serviceType);
            }
            return null;
        }

        void IWindowsFormsEditorService.CloseDropDown()
        {
        }

        void IWindowsFormsEditorService.DropDownControl(Control control)
        {
        }

        DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
        {
            IUIService service = (IUIService) ((IServiceProvider) this).GetService(typeof(IUIService));
            if (service != null)
            {
                return service.ShowDialog(dialog);
            }
            return dialog.ShowDialog(this._designer.Component as IWin32Window);
        }

        private IComponentChangeService ChangeService
        {
            get
            {
                if (this._componentChangeSvc == null)
                {
                    this._componentChangeSvc = (IComponentChangeService) ((IServiceProvider) this).GetService(typeof(IComponentChangeService));
                }
                return this._componentChangeSvc;
            }
        }

        public DesignerVerb EditItemsVerb
        {
            get
            {
                return this._editItemsVerb;
            }
        }

        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                if (this._designer.Component.Site != null)
                {
                    return this._designer.Component.Site.Container;
                }
                return null;
            }
        }

        object ITypeDescriptorContext.Instance
        {
            get
            {
                return this._designer.Component;
            }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return this._targetProperty;
            }
        }
    }
}

