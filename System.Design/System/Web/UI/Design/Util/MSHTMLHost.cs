namespace System.Web.UI.Design.Util
{
    using System;
    using System.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class MSHTMLHost : Control
    {
        private TridentSite tridentSite;

        public void ActivateTrident()
        {
            this.tridentSite.Activate();
        }

        public bool CreateTrident()
        {
            try
            {
                this.tridentSite = new TridentSite(this);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public System.Design.NativeMethods.IHTMLDocument2 GetDocument()
        {
            return this.tridentSite.GetDocument();
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= 0x20000;
                return createParams;
            }
        }
    }
}

