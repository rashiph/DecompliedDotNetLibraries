namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    [DefaultProperty("Value")]
    public class ToolStripProgressBar : ToolStripControlHost
    {
        internal static readonly object EventRightToLeftLayoutChanged = new object();

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event KeyEventHandler KeyDown
        {
            add
            {
                base.KeyDown += value;
            }
            remove
            {
                base.KeyDown -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event KeyPressEventHandler KeyPress
        {
            add
            {
                base.KeyPress += value;
            }
            remove
            {
                base.KeyPress -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event KeyEventHandler KeyUp
        {
            add
            {
                base.KeyUp += value;
            }
            remove
            {
                base.KeyUp -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler OwnerChanged
        {
            add
            {
                base.OwnerChanged += value;
            }
            remove
            {
                base.OwnerChanged -= value;
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ControlOnRightToLeftLayoutChangedDescr")]
        public event EventHandler RightToLeftLayoutChanged
        {
            add
            {
                base.Events.AddHandler(EventRightToLeftLayoutChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventRightToLeftLayoutChanged, value);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler TextChanged
        {
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler Validated
        {
            add
            {
                base.Validated += value;
            }
            remove
            {
                base.Validated -= value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event CancelEventHandler Validating
        {
            add
            {
                base.Validating += value;
            }
            remove
            {
                base.Validating -= value;
            }
        }

        public ToolStripProgressBar() : base(CreateControlInstance())
        {
        }

        public ToolStripProgressBar(string name) : this()
        {
            base.Name = name;
        }

        private static Control CreateControlInstance()
        {
            return new System.Windows.Forms.ProgressBar { Size = new Size(100, 15) };
        }

        private void HandleRightToLeftLayoutChanged(object sender, EventArgs e)
        {
            this.OnRightToLeftLayoutChanged(e);
        }

        public void Increment(int value)
        {
            this.ProgressBar.Increment(value);
        }

        protected virtual void OnRightToLeftLayoutChanged(EventArgs e)
        {
            base.RaiseEvent(EventRightToLeftLayoutChanged, e);
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            System.Windows.Forms.ProgressBar bar = control as System.Windows.Forms.ProgressBar;
            if (bar != null)
            {
                bar.RightToLeftLayoutChanged += new EventHandler(this.HandleRightToLeftLayoutChanged);
            }
            base.OnSubscribeControlEvents(control);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            System.Windows.Forms.ProgressBar bar = control as System.Windows.Forms.ProgressBar;
            if (bar != null)
            {
                bar.RightToLeftLayoutChanged -= new EventHandler(this.HandleRightToLeftLayoutChanged);
            }
            base.OnUnsubscribeControlEvents(control);
        }

        public void PerformStep()
        {
            this.ProgressBar.PerformStep();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override Image BackgroundImage
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
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

        protected internal override Padding DefaultMargin
        {
            get
            {
                if ((base.Owner != null) && (base.Owner is StatusStrip))
                {
                    return new Padding(1, 3, 1, 3);
                }
                return new Padding(1, 2, 1, 1);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 15);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ProgressBarMarqueeAnimationSpeed"), DefaultValue(100)]
        public int MarqueeAnimationSpeed
        {
            get
            {
                return this.ProgressBar.MarqueeAnimationSpeed;
            }
            set
            {
                this.ProgressBar.MarqueeAnimationSpeed = value;
            }
        }

        [DefaultValue(100), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ProgressBarMaximumDescr")]
        public int Maximum
        {
            get
            {
                return this.ProgressBar.Maximum;
            }
            set
            {
                this.ProgressBar.Maximum = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ProgressBarMinimumDescr")]
        public int Minimum
        {
            get
            {
                return this.ProgressBar.Minimum;
            }
            set
            {
                this.ProgressBar.Minimum = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.ProgressBar ProgressBar
        {
            get
            {
                return (base.Control as System.Windows.Forms.ProgressBar);
            }
        }

        [System.Windows.Forms.SRDescription("ControlRightToLeftLayoutDescr"), Localizable(true), DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance")]
        public virtual bool RightToLeftLayout
        {
            get
            {
                return this.ProgressBar.RightToLeftLayout;
            }
            set
            {
                this.ProgressBar.RightToLeftLayout = value;
            }
        }

        [DefaultValue(10), System.Windows.Forms.SRDescription("ProgressBarStepDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int Step
        {
            get
            {
                return this.ProgressBar.Step;
            }
            set
            {
                this.ProgressBar.Step = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRDescription("ProgressBarStyleDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public ProgressBarStyle Style
        {
            get
            {
                return this.ProgressBar.Style;
            }
            set
            {
                this.ProgressBar.Style = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public override string Text
        {
            get
            {
                return base.Control.Text;
            }
            set
            {
                base.Control.Text = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0), System.Windows.Forms.SRDescription("ProgressBarValueDescr"), Bindable(true)]
        public int Value
        {
            get
            {
                return this.ProgressBar.Value;
            }
            set
            {
                this.ProgressBar.Value = value;
            }
        }
    }
}

