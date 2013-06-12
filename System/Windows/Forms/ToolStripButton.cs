namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms.Design;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class ToolStripButton : ToolStripItem
    {
        private bool checkOnClick;
        private System.Windows.Forms.CheckState checkState;
        private static readonly object EventCheckedChanged = new object();
        private static readonly object EventCheckStateChanged = new object();
        private const int StandardButtonWidth = 0x17;

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckedChangedDescr")]
        public event EventHandler CheckedChanged
        {
            add
            {
                base.Events.AddHandler(EventCheckedChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCheckedChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxOnCheckStateChangedDescr")]
        public event EventHandler CheckStateChanged
        {
            add
            {
                base.Events.AddHandler(EventCheckStateChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCheckStateChanged, value);
            }
        }

        public ToolStripButton()
        {
            this.Initialize();
        }

        public ToolStripButton(Image image) : base(null, image, null)
        {
            this.Initialize();
        }

        public ToolStripButton(string text) : base(text, null, null)
        {
            this.Initialize();
        }

        public ToolStripButton(string text, Image image) : base(text, image, null)
        {
            this.Initialize();
        }

        public ToolStripButton(string text, Image image, EventHandler onClick) : base(text, image, onClick)
        {
            this.Initialize();
        }

        public ToolStripButton(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name)
        {
            this.Initialize();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new ToolStripButtonAccessibleObject(this);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size preferredSize = base.GetPreferredSize(constrainingSize);
            preferredSize.Width = Math.Max(preferredSize.Width, 0x17);
            return preferredSize;
        }

        private void Initialize()
        {
            base.SupportsSpaceKey = true;
        }

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventCheckedChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnCheckStateChanged(EventArgs e)
        {
            base.AccessibilityNotifyClients(AccessibleEvents.StateChange);
            EventHandler handler = (EventHandler) base.Events[EventCheckStateChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.checkOnClick)
            {
                this.Checked = !this.Checked;
            }
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (base.Owner != null)
            {
                ToolStripRenderer renderer = base.Renderer;
                renderer.DrawButtonBackground(new ToolStripItemRenderEventArgs(e.Graphics, this));
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Image) == ToolStripItemDisplayStyle.Image)
                {
                    ToolStripItemImageRenderEventArgs args = new ToolStripItemImageRenderEventArgs(e.Graphics, this, base.InternalLayout.ImageRectangle) {
                        ShiftOnPress = true
                    };
                    renderer.DrawItemImage(args);
                }
                if ((this.DisplayStyle & ToolStripItemDisplayStyle.Text) == ToolStripItemDisplayStyle.Text)
                {
                    renderer.DrawItemText(new ToolStripItemTextRenderEventArgs(e.Graphics, this, this.Text, base.InternalLayout.TextRectangle, this.ForeColor, this.Font, base.InternalLayout.TextFormat));
                }
            }
        }

        [DefaultValue(true)]
        public bool AutoToolTip
        {
            get
            {
                return base.AutoToolTip;
            }
            set
            {
                base.AutoToolTip = value;
            }
        }

        public override bool CanSelect
        {
            get
            {
                return true;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolStripButtonCheckedDescr"), System.Windows.Forms.SRCategory("CatAppearance")]
        public bool Checked
        {
            get
            {
                return (this.checkState != System.Windows.Forms.CheckState.Unchecked);
            }
            set
            {
                if (value != this.Checked)
                {
                    this.CheckState = value ? System.Windows.Forms.CheckState.Checked : System.Windows.Forms.CheckState.Unchecked;
                    base.InvokePaint();
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolStripButtonCheckOnClickDescr"), DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior")]
        public bool CheckOnClick
        {
            get
            {
                return this.checkOnClick;
            }
            set
            {
                this.checkOnClick = value;
            }
        }

        [System.Windows.Forms.SRDescription("CheckBoxCheckStateDescr"), System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(0)]
        public System.Windows.Forms.CheckState CheckState
        {
            get
            {
                return this.checkState;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 0, 2))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Windows.Forms.CheckState));
                }
                if (value != this.checkState)
                {
                    this.checkState = value;
                    base.Invalidate();
                    this.OnCheckedChanged(EventArgs.Empty);
                    this.OnCheckStateChanged(EventArgs.Empty);
                }
            }
        }

        protected override bool DefaultAutoToolTip
        {
            get
            {
                return true;
            }
        }

        [ComVisible(true)]
        internal class ToolStripButtonAccessibleObject : ToolStripItem.ToolStripItemAccessibleObject
        {
            private ToolStripButton ownerItem;

            public ToolStripButtonAccessibleObject(ToolStripButton ownerItem) : base(ownerItem)
            {
                this.ownerItem = ownerItem;
            }

            public override AccessibleStates State
            {
                get
                {
                    if (this.ownerItem.Enabled && this.ownerItem.Checked)
                    {
                        return (base.State | AccessibleStates.Checked);
                    }
                    return base.State;
                }
            }
        }
    }
}

