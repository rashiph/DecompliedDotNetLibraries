namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [ToolboxItemFilter("System.Windows.Forms.MainMenu")]
    public class MainMenu : Menu
    {
        internal Form form;
        internal Form ownerForm;
        private System.Windows.Forms.RightToLeft rightToLeft;

        [System.Windows.Forms.SRDescription("MainMenuCollapseDescr")]
        public event EventHandler Collapse;

        public MainMenu() : base(null)
        {
            this.rightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        public MainMenu(IContainer container) : this()
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        public MainMenu(MenuItem[] items) : base(items)
        {
            this.rightToLeft = System.Windows.Forms.RightToLeft.Inherit;
        }

        public virtual MainMenu CloneMenu()
        {
            MainMenu menu = new MainMenu();
            menu.CloneMenu(this);
            return menu;
        }

        protected override IntPtr CreateMenuHandle()
        {
            return System.Windows.Forms.UnsafeNativeMethods.CreateMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if ((disposing && (this.form != null)) && ((this.ownerForm == null) || (this.form == this.ownerForm)))
            {
                this.form.Menu = null;
            }
            base.Dispose(disposing);
        }

        [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
        public Form GetForm()
        {
            return this.form;
        }

        internal Form GetFormUnsafe()
        {
            return this.form;
        }

        internal override void ItemsChanged(int change)
        {
            base.ItemsChanged(change);
            if (this.form != null)
            {
                this.form.MenuChanged(change, this);
            }
        }

        internal virtual void ItemsChanged(int change, Menu menu)
        {
            if (this.form != null)
            {
                this.form.MenuChanged(change, menu);
            }
        }

        protected internal virtual void OnCollapse(EventArgs e)
        {
            if (this.onCollapse != null)
            {
                this.onCollapse(this, e);
            }
        }

        internal virtual bool ShouldSerializeRightToLeft()
        {
            if (System.Windows.Forms.RightToLeft.Inherit == this.RightToLeft)
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        internal override bool RenderIsRightToLeft
        {
            get
            {
                if (this.RightToLeft != System.Windows.Forms.RightToLeft.Yes)
                {
                    return false;
                }
                if (this.form != null)
                {
                    return !this.form.IsMirrored;
                }
                return true;
            }
        }

        [AmbientValue(2), System.Windows.Forms.SRDescription("MenuRightToLeftDescr"), Localizable(true)]
        public virtual System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                if (System.Windows.Forms.RightToLeft.Inherit != this.rightToLeft)
                {
                    return this.rightToLeft;
                }
                if (this.form != null)
                {
                    return this.form.RightToLeft;
                }
                return System.Windows.Forms.RightToLeft.Inherit;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("RightToLeft", (int) value, typeof(System.Windows.Forms.RightToLeft));
                }
                if (this.rightToLeft != value)
                {
                    this.rightToLeft = value;
                    base.UpdateRtl(value == System.Windows.Forms.RightToLeft.Yes);
                }
            }
        }
    }
}

