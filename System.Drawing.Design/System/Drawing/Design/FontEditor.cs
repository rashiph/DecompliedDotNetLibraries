namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class FontEditor : UITypeEditor
    {
        private FontDialog fontDialog;
        private object value;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            this.value = value;
            if ((provider != null) && (((IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService))) != null))
            {
                if (this.fontDialog == null)
                {
                    this.fontDialog = new FontDialog();
                    this.fontDialog.ShowApply = false;
                    this.fontDialog.ShowColor = false;
                    this.fontDialog.AllowVerticalFonts = false;
                }
                Font font = value as Font;
                if (font != null)
                {
                    this.fontDialog.Font = font;
                }
                IntPtr focus = System.Drawing.Design.UnsafeNativeMethods.GetFocus();
                try
                {
                    if (this.fontDialog.ShowDialog() == DialogResult.OK)
                    {
                        this.value = this.fontDialog.Font;
                    }
                }
                finally
                {
                    if (focus != IntPtr.Zero)
                    {
                        System.Drawing.Design.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
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

