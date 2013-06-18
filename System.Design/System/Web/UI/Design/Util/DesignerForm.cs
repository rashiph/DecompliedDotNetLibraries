namespace System.Web.UI.Design.Util
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class DesignerForm : Form
    {
        private bool _firstActivate;
        private IServiceProvider _serviceProvider;
        private const int SC_CONTEXTHELP = 0xf180;
        private const int WM_SYSCOMMAND = 0x112;

        protected DesignerForm(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._firstActivate = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._serviceProvider = null;
            }
            base.Dispose(disposing);
        }

        protected override object GetService(System.Type serviceType)
        {
            if (this._serviceProvider != null)
            {
                return this._serviceProvider.GetService(serviceType);
            }
            return null;
        }

        protected void InitializeForm()
        {
            Font dialogFont = UIServiceHelper.GetDialogFont(this.ServiceProvider);
            if (dialogFont != null)
            {
                this.Font = dialogFont;
            }
            if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
            }
            this.AutoScaleBaseSize = new Size(5, 14);
            base.HelpButton = true;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (this._firstActivate)
            {
                this._firstActivate = false;
                this.OnInitialActivated(e);
            }
        }

        protected sealed override void OnHelpRequested(HelpEventArgs hevent)
        {
            this.ShowHelp();
            hevent.Handled = true;
        }

        protected virtual void OnInitialActivated(EventArgs e)
        {
        }

        private void ShowHelp()
        {
            if (this.ServiceProvider != null)
            {
                IHelpService service = (IHelpService) this.ServiceProvider.GetService(typeof(IHelpService));
                if (service != null)
                {
                    service.ShowHelpFromKeyword(this.HelpTopic);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == 0x112) && (((int) m.WParam) == 0xf180))
            {
                this.ShowHelp();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected abstract string HelpTopic { get; }

        protected internal IServiceProvider ServiceProvider
        {
            get
            {
                return this._serviceProvider;
            }
        }
    }
}

