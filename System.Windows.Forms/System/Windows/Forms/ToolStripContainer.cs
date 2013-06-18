namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [System.Windows.Forms.SRDescription("ToolStripContainerDesc"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), Designer("System.Windows.Forms.Design.ToolStripContainerDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class ToolStripContainer : ContainerControl
    {
        private ToolStripPanel bottomPanel;
        private ToolStripContentPanel contentPanel;
        private ToolStripPanel leftPanel;
        private ToolStripPanel rightPanel;
        private ToolStripPanel topPanel;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler BackColorChanged
        {
            add
            {
                base.BackColorChanged += value;
            }
            remove
            {
                base.BackColorChanged -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler BackgroundImageChanged
        {
            add
            {
                base.BackgroundImageChanged += value;
            }
            remove
            {
                base.BackgroundImageChanged -= value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler BackgroundImageLayoutChanged
        {
            add
            {
                base.BackgroundImageLayoutChanged += value;
            }
            remove
            {
                base.BackgroundImageLayoutChanged += value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler CausesValidationChanged
        {
            add
            {
                base.CausesValidationChanged += value;
            }
            remove
            {
                base.CausesValidationChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler ContextMenuStripChanged
        {
            add
            {
                base.ContextMenuStripChanged += value;
            }
            remove
            {
                base.ContextMenuStripChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler CursorChanged
        {
            add
            {
                base.CursorChanged += value;
            }
            remove
            {
                base.CursorChanged -= value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler ForeColorChanged
        {
            add
            {
                base.ForeColorChanged += value;
            }
            remove
            {
                base.ForeColorChanged -= value;
            }
        }

        public ToolStripContainer()
        {
            base.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            base.SuspendLayout();
            try
            {
                this.topPanel = new ToolStripPanel(this);
                this.bottomPanel = new ToolStripPanel(this);
                this.leftPanel = new ToolStripPanel(this);
                this.rightPanel = new ToolStripPanel(this);
                this.contentPanel = new ToolStripContentPanel();
                this.contentPanel.Dock = DockStyle.Fill;
                this.topPanel.Dock = DockStyle.Top;
                this.bottomPanel.Dock = DockStyle.Bottom;
                this.rightPanel.Dock = DockStyle.Right;
                this.leftPanel.Dock = DockStyle.Left;
                ToolStripContainerTypedControlCollection controls = this.Controls as ToolStripContainerTypedControlCollection;
                if (controls != null)
                {
                    controls.AddInternal(this.contentPanel);
                    controls.AddInternal(this.leftPanel);
                    controls.AddInternal(this.rightPanel);
                    controls.AddInternal(this.topPanel);
                    controls.AddInternal(this.bottomPanel);
                }
            }
            finally
            {
                base.ResumeLayout(true);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override Control.ControlCollection CreateControlsInstance()
        {
            return new ToolStripContainerTypedControlCollection(this, true);
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            if (this.RightToLeft == RightToLeft.Yes)
            {
                this.RightToolStripPanel.Dock = DockStyle.Left;
                this.LeftToolStripPanel.Dock = DockStyle.Right;
            }
            else
            {
                this.RightToolStripPanel.Dock = DockStyle.Right;
                this.LeftToolStripPanel.Dock = DockStyle.Left;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                control.SuspendLayout();
            }
            base.OnSizeChanged(e);
            foreach (Control control2 in this.Controls)
            {
                control2.ResumeLayout();
            }
        }

        internal override void RecreateHandleCore()
        {
            if (base.IsHandleCreated)
            {
                foreach (Control control in this.Controls)
                {
                    control.CreateControl(true);
                }
            }
            base.RecreateHandleCore();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                base.AutoScroll = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size AutoScrollMargin
        {
            get
            {
                return base.AutoScrollMargin;
            }
            set
            {
                base.AutoScrollMargin = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public Size AutoScrollMinSize
        {
            get
            {
                return base.AutoScrollMinSize;
            }
            set
            {
                base.AutoScrollMinSize = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                base.BackgroundImage = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout
        {
            get
            {
                return base.BackgroundImageLayout;
            }
            set
            {
                base.BackgroundImageLayout = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripContainerBottomToolStripPanelDescr"), Localizable(false)]
        public ToolStripPanel BottomToolStripPanel
        {
            get
            {
                return this.bottomPanel;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("ToolStripContainerBottomToolStripPanelVisibleDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool BottomToolStripPanelVisible
        {
            get
            {
                return this.BottomToolStripPanel.Visible;
            }
            set
            {
                this.BottomToolStripPanel.Visible = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public bool CausesValidation
        {
            get
            {
                return base.CausesValidation;
            }
            set
            {
                base.CausesValidation = value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripContainerContentPanelDescr"), Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatAppearance")]
        public ToolStripContentPanel ContentPanel
        {
            get
            {
                return this.contentPanel;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return base.ContextMenuStrip;
            }
            set
            {
                base.ContextMenuStrip = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public Control.ControlCollection Controls
        {
            get
            {
                return base.Controls;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(150, 0xaf);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(false), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripContainerLeftToolStripPanelDescr")]
        public ToolStripPanel LeftToolStripPanel
        {
            get
            {
                return this.leftPanel;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRDescription("ToolStripContainerLeftToolStripPanelVisibleDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool LeftToolStripPanelVisible
        {
            get
            {
                return this.LeftToolStripPanel.Visible;
            }
            set
            {
                this.LeftToolStripPanel.Visible = value;
            }
        }

        [Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripContainerRightToolStripPanelDescr")]
        public ToolStripPanel RightToolStripPanel
        {
            get
            {
                return this.rightPanel;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripContainerRightToolStripPanelVisibleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true)]
        public bool RightToolStripPanelVisible
        {
            get
            {
                return this.RightToolStripPanel.Visible;
            }
            set
            {
                this.RightToolStripPanel.Visible = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ToolStripContainerTopToolStripPanelDescr"), Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToolStripPanel TopToolStripPanel
        {
            get
            {
                return this.topPanel;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripContainerTopToolStripPanelVisibleDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(true)]
        public bool TopToolStripPanelVisible
        {
            get
            {
                return this.TopToolStripPanel.Visible;
            }
            set
            {
                this.TopToolStripPanel.Visible = value;
            }
        }

        internal class ToolStripContainerTypedControlCollection : WindowsFormsUtils.ReadOnlyControlCollection
        {
            private System.Type contentPanelType;
            private ToolStripContainer owner;
            private System.Type panelType;

            public ToolStripContainerTypedControlCollection(Control c, bool isReadOnly) : base(c, isReadOnly)
            {
                this.contentPanelType = typeof(ToolStripContentPanel);
                this.panelType = typeof(ToolStripPanel);
                this.owner = c as ToolStripContainer;
            }

            public override void Add(Control value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.IsReadOnly)
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripContainerUseContentPanel"));
                }
                System.Type c = value.GetType();
                if (!this.contentPanelType.IsAssignableFrom(c) && !this.panelType.IsAssignableFrom(c))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("TypedControlCollectionShouldBeOfTypes", new object[] { this.contentPanelType.Name, this.panelType.Name }), new object[0]), value.GetType().Name);
                }
                base.Add(value);
            }

            public override void Remove(Control value)
            {
                if (((value is ToolStripPanel) || (value is ToolStripContentPanel)) && (!this.owner.DesignMode && this.IsReadOnly))
                {
                    throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                }
                base.Remove(value);
            }

            internal override void SetChildIndexInternal(Control child, int newIndex)
            {
                if ((child is ToolStripPanel) || (child is ToolStripContentPanel))
                {
                    if (this.owner.DesignMode)
                    {
                        return;
                    }
                    if (this.IsReadOnly)
                    {
                        throw new NotSupportedException(System.Windows.Forms.SR.GetString("ReadonlyControlsCollection"));
                    }
                }
                base.SetChildIndexInternal(child, newIndex);
            }
        }
    }
}

