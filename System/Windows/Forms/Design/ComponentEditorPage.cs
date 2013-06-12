namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class ComponentEditorPage : Panel
    {
        private bool commitOnDeactivate = false;
        private IComponent component;
        private bool firstActivate = true;
        private System.Drawing.Icon icon;
        private int loading = 0;
        private bool loadRequired = false;
        private IComponentEditorPageSite pageSite;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        public ComponentEditorPage()
        {
            base.Visible = false;
        }

        public virtual void Activate()
        {
            if (this.loadRequired)
            {
                this.EnterLoadingMode();
                this.LoadComponent();
                this.ExitLoadingMode();
                this.loadRequired = false;
            }
            base.Visible = true;
            this.firstActivate = false;
        }

        public virtual void ApplyChanges()
        {
            this.SaveComponent();
        }

        public virtual void Deactivate()
        {
            base.Visible = false;
        }

        protected void EnterLoadingMode()
        {
            this.loading++;
        }

        protected void ExitLoadingMode()
        {
            this.loading--;
        }

        public virtual Control GetControl()
        {
            return this;
        }

        protected IComponent GetSelectedComponent()
        {
            return this.component;
        }

        protected bool IsFirstActivate()
        {
            return this.firstActivate;
        }

        protected bool IsLoading()
        {
            return (this.loading != 0);
        }

        public virtual bool IsPageMessage(ref Message msg)
        {
            return this.PreProcessMessage(ref msg);
        }

        protected abstract void LoadComponent();
        public virtual void OnApplyComplete()
        {
            this.ReloadComponent();
        }

        protected virtual void ReloadComponent()
        {
            if (!base.Visible)
            {
                this.loadRequired = true;
            }
        }

        protected abstract void SaveComponent();
        public virtual void SetComponent(IComponent component)
        {
            this.component = component;
            this.loadRequired = true;
        }

        protected virtual void SetDirty()
        {
            if (!this.IsLoading())
            {
                this.pageSite.SetDirty();
            }
        }

        public virtual void SetSite(IComponentEditorPageSite site)
        {
            this.pageSite = site;
            this.pageSite.GetControl().Controls.Add(this);
        }

        public virtual void ShowHelp()
        {
        }

        public virtual bool SupportsHelp()
        {
            return false;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }

        public bool CommitOnDeactivate
        {
            get
            {
                return this.commitOnDeactivate;
            }
            set
            {
                this.commitOnDeactivate = value;
            }
        }

        protected IComponent Component
        {
            get
            {
                return this.component;
            }
            set
            {
                this.component = value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style &= -12582913;
                return createParams;
            }
        }

        protected bool FirstActivate
        {
            get
            {
                return this.firstActivate;
            }
            set
            {
                this.firstActivate = value;
            }
        }

        public System.Drawing.Icon Icon
        {
            get
            {
                if (this.icon == null)
                {
                    this.icon = new System.Drawing.Icon(typeof(ComponentEditorPage), "ComponentEditorPage.ico");
                }
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        protected int Loading
        {
            get
            {
                return this.loading;
            }
            set
            {
                this.loading = value;
            }
        }

        protected bool LoadRequired
        {
            get
            {
                return this.loadRequired;
            }
            set
            {
                this.loadRequired = value;
            }
        }

        protected IComponentEditorPageSite PageSite
        {
            get
            {
                return this.pageSite;
            }
            set
            {
                this.pageSite = value;
            }
        }

        public virtual string Title
        {
            get
            {
                return base.Text;
            }
        }
    }
}

