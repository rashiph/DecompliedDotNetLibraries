namespace System.Windows.Forms.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class TableLayoutPanelDesigner : FlowPanelDesigner
    {
        private DesignerActionListCollection actionLists;
        private int colCountBeforeAdd;
        private PropertyDescriptor colStyleProp;
        private IComponentChangeService compSvc;
        private ToolStripMenuItem contextMenuCol;
        private ToolStripMenuItem contextMenuRow;
        private DesignerTableLayoutControlCollection controls;
        private int curCol = -1;
        private int curRow = -1;
        private BaseContextMenuStrip designerContextMenuStrip;
        private ArrayList dragComps;
        private Point droppedCellPosition = ControlDesigner.InvalidPoint;
        private int ensureSuspendCount;
        private Dictionary<string, bool> extenderProperties;
        private Control localDragControl;
        private DesignerVerb removeColVerb;
        private DesignerVerb removeRowVerb;
        private int rowCountBeforeAdd;
        private PropertyDescriptor rowStyleProp;
        private TableLayoutPanelBehavior tlpBehavior;
        private UndoEngine undoEngine;
        private bool undoing;
        private DesignerVerbCollection verbs;

        private void AddControlInternal(Control c, int col, int row)
        {
            this.Table.ControlAdded -= new ControlEventHandler(this.OnControlAdded);
            this.Table.Controls.Add(c, col, row);
            this.Table.ControlAdded += new ControlEventHandler(this.OnControlAdded);
        }

        private void BuildActionLists()
        {
            this.actionLists = new DesignerActionListCollection();
            this.actionLists.Add(new TableLayouPanelRowColumnActionList(this));
            this.actionLists[0].AutoShow = true;
        }

        private ToolStripDropDownMenu BuildMenu(bool isRow)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            ToolStripMenuItem item2 = new ToolStripMenuItem();
            ToolStripMenuItem item3 = new ToolStripMenuItem();
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripLabel label = new ToolStripLabel();
            ToolStripMenuItem item4 = new ToolStripMenuItem();
            ToolStripMenuItem item5 = new ToolStripMenuItem();
            ToolStripMenuItem item6 = new ToolStripMenuItem();
            item.Text = System.Design.SR.GetString("TableLayoutPanelDesignerAddMenu");
            item.Tag = isRow;
            item.Name = "add";
            item.Click += new EventHandler(this.OnAddClick);
            item2.Text = System.Design.SR.GetString("TableLayoutPanelDesignerInsertMenu");
            item2.Tag = isRow;
            item2.Name = "insert";
            item2.Click += new EventHandler(this.OnInsertClick);
            item3.Text = System.Design.SR.GetString("TableLayoutPanelDesignerDeleteMenu");
            item3.Tag = isRow;
            item3.Name = "delete";
            item3.Click += new EventHandler(this.OnDeleteClick);
            label.Text = System.Design.SR.GetString("TableLayoutPanelDesignerLabelMenu");
            if (System.Design.SR.GetString("TableLayoutPanelDesignerDontBoldLabel") == "0")
            {
                label.Font = new Font(label.Font, FontStyle.Bold);
            }
            label.Name = "sizemode";
            item4.Text = System.Design.SR.GetString("TableLayoutPanelDesignerAbsoluteMenu");
            item4.Tag = isRow;
            item4.Name = "absolute";
            item4.Click += new EventHandler(this.OnAbsoluteClick);
            item5.Text = System.Design.SR.GetString("TableLayoutPanelDesignerPercentageMenu");
            item5.Tag = isRow;
            item5.Name = "percent";
            item5.Click += new EventHandler(this.OnPercentClick);
            item6.Text = System.Design.SR.GetString("TableLayoutPanelDesignerAutoSizeMenu");
            item6.Tag = isRow;
            item6.Name = "autosize";
            item6.Click += new EventHandler(this.OnAutoSizeClick);
            ToolStripDropDownMenu menu = new ToolStripDropDownMenu();
            menu.Items.AddRange(new ToolStripItem[] { item, item2, item3, separator, label, item4, item5, item6 });
            menu.Tag = isRow;
            menu.Opening += new CancelEventHandler(this.OnRowColMenuOpening);
            IUIService service = this.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                menu.Renderer = (ToolStripProfessionalRenderer) service.Styles["VsRenderer"];
            }
            return menu;
        }

        protected internal override bool CanAddComponent(IComponent component)
        {
            if (this.Table.GrowStyle == TableLayoutPanelGrowStyle.FixedSize)
            {
                Control control = base.GetControl(component);
                if (control == null)
                {
                    return false;
                }
                int rowSpan = this.Table.GetRowSpan(control);
                int columnSpan = this.Table.GetColumnSpan(control);
                int length = this.Table.GetRowHeights().Length;
                int columns = this.Table.GetColumnWidths().Length;
                int num5 = 0;
                int num6 = length * columns;
                int num7 = rowSpan * columnSpan;
                bool[,] cells = null;
                if (num7 > 1)
                {
                    cells = new bool[columns, length];
                }
                if (num7 <= num6)
                {
                    for (int i = 0; i < length; i++)
                    {
                        for (int j = 0; j < columns; j++)
                        {
                            if (this.Table.GetControlFromPosition(j, i) != null)
                            {
                                num5++;
                                if (num7 > 1)
                                {
                                    cells[j, i] = true;
                                }
                            }
                        }
                    }
                }
                if ((num5 + num7) > num6)
                {
                    ((IUIService) this.GetService(typeof(IUIService))).ShowError(System.Design.SR.GetString("TableLayoutPanelFullDesc"));
                    return false;
                }
                if ((num7 > 1) && !SubsetExists(cells, columns, length, columnSpan, rowSpan))
                {
                    ((IUIService) this.GetService(typeof(IUIService))).ShowError(System.Design.SR.GetString("TableLayoutPanelSpanDesc"));
                    return false;
                }
            }
            return true;
        }

        private void ChangeSizeType(bool isRow, SizeType newType)
        {
            TableLayoutStyleCollection rowStyles = null;
            try
            {
                if (isRow)
                {
                    rowStyles = this.Table.RowStyles;
                }
                else
                {
                    rowStyles = this.Table.ColumnStyles;
                }
                int index = isRow ? this.curRow : this.curCol;
                if (rowStyles[index].SizeType != newType)
                {
                    int[] rowHeights = this.Table.GetRowHeights();
                    int[] columnWidths = this.Table.GetColumnWidths();
                    if ((!isRow || (rowHeights.Length >= (index - 1))) && (isRow || (columnWidths.Length >= (index - 1))))
                    {
                        IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if ((service != null) && (this.Table.Site != null))
                        {
                            using (DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString("TableLayoutPanelDesignerChangeSizeTypeUndoUnit", new object[] { this.Table.Site.Name })))
                            {
                                try
                                {
                                    this.Table.SuspendLayout();
                                    this.PropChanging(isRow ? this.rowStyleProp : this.colStyleProp);
                                    switch (newType)
                                    {
                                        case SizeType.AutoSize:
                                            rowStyles[index].SizeType = SizeType.AutoSize;
                                            goto Label_01B5;

                                        case SizeType.Absolute:
                                            rowStyles[index].SizeType = SizeType.Absolute;
                                            if (!isRow)
                                            {
                                                break;
                                            }
                                            this.Table.RowStyles[index].Height = rowHeights[index];
                                            goto Label_01B5;

                                        case SizeType.Percent:
                                            rowStyles[index].SizeType = SizeType.Percent;
                                            if (!isRow)
                                            {
                                                goto Label_018A;
                                            }
                                            this.Table.RowStyles[index].Height = DesignerUtils.MINIMUMSTYLEPERCENT;
                                            goto Label_01B5;

                                        default:
                                            goto Label_01B5;
                                    }
                                    this.Table.ColumnStyles[index].Width = columnWidths[index];
                                    goto Label_01B5;
                                Label_018A:
                                    this.Table.ColumnStyles[index].Width = DesignerUtils.MINIMUMSTYLEPERCENT;
                                Label_01B5:
                                    this.PropChanged(isRow ? this.rowStyleProp : this.colStyleProp);
                                    this.Table.ResumeLayout();
                                    transaction.Commit();
                                }
                                catch (CheckoutException exception)
                                {
                                    if (!CheckoutException.Canceled.Equals(exception))
                                    {
                                        throw;
                                    }
                                    if (transaction != null)
                                    {
                                        transaction.Cancel();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException exception2)
            {
                ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception2.Message);
            }
        }

        private void CheckVerbStatus()
        {
            if (this.Table != null)
            {
                if (this.removeColVerb != null)
                {
                    bool flag = this.Table.ColumnCount > 1;
                    if (this.removeColVerb.Enabled != flag)
                    {
                        this.removeColVerb.Enabled = flag;
                    }
                }
                if (this.removeRowVerb != null)
                {
                    bool flag2 = this.Table.RowCount > 1;
                    if (this.removeRowVerb.Enabled != flag2)
                    {
                        this.removeRowVerb.Enabled = flag2;
                    }
                }
                this.RefreshSmartTag();
            }
        }

        private void ControlAddedInternal(Control control, Point newControlPosition, bool localReposition, bool fullTable, DragEventArgs de)
        {
            if (fullTable)
            {
                if (this.Table.GrowStyle == TableLayoutPanelGrowStyle.AddRows)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.Table)["RowCount"];
                    if (descriptor != null)
                    {
                        descriptor.SetValue(this.Table, this.Table.GetRowHeights().Length);
                    }
                    newControlPosition.X = 0;
                    newControlPosition.Y = this.Table.RowCount - 1;
                }
                else if (this.Table.GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
                {
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.Table)["ColumnCount"];
                    if (descriptor2 != null)
                    {
                        descriptor2.SetValue(this.Table, this.Table.GetColumnWidths().Length);
                    }
                    newControlPosition.X = this.Table.ColumnCount - 1;
                    newControlPosition.Y = 0;
                }
            }
            DesignerTransaction transaction = null;
            PropertyDescriptor prop = TypeDescriptor.GetProperties(this.Table)["Controls"];
            try
            {
                bool flag = ((de != null) && (de.Effect == DragDropEffects.Copy)) && localReposition;
                Control controlFromPosition = ((TableLayoutPanel) this.Control).GetControlFromPosition(newControlPosition.X, newControlPosition.Y);
                if (flag)
                {
                    IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (host != null)
                    {
                        transaction = host.CreateTransaction(System.Design.SR.GetString("BehaviorServiceCopyControl", new object[] { control.Site.Name }));
                    }
                    this.PropChanging(prop);
                }
                else if ((controlFromPosition != null) && !controlFromPosition.Equals(control))
                {
                    if (localReposition)
                    {
                        IDesignerHost host2 = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host2 != null)
                        {
                            transaction = host2.CreateTransaction(System.Design.SR.GetString("TableLayoutPanelDesignerControlsSwapped", new object[] { control.Site.Name, controlFromPosition.Site.Name }));
                        }
                        this.PropChanging(prop);
                        this.RemoveControlInternal(controlFromPosition);
                    }
                    else
                    {
                        this.PropChanging(prop);
                        controlFromPosition = null;
                    }
                }
                else
                {
                    if (localReposition)
                    {
                        IDesignerHost host3 = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host3 != null)
                        {
                            transaction = host3.CreateTransaction(System.Design.SR.GetString("BehaviorServiceMoveControl", new object[] { control.Site.Name }));
                        }
                    }
                    controlFromPosition = null;
                    this.PropChanging(prop);
                }
                if (flag)
                {
                    ArrayList objects = new ArrayList();
                    objects.Add(control);
                    objects = DesignerUtils.CopyDragObjects(objects, base.Component.Site) as ArrayList;
                    control = objects[0] as Control;
                }
                if (localReposition)
                {
                    Point controlPosition = this.GetControlPosition(control);
                    if (controlPosition != ControlDesigner.InvalidPoint)
                    {
                        this.RemoveControlInternal(control);
                        if ((controlPosition != newControlPosition) && (controlFromPosition != null))
                        {
                            this.AddControlInternal(controlFromPosition, controlPosition.X, controlPosition.Y);
                        }
                    }
                }
                if (localReposition)
                {
                    this.AddControlInternal(control, newControlPosition.X, newControlPosition.Y);
                }
                else
                {
                    this.Table.SetCellPosition(control, new TableLayoutPanelCellPosition(newControlPosition.X, newControlPosition.Y));
                }
                this.PropChanged(prop);
                if (de != null)
                {
                    base.OnDragComplete(de);
                }
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
                if (flag)
                {
                    ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service != null)
                    {
                        service.SetSelectedComponents(new object[] { control }, SelectionTypes.Click | SelectionTypes.Replace);
                    }
                }
            }
            catch (ArgumentException exception)
            {
                IUIService service2 = this.GetService(typeof(IUIService)) as IUIService;
                if (service2 != null)
                {
                    service2.ShowError(exception);
                }
            }
            catch (Exception exception2)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
            }
        }

        private void CreateEmptyTable()
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.Table)["ColumnCount"];
            if (descriptor != null)
            {
                descriptor.SetValue(this.Table, DesignerUtils.DEFAULTCOLUMNCOUNT);
            }
            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.Table)["RowCount"];
            if (descriptor2 != null)
            {
                descriptor2.SetValue(this.Table, DesignerUtils.DEFAULTROWCOUNT);
            }
            this.EnsureAvailableStyles();
            this.InitializeNewStyles();
        }

        protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            this.rowCountBeforeAdd = Math.Max(0, this.Table.GetRowHeights().Length);
            this.colCountBeforeAdd = Math.Max(0, this.Table.GetColumnWidths().Length);
            return base.CreateToolCore(tool, x, y, width, height, hasLocation, hasSize);
        }

        internal void DeleteRowCol(bool isRow, int index)
        {
            if (isRow)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.Table)["RowCount"];
                if (descriptor != null)
                {
                    descriptor.SetValue(this.Table, this.Table.RowCount - 1);
                    this.PropChanging(this.rowStyleProp);
                    this.Table.RowStyles.RemoveAt(index);
                    this.PropChanged(this.rowStyleProp);
                }
            }
            else
            {
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.Table)["ColumnCount"];
                if (descriptor2 != null)
                {
                    descriptor2.SetValue(this.Table, this.Table.ColumnCount - 1);
                    this.PropChanging(this.colStyleProp);
                    this.Table.ColumnStyles.RemoveAt(index);
                    this.PropChanged(this.colStyleProp);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    service.TransactionClosing -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosing);
                }
                if (this.undoEngine != null)
                {
                    if (this.Undoing)
                    {
                        this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                    }
                    this.undoEngine.Undoing -= new EventHandler(this.OnUndoing);
                }
                if (this.compSvc != null)
                {
                    this.compSvc.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    this.compSvc.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                }
                if (this.Table != null)
                {
                    this.Table.ControlAdded -= new ControlEventHandler(this.OnControlAdded);
                    this.Table.ControlRemoved -= new ControlEventHandler(this.OnControlRemoved);
                }
                if (this.contextMenuRow != null)
                {
                    this.contextMenuRow.Dispose();
                }
                if (this.contextMenuCol != null)
                {
                    this.contextMenuCol.Dispose();
                }
                this.rowStyleProp = null;
                this.colStyleProp = null;
            }
            base.Dispose(disposing);
        }

        private bool DoesPropertyAffectPosition(MemberDescriptor member)
        {
            bool flag = false;
            DesignerSerializationVisibilityAttribute attribute = member.Attributes[typeof(DesignerSerializationVisibilityAttribute)] as DesignerSerializationVisibilityAttribute;
            if (attribute != null)
            {
                flag = (attribute.Visibility == DesignerSerializationVisibility.Hidden) && this.ExtenderProperties.ContainsKey(member.Name);
            }
            return flag;
        }

        protected override void DrawBorder(Graphics graphics)
        {
            if (this.Table.CellBorderStyle == TableLayoutPanelCellBorderStyle.None)
            {
                base.DrawBorder(graphics);
                Rectangle displayRectangle = this.Control.DisplayRectangle;
                displayRectangle.Width--;
                displayRectangle.Height--;
                int[] columnWidths = this.Table.GetColumnWidths();
                int[] rowHeights = this.Table.GetRowHeights();
                using (Pen pen = base.BorderPen)
                {
                    if (columnWidths.Length > 1)
                    {
                        bool flag = this.Table.RightToLeft == RightToLeft.Yes;
                        int num = flag ? displayRectangle.Right : displayRectangle.Left;
                        for (int i = 0; i < (columnWidths.Length - 1); i++)
                        {
                            if (flag)
                            {
                                num -= columnWidths[i];
                            }
                            else
                            {
                                num += columnWidths[i];
                            }
                            graphics.DrawLine(pen, num, displayRectangle.Top, num, displayRectangle.Bottom);
                        }
                    }
                    if (rowHeights.Length > 1)
                    {
                        int top = displayRectangle.Top;
                        for (int j = 0; j < (rowHeights.Length - 1); j++)
                        {
                            top += rowHeights[j];
                            graphics.DrawLine(pen, displayRectangle.Left, top, displayRectangle.Right, top);
                        }
                    }
                }
            }
        }

        private bool EnsureAvailableStyles()
        {
            if ((this.IsLoading || this.Undoing) || (this.ensureSuspendCount > 0))
            {
                return false;
            }
            int[] columnWidths = this.Table.GetColumnWidths();
            int[] rowHeights = this.Table.GetRowHeights();
            this.Table.SuspendLayout();
            try
            {
                if (columnWidths.Length > this.Table.ColumnStyles.Count)
                {
                    int num = columnWidths.Length - this.Table.ColumnStyles.Count;
                    this.PropChanging(this.rowStyleProp);
                    for (int i = 0; i < num; i++)
                    {
                        this.Table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, (float) DesignerUtils.MINIMUMSTYLESIZE));
                    }
                    this.PropChanged(this.rowStyleProp);
                }
                if (rowHeights.Length > this.Table.RowStyles.Count)
                {
                    int num3 = rowHeights.Length - this.Table.RowStyles.Count;
                    this.PropChanging(this.colStyleProp);
                    for (int j = 0; j < num3; j++)
                    {
                        this.Table.RowStyles.Add(new RowStyle(SizeType.Absolute, (float) DesignerUtils.MINIMUMSTYLESIZE));
                    }
                    this.PropChanged(this.colStyleProp);
                }
            }
            finally
            {
                this.Table.ResumeLayout();
            }
            return true;
        }

        private Control ExtractControlFromDragEvent(DragEventArgs de)
        {
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                this.dragComps = new ArrayList(data.DragComponents);
                return (this.dragComps[0] as Control);
            }
            return null;
        }

        internal void FixUpControlsOnDelete(bool isRow, int index, ArrayList deleteList)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(this.Table)["Controls"];
            this.PropChanging(prop);
            foreach (Control control in this.Table.Controls)
            {
                int num = isRow ? this.Table.GetRow(control) : this.Table.GetColumn(control);
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control)[isRow ? "Row" : "Column"];
                PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(control)[isRow ? "RowSpan" : "ColumnSpan"];
                if (num == index)
                {
                    if (!deleteList.Contains(control))
                    {
                        deleteList.Add(control);
                    }
                }
                else if ((num != -1) && !deleteList.Contains(control))
                {
                    if (num > index)
                    {
                        if (descriptor2 != null)
                        {
                            descriptor2.SetValue(control, num - 1);
                        }
                    }
                    else
                    {
                        int num2 = isRow ? this.Table.GetRowSpan(control) : this.Table.GetColumnSpan(control);
                        if (((num + num2) > index) && (descriptor3 != null))
                        {
                            descriptor3.SetValue(control, num2 - 1);
                        }
                    }
                }
            }
            this.PropChanged(prop);
        }

        internal void FixUpControlsOnInsert(bool isRow, int index)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(this.Table)["Controls"];
            this.PropChanging(prop);
            foreach (Control control in this.Table.Controls)
            {
                int num = isRow ? this.Table.GetRow(control) : this.Table.GetColumn(control);
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control)[isRow ? "Row" : "Column"];
                PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(control)[isRow ? "RowSpan" : "ColumnSpan"];
                if (num != -1)
                {
                    if (num >= index)
                    {
                        if (descriptor2 != null)
                        {
                            descriptor2.SetValue(control, num + 1);
                        }
                    }
                    else
                    {
                        int num2 = isRow ? this.Table.GetRowSpan(control) : this.Table.GetColumnSpan(control);
                        if (((num + num2) > index) && (descriptor3 != null))
                        {
                            descriptor3.SetValue(control, num2 + 1);
                        }
                    }
                }
            }
            this.PropChanged(prop);
        }

        private Point GetCellPosition(Point pos)
        {
            int[] rowHeights = this.Table.GetRowHeights();
            int[] columnWidths = this.Table.GetColumnWidths();
            Point location = this.Table.PointToScreen(this.Table.DisplayRectangle.Location);
            Rectangle rectangle = new Rectangle(location, this.Table.DisplayRectangle.Size);
            Point point2 = new Point(-1, -1);
            bool flag = this.Table.RightToLeft == RightToLeft.Yes;
            int x = rectangle.X;
            if (flag)
            {
                if (pos.X <= rectangle.X)
                {
                    point2.X = columnWidths.Length;
                }
                else if (pos.X < rectangle.Right)
                {
                    x = rectangle.Right;
                    for (int i = 0; i < columnWidths.Length; i++)
                    {
                        point2.X = i;
                        if (pos.X >= (x - columnWidths[i]))
                        {
                            break;
                        }
                        x -= columnWidths[i];
                    }
                }
            }
            else if (pos.X >= rectangle.Right)
            {
                point2.X = columnWidths.Length;
            }
            else if (pos.X > rectangle.X)
            {
                for (int j = 0; j < columnWidths.Length; j++)
                {
                    point2.X = j;
                    if (pos.X <= (x + columnWidths[j]))
                    {
                        break;
                    }
                    x += columnWidths[j];
                }
            }
            x = rectangle.Y;
            if (pos.Y >= rectangle.Bottom)
            {
                point2.Y = rowHeights.Length;
                return point2;
            }
            if (pos.Y > rectangle.Y)
            {
                for (int k = 0; k < rowHeights.Length; k++)
                {
                    if (pos.Y <= (x + rowHeights[k]))
                    {
                        point2.Y = k;
                        return point2;
                    }
                    x += rowHeights[k];
                }
            }
            return point2;
        }

        private Point GetControlPosition(Control control)
        {
            TableLayoutPanelCellPosition positionFromControl = this.Table.GetPositionFromControl(control);
            if ((positionFromControl.Row == -1) && (positionFromControl.Column == -1))
            {
                return ControlDesigner.InvalidPoint;
            }
            return new Point(positionFromControl.Column, positionFromControl.Row);
        }

        public override GlyphCollection GetGlyphs(GlyphSelectionType selectionType)
        {
            GlyphCollection glyphs = base.GetGlyphs(selectionType);
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Locked"];
            bool flag = (descriptor != null) ? ((bool) descriptor.GetValue(base.Component)) : false;
            bool flag2 = this.EnsureAvailableStyles();
            if (((selectionType != GlyphSelectionType.NotSelected) && !flag) && (this.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
            {
                Point location = base.BehaviorService.MapAdornerWindowPoint(this.Table.Handle, this.Table.DisplayRectangle.Location);
                Rectangle rectangle = new Rectangle(location, this.Table.DisplayRectangle.Size);
                Point point2 = base.BehaviorService.ControlToAdornerWindow(this.Control);
                Rectangle rectangle2 = new Rectangle(point2, this.Control.ClientSize);
                int[] columnWidths = this.Table.GetColumnWidths();
                int[] rowHeights = this.Table.GetRowHeights();
                int num = DesignerUtils.RESIZEGLYPHSIZE / 2;
                bool flag3 = this.Table.RightToLeft == RightToLeft.Yes;
                int y = flag3 ? rectangle.Right : rectangle.X;
                if (!flag2)
                {
                    return glyphs;
                }
                for (int i = 0; i < (columnWidths.Length - 1); i++)
                {
                    if (columnWidths[i] != 0)
                    {
                        if (flag3)
                        {
                            y -= columnWidths[i];
                        }
                        else
                        {
                            y += columnWidths[i];
                        }
                        Rectangle rect = new Rectangle(y - num, rectangle2.Top, DesignerUtils.RESIZEGLYPHSIZE, rectangle2.Height);
                        if (rectangle2.Contains(rect) && (this.Table.ColumnStyles[i] != null))
                        {
                            TableLayoutPanelResizeGlyph glyph = new TableLayoutPanelResizeGlyph(rect, this.Table.ColumnStyles[i], Cursors.VSplit, this.Behavior);
                            glyphs.Add(glyph);
                        }
                    }
                }
                y = rectangle.Y;
                for (int j = 0; j < (rowHeights.Length - 1); j++)
                {
                    if (rowHeights[j] != 0)
                    {
                        y += rowHeights[j];
                        Rectangle rectangle4 = new Rectangle(rectangle2.Left, y - num, rectangle2.Width, DesignerUtils.RESIZEGLYPHSIZE);
                        if (rectangle2.Contains(rectangle4) && (this.Table.RowStyles[j] != null))
                        {
                            TableLayoutPanelResizeGlyph glyph2 = new TableLayoutPanelResizeGlyph(rectangle4, this.Table.RowStyles[j], Cursors.HSplit, this.Behavior);
                            glyphs.Add(glyph2);
                        }
                    }
                }
            }
            return glyphs;
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                service.TransactionClosing += new DesignerTransactionCloseEventHandler(this.OnTransactionClosing);
                this.compSvc = service.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            }
            if (this.compSvc != null)
            {
                this.compSvc.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
                this.compSvc.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            this.Control.ControlAdded += new ControlEventHandler(this.OnControlAdded);
            this.Control.ControlRemoved += new ControlEventHandler(this.OnControlRemoved);
            this.rowStyleProp = TypeDescriptor.GetProperties(this.Table)["RowStyles"];
            this.colStyleProp = TypeDescriptor.GetProperties(this.Table)["ColumnStyles"];
            if (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                for (int i = 0; i < this.Control.Controls.Count; i++)
                {
                    TypeDescriptor.AddAttributes(this.Control.Controls[i], new Attribute[] { System.ComponentModel.InheritanceAttribute.InheritedReadOnly });
                }
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            this.CreateEmptyTable();
        }

        private void InitializeNewStyles()
        {
            this.Table.ColumnStyles[0].SizeType = SizeType.Percent;
            this.Table.ColumnStyles[0].Width = DesignerUtils.MINIMUMSTYLEPERCENT;
            this.Table.ColumnStyles[1].SizeType = SizeType.Percent;
            this.Table.ColumnStyles[1].Width = DesignerUtils.MINIMUMSTYLEPERCENT;
            this.Table.RowStyles[0].SizeType = SizeType.Percent;
            this.Table.RowStyles[0].Height = DesignerUtils.MINIMUMSTYLEPERCENT;
            this.Table.RowStyles[1].SizeType = SizeType.Percent;
            this.Table.RowStyles[1].Height = DesignerUtils.MINIMUMSTYLEPERCENT;
        }

        internal void InsertRowCol(bool isRow, int index)
        {
            try
            {
                if (isRow)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.Table)["RowCount"];
                    if (descriptor != null)
                    {
                        this.PropChanging(this.rowStyleProp);
                        this.Table.RowStyles.Insert(index, new RowStyle(SizeType.Absolute, (float) DesignerUtils.MINIMUMSTYLESIZE));
                        this.PropChanged(this.rowStyleProp);
                        descriptor.SetValue(this.Table, this.Table.RowCount + 1);
                    }
                }
                else
                {
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.Table)["ColumnCount"];
                    if (descriptor2 != null)
                    {
                        this.PropChanging(this.colStyleProp);
                        this.Table.ColumnStyles.Insert(index, new ColumnStyle(SizeType.Absolute, (float) DesignerUtils.MINIMUMSTYLESIZE));
                        this.PropChanged(this.colStyleProp);
                        descriptor2.SetValue(this.Table, this.Table.ColumnCount + 1);
                    }
                }
            }
            catch (InvalidOperationException exception)
            {
                ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception.Message);
            }
            base.BehaviorService.Invalidate(base.BehaviorService.ControlRectInAdornerWindow(this.Control));
        }

        private bool IsLocalizable()
        {
            IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(service.RootComponent)["Localizable"];
                if ((descriptor != null) && (descriptor.PropertyType == typeof(bool)))
                {
                    return (bool) descriptor.GetValue(service.RootComponent);
                }
            }
            return false;
        }

        private bool IsOverValidCell(bool dragOp)
        {
            Point cellPosition = this.GetCellPosition(Control.MousePosition);
            int[] rowHeights = this.Table.GetRowHeights();
            int[] columnWidths = this.Table.GetColumnWidths();
            if (((cellPosition.Y < 0) || (cellPosition.Y >= rowHeights.Length)) || ((cellPosition.X < 0) || (cellPosition.X >= columnWidths.Length)))
            {
                return false;
            }
            if (dragOp)
            {
                Control controlFromPosition = ((TableLayoutPanel) this.Control).GetControlFromPosition(cellPosition.X, cellPosition.Y);
                if ((((controlFromPosition != null) && (this.localDragControl == null)) || ((this.localDragControl != null) && (this.dragComps.Count > 1))) || (((this.localDragControl != null) && (controlFromPosition != null)) && (Control.ModifierKeys == Keys.Control)))
                {
                    return false;
                }
            }
            return true;
        }

        private void OnAbsoluteClick(object sender, EventArgs e)
        {
            this.ChangeSizeType((bool) ((ToolStripMenuItem) sender).Tag, SizeType.Absolute);
        }

        private void OnAdd(bool isRow)
        {
            IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service != null) && (this.Table.Site != null))
            {
                DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString(isRow ? "TableLayoutPanelDesignerAddRowUndoUnit" : "TableLayoutPanelDesignerAddColumnUndoUnit", new object[] { this.Table.Site.Name }));
                try
                {
                    this.Table.SuspendLayout();
                    this.InsertRowCol(isRow, isRow ? this.Table.RowCount : this.Table.ColumnCount);
                    this.Table.ResumeLayout();
                    transaction.Commit();
                }
                catch (CheckoutException exception)
                {
                    if (!CheckoutException.Canceled.Equals(exception))
                    {
                        throw;
                    }
                    if (transaction != null)
                    {
                        transaction.Cancel();
                    }
                }
                finally
                {
                    if (transaction != null)
                    {
                        ((IDisposable) transaction).Dispose();
                    }
                }
            }
        }

        private void OnAddClick(object sender, EventArgs e)
        {
            this.OnAdd((bool) ((ToolStripMenuItem) sender).Tag);
        }

        private void OnAutoSizeClick(object sender, EventArgs e)
        {
            this.ChangeSizeType((bool) ((ToolStripMenuItem) sender).Tag, SizeType.AutoSize);
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.Component != null)
            {
                Control component = e.Component as Control;
                if ((((component != null) && (component.Parent != null)) && (component.Parent.Equals(this.Control) && (e.Member != null))) && ((e.Member.Name == "Row") || (e.Member.Name == "Column")))
                {
                    this.EnsureAvailableStyles();
                }
                if (((component != null) && (component.Parent == base.Component)) && ((e.Member != null) && this.DoesPropertyAffectPosition(e.Member)))
                {
                    PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
                    this.compSvc.OnComponentChanged(base.Component, member, null, null);
                }
            }
            this.CheckVerbStatus();
        }

        private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            Control component = e.Component as Control;
            if (((component != null) && (component.Parent == base.Component)) && ((e.Member != null) && this.DoesPropertyAffectPosition(e.Member)))
            {
                PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Controls"];
                this.compSvc.OnComponentChanging(base.Component, member);
            }
        }

        protected override void OnContextMenu(int x, int y)
        {
            Point cellPosition = this.GetCellPosition(new Point(x, y));
            this.curRow = cellPosition.Y;
            this.curCol = cellPosition.X;
            this.EnsureAvailableStyles();
            this.DesignerContextMenuStrip.Show(x, y);
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            if (!this.IsLoading && !this.Undoing)
            {
                int num = 0;
                int[] rowHeights = this.Table.GetRowHeights();
                int[] columnWidths = this.Table.GetColumnWidths();
                for (int i = 0; i < rowHeights.Length; i++)
                {
                    for (int j = 0; j < columnWidths.Length; j++)
                    {
                        if (this.Table.GetControlFromPosition(j, i) != null)
                        {
                            num++;
                        }
                    }
                }
                bool fullTable = (num - 1) >= (Math.Max(1, this.colCountBeforeAdd) * Math.Max(1, this.rowCountBeforeAdd));
                if (this.droppedCellPosition == ControlDesigner.InvalidPoint)
                {
                    this.droppedCellPosition = this.GetControlPosition(e.Control);
                }
                this.ControlAddedInternal(e.Control, this.droppedCellPosition, false, fullTable, null);
                this.droppedCellPosition = ControlDesigner.InvalidPoint;
            }
        }

        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            if ((e != null) && (e.Control != null))
            {
                this.Table.SetCellPosition(e.Control, new TableLayoutPanelCellPosition(-1, -1));
            }
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            try
            {
                bool tag = (bool) ((ToolStripMenuItem) sender).Tag;
                this.OnRemoveInternal(tag, tag ? this.curRow : this.curCol);
            }
            catch (InvalidOperationException exception)
            {
                ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception.Message);
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            this.droppedCellPosition = this.GetCellPosition(Control.MousePosition);
            if (this.localDragControl != null)
            {
                this.ControlAddedInternal(this.localDragControl, this.droppedCellPosition, true, false, de);
                this.localDragControl = null;
            }
            else
            {
                this.rowCountBeforeAdd = Math.Max(0, this.Table.GetRowHeights().Length);
                this.colCountBeforeAdd = Math.Max(0, this.Table.GetColumnWidths().Length);
                base.OnDragDrop(de);
                if (this.dragComps != null)
                {
                    foreach (Control control in this.dragComps)
                    {
                        if (control != null)
                        {
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control)["ColumnSpan"];
                            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control)["RowSpan"];
                            if (descriptor != null)
                            {
                                descriptor.SetValue(control, 1);
                            }
                            if (descriptor2 != null)
                            {
                                descriptor2.SetValue(control, 1);
                            }
                        }
                    }
                }
            }
            this.droppedCellPosition = ControlDesigner.InvalidPoint;
            this.dragComps = null;
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            base.OnDragEnter(de);
            if (this.localDragControl == null)
            {
                Control control = this.ExtractControlFromDragEvent(de);
                if ((control != null) && this.Table.Controls.Contains(control))
                {
                    this.localDragControl = control;
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            this.localDragControl = null;
            this.dragComps = null;
            base.OnDragLeave(e);
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            if (!this.IsOverValidCell(true))
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                base.OnDragOver(de);
            }
        }

        private void OnEdit()
        {
            try
            {
                EditorServiceContext.EditValue(this, this.Table, "ColumnStyles");
            }
            catch (InvalidOperationException exception)
            {
                ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception.Message);
            }
        }

        private void OnInsertClick(object sender, EventArgs e)
        {
            IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service != null) && (this.Table.Site != null))
            {
                bool tag = (bool) ((ToolStripMenuItem) sender).Tag;
                DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString(tag ? "TableLayoutPanelDesignerAddRowUndoUnit" : "TableLayoutPanelDesignerAddColumnUndoUnit", new object[] { this.Table.Site.Name }));
                try
                {
                    this.Table.SuspendLayout();
                    this.InsertRowCol(tag, tag ? this.curRow : this.curCol);
                    this.FixUpControlsOnInsert(tag, tag ? this.curRow : this.curCol);
                    this.Table.ResumeLayout();
                    transaction.Commit();
                }
                catch (CheckoutException exception)
                {
                    if (!CheckoutException.Canceled.Equals(exception))
                    {
                        throw;
                    }
                    if (transaction != null)
                    {
                        transaction.Cancel();
                    }
                }
                catch (InvalidOperationException exception2)
                {
                    ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception2.Message);
                }
                finally
                {
                    if (transaction != null)
                    {
                        ((IDisposable) transaction).Dispose();
                    }
                }
            }
        }

        protected override void OnMouseDragBegin(int x, int y)
        {
            if (this.IsOverValidCell(true))
            {
                IToolboxService service = (IToolboxService) this.GetService(typeof(IToolboxService));
                if ((service != null) && (service.GetSelectedToolboxItem((IDesignerHost) this.GetService(typeof(IDesignerHost))) != null))
                {
                    this.droppedCellPosition = this.GetCellPosition(Control.MousePosition);
                }
            }
            else
            {
                this.droppedCellPosition = ControlDesigner.InvalidPoint;
                Cursor.Current = Cursors.No;
            }
            base.OnMouseDragBegin(x, y);
        }

        protected override void OnMouseDragEnd(bool cancel)
        {
            if (this.droppedCellPosition == ControlDesigner.InvalidPoint)
            {
                cancel = true;
            }
            base.OnMouseDragEnd(cancel);
        }

        protected override void OnMouseDragMove(int x, int y)
        {
            if (this.droppedCellPosition == ControlDesigner.InvalidPoint)
            {
                Cursor.Current = Cursors.No;
            }
            else
            {
                base.OnMouseDragMove(x, y);
            }
        }

        private void OnPercentClick(object sender, EventArgs e)
        {
            this.ChangeSizeType((bool) ((ToolStripMenuItem) sender).Tag, SizeType.Percent);
        }

        private void OnRemove(bool isRow)
        {
            this.OnRemoveInternal(isRow, isRow ? (this.Table.RowCount - 1) : (this.Table.ColumnCount - 1));
        }

        private void OnRemoveInternal(bool isRow, int index)
        {
            if ((isRow ? this.Table.RowCount : this.Table.ColumnCount) >= 2)
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service != null) && (this.Table.Site != null))
                {
                    DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString(isRow ? "TableLayoutPanelDesignerRemoveRowUndoUnit" : "TableLayoutPanelDesignerRemoveColumnUndoUnit", new object[] { this.Table.Site.Name }));
                    try
                    {
                        this.Table.SuspendLayout();
                        ArrayList deleteList = new ArrayList();
                        this.FixUpControlsOnDelete(isRow, index, deleteList);
                        this.DeleteRowCol(isRow, index);
                        if (deleteList.Count > 0)
                        {
                            PropertyDescriptor prop = TypeDescriptor.GetProperties(this.Table)["Controls"];
                            this.PropChanging(prop);
                            foreach (object obj2 in deleteList)
                            {
                                ArrayList list = new ArrayList();
                                DesignerUtils.GetAssociatedComponents((IComponent) obj2, service, list);
                                foreach (IComponent component in list)
                                {
                                    this.compSvc.OnComponentChanging(component, null);
                                }
                                service.DestroyComponent(obj2 as Component);
                            }
                            this.PropChanged(prop);
                        }
                        this.Table.ResumeLayout();
                        transaction.Commit();
                    }
                    catch (CheckoutException exception)
                    {
                        if (!CheckoutException.Canceled.Equals(exception))
                        {
                            throw;
                        }
                        if (transaction != null)
                        {
                            transaction.Cancel();
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            ((IDisposable) transaction).Dispose();
                        }
                    }
                }
            }
        }

        private void OnRowColMenuOpening(object sender, CancelEventArgs e)
        {
            e.Cancel = false;
            ToolStripDropDownMenu menu = sender as ToolStripDropDownMenu;
            if (menu != null)
            {
                int selectionCount = 0;
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    selectionCount = service.SelectionCount;
                }
                bool flag = (selectionCount == 1) && (this.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly);
                menu.Items["add"].Enabled = flag;
                menu.Items["insert"].Enabled = flag;
                menu.Items["delete"].Enabled = flag;
                menu.Items["sizemode"].Enabled = flag;
                menu.Items["absolute"].Enabled = flag;
                menu.Items["percent"].Enabled = flag;
                menu.Items["autosize"].Enabled = flag;
                if (selectionCount == 1)
                {
                    ((ToolStripMenuItem) menu.Items["absolute"]).Checked = false;
                    ((ToolStripMenuItem) menu.Items["percent"]).Checked = false;
                    ((ToolStripMenuItem) menu.Items["autosize"]).Checked = false;
                    bool tag = (bool) menu.Tag;
                    switch ((tag ? this.Table.RowStyles[this.curRow].SizeType : this.Table.ColumnStyles[this.curCol].SizeType))
                    {
                        case SizeType.AutoSize:
                            ((ToolStripMenuItem) menu.Items["autosize"]).Checked = true;
                            break;

                        case SizeType.Absolute:
                            ((ToolStripMenuItem) menu.Items["absolute"]).Checked = true;
                            break;

                        case SizeType.Percent:
                            ((ToolStripMenuItem) menu.Items["percent"]).Checked = true;
                            break;
                    }
                    if ((tag ? this.Table.RowCount : this.Table.ColumnCount) < 2)
                    {
                        menu.Items["delete"].Enabled = false;
                    }
                }
            }
        }

        private void OnTransactionClosing(object sender, DesignerTransactionCloseEventArgs e)
        {
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if ((service != null) && (this.Table != null))
            {
                ICollection selectedComponents = service.GetSelectedComponents();
                bool flag = false;
                foreach (object obj2 in selectedComponents)
                {
                    Control control = obj2 as Control;
                    if ((control != null) && (control.Parent == this.Table))
                    {
                        flag = true;
                        break;
                    }
                }
                if (service.GetComponentSelected(this.Table) || flag)
                {
                    this.Table.SuspendLayout();
                    this.EnsureAvailableStyles();
                    this.Table.ResumeLayout(false);
                    this.Table.PerformLayout();
                }
            }
        }

        private void OnUndoing(object sender, EventArgs e)
        {
            if (!this.Undoing)
            {
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undone += new EventHandler(this.OnUndone);
                }
                this.Undoing = true;
            }
        }

        private void OnUndone(object sender, EventArgs e)
        {
            if (this.Undoing)
            {
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                }
                this.Undoing = false;
                if (this.EnsureAvailableStyles())
                {
                    this.Refresh();
                }
            }
        }

        private void OnVerbAdd(object sender, EventArgs e)
        {
            bool isRow = ((DesignerVerb) sender).Text.Equals(System.Design.SR.GetString("TableLayoutPanelDesignerAddRow"));
            this.OnAdd(isRow);
        }

        private void OnVerbEdit(object sender, EventArgs e)
        {
            this.OnEdit();
        }

        private void OnVerbRemove(object sender, EventArgs e)
        {
            bool isRow = ((DesignerVerb) sender).Text.Equals(System.Design.SR.GetString("TableLayoutPanelDesignerRemoveRow"));
            this.OnRemove(isRow);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "ColumnStyles", "RowStyles", "ColumnCount", "RowCount" };
            Attribute[] attributes = new Attribute[] { new BrowsableAttribute(true) };
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(TableLayoutPanelDesigner), oldPropertyDescriptor, attributes);
                }
            }
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["Controls"];
            if (descriptor2 != null)
            {
                Attribute[] array = new Attribute[descriptor2.Attributes.Count];
                descriptor2.Attributes.CopyTo(array, 0);
                properties["Controls"] = TypeDescriptor.CreateProperty(typeof(TableLayoutPanelDesigner), "Controls", typeof(DesignerTableLayoutControlCollection), array);
            }
        }

        private void PropChanged(PropertyDescriptor prop)
        {
            if ((this.compSvc != null) && (prop != null))
            {
                this.compSvc.OnComponentChanged(this.Table, prop, null, null);
            }
        }

        private void PropChanging(PropertyDescriptor prop)
        {
            if ((this.compSvc != null) && (prop != null))
            {
                this.compSvc.OnComponentChanging(this.Table, prop);
            }
        }

        private void Refresh()
        {
            base.BehaviorService.SyncSelection();
            if (this.Table != null)
            {
                this.Table.Invalidate(true);
            }
        }

        private void RefreshSmartTag()
        {
            DesignerActionUIService service = (DesignerActionUIService) this.GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.Refresh(base.Component);
            }
        }

        private void RemoveControlInternal(Control c)
        {
            this.Table.ControlRemoved -= new ControlEventHandler(this.OnControlRemoved);
            this.Table.Controls.Remove(c);
            this.Table.ControlRemoved += new ControlEventHandler(this.OnControlRemoved);
        }

        internal void ResumeEnsureAvailableStyles(bool performEnsure)
        {
            if (this.ensureSuspendCount > 0)
            {
                this.ensureSuspendCount--;
                if ((this.ensureSuspendCount == 0) && performEnsure)
                {
                    this.EnsureAvailableStyles();
                }
            }
        }

        private bool ShouldSerializeColumnStyles()
        {
            return !this.IsLocalizable();
        }

        private bool ShouldSerializeRowStyles()
        {
            return !this.IsLocalizable();
        }

        private static bool SubsetExists(bool[,] cells, int columns, int rows, int subsetColumns, int subsetRows)
        {
            bool flag = false;
            int num = 0;
            for (int i = 0; i < ((rows - subsetRows) + 1); i++)
            {
                for (num = 0; num < ((columns - subsetColumns) + 1); num++)
                {
                    if (!cells[num, i])
                    {
                        flag = true;
                        for (int j = i; (j < (i + subsetRows)) && flag; j++)
                        {
                            for (int k = num; k < (num + subsetColumns); k++)
                            {
                                if (cells[k, j])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                if (flag)
                {
                    return flag;
                }
            }
            return flag;
        }

        internal void SuspendEnsureAvailableStyles()
        {
            this.ensureSuspendCount++;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this.actionLists == null)
                {
                    this.BuildActionLists();
                }
                return this.actionLists;
            }
        }

        private TableLayoutPanelBehavior Behavior
        {
            get
            {
                if (this.tlpBehavior == null)
                {
                    this.tlpBehavior = new TableLayoutPanelBehavior(this.Table, this, base.Component.Site);
                }
                return this.tlpBehavior;
            }
        }

        public int ColumnCount
        {
            get
            {
                return this.Table.ColumnCount;
            }
            set
            {
                if ((value <= 0) && !this.Undoing)
                {
                    throw new ArgumentException(System.Design.SR.GetString("TableLayoutPanelDesignerInvalidColumnRowCount", new object[] { "ColumnCount" }));
                }
                this.Table.ColumnCount = value;
            }
        }

        private TableLayoutColumnStyleCollection ColumnStyles
        {
            get
            {
                return this.Table.ColumnStyles;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        private DesignerTableLayoutControlCollection Controls
        {
            get
            {
                if (this.controls == null)
                {
                    this.controls = new DesignerTableLayoutControlCollection((TableLayoutPanel) this.Control);
                }
                return this.controls;
            }
        }

        private ContextMenuStrip DesignerContextMenuStrip
        {
            get
            {
                if (this.designerContextMenuStrip == null)
                {
                    this.designerContextMenuStrip = new BaseContextMenuStrip(base.Component.Site, this.Table);
                    ContextMenuStripGroup group = this.designerContextMenuStrip.Groups["Verbs"];
                    foreach (DesignerVerb verb in this.Verbs)
                    {
                        if (!verb.Text.Equals(System.Design.SR.GetString("TableLayoutPanelDesignerEditRowAndCol")))
                        {
                            foreach (ToolStripItem item in group.Items)
                            {
                                if (item.Text.Equals(verb.Text))
                                {
                                    group.Items.Remove(item);
                                    break;
                                }
                            }
                        }
                    }
                    ToolStripDropDownMenu menu = this.BuildMenu(true);
                    ToolStripDropDownMenu menu2 = this.BuildMenu(false);
                    this.contextMenuRow = new ToolStripMenuItem();
                    this.contextMenuRow.DropDown = menu;
                    this.contextMenuRow.Text = System.Design.SR.GetString("TableLayoutPanelDesignerRowMenu");
                    this.contextMenuCol = new ToolStripMenuItem();
                    this.contextMenuCol.DropDown = menu2;
                    this.contextMenuCol.Text = System.Design.SR.GetString("TableLayoutPanelDesignerColMenu");
                    group.Items.Insert(0, this.contextMenuCol);
                    group.Items.Insert(0, this.contextMenuRow);
                    group = this.designerContextMenuStrip.Groups["Edit"];
                    foreach (ToolStripItem item2 in group.Items)
                    {
                        if (item2.Text.Equals(System.Design.SR.GetString("ContextMenuCut")))
                        {
                            item2.Text = System.Design.SR.GetString("TableLayoutPanelDesignerContextMenuCut");
                        }
                        else if (item2.Text.Equals(System.Design.SR.GetString("ContextMenuCopy")))
                        {
                            item2.Text = System.Design.SR.GetString("TableLayoutPanelDesignerContextMenuCopy");
                        }
                        else if (item2.Text.Equals(System.Design.SR.GetString("ContextMenuDelete")))
                        {
                            item2.Text = System.Design.SR.GetString("TableLayoutPanelDesignerContextMenuDelete");
                        }
                    }
                }
                bool flag = this.IsOverValidCell(false);
                this.contextMenuRow.Enabled = flag;
                this.contextMenuCol.Enabled = flag;
                return this.designerContextMenuStrip;
            }
        }

        private Dictionary<string, bool> ExtenderProperties
        {
            get
            {
                if ((this.extenderProperties == null) && (base.Component != null))
                {
                    this.extenderProperties = new Dictionary<string, bool>();
                    foreach (Attribute attribute in TypeDescriptor.GetAttributes(base.Component.GetType()))
                    {
                        ProvidePropertyAttribute attribute2 = attribute as ProvidePropertyAttribute;
                        if (attribute2 != null)
                        {
                            this.extenderProperties[attribute2.PropertyName] = true;
                        }
                    }
                }
                return this.extenderProperties;
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if ((base.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.Inherited) && (base.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
                {
                    return base.InheritanceAttribute;
                }
                return System.ComponentModel.InheritanceAttribute.InheritedReadOnly;
            }
        }

        private bool IsLoading
        {
            get
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                return ((service != null) && service.Loading);
            }
        }

        public int RowCount
        {
            get
            {
                return this.Table.RowCount;
            }
            set
            {
                if ((value <= 0) && !this.Undoing)
                {
                    throw new ArgumentException(System.Design.SR.GetString("TableLayoutPanelDesignerInvalidColumnRowCount", new object[] { "RowCount" }));
                }
                this.Table.RowCount = value;
            }
        }

        private TableLayoutRowStyleCollection RowStyles
        {
            get
            {
                return this.Table.RowStyles;
            }
        }

        internal TableLayoutPanel Table
        {
            get
            {
                return (base.Component as TableLayoutPanel);
            }
        }

        private bool Undoing
        {
            get
            {
                if (this.undoEngine == null)
                {
                    this.undoEngine = this.GetService(typeof(UndoEngine)) as UndoEngine;
                    if (this.undoEngine != null)
                    {
                        this.undoEngine.Undoing += new EventHandler(this.OnUndoing);
                        if (this.undoEngine.UndoInProgress)
                        {
                            this.undoing = true;
                            this.undoEngine.Undone += new EventHandler(this.OnUndone);
                        }
                    }
                }
                return this.undoing;
            }
            set
            {
                this.undoing = value;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this.verbs == null)
                {
                    this.removeColVerb = new DesignerVerb(System.Design.SR.GetString("TableLayoutPanelDesignerRemoveColumn"), new EventHandler(this.OnVerbRemove));
                    this.removeRowVerb = new DesignerVerb(System.Design.SR.GetString("TableLayoutPanelDesignerRemoveRow"), new EventHandler(this.OnVerbRemove));
                    this.verbs = new DesignerVerbCollection();
                    this.verbs.Add(new DesignerVerb(System.Design.SR.GetString("TableLayoutPanelDesignerAddColumn"), new EventHandler(this.OnVerbAdd)));
                    this.verbs.Add(new DesignerVerb(System.Design.SR.GetString("TableLayoutPanelDesignerAddRow"), new EventHandler(this.OnVerbAdd)));
                    this.verbs.Add(this.removeColVerb);
                    this.verbs.Add(this.removeRowVerb);
                    this.verbs.Add(new DesignerVerb(System.Design.SR.GetString("TableLayoutPanelDesignerEditRowAndCol"), new EventHandler(this.OnVerbEdit)));
                    this.CheckVerbStatus();
                }
                return this.verbs;
            }
        }

        [ListBindable(false), DesignerSerializer(typeof(TableLayoutPanelDesigner.DesignerTableLayoutControlCollectionCodeDomSerializer), typeof(CodeDomSerializer))]
        internal class DesignerTableLayoutControlCollection : TableLayoutControlCollection, IList, ICollection, IEnumerable
        {
            private TableLayoutControlCollection realCollection;

            public DesignerTableLayoutControlCollection(TableLayoutPanel owner) : base(owner)
            {
                this.realCollection = owner.Controls;
            }

            public override void Add(Control c)
            {
                this.realCollection.Add(c);
            }

            public override void Add(Control control, int column, int row)
            {
                this.realCollection.Add(control, column, row);
            }

            public override void AddRange(Control[] controls)
            {
                this.realCollection.AddRange(controls);
            }

            public override void Clear()
            {
                for (int i = this.realCollection.Count - 1; i >= 0; i--)
                {
                    if (((this.realCollection[i] != null) && (this.realCollection[i].Site != null)) && TypeDescriptor.GetAttributes(this.realCollection[i]).Contains(InheritanceAttribute.NotInherited))
                    {
                        this.realCollection.RemoveAt(i);
                    }
                }
            }

            public void CopyTo(Array dest, int index)
            {
                this.realCollection.CopyTo(dest, index);
            }

            public override bool Equals(object other)
            {
                return this.realCollection.Equals(other);
            }

            public override int GetChildIndex(Control child, bool throwException)
            {
                return this.realCollection.GetChildIndex(child, throwException);
            }

            public IEnumerator GetEnumerator()
            {
                return this.realCollection.GetEnumerator();
            }

            public override int GetHashCode()
            {
                return this.realCollection.GetHashCode();
            }

            public override void SetChildIndex(Control child, int newIndex)
            {
                this.realCollection.SetChildIndex(child, newIndex);
            }

            int IList.Add(object control)
            {
                return ((IList) this.realCollection).Add(control);
            }

            bool IList.Contains(object control)
            {
                return ((IList) this.realCollection).Contains(control);
            }

            int IList.IndexOf(object control)
            {
                return ((IList) this.realCollection).IndexOf(control);
            }

            void IList.Insert(int index, object value)
            {
                ((IList) this.realCollection).Insert(index, value);
            }

            void IList.Remove(object control)
            {
                ((IList) this.realCollection).Remove(control);
            }

            void IList.RemoveAt(int index)
            {
                ((IList) this.realCollection).RemoveAt(index);
            }

            public override int Count
            {
                get
                {
                    return this.realCollection.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return this.realCollection.IsReadOnly;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this.realCollection[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        internal class DesignerTableLayoutControlCollectionCodeDomSerializer : TableLayoutControlCollectionCodeDomSerializer
        {
            protected override object SerializeCollection(IDesignerSerializationManager manager, CodeExpression targetExpression, System.Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
            {
                ArrayList list = new ArrayList();
                if ((valuesToSerialize != null) && (valuesToSerialize.Count > 0))
                {
                    foreach (object obj2 in valuesToSerialize)
                    {
                        IComponent component = obj2 as IComponent;
                        if (((component != null) && (component.Site != null)) && !(component.Site is INestedSite))
                        {
                            list.Add(component);
                        }
                    }
                }
                return base.SerializeCollection(manager, targetExpression, targetType, originalCollection, list);
            }
        }

        private class TableLayouPanelRowColumnActionList : DesignerActionList
        {
            private TableLayoutPanelDesigner owner;

            public TableLayouPanelRowColumnActionList(TableLayoutPanelDesigner owner) : base(owner.Component)
            {
                this.owner = owner;
            }

            public void AddColumn()
            {
                this.owner.OnAdd(false);
            }

            public void AddRow()
            {
                this.owner.OnAdd(true);
            }

            public void EditRowAndCol()
            {
                this.owner.OnEdit();
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionMethodItem(this, "AddColumn", System.Design.SR.GetString("TableLayoutPanelDesignerAddColumn"), false));
                items.Add(new DesignerActionMethodItem(this, "AddRow", System.Design.SR.GetString("TableLayoutPanelDesignerAddRow"), false));
                if (this.owner.Table.ColumnCount > 1)
                {
                    items.Add(new DesignerActionMethodItem(this, "RemoveColumn", System.Design.SR.GetString("TableLayoutPanelDesignerRemoveColumn"), false));
                }
                if (this.owner.Table.RowCount > 1)
                {
                    items.Add(new DesignerActionMethodItem(this, "RemoveRow", System.Design.SR.GetString("TableLayoutPanelDesignerRemoveRow"), false));
                }
                items.Add(new DesignerActionMethodItem(this, "EditRowAndCol", System.Design.SR.GetString("TableLayoutPanelDesignerEditRowAndCol"), false));
                return items;
            }

            public void RemoveColumn()
            {
                this.owner.OnRemove(false);
            }

            public void RemoveRow()
            {
                this.owner.OnRemove(true);
            }
        }
    }
}

