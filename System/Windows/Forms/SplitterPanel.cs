namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [ToolboxItem(false), Designer("System.Windows.Forms.Design.SplitterPanelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch), Docking(DockingBehavior.Never)]
    public sealed class SplitterPanel : Panel
    {
        private bool collapsed;
        private SplitContainer owner;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public event EventHandler VisibleChanged
        {
            add
            {
                base.VisibleChanged += value;
            }
            remove
            {
                base.VisibleChanged -= value;
            }
        }

        public SplitterPanel(SplitContainer owner)
        {
            this.owner = owner;
            base.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public AnchorStyles Anchor
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool AutoSize
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

        [Localizable(false), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Windows.Forms.BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;
            }
        }

        internal bool Collapsed
        {
            get
            {
                return this.collapsed;
            }
            set
            {
                this.collapsed = value;
            }
        }

        protected override Padding DefaultMargin
        {
            get
            {
                return new Padding(0, 0, 0, 0);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockStyle Dock
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public ScrollableControl.DockPaddingEdges DockPadding
        {
            get
            {
                return base.DockPadding;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatLayout"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ControlHeightDescr")]
        public int Height
        {
            get
            {
                if (this.Collapsed)
                {
                    return 0;
                }
                return base.Height;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("SplitContainerPanelHeight"));
            }
        }

        internal int HeightInternal
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public System.Drawing.Size MaximumSize
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Drawing.Size MinimumSize
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

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        internal SplitContainer Owner
        {
            get
            {
                return this.owner;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Drawing.Size Size
        {
            get
            {
                if (this.Collapsed)
                {
                    return System.Drawing.Size.Empty;
                }
                return base.Size;
            }
            set
            {
                base.Size = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
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

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ControlWidthDescr"), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRCategory("CatLayout")]
        public int Width
        {
            get
            {
                if (this.Collapsed)
                {
                    return 0;
                }
                return base.Width;
            }
            set
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("SplitContainerPanelWidth"));
            }
        }

        internal int WidthInternal
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }
    }
}

