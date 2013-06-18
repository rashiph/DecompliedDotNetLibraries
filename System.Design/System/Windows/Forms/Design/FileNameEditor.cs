namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    public class FileNameEditor : UITypeEditor
    {
        private OpenFileDialog openFileDialog;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                if (this.openFileDialog == null)
                {
                    this.openFileDialog = new OpenFileDialog();
                    this.InitializeDialog(this.openFileDialog);
                }
                if (value is string)
                {
                    this.openFileDialog.FileName = (string) value;
                }
                if (this.openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    value = this.openFileDialog.FileName;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual void InitializeDialog(OpenFileDialog openFileDialog)
        {
            openFileDialog.Filter = System.Design.SR.GetString("GenericFileFilter");
            openFileDialog.Title = System.Design.SR.GetString("GenericOpenFile");
        }
    }
}

