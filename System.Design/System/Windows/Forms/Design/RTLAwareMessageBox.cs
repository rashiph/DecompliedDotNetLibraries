namespace System.Windows.Forms.Design
{
    using System;
    using System.Design;
    using System.Windows.Forms;

    internal static class RTLAwareMessageBox
    {
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            if (IsRTLResources)
            {
                options |= MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
            }
            return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
        }

        public static bool IsRTLResources
        {
            get
            {
                return (System.Design.SR.GetString("RTL") != "RTL_False");
            }
        }
    }
}

