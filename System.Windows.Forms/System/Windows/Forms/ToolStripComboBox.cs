namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Windows.Forms.Design;

    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ContextMenuStrip | ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ToolStrip), DefaultProperty("Items")]
    public class ToolStripComboBox : ToolStripControlHost
    {
        internal static readonly object EventDropDown = new object();
        internal static readonly object EventDropDownClosed = new object();
        internal static readonly object EventDropDownStyleChanged = new object();
        internal static readonly object EventSelectedIndexChanged = new object();
        internal static readonly object EventSelectionChangeCommitted = new object();
        internal static readonly object EventTextUpdate = new object();

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler DoubleClick
        {
            add
            {
                base.DoubleClick += value;
            }
            remove
            {
                base.DoubleClick -= value;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxOnDropDownDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler DropDown
        {
            add
            {
                base.Events.AddHandler(EventDropDown, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDropDown, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxOnDropDownClosedDescr")]
        public event EventHandler DropDownClosed
        {
            add
            {
                base.Events.AddHandler(EventDropDownClosed, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDropDownClosed, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("ComboBoxDropDownStyleChangedDescr")]
        public event EventHandler DropDownStyleChanged
        {
            add
            {
                base.Events.AddHandler(EventDropDownStyleChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventDropDownStyleChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("selectedIndexChangedEventDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxOnTextUpdateDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public event EventHandler TextUpdate
        {
            add
            {
                base.Events.AddHandler(EventTextUpdate, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTextUpdate, value);
            }
        }

        public ToolStripComboBox() : base(CreateControlInstance())
        {
            ToolStripComboBoxControl control = base.Control as ToolStripComboBoxControl;
            control.Owner = this;
        }

        public ToolStripComboBox(string name) : this()
        {
            base.Name = name;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ToolStripComboBox(Control c) : base(c)
        {
            throw new NotSupportedException(System.Windows.Forms.SR.GetString("ToolStripMustSupplyItsOwnComboBox"));
        }

        public void BeginUpdate()
        {
            this.ComboBox.BeginUpdate();
        }

        private static Control CreateControlInstance()
        {
            return new ToolStripComboBoxControl { FlatStyle = System.Windows.Forms.FlatStyle.Popup, Font = ToolStripManager.DefaultFont };
        }

        public void EndUpdate()
        {
            this.ComboBox.EndUpdate();
        }

        public int FindString(string s)
        {
            return this.ComboBox.FindString(s);
        }

        public int FindString(string s, int startIndex)
        {
            return this.ComboBox.FindString(s, startIndex);
        }

        public int FindStringExact(string s)
        {
            return this.ComboBox.FindStringExact(s);
        }

        public int FindStringExact(string s, int startIndex)
        {
            return this.ComboBox.FindStringExact(s, startIndex);
        }

        public int GetItemHeight(int index)
        {
            return this.ComboBox.GetItemHeight(index);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            Size preferredSize = base.GetPreferredSize(constrainingSize);
            preferredSize.Width = Math.Max(preferredSize.Width, 0x4b);
            return preferredSize;
        }

        private void HandleDropDown(object sender, EventArgs e)
        {
            this.OnDropDown(e);
        }

        private void HandleDropDownClosed(object sender, EventArgs e)
        {
            this.OnDropDownClosed(e);
        }

        private void HandleDropDownStyleChanged(object sender, EventArgs e)
        {
            this.OnDropDownStyleChanged(e);
        }

        private void HandleSelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnSelectedIndexChanged(e);
        }

        private void HandleSelectionChangeCommitted(object sender, EventArgs e)
        {
            this.OnSelectionChangeCommitted(e);
        }

        private void HandleTextUpdate(object sender, EventArgs e)
        {
            this.OnTextUpdate(e);
        }

        protected virtual void OnDropDown(EventArgs e)
        {
            if (base.ParentInternal != null)
            {
                Application.ThreadContext.FromCurrent().RemoveMessageFilter(base.ParentInternal.RestoreFocusFilter);
                ToolStripManager.ModalMenuFilter.SuspendMenuMode();
            }
            base.RaiseEvent(EventDropDown, e);
        }

        protected virtual void OnDropDownClosed(EventArgs e)
        {
            if (base.ParentInternal != null)
            {
                Application.ThreadContext.FromCurrent().RemoveMessageFilter(base.ParentInternal.RestoreFocusFilter);
                ToolStripManager.ModalMenuFilter.ResumeMenuMode();
            }
            base.RaiseEvent(EventDropDownClosed, e);
        }

        protected virtual void OnDropDownStyleChanged(EventArgs e)
        {
            base.RaiseEvent(EventDropDownStyleChanged, e);
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            base.RaiseEvent(EventSelectedIndexChanged, e);
        }

        protected virtual void OnSelectionChangeCommitted(EventArgs e)
        {
            base.RaiseEvent(EventSelectionChangeCommitted, e);
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            System.Windows.Forms.ComboBox box = control as System.Windows.Forms.ComboBox;
            if (box != null)
            {
                box.DropDown += new EventHandler(this.HandleDropDown);
                box.DropDownClosed += new EventHandler(this.HandleDropDownClosed);
                box.DropDownStyleChanged += new EventHandler(this.HandleDropDownStyleChanged);
                box.SelectedIndexChanged += new EventHandler(this.HandleSelectedIndexChanged);
                box.SelectionChangeCommitted += new EventHandler(this.HandleSelectionChangeCommitted);
                box.TextUpdate += new EventHandler(this.HandleTextUpdate);
            }
            base.OnSubscribeControlEvents(control);
        }

        protected virtual void OnTextUpdate(EventArgs e)
        {
            base.RaiseEvent(EventTextUpdate, e);
        }

        protected override void OnUnsubscribeControlEvents(Control control)
        {
            System.Windows.Forms.ComboBox box = control as System.Windows.Forms.ComboBox;
            if (box != null)
            {
                box.DropDown -= new EventHandler(this.HandleDropDown);
                box.DropDownClosed -= new EventHandler(this.HandleDropDownClosed);
                box.DropDownStyleChanged -= new EventHandler(this.HandleDropDownStyleChanged);
                box.SelectedIndexChanged -= new EventHandler(this.HandleSelectedIndexChanged);
                box.SelectionChangeCommitted -= new EventHandler(this.HandleSelectionChangeCommitted);
                box.TextUpdate -= new EventHandler(this.HandleTextUpdate);
            }
            base.OnUnsubscribeControlEvents(control);
        }

        public void Select(int start, int length)
        {
            this.ComboBox.Select(start, length);
        }

        public void SelectAll()
        {
            this.ComboBox.SelectAll();
        }

        private bool ShouldSerializeDropDownWidth()
        {
            return this.ComboBox.ShouldSerializeDropDownWidth();
        }

        internal override bool ShouldSerializeFont()
        {
            return !object.Equals(this.Font, ToolStripManager.DefaultFont);
        }

        public override string ToString()
        {
            return (base.ToString() + ", Items.Count: " + this.Items.Count.ToString(CultureInfo.CurrentCulture));
        }

        [EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteCustomSourceDescr"), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), Browsable(true)]
        public AutoCompleteStringCollection AutoCompleteCustomSource
        {
            get
            {
                return this.ComboBox.AutoCompleteCustomSource;
            }
            set
            {
                this.ComboBox.AutoCompleteCustomSource = value;
            }
        }

        [DefaultValue(0), EditorBrowsable(EditorBrowsableState.Always), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteModeDescr"), Browsable(true)]
        public System.Windows.Forms.AutoCompleteMode AutoCompleteMode
        {
            get
            {
                return this.ComboBox.AutoCompleteMode;
            }
            set
            {
                this.ComboBox.AutoCompleteMode = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), DefaultValue(0x80), System.Windows.Forms.SRDescription("ComboBoxAutoCompleteSourceDescr"), Browsable(true)]
        public System.Windows.Forms.AutoCompleteSource AutoCompleteSource
        {
            get
            {
                return this.ComboBox.AutoCompleteSource;
            }
            set
            {
                this.ComboBox.AutoCompleteSource = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public System.Windows.Forms.ComboBox ComboBox
        {
            get
            {
                return (base.Control as System.Windows.Forms.ComboBox);
            }
        }

        protected internal override Padding DefaultMargin
        {
            get
            {
                if (base.IsOnDropDown)
                {
                    return new Padding(2);
                }
                return new Padding(1, 0, 1, 0);
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(100, 0x16);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(0x6a), System.Windows.Forms.SRDescription("ComboBoxDropDownHeightDescr"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public int DropDownHeight
        {
            get
            {
                return this.ComboBox.DropDownHeight;
            }
            set
            {
                this.ComboBox.DropDownHeight = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("ComboBoxStyleDescr"), RefreshProperties(RefreshProperties.Repaint), DefaultValue(1)]
        public ComboBoxStyle DropDownStyle
        {
            get
            {
                return this.ComboBox.DropDownStyle;
            }
            set
            {
                this.ComboBox.DropDownStyle = value;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxDropDownWidthDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public int DropDownWidth
        {
            get
            {
                return this.ComboBox.DropDownWidth;
            }
            set
            {
                this.ComboBox.DropDownWidth = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ComboBoxDroppedDownDescr")]
        public bool DroppedDown
        {
            get
            {
                return this.ComboBox.DroppedDown;
            }
            set
            {
                this.ComboBox.DroppedDown = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatAppearance"), DefaultValue(1), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxFlatStyleDescr")]
        public System.Windows.Forms.FlatStyle FlatStyle
        {
            get
            {
                return this.ComboBox.FlatStyle;
            }
            set
            {
                this.ComboBox.FlatStyle = value;
            }
        }

        [DefaultValue(true), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxIntegralHeightDescr")]
        public bool IntegralHeight
        {
            get
            {
                return this.ComboBox.IntegralHeight;
            }
            set
            {
                this.ComboBox.IntegralHeight = value;
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxItemsDescr"), Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public System.Windows.Forms.ComboBox.ObjectCollection Items
        {
            get
            {
                return this.ComboBox.Items;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxMaxDropDownItemsDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(8), Localizable(true)]
        public int MaxDropDownItems
        {
            get
            {
                return this.ComboBox.MaxDropDownItems;
            }
            set
            {
                this.ComboBox.MaxDropDownItems = value;
            }
        }

        [DefaultValue(0), System.Windows.Forms.SRCategory("CatBehavior"), Localizable(true), System.Windows.Forms.SRDescription("ComboBoxMaxLengthDescr")]
        public int MaxLength
        {
            get
            {
                return this.ComboBox.MaxLength;
            }
            set
            {
                this.ComboBox.MaxLength = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ComboBoxSelectedIndexDescr")]
        public int SelectedIndex
        {
            get
            {
                return this.ComboBox.SelectedIndex;
            }
            set
            {
                this.ComboBox.SelectedIndex = value;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectedItemDescr"), Bindable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedItem
        {
            get
            {
                return this.ComboBox.SelectedItem;
            }
            set
            {
                this.ComboBox.SelectedItem = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectedTextDescr")]
        public string SelectedText
        {
            get
            {
                return this.ComboBox.SelectedText;
            }
            set
            {
                this.ComboBox.SelectedText = value;
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ComboBoxSelectionLengthDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                return this.ComboBox.SelectionLength;
            }
            set
            {
                this.ComboBox.SelectionLength = value;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxSelectionStartDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                return this.ComboBox.SelectionStart;
            }
            set
            {
                this.ComboBox.SelectionStart = value;
            }
        }

        [System.Windows.Forms.SRDescription("ComboBoxSortedDescr"), System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false)]
        public bool Sorted
        {
            get
            {
                return this.ComboBox.Sorted;
            }
            set
            {
                this.ComboBox.Sorted = value;
            }
        }

        internal class ToolStripComboBoxControl : ComboBox
        {
            private ToolStripComboBox owner;

            public ToolStripComboBoxControl()
            {
                base.FlatStyle = FlatStyle.Popup;
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            }

            internal override ComboBox.FlatComboAdapter CreateFlatComboAdapterInstance()
            {
                return new ToolStripComboBoxFlatComboAdapter(this);
            }

            protected override bool IsInputKey(Keys keyData)
            {
                if (((keyData & Keys.Alt) != Keys.Alt) || (((keyData & Keys.Down) != Keys.Down) && ((keyData & Keys.Up) != Keys.Up)))
                {
                    return base.IsInputKey(keyData);
                }
                return true;
            }

            protected override void OnDropDownClosed(EventArgs e)
            {
                base.OnDropDownClosed(e);
                base.Invalidate();
                base.Update();
            }

            private ProfessionalColorTable ColorTable
            {
                get
                {
                    if (this.Owner != null)
                    {
                        ToolStripProfessionalRenderer renderer = this.Owner.Renderer as ToolStripProfessionalRenderer;
                        if (renderer != null)
                        {
                            return renderer.ColorTable;
                        }
                    }
                    return ProfessionalColors.ColorTable;
                }
            }

            public ToolStripComboBox Owner
            {
                get
                {
                    return this.owner;
                }
                set
                {
                    this.owner = value;
                }
            }

            internal class ToolStripComboBoxFlatComboAdapter : ComboBox.FlatComboAdapter
            {
                public ToolStripComboBoxFlatComboAdapter(ComboBox comboBox) : base(comboBox, true)
                {
                }

                protected override void DrawFlatComboDropDown(ComboBox comboBox, Graphics g, Rectangle dropDownRect)
                {
                    Brush brush5;
                    Point point;
                    if (UseBaseAdapter(comboBox))
                    {
                        base.DrawFlatComboDropDown(comboBox, g, dropDownRect);
                        return;
                    }
                    if (!comboBox.Enabled || !ToolStripManager.VisualStylesEnabled)
                    {
                        g.FillRectangle(SystemBrushes.Control, dropDownRect);
                    }
                    else
                    {
                        ToolStripComboBox.ToolStripComboBoxControl toolStripComboBoxControl = comboBox as ToolStripComboBox.ToolStripComboBoxControl;
                        ProfessionalColorTable colorTable = GetColorTable(toolStripComboBoxControl);
                        if (!comboBox.DroppedDown)
                        {
                            if (comboBox.ContainsFocus || comboBox.MouseIsOver)
                            {
                                using (Brush brush = new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonSelectedGradientBegin, colorTable.ComboBoxButtonSelectedGradientEnd, LinearGradientMode.Vertical))
                                {
                                    g.FillRectangle(brush, dropDownRect);
                                    goto Label_0114;
                                }
                            }
                            if (toolStripComboBoxControl.Owner.IsOnOverflow)
                            {
                                using (Brush brush2 = new SolidBrush(colorTable.ComboBoxButtonOnOverflow))
                                {
                                    g.FillRectangle(brush2, dropDownRect);
                                    goto Label_0114;
                                }
                            }
                            using (Brush brush3 = new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonGradientBegin, colorTable.ComboBoxButtonGradientEnd, LinearGradientMode.Vertical))
                            {
                                g.FillRectangle(brush3, dropDownRect);
                                goto Label_0114;
                            }
                        }
                        using (Brush brush4 = new LinearGradientBrush(dropDownRect, colorTable.ComboBoxButtonPressedGradientBegin, colorTable.ComboBoxButtonPressedGradientEnd, LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(brush4, dropDownRect);
                        }
                    }
                Label_0114:
                    brush5 = comboBox.Enabled ? SystemBrushes.ControlText : SystemBrushes.GrayText;
                    point = new Point(dropDownRect.Left + (dropDownRect.Width / 2), dropDownRect.Top + (dropDownRect.Height / 2)) {
                        X = point.X + (dropDownRect.Width % 2)
                    };
                    Point[] points = new Point[] { new Point(point.X - 2, point.Y - 1), new Point(point.X + 3, point.Y - 1), new Point(point.X, point.Y + 2) };
                    g.FillPolygon(brush5, points);
                }

                private static ProfessionalColorTable GetColorTable(ToolStripComboBox.ToolStripComboBoxControl toolStripComboBoxControl)
                {
                    if (toolStripComboBoxControl != null)
                    {
                        return toolStripComboBoxControl.ColorTable;
                    }
                    return ProfessionalColors.ColorTable;
                }

                protected override Color GetOuterBorderColor(ComboBox comboBox)
                {
                    if (UseBaseAdapter(comboBox))
                    {
                        return base.GetOuterBorderColor(comboBox);
                    }
                    if (!comboBox.Enabled)
                    {
                        return GetColorTable(comboBox as ToolStripComboBox.ToolStripComboBoxControl).ComboBoxBorder;
                    }
                    return SystemColors.Window;
                }

                protected override Color GetPopupOuterBorderColor(ComboBox comboBox, bool focused)
                {
                    if (UseBaseAdapter(comboBox))
                    {
                        return base.GetPopupOuterBorderColor(comboBox, focused);
                    }
                    if (!comboBox.Enabled)
                    {
                        return SystemColors.ControlDark;
                    }
                    if (!focused)
                    {
                        return SystemColors.Window;
                    }
                    return GetColorTable(comboBox as ToolStripComboBox.ToolStripComboBoxControl).ComboBoxBorder;
                }

                private static bool UseBaseAdapter(ComboBox comboBox)
                {
                    ToolStripComboBox.ToolStripComboBoxControl control = comboBox as ToolStripComboBox.ToolStripComboBoxControl;
                    if ((control != null) && (control.Owner.Renderer is ToolStripProfessionalRenderer))
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}

