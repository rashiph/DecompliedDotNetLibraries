namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class DataGridViewCellStyleEditor : UITypeEditor
    {
        private DataGridViewCellStyleBuilder builderDialog;
        private object value;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            this.value = value;
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                IUIService service2 = (IUIService) provider.GetService(typeof(IUIService));
                IComponent instance = context.Instance as IComponent;
                if (service != null)
                {
                    if (this.builderDialog == null)
                    {
                        this.builderDialog = new DataGridViewCellStyleBuilder(provider, instance);
                    }
                    if (service2 != null)
                    {
                        this.builderDialog.Font = (Font) service2.Styles["DialogFont"];
                    }
                    DataGridViewCellStyle style = value as DataGridViewCellStyle;
                    if (style != null)
                    {
                        this.builderDialog.CellStyle = style;
                    }
                    this.builderDialog.Context = context;
                    if (this.builderDialog.ShowDialog() == DialogResult.OK)
                    {
                        this.value = this.builderDialog.CellStyle;
                    }
                }
            }
            value = this.value;
            this.value = null;
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

