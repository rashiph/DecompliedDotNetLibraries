namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;

    internal class HotCommands : PropertyGrid.SnappableControl
    {
        private bool allowVisible;
        private object component;
        private LinkLabel label;
        private int optimalHeight;
        private DesignerVerb[] verbs;

        internal HotCommands(PropertyGrid owner) : base(owner)
        {
            this.allowVisible = true;
            this.optimalHeight = -1;
            this.Text = "Command Pane";
        }

        public override int GetOptimalHeight(int width)
        {
            if (this.optimalHeight == -1)
            {
                int num = (int) (1.5 * this.Font.Height);
                int length = 0;
                if (this.verbs != null)
                {
                    length = this.verbs.Length;
                }
                this.optimalHeight = (length * num) + 8;
            }
            return this.optimalHeight;
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                if (e.Link.Enabled)
                {
                    ((DesignerVerb) e.Link.LinkData).Invoke();
                }
            }
            catch (Exception exception)
            {
                RTLAwareMessageBox.Show(this, exception.Message, System.Windows.Forms.SR.GetString("PBRSErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        private void OnCommandChanged(object sender, EventArgs e)
        {
            this.SetupLabel();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.optimalHeight = -1;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            this.Label.FocusInternal();
            this.Label.Invalidate();
        }

        public void Select(bool forward)
        {
            this.Label.FocusInternal();
        }

        internal void SetColors(Color background, Color normalText, Color link, Color activeLink, Color visitedLink, Color disabledLink)
        {
            this.Label.BackColor = background;
            this.Label.ForeColor = normalText;
            this.Label.LinkColor = link;
            this.Label.ActiveLinkColor = activeLink;
            this.Label.VisitedLinkColor = visitedLink;
            this.Label.DisabledLinkColor = disabledLink;
        }

        private void SetupLabel()
        {
            this.Label.Links.Clear();
            StringBuilder builder = new StringBuilder();
            Point[] pointArray = new Point[this.verbs.Length];
            int x = 0;
            bool flag = true;
            for (int i = 0; i < this.verbs.Length; i++)
            {
                if (this.verbs[i].Visible && this.verbs[i].Supported)
                {
                    if (!flag)
                    {
                        builder.Append(Application.CurrentCulture.TextInfo.ListSeparator);
                        builder.Append(" ");
                        x += 2;
                    }
                    string text = this.verbs[i].Text;
                    pointArray[i] = new Point(x, text.Length);
                    builder.Append(text);
                    x += text.Length;
                    flag = false;
                }
            }
            this.Label.Text = builder.ToString();
            for (int j = 0; j < this.verbs.Length; j++)
            {
                if (this.verbs[j].Visible && this.verbs[j].Supported)
                {
                    LinkLabel.Link link = this.Label.Links.Add(pointArray[j].X, pointArray[j].Y, this.verbs[j]);
                    if (!this.verbs[j].Enabled)
                    {
                        link.Enabled = false;
                    }
                }
            }
        }

        public virtual void SetVerbs(object component, DesignerVerb[] verbs)
        {
            if (this.verbs != null)
            {
                for (int i = 0; i < this.verbs.Length; i++)
                {
                    this.verbs[i].CommandChanged -= new EventHandler(this.OnCommandChanged);
                }
                this.component = null;
                this.verbs = null;
            }
            if (((component == null) || (verbs == null)) || (verbs.Length == 0))
            {
                base.Visible = false;
                this.Label.Links.Clear();
                this.Label.Text = null;
            }
            else
            {
                this.component = component;
                this.verbs = verbs;
                for (int j = 0; j < verbs.Length; j++)
                {
                    verbs[j].CommandChanged += new EventHandler(this.OnCommandChanged);
                }
                if (this.allowVisible)
                {
                    base.Visible = true;
                }
                this.SetupLabel();
            }
            this.optimalHeight = -1;
        }

        public override int SnapHeightRequest(int request)
        {
            return request;
        }

        public virtual bool AllowVisible
        {
            get
            {
                return this.allowVisible;
            }
            set
            {
                if (this.allowVisible != value)
                {
                    this.allowVisible = value;
                    if (value && this.WouldBeVisible)
                    {
                        base.Visible = true;
                    }
                    else
                    {
                        base.Visible = false;
                    }
                }
            }
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Size clientSize = base.ClientSize;
                return new Rectangle(4, 4, clientSize.Width - 8, clientSize.Height - 8);
            }
        }

        public LinkLabel Label
        {
            get
            {
                if (this.label == null)
                {
                    this.label = new LinkLabel();
                    this.label.Dock = DockStyle.Fill;
                    this.label.LinkBehavior = LinkBehavior.AlwaysUnderline;
                    this.label.DisabledLinkColor = SystemColors.ControlDark;
                    this.label.LinkClicked += new LinkLabelLinkClickedEventHandler(this.LinkClicked);
                    base.Controls.Add(this.label);
                }
                return this.label;
            }
        }

        public virtual bool WouldBeVisible
        {
            get
            {
                return (this.component != null);
            }
        }
    }
}

