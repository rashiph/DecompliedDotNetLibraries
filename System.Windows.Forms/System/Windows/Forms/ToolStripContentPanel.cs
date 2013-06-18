namespace System.Windows.Forms
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [ToolboxItem(false), ClassInterface(ClassInterfaceType.AutoDispatch), Docking(DockingBehavior.Never), InitializationEvent("Load"), Designer("System.Windows.Forms.Design.ToolStripContentPanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultEvent("Load"), ComVisible(true)]
    public class ToolStripContentPanel : Panel
    {
        private static readonly object EventLoad = new object();
        private static readonly object EventRendererChanged = new object();
        private ToolStripRendererSwitcher rendererSwitcher;
        private BitVector32 state = new BitVector32();
        private static readonly int stateLastDoubleBuffer = BitVector32.CreateMask();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler DockChanged
        {
            add
            {
                base.DockChanged += value;
            }
            remove
            {
                base.DockChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripContentPanelOnLoadDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler Load
        {
            add
            {
                base.Events.AddHandler(EventLoad, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventLoad, value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler LocationChanged
        {
            add
            {
                base.LocationChanged += value;
            }
            remove
            {
                base.LocationChanged -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripRendererChanged"), System.Windows.Forms.SRCategory("CatAppearance")]
        public event EventHandler RendererChanged
        {
            add
            {
                base.Events.AddHandler(EventRendererChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRendererChanged, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TabIndexChanged
        {
            add
            {
                base.TabIndexChanged += value;
            }
            remove
            {
                base.TabIndexChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        public ToolStripContentPanel()
        {
            base.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
        }

        private void HandleRendererChanged(object sender, EventArgs e)
        {
            this.OnRendererChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!base.RecreatingHandle)
            {
                this.OnLoad(EventArgs.Empty);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnLoad(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventLoad];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ToolStripContentPanelRenderEventArgs args = new ToolStripContentPanelRenderEventArgs(e.Graphics, this);
            this.Renderer.DrawToolStripContentPanelBackground(args);
            if (!args.Handled)
            {
                base.OnPaintBackground(e);
            }
        }

        protected virtual void OnRendererChanged(EventArgs e)
        {
            if (this.Renderer is ToolStripProfessionalRenderer)
            {
                this.state[stateLastDoubleBuffer] = this.DoubleBuffered;
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }
            else
            {
                this.DoubleBuffered = this.state[stateLastDoubleBuffer];
            }
            this.Renderer.InitializeContentPanel(this);
            base.Invalidate();
            EventHandler handler = (EventHandler) base.Events[EventRendererChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ResetRenderMode()
        {
            this.RendererSwitcher.ResetRenderMode();
        }

        private bool ShouldSerializeRenderMode()
        {
            return this.RendererSwitcher.ShouldSerializeRenderMode();
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AnchorStyles Anchor
        {
            get
            {
                return base.Anchor;
            }
            set
            {
                base.Anchor = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false), Localizable(false)]
        public override System.Windows.Forms.AutoSizeMode AutoSizeMode
        {
            get
            {
                return System.Windows.Forms.AutoSizeMode.GrowOnly;
            }
            set
            {
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                if ((this.ParentInternal is ToolStripContainer) && (value == Color.Transparent))
                {
                    this.ParentInternal.BackColor = Color.Transparent;
                }
                base.BackColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point Location
        {
            get
            {
                return base.Location;
            }
            set
            {
                base.Location = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MaximumSize
        {
            get
            {
                return base.MaximumSize;
            }
            set
            {
                base.MaximumSize = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                base.MinimumSize = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToolStripRenderer Renderer
        {
            get
            {
                return this.RendererSwitcher.Renderer;
            }
            set
            {
                this.RendererSwitcher.Renderer = value;
            }
        }

        private ToolStripRendererSwitcher RendererSwitcher
        {
            get
            {
                if (this.rendererSwitcher == null)
                {
                    this.rendererSwitcher = new ToolStripRendererSwitcher(this, ToolStripRenderMode.System);
                    this.HandleRendererChanged(this, EventArgs.Empty);
                    this.rendererSwitcher.RendererChanged += new EventHandler(this.HandleRendererChanged);
                }
                return this.rendererSwitcher;
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripRenderModeDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public ToolStripRenderMode RenderMode
        {
            get
            {
                return this.RendererSwitcher.RenderMode;
            }
            set
            {
                this.RendererSwitcher.RenderMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabIndex
        {
            get
            {
                return base.TabIndex;
            }
            set
            {
                base.TabIndex = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }
    }
}

