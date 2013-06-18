namespace System.Web.UI.Design.Util
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal static class UIServiceHelper
    {
        public static Font GetDialogFont(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    IDictionary styles = service.Styles;
                    if (styles != null)
                    {
                        return (Font) styles["DialogFont"];
                    }
                }
            }
            return null;
        }

        public static IWin32Window GetDialogOwnerWindow(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    return service.GetDialogOwnerWindow();
                }
            }
            return null;
        }

        public static ToolStripRenderer GetToolStripRenderer(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    IDictionary styles = service.Styles;
                    if (styles != null)
                    {
                        return (ToolStripRenderer) styles["VsRenderer"];
                    }
                }
            }
            return null;
        }

        public static DialogResult ShowDialog(IServiceProvider serviceProvider, Form form)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    return service.ShowDialog(form);
                }
            }
            return form.ShowDialog();
        }

        public static void ShowError(IServiceProvider serviceProvider, string message)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    service.ShowError(message);
                    return;
                }
            }
            System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, System.Design.SR.GetString("UIServiceHelper_ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
        }

        public static void ShowError(IServiceProvider serviceProvider, Exception ex, string message)
        {
            if (ex != null)
            {
                message = message + Environment.NewLine + Environment.NewLine + ex.Message;
            }
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    service.ShowError(message);
                    return;
                }
            }
            System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, System.Design.SR.GetString("UIServiceHelper_ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
        }

        public static void ShowMessage(IServiceProvider serviceProvider, string message)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    service.ShowMessage(message);
                    return;
                }
            }
            System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, 0);
        }

        public static DialogResult ShowMessage(IServiceProvider serviceProvider, string message, string caption, MessageBoxButtons buttons)
        {
            if (serviceProvider != null)
            {
                IUIService service = (IUIService) serviceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    return service.ShowMessage(message, caption, buttons);
                }
            }
            return System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, caption, buttons, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1, 0);
        }
    }
}

