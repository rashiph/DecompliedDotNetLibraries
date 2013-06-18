namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;

    [DesignTimeVisible(false), ToolboxItem(false)]
    internal class TabOrder : Control, IMouseHandler, IMenuStatusHandler
    {
        private MenuCommand[] commands;
        private Control ctlHover;
        private string decimalSep;
        private StringBuilder drawString;
        private Pen highlightPen;
        private Brush highlightTextBrush;
        private IDesignerHost host;
        private MenuCommand[] newCommands;
        private Region region;
        private int selSize;
        private ArrayList tabComplete;
        private ArrayList tabControls;
        private Font tabFont;
        private Rectangle[] tabGlyphs;
        private Hashtable tabNext;
        private Hashtable tabProperties;

        public TabOrder(IDesignerHost host)
        {
            this.host = host;
            IUIService service = (IUIService) host.GetService(typeof(IUIService));
            if (service != null)
            {
                this.tabFont = (Font) service.Styles["DialogFont"];
            }
            else
            {
                this.tabFont = Control.DefaultFont;
            }
            this.tabFont = new Font(this.tabFont, FontStyle.Bold);
            this.selSize = DesignerUtils.GetAdornmentDimensions(AdornmentType.GrabHandle).Width;
            this.drawString = new StringBuilder(12);
            this.highlightTextBrush = new SolidBrush(SystemColors.HighlightText);
            this.highlightPen = new Pen(SystemColors.Highlight);
            NumberFormatInfo format = (NumberFormatInfo) CultureInfo.CurrentCulture.GetFormat(typeof(NumberFormatInfo));
            if (format != null)
            {
                this.decimalSep = format.NumberDecimalSeparator;
            }
            else
            {
                this.decimalSep = ".";
            }
            this.tabProperties = new Hashtable();
            base.SetStyle(ControlStyles.Opaque, true);
            IOverlayService service2 = (IOverlayService) host.GetService(typeof(IOverlayService));
            if (service2 != null)
            {
                service2.PushOverlay(this);
            }
            IHelpService service3 = (IHelpService) host.GetService(typeof(IHelpService));
            if (service3 != null)
            {
                service3.AddContextAttribute("Keyword", "TabOrderView", HelpKeywordType.FilterKeyword);
            }
            this.commands = new MenuCommand[] { new MenuCommand(new EventHandler(this.OnKeyCancel), MenuCommands.KeyCancel), new MenuCommand(new EventHandler(this.OnKeyDefault), MenuCommands.KeyDefaultAction), new MenuCommand(new EventHandler(this.OnKeyPrevious), MenuCommands.KeyMoveUp), new MenuCommand(new EventHandler(this.OnKeyNext), MenuCommands.KeyMoveDown), new MenuCommand(new EventHandler(this.OnKeyPrevious), MenuCommands.KeyMoveLeft), new MenuCommand(new EventHandler(this.OnKeyNext), MenuCommands.KeyMoveRight), new MenuCommand(new EventHandler(this.OnKeyNext), MenuCommands.KeySelectNext), new MenuCommand(new EventHandler(this.OnKeyPrevious), MenuCommands.KeySelectPrevious) };
            this.newCommands = new MenuCommand[] { new MenuCommand(new EventHandler(this.OnKeyDefault), MenuCommands.KeyTabOrderSelect) };
            IMenuCommandService service4 = (IMenuCommandService) host.GetService(typeof(IMenuCommandService));
            if (service4 != null)
            {
                foreach (MenuCommand command in this.newCommands)
                {
                    service4.AddCommand(command);
                }
            }
            IEventHandlerService service5 = (IEventHandlerService) host.GetService(typeof(IEventHandlerService));
            if (service5 != null)
            {
                service5.PushHandler(this);
            }
            IComponentChangeService service6 = (IComponentChangeService) host.GetService(typeof(IComponentChangeService));
            if (service6 != null)
            {
                service6.ComponentAdded += new ComponentEventHandler(this.OnComponentAddRemove);
                service6.ComponentRemoved += new ComponentEventHandler(this.OnComponentAddRemove);
                service6.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.region != null)
                {
                    this.region.Dispose();
                    this.region = null;
                }
                if (this.host != null)
                {
                    IOverlayService service = (IOverlayService) this.host.GetService(typeof(IOverlayService));
                    if (service != null)
                    {
                        service.RemoveOverlay(this);
                    }
                    IEventHandlerService service2 = (IEventHandlerService) this.host.GetService(typeof(IEventHandlerService));
                    if (service2 != null)
                    {
                        service2.PopHandler(this);
                    }
                    IMenuCommandService service3 = (IMenuCommandService) this.host.GetService(typeof(IMenuCommandService));
                    if (service3 != null)
                    {
                        foreach (MenuCommand command in this.newCommands)
                        {
                            service3.RemoveCommand(command);
                        }
                    }
                    IComponentChangeService service4 = (IComponentChangeService) this.host.GetService(typeof(IComponentChangeService));
                    if (service4 != null)
                    {
                        service4.ComponentAdded -= new ComponentEventHandler(this.OnComponentAddRemove);
                        service4.ComponentRemoved -= new ComponentEventHandler(this.OnComponentAddRemove);
                        service4.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    }
                    IHelpService service5 = (IHelpService) this.host.GetService(typeof(IHelpService));
                    if (service5 != null)
                    {
                        service5.RemoveContextAttribute("Keyword", "TabOrderView");
                    }
                    this.host = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DrawTabs(IList tabs, Graphics gr, bool fRegion)
        {
            Control current;
            IEnumerator enumerator = tabs.GetEnumerator();
            int num = 0;
            Rectangle empty = Rectangle.Empty;
            Size size = Size.Empty;
            Font tabFont = this.tabFont;
            if (fRegion)
            {
                this.region = new Region(new Rectangle(0, 0, 0, 0));
            }
            if (this.ctlHover != null)
            {
                Rectangle convertedBounds = this.GetConvertedBounds(this.ctlHover);
                Rectangle rect = convertedBounds;
                rect.Inflate(this.selSize, this.selSize);
                if (fRegion)
                {
                    this.region = new Region(rect);
                    this.region.Exclude(convertedBounds);
                }
                else
                {
                    Color backColor = this.ctlHover.Parent.BackColor;
                    Region clip = gr.Clip;
                    gr.ExcludeClip(convertedBounds);
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        gr.FillRectangle(brush, rect);
                    }
                    ControlPaint.DrawSelectionFrame(gr, false, rect, convertedBounds, backColor);
                    gr.Clip = clip;
                }
            }
            while (enumerator.MoveNext())
            {
                current = (Control) enumerator.Current;
                empty = this.GetConvertedBounds(current);
                this.drawString.Length = 0;
                Control sitedParent = this.GetSitedParent(current);
                Control rootComponent = (Control) this.host.RootComponent;
                while ((sitedParent != rootComponent) && (sitedParent != null))
                {
                    this.drawString.Insert(0, this.decimalSep);
                    this.drawString.Insert(0, sitedParent.TabIndex.ToString(CultureInfo.CurrentCulture));
                    sitedParent = this.GetSitedParent(sitedParent);
                }
                this.drawString.Insert(0, ' ');
                this.drawString.Append(current.TabIndex.ToString(CultureInfo.CurrentCulture));
                this.drawString.Append(' ');
                if (((PropertyDescriptor) this.tabProperties[current]).IsReadOnly)
                {
                    this.drawString.Append(System.Design.SR.GetString("WindowsFormsTabOrderReadOnly"));
                    this.drawString.Append(' ');
                }
                string text = this.drawString.ToString();
                size = Size.Ceiling(gr.MeasureString(text, tabFont));
                empty.Width = size.Width + 2;
                empty.Height = size.Height + 2;
                this.tabGlyphs[num++] = empty;
                if (fRegion)
                {
                    this.region.Union(empty);
                }
                else
                {
                    Brush highlightTextBrush;
                    Pen highlightPen;
                    Color highlight;
                    if (this.tabComplete.IndexOf(current) != -1)
                    {
                        highlightTextBrush = this.highlightTextBrush;
                        highlightPen = this.highlightPen;
                        highlight = SystemColors.Highlight;
                    }
                    else
                    {
                        highlightTextBrush = SystemBrushes.Highlight;
                        highlightPen = SystemPens.HighlightText;
                        highlight = SystemColors.HighlightText;
                    }
                    gr.FillRectangle(highlightTextBrush, empty);
                    gr.DrawRectangle(highlightPen, empty.X, empty.Y, empty.Width - 1, empty.Height - 1);
                    Brush brush3 = new SolidBrush(highlight);
                    gr.DrawString(text, tabFont, brush3, (float) (empty.X + 1), (float) (empty.Y + 1));
                    brush3.Dispose();
                }
            }
            if (fRegion)
            {
                current = (Control) this.host.RootComponent;
                empty = this.GetConvertedBounds(current);
                this.region.Intersect(empty);
                base.Region = this.region;
            }
        }

        private Control GetControlAtPoint(IList tabs, int x, int y)
        {
            IEnumerator enumerator = tabs.GetEnumerator();
            Control control = null;
            while (enumerator.MoveNext())
            {
                Control current = (Control) enumerator.Current;
                Control sitedParent = this.GetSitedParent(current);
                Rectangle bounds = current.Bounds;
                if (sitedParent.RectangleToScreen(bounds).Contains(x, y))
                {
                    control = current;
                }
            }
            return control;
        }

        private Rectangle GetConvertedBounds(Control ctl)
        {
            Control parent = ctl.Parent;
            Rectangle bounds = ctl.Bounds;
            bounds = parent.RectangleToScreen(bounds);
            return base.RectangleToClient(bounds);
        }

        private int GetMaxControlCount(Control ctl)
        {
            int num = 0;
            for (int i = 0; i < ctl.Controls.Count; i++)
            {
                if (this.GetTabbable(ctl.Controls[i]))
                {
                    num++;
                }
            }
            return num;
        }

        private Control GetSitedParent(Control child)
        {
            Control parent = child.Parent;
            while (parent != null)
            {
                ISite site = parent.Site;
                IContainer container = null;
                if (site != null)
                {
                    container = site.Container;
                }
                container = DesignerUtils.CheckForNestedContainer(container);
                if ((site != null) && (container == this.host))
                {
                    return parent;
                }
                parent = parent.Parent;
            }
            return parent;
        }

        private bool GetTabbable(Control control)
        {
            for (Control control2 = control; control2 != null; control2 = control2.Parent)
            {
                if (!control2.Visible)
                {
                    return false;
                }
            }
            ISite site = control.Site;
            if ((site == null) || (site.Container != this.host))
            {
                return false;
            }
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control)["TabIndex"];
            if ((descriptor == null) || !descriptor.IsBrowsable)
            {
                return false;
            }
            this.tabProperties[control] = descriptor;
            return true;
        }

        private void GetTabbing(Control ctl, IList tabs)
        {
            for (int i = ctl.Controls.Count - 1; i >= 0; i--)
            {
                Control child = ctl.Controls[i];
                if ((this.GetSitedParent(child) != null) && this.GetTabbable(child))
                {
                    tabs.Add(child);
                }
                if (child.Controls.Count > 0)
                {
                    this.GetTabbing(child, tabs);
                }
            }
        }

        private void OnComponentAddRemove(object sender, ComponentEventArgs ce)
        {
            this.ctlHover = null;
            this.tabControls = null;
            this.tabGlyphs = null;
            if (this.tabComplete != null)
            {
                this.tabComplete.Clear();
            }
            if (this.tabNext != null)
            {
                this.tabNext.Clear();
            }
            if (this.region != null)
            {
                this.region.Dispose();
                this.region = null;
            }
            base.Invalidate();
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            this.tabControls = null;
            this.tabGlyphs = null;
            if (this.region != null)
            {
                this.region.Dispose();
                this.region = null;
            }
            base.Invalidate();
        }

        private void OnKeyCancel(object sender, EventArgs e)
        {
            IMenuCommandService service = (IMenuCommandService) this.host.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                MenuCommand command = service.FindCommand(StandardCommands.TabOrder);
                if (command != null)
                {
                    command.Invoke();
                }
            }
        }

        private void OnKeyDefault(object sender, EventArgs e)
        {
            if (this.ctlHover != null)
            {
                this.SetNextTabIndex(this.ctlHover);
                this.RotateControls(true);
            }
        }

        private void OnKeyNext(object sender, EventArgs e)
        {
            this.RotateControls(true);
        }

        private void OnKeyPrevious(object sender, EventArgs e)
        {
            this.RotateControls(false);
        }

        public virtual void OnMouseDoubleClick(IComponent component)
        {
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (this.ctlHover != null)
            {
                this.SetNextTabIndex(this.ctlHover);
            }
        }

        public virtual void OnMouseDown(IComponent component, MouseButtons button, int x, int y)
        {
            if (this.ctlHover != null)
            {
                this.SetNextTabIndex(this.ctlHover);
            }
        }

        public virtual void OnMouseHover(IComponent component)
        {
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.tabGlyphs != null)
            {
                Control ctl = null;
                for (int i = 0; i < this.tabGlyphs.Length; i++)
                {
                    if (this.tabGlyphs[i].Contains(e.X, e.Y))
                    {
                        ctl = (Control) this.tabControls[i];
                    }
                }
                this.SetNewHover(ctl);
            }
            this.SetAppropriateCursor();
        }

        public virtual void OnMouseMove(IComponent component, int x, int y)
        {
            if (this.tabControls != null)
            {
                Control ctl = this.GetControlAtPoint(this.tabControls, x, y);
                this.SetNewHover(ctl);
            }
        }

        public virtual void OnMouseUp(IComponent component, MouseButtons button)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.tabControls == null)
            {
                this.tabControls = new ArrayList();
                this.GetTabbing((Control) this.host.RootComponent, this.tabControls);
                this.tabGlyphs = new Rectangle[this.tabControls.Count];
            }
            if (this.tabComplete == null)
            {
                this.tabComplete = new ArrayList();
            }
            if (this.tabNext == null)
            {
                this.tabNext = new Hashtable();
            }
            if (this.region == null)
            {
                this.DrawTabs(this.tabControls, e.Graphics, true);
            }
            this.DrawTabs(this.tabControls, e.Graphics, false);
        }

        public virtual void OnSetCursor(IComponent component)
        {
            this.SetAppropriateCursor();
        }

        public bool OverrideInvoke(MenuCommand cmd)
        {
            for (int i = 0; i < this.commands.Length; i++)
            {
                if (this.commands[i].CommandID.Equals(cmd.CommandID))
                {
                    this.commands[i].Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool OverrideStatus(MenuCommand cmd)
        {
            for (int i = 0; i < this.commands.Length; i++)
            {
                if (this.commands[i].CommandID.Equals(cmd.CommandID))
                {
                    cmd.Enabled = this.commands[i].Enabled;
                    return true;
                }
            }
            if (!cmd.CommandID.Equals(StandardCommands.TabOrder))
            {
                cmd.Enabled = false;
                return true;
            }
            return false;
        }

        private void RotateControls(bool forward)
        {
            Control ctlHover = this.ctlHover;
            Control rootComponent = (Control) this.host.RootComponent;
            if (ctlHover == null)
            {
                ctlHover = rootComponent;
            }
            while ((ctlHover = rootComponent.GetNextControl(ctlHover, forward)) != null)
            {
                if (this.GetTabbable(ctlHover))
                {
                    break;
                }
            }
            this.SetNewHover(ctlHover);
        }

        private void SetAppropriateCursor()
        {
            if (this.ctlHover != null)
            {
                Cursor.Current = Cursors.Cross;
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void SetNewHover(Control ctl)
        {
            if (this.ctlHover != ctl)
            {
                if (this.ctlHover != null)
                {
                    if (this.region != null)
                    {
                        this.region.Dispose();
                        this.region = null;
                    }
                    Rectangle convertedBounds = this.GetConvertedBounds(this.ctlHover);
                    convertedBounds.Inflate(this.selSize, this.selSize);
                    base.Invalidate(convertedBounds);
                }
                this.ctlHover = ctl;
                if (this.ctlHover != null)
                {
                    if (this.region != null)
                    {
                        this.region.Dispose();
                        this.region = null;
                    }
                    Rectangle rc = this.GetConvertedBounds(this.ctlHover);
                    rc.Inflate(this.selSize, this.selSize);
                    base.Invalidate(rc);
                }
            }
        }

        private void SetNextTabIndex(Control ctl)
        {
            if (this.tabControls != null)
            {
                int num;
                Control sitedParent = this.GetSitedParent(ctl);
                object obj2 = this.tabNext[sitedParent];
                if (this.tabComplete.IndexOf(ctl) == -1)
                {
                    this.tabComplete.Add(ctl);
                }
                if (obj2 != null)
                {
                    num = (int) obj2;
                }
                else
                {
                    num = 0;
                }
                try
                {
                    PropertyDescriptor descriptor = (PropertyDescriptor) this.tabProperties[ctl];
                    if (descriptor != null)
                    {
                        int num3 = num + 1;
                        if (descriptor.IsReadOnly)
                        {
                            num3 = ((int) descriptor.GetValue(ctl)) + 1;
                        }
                        int maxControlCount = this.GetMaxControlCount(sitedParent);
                        if (num3 >= maxControlCount)
                        {
                            num3 = 0;
                        }
                        this.tabNext[sitedParent] = num3;
                        if (this.tabComplete.Count == this.tabControls.Count)
                        {
                            this.tabComplete.Clear();
                        }
                        if (!descriptor.IsReadOnly)
                        {
                            try
                            {
                                descriptor.SetValue(ctl, num);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            base.Invalidate();
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
            }
        }
    }
}

