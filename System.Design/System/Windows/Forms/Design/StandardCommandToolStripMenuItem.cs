namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class StandardCommandToolStripMenuItem : ToolStripMenuItem
    {
        private bool _cachedImage;
        private System.Drawing.Image _image;
        private MenuCommand menuCommand;
        private IMenuCommandService menuCommandService;
        private CommandID menuID;
        private string name;
        private IServiceProvider serviceProvider;

        public StandardCommandToolStripMenuItem(CommandID menuID, string text, string imageName, IServiceProvider serviceProvider)
        {
            this.menuID = menuID;
            this.serviceProvider = serviceProvider;
            try
            {
                this.menuCommand = this.MenuService.FindCommand(menuID);
            }
            catch
            {
                this.Enabled = false;
            }
            this.Text = text;
            this.name = imageName;
            this.RefreshItem();
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.menuCommand != null)
            {
                this.menuCommand.Invoke();
            }
            else if (this.MenuService != null)
            {
                this.MenuService.GlobalInvoke(this.menuID);
            }
        }

        public void RefreshItem()
        {
            if (this.menuCommand != null)
            {
                base.Visible = this.menuCommand.Visible;
                this.Enabled = this.menuCommand.Enabled;
                base.Checked = this.menuCommand.Checked;
            }
        }

        public override System.Drawing.Image Image
        {
            get
            {
                if (!this._cachedImage)
                {
                    this._cachedImage = true;
                    try
                    {
                        if (this.name != null)
                        {
                            this._image = new Bitmap(typeof(ToolStripMenuItem), this.name + ".bmp");
                        }
                        base.ImageTransparentColor = Color.Magenta;
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                return this._image;
            }
            set
            {
                this._image = value;
                this._cachedImage = true;
            }
        }

        public IMenuCommandService MenuService
        {
            get
            {
                if (this.menuCommandService == null)
                {
                    this.menuCommandService = (IMenuCommandService) this.serviceProvider.GetService(typeof(IMenuCommandService));
                }
                return this.menuCommandService;
            }
        }
    }
}

