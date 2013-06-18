namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.VisualStyles;

    internal sealed class DesignerActionPanel : ContainerControl
    {
        private Color _activeLinkColor = SystemColors.HotTrack;
        private Color _borderColor = SystemColors.ActiveBorder;
        private bool _dropDownActive;
        private CommandID[] _filteredCommandIDs;
        private Color _gradientDarkColor = SystemColors.Control;
        private Color _gradientLightColor = SystemColors.Control;
        private bool _inMethodInvoke;
        private List<int> _lineHeights;
        private List<Line> _lines;
        private List<int> _lineYPositions;
        private Color _linkColor = SystemColors.HotTrack;
        private Color _separatorColor = SystemColors.ControlDark;
        private IServiceProvider _serviceProvider;
        private Color _titleBarColor = SystemColors.ActiveCaption;
        private Color _titleBarTextColor = SystemColors.ActiveCaptionText;
        private Color _titleBarUnselectedColor = SystemColors.InactiveCaption;
        private System.Windows.Forms.ToolTip _toolTip;
        private bool _updatingTasks;
        private const int BottomPadding = 2;
        private const int EditInputWidth = 150;
        private const int EditorLineButtonPadding = 1;
        private const int EditorLineSwatchPadding = 1;
        private static readonly object EventFormActivated = new object();
        private static readonly object EventFormDeactivate = new object();
        public const string ExternDllGdi32 = "gdi32.dll";
        public const string ExternDllUser32 = "user32.dll";
        private const int LineLeftMargin = 5;
        private const int LineRightMargin = 4;
        private const int LineVerticalPadding = 7;
        private const int ListBoxMaximumHeight = 200;
        private const int MinimumWidth = 150;
        private const int PanelHeaderHorizontalPadding = 5;
        private const int PanelHeaderVerticalPadding = 3;
        private const int SeparatorHorizontalPadding = 3;
        private const int TextBoxHeightFixup = 2;
        private const int TextBoxLineCenterMargin = 5;
        private const int TextBoxLineInnerPadding = 1;
        private const int TextBoxTopPadding = 4;
        private const int TopPadding = 2;

        private event EventHandler FormActivated
        {
            add
            {
                base.Events.AddHandler(EventFormActivated, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFormActivated, value);
            }
        }

        private event EventHandler FormDeactivate
        {
            add
            {
                base.Events.AddHandler(EventFormDeactivate, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventFormDeactivate, value);
            }
        }

        public DesignerActionPanel(IServiceProvider serviceProvider)
        {
            base.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            base.SetStyle(ControlStyles.Opaque, true);
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            base.SetStyle(ControlStyles.ResizeRedraw, true);
            base.SetStyle(ControlStyles.UserPaint, true);
            this._serviceProvider = serviceProvider;
            this._lines = new List<Line>();
            this._lineHeights = new List<int>();
            this._lineYPositions = new List<int>();
            this._toolTip = new System.Windows.Forms.ToolTip();
            IUIService service = (IUIService) this.ServiceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
                if (service.Styles["VsColorPanelGradientDark"] is Color)
                {
                    this._gradientDarkColor = (Color) service.Styles["VsColorPanelGradientDark"];
                }
                if (service.Styles["VsColorPanelGradientLight"] is Color)
                {
                    this._gradientLightColor = (Color) service.Styles["VsColorPanelGradientLight"];
                }
                if (service.Styles["VsColorPanelHyperLink"] is Color)
                {
                    this._linkColor = (Color) service.Styles["VsColorPanelHyperLink"];
                }
                if (service.Styles["VsColorPanelHyperLinkPressed"] is Color)
                {
                    this._activeLinkColor = (Color) service.Styles["VsColorPanelHyperLinkPressed"];
                }
                if (service.Styles["VsColorPanelTitleBar"] is Color)
                {
                    this._titleBarColor = (Color) service.Styles["VsColorPanelTitleBar"];
                }
                if (service.Styles["VsColorPanelTitleBarUnselected"] is Color)
                {
                    this._titleBarUnselectedColor = (Color) service.Styles["VsColorPanelTitleBarUnselected"];
                }
                if (service.Styles["VsColorPanelTitleBarText"] is Color)
                {
                    this._titleBarTextColor = (Color) service.Styles["VsColorPanelTitleBarText"];
                }
                if (service.Styles["VsColorPanelBorder"] is Color)
                {
                    this._borderColor = (Color) service.Styles["VsColorPanelBorder"];
                }
                if (service.Styles["VsColorPanelSeparator"] is Color)
                {
                    this._separatorColor = (Color) service.Styles["VsColorPanelSeparator"];
                }
            }
            this.MinimumSize = new Size(150, 0);
        }

        private void AddToCategories(LineInfo lineInfo, ListDictionary categories)
        {
            string category = lineInfo.Item.Category;
            if (category == null)
            {
                category = string.Empty;
            }
            ListDictionary dictionary = (ListDictionary) categories[category];
            if (dictionary == null)
            {
                dictionary = new ListDictionary();
                categories.Add(category, dictionary);
            }
            List<LineInfo> list = (List<LineInfo>) dictionary[lineInfo.List];
            if (list == null)
            {
                list = new List<LineInfo>();
                dictionary.Add(lineInfo.List, list);
            }
            list.Add(lineInfo);
        }

        public static Point ComputePreferredDesktopLocation(Rectangle rectangleAnchor, Size sizePanel, out DockStyle edgeToDock)
        {
            Rectangle workingArea = Screen.FromPoint(rectangleAnchor.Location).WorkingArea;
            bool flag = true;
            bool flag2 = false;
            if ((rectangleAnchor.Right + sizePanel.Width) > workingArea.Right)
            {
                flag = false;
                if ((rectangleAnchor.Left - sizePanel.Width) < workingArea.Left)
                {
                    flag2 = true;
                }
            }
            bool flag3 = flag;
            bool flag4 = false;
            if (flag3)
            {
                if ((rectangleAnchor.Bottom + sizePanel.Height) > workingArea.Bottom)
                {
                    flag3 = false;
                    if ((rectangleAnchor.Top - sizePanel.Height) < workingArea.Top)
                    {
                        flag4 = true;
                    }
                }
            }
            else if ((rectangleAnchor.Top - sizePanel.Height) < workingArea.Top)
            {
                flag3 = true;
                if ((rectangleAnchor.Bottom + sizePanel.Height) > workingArea.Bottom)
                {
                    flag4 = true;
                }
            }
            if (flag4)
            {
                flag2 = false;
            }
            int x = 0;
            int y = 0;
            edgeToDock = DockStyle.None;
            if (flag2 && flag3)
            {
                x = workingArea.Left;
                y = rectangleAnchor.Bottom;
                edgeToDock = DockStyle.Bottom;
            }
            else if (flag2 && !flag3)
            {
                x = workingArea.Left;
                y = rectangleAnchor.Top - sizePanel.Height;
                edgeToDock = DockStyle.Top;
            }
            else if (flag && flag4)
            {
                x = rectangleAnchor.Right;
                y = workingArea.Top;
                edgeToDock = DockStyle.Right;
            }
            else if (flag && flag3)
            {
                x = rectangleAnchor.Right;
                y = rectangleAnchor.Top;
                edgeToDock = DockStyle.Right;
            }
            else if (flag && !flag3)
            {
                x = rectangleAnchor.Right;
                y = rectangleAnchor.Bottom - sizePanel.Height;
                edgeToDock = DockStyle.Right;
            }
            else if (!flag && flag4)
            {
                x = rectangleAnchor.Left - sizePanel.Width;
                y = workingArea.Top;
                edgeToDock = DockStyle.Left;
            }
            else if (!flag && flag3)
            {
                x = rectangleAnchor.Left - sizePanel.Width;
                y = rectangleAnchor.Top;
                edgeToDock = DockStyle.Left;
            }
            else if (!flag && !flag3)
            {
                x = rectangleAnchor.Right - sizePanel.Width;
                y = rectangleAnchor.Top - sizePanel.Height;
                edgeToDock = DockStyle.Top;
            }
            return new Point(x, y);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._toolTip.Dispose();
            }
            base.Dispose(disposing);
        }

        private Size DoLayout(Size proposedSize, bool measureOnly)
        {
            if (base.Disposing || base.IsDisposed)
            {
                return Size.Empty;
            }
            int num = 150;
            int item = 0;
            base.SuspendLayout();
            try
            {
                this._lineYPositions.Clear();
                this._lineHeights.Clear();
                for (int i = 0; i < this._lines.Count; i++)
                {
                    Line line = this._lines[i];
                    this._lineYPositions.Add(item);
                    Size size = line.LayoutControls(item, proposedSize.Width, measureOnly);
                    num = Math.Max(num, size.Width);
                    this._lineHeights.Add(size.Height);
                    item += size.Height;
                }
            }
            finally
            {
                base.ResumeLayout(!measureOnly);
            }
            return new Size(num, item + 2);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            if (proposedSize.IsEmpty)
            {
                return proposedSize;
            }
            return this.DoLayout(proposedSize, true);
        }

        private static bool IsReadOnlyProperty(PropertyDescriptor pd)
        {
            return (pd.IsReadOnly || (pd.ComponentType.GetProperty(pd.Name).GetSetMethod() == null));
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateEditXPos();
        }

        private void OnFormActivated(object sender, EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventFormActivated];
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        private void OnFormClosing(object sender, CancelEventArgs e)
        {
            if (!e.Cancel && (base.TopLevelControl != null))
            {
                Form topLevelControl = (Form) base.TopLevelControl;
                if (topLevelControl != null)
                {
                    topLevelControl.Activated -= new EventHandler(this.OnFormActivated);
                    topLevelControl.Deactivate -= new EventHandler(this.OnFormDeactivate);
                    topLevelControl.Closing -= new CancelEventHandler(this.OnFormClosing);
                }
            }
        }

        private void OnFormDeactivate(object sender, EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventFormDeactivate];
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Form topLevelControl = base.TopLevelControl as Form;
            if (topLevelControl != null)
            {
                topLevelControl.Activated += new EventHandler(this.OnFormActivated);
                topLevelControl.Deactivate += new EventHandler(this.OnFormDeactivate);
                topLevelControl.Closing += new CancelEventHandler(this.OnFormClosing);
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!this._updatingTasks)
            {
                this.DoLayout(base.Size, false);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (this._updatingTasks)
            {
                return;
            }
            Rectangle bounds = base.Bounds;
            if (this.RightToLeft == RightToLeft.Yes)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(bounds, this.GradientDarkColor, this.GradientLightColor, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, base.ClientRectangle);
                    goto Label_0084;
                }
            }
            using (LinearGradientBrush brush2 = new LinearGradientBrush(bounds, this.GradientLightColor, this.GradientDarkColor, LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush2, base.ClientRectangle);
            }
        Label_0084:
            using (Pen pen = new Pen(this.BorderColor))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, base.Width - 1, base.Height - 1));
            }
            Rectangle clipRectangle = e.ClipRectangle;
            int num = 0;
            while ((num < (this._lineYPositions.Count - 1)) && (this._lineYPositions[num + 1] <= clipRectangle.Top))
            {
                num++;
            }
            Graphics g = e.Graphics;
            for (int i = num; i < this._lineYPositions.Count; i++)
            {
                Line line = this._lines[i];
                int y = this._lineYPositions[i];
                int height = this._lineHeights[i];
                int width = base.Width;
                g.SetClip(new Rectangle(0, y, width, height));
                g.TranslateTransform(0f, (float) y);
                line.PaintLine(g, width, height);
                g.ResetTransform();
                if ((y + height) > clipRectangle.Bottom)
                {
                    return;
                }
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            base.PerformLayout();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            Line focusedLine = this.FocusedLine;
            return (((focusedLine != null) && focusedLine.ProcessDialogKey(keyData)) || base.ProcessDialogKey(keyData));
        }

        private void ProcessLists(DesignerActionListCollection lists, ListDictionary categories)
        {
            if (lists != null)
            {
                foreach (DesignerActionList list in lists)
                {
                    if (list != null)
                    {
                        IEnumerable sortedActionItems = list.GetSortedActionItems();
                        if (sortedActionItems != null)
                        {
                            foreach (DesignerActionItem item in sortedActionItems)
                            {
                                if (item != null)
                                {
                                    LineInfo lineInfo = this.ProcessTaskItem(list, item);
                                    if (lineInfo != null)
                                    {
                                        this.AddToCategories(lineInfo, categories);
                                        IComponent relatedComponent = null;
                                        DesignerActionPropertyItem item2 = item as DesignerActionPropertyItem;
                                        if (item2 != null)
                                        {
                                            relatedComponent = item2.RelatedComponent;
                                        }
                                        else
                                        {
                                            DesignerActionMethodItem item3 = item as DesignerActionMethodItem;
                                            if (item3 != null)
                                            {
                                                relatedComponent = item3.RelatedComponent;
                                            }
                                        }
                                        if (relatedComponent != null)
                                        {
                                            IEnumerable<LineInfo> enumerable2 = this.ProcessRelatedTaskItems(relatedComponent);
                                            if (enumerable2 != null)
                                            {
                                                foreach (LineInfo info2 in enumerable2)
                                                {
                                                    this.AddToCategories(info2, categories);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<LineInfo> ProcessRelatedTaskItems(IComponent relatedComponent)
        {
            DesignerActionListCollection componentActions = null;
            DesignerActionService service = (DesignerActionService) this.ServiceProvider.GetService(typeof(DesignerActionService));
            if (service != null)
            {
                componentActions = service.GetComponentActions(relatedComponent);
            }
            else
            {
                IServiceProvider site = relatedComponent.Site;
                if (site == null)
                {
                    site = this.ServiceProvider;
                }
                IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    ComponentDesigner designer = host.GetDesigner(relatedComponent) as ComponentDesigner;
                    if (designer != null)
                    {
                        componentActions = designer.ActionLists;
                    }
                }
            }
            List<LineInfo> list = new List<LineInfo>();
            if (componentActions != null)
            {
                foreach (DesignerActionList list2 in componentActions)
                {
                    if (list2 != null)
                    {
                        IEnumerable sortedActionItems = list2.GetSortedActionItems();
                        if (sortedActionItems != null)
                        {
                            foreach (DesignerActionItem item in sortedActionItems)
                            {
                                if ((item != null) && item.AllowAssociate)
                                {
                                    LineInfo info = this.ProcessTaskItem(list2, item);
                                    if (info != null)
                                    {
                                        list.Add(info);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }

        protected override bool ProcessTabKey(bool forward)
        {
            return base.SelectNextControl(base.ActiveControl, forward, true, true, true);
        }

        private LineInfo ProcessTaskItem(DesignerActionList list, DesignerActionItem item)
        {
            Line line = null;
            if (item is DesignerActionMethodItem)
            {
                line = new MethodLine(this._serviceProvider, this);
            }
            else if (item is DesignerActionPropertyItem)
            {
                DesignerActionPropertyItem item2 = (DesignerActionPropertyItem) item;
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(list)[item2.MemberName];
                if (propDesc == null)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("DesignerActionPanel_CouldNotFindProperty", new object[] { item2.MemberName, list.GetType().FullName }));
                }
                TypeDescriptorContext context = new TypeDescriptorContext(this._serviceProvider, propDesc, list);
                UITypeEditor editor = (UITypeEditor) propDesc.GetEditor(typeof(UITypeEditor));
                bool standardValuesSupported = propDesc.Converter.GetStandardValuesSupported(context);
                if (editor == null)
                {
                    if (propDesc.PropertyType == typeof(bool))
                    {
                        if (IsReadOnlyProperty(propDesc))
                        {
                            line = new TextBoxPropertyLine(this._serviceProvider, this);
                        }
                        else
                        {
                            line = new CheckBoxPropertyLine(this._serviceProvider, this);
                        }
                    }
                    else if (standardValuesSupported)
                    {
                        line = new EditorPropertyLine(this._serviceProvider, this);
                    }
                    else
                    {
                        line = new TextBoxPropertyLine(this._serviceProvider, this);
                    }
                }
                else
                {
                    line = new EditorPropertyLine(this._serviceProvider, this);
                }
            }
            else
            {
                if (!(item is DesignerActionTextItem))
                {
                    return null;
                }
                if (item is DesignerActionHeaderItem)
                {
                    line = new HeaderLine(this._serviceProvider, this);
                }
                else
                {
                    line = new TextLine(this._serviceProvider, this);
                }
            }
            return new LineInfo(list, item, line);
        }

        private void SetDropDownActive(bool active)
        {
            this._dropDownActive = active;
        }

        private void ShowError(string errorMessage)
        {
            IUIService service = (IUIService) this.ServiceProvider.GetService(typeof(IUIService));
            if (service != null)
            {
                service.ShowError(errorMessage);
            }
            else
            {
                MessageBoxOptions options = 0;
                if (System.Design.SR.GetString("RTL") != "RTL_False")
                {
                    options = MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign;
                }
                MessageBox.Show(this, errorMessage, System.Design.SR.GetString("UIServiceHelper_ErrorCaption"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, options);
            }
        }

        private static string StripAmpersands(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '&')
                {
                    i++;
                    if (i == s.Length)
                    {
                        builder.Append('&');
                        break;
                    }
                }
                builder.Append(s[i]);
            }
            return builder.ToString();
        }

        private void UpdateEditXPos()
        {
            int num = 0;
            for (int i = 0; i < this._lines.Count; i++)
            {
                TextBoxPropertyLine line = this._lines[i] as TextBoxPropertyLine;
                if (line != null)
                {
                    num = Math.Max(num, line.GetEditRegionXPos());
                }
            }
            for (int j = 0; j < this._lines.Count; j++)
            {
                TextBoxPropertyLine line2 = this._lines[j] as TextBoxPropertyLine;
                if (line2 != null)
                {
                    line2.SetEditRegionXPos(num);
                }
            }
        }

        public void UpdateTasks(DesignerActionListCollection actionLists, DesignerActionListCollection serviceActionLists, string title, string subtitle)
        {
            this._updatingTasks = true;
            base.SuspendLayout();
            try
            {
                base.AccessibleName = title;
                base.AccessibleDescription = subtitle;
                string focusId = string.Empty;
                Line focusedLine = this.FocusedLine;
                if (focusedLine != null)
                {
                    focusId = focusedLine.FocusId;
                }
                ListDictionary categories = new ListDictionary();
                this.ProcessLists(actionLists, categories);
                this.ProcessLists(serviceActionLists, categories);
                List<LineInfo> list = new List<LineInfo> {
                    new LineInfo(null, new DesignerActionPanelHeaderItem(title, subtitle), new PanelHeaderLine(this._serviceProvider, this))
                };
                int num = 0;
                foreach (ListDictionary dictionary2 in categories.Values)
                {
                    int num2 = 0;
                    foreach (List<LineInfo> list2 in dictionary2.Values)
                    {
                        for (int k = 0; k < list2.Count; k++)
                        {
                            list.Add(list2[k]);
                        }
                        num2++;
                        if (num2 < dictionary2.Count)
                        {
                            list.Add(new LineInfo(null, null, new SeparatorLine(this._serviceProvider, this, true)));
                        }
                    }
                    num++;
                    if (num < categories.Count)
                    {
                        list.Add(new LineInfo(null, null, new SeparatorLine(this._serviceProvider, this)));
                    }
                }
                int currentTabIndex = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    LineInfo info = list[i];
                    Line line = info.Line;
                    bool flag = false;
                    if (i < this._lines.Count)
                    {
                        Line line3 = this._lines[i];
                        if (line3.GetType() == line.GetType())
                        {
                            line3.UpdateActionItem(info.List, info.Item, this._toolTip, ref currentTabIndex);
                            flag = true;
                        }
                        else
                        {
                            line3.RemoveControls(base.Controls);
                            this._lines.RemoveAt(i);
                        }
                    }
                    if (!flag)
                    {
                        List<Control> controls = line.GetControls();
                        Control[] array = new Control[controls.Count];
                        controls.CopyTo(array);
                        base.Controls.AddRange(array);
                        line.UpdateActionItem(info.List, info.Item, this._toolTip, ref currentTabIndex);
                        this._lines.Insert(i, line);
                    }
                }
                for (int j = this._lines.Count - 1; j >= list.Count; j--)
                {
                    this._lines[j].RemoveControls(base.Controls);
                    this._lines.RemoveAt(j);
                }
                if (!string.IsNullOrEmpty(focusId))
                {
                    foreach (Line line5 in this._lines)
                    {
                        if (string.Equals(line5.FocusId, focusId, StringComparison.Ordinal))
                        {
                            line5.Focus();
                        }
                    }
                }
            }
            finally
            {
                this.UpdateEditXPos();
                this._updatingTasks = false;
                base.ResumeLayout(true);
            }
            base.Invalidate();
        }

        public Color ActiveLinkColor
        {
            get
            {
                return this._activeLinkColor;
            }
        }

        public Color BorderColor
        {
            get
            {
                return this._borderColor;
            }
        }

        private bool DropDownActive
        {
            get
            {
                return this._dropDownActive;
            }
        }

        public CommandID[] FilteredCommandIDs
        {
            get
            {
                if (this._filteredCommandIDs == null)
                {
                    this._filteredCommandIDs = new CommandID[] { 
                        StandardCommands.Copy, StandardCommands.Cut, StandardCommands.Delete, StandardCommands.F1Help, StandardCommands.Paste, StandardCommands.Redo, StandardCommands.SelectAll, StandardCommands.Undo, MenuCommands.KeyCancel, MenuCommands.KeyReverseCancel, MenuCommands.KeyDefaultAction, MenuCommands.KeyEnd, MenuCommands.KeyHome, MenuCommands.KeyMoveDown, MenuCommands.KeyMoveLeft, MenuCommands.KeyMoveRight, 
                        MenuCommands.KeyMoveUp, MenuCommands.KeyNudgeDown, MenuCommands.KeyNudgeHeightDecrease, MenuCommands.KeyNudgeHeightIncrease, MenuCommands.KeyNudgeLeft, MenuCommands.KeyNudgeRight, MenuCommands.KeyNudgeUp, MenuCommands.KeyNudgeWidthDecrease, MenuCommands.KeyNudgeWidthIncrease, MenuCommands.KeySizeHeightDecrease, MenuCommands.KeySizeHeightIncrease, MenuCommands.KeySizeWidthDecrease, MenuCommands.KeySizeWidthIncrease, MenuCommands.KeySelectNext, MenuCommands.KeySelectPrevious, MenuCommands.KeyShiftEnd, 
                        MenuCommands.KeyShiftHome
                     };
                }
                return this._filteredCommandIDs;
            }
        }

        private Line FocusedLine
        {
            get
            {
                Control activeControl = base.ActiveControl;
                if (activeControl != null)
                {
                    return (activeControl.Tag as Line);
                }
                return null;
            }
        }

        public Color GradientDarkColor
        {
            get
            {
                return this._gradientDarkColor;
            }
        }

        public Color GradientLightColor
        {
            get
            {
                return this._gradientLightColor;
            }
        }

        public bool InMethodInvoke
        {
            get
            {
                return this._inMethodInvoke;
            }
            internal set
            {
                this._inMethodInvoke = value;
            }
        }

        public Color LinkColor
        {
            get
            {
                return this._linkColor;
            }
        }

        public Color SeparatorColor
        {
            get
            {
                return this._separatorColor;
            }
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this._serviceProvider;
            }
        }

        public Color TitleBarColor
        {
            get
            {
                return this._titleBarColor;
            }
        }

        public Color TitleBarTextColor
        {
            get
            {
                return this._titleBarTextColor;
            }
        }

        public Color TitleBarUnselectedColor
        {
            get
            {
                return this._titleBarUnselectedColor;
            }
        }

        private sealed class CheckBoxPropertyLine : DesignerActionPanel.PropertyLine
        {
            private System.Windows.Forms.CheckBox _checkBox;

            public CheckBoxPropertyLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            protected override void AddControls(List<Control> controls)
            {
                this._checkBox = new System.Windows.Forms.CheckBox();
                this._checkBox.BackColor = Color.Transparent;
                this._checkBox.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._checkBox.CheckedChanged += new EventHandler(this.OnCheckBoxCheckedChanged);
                this._checkBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._checkBox.UseMnemonic = false;
                controls.Add(this._checkBox);
            }

            public sealed override void Focus()
            {
                this._checkBox.Focus();
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                Size preferredSize = this._checkBox.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff));
                if (!measureOnly)
                {
                    this._checkBox.Location = new Point(5, top + 3);
                    this._checkBox.Size = preferredSize;
                }
                return (preferredSize + new Size(9, 7));
            }

            private void OnCheckBoxCheckedChanged(object sender, EventArgs e)
            {
                base.SetValue(this._checkBox.Checked);
            }

            protected override void OnPropertyTaskItemUpdated(System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._checkBox.Text = DesignerActionPanel.StripAmpersands(base.PropertyItem.DisplayName);
                this._checkBox.AccessibleDescription = base.PropertyItem.Description;
                this._checkBox.TabIndex = currentTabIndex++;
                toolTip.SetToolTip(this._checkBox, base.PropertyItem.Description);
            }

            protected override void OnValueChanged()
            {
                this._checkBox.Checked = (bool) base.Value;
            }
        }

        private sealed class DesignerActionPanelHeaderItem : DesignerActionItem
        {
            private string _subtitle;

            public DesignerActionPanelHeaderItem(string title, string subtitle) : base(title, null, null)
            {
                this._subtitle = subtitle;
            }

            public string Subtitle
            {
                get
                {
                    return this._subtitle;
                }
            }
        }

        private sealed class EditorPropertyLine : DesignerActionPanel.TextBoxPropertyLine, IWindowsFormsEditorService, IServiceProvider
        {
            private EditorButton _button;
            private FlyoutDialog _dropDownHolder;
            private UITypeEditor _editor;
            private bool _hasSwatch;
            private bool _ignoreDropDownValue;
            private bool _ignoreNextSelectChange;
            private Image _swatch;

            public EditorPropertyLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            private void ActivateDropDown()
            {
                if (this._editor != null)
                {
                    try
                    {
                        object newValue = this._editor.EditValue(base.TypeDescriptorContext, this, base.Value);
                        base.SetValue(newValue);
                    }
                    catch (Exception exception)
                    {
                        base.ActionPanel.ShowError(System.Design.SR.GetString("DesignerActionPanel_ErrorActivatingDropDown", new object[] { exception.Message }));
                    }
                }
                else
                {
                    ListBox wrapper = new ListBox {
                        BorderStyle = BorderStyle.None,
                        IntegralHeight = false,
                        Font = base.ActionPanel.Font
                    };
                    wrapper.SelectedIndexChanged += new EventHandler(this.OnListBoxSelectedIndexChanged);
                    wrapper.KeyDown += new KeyEventHandler(this.OnListBoxKeyDown);
                    TypeConverter.StandardValuesCollection standardValues = base.GetStandardValues();
                    if (standardValues != null)
                    {
                        foreach (object obj3 in standardValues)
                        {
                            string item = base.PropertyDescriptor.Converter.ConvertToString(base.TypeDescriptorContext, CultureInfo.CurrentCulture, obj3);
                            wrapper.Items.Add(item);
                            if ((obj3 != null) && obj3.Equals(base.Value))
                            {
                                wrapper.SelectedItem = item;
                            }
                        }
                    }
                    int num = 0;
                    IntPtr dC = UnsafeNativeMethods.GetDC(new HandleRef(wrapper, wrapper.Handle));
                    IntPtr handle = wrapper.Font.ToHfont();
                    NativeMethods.CommonHandles.GdiHandleCollector.Add();
                    NativeMethods.TEXTMETRIC lptm = new NativeMethods.TEXTMETRIC();
                    try
                    {
                        handle = SafeNativeMethods.SelectObject(new HandleRef(wrapper, dC), new HandleRef(wrapper.Font, handle));
                        if (wrapper.Items.Count > 0)
                        {
                            NativeMethods.SIZE size = new NativeMethods.SIZE();
                            foreach (string str2 in wrapper.Items)
                            {
                                SafeNativeMethods.GetTextExtentPoint32(new HandleRef(wrapper, dC), str2, str2.Length, size);
                                num = Math.Max(size.cx, num);
                            }
                        }
                        SafeNativeMethods.GetTextMetrics(new HandleRef(wrapper, dC), ref lptm);
                        num += (2 + lptm.tmMaxCharWidth) + SystemInformation.VerticalScrollBarWidth;
                        handle = SafeNativeMethods.SelectObject(new HandleRef(wrapper, dC), new HandleRef(wrapper.Font, handle));
                    }
                    finally
                    {
                        SafeNativeMethods.DeleteObject(new HandleRef(wrapper.Font, handle));
                        UnsafeNativeMethods.ReleaseDC(new HandleRef(wrapper, wrapper.Handle), new HandleRef(wrapper, dC));
                    }
                    wrapper.Height = Math.Max(lptm.tmHeight + 2, Math.Min(200, wrapper.PreferredHeight));
                    wrapper.Width = Math.Max(num, base.EditRegionSize.Width);
                    this._ignoreDropDownValue = false;
                    try
                    {
                        this.ShowDropDown(wrapper, SystemColors.ControlDark);
                    }
                    finally
                    {
                        wrapper.SelectedIndexChanged -= new EventHandler(this.OnListBoxSelectedIndexChanged);
                        wrapper.KeyDown -= new KeyEventHandler(this.OnListBoxKeyDown);
                    }
                    if (!this._ignoreDropDownValue && (wrapper.SelectedItem != null))
                    {
                        base.SetValue(wrapper.SelectedItem);
                    }
                }
            }

            protected override void AddControls(List<Control> controls)
            {
                base.AddControls(controls);
                this._button = new EditorButton();
                this._button.Click += new EventHandler(this.OnButtonClick);
                this._button.GotFocus += new EventHandler(this.OnButtonGotFocus);
                controls.Add(this._button);
            }

            private void CloseDropDown()
            {
                if (this._dropDownHolder != null)
                {
                    this._dropDownHolder.Visible = false;
                }
            }

            protected override int GetTextBoxLeftPadding(int textBoxHeight)
            {
                if (this._hasSwatch)
                {
                    return ((base.GetTextBoxLeftPadding(textBoxHeight) + textBoxHeight) + 2);
                }
                return base.GetTextBoxLeftPadding(textBoxHeight);
            }

            protected override int GetTextBoxRightPadding(int textBoxHeight)
            {
                return ((base.GetTextBoxRightPadding(textBoxHeight) + textBoxHeight) + 2);
            }

            protected override bool IsReadOnly()
            {
                if (!base.IsReadOnly())
                {
                    bool flag = !base.PropertyDescriptor.Converter.CanConvertFrom(base.TypeDescriptorContext, typeof(string));
                    bool flag2 = base.PropertyDescriptor.Converter.GetStandardValuesSupported(base.TypeDescriptorContext) && base.PropertyDescriptor.Converter.GetStandardValuesExclusive(base.TypeDescriptorContext);
                    if (!flag)
                    {
                        return flag2;
                    }
                }
                return true;
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                Size size = base.LayoutControls(top, width, measureOnly);
                if (!measureOnly)
                {
                    int num = (base.EditRegionSize.Height - 2) - 1;
                    this._button.Location = new Point(((base.EditRegionLocation.X + base.EditRegionSize.Width) - num) - 1, (base.EditRegionLocation.Y + 1) + 1);
                    this._button.Size = new Size(num, num);
                }
                return size;
            }

            private void OnButtonClick(object sender, EventArgs e)
            {
                this.ActivateDropDown();
            }

            private void OnButtonGotFocus(object sender, EventArgs e)
            {
                if (!this._button.Ellipsis)
                {
                    this.Focus();
                }
            }

            private void OnListBoxKeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyData == Keys.Enter)
                {
                    this._ignoreNextSelectChange = false;
                    this.CloseDropDown();
                    e.Handled = true;
                }
                else
                {
                    this._ignoreNextSelectChange = true;
                }
            }

            private void OnListBoxSelectedIndexChanged(object sender, EventArgs e)
            {
                if (this._ignoreNextSelectChange)
                {
                    this._ignoreNextSelectChange = false;
                }
                else
                {
                    this.CloseDropDown();
                }
            }

            protected override void OnPropertyTaskItemUpdated(System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._editor = (UITypeEditor) base.PropertyDescriptor.GetEditor(typeof(UITypeEditor));
                base.OnPropertyTaskItemUpdated(toolTip, ref currentTabIndex);
                if (this._editor != null)
                {
                    this._button.Ellipsis = this._editor.GetEditStyle(base.TypeDescriptorContext) == UITypeEditorEditStyle.Modal;
                    this._hasSwatch = this._editor.GetPaintValueSupported(base.TypeDescriptorContext);
                }
                else
                {
                    this._button.Ellipsis = false;
                }
                if (this._button.Ellipsis)
                {
                    base.EditControl.AccessibleRole = this.IsReadOnly() ? AccessibleRole.StaticText : AccessibleRole.Text;
                }
                else
                {
                    base.EditControl.AccessibleRole = this.IsReadOnly() ? AccessibleRole.DropList : AccessibleRole.ComboBox;
                }
                this._button.TabStop = this._button.Ellipsis;
                this._button.TabIndex = currentTabIndex++;
                this._button.AccessibleRole = this._button.Ellipsis ? AccessibleRole.PushButton : AccessibleRole.ButtonDropDown;
                this._button.AccessibleDescription = base.EditControl.AccessibleDescription;
                this._button.AccessibleName = base.EditControl.AccessibleName;
            }

            protected override void OnReadOnlyTextBoxLabelClick(object sender, MouseEventArgs e)
            {
                base.OnReadOnlyTextBoxLabelClick(sender, e);
                if (e.Button == MouseButtons.Left)
                {
                    if (base.ActionPanel.DropDownActive)
                    {
                        this._ignoreDropDownValue = true;
                        this.CloseDropDown();
                    }
                    else
                    {
                        this.ActivateDropDown();
                    }
                }
            }

            protected override void OnValueChanged()
            {
                base.OnValueChanged();
                this._swatch = null;
                if (this._hasSwatch)
                {
                    base.ActionPanel.Invalidate(new Rectangle(base.EditRegionLocation, base.EditRegionSize), false);
                }
            }

            public override void PaintLine(Graphics g, int lineWidth, int lineHeight)
            {
                base.PaintLine(g, lineWidth, lineHeight);
                if (this._hasSwatch)
                {
                    if (this._swatch == null)
                    {
                        int width = base.EditRegionSize.Height - 2;
                        int height = width - 1;
                        this._swatch = new Bitmap(width, height);
                        Rectangle rectangle = new Rectangle(1, 1, width - 2, height - 2);
                        using (Graphics graphics = Graphics.FromImage(this._swatch))
                        {
                            this._editor.PaintValue(base.Value, graphics, rectangle);
                            graphics.DrawRectangle(SystemPens.ControlDark, new Rectangle(0, 0, width - 1, height - 1));
                        }
                    }
                    g.DrawImage(this._swatch, new Point(base.EditRegionRelativeLocation.X + 2, 6));
                }
            }

            protected internal override bool ProcessDialogKey(Keys keyData)
            {
                if (((this._button.Focused || this._button.Ellipsis) || base.ActionPanel.DropDownActive) || (((keyData != (Keys.Alt | Keys.Down)) && (keyData != (Keys.Alt | Keys.Up))) && (keyData != Keys.F4)))
                {
                    return base.ProcessDialogKey(keyData);
                }
                this.ActivateDropDown();
                return true;
            }

            private void ShowDropDown(Control hostedControl, Color borderColor)
            {
                hostedControl.Width = Math.Max(hostedControl.Width, base.EditRegionSize.Width - 2);
                this._dropDownHolder = new DropDownHolder(hostedControl, base.ActionPanel, borderColor, base.ActionPanel.Font, this);
                if (base.ActionPanel.RightToLeft != RightToLeft.Yes)
                {
                    Rectangle r = new Rectangle(Point.Empty, base.EditRegionSize);
                    Size size = this._dropDownHolder.Size;
                    Point point = base.ActionPanel.PointToScreen(base.EditRegionLocation);
                    Rectangle workingArea = Screen.FromRectangle(base.ActionPanel.RectangleToScreen(r)).WorkingArea;
                    size.Width = Math.Max(r.Width + 1, size.Width);
                    point.X = Math.Min(workingArea.Right - size.Width, Math.Max(workingArea.X, (point.X + r.Right) - size.Width));
                    point.Y += r.Y;
                    if (workingArea.Bottom < ((size.Height + point.Y) + r.Height))
                    {
                        point.Y -= size.Height + 1;
                    }
                    else
                    {
                        point.Y += r.Height;
                    }
                    this._dropDownHolder.Location = point;
                }
                else
                {
                    this._dropDownHolder.RightToLeft = base.ActionPanel.RightToLeft;
                    Rectangle rectangle3 = new Rectangle(Point.Empty, base.EditRegionSize);
                    Size size2 = this._dropDownHolder.Size;
                    Point point2 = base.ActionPanel.PointToScreen(base.EditRegionLocation);
                    Rectangle rectangle4 = Screen.FromRectangle(base.ActionPanel.RectangleToScreen(rectangle3)).WorkingArea;
                    size2.Width = Math.Max(rectangle3.Width + 1, size2.Width);
                    point2.X = Math.Min(rectangle4.Right - size2.Width, Math.Max(rectangle4.X, point2.X - rectangle3.Width));
                    point2.Y += rectangle3.Y;
                    if (rectangle4.Bottom < ((size2.Height + point2.Y) + rectangle3.Height))
                    {
                        point2.Y -= size2.Height + 1;
                    }
                    else
                    {
                        point2.Y += rectangle3.Height;
                    }
                    this._dropDownHolder.Location = point2;
                }
                base.ActionPanel.InMethodInvoke = true;
                base.ActionPanel.SetDropDownActive(true);
                try
                {
                    this._dropDownHolder.ShowDropDown(this._button);
                }
                finally
                {
                    this._button.ResetMouseStates();
                    base.ActionPanel.SetDropDownActive(false);
                    base.ActionPanel.InMethodInvoke = false;
                }
            }

            object IServiceProvider.GetService(System.Type serviceType)
            {
                if (serviceType == typeof(IWindowsFormsEditorService))
                {
                    return this;
                }
                return base.ServiceProvider.GetService(serviceType);
            }

            void IWindowsFormsEditorService.CloseDropDown()
            {
                this.CloseDropDown();
            }

            void IWindowsFormsEditorService.DropDownControl(Control control)
            {
                this.ShowDropDown(control, base.ActionPanel.BorderColor);
            }

            DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
            {
                IUIService service = (IUIService) base.ServiceProvider.GetService(typeof(IUIService));
                if (service != null)
                {
                    return service.ShowDialog(dialog);
                }
                return dialog.ShowDialog();
            }

            private class DropDownHolder : DesignerActionPanel.EditorPropertyLine.FlyoutDialog
            {
                private DesignerActionPanel.EditorPropertyLine _parent;

                public DropDownHolder(Control hostedControl, Control parentControl, Color borderColor, Font font, DesignerActionPanel.EditorPropertyLine parent) : base(hostedControl, parentControl, borderColor, font)
                {
                    this._parent = parent;
                    this._parent.ActionPanel.SetDropDownActive(true);
                }

                protected override void OnClosed(EventArgs e)
                {
                    base.OnClosed(e);
                    this._parent.ActionPanel.SetDropDownActive(false);
                }

                protected override bool ProcessDialogKey(Keys keyData)
                {
                    if (keyData == Keys.Escape)
                    {
                        this._parent._ignoreDropDownValue = true;
                        base.Visible = false;
                        return true;
                    }
                    return base.ProcessDialogKey(keyData);
                }
            }

            internal sealed class EditorButton : System.Windows.Forms.Button
            {
                private bool _ellipsis;
                private bool _mouseDown;
                private bool _mouseOver;

                protected override void OnMouseDown(MouseEventArgs e)
                {
                    base.OnMouseDown(e);
                    if (e.Button == MouseButtons.Left)
                    {
                        this._mouseDown = true;
                    }
                }

                protected override void OnMouseEnter(EventArgs e)
                {
                    base.OnMouseEnter(e);
                    this._mouseOver = true;
                }

                protected override void OnMouseLeave(EventArgs e)
                {
                    base.OnMouseLeave(e);
                    this._mouseOver = false;
                }

                protected override void OnMouseUp(MouseEventArgs e)
                {
                    base.OnMouseUp(e);
                    if (e.Button == MouseButtons.Left)
                    {
                        this._mouseDown = false;
                    }
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    Graphics g = e.Graphics;
                    if (this._ellipsis)
                    {
                        PushButtonState normal = PushButtonState.Normal;
                        if (this._mouseDown)
                        {
                            normal = PushButtonState.Pressed;
                        }
                        else if (this._mouseOver)
                        {
                            normal = PushButtonState.Hot;
                        }
                        ButtonRenderer.DrawButton(g, new Rectangle(-1, -1, base.Width + 2, base.Height + 2), "…", this.Font, this.Focused, normal);
                    }
                    else
                    {
                        if (ComboBoxRenderer.IsSupported)
                        {
                            ComboBoxState state = ComboBoxState.Normal;
                            if (base.Enabled)
                            {
                                if (this._mouseDown)
                                {
                                    state = ComboBoxState.Pressed;
                                }
                                else if (this._mouseOver)
                                {
                                    state = ComboBoxState.Hot;
                                }
                            }
                            else
                            {
                                state = ComboBoxState.Disabled;
                            }
                            ComboBoxRenderer.DrawDropDownButton(g, new Rectangle(0, 0, base.Width, base.Height), state);
                        }
                        else
                        {
                            PushButtonState pressed = PushButtonState.Normal;
                            if (base.Enabled)
                            {
                                if (this._mouseDown)
                                {
                                    pressed = PushButtonState.Pressed;
                                }
                                else if (this._mouseOver)
                                {
                                    pressed = PushButtonState.Hot;
                                }
                            }
                            else
                            {
                                pressed = PushButtonState.Disabled;
                            }
                            ButtonRenderer.DrawButton(g, new Rectangle(-1, -1, base.Width + 2, base.Height + 2), string.Empty, this.Font, this.Focused, pressed);
                            try
                            {
                                using (Icon icon = new Icon(typeof(DesignerActionPanel), "Arrow.ico"))
                                {
                                    Bitmap image = icon.ToBitmap();
                                    using (ImageAttributes attributes = new ImageAttributes())
                                    {
                                        ColorMap map = new ColorMap {
                                            OldColor = Color.Black,
                                            NewColor = SystemColors.WindowText
                                        };
                                        attributes.SetRemapTable(new ColorMap[] { map }, ColorAdjustType.Bitmap);
                                        int width = image.Width;
                                        int height = image.Height;
                                        g.DrawImage(image, new Rectangle(((base.Width - width) + 1) / 2, ((base.Height - height) + 1) / 2, width, height), 0, 0, width, width, GraphicsUnit.Pixel, attributes, null, IntPtr.Zero);
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (this.Focused)
                        {
                            ControlPaint.DrawFocusRectangle(g, new Rectangle(2, 2, base.Width - 5, base.Height - 5));
                        }
                    }
                }

                public void ResetMouseStates()
                {
                    this._mouseDown = false;
                    this._mouseOver = false;
                    base.Invalidate();
                }

                public bool Ellipsis
                {
                    get
                    {
                        return this._ellipsis;
                    }
                    set
                    {
                        this._ellipsis = value;
                    }
                }
            }

            internal class FlyoutDialog : Form
            {
                private Control _hostedControl;
                private Control _parentControl;

                public FlyoutDialog(Control hostedControl, Control parentControl, Color borderColor, Font font)
                {
                    this._hostedControl = hostedControl;
                    this._parentControl = parentControl;
                    this.BackColor = SystemColors.Window;
                    base.ControlBox = false;
                    this.Font = font;
                    base.FormBorderStyle = FormBorderStyle.None;
                    base.MinimizeBox = false;
                    base.MaximizeBox = false;
                    base.ShowInTaskbar = false;
                    base.StartPosition = FormStartPosition.Manual;
                    this.Text = string.Empty;
                    base.SuspendLayout();
                    try
                    {
                        base.Controls.Add(hostedControl);
                        int num = Math.Max(this._hostedControl.Width, SystemInformation.MinimumWindowSize.Width);
                        int num2 = Math.Max(this._hostedControl.Height, SystemInformation.MinimizedWindowSize.Height);
                        if (!borderColor.IsEmpty)
                        {
                            base.DockPadding.All = 1;
                            this.BackColor = borderColor;
                            num += 2;
                            num2 += 4;
                        }
                        this._hostedControl.Dock = DockStyle.Fill;
                        base.Width = num;
                        base.Height = num2;
                    }
                    finally
                    {
                        base.ResumeLayout();
                    }
                }

                public void DoModalLoop()
                {
                    while (base.Visible)
                    {
                        Application.DoEvents();
                        DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.MsgWaitForMultipleObjectsEx(0, IntPtr.Zero, 250, 0xff, 4);
                    }
                }

                public virtual void FocusComponent()
                {
                    if ((this._hostedControl != null) && base.Visible)
                    {
                        this._hostedControl.Focus();
                    }
                }

                private bool OwnsWindow(IntPtr hWnd)
                {
                    while (hWnd != IntPtr.Zero)
                    {
                        hWnd = DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.GetWindowLong(new HandleRef(null, hWnd), -8);
                        if (hWnd == IntPtr.Zero)
                        {
                            return false;
                        }
                        if (hWnd == base.Handle)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                protected override bool ProcessDialogKey(Keys keyData)
                {
                    if (((keyData != (Keys.Alt | Keys.Down)) && (keyData != (Keys.Alt | Keys.Up))) && (keyData != Keys.F4))
                    {
                        return base.ProcessDialogKey(keyData);
                    }
                    base.Visible = false;
                    return true;
                }

                public void ShowDropDown(Control parent)
                {
                    try
                    {
                        DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, new HandleRef(parent, parent.Handle));
                        IntPtr capture = DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.GetCapture();
                        if (capture != IntPtr.Zero)
                        {
                            DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.SendMessage(new HandleRef(null, capture), 0x1f, 0, 0);
                            DesignerActionPanel.EditorPropertyLine.SafeNativeMethods.ReleaseCapture();
                        }
                        base.Visible = true;
                        this.FocusComponent();
                        this.DoModalLoop();
                    }
                    finally
                    {
                        DesignerActionPanel.EditorPropertyLine.UnsafeNativeMethods.SetWindowLong(new HandleRef(this, base.Handle), -8, new HandleRef(null, IntPtr.Zero));
                        if ((parent != null) && parent.Visible)
                        {
                            parent.Focus();
                        }
                    }
                }

                protected override void WndProc(ref Message m)
                {
                    if (((m.Msg == 6) && base.Visible) && ((DesignerActionPanel.EditorPropertyLine.NativeMethods.Util.LOWORD((int) ((long) m.WParam)) == 0) && !this.OwnsWindow(m.LParam)))
                    {
                        base.Visible = false;
                        if (m.LParam == IntPtr.Zero)
                        {
                            Control topLevelControl = this._parentControl.TopLevelControl;
                            ToolStripDropDown down = topLevelControl as ToolStripDropDown;
                            if (down != null)
                            {
                                down.Close();
                            }
                            else if (topLevelControl != null)
                            {
                                topLevelControl.Visible = false;
                            }
                        }
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                }

                protected override System.Windows.Forms.CreateParams CreateParams
                {
                    get
                    {
                        System.Windows.Forms.CreateParams createParams = base.CreateParams;
                        createParams.ExStyle |= 0x80;
                        createParams.Style |= -2139095040;
                        createParams.ClassStyle |= 0x800;
                        if ((this._parentControl != null) && !this._parentControl.IsDisposed)
                        {
                            createParams.Parent = this._parentControl.Handle;
                        }
                        return createParams;
                    }
                }
            }

            private static class NativeMethods
            {
                public const int CS_SAVEBITS = 0x800;
                public const int GWL_HWNDPARENT = -8;
                public const int MWMO_INPUTAVAILABLE = 4;
                public const int QS_ALLEVENTS = 0xbf;
                public const int QS_ALLINPUT = 0xff;
                public const int QS_ALLPOSTMESSAGE = 0x100;
                public const int QS_HOTKEY = 0x80;
                public const int QS_INPUT = 7;
                public const int QS_KEY = 1;
                public const int QS_MOUSE = 6;
                public const int QS_MOUSEBUTTON = 4;
                public const int QS_MOUSEMOVE = 2;
                public const int QS_PAINT = 0x20;
                public const int QS_POSTMESSAGE = 8;
                public const int QS_SENDMESSAGE = 0x40;
                public const int QS_TIMER = 0x10;
                public const int WA_ACTIVE = 1;
                public const int WA_INACTIVE = 0;
                public const int WM_ACTIVATE = 6;
                public const int WM_CANCELMODE = 0x1f;
                public const int WM_LBUTTONDOWN = 0x201;
                public const int WM_MBUTTONDOWN = 0x207;
                public const int WM_MOUSEACTIVATE = 0x21;
                public const int WM_NCLBUTTONDOWN = 0xa1;
                public const int WM_NCMBUTTONDOWN = 0xa7;
                public const int WM_NCRBUTTONDOWN = 0xa4;
                public const int WM_RBUTTONDOWN = 0x204;
                public const int WS_BORDER = 0x800000;
                public const int WS_EX_TOOLWINDOW = 0x80;
                public const int WS_POPUP = -2147483648;

                public static class CommonHandles
                {
                    public static HandleCollector GdiHandleCollector = new HandleCollector("GDI", 500);
                    public static HandleCollector HdcHandleCollector = new HandleCollector("HDC", 2);
                }

                [StructLayout(LayoutKind.Sequential)]
                public class SIZE
                {
                    public int cx;
                    public int cy;
                }

                [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
                public struct TEXTMETRIC
                {
                    public int tmHeight;
                    public int tmAscent;
                    public int tmDescent;
                    public int tmInternalLeading;
                    public int tmExternalLeading;
                    public int tmAveCharWidth;
                    public int tmMaxCharWidth;
                    public int tmWeight;
                    public int tmOverhang;
                    public int tmDigitizedAspectX;
                    public int tmDigitizedAspectY;
                    public char tmFirstChar;
                    public char tmLastChar;
                    public char tmDefaultChar;
                    public char tmBreakChar;
                    public byte tmItalic;
                    public byte tmUnderlined;
                    public byte tmStruckOut;
                    public byte tmPitchAndFamily;
                    public byte tmCharSet;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct TEXTMETRICA
                {
                    public int tmHeight;
                    public int tmAscent;
                    public int tmDescent;
                    public int tmInternalLeading;
                    public int tmExternalLeading;
                    public int tmAveCharWidth;
                    public int tmMaxCharWidth;
                    public int tmWeight;
                    public int tmOverhang;
                    public int tmDigitizedAspectX;
                    public int tmDigitizedAspectY;
                    public byte tmFirstChar;
                    public byte tmLastChar;
                    public byte tmDefaultChar;
                    public byte tmBreakChar;
                    public byte tmItalic;
                    public byte tmUnderlined;
                    public byte tmStruckOut;
                    public byte tmPitchAndFamily;
                    public byte tmCharSet;
                }

                internal static class Util
                {
                    public static int LOWORD(int n)
                    {
                        return (n & 0xffff);
                    }
                }
            }

            private static class SafeNativeMethods
            {
                public static bool DeleteObject(HandleRef hObject)
                {
                    DesignerActionPanel.EditorPropertyLine.NativeMethods.CommonHandles.GdiHandleCollector.Remove();
                    return IntDeleteObject(hObject);
                }

                [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
                public static extern int GetTextExtentPoint32(HandleRef hDC, string str, int len, [In, Out] DesignerActionPanel.EditorPropertyLine.NativeMethods.SIZE size);
                public static int GetTextMetrics(HandleRef hDC, ref DesignerActionPanel.EditorPropertyLine.NativeMethods.TEXTMETRIC lptm)
                {
                    if (Marshal.SystemDefaultCharSize == 1)
                    {
                        DesignerActionPanel.EditorPropertyLine.NativeMethods.TEXTMETRICA textmetrica = new DesignerActionPanel.EditorPropertyLine.NativeMethods.TEXTMETRICA();
                        int textMetricsA = GetTextMetricsA(hDC, ref textmetrica);
                        lptm.tmHeight = textmetrica.tmHeight;
                        lptm.tmAscent = textmetrica.tmAscent;
                        lptm.tmDescent = textmetrica.tmDescent;
                        lptm.tmInternalLeading = textmetrica.tmInternalLeading;
                        lptm.tmExternalLeading = textmetrica.tmExternalLeading;
                        lptm.tmAveCharWidth = textmetrica.tmAveCharWidth;
                        lptm.tmMaxCharWidth = textmetrica.tmMaxCharWidth;
                        lptm.tmWeight = textmetrica.tmWeight;
                        lptm.tmOverhang = textmetrica.tmOverhang;
                        lptm.tmDigitizedAspectX = textmetrica.tmDigitizedAspectX;
                        lptm.tmDigitizedAspectY = textmetrica.tmDigitizedAspectY;
                        lptm.tmFirstChar = (char) textmetrica.tmFirstChar;
                        lptm.tmLastChar = (char) textmetrica.tmLastChar;
                        lptm.tmDefaultChar = (char) textmetrica.tmDefaultChar;
                        lptm.tmBreakChar = (char) textmetrica.tmBreakChar;
                        lptm.tmItalic = textmetrica.tmItalic;
                        lptm.tmUnderlined = textmetrica.tmUnderlined;
                        lptm.tmStruckOut = textmetrica.tmStruckOut;
                        lptm.tmPitchAndFamily = textmetrica.tmPitchAndFamily;
                        lptm.tmCharSet = textmetrica.tmCharSet;
                        return textMetricsA;
                    }
                    return GetTextMetricsW(hDC, ref lptm);
                }

                [DllImport("gdi32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
                public static extern int GetTextMetricsA(HandleRef hDC, [In, Out] ref DesignerActionPanel.EditorPropertyLine.NativeMethods.TEXTMETRICA lptm);
                [DllImport("gdi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
                public static extern int GetTextMetricsW(HandleRef hDC, [In, Out] ref DesignerActionPanel.EditorPropertyLine.NativeMethods.TEXTMETRIC lptm);
                [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
                private static extern bool IntDeleteObject(HandleRef hObject);
                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern bool ReleaseCapture();
                [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
                public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);
            }

            private static class UnsafeNativeMethods
            {
                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern IntPtr GetCapture();
                public static IntPtr GetDC(HandleRef hWnd)
                {
                    DesignerActionPanel.EditorPropertyLine.NativeMethods.CommonHandles.HdcHandleCollector.Add();
                    return IntGetDC(hWnd);
                }

                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern IntPtr GetWindowLong(HandleRef hWnd, int nIndex);
                [DllImport("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Auto, ExactSpelling=true)]
                private static extern IntPtr IntGetDC(HandleRef hWnd);
                [DllImport("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Auto, ExactSpelling=true)]
                private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);
                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern int MsgWaitForMultipleObjectsEx(int nCount, IntPtr pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags);
                public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
                {
                    DesignerActionPanel.EditorPropertyLine.NativeMethods.CommonHandles.HdcHandleCollector.Remove();
                    return IntReleaseDC(hWnd, hDC);
                }

                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);
                [DllImport("user32.dll", CharSet=CharSet.Auto)]
                public static extern IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
            }
        }

        private sealed class HeaderLine : DesignerActionPanel.TextLine
        {
            public HeaderLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            protected override Font GetFont()
            {
                return new Font(base.ActionPanel.Font, FontStyle.Bold);
            }
        }

        private abstract class Line
        {
            private DesignerActionPanel _actionPanel;
            private List<Control> _addedControls;
            private IServiceProvider _serviceProvider;

            public Line(IServiceProvider serviceProvider, DesignerActionPanel actionPanel)
            {
                if (actionPanel == null)
                {
                    throw new ArgumentNullException("actionPanel");
                }
                this._serviceProvider = serviceProvider;
                this._actionPanel = actionPanel;
            }

            protected abstract void AddControls(List<Control> controls);
            public abstract void Focus();
            internal List<Control> GetControls()
            {
                this._addedControls = new List<Control>();
                this.AddControls(this._addedControls);
                foreach (Control control in this._addedControls)
                {
                    control.Tag = this;
                }
                return this._addedControls;
            }

            public abstract Size LayoutControls(int top, int width, bool measureOnly);
            public virtual void PaintLine(Graphics g, int lineWidth, int lineHeight)
            {
            }

            protected internal virtual bool ProcessDialogKey(Keys keyData)
            {
                return false;
            }

            internal void RemoveControls(Control.ControlCollection controls)
            {
                for (int i = 0; i < this._addedControls.Count; i++)
                {
                    Control control = this._addedControls[i];
                    control.Tag = null;
                    controls.Remove(control);
                }
            }

            internal abstract void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex);

            protected DesignerActionPanel ActionPanel
            {
                get
                {
                    return this._actionPanel;
                }
            }

            public abstract string FocusId { get; }

            protected IServiceProvider ServiceProvider
            {
                get
                {
                    return this._serviceProvider;
                }
            }
        }

        private class LineInfo
        {
            public DesignerActionItem Item;
            public System.ComponentModel.Design.DesignerActionPanel.Line Line;
            public DesignerActionList List;

            public LineInfo(DesignerActionList list, DesignerActionItem item, System.ComponentModel.Design.DesignerActionPanel.Line line)
            {
                this.Line = line;
                this.Item = item;
                this.List = list;
            }
        }

        private sealed class MethodLine : DesignerActionPanel.Line
        {
            private DesignerActionList _actionList;
            private MethodItemLinkLabel _linkLabel;
            private DesignerActionMethodItem _methodItem;

            public MethodLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            protected override void AddControls(List<Control> controls)
            {
                this._linkLabel = new MethodItemLinkLabel();
                this._linkLabel.ActiveLinkColor = base.ActionPanel.ActiveLinkColor;
                this._linkLabel.AutoSize = false;
                this._linkLabel.BackColor = Color.Transparent;
                this._linkLabel.LinkBehavior = LinkBehavior.HoverUnderline;
                this._linkLabel.LinkColor = base.ActionPanel.LinkColor;
                this._linkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._linkLabel.UseMnemonic = false;
                this._linkLabel.VisitedLinkColor = base.ActionPanel.LinkColor;
                this._linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnLinkLabelLinkClicked);
                controls.Add(this._linkLabel);
            }

            public sealed override void Focus()
            {
                this._linkLabel.Focus();
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                Size preferredSize = this._linkLabel.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff));
                if (!measureOnly)
                {
                    this._linkLabel.Location = new Point(5, top + 3);
                    this._linkLabel.Size = preferredSize;
                }
                return (preferredSize + new Size(9, 7));
            }

            private void OnLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                base.ActionPanel.InMethodInvoke = true;
                try
                {
                    this._methodItem.Invoke();
                }
                catch (Exception innerException)
                {
                    if (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }
                    base.ActionPanel.ShowError(System.Design.SR.GetString("DesignerActionPanel_ErrorInvokingAction", new object[] { this._methodItem.DisplayName, Environment.NewLine + innerException.Message }));
                }
                finally
                {
                    base.ActionPanel.InMethodInvoke = false;
                }
            }

            internal override void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._actionList = actionList;
                this._methodItem = (DesignerActionMethodItem) actionItem;
                toolTip.SetToolTip(this._linkLabel, this._methodItem.Description);
                this._linkLabel.Text = DesignerActionPanel.StripAmpersands(this._methodItem.DisplayName);
                this._linkLabel.AccessibleDescription = actionItem.Description;
                this._linkLabel.TabIndex = currentTabIndex++;
            }

            public sealed override string FocusId
            {
                get
                {
                    return ("METHOD:" + this._actionList.GetType().FullName + "." + this._methodItem.MemberName);
                }
            }

            private sealed class MethodItemLinkLabel : LinkLabel
            {
                protected override bool ProcessDialogKey(Keys keyData)
                {
                    if ((keyData & Keys.Control) == Keys.Control)
                    {
                        Keys keys = keyData & Keys.KeyCode;
                        switch (keys)
                        {
                            case Keys.Tab:
                                return false;
                        }
                    }
                    return base.ProcessDialogKey(keyData);
                }
            }
        }

        private sealed class PanelHeaderLine : DesignerActionPanel.Line
        {
            private DesignerActionList _actionList;
            private bool _formActive;
            private DesignerActionPanel.DesignerActionPanelHeaderItem _panelHeaderItem;
            private Label _subtitleLabel;
            private Label _titleLabel;

            public PanelHeaderLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
                actionPanel.FontChanged += new EventHandler(this.OnParentControlFontChanged);
            }

            protected override void AddControls(List<Control> controls)
            {
                this._titleLabel = new Label();
                this._titleLabel.BackColor = Color.Transparent;
                this._titleLabel.ForeColor = base.ActionPanel.TitleBarTextColor;
                this._titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._titleLabel.UseMnemonic = false;
                this._subtitleLabel = new Label();
                this._subtitleLabel.BackColor = Color.Transparent;
                this._subtitleLabel.ForeColor = base.ActionPanel.TitleBarTextColor;
                this._subtitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._subtitleLabel.UseMnemonic = false;
                controls.Add(this._titleLabel);
                controls.Add(this._subtitleLabel);
                base.ActionPanel.FormActivated += new EventHandler(this.OnFormActivated);
                base.ActionPanel.FormDeactivate += new EventHandler(this.OnFormDeactivate);
            }

            public sealed override void Focus()
            {
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                Size preferredSize = this._titleLabel.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff));
                Size empty = Size.Empty;
                if (!string.IsNullOrEmpty(this._panelHeaderItem.Subtitle))
                {
                    empty = this._subtitleLabel.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff));
                }
                if (!measureOnly)
                {
                    this._titleLabel.Location = new Point(5, top + 3);
                    this._titleLabel.Size = preferredSize;
                    this._subtitleLabel.Location = new Point(5, (top + 6) + preferredSize.Height);
                    this._subtitleLabel.Size = empty;
                }
                int num = Math.Max(preferredSize.Width, empty.Width) + 10;
                int num2 = empty.IsEmpty ? (preferredSize.Height + 6) : ((preferredSize.Height + empty.Height) + 9);
                return new Size(num + 2, num2 + 1);
            }

            private void OnFormActivated(object sender, EventArgs e)
            {
                this._formActive = true;
                base.ActionPanel.Invalidate();
            }

            private void OnFormDeactivate(object sender, EventArgs e)
            {
                this._formActive = false;
                base.ActionPanel.Invalidate();
            }

            private void OnParentControlFontChanged(object sender, EventArgs e)
            {
                if ((this._titleLabel != null) && (this._subtitleLabel != null))
                {
                    this._titleLabel.Font = new Font(base.ActionPanel.Font, FontStyle.Bold);
                    this._subtitleLabel.Font = base.ActionPanel.Font;
                }
            }

            public override void PaintLine(Graphics g, int lineWidth, int lineHeight)
            {
                Color color;
                color = (this._formActive || base.ActionPanel.DropDownActive) ? (color = base.ActionPanel.TitleBarColor) : base.ActionPanel.TitleBarUnselectedColor;
                using (SolidBrush brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, 1, 1, lineWidth - 2, lineHeight - 1);
                }
                using (Pen pen = new Pen(base.ActionPanel.BorderColor))
                {
                    g.DrawLine(pen, 0, lineHeight - 1, lineWidth, lineHeight - 1);
                }
            }

            internal override void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._actionList = actionList;
                this._panelHeaderItem = (DesignerActionPanel.DesignerActionPanelHeaderItem) actionItem;
                this._titleLabel.Text = this._panelHeaderItem.DisplayName;
                this._titleLabel.TabIndex = currentTabIndex++;
                this._subtitleLabel.Text = this._panelHeaderItem.Subtitle;
                this._subtitleLabel.TabIndex = currentTabIndex++;
                this._subtitleLabel.Visible = this._subtitleLabel.Text.Length != 0;
                this.OnParentControlFontChanged(null, EventArgs.Empty);
            }

            public sealed override string FocusId
            {
                get
                {
                    return string.Empty;
                }
            }
        }

        private abstract class PropertyLine : DesignerActionPanel.Line
        {
            private DesignerActionList _actionList;
            private System.ComponentModel.PropertyDescriptor _propDesc;
            private DesignerActionPropertyItem _propertyItem;
            private bool _pushingValue;
            private ITypeDescriptorContext _typeDescriptorContext;
            private object _value;

            public PropertyLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            protected abstract void OnPropertyTaskItemUpdated(System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex);
            protected abstract void OnValueChanged();
            protected void SetValue(object newValue)
            {
                if (!this._pushingValue && !base.ActionPanel.DropDownActive)
                {
                    this._pushingValue = true;
                    try
                    {
                        if (newValue != null)
                        {
                            System.Type c = newValue.GetType();
                            if (!this.PropertyDescriptor.PropertyType.IsAssignableFrom(c) && (this.PropertyDescriptor.Converter != null))
                            {
                                if (!this.PropertyDescriptor.Converter.CanConvertFrom(this._typeDescriptorContext, c))
                                {
                                    base.ActionPanel.ShowError(System.Design.SR.GetString("DesignerActionPanel_CouldNotConvertValue", new object[] { newValue, this._propDesc.PropertyType }));
                                    return;
                                }
                                newValue = this.PropertyDescriptor.Converter.ConvertFrom(this._typeDescriptorContext, CultureInfo.CurrentCulture, newValue);
                            }
                        }
                        if (!object.Equals(this._value, newValue))
                        {
                            this.PropertyDescriptor.SetValue(this._actionList, newValue);
                            this._value = this.PropertyDescriptor.GetValue(this._actionList);
                            this.OnValueChanged();
                        }
                    }
                    catch (Exception innerException)
                    {
                        if (innerException is TargetInvocationException)
                        {
                            innerException = innerException.InnerException;
                        }
                        base.ActionPanel.ShowError(System.Design.SR.GetString("DesignerActionPanel_ErrorSettingValue", new object[] { newValue, this.PropertyDescriptor.Name, innerException.Message }));
                    }
                    finally
                    {
                        this._pushingValue = false;
                    }
                }
            }

            internal sealed override void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._actionList = actionList;
                this._propertyItem = (DesignerActionPropertyItem) actionItem;
                this._propDesc = null;
                this._typeDescriptorContext = null;
                this._value = this.PropertyDescriptor.GetValue(actionList);
                this.OnPropertyTaskItemUpdated(toolTip, ref currentTabIndex);
                this._pushingValue = true;
                try
                {
                    this.OnValueChanged();
                }
                finally
                {
                    this._pushingValue = false;
                }
            }

            public sealed override string FocusId
            {
                get
                {
                    return ("PROPERTY:" + this._actionList.GetType().FullName + "." + this._propertyItem.MemberName);
                }
            }

            protected System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    if (this._propDesc == null)
                    {
                        this._propDesc = TypeDescriptor.GetProperties(this._actionList)[this._propertyItem.MemberName];
                    }
                    return this._propDesc;
                }
            }

            protected DesignerActionPropertyItem PropertyItem
            {
                get
                {
                    return this._propertyItem;
                }
            }

            protected ITypeDescriptorContext TypeDescriptorContext
            {
                get
                {
                    if (this._typeDescriptorContext == null)
                    {
                        this._typeDescriptorContext = new System.ComponentModel.Design.DesignerActionPanel.TypeDescriptorContext(base.ServiceProvider, this.PropertyDescriptor, this._actionList);
                    }
                    return this._typeDescriptorContext;
                }
            }

            protected object Value
            {
                get
                {
                    return this._value;
                }
            }
        }

        private sealed class SeparatorLine : DesignerActionPanel.Line
        {
            private bool _isSubSeparator;

            public SeparatorLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : this(serviceProvider, actionPanel, false)
            {
            }

            public SeparatorLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel, bool isSubSeparator) : base(serviceProvider, actionPanel)
            {
                this._isSubSeparator = isSubSeparator;
            }

            protected override void AddControls(List<Control> controls)
            {
            }

            public sealed override void Focus()
            {
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                return new Size(150, 1);
            }

            public override void PaintLine(Graphics g, int lineWidth, int lineHeight)
            {
                using (Pen pen = new Pen(base.ActionPanel.SeparatorColor))
                {
                    g.DrawLine(pen, 3, 0, lineWidth - 4, 0);
                }
            }

            internal override void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
            }

            public sealed override string FocusId
            {
                get
                {
                    return string.Empty;
                }
            }
        }

        private class TextBoxPropertyLine : DesignerActionPanel.PropertyLine
        {
            private Control _editControl;
            private Point _editRegionLocation;
            private Point _editRegionRelativeLocation;
            private Size _editRegionSize;
            private int _editXPos;
            private Label _label;
            private EditorLabel _readOnlyTextBoxLabel;
            private System.Windows.Forms.TextBox _textBox;
            private bool _textBoxDirty;

            public TextBoxPropertyLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
            }

            protected override void AddControls(List<Control> controls)
            {
                this._label = new Label();
                this._label.BackColor = Color.Transparent;
                this._label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._label.UseMnemonic = false;
                this._readOnlyTextBoxLabel = new EditorLabel();
                this._readOnlyTextBoxLabel.BackColor = Color.Transparent;
                this._readOnlyTextBoxLabel.TabStop = true;
                this._readOnlyTextBoxLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                this._readOnlyTextBoxLabel.UseMnemonic = false;
                this._readOnlyTextBoxLabel.Visible = false;
                this._readOnlyTextBoxLabel.MouseClick += new MouseEventHandler(this.OnReadOnlyTextBoxLabelClick);
                this._readOnlyTextBoxLabel.Enter += new EventHandler(this.OnReadOnlyTextBoxLabelEnter);
                this._readOnlyTextBoxLabel.Leave += new EventHandler(this.OnReadOnlyTextBoxLabelLeave);
                this._readOnlyTextBoxLabel.KeyDown += new KeyEventHandler(this.OnReadOnlyTextBoxLabelKeyDown);
                this._textBox = new System.Windows.Forms.TextBox();
                this._textBox.BorderStyle = BorderStyle.None;
                this._textBox.TextAlign = HorizontalAlignment.Left;
                this._textBox.Visible = false;
                this._textBox.TextChanged += new EventHandler(this.OnTextBoxTextChanged);
                this._textBox.KeyDown += new KeyEventHandler(this.OnTextBoxKeyDown);
                this._textBox.LostFocus += new EventHandler(this.OnTextBoxLostFocus);
                controls.Add(this._readOnlyTextBoxLabel);
                controls.Add(this._textBox);
                controls.Add(this._label);
            }

            public sealed override void Focus()
            {
                this._editControl.Focus();
            }

            internal int GetEditRegionXPos()
            {
                if (string.IsNullOrEmpty(this._label.Text))
                {
                    return 5;
                }
                return ((5 + this._label.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff)).Width) + 5);
            }

            protected TypeConverter.StandardValuesCollection GetStandardValues()
            {
                TypeConverter converter = base.PropertyDescriptor.Converter;
                if ((converter != null) && converter.GetStandardValuesSupported(base.TypeDescriptorContext))
                {
                    return converter.GetStandardValues(base.TypeDescriptorContext);
                }
                return null;
            }

            protected virtual int GetTextBoxLeftPadding(int textBoxHeight)
            {
                return 1;
            }

            protected virtual int GetTextBoxRightPadding(int textBoxHeight)
            {
                return 1;
            }

            protected virtual bool IsReadOnly()
            {
                return DesignerActionPanel.IsReadOnlyProperty(base.PropertyDescriptor);
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                int textBoxHeight = this._textBox.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff)).Height + 2;
                int height = ((textBoxHeight + 7) + 2) + 2;
                int x = Math.Max(this._editXPos, this.GetEditRegionXPos());
                int num4 = (x + 150) + 4;
                width = Math.Max(width, num4);
                int num5 = width - num4;
                if (!measureOnly)
                {
                    this._editRegionLocation = new Point(x, top + 4);
                    this._editRegionRelativeLocation = new Point(x, 4);
                    this._editRegionSize = new Size(150 + num5, textBoxHeight + 2);
                    this._label.Location = new Point(5, top);
                    int num6 = this._label.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff)).Width;
                    this._label.Size = new Size(num6, height);
                    int num7 = 0;
                    if (this._editControl is System.Windows.Forms.TextBox)
                    {
                        num7 = 2;
                    }
                    this._editControl.Location = new Point(((this._editRegionLocation.X + this.GetTextBoxLeftPadding(textBoxHeight)) + 1) + num7, (this._editRegionLocation.Y + 1) + 1);
                    this._editControl.Width = ((this._editRegionSize.Width - this.GetTextBoxRightPadding(textBoxHeight)) - this.GetTextBoxLeftPadding(textBoxHeight)) - num7;
                    this._editControl.Height = (this._editRegionSize.Height - 2) - 1;
                }
                return new Size(width, height);
            }

            private void OnEditControlKeyDown(KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Down)
                {
                    e.Handled = true;
                    TypeConverter.StandardValuesCollection standardValues = this.GetStandardValues();
                    if (standardValues != null)
                    {
                        for (int i = 0; i < standardValues.Count; i++)
                        {
                            if (object.Equals(base.Value, standardValues[i]))
                            {
                                if (i < (standardValues.Count - 1))
                                {
                                    base.SetValue(standardValues[i + 1]);
                                }
                                return;
                            }
                        }
                        if (standardValues.Count > 0)
                        {
                            base.SetValue(standardValues[0]);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    e.Handled = true;
                    TypeConverter.StandardValuesCollection valuess2 = this.GetStandardValues();
                    if (valuess2 != null)
                    {
                        for (int j = 0; j < valuess2.Count; j++)
                        {
                            if (object.Equals(base.Value, valuess2[j]))
                            {
                                if (j > 0)
                                {
                                    base.SetValue(valuess2[j - 1]);
                                }
                                return;
                            }
                        }
                        if (valuess2.Count > 0)
                        {
                            base.SetValue(valuess2[valuess2.Count - 1]);
                        }
                    }
                }
            }

            protected override void OnPropertyTaskItemUpdated(System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._label.Text = DesignerActionPanel.StripAmpersands(base.PropertyItem.DisplayName);
                this._label.TabIndex = currentTabIndex++;
                toolTip.SetToolTip(this._label, base.PropertyItem.Description);
                this._textBoxDirty = false;
                if (this.IsReadOnly())
                {
                    this._readOnlyTextBoxLabel.Visible = true;
                    this._textBox.Visible = false;
                    this._textBox.Location = new Point(0x7fffffff, 0x7fffffff);
                    this._editControl = this._readOnlyTextBoxLabel;
                }
                else
                {
                    this._readOnlyTextBoxLabel.Visible = false;
                    this._readOnlyTextBoxLabel.Location = new Point(0x7fffffff, 0x7fffffff);
                    this._textBox.Visible = true;
                    this._editControl = this._textBox;
                }
                this._editControl.AccessibleDescription = base.PropertyItem.Description;
                this._editControl.AccessibleName = DesignerActionPanel.StripAmpersands(base.PropertyItem.DisplayName);
                this._editControl.TabIndex = currentTabIndex++;
                this._editControl.BringToFront();
            }

            protected virtual void OnReadOnlyTextBoxLabelClick(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.Focus();
                }
            }

            private void OnReadOnlyTextBoxLabelEnter(object sender, EventArgs e)
            {
                this._readOnlyTextBoxLabel.ForeColor = SystemColors.HighlightText;
                this._readOnlyTextBoxLabel.BackColor = SystemColors.Highlight;
            }

            private void OnReadOnlyTextBoxLabelKeyDown(object sender, KeyEventArgs e)
            {
                this.OnEditControlKeyDown(e);
            }

            private void OnReadOnlyTextBoxLabelLeave(object sender, EventArgs e)
            {
                this._readOnlyTextBoxLabel.ForeColor = SystemColors.WindowText;
                this._readOnlyTextBoxLabel.BackColor = SystemColors.Window;
            }

            private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
            {
                if (!base.ActionPanel.DropDownActive)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        this.UpdateValue();
                        e.Handled = true;
                    }
                    else
                    {
                        this.OnEditControlKeyDown(e);
                    }
                }
            }

            private void OnTextBoxLostFocus(object sender, EventArgs e)
            {
                if (!base.ActionPanel.DropDownActive)
                {
                    this.UpdateValue();
                }
            }

            private void OnTextBoxTextChanged(object sender, EventArgs e)
            {
                this._textBoxDirty = true;
            }

            protected override void OnValueChanged()
            {
                this._editControl.Text = base.PropertyDescriptor.Converter.ConvertToString(base.TypeDescriptorContext, base.Value);
            }

            public override void PaintLine(Graphics g, int lineWidth, int lineHeight)
            {
                Rectangle rect = new Rectangle(this.EditRegionRelativeLocation, this.EditRegionSize);
                g.FillRectangle(SystemBrushes.Window, rect);
                g.DrawRectangle(SystemPens.ControlDark, rect);
            }

            internal void SetEditRegionXPos(int xPos)
            {
                if (!string.IsNullOrEmpty(this._label.Text))
                {
                    this._editXPos = xPos;
                }
                else
                {
                    this._editXPos = 5;
                }
            }

            private void UpdateValue()
            {
                if (this._textBoxDirty)
                {
                    base.SetValue(this._editControl.Text);
                    this._textBoxDirty = false;
                }
            }

            protected Control EditControl
            {
                get
                {
                    return this._editControl;
                }
            }

            protected Point EditRegionLocation
            {
                get
                {
                    return this._editRegionLocation;
                }
            }

            protected Point EditRegionRelativeLocation
            {
                get
                {
                    return this._editRegionRelativeLocation;
                }
            }

            protected Size EditRegionSize
            {
                get
                {
                    return this._editRegionSize;
                }
            }

            private sealed class EditorLabel : Label
            {
                public EditorLabel()
                {
                    base.SetStyle(ControlStyles.Selectable, true);
                }

                protected override AccessibleObject CreateAccessibilityInstance()
                {
                    return new EditorLabelAccessibleObject(this);
                }

                protected override bool IsInputKey(Keys keyData)
                {
                    if ((keyData != Keys.Down) && (keyData != Keys.Up))
                    {
                        return base.IsInputKey(keyData);
                    }
                    return true;
                }

                protected override void OnGotFocus(EventArgs e)
                {
                    base.OnGotFocus(e);
                    base.AccessibilityNotifyClients(AccessibleEvents.Focus, 0, -1);
                }

                private sealed class EditorLabelAccessibleObject : Control.ControlAccessibleObject
                {
                    public EditorLabelAccessibleObject(DesignerActionPanel.TextBoxPropertyLine.EditorLabel owner) : base(owner)
                    {
                    }

                    public override string Value
                    {
                        get
                        {
                            return base.Owner.Text;
                        }
                    }
                }
            }
        }

        private class TextLine : DesignerActionPanel.Line
        {
            private Label _label;
            private DesignerActionTextItem _textItem;

            public TextLine(IServiceProvider serviceProvider, DesignerActionPanel actionPanel) : base(serviceProvider, actionPanel)
            {
                actionPanel.FontChanged += new EventHandler(this.OnParentControlFontChanged);
            }

            protected override void AddControls(List<Control> controls)
            {
                this._label = new Label();
                this._label.BackColor = Color.Transparent;
                this._label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                this._label.UseMnemonic = false;
                controls.Add(this._label);
            }

            public sealed override void Focus()
            {
            }

            protected virtual Font GetFont()
            {
                return base.ActionPanel.Font;
            }

            public override Size LayoutControls(int top, int width, bool measureOnly)
            {
                Size preferredSize = this._label.GetPreferredSize(new Size(0x7fffffff, 0x7fffffff));
                if (!measureOnly)
                {
                    this._label.Location = new Point(5, top + 3);
                    this._label.Size = preferredSize;
                }
                return (preferredSize + new Size(9, 7));
            }

            private void OnParentControlFontChanged(object sender, EventArgs e)
            {
                if ((this._label != null) && (this._label.Font != null))
                {
                    this._label.Font = this.GetFont();
                }
            }

            internal override void UpdateActionItem(DesignerActionList actionList, DesignerActionItem actionItem, System.Windows.Forms.ToolTip toolTip, ref int currentTabIndex)
            {
                this._textItem = (DesignerActionTextItem) actionItem;
                this._label.Text = DesignerActionPanel.StripAmpersands(this._textItem.DisplayName);
                this._label.Font = this.GetFont();
                this._label.TabIndex = currentTabIndex++;
                toolTip.SetToolTip(this._label, this._textItem.Description);
            }

            public sealed override string FocusId
            {
                get
                {
                    return string.Empty;
                }
            }
        }

        internal sealed class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
        {
            private object _instance;
            private System.ComponentModel.PropertyDescriptor _propDesc;
            private IServiceProvider _serviceProvider;

            public TypeDescriptorContext(IServiceProvider serviceProvider, System.ComponentModel.PropertyDescriptor propDesc, object instance)
            {
                this._serviceProvider = serviceProvider;
                this._propDesc = propDesc;
                this._instance = instance;
            }

            public object GetService(System.Type serviceType)
            {
                return this._serviceProvider.GetService(serviceType);
            }

            public void OnComponentChanged()
            {
                if (this.ComponentChangeService != null)
                {
                    this.ComponentChangeService.OnComponentChanged(this._instance, this._propDesc, null, null);
                }
            }

            public bool OnComponentChanging()
            {
                if (this.ComponentChangeService != null)
                {
                    try
                    {
                        this.ComponentChangeService.OnComponentChanging(this._instance, this._propDesc);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return false;
                    }
                }
                return true;
            }

            private IComponentChangeService ComponentChangeService
            {
                get
                {
                    return (IComponentChangeService) this._serviceProvider.GetService(typeof(IComponentChangeService));
                }
            }

            public IContainer Container
            {
                get
                {
                    return (IContainer) this._serviceProvider.GetService(typeof(IContainer));
                }
            }

            public object Instance
            {
                get
                {
                    return this._instance;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return this._propDesc;
                }
            }
        }
    }
}

