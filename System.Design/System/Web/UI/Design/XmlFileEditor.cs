namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class XmlFileEditor : UITypeEditor
    {
        internal FileDialog fileDialog;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                if (this.fileDialog == null)
                {
                    this.fileDialog = new OpenFileDialog();
                    this.fileDialog.Title = System.Design.SR.GetString("XMLFilePicker_Caption");
                    this.fileDialog.Filter = System.Design.SR.GetString("XMLFilePicker_Filter");
                }
                if (value != null)
                {
                    this.fileDialog.FileName = value.ToString();
                }
                if (this.fileDialog.ShowDialog() == DialogResult.OK)
                {
                    value = this.fileDialog.FileName;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

