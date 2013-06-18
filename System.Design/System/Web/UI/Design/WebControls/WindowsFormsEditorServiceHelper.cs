namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class WindowsFormsEditorServiceHelper : IWindowsFormsEditorService, IServiceProvider
    {
        private ComponentDesigner _componentDesigner;

        public WindowsFormsEditorServiceHelper(ComponentDesigner componentDesigner)
        {
            this._componentDesigner = componentDesigner;
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return this;
            }
            if (this._componentDesigner.Component != null)
            {
                ISite site = this._componentDesigner.Component.Site;
                if (site != null)
                {
                    return site.GetService(serviceType);
                }
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
            return UIServiceHelper.ShowDialog(this, dialog);
        }
    }
}

