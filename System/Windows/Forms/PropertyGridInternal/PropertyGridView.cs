namespace System.Windows.Forms.PropertyGridInternal
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Internal;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.VisualStyles;

    internal class PropertyGridView : Control, IWin32Window, IWindowsFormsEditorService, IServiceProvider
    {
        private GridEntryCollection allGridEntries;
        private Brush backgroundBrush;
        private IntPtr baseHfont;
        private IntPtr boldHfont;
        private System.Windows.Forms.PropertyGridInternal.DropDownButton btnDialog;
        private System.Windows.Forms.PropertyGridInternal.DropDownButton btnDropDown;
        private int cachedRowHeight = -1;
        private int cumulativeVerticalWheelDelta;
        private Control currentEditor;
        private DropDownHolder dropDownHolder;
        private GridViewEdit edit;
        protected const int EDIT_INDENT = 0;
        private EventHandler ehLabelClick;
        private EventHandler ehLabelDblClick;
        private EventHandler ehOutlineClick;
        private GridEntryRecreateChildrenEventHandler ehRecreateChildren;
        private EventHandler ehValueClick;
        private EventHandler ehValueDblClick;
        protected const short ERROR_MSGBOX_UP = 2;
        protected const short ERROR_NONE = 0;
        protected const short ERROR_THROWN = 1;
        private GridErrorDlg errorDlg;
        private short errorState;
        private const short FlagBtnLaunchedEditor = 0x100;
        private const short FlagDropDownClosing = 0x20;
        private const short FlagDropDownCommit = 0x40;
        private const short FlagInPropertySet = 0x10;
        private const short FlagIsNewSelection = 2;
        private const short FlagIsSpecialKey = 8;
        private const short FlagIsSplitterMove = 4;
        private const short FlagNeedsRefresh = 1;
        private const short FlagNeedUpdateUIBasedOnFont = 0x80;
        private const short FlagNoDefault = 0x200;
        private const short FlagResizableDropDown = 0x400;
        private short flags = 0x83;
        private Font fontBold;
        internal const short GDIPLUS_SPACE = 2;
        public static TraceSwitch GridViewDebugPaint = new TraceSwitch("GridViewDebugPaint", "PropertyGridView: Debug property painting");
        private IHelpService helpService;
        public static int inheritRenderMode = 3;
        protected static readonly Point InvalidPoint = new Point(-2147483648, -2147483648);
        protected static readonly Point InvalidPosition = new Point(-2147483648, -2147483648);
        public double labelRatio = 2.0;
        private int labelWidth = -1;
        private GridEntry lastClickedEntry;
        private Rectangle lastClientRect = Rectangle.Empty;
        private Point lastMouseDown = InvalidPosition;
        private int lastMouseMove;
        private const int LEFTDOT_SIZE = 4;
        private GridViewListBox listBox;
        protected const int MAX_LISTBOX_HEIGHT = 200;
        internal const int MaxRecurseExpand = 10;
        private string originalTextValue;
        protected const int OUTLINE_INDENT = 10;
        protected const int OUTLINE_SIZE = 9;
        protected const int OUTLINE_SIZE_EXPLORER_TREE_STYLE = 0x10;
        private PropertyGrid ownerGrid;
        protected const int PAINT_INDENT = 0x1a;
        protected const int PAINT_WIDTH = 20;
        private GridPositionData positionData;
        private Point ptOurLocation = new Point(1, 1);
        public const int RENDERMODE_BOLD = 3;
        public const int RENDERMODE_LEFTDOT = 2;
        public const int RENDERMODE_TRIANGLE = 4;
        private short requiredLabelPaintMargin = 2;
        protected const int ROWLABEL = 1;
        private Point rowSelectPos = Point.Empty;
        private long rowSelectTime;
        protected const int ROWVALUE = 2;
        private System.Windows.Forms.ScrollBar scrollBar;
        private GridEntry selectedGridEntry;
        private int selectedRow = -1;
        private IServiceProvider serviceProvider;
        private int tipInfo = -1;
        internal GridToolTip toolTip;
        private IHelpService topHelpService;
        private GridEntryCollection topLevelGridEntries;
        internal int totalProps = -1;
        private int visibleRows = -1;

        public PropertyGridView(IServiceProvider serviceProvider, PropertyGrid propertyGrid)
        {
            this.ehValueClick = new EventHandler(this.OnGridEntryValueClick);
            this.ehLabelClick = new EventHandler(this.OnGridEntryLabelClick);
            this.ehOutlineClick = new EventHandler(this.OnGridEntryOutlineClick);
            this.ehValueDblClick = new EventHandler(this.OnGridEntryValueDoubleClick);
            this.ehLabelDblClick = new EventHandler(this.OnGridEntryLabelDoubleClick);
            this.ehRecreateChildren = new GridEntryRecreateChildrenEventHandler(this.OnRecreateChildren);
            this.ownerGrid = propertyGrid;
            this.serviceProvider = serviceProvider;
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            base.SetStyle(ControlStyles.ResizeRedraw, false);
            base.SetStyle(ControlStyles.UserMouse, true);
            this.BackColor = SystemColors.Window;
            this.ForeColor = SystemColors.WindowText;
            this.backgroundBrush = SystemBrushes.Window;
            base.TabStop = true;
            this.Text = "PropertyGridView";
            this.CreateUI();
            this.LayoutWindow(true);
        }

        internal virtual bool _Commit()
        {
            return this.Commit();
        }

        internal GridEntryCollection AccessibilityGetGridEntries()
        {
            return this.GetAllGridEntries();
        }

        internal Rectangle AccessibilityGetGridEntryBounds(GridEntry gridEntry)
        {
            int rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
            if (rowFromGridEntry == -1)
            {
                return new Rectangle(0, 0, 0, 0);
            }
            Rectangle rectangle = this.GetRectangle(rowFromGridEntry, 3);
            System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(rectangle.X, rectangle.Y);
            System.Windows.Forms.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, base.Handle), pt);
            return new Rectangle(pt.x, pt.y, rectangle.Width, rectangle.Height);
        }

        internal int AccessibilityGetGridEntryChildID(GridEntry gridEntry)
        {
            GridEntryCollection allGridEntries = this.GetAllGridEntries();
            if (allGridEntries != null)
            {
                for (int i = 0; i < allGridEntries.Count; i++)
                {
                    if (allGridEntries[i].Equals(gridEntry))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        internal void AccessibilitySelect(GridEntry entry)
        {
            this.SelectGridEntry(entry, true);
            this.FocusInternal();
        }

        private void AddGridEntryEvents(GridEntryCollection ipeArray, int startIndex, int count)
        {
            if (ipeArray != null)
            {
                if (count == -1)
                {
                    count = ipeArray.Count - startIndex;
                }
                for (int i = startIndex; i < (startIndex + count); i++)
                {
                    if (ipeArray[i] != null)
                    {
                        GridEntry entry = ipeArray.GetEntry(i);
                        entry.AddOnValueClick(this.ehValueClick);
                        entry.AddOnLabelClick(this.ehLabelClick);
                        entry.AddOnOutlineClick(this.ehOutlineClick);
                        entry.AddOnOutlineDoubleClick(this.ehOutlineClick);
                        entry.AddOnValueDoubleClick(this.ehValueDblClick);
                        entry.AddOnLabelDoubleClick(this.ehLabelDblClick);
                        entry.AddOnRecreateChildren(this.ehRecreateChildren);
                    }
                }
            }
        }

        protected virtual void AdjustOrigin(Graphics g, Point newOrigin, ref Rectangle r)
        {
            g.ResetTransform();
            g.TranslateTransform((float) newOrigin.X, (float) newOrigin.Y);
            r.Offset(-newOrigin.X, -newOrigin.Y);
        }

        private void CancelSplitterMove()
        {
            if (this.GetFlag(4))
            {
                this.SetFlag(4, false);
                base.CaptureInternal = false;
                if (this.selectedRow != -1)
                {
                    this.SelectRow(this.selectedRow);
                }
            }
        }

        internal GridPositionData CaptureGridPositionData()
        {
            return new GridPositionData(this);
        }

        private void ClearCachedFontInfo()
        {
            if (this.baseHfont != IntPtr.Zero)
            {
                System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(this, this.baseHfont));
                this.baseHfont = IntPtr.Zero;
            }
            if (this.boldHfont != IntPtr.Zero)
            {
                System.Windows.Forms.SafeNativeMethods.ExternalDeleteObject(new HandleRef(this, this.boldHfont));
                this.boldHfont = IntPtr.Zero;
            }
        }

        private void ClearGridEntryEvents(GridEntryCollection ipeArray, int startIndex, int count)
        {
            if (ipeArray != null)
            {
                if (count == -1)
                {
                    count = ipeArray.Count - startIndex;
                }
                for (int i = startIndex; i < (startIndex + count); i++)
                {
                    if (ipeArray[i] != null)
                    {
                        GridEntry entry = ipeArray.GetEntry(i);
                        entry.RemoveOnValueClick(this.ehValueClick);
                        entry.RemoveOnLabelClick(this.ehLabelClick);
                        entry.RemoveOnOutlineClick(this.ehOutlineClick);
                        entry.RemoveOnOutlineDoubleClick(this.ehOutlineClick);
                        entry.RemoveOnValueDoubleClick(this.ehValueDblClick);
                        entry.RemoveOnLabelDoubleClick(this.ehLabelDblClick);
                        entry.RemoveOnRecreateChildren(this.ehRecreateChildren);
                    }
                }
            }
        }

        public void ClearProps()
        {
            if (this.HasEntries)
            {
                this.CommonEditorHide();
                this.topLevelGridEntries = null;
                this.ClearGridEntryEvents(this.allGridEntries, 0, -1);
                this.allGridEntries = null;
                this.selectedRow = -1;
                this.tipInfo = -1;
            }
        }

        public void CloseDropDown()
        {
            this.CloseDropDownInternal(true);
        }

        private void CloseDropDownInternal(bool resetFocus)
        {
            if (!this.GetFlag(0x20))
            {
                try
                {
                    this.SetFlag(0x20, true);
                    if ((this.dropDownHolder != null) && this.dropDownHolder.Visible)
                    {
                        if ((this.dropDownHolder.Component == this.DropDownListBox) && this.GetFlag(0x40))
                        {
                            this.OnListClick(null, null);
                        }
                        this.Edit.Filter = false;
                        this.dropDownHolder.SetComponent(null, false);
                        this.dropDownHolder.Visible = false;
                        if (resetFocus)
                        {
                            if (this.DialogButton.Visible)
                            {
                                this.DialogButton.FocusInternal();
                            }
                            else if (this.DropDownButton.Visible)
                            {
                                this.DropDownButton.FocusInternal();
                            }
                            else if (this.Edit.Visible)
                            {
                                this.Edit.FocusInternal();
                            }
                            else
                            {
                                this.FocusInternal();
                            }
                            if (this.selectedRow != -1)
                            {
                                this.SelectRow(this.selectedRow);
                            }
                        }
                    }
                }
                finally
                {
                    this.SetFlag(0x20, false);
                }
            }
        }

        private bool Commit()
        {
            if (this.errorState == 2)
            {
                return false;
            }
            if (!this.NeedsCommit)
            {
                this.SetCommitError(0);
                return true;
            }
            if (this.GetInPropertySet())
            {
                return false;
            }
            if (this.GetGridEntryFromRow(this.selectedRow) == null)
            {
                return true;
            }
            bool flag = false;
            try
            {
                flag = this.CommitText(this.Edit.Text);
            }
            finally
            {
                if (!flag)
                {
                    this.Edit.FocusInternal();
                    this.SelectEdit(false);
                }
                else
                {
                    this.SetCommitError(0);
                }
            }
            return flag;
        }

        private bool CommitText(string text)
        {
            object obj2 = null;
            GridEntry selectedGridEntry = this.selectedGridEntry;
            if ((this.selectedGridEntry == null) && (this.selectedRow != -1))
            {
                selectedGridEntry = this.GetGridEntryFromRow(this.selectedRow);
            }
            if (selectedGridEntry == null)
            {
                return true;
            }
            try
            {
                obj2 = selectedGridEntry.ConvertTextToValue(text);
            }
            catch (Exception exception)
            {
                this.SetCommitError(1);
                this.ShowInvalidMessage(selectedGridEntry.PropertyLabel, text, exception);
                return false;
            }
            this.SetCommitError(0);
            return this.CommitValue(obj2);
        }

        private bool CommitValue(object value)
        {
            GridEntry selectedGridEntry = this.selectedGridEntry;
            if ((this.selectedGridEntry == null) && (this.selectedRow != -1))
            {
                selectedGridEntry = this.GetGridEntryFromRow(this.selectedRow);
            }
            return ((selectedGridEntry == null) || this.CommitValue(selectedGridEntry, value));
        }

        internal bool CommitValue(GridEntry ipeCur, object value)
        {
            int childCount = ipeCur.ChildCount;
            bool hookMouseDown = this.Edit.HookMouseDown;
            object oldValue = null;
            try
            {
                oldValue = ipeCur.PropertyValue;
            }
            catch
            {
            }
            try
            {
                this.SetFlag(0x10, true);
                if ((ipeCur != null) && ipeCur.Enumerable)
                {
                    this.CloseDropDown();
                }
                try
                {
                    this.Edit.DisableMouseHook = true;
                    ipeCur.PropertyValue = value;
                }
                finally
                {
                    this.Edit.DisableMouseHook = false;
                    this.Edit.HookMouseDown = hookMouseDown;
                }
            }
            catch (Exception exception)
            {
                this.SetCommitError(1);
                this.ShowInvalidMessage(ipeCur.PropertyLabel, value, exception);
                return false;
            }
            finally
            {
                this.SetFlag(0x10, false);
            }
            this.SetCommitError(0);
            string propertyTextValue = ipeCur.GetPropertyTextValue();
            if (!string.Equals(propertyTextValue, this.Edit.Text))
            {
                this.Edit.Text = propertyTextValue;
                this.Edit.SelectionStart = 0;
                this.Edit.SelectionLength = 0;
            }
            this.originalTextValue = propertyTextValue;
            this.UpdateResetCommand(ipeCur);
            if (ipeCur.ChildCount != childCount)
            {
                this.ClearGridEntryEvents(this.allGridEntries, 0, -1);
                this.allGridEntries = null;
                this.SelectGridEntry(ipeCur, true);
            }
            if (ipeCur.Disposed)
            {
                bool flag2 = (this.edit != null) && this.edit.Focused;
                this.SelectGridEntry(ipeCur, true);
                ipeCur = this.selectedGridEntry;
                if (flag2 && (this.edit != null))
                {
                    this.edit.Focus();
                }
            }
            this.ownerGrid.OnPropertyValueSet(ipeCur, oldValue);
            return true;
        }

        private void CommonEditorHide()
        {
            this.CommonEditorHide(false);
        }

        private void CommonEditorHide(bool always)
        {
            if (always || this.HasEntries)
            {
                this.CloseDropDown();
                bool flag = false;
                if (((this.Edit.Focused || this.DialogButton.Focused) || this.DropDownButton.Focused) && ((base.IsHandleCreated && base.Visible) && base.Enabled))
                {
                    flag = IntPtr.Zero != System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(this, base.Handle));
                }
                try
                {
                    this.Edit.DontFocus = true;
                    if (this.Edit.Focused && !flag)
                    {
                        flag = this.FocusInternal();
                    }
                    this.Edit.Visible = false;
                    this.Edit.SelectionStart = 0;
                    this.Edit.SelectionLength = 0;
                    if (this.DialogButton.Focused && !flag)
                    {
                        flag = this.FocusInternal();
                    }
                    this.DialogButton.Visible = false;
                    if (this.DropDownButton.Focused && !flag)
                    {
                        flag = this.FocusInternal();
                    }
                    this.DropDownButton.Visible = false;
                    this.currentEditor = null;
                }
                finally
                {
                    this.Edit.DontFocus = false;
                }
            }
        }

        protected virtual void CommonEditorSetup(Control ctl)
        {
            ctl.Visible = false;
            base.Controls.Add(ctl);
        }

        protected virtual void CommonEditorUse(Control ctl, Rectangle rectTarget)
        {
            Rectangle bounds = ctl.Bounds;
            Rectangle clientRectangle = base.ClientRectangle;
            clientRectangle.Inflate(-1, -1);
            try
            {
                rectTarget = Rectangle.Intersect(clientRectangle, rectTarget);
                if (!rectTarget.IsEmpty)
                {
                    if (!rectTarget.Equals(bounds))
                    {
                        ctl.SetBounds(rectTarget.X, rectTarget.Y, rectTarget.Width, rectTarget.Height);
                    }
                    ctl.Visible = true;
                }
            }
            catch
            {
                rectTarget = Rectangle.Empty;
            }
            if (rectTarget.IsEmpty)
            {
                ctl.Visible = false;
            }
            this.currentEditor = ctl;
        }

        private int CountPropsFromOutline(GridEntryCollection rgipes)
        {
            if (rgipes == null)
            {
                return 0;
            }
            int count = rgipes.Count;
            for (int i = 0; i < rgipes.Count; i++)
            {
                if (((GridEntry) rgipes[i]).InternalExpanded)
                {
                    count += this.CountPropsFromOutline(((GridEntry) rgipes[i]).Children);
                }
            }
            return count;
        }

        protected override AccessibleObject CreateAccessibilityInstance()
        {
            return new PropertyGridViewAccessibleObject(this);
        }

        private Bitmap CreateDownArrow()
        {
            Bitmap bitmap = null;
            try
            {
                Icon icon = new Icon(typeof(PropertyGrid), "Arrow.ico");
                bitmap = icon.ToBitmap();
                icon.Dispose();
            }
            catch (Exception)
            {
                bitmap = new Bitmap(0x10, 0x10);
            }
            return bitmap;
        }

        protected virtual void CreateUI()
        {
            this.UpdateUIBasedOnFont(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.scrollBar != null)
                {
                    this.scrollBar.Dispose();
                }
                if (this.listBox != null)
                {
                    this.listBox.Dispose();
                }
                if (this.dropDownHolder != null)
                {
                    this.dropDownHolder.Dispose();
                }
                this.scrollBar = null;
                this.listBox = null;
                this.dropDownHolder = null;
                this.ownerGrid = null;
                this.topLevelGridEntries = null;
                this.allGridEntries = null;
                this.serviceProvider = null;
                this.topHelpService = null;
                if ((this.helpService != null) && (this.helpService is IDisposable))
                {
                    ((IDisposable) this.helpService).Dispose();
                }
                this.helpService = null;
                if (this.edit != null)
                {
                    this.edit.Dispose();
                    this.edit = null;
                }
                if (this.fontBold != null)
                {
                    this.fontBold.Dispose();
                    this.fontBold = null;
                }
                if (this.btnDropDown != null)
                {
                    this.btnDropDown.Dispose();
                    this.btnDropDown = null;
                }
                if (this.btnDialog != null)
                {
                    this.btnDialog.Dispose();
                    this.btnDialog = null;
                }
                if (this.toolTip != null)
                {
                    this.toolTip.Dispose();
                    this.toolTip = null;
                }
            }
            base.Dispose(disposing);
        }

        public void DoCopyCommand()
        {
            if (this.CanCopy)
            {
                if (this.Edit.Focused)
                {
                    this.Edit.Copy();
                }
                else
                {
                    Clipboard.SetDataObject(this.selectedGridEntry.GetPropertyTextValue());
                }
            }
        }

        public void DoCutCommand()
        {
            if (this.CanCut)
            {
                this.DoCopyCommand();
                if (this.Edit.Visible)
                {
                    this.Edit.Cut();
                }
            }
        }

        public void DoPasteCommand()
        {
            if (this.CanPaste && this.Edit.Visible)
            {
                if (this.Edit.Focused)
                {
                    this.Edit.Paste();
                }
                else
                {
                    IDataObject dataObject = Clipboard.GetDataObject();
                    if (dataObject != null)
                    {
                        string data = (string) dataObject.GetData(typeof(string));
                        if (data != null)
                        {
                            this.Edit.FocusInternal();
                            this.Edit.Text = data;
                            this.SetCommitError(0, true);
                        }
                    }
                }
            }
        }

        public void DoubleClickRow(int row, bool toggleExpand, int type)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(row);
            if (gridEntryFromRow != null)
            {
                if (!toggleExpand || (type == 2))
                {
                    try
                    {
                        if (gridEntryFromRow.DoubleClickPropertyValue())
                        {
                            this.SelectRow(row);
                            return;
                        }
                    }
                    catch (Exception exception)
                    {
                        this.SetCommitError(1);
                        this.ShowInvalidMessage(gridEntryFromRow.PropertyLabel, null, exception);
                        return;
                    }
                }
                this.SelectGridEntry(gridEntryFromRow, true);
                if (((type != 1) || !toggleExpand) || !gridEntryFromRow.Expandable)
                {
                    if (gridEntryFromRow.IsValueEditable && gridEntryFromRow.Enumerable)
                    {
                        int currentValueIndex = this.GetCurrentValueIndex(gridEntryFromRow);
                        if (currentValueIndex != -1)
                        {
                            object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
                            if ((propertyValueList == null) || (currentValueIndex >= (propertyValueList.Length - 1)))
                            {
                                currentValueIndex = 0;
                            }
                            else
                            {
                                currentValueIndex++;
                            }
                            this.CommitValue(propertyValueList[currentValueIndex]);
                            this.SelectRow(this.selectedRow);
                            this.Refresh();
                            return;
                        }
                    }
                    if (this.Edit.Visible)
                    {
                        this.Edit.FocusInternal();
                        this.SelectEdit(false);
                    }
                }
                else
                {
                    this.SetExpand(gridEntryFromRow, !gridEntryFromRow.InternalExpanded);
                }
            }
        }

        public void DoUndoCommand()
        {
            if (this.CanUndo && this.Edit.Visible)
            {
                this.Edit.SendMessage(0x304, 0, 0);
            }
        }

        protected virtual void DrawLabel(Graphics g, int row, Rectangle rect, bool selected, bool fLongLabelRequest, ref Rectangle clipRect)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(row);
            if ((gridEntryFromRow != null) && !rect.IsEmpty)
            {
                Point newOrigin = new Point(rect.X, rect.Y);
                Rectangle rectangle = Rectangle.Intersect(rect, clipRect);
                if (!rectangle.IsEmpty)
                {
                    this.AdjustOrigin(g, newOrigin, ref rect);
                    rectangle.Offset(-newOrigin.X, -newOrigin.Y);
                    try
                    {
                        bool paintFullLabel = false;
                        this.GetIPELabelIndent(gridEntryFromRow);
                        if (fLongLabelRequest)
                        {
                            this.GetIPELabelLength(g, gridEntryFromRow);
                            paintFullLabel = this.IsIPELabelLong(g, gridEntryFromRow);
                        }
                        gridEntryFromRow.PaintLabel(g, rect, rectangle, selected, paintFullLabel);
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        this.ResetOrigin(g);
                    }
                }
            }
        }

        private void DrawValue(Graphics g, Rectangle rect, Rectangle clipRect, GridEntry gridEntry, object value, bool drawSelected, bool checkShouldSerialize, bool fetchValue, bool paintInPlace)
        {
            GridEntry.PaintValueFlags none = GridEntry.PaintValueFlags.None;
            if (drawSelected)
            {
                none |= GridEntry.PaintValueFlags.DrawSelected;
            }
            if (checkShouldSerialize)
            {
                none |= GridEntry.PaintValueFlags.CheckShouldSerialize;
            }
            if (fetchValue)
            {
                none |= GridEntry.PaintValueFlags.FetchValue;
            }
            if (paintInPlace)
            {
                none |= GridEntry.PaintValueFlags.PaintInPlace;
            }
            gridEntry.PaintValue(value, g, rect, clipRect, none);
        }

        protected virtual void DrawValueEntry(Graphics g, int row, ref Rectangle clipRect)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(row);
            if (gridEntryFromRow != null)
            {
                Rectangle b = this.GetRectangle(row, 2);
                Point newOrigin = new Point(b.X, b.Y);
                Rectangle rectangle2 = Rectangle.Intersect(clipRect, b);
                if (!rectangle2.IsEmpty)
                {
                    this.AdjustOrigin(g, newOrigin, ref b);
                    rectangle2.Offset(-newOrigin.X, -newOrigin.Y);
                    try
                    {
                        this.DrawValueEntry(g, b, rectangle2, gridEntryFromRow, null, true);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        this.ResetOrigin(g);
                    }
                }
            }
        }

        private void DrawValueEntry(Graphics g, Rectangle rect, Rectangle clipRect, GridEntry gridEntry, object value, bool fetchValue)
        {
            this.DrawValue(g, rect, clipRect, gridEntry, value, false, true, fetchValue, true);
        }

        public void DropDownControl(Control ctl)
        {
            if (this.dropDownHolder == null)
            {
                this.dropDownHolder = new DropDownHolder(this);
            }
            this.dropDownHolder.Visible = false;
            this.dropDownHolder.SetComponent(ctl, this.GetFlag(0x400));
            Rectangle rectangle = this.GetRectangle(this.selectedRow, 2);
            Size size = this.dropDownHolder.Size;
            Point point = base.PointToScreen(new Point(0, 0));
            Rectangle workingArea = Screen.FromControl(this.Edit).WorkingArea;
            size.Width = Math.Max(rectangle.Width + 1, size.Width);
            point.X = Math.Min((workingArea.X + workingArea.Width) - size.Width, Math.Max(workingArea.X, ((point.X + rectangle.X) + rectangle.Width) - size.Width));
            point.Y += rectangle.Y;
            if ((workingArea.Y + workingArea.Height) < ((size.Height + point.Y) + this.Edit.Height))
            {
                point.Y -= size.Height;
                this.dropDownHolder.ResizeUp = true;
            }
            else
            {
                point.Y += rectangle.Height + 1;
                this.dropDownHolder.ResizeUp = false;
            }
            System.Windows.Forms.UnsafeNativeMethods.SetWindowLong(new HandleRef(this.dropDownHolder, this.dropDownHolder.Handle), -8, new HandleRef(this, base.Handle));
            this.dropDownHolder.SetBounds(point.X, point.Y, size.Width, size.Height);
            System.Windows.Forms.SafeNativeMethods.ShowWindow(new HandleRef(this.dropDownHolder, this.dropDownHolder.Handle), 8);
            this.Edit.Filter = true;
            this.dropDownHolder.Visible = true;
            this.dropDownHolder.FocusComponent();
            this.SelectEdit(false);
            try
            {
                this.DropDownButton.IgnoreMouse = true;
                this.dropDownHolder.DoModalLoop();
            }
            finally
            {
                this.DropDownButton.IgnoreMouse = false;
            }
            if (this.selectedRow != -1)
            {
                this.FocusInternal();
                this.SelectRow(this.selectedRow);
            }
        }

        public virtual void DropDownDone()
        {
            this.CloseDropDown();
        }

        public virtual void DropDownUpdate()
        {
            if ((this.dropDownHolder != null) && this.dropDownHolder.GetUsed())
            {
                int selectedRow = this.selectedRow;
                this.Edit.Text = this.GetGridEntryFromRow(selectedRow).GetPropertyTextValue();
            }
        }

        internal void DumpPropsToConsole(GridEntry entry, string prefix)
        {
            System.Type propertyType = entry.PropertyType;
            if (entry.PropertyValue != null)
            {
                propertyType = entry.PropertyValue.GetType();
            }
            Console.WriteLine(prefix + entry.PropertyLabel + ", value type=" + ((propertyType == null) ? "(null)" : propertyType.FullName) + ", value=" + ((entry.PropertyValue == null) ? "(null)" : entry.PropertyValue.ToString()) + ", flags=" + entry.Flags.ToString(CultureInfo.InvariantCulture) + ", TypeConverter=" + ((entry.TypeConverter == null) ? "(null)" : entry.TypeConverter.GetType().FullName) + ", UITypeEditor=" + ((entry.UITypeEditor == null) ? "(null)" : entry.UITypeEditor.GetType().FullName));
            GridEntryCollection children = entry.Children;
            if (children != null)
            {
                foreach (GridEntry entry2 in children)
                {
                    this.DumpPropsToConsole(entry2, prefix + "\t");
                }
            }
        }

        public bool EnsurePendingChangesCommitted()
        {
            this.CloseDropDown();
            return this.Commit();
        }

        private void F4Selection(bool popupModalDialog)
        {
            if (this.GetGridEntryFromRow(this.selectedRow) != null)
            {
                if ((this.errorState != 0) && this.Edit.Visible)
                {
                    this.Edit.FocusInternal();
                }
                else if (this.DropDownButton.Visible)
                {
                    this.PopupDialog(this.selectedRow);
                }
                else if (this.DialogButton.Visible)
                {
                    if (popupModalDialog)
                    {
                        this.PopupDialog(this.selectedRow);
                    }
                    else
                    {
                        this.DialogButton.FocusInternal();
                    }
                }
                else if (this.Edit.Visible)
                {
                    this.Edit.FocusInternal();
                    this.SelectEdit(false);
                }
            }
        }

        private bool FilterEditWndProc(ref Message m)
        {
            if (((this.dropDownHolder != null) && this.dropDownHolder.Visible) && ((m.Msg == 0x100) && (((int) m.WParam) != 9)))
            {
                Control component = this.dropDownHolder.Component;
                if (component != null)
                {
                    m.Result = component.SendMessage(m.Msg, m.WParam, m.LParam);
                    return true;
                }
            }
            return false;
        }

        public void FilterKeyPress(char keyChar)
        {
            if (this.GetGridEntryFromRow(this.selectedRow) != null)
            {
                this.Edit.FilterKeyPress(keyChar);
            }
        }

        private bool FilterReadOnlyEditKeyPress(char keyChar)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
            if (gridEntryFromRow.Enumerable && gridEntryFromRow.IsValueEditable)
            {
                int currentValueIndex = this.GetCurrentValueIndex(gridEntryFromRow);
                object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
                string strB = new string(new char[] { keyChar });
                for (int i = 0; i < propertyValueList.Length; i++)
                {
                    object obj2 = propertyValueList[((i + currentValueIndex) + 1) % propertyValueList.Length];
                    string propertyTextValue = gridEntryFromRow.GetPropertyTextValue(obj2);
                    if (((propertyTextValue != null) && (propertyTextValue.Length > 0)) && (string.Compare(propertyTextValue.Substring(0, 1), strB, true, CultureInfo.InvariantCulture) == 0))
                    {
                        this.CommitValue(obj2);
                        if (this.Edit.Focused)
                        {
                            this.SelectEdit(false);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private GridEntry FindEquivalentGridEntry(GridEntryCollection ipeHier)
        {
            if ((ipeHier == null) || (ipeHier.Count == 0))
            {
                return null;
            }
            GridEntryCollection allGridEntries = this.GetAllGridEntries();
            if ((allGridEntries == null) || (allGridEntries.Count == 0))
            {
                return null;
            }
            GridEntry gridEntry = null;
            int index = 0;
            int count = allGridEntries.Count;
            for (int i = 0; i < ipeHier.Count; i++)
            {
                if (ipeHier[i] == null)
                {
                    continue;
                }
                if (gridEntry != null)
                {
                    int num1 = allGridEntries.Count;
                    if (!gridEntry.InternalExpanded)
                    {
                        this.SetExpand(gridEntry, true);
                        allGridEntries = this.GetAllGridEntries();
                    }
                    count = gridEntry.VisibleChildCount;
                }
                int num4 = index;
                gridEntry = null;
                while ((index < allGridEntries.Count) && ((index - num4) <= count))
                {
                    if (ipeHier.GetEntry(i).NonParentEquals(allGridEntries[index]))
                    {
                        gridEntry = allGridEntries.GetEntry(index);
                        index++;
                        break;
                    }
                    index++;
                }
                if (gridEntry == null)
                {
                    return gridEntry;
                }
            }
            return gridEntry;
        }

        protected virtual Point FindPosition(int x, int y)
        {
            if (this.RowHeight == -1)
            {
                return InvalidPosition;
            }
            Size ourSize = this.GetOurSize();
            if ((x < 0) || (x > (ourSize.Width + this.ptOurLocation.X)))
            {
                return InvalidPosition;
            }
            Point point = new Point(1, 0);
            if (x > (this.InternalLabelWidth + this.ptOurLocation.X))
            {
                point.X = 2;
            }
            point.Y = (y - this.ptOurLocation.Y) / (1 + this.RowHeight);
            return point;
        }

        public virtual void Flush()
        {
            if (this.Commit() && this.Edit.Focused)
            {
                this.FocusInternal();
            }
        }

        private GridEntryCollection GetAllGridEntries()
        {
            return this.GetAllGridEntries(false);
        }

        private GridEntryCollection GetAllGridEntries(bool fUpdateCache)
        {
            if (((this.visibleRows == -1) || (this.totalProps == -1)) || !this.HasEntries)
            {
                return null;
            }
            if ((this.allGridEntries == null) || fUpdateCache)
            {
                GridEntry[] rgipeTarget = new GridEntry[this.totalProps];
                try
                {
                    this.GetGridEntriesFromOutline(this.topLevelGridEntries, 0, 0, rgipeTarget);
                }
                catch (Exception)
                {
                }
                this.allGridEntries = new GridEntryCollection(null, rgipeTarget);
                this.AddGridEntryEvents(this.allGridEntries, 0, -1);
            }
            return this.allGridEntries;
        }

        internal Brush GetBackgroundBrush(Graphics g)
        {
            return this.backgroundBrush;
        }

        public Font GetBaseFont()
        {
            return this.Font;
        }

        internal IntPtr GetBaseHfont()
        {
            if (this.baseHfont == IntPtr.Zero)
            {
                this.baseHfont = this.GetBaseFont().ToHfont();
            }
            return this.baseHfont;
        }

        public Font GetBoldFont()
        {
            if (this.fontBold == null)
            {
                this.fontBold = new Font(this.Font, FontStyle.Bold);
            }
            return this.fontBold;
        }

        internal IntPtr GetBoldHfont()
        {
            if (this.boldHfont == IntPtr.Zero)
            {
                this.boldHfont = this.GetBoldFont().ToHfont();
            }
            return this.boldHfont;
        }

        private int GetCurrentValueIndex(GridEntry gridEntry)
        {
            if (gridEntry.Enumerable)
            {
                try
                {
                    object[] propertyValueList = gridEntry.GetPropertyValueList();
                    object propertyValue = gridEntry.PropertyValue;
                    string strA = gridEntry.TypeConverter.ConvertToString(gridEntry, propertyValue);
                    if ((propertyValueList != null) && (propertyValueList.Length > 0))
                    {
                        int num = -1;
                        int num2 = -1;
                        for (int i = 0; i < propertyValueList.Length; i++)
                        {
                            object obj3 = propertyValueList[i];
                            string strB = gridEntry.TypeConverter.ConvertToString(obj3);
                            if ((propertyValue == obj3) || (string.Compare(strA, strB, true, CultureInfo.InvariantCulture) == 0))
                            {
                                num = i;
                            }
                            if (((propertyValue != null) && (obj3 != null)) && obj3.Equals(propertyValue))
                            {
                                num2 = i;
                            }
                            if ((num == num2) && (num != -1))
                            {
                                return num;
                            }
                        }
                        if (num != -1)
                        {
                            return num;
                        }
                        if (num2 != -1)
                        {
                            return num2;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return -1;
        }

        public virtual int GetDefaultOutlineIndent()
        {
            return 10;
        }

        private bool GetFlag(short flag)
        {
            return ((this.flags & flag) != 0);
        }

        private int GetGridEntriesFromOutline(GridEntryCollection rgipe, int cCur, int cTarget, GridEntry[] rgipeTarget)
        {
            if ((rgipe != null) && (rgipe.Count != 0))
            {
                cCur--;
                for (int i = 0; i < rgipe.Count; i++)
                {
                    cCur++;
                    if (cCur >= (cTarget + rgipeTarget.Length))
                    {
                        return cCur;
                    }
                    GridEntry entry = rgipe.GetEntry(i);
                    if (cCur >= cTarget)
                    {
                        rgipeTarget[cCur - cTarget] = entry;
                    }
                    if (entry.InternalExpanded)
                    {
                        GridEntryCollection children = entry.Children;
                        if ((children != null) && (children.Count > 0))
                        {
                            cCur = this.GetGridEntriesFromOutline(children, cCur + 1, cTarget, rgipeTarget);
                        }
                    }
                }
            }
            return cCur;
        }

        private GridEntry GetGridEntryFromOffset(int offset)
        {
            GridEntryCollection allGridEntries = this.GetAllGridEntries();
            if (((allGridEntries != null) && (offset >= 0)) && (offset < allGridEntries.Count))
            {
                return allGridEntries.GetEntry(offset);
            }
            return null;
        }

        private GridEntry GetGridEntryFromRow(int row)
        {
            return this.GetGridEntryFromOffset(row + this.GetScrollOffset());
        }

        public virtual int GetGridEntryHeight()
        {
            return this.RowHeight;
        }

        private GridEntryCollection GetGridEntryHierarchy(GridEntry gridEntry)
        {
            if (gridEntry == null)
            {
                return null;
            }
            int propertyDepth = gridEntry.PropertyDepth;
            if (propertyDepth > 0)
            {
                GridEntry[] entries = new GridEntry[propertyDepth + 1];
                while ((gridEntry != null) && (propertyDepth >= 0))
                {
                    entries[propertyDepth] = gridEntry;
                    gridEntry = gridEntry.ParentGridEntry;
                    propertyDepth = gridEntry.PropertyDepth;
                }
                return new GridEntryCollection(null, entries);
            }
            return new GridEntryCollection(null, new GridEntry[] { gridEntry });
        }

        private IHelpService GetHelpService()
        {
            if ((this.helpService == null) && (this.ServiceProvider != null))
            {
                this.topHelpService = (IHelpService) this.ServiceProvider.GetService(typeof(IHelpService));
                if (this.topHelpService != null)
                {
                    IHelpService service = this.topHelpService.CreateLocalContext(HelpContextType.ToolWindowSelection);
                    if (service != null)
                    {
                        this.helpService = service;
                    }
                }
            }
            return this.helpService;
        }

        public virtual IntPtr GetHostHandle()
        {
            return base.Handle;
        }

        public virtual bool GetInPropertySet()
        {
            return this.GetFlag(0x10);
        }

        private int GetIPELabelIndent(GridEntry gridEntry)
        {
            return (gridEntry.PropertyLabelIndent + 1);
        }

        private int GetIPELabelLength(Graphics g, GridEntry gridEntry)
        {
            Size size = Size.Ceiling(PropertyGrid.MeasureTextHelper.MeasureText(this.ownerGrid, g, gridEntry.PropertyLabel, this.Font));
            return ((this.ptOurLocation.X + this.GetIPELabelIndent(gridEntry)) + size.Width);
        }

        public virtual int GetLabelWidth()
        {
            return this.InternalLabelWidth;
        }

        public virtual Brush GetLineBrush(Graphics g)
        {
            if (this.ownerGrid.lineBrush == null)
            {
                System.Drawing.Color nearestColor = g.GetNearestColor(this.ownerGrid.LineColor);
                this.ownerGrid.lineBrush = new SolidBrush(nearestColor);
            }
            return this.ownerGrid.lineBrush;
        }

        public virtual System.Drawing.Color GetLineColor()
        {
            return this.ownerGrid.LineColor;
        }

        private Size GetOurSize()
        {
            Size clientSize = base.ClientSize;
            if (clientSize.Width == 0)
            {
                Size size = base.Size;
                if (size.Width > 10)
                {
                    clientSize.Width = size.Width;
                    clientSize.Height = size.Height;
                }
            }
            if (!this.GetScrollbarHidden())
            {
                Size size3 = this.ScrollBar.Size;
                clientSize.Width -= size3.Width;
            }
            clientSize.Width -= 2;
            clientSize.Height -= 2;
            return clientSize;
        }

        public virtual int GetOutlineIconSize()
        {
            if (this.IsExplorerTreeSupported)
            {
                return 0x10;
            }
            return 9;
        }

        internal int GetPropertyLocation(string propName, bool getXY, bool rowValue)
        {
            if ((this.allGridEntries != null) && (this.allGridEntries.Count > 0))
            {
                for (int i = 0; i < this.allGridEntries.Count; i++)
                {
                    if (string.Compare(propName, this.allGridEntries.GetEntry(i).PropertyLabel, true, CultureInfo.InvariantCulture) == 0)
                    {
                        if (!getXY)
                        {
                            return i;
                        }
                        int rowFromGridEntry = this.GetRowFromGridEntry(this.allGridEntries.GetEntry(i));
                        if ((rowFromGridEntry < 0) || (rowFromGridEntry >= this.visibleRows))
                        {
                            return -1;
                        }
                        Rectangle rectangle = this.GetRectangle(rowFromGridEntry, rowValue ? 2 : 1);
                        return ((rectangle.Y << 0x10) | (rectangle.X & 0xffff));
                    }
                }
            }
            return -1;
        }

        public Rectangle GetRectangle(int row, int flRow)
        {
            Rectangle rectangle = new Rectangle(0, 0, 0, 0);
            Size ourSize = this.GetOurSize();
            rectangle.X = this.ptOurLocation.X;
            bool flag = (flRow & 1) != 0;
            bool flag2 = (flRow & 2) != 0;
            if (flag && flag2)
            {
                rectangle.X = 1;
                rectangle.Width = ourSize.Width - 1;
            }
            else if (flag)
            {
                rectangle.X = 1;
                rectangle.Width = this.InternalLabelWidth - 1;
            }
            else if (flag2)
            {
                rectangle.X = this.ptOurLocation.X + this.InternalLabelWidth;
                rectangle.Width = ourSize.Width - this.InternalLabelWidth;
            }
            rectangle.Y = ((row * (this.RowHeight + 1)) + 1) + this.ptOurLocation.Y;
            rectangle.Height = this.RowHeight;
            return rectangle;
        }

        private int GetRowFromGridEntry(GridEntry gridEntry)
        {
            GridEntryCollection allGridEntries = this.GetAllGridEntries();
            if ((gridEntry == null) || (allGridEntries == null))
            {
                return -1;
            }
            int num = -1;
            for (int i = 0; i < allGridEntries.Count; i++)
            {
                if (gridEntry == allGridEntries[i])
                {
                    return (i - this.GetScrollOffset());
                }
                if ((num == -1) && gridEntry.Equals(allGridEntries[i]))
                {
                    num = i - this.GetScrollOffset();
                }
            }
            if (num != -1)
            {
                return num;
            }
            return (-1 - this.GetScrollOffset());
        }

        protected virtual bool GetScrollbarHidden()
        {
            return ((this.scrollBar == null) || !this.ScrollBar.Visible);
        }

        public virtual int GetScrollOffset()
        {
            if (this.scrollBar == null)
            {
                return 0;
            }
            return this.ScrollBar.Value;
        }

        public object GetService(System.Type classService)
        {
            if (classService == typeof(IWindowsFormsEditorService))
            {
                return this;
            }
            if (this.ServiceProvider != null)
            {
                return this.serviceProvider.GetService(classService);
            }
            return null;
        }

        public virtual int GetSplitterWidth()
        {
            return 1;
        }

        public virtual string GetTestingInfo(int entry)
        {
            GridEntry entry2 = (entry < 0) ? this.GetGridEntryFromRow(this.selectedRow) : this.GetGridEntryFromOffset(entry);
            if (entry2 == null)
            {
                return "";
            }
            return entry2.GetTestingInfo();
        }

        public System.Drawing.Color GetTextColor()
        {
            return this.ForeColor;
        }

        public virtual int GetTotalWidth()
        {
            return ((this.GetLabelWidth() + this.GetSplitterWidth()) + this.GetValueWidth());
        }

        public virtual int GetValuePaintIndent()
        {
            return 0x1a;
        }

        public virtual int GetValuePaintWidth()
        {
            return 20;
        }

        public virtual int GetValueStringIndent()
        {
            return 0;
        }

        public virtual int GetValueWidth()
        {
            return (int) (this.InternalLabelWidth * (this.labelRatio - 1.0));
        }

        internal void InvalidateGridEntryValue(GridEntry ge)
        {
            int rowFromGridEntry = this.GetRowFromGridEntry(ge);
            if (rowFromGridEntry != -1)
            {
                this.InvalidateRows(rowFromGridEntry, rowFromGridEntry, 2);
            }
        }

        private void InvalidateRow(int row)
        {
            this.InvalidateRows(row, row, 3);
        }

        private void InvalidateRows(int startRow, int endRow)
        {
            this.InvalidateRows(startRow, endRow, 3);
        }

        private void InvalidateRows(int startRow, int endRow, int type)
        {
            Rectangle rectangle;
            if (endRow == -1)
            {
                rectangle = this.GetRectangle(startRow, type);
                rectangle.Height = (base.Size.Height - rectangle.Y) - 1;
                base.Invalidate(rectangle);
            }
            else
            {
                for (int i = startRow; i <= endRow; i++)
                {
                    rectangle = this.GetRectangle(i, type);
                    base.Invalidate(rectangle);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            Keys keys = keyData & Keys.KeyCode;
            if (keys <= Keys.Enter)
            {
                switch (keys)
                {
                    case Keys.Tab:
                        goto Label_0023;

                    case Keys.Enter:
                        if (this.Edit.Focused)
                        {
                            return false;
                        }
                        break;
                }
                goto Label_0034;
            }
            if ((keys != Keys.Escape) && (keys != Keys.F4))
            {
                goto Label_0034;
            }
        Label_0023:
            return false;
        Label_0034:
            return base.IsInputKey(keyData);
        }

        private bool IsIPELabelLong(Graphics g, GridEntry gridEntry)
        {
            if (gridEntry == null)
            {
                return false;
            }
            return (this.GetIPELabelLength(g, gridEntry) > (this.ptOurLocation.X + this.InternalLabelWidth));
        }

        private bool IsMyChild(Control c)
        {
            if ((c != this) && (c != null))
            {
                for (Control control = c.ParentInternal; control != null; control = control.ParentInternal)
                {
                    if (control == this)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsScrollValueValid(int newValue)
        {
            return (((newValue != this.ScrollBar.Value) && (newValue >= 0)) && ((newValue <= this.ScrollBar.Maximum) && ((newValue + (this.ScrollBar.LargeChange - 1)) < this.totalProps)));
        }

        internal bool IsSiblingControl(Control c1, Control c2)
        {
            Control parentInternal = c1.ParentInternal;
            for (Control control2 = c2.ParentInternal; control2 != null; control2 = control2.ParentInternal)
            {
                if (parentInternal == control2)
                {
                    return true;
                }
            }
            return false;
        }

        private void LayoutWindow(bool invalidate)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            Size size = new Size(clientRectangle.Width, clientRectangle.Height);
            if (this.scrollBar != null)
            {
                Rectangle bounds = this.ScrollBar.Bounds;
                bounds.X = (size.Width - bounds.Width) - 1;
                bounds.Y = 1;
                bounds.Height = size.Height - 2;
                this.ScrollBar.Bounds = bounds;
            }
            if (invalidate)
            {
                base.Invalidate();
            }
        }

        private void MoveSplitterTo(int xpos)
        {
            int width = this.GetOurSize().Width;
            int x = this.ptOurLocation.X;
            int num3 = Math.Max(Math.Min(xpos, width - 10), this.GetOutlineIconSize() * 2);
            int internalLabelWidth = this.InternalLabelWidth;
            this.labelRatio = ((double) width) / ((double) (num3 - x));
            this.SetConstants();
            if (this.selectedRow != -1)
            {
                this.SelectRow(this.selectedRow);
            }
            Rectangle clientRectangle = base.ClientRectangle;
            if (internalLabelWidth > this.InternalLabelWidth)
            {
                int num5 = this.InternalLabelWidth - this.requiredLabelPaintMargin;
                base.Invalidate(new Rectangle(num5, 0, base.Size.Width - num5, base.Size.Height));
            }
            else
            {
                clientRectangle.X = internalLabelWidth - this.requiredLabelPaintMargin;
                clientRectangle.Width -= clientRectangle.X;
                base.Invalidate(clientRectangle);
            }
        }

        private void OnBtnClick(object sender, EventArgs e)
        {
            if (!this.GetFlag(0x100) && ((sender != this.DialogButton) || this.Commit()))
            {
                this.SetCommitError(0);
                try
                {
                    this.Commit();
                    this.SetFlag(0x100, true);
                    this.PopupDialog(this.selectedRow);
                }
                finally
                {
                    this.SetFlag(0x100, false);
                }
            }
        }

        private void OnBtnKeyDown(object sender, KeyEventArgs ke)
        {
            this.OnKeyDown(sender, ke);
        }

        private void OnChildLostFocus(object sender, EventArgs e)
        {
            this.OnLostFocus(null);
        }

        private void OnEditChange(object sender, EventArgs e)
        {
            this.SetCommitError(0, this.Edit.Focused);
            this.ToolTip.ToolTip = "";
            this.ToolTip.Visible = false;
            if (!this.Edit.InSetText())
            {
                GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
                if ((gridEntryFromRow != null) && ((gridEntryFromRow.Flags & 8) != 0))
                {
                    this.Commit();
                }
            }
        }

        private void OnEditGotFocus(object sender, EventArgs e)
        {
            if (!this.Edit.Visible)
            {
                this.FocusInternal();
            }
            else
            {
                switch (this.errorState)
                {
                    case 1:
                        if (this.Edit.Visible)
                        {
                            this.Edit.HookMouseDown = true;
                        }
                        break;

                    case 2:
                        return;

                    default:
                        if (this.NeedsCommit)
                        {
                            this.SetCommitError(0, true);
                        }
                        break;
                }
                if ((this.selectedGridEntry != null) && (this.GetRowFromGridEntry(this.selectedGridEntry) != -1))
                {
                    this.selectedGridEntry.Focus = true;
                    this.InvalidateRow(this.selectedRow);
                    (this.Edit.AccessibilityObject as Control.ControlAccessibleObject).NotifyClients(AccessibleEvents.Focus);
                }
                else
                {
                    this.SelectRow(0);
                }
            }
        }

        private void OnEditKeyDown(object sender, KeyEventArgs ke)
        {
            if (!ke.Alt && ((ke.KeyCode == Keys.Up) || (ke.KeyCode == Keys.Down)))
            {
                GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
                if (!gridEntryFromRow.Enumerable || !gridEntryFromRow.IsValueEditable)
                {
                    return;
                }
                object propertyValue = gridEntryFromRow.PropertyValue;
                object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
                ke.Handled = true;
                if (propertyValueList != null)
                {
                    for (int i = 0; i < propertyValueList.Length; i++)
                    {
                        object obj3 = propertyValueList[i];
                        if (((propertyValue != null) && (obj3 != null)) && ((propertyValue.GetType() != obj3.GetType()) && gridEntryFromRow.TypeConverter.CanConvertTo(gridEntryFromRow, propertyValue.GetType())))
                        {
                            obj3 = gridEntryFromRow.TypeConverter.ConvertTo(gridEntryFromRow, CultureInfo.CurrentCulture, obj3, propertyValue.GetType());
                        }
                        bool flag2 = (propertyValue == obj3) || ((propertyValue != null) && propertyValue.Equals(obj3));
                        if ((!flag2 && (propertyValue is string)) && (obj3 != null))
                        {
                            flag2 = 0 == string.Compare((string) propertyValue, obj3.ToString(), true, CultureInfo.CurrentCulture);
                        }
                        if (flag2)
                        {
                            object obj4 = null;
                            if (ke.KeyCode == Keys.Up)
                            {
                                if (i == 0)
                                {
                                    return;
                                }
                                obj4 = propertyValueList[i - 1];
                            }
                            else
                            {
                                if (i == (propertyValueList.Length - 1))
                                {
                                    return;
                                }
                                obj4 = propertyValueList[i + 1];
                            }
                            this.CommitValue(obj4);
                            this.SelectEdit(false);
                            return;
                        }
                    }
                }
            }
            else if (((ke.KeyCode == Keys.Left) || (ke.KeyCode == Keys.Right)) && ((ke.Modifiers & ~Keys.Shift) != Keys.None))
            {
                return;
            }
            this.OnKeyDown(sender, ke);
        }

        private void OnEditKeyPress(object sender, KeyPressEventArgs ke)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
            if ((gridEntryFromRow != null) && !gridEntryFromRow.IsTextEditable)
            {
                ke.Handled = this.FilterReadOnlyEditKeyPress(ke.KeyChar);
            }
        }

        private void OnEditLostFocus(object sender, EventArgs e)
        {
            if ((!this.Edit.Focused && (this.errorState != 2)) && ((this.errorState != 1) && !this.GetInPropertySet()))
            {
                if ((this.dropDownHolder != null) && this.dropDownHolder.Visible)
                {
                    bool flag = false;
                    for (IntPtr ptr = System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow(); ptr != IntPtr.Zero; ptr = System.Windows.Forms.UnsafeNativeMethods.GetParent(new HandleRef(null, ptr)))
                    {
                        if (ptr == this.dropDownHolder.Handle)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        return;
                    }
                }
                if (!this.FocusInside)
                {
                    if (!this.Commit())
                    {
                        this.Edit.FocusInternal();
                    }
                    else
                    {
                        this.OnLostFocus(null);
                    }
                }
            }
        }

        private void OnEditMouseDown(object sender, MouseEventArgs me)
        {
            if (!this.FocusInside)
            {
                this.SelectGridEntry(this.selectedGridEntry, false);
            }
            if ((me.Clicks % 2) == 0)
            {
                this.DoubleClickRow(this.selectedRow, false, 2);
                this.Edit.SelectAll();
            }
            if (this.rowSelectTime != 0L)
            {
                int num2 = (int) ((DateTime.Now.Ticks - this.rowSelectTime) / 0x2710L);
                if (num2 < SystemInformation.DoubleClickTime)
                {
                    Point point = this.Edit.PointToScreen(new Point(me.X, me.Y));
                    if ((Math.Abs((int) (point.X - this.rowSelectPos.X)) < SystemInformation.DoubleClickSize.Width) && (Math.Abs((int) (point.Y - this.rowSelectPos.Y)) < SystemInformation.DoubleClickSize.Height))
                    {
                        this.DoubleClickRow(this.selectedRow, false, 2);
                        this.Edit.SendMessage(0x202, 0, (int) ((me.Y << 0x10) | (me.X & 0xffff)));
                        this.Edit.SelectAll();
                    }
                    this.rowSelectPos = Point.Empty;
                    this.rowSelectTime = 0L;
                }
            }
        }

        private bool OnEscape(Control sender)
        {
            if ((Control.ModifierKeys & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                this.SetFlag(0x40, false);
                if ((sender == this.Edit) && this.Edit.Focused)
                {
                    if (this.errorState == 0)
                    {
                        this.Edit.Text = this.originalTextValue;
                        this.FocusInternal();
                        return true;
                    }
                    if (this.NeedsCommit)
                    {
                        bool flag = false;
                        this.Edit.Text = this.originalTextValue;
                        bool flag2 = true;
                        if (this.selectedGridEntry != null)
                        {
                            string propertyTextValue = this.selectedGridEntry.GetPropertyTextValue();
                            flag2 = (this.originalTextValue != propertyTextValue) && (!string.IsNullOrEmpty(this.originalTextValue) || !string.IsNullOrEmpty(propertyTextValue));
                        }
                        if (flag2)
                        {
                            try
                            {
                                flag = this.CommitText(this.originalTextValue);
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                        if (!flag)
                        {
                            this.Edit.FocusInternal();
                            this.SelectEdit(false);
                            return true;
                        }
                    }
                    this.SetCommitError(0);
                    this.FocusInternal();
                    return true;
                }
                if (sender != this)
                {
                    this.CloseDropDown();
                    this.FocusInternal();
                }
            }
            return false;
        }

        private bool OnF4(Control sender)
        {
            if (Control.ModifierKeys != Keys.None)
            {
                return false;
            }
            if ((sender == this) || (sender == this.ownerGrid))
            {
                this.F4Selection(true);
            }
            else
            {
                this.UnfocusSelection();
            }
            return true;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            this.ClearCachedFontInfo();
            this.cachedRowHeight = -1;
            if ((!base.Disposing && (this.ParentInternal != null)) && !this.ParentInternal.Disposing)
            {
                this.fontBold = null;
                this.ToolTip.Font = this.Font;
                this.SetFlag(0x80, true);
                this.UpdateUIBasedOnFont(true);
                base.OnFontChanged(e);
                if (this.selectedGridEntry != null)
                {
                    this.SelectGridEntry(this.selectedGridEntry, true);
                }
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (((e != null) && !this.GetInPropertySet()) && !this.Commit())
            {
                this.Edit.FocusInternal();
            }
            else
            {
                if ((this.selectedGridEntry != null) && (this.GetRowFromGridEntry(this.selectedGridEntry) != -1))
                {
                    this.selectedGridEntry.Focus = true;
                    this.SelectGridEntry(this.selectedGridEntry, false);
                }
                else
                {
                    this.SelectRow(0);
                }
                if ((this.selectedGridEntry != null) && (this.selectedGridEntry.GetValueOwner() != null))
                {
                    this.UpdateHelpAttributes(null, this.selectedGridEntry);
                }
            }
        }

        private void OnGridEntryLabelClick(object s, EventArgs e)
        {
            this.lastClickedEntry = (GridEntry) s;
            this.SelectGridEntry(this.lastClickedEntry, true);
        }

        private void OnGridEntryLabelDoubleClick(object s, EventArgs e)
        {
            GridEntry gridEntry = (GridEntry) s;
            if (gridEntry == this.lastClickedEntry)
            {
                int rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
                this.DoubleClickRow(rowFromGridEntry, gridEntry.Expandable, 1);
            }
        }

        private void OnGridEntryOutlineClick(object s, EventArgs e)
        {
            GridEntry gridEntry = (GridEntry) s;
            Cursor cursor = this.Cursor;
            if (!this.ShouldSerializeCursor())
            {
                cursor = null;
            }
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.SetExpand(gridEntry, !gridEntry.InternalExpanded);
                this.SelectGridEntry(gridEntry, false);
            }
            finally
            {
                this.Cursor = cursor;
            }
        }

        private void OnGridEntryValueClick(object s, EventArgs e)
        {
            this.lastClickedEntry = (GridEntry) s;
            bool flag = s != this.selectedGridEntry;
            this.SelectGridEntry(this.lastClickedEntry, true);
            this.Edit.FocusInternal();
            if (this.lastMouseDown != InvalidPosition)
            {
                this.rowSelectTime = 0L;
                Point p = base.PointToScreen(this.lastMouseDown);
                p = this.Edit.PointToClientInternal(p);
                this.Edit.SendMessage(0x201, 0, (int) ((p.Y << 0x10) | (p.X & 0xffff)));
                this.Edit.SendMessage(0x202, 0, (int) ((p.Y << 0x10) | (p.X & 0xffff)));
            }
            if (flag)
            {
                this.rowSelectTime = DateTime.Now.Ticks;
                this.rowSelectPos = base.PointToScreen(this.lastMouseDown);
            }
            else
            {
                this.rowSelectTime = 0L;
                this.rowSelectPos = Point.Empty;
            }
        }

        private void OnGridEntryValueDoubleClick(object s, EventArgs e)
        {
            GridEntry gridEntry = (GridEntry) s;
            if (gridEntry == this.lastClickedEntry)
            {
                int rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
                this.DoubleClickRow(rowFromGridEntry, gridEntry.Expandable, 2);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnSysColorChange);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnSysColorChange);
            if ((this.toolTip != null) && !base.RecreatingHandle)
            {
                this.toolTip.Dispose();
                this.toolTip = null;
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            this.OnKeyDown(this, ke);
        }

        private void OnKeyDown(object sender, KeyEventArgs ke)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
            if (gridEntryFromRow == null)
            {
                return;
            }
            ke.Handled = true;
            bool control = ke.Control;
            bool shift = ke.Shift;
            bool flag3 = control && shift;
            bool alt = ke.Alt;
            Keys keyCode = ke.KeyCode;
            bool flag5 = false;
            if ((keyCode == Keys.Tab) && this.ProcessDialogKey(ke.KeyData))
            {
                ke.Handled = true;
                return;
            }
            if (((keyCode == Keys.Down) && alt) && this.DropDownButton.Visible)
            {
                this.F4Selection(false);
                return;
            }
            if ((((keyCode == Keys.Up) && alt) && (this.DropDownButton.Visible && (this.dropDownHolder != null))) && this.dropDownHolder.Visible)
            {
                this.UnfocusSelection();
                return;
            }
            if (this.ToolTip.Visible)
            {
                this.ToolTip.ToolTip = "";
            }
            if ((flag3 || (sender == this)) || (sender == this.ownerGrid))
            {
                switch (keyCode)
                {
                    case Keys.PageUp:
                    case Keys.Next:
                    {
                        bool flag7 = keyCode == Keys.Next;
                        int num3 = flag7 ? (this.visibleRows - 1) : (1 - this.visibleRows);
                        int selectedRow = this.selectedRow;
                        if (!control || shift)
                        {
                            if (this.selectedRow != -1)
                            {
                                int scrollOffset = this.GetScrollOffset();
                                this.SetScrollOffset(scrollOffset + num3);
                                this.SetConstants();
                                if (this.GetScrollOffset() != (scrollOffset + num3))
                                {
                                    if (flag7)
                                    {
                                        selectedRow = this.visibleRows - 1;
                                    }
                                    else
                                    {
                                        selectedRow = 0;
                                    }
                                }
                            }
                            this.SelectRow(selectedRow);
                            this.Refresh();
                            return;
                        }
                        return;
                    }
                    case Keys.End:
                    case Keys.Home:
                    {
                        GridEntryCollection allGridEntries = this.GetAllGridEntries();
                        int index = (keyCode == Keys.Home) ? 0 : (allGridEntries.Count - 1);
                        this.SelectGridEntry(allGridEntries.GetEntry(index), true);
                        return;
                    }
                    case Keys.Left:
                        if (!control)
                        {
                            if (gridEntryFromRow.InternalExpanded)
                            {
                                this.SetExpand(gridEntryFromRow, false);
                                return;
                            }
                            this.SelectGridEntry(this.GetGridEntryFromRow(this.selectedRow - 1), true);
                            return;
                        }
                        this.MoveSplitterTo(this.InternalLabelWidth - 3);
                        return;

                    case Keys.Up:
                    case Keys.Down:
                    {
                        int row = (keyCode == Keys.Up) ? (this.selectedRow - 1) : (this.selectedRow + 1);
                        this.SelectGridEntry(this.GetGridEntryFromRow(row), true);
                        this.SetFlag(0x200, false);
                        return;
                    }
                    case Keys.Right:
                        if (!control)
                        {
                            if (gridEntryFromRow.Expandable)
                            {
                                if (gridEntryFromRow.InternalExpanded)
                                {
                                    GridEntryCollection children = gridEntryFromRow.Children;
                                    this.SelectGridEntry(children.GetEntry(0), true);
                                    return;
                                }
                                this.SetExpand(gridEntryFromRow, true);
                                return;
                            }
                            this.SelectGridEntry(this.GetGridEntryFromRow(this.selectedRow + 1), true);
                            return;
                        }
                        this.MoveSplitterTo(this.InternalLabelWidth + 3);
                        return;

                    case Keys.Insert:
                        if ((!shift || control) || alt)
                        {
                            goto Label_03B6;
                        }
                        flag5 = true;
                        goto Label_0400;

                    case Keys.Delete:
                        if ((!shift || control) || alt)
                        {
                            break;
                        }
                        flag5 = true;
                        goto Label_03DA;

                    case Keys.D8:
                        if (!shift)
                        {
                            break;
                        }
                        goto Label_030C;

                    case Keys.Enter:
                        if (gridEntryFromRow.Expandable)
                        {
                            this.SetExpand(gridEntryFromRow, !gridEntryFromRow.InternalExpanded);
                            return;
                        }
                        gridEntryFromRow.OnValueReturnKey();
                        return;

                    case Keys.A:
                        if ((control && !alt) && (!shift && this.Edit.Visible))
                        {
                            this.Edit.FocusInternal();
                            this.Edit.SelectAll();
                        }
                        break;

                    case Keys.C:
                        goto Label_03B6;

                    case Keys.V:
                        goto Label_0400;

                    case Keys.X:
                        goto Label_03DA;

                    case Keys.Multiply:
                        goto Label_030C;

                    case Keys.Add:
                    case Keys.Subtract:
                    case Keys.Oemplus:
                    case Keys.OemMinus:
                    {
                        if (!gridEntryFromRow.Expandable)
                        {
                            break;
                        }
                        this.SetFlag(8, true);
                        bool flag6 = (keyCode == Keys.Add) || (keyCode == Keys.Oemplus);
                        this.SetExpand(gridEntryFromRow, flag6);
                        base.Invalidate();
                        ke.Handled = true;
                        return;
                    }
                }
            }
            goto Label_0444;
        Label_030C:
            this.SetFlag(8, true);
            this.RecursivelyExpand(gridEntryFromRow, true, true, 10);
            ke.Handled = false;
            return;
        Label_03B6:
            if ((!control || alt) || shift)
            {
                goto Label_0444;
            }
            this.DoCopyCommand();
            return;
        Label_03DA:
            if (!flag5 && ((!control || alt) || shift))
            {
                goto Label_0444;
            }
            Clipboard.SetDataObject(gridEntryFromRow.GetPropertyTextValue());
            this.CommitText("");
            return;
        Label_0400:
            if (flag5 || ((control && !alt) && !shift))
            {
                this.DoPasteCommand();
            }
        Label_0444:
            if ((gridEntryFromRow != null) && (ke.KeyData == (Keys.Alt | Keys.Control | Keys.Shift | Keys.C)))
            {
                Clipboard.SetDataObject(gridEntryFromRow.GetTestingInfo());
            }
            else
            {
                ke.Handled = false;
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs ke)
        {
            bool flag = false;
            bool flag2 = false;
            if (!(flag && flag2) && this.WillFilterKeyPress(ke.KeyChar))
            {
                this.FilterKeyPress(ke.KeyChar);
            }
            this.SetFlag(8, false);
        }

        private void OnListChange(object sender, EventArgs e)
        {
            if (!this.DropDownListBox.InSetSelectedIndex())
            {
                this.Edit.Text = this.GetGridEntryFromRow(this.selectedRow).GetPropertyTextValue(this.DropDownListBox.SelectedItem);
                this.Edit.FocusInternal();
                this.SelectEdit(false);
            }
            this.SetFlag(0x40, true);
        }

        private void OnListClick(object sender, EventArgs e)
        {
            this.GetGridEntryFromRow(this.selectedRow);
            if (this.DropDownListBox.Items.Count == 0)
            {
                this.CommonEditorHide();
                this.SetCommitError(0);
                this.SelectRow(this.selectedRow);
            }
            else
            {
                object selectedItem = this.DropDownListBox.SelectedItem;
                this.SetFlag(0x40, false);
                if ((selectedItem != null) && !this.CommitText((string) selectedItem))
                {
                    this.SetCommitError(0);
                    this.SelectRow(this.selectedRow);
                }
            }
        }

        private void OnListDrawItem(object sender, DrawItemEventArgs die)
        {
            if ((die.Index >= 0) && (this.selectedGridEntry != null))
            {
                string text = (string) this.DropDownListBox.Items[die.Index];
                die.DrawBackground();
                die.DrawFocusRectangle();
                Rectangle bounds = die.Bounds;
                bounds.Y++;
                bounds.X--;
                GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
                try
                {
                    this.DrawValue(die.Graphics, bounds, bounds, gridEntryFromRow, gridEntryFromRow.ConvertTextToValue(text), (die.State & DrawItemState.Selected) != DrawItemState.None, false, false, false);
                }
                catch (FormatException exception)
                {
                    this.ShowFormatExceptionMessage(gridEntryFromRow.PropertyLabel, text, exception);
                    if (this.DropDownListBox.IsHandleCreated)
                    {
                        this.DropDownListBox.Visible = false;
                    }
                }
            }
        }

        private void OnListKeyDown(object sender, KeyEventArgs ke)
        {
            if (ke.KeyCode == Keys.Enter)
            {
                this.OnListClick(null, null);
                if (this.selectedGridEntry != null)
                {
                    this.selectedGridEntry.OnValueReturnKey();
                }
            }
            this.OnKeyDown(sender, ke);
        }

        private void OnListMouseUp(object sender, MouseEventArgs me)
        {
            this.OnListClick(sender, me);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (e != null)
            {
                base.OnLostFocus(e);
            }
            if (this.FocusInside)
            {
                base.OnLostFocus(e);
            }
            else
            {
                GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
                if (gridEntryFromRow != null)
                {
                    gridEntryFromRow.Focus = false;
                    this.CommonEditorHide();
                    this.InvalidateRow(this.selectedRow);
                }
                base.OnLostFocus(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs me)
        {
            if (((me.Button == MouseButtons.Left) && this.SplitterInside(me.X, me.Y)) && (this.totalProps != 0))
            {
                if (this.Commit())
                {
                    if (me.Clicks == 2)
                    {
                        this.MoveSplitterTo(base.Width / 2);
                    }
                    else
                    {
                        this.UnfocusSelection();
                        this.SetFlag(4, true);
                        this.tipInfo = -1;
                        base.CaptureInternal = true;
                    }
                }
            }
            else
            {
                Point point = this.FindPosition(me.X, me.Y);
                if (point != InvalidPosition)
                {
                    GridEntry gridEntryFromRow = this.GetGridEntryFromRow(point.Y);
                    if (gridEntryFromRow != null)
                    {
                        Rectangle rectangle = this.GetRectangle(point.Y, 1);
                        this.lastMouseDown = new Point(me.X, me.Y);
                        if (me.Button == MouseButtons.Left)
                        {
                            gridEntryFromRow.OnMouseClick(me.X - rectangle.X, me.Y - rectangle.Y, me.Clicks, me.Button);
                        }
                        else
                        {
                            this.SelectGridEntry(gridEntryFromRow, false);
                        }
                        this.lastMouseDown = InvalidPosition;
                        gridEntryFromRow.Focus = true;
                        this.SetFlag(0x200, false);
                    }
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!this.GetFlag(4))
            {
                this.Cursor = Cursors.Default;
            }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs me)
        {
            int y;
            Point empty = Point.Empty;
            bool flag = false;
            if (me == null)
            {
                y = -1;
                empty = InvalidPosition;
            }
            else
            {
                empty = this.FindPosition(me.X, me.Y);
                if ((empty == InvalidPosition) || ((empty.X != 1) && (empty.X != 2)))
                {
                    y = -1;
                    this.ToolTip.ToolTip = "";
                }
                else
                {
                    y = empty.Y;
                    flag = empty.X == 1;
                }
            }
            if ((empty != InvalidPosition) && (me != null))
            {
                if (this.GetFlag(4))
                {
                    this.MoveSplitterTo(me.X);
                }
                if (((y != this.TipRow) || (empty.X != this.TipColumn)) && !this.GetFlag(4))
                {
                    GridEntry gridEntryFromRow = this.GetGridEntryFromRow(y);
                    string labelToolTipText = "";
                    this.tipInfo = -1;
                    if (gridEntryFromRow != null)
                    {
                        Rectangle rectangle = this.GetRectangle(empty.Y, empty.X);
                        if (flag && (gridEntryFromRow.GetLabelToolTipLocation(me.X - rectangle.X, me.Y - rectangle.Y) != InvalidPoint))
                        {
                            labelToolTipText = gridEntryFromRow.LabelToolTipText;
                            this.TipRow = y;
                            this.TipColumn = empty.X;
                        }
                        else if ((!flag && (gridEntryFromRow.ValueToolTipLocation != InvalidPoint)) && !this.Edit.Focused)
                        {
                            if (!this.NeedsCommit)
                            {
                                labelToolTipText = gridEntryFromRow.GetPropertyTextValue();
                            }
                            this.TipRow = y;
                            this.TipColumn = empty.X;
                        }
                    }
                    IntPtr foregroundWindow = System.Windows.Forms.UnsafeNativeMethods.GetForegroundWindow();
                    if (System.Windows.Forms.UnsafeNativeMethods.IsChild(new HandleRef(null, foregroundWindow), new HandleRef(null, base.Handle)))
                    {
                        if (((this.dropDownHolder == null) || (this.dropDownHolder.Component == null)) || (y == this.selectedRow))
                        {
                            this.ToolTip.ToolTip = labelToolTipText;
                        }
                    }
                    else
                    {
                        this.ToolTip.ToolTip = "";
                    }
                }
                if ((this.totalProps != 0) && (this.SplitterInside(me.X, me.Y) || this.GetFlag(4)))
                {
                    this.Cursor = Cursors.VSplit;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
                base.OnMouseMove(me);
            }
        }

        protected override void OnMouseUp(MouseEventArgs me)
        {
            this.CancelSplitterMove();
        }

        protected override void OnMouseWheel(MouseEventArgs me)
        {
            this.ownerGrid.OnGridViewMouseWheel(me);
            HandledMouseEventArgs args = me as HandledMouseEventArgs;
            if (args != null)
            {
                if (args.Handled)
                {
                    return;
                }
                args.Handled = true;
            }
            if (((Control.ModifierKeys & (Keys.Alt | Keys.Shift)) == Keys.None) && (Control.MouseButtons == MouseButtons.None))
            {
                int mouseWheelScrollLines = SystemInformation.MouseWheelScrollLines;
                if (mouseWheelScrollLines != 0)
                {
                    if (((this.selectedGridEntry != null) && this.selectedGridEntry.Enumerable) && (this.Edit.Focused && this.selectedGridEntry.IsValueEditable))
                    {
                        int currentValueIndex = this.GetCurrentValueIndex(this.selectedGridEntry);
                        if (currentValueIndex != -1)
                        {
                            int num3 = (me.Delta > 0) ? -1 : 1;
                            object[] propertyValueList = this.selectedGridEntry.GetPropertyValueList();
                            if ((num3 > 0) && (currentValueIndex >= (propertyValueList.Length - 1)))
                            {
                                currentValueIndex = 0;
                            }
                            else if ((num3 < 0) && (currentValueIndex == 0))
                            {
                                currentValueIndex = propertyValueList.Length - 1;
                            }
                            else
                            {
                                currentValueIndex += num3;
                            }
                            this.CommitValue(propertyValueList[currentValueIndex]);
                            this.SelectGridEntry(this.selectedGridEntry, true);
                            this.Edit.FocusInternal();
                            return;
                        }
                    }
                    int scrollOffset = this.GetScrollOffset();
                    this.cumulativeVerticalWheelDelta += me.Delta;
                    float num5 = ((float) this.cumulativeVerticalWheelDelta) / 120f;
                    int num6 = (int) num5;
                    if (mouseWheelScrollLines == -1)
                    {
                        if (num6 != 0)
                        {
                            int num7 = scrollOffset;
                            int num8 = num6 * this.scrollBar.LargeChange;
                            int newOffset = Math.Min(Math.Max(0, scrollOffset - num8), (this.totalProps - this.visibleRows) + 1);
                            scrollOffset -= num6 * this.scrollBar.LargeChange;
                            if (Math.Abs((int) (scrollOffset - num7)) >= Math.Abs((int) (num6 * this.scrollBar.LargeChange)))
                            {
                                this.cumulativeVerticalWheelDelta -= num6 * 120;
                            }
                            else
                            {
                                this.cumulativeVerticalWheelDelta = 0;
                            }
                            if (!this.ScrollRows(newOffset))
                            {
                                this.cumulativeVerticalWheelDelta = 0;
                            }
                        }
                    }
                    else
                    {
                        int num10 = (int) (mouseWheelScrollLines * num5);
                        if (num10 != 0)
                        {
                            if (this.ToolTip.Visible)
                            {
                                this.ToolTip.ToolTip = "";
                            }
                            int num11 = Math.Min(Math.Max(0, scrollOffset - num10), (this.totalProps - this.visibleRows) + 1);
                            if (num10 > 0)
                            {
                                if (this.scrollBar.Value <= this.scrollBar.Minimum)
                                {
                                    this.cumulativeVerticalWheelDelta = 0;
                                }
                                else
                                {
                                    this.cumulativeVerticalWheelDelta -= (int) (num10 * (120f / ((float) mouseWheelScrollLines)));
                                }
                            }
                            else if (this.scrollBar.Value > ((this.scrollBar.Maximum - this.visibleRows) + 1))
                            {
                                this.cumulativeVerticalWheelDelta = 0;
                            }
                            else
                            {
                                this.cumulativeVerticalWheelDelta -= (int) (num10 * (120f / ((float) mouseWheelScrollLines)));
                            }
                            if (!this.ScrollRows(num11))
                            {
                                this.cumulativeVerticalWheelDelta = 0;
                            }
                        }
                        else
                        {
                            this.cumulativeVerticalWheelDelta = 0;
                        }
                    }
                }
            }
        }

        protected override void OnMove(EventArgs e)
        {
            this.CloseDropDown();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            int y = 0;
            int num2 = 0;
            int num3 = this.visibleRows - 1;
            Rectangle clipRectangle = pe.ClipRectangle;
            clipRectangle.Inflate(0, 2);
            try
            {
                Size size = base.Size;
                Point point = this.FindPosition(clipRectangle.X, clipRectangle.Y);
                Point point2 = this.FindPosition(clipRectangle.X, clipRectangle.Y + clipRectangle.Height);
                if (point != InvalidPosition)
                {
                    num2 = Math.Max(0, point.Y);
                }
                if (point2 != InvalidPosition)
                {
                    num3 = point2.Y;
                }
                int num4 = Math.Min((int) (this.totalProps - this.GetScrollOffset()), (int) (1 + this.visibleRows));
                this.SetFlag(1, false);
                Size ourSize = this.GetOurSize();
                Point ptOurLocation = this.ptOurLocation;
                if (this.GetGridEntryFromRow(num4 - 1) == null)
                {
                    num4--;
                }
                if (this.totalProps > 0)
                {
                    num4 = Math.Min(num4, num3 + 1);
                    Pen pen = new Pen(this.ownerGrid.LineColor, (float) this.GetSplitterWidth()) {
                        DashStyle = DashStyle.Solid
                    };
                    g.DrawLine(pen, this.labelWidth, ptOurLocation.Y, this.labelWidth, (num4 * (this.RowHeight + 1)) + ptOurLocation.Y);
                    pen.Dispose();
                    Pen pen2 = new Pen(g.GetNearestColor(this.ownerGrid.LineColor));
                    int num5 = 0;
                    int num6 = ptOurLocation.X + ourSize.Width;
                    int x = ptOurLocation.X;
                    this.GetTotalWidth();
                    for (int i = num2; i < num4; i++)
                    {
                        try
                        {
                            num5 = (i * (this.RowHeight + 1)) + ptOurLocation.Y;
                            g.DrawLine(pen2, x, num5, num6, num5);
                            this.DrawValueEntry(g, i, ref clipRectangle);
                            Rectangle rectangle = this.GetRectangle(i, 1);
                            y = rectangle.Y + rectangle.Height;
                            this.DrawLabel(g, i, rectangle, i == this.selectedRow, false, ref clipRectangle);
                            if (i == this.selectedRow)
                            {
                                this.Edit.Invalidate();
                            }
                        }
                        catch
                        {
                        }
                    }
                    num5 = (num4 * (this.RowHeight + 1)) + ptOurLocation.Y;
                    g.DrawLine(pen2, x, num5, num6, num5);
                    pen2.Dispose();
                }
                if (y < base.Size.Height)
                {
                    y++;
                    Rectangle rect = new Rectangle(1, y, base.Size.Width - 2, (base.Size.Height - y) - 1);
                    g.FillRectangle(this.backgroundBrush, rect);
                }
                g.DrawRectangle(SystemPens.ControlDark, 0, 0, size.Width - 1, size.Height - 1);
                this.fontBold = null;
            }
            catch
            {
            }
            finally
            {
                this.ClearCachedFontInfo();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pe)
        {
        }

        protected virtual void OnRecreateChildren(object s, GridEntryRecreateChildrenEventArgs e)
        {
            GridEntry entry = (GridEntry) s;
            if (entry.Expanded)
            {
                GridEntry[] dest = new GridEntry[this.allGridEntries.Count];
                this.allGridEntries.CopyTo(dest, 0);
                int num = -1;
                for (int i = 0; i < dest.Length; i++)
                {
                    if (dest[i] == entry)
                    {
                        num = i;
                        break;
                    }
                }
                this.ClearGridEntryEvents(this.allGridEntries, num + 1, e.OldChildCount);
                if (e.OldChildCount != e.NewChildCount)
                {
                    int num3 = dest.Length + (e.NewChildCount - e.OldChildCount);
                    GridEntry[] destinationArray = new GridEntry[num3];
                    Array.Copy(dest, 0, destinationArray, 0, num + 1);
                    Array.Copy(dest, (int) ((num + e.OldChildCount) + 1), destinationArray, (int) ((num + e.NewChildCount) + 1), (int) (dest.Length - ((num + e.OldChildCount) + 1)));
                    dest = destinationArray;
                }
                GridEntryCollection children = entry.Children;
                int count = children.Count;
                for (int j = 0; j < count; j++)
                {
                    dest[(num + j) + 1] = children.GetEntry(j);
                }
                this.allGridEntries.Clear();
                this.allGridEntries.AddRange(dest);
                this.AddGridEntryEvents(this.allGridEntries, num + 1, count);
            }
            if (e.OldChildCount != e.NewChildCount)
            {
                this.totalProps = this.CountPropsFromOutline(this.topLevelGridEntries);
                this.SetConstants();
            }
            base.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            Rectangle clientRectangle = base.ClientRectangle;
            int num = (this.lastClientRect == Rectangle.Empty) ? 0 : (clientRectangle.Height - this.lastClientRect.Height);
            bool visible = this.ScrollBar.Visible;
            if (!this.lastClientRect.IsEmpty && (clientRectangle.Width > this.lastClientRect.Width))
            {
                Rectangle rc = new Rectangle(this.lastClientRect.Width - 1, 0, (clientRectangle.Width - this.lastClientRect.Width) + 1, this.lastClientRect.Height);
                base.Invalidate(rc);
            }
            if (!this.lastClientRect.IsEmpty && (num > 0))
            {
                Rectangle rectangle3 = new Rectangle(0, this.lastClientRect.Height - 1, this.lastClientRect.Width, (clientRectangle.Height - this.lastClientRect.Height) + 1);
                base.Invalidate(rectangle3);
            }
            int scrollOffset = this.GetScrollOffset();
            this.SetScrollOffset(0);
            this.SetConstants();
            this.SetScrollOffset(scrollOffset);
            this.CommonEditorHide();
            this.LayoutWindow(false);
            bool fPageIn = ((this.selectedGridEntry != null) && (this.selectedRow >= 0)) && (this.selectedRow <= this.visibleRows);
            this.SelectGridEntry(this.selectedGridEntry, fPageIn);
            this.lastClientRect = clientRectangle;
        }

        private void OnScroll(object sender, ScrollEventArgs se)
        {
            if (!this.Commit() || !this.IsScrollValueValid(se.NewValue))
            {
                se.NewValue = this.ScrollBar.Value;
            }
            else
            {
                int rowFromGridEntry = -1;
                GridEntry selectedGridEntry = this.selectedGridEntry;
                if (this.selectedGridEntry != null)
                {
                    rowFromGridEntry = this.GetRowFromGridEntry(selectedGridEntry);
                }
                this.ScrollBar.Value = se.NewValue;
                if (selectedGridEntry != null)
                {
                    this.selectedRow = -1;
                    this.SelectGridEntry(selectedGridEntry, this.ScrollBar.Value == this.totalProps);
                    int num2 = this.GetRowFromGridEntry(selectedGridEntry);
                    if (rowFromGridEntry != num2)
                    {
                        base.Invalidate();
                    }
                }
                else
                {
                    base.Invalidate();
                }
            }
        }

        private void OnSysColorChange(object sender, UserPreferenceChangedEventArgs e)
        {
            if ((e.Category == UserPreferenceCategory.Color) || (e.Category == UserPreferenceCategory.Accessibility))
            {
                this.SetFlag(0x80, true);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if ((!base.Disposing && (this.ParentInternal != null)) && !this.ParentInternal.Disposing)
            {
                if (base.Visible && (this.ParentInternal != null))
                {
                    this.SetConstants();
                    if (this.selectedGridEntry != null)
                    {
                        this.SelectGridEntry(this.selectedGridEntry, true);
                    }
                    if (this.toolTip != null)
                    {
                        this.ToolTip.Font = this.Font;
                    }
                }
                base.OnVisibleChanged(e);
            }
        }

        public virtual void PopupDialog(int row)
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(row);
            if (gridEntryFromRow != null)
            {
                if ((this.dropDownHolder != null) && this.dropDownHolder.GetUsed())
                {
                    this.CloseDropDown();
                }
                else
                {
                    bool needsDropDownButton = gridEntryFromRow.NeedsDropDownButton;
                    bool enumerable = gridEntryFromRow.Enumerable;
                    bool needsCustomEditorButton = gridEntryFromRow.NeedsCustomEditorButton;
                    if (enumerable && !needsDropDownButton)
                    {
                        this.DropDownListBox.Items.Clear();
                        object propertyValue = gridEntryFromRow.PropertyValue;
                        object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
                        int num = 0;
                        IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(new HandleRef(this.DropDownListBox, this.DropDownListBox.Handle));
                        IntPtr handle = this.Font.ToHfont();
                        System.Internal.HandleCollector.Add(handle, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
                        System.Windows.Forms.NativeMethods.TEXTMETRIC lptm = new System.Windows.Forms.NativeMethods.TEXTMETRIC();
                        int currentValueIndex = -1;
                        try
                        {
                            handle = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this.DropDownListBox, dC), new HandleRef(this.Font, handle));
                            currentValueIndex = this.GetCurrentValueIndex(gridEntryFromRow);
                            if ((propertyValueList != null) && (propertyValueList.Length > 0))
                            {
                                IntNativeMethods.SIZE size = new IntNativeMethods.SIZE();
                                for (int i = 0; i < propertyValueList.Length; i++)
                                {
                                    string propertyTextValue = gridEntryFromRow.GetPropertyTextValue(propertyValueList[i]);
                                    this.DropDownListBox.Items.Add(propertyTextValue);
                                    IntUnsafeNativeMethods.GetTextExtentPoint32(new HandleRef(this.DropDownListBox, dC), propertyTextValue, size);
                                    num = Math.Max(size.cx, num);
                                }
                            }
                            System.Windows.Forms.SafeNativeMethods.GetTextMetrics(new HandleRef(this.DropDownListBox, dC), ref lptm);
                            num += (2 + lptm.tmMaxCharWidth) + SystemInformation.VerticalScrollBarWidth;
                            handle = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(this.DropDownListBox, dC), new HandleRef(this.Font, handle));
                        }
                        finally
                        {
                            System.Windows.Forms.SafeNativeMethods.DeleteObject(new HandleRef(this.Font, handle));
                            System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(new HandleRef(this.DropDownListBox, this.DropDownListBox.Handle), new HandleRef(this.DropDownListBox, dC));
                        }
                        if (currentValueIndex != -1)
                        {
                            this.DropDownListBox.SelectedIndex = currentValueIndex;
                        }
                        this.SetFlag(0x40, false);
                        this.DropDownListBox.Height = Math.Max(lptm.tmHeight + 2, Math.Min(200, this.DropDownListBox.PreferredHeight));
                        this.DropDownListBox.Width = Math.Max(num, this.GetRectangle(row, 2).Width);
                        try
                        {
                            bool flag4 = this.DropDownListBox.Items.Count > (this.DropDownListBox.Height / this.DropDownListBox.ItemHeight);
                            this.SetFlag(0x400, flag4);
                            this.DropDownControl(this.DropDownListBox);
                        }
                        finally
                        {
                            this.SetFlag(0x400, false);
                        }
                        this.Refresh();
                    }
                    else if (needsCustomEditorButton || needsDropDownButton)
                    {
                        try
                        {
                            this.SetFlag(0x10, true);
                            this.Edit.DisableMouseHook = true;
                            try
                            {
                                this.SetFlag(0x400, gridEntryFromRow.UITypeEditor.IsDropDownResizable);
                                gridEntryFromRow.EditPropertyValue(this);
                            }
                            finally
                            {
                                this.SetFlag(0x400, false);
                            }
                        }
                        finally
                        {
                            this.SetFlag(0x10, false);
                            this.Edit.DisableMouseHook = false;
                        }
                        this.Refresh();
                        if (this.FocusInside)
                        {
                            this.SelectGridEntry(gridEntryFromRow, false);
                        }
                    }
                }
            }
        }

        internal static void PositionTooltip(Control parent, GridToolTip ToolTip, Rectangle itemRect)
        {
            ToolTip.Visible = false;
            System.Windows.Forms.NativeMethods.RECT lparam = System.Windows.Forms.NativeMethods.RECT.FromXYWH(itemRect.X, itemRect.Y, itemRect.Width, itemRect.Height);
            ToolTip.SendMessage(0x41f, 1, ref lparam);
            Point point = parent.PointToScreen(new Point(lparam.left, lparam.top));
            ToolTip.Location = point;
            int num = (ToolTip.Location.X + ToolTip.Size.Width) - SystemInformation.VirtualScreen.Width;
            if (num > 0)
            {
                point.X -= num;
                ToolTip.Location = point;
            }
            ToolTip.Visible = true;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (this.HasEntries)
            {
                Keys keys2 = keyData & Keys.KeyCode;
                if (keys2 <= Keys.Enter)
                {
                    switch (keys2)
                    {
                        case Keys.Tab:
                        {
                            if (((keyData & Keys.Control) != Keys.None) || ((keyData & Keys.Alt) != Keys.None))
                            {
                                break;
                            }
                            bool flag = (keyData & Keys.Shift) == Keys.None;
                            Control c = Control.FromHandleInternal(System.Windows.Forms.UnsafeNativeMethods.GetFocus());
                            if ((c == null) || !this.IsMyChild(c))
                            {
                                if (flag)
                                {
                                    this.TabSelection();
                                    c = Control.FromHandleInternal(System.Windows.Forms.UnsafeNativeMethods.GetFocus());
                                    return (this.IsMyChild(c) || base.ProcessDialogKey(keyData));
                                }
                                break;
                            }
                            if (this.Edit.Focused)
                            {
                                if (!flag)
                                {
                                    this.SelectGridEntry(this.GetGridEntryFromRow(this.selectedRow), false);
                                    return true;
                                }
                                if (this.DropDownButton.Visible)
                                {
                                    this.DropDownButton.FocusInternal();
                                    return true;
                                }
                                if (this.DialogButton.Visible)
                                {
                                    this.DialogButton.FocusInternal();
                                    return true;
                                }
                                break;
                            }
                            if ((!this.DialogButton.Focused && !this.DropDownButton.Focused) || (flag || !this.Edit.Visible))
                            {
                                break;
                            }
                            this.Edit.FocusInternal();
                            return true;
                        }
                        case Keys.Enter:
                            if (this.DialogButton.Focused || this.DropDownButton.Focused)
                            {
                                this.OnBtnClick(this.DialogButton.Focused ? this.DialogButton : this.DropDownButton, new EventArgs());
                                return true;
                            }
                            if ((this.selectedGridEntry != null) && this.selectedGridEntry.Expandable)
                            {
                                this.SetExpand(this.selectedGridEntry, !this.selectedGridEntry.InternalExpanded);
                                return true;
                            }
                            break;
                    }
                }
                else
                {
                    switch (keys2)
                    {
                        case Keys.Left:
                        case Keys.Up:
                        case Keys.Right:
                        case Keys.Down:
                            return false;

                        default:
                            if ((keys2 != Keys.F4) || !this.FocusInside)
                            {
                                break;
                            }
                            return this.OnF4(this);
                    }
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected virtual void RecalculateProps()
        {
            int num = this.CountPropsFromOutline(this.topLevelGridEntries);
            if (this.totalProps != num)
            {
                this.totalProps = num;
                this.ClearGridEntryEvents(this.allGridEntries, 0, -1);
                this.allGridEntries = null;
            }
        }

        internal void RecursivelyExpand(GridEntry gridEntry, bool fInit, bool expand, int maxExpands)
        {
            if ((gridEntry != null) && (!expand || (--maxExpands >= 0)))
            {
                this.SetExpand(gridEntry, expand);
                GridEntryCollection children = gridEntry.Children;
                if (children != null)
                {
                    for (int i = 0; i < children.Count; i++)
                    {
                        this.RecursivelyExpand(children.GetEntry(i), false, expand, maxExpands);
                    }
                }
                if (fInit)
                {
                    GridEntry selectedGridEntry = this.selectedGridEntry;
                    this.Refresh();
                    this.SelectGridEntry(selectedGridEntry, false);
                    base.Invalidate();
                }
            }
        }

        public override void Refresh()
        {
            this.Refresh(false, -1, -1);
            base.Invalidate();
        }

        public void Refresh(bool fullRefresh)
        {
            this.Refresh(fullRefresh, -1, -1);
        }

        private void Refresh(bool fullRefresh, int rowStart, int rowEnd)
        {
            this.SetFlag(1, true);
            GridEntry gridEntry = null;
            if (!base.IsDisposed)
            {
                bool fPageIn = true;
                if (rowStart == -1)
                {
                    rowStart = 0;
                }
                if (fullRefresh || this.ownerGrid.HavePropEntriesChanged())
                {
                    if ((this.HasEntries && !this.GetInPropertySet()) && !this.Commit())
                    {
                        this.OnEscape(this);
                    }
                    int totalProps = this.totalProps;
                    object obj2 = ((this.topLevelGridEntries == null) || (this.topLevelGridEntries.Count == 0)) ? null : ((GridEntry) this.topLevelGridEntries[0]).GetValueOwner();
                    if (fullRefresh)
                    {
                        this.ownerGrid.RefreshProperties(true);
                    }
                    if ((totalProps > 0) && !this.GetFlag(0x200))
                    {
                        this.positionData = this.CaptureGridPositionData();
                        this.CommonEditorHide(true);
                    }
                    this.UpdateHelpAttributes(this.selectedGridEntry, null);
                    this.selectedGridEntry = null;
                    this.SetFlag(2, true);
                    this.topLevelGridEntries = this.ownerGrid.GetPropEntries();
                    this.ClearGridEntryEvents(this.allGridEntries, 0, -1);
                    this.allGridEntries = null;
                    this.RecalculateProps();
                    int num2 = this.totalProps;
                    if (num2 > 0)
                    {
                        if (num2 < totalProps)
                        {
                            this.SetScrollbarLength();
                            this.SetScrollOffset(0);
                        }
                        this.SetConstants();
                        if (this.positionData != null)
                        {
                            gridEntry = this.positionData.Restore(this);
                            object obj3 = ((this.topLevelGridEntries == null) || (this.topLevelGridEntries.Count == 0)) ? null : ((GridEntry) this.topLevelGridEntries[0]).GetValueOwner();
                            fPageIn = ((gridEntry == null) || (totalProps != num2)) || (obj3 != obj2);
                        }
                        if (gridEntry == null)
                        {
                            gridEntry = this.ownerGrid.GetDefaultGridEntry();
                            this.SetFlag(0x200, (gridEntry == null) && (this.totalProps > 0));
                        }
                        this.InvalidateRows(rowStart, rowEnd);
                        if (gridEntry == null)
                        {
                            this.selectedRow = 0;
                            this.selectedGridEntry = this.GetGridEntryFromRow(this.selectedRow);
                        }
                    }
                    else
                    {
                        if (totalProps == 0)
                        {
                            return;
                        }
                        this.SetConstants();
                    }
                    this.positionData = null;
                    this.lastClickedEntry = null;
                }
                if (!this.HasEntries)
                {
                    this.CommonEditorHide(this.selectedRow != -1);
                    this.ownerGrid.SetStatusBox(null, null);
                    this.SetScrollOffset(0);
                    this.selectedRow = -1;
                    base.Invalidate();
                }
                else
                {
                    this.ownerGrid.ClearValueCaches();
                    this.InvalidateRows(rowStart, rowEnd);
                    if (gridEntry != null)
                    {
                        this.SelectGridEntry(gridEntry, fPageIn);
                    }
                }
            }
        }

        internal void RemoveSelectedEntryHelpAttributes()
        {
            this.UpdateHelpAttributes(this.selectedGridEntry, null);
        }

        public virtual void Reset()
        {
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(this.selectedRow);
            if (gridEntryFromRow != null)
            {
                gridEntryFromRow.ResetPropertyValue();
                this.SelectRow(this.selectedRow);
            }
        }

        protected virtual void ResetOrigin(Graphics g)
        {
            g.ResetTransform();
        }

        internal void RestoreHierarchyState(ArrayList expandedItems)
        {
            if (expandedItems != null)
            {
                foreach (GridEntryCollection entrys in expandedItems)
                {
                    this.FindEquivalentGridEntry(entrys);
                }
            }
        }

        internal void ReverseFocus()
        {
            if (this.selectedGridEntry == null)
            {
                this.FocusInternal();
            }
            else
            {
                this.SelectGridEntry(this.selectedGridEntry, true);
                if (this.DialogButton.Visible)
                {
                    this.DialogButton.FocusInternal();
                }
                else if (this.DropDownButton.Visible)
                {
                    this.DropDownButton.FocusInternal();
                }
                else if (this.Edit.Visible)
                {
                    this.Edit.SelectAll();
                    this.Edit.FocusInternal();
                }
            }
        }

        public virtual DialogResult RunDialog(Form dialog)
        {
            return this.ShowDialog(dialog);
        }

        internal ArrayList SaveHierarchyState(GridEntryCollection entries)
        {
            return this.SaveHierarchyState(entries, null);
        }

        private ArrayList SaveHierarchyState(GridEntryCollection entries, ArrayList expandedItems)
        {
            if (entries == null)
            {
                return new ArrayList();
            }
            if (expandedItems == null)
            {
                expandedItems = new ArrayList();
            }
            for (int i = 0; i < entries.Count; i++)
            {
                if (((GridEntry) entries[i]).InternalExpanded)
                {
                    GridEntry entry = entries.GetEntry(i);
                    expandedItems.Add(this.GetGridEntryHierarchy(entry.Children.GetEntry(0)));
                    this.SaveHierarchyState(entry.Children, expandedItems);
                }
            }
            return expandedItems;
        }

        private bool ScrollRows(int newOffset)
        {
            GridEntry selectedGridEntry = this.selectedGridEntry;
            if (!this.IsScrollValueValid(newOffset) || !this.Commit())
            {
                return false;
            }
            bool visible = this.Edit.Visible;
            bool flag2 = this.DropDownButton.Visible;
            bool flag3 = this.DialogButton.Visible;
            this.Edit.Visible = false;
            this.DialogButton.Visible = false;
            this.DropDownButton.Visible = false;
            this.SetScrollOffset(newOffset);
            if (selectedGridEntry != null)
            {
                int rowFromGridEntry = this.GetRowFromGridEntry(selectedGridEntry);
                if ((rowFromGridEntry >= 0) && (rowFromGridEntry < (this.visibleRows - 1)))
                {
                    this.Edit.Visible = visible;
                    this.DialogButton.Visible = flag3;
                    this.DropDownButton.Visible = flag2;
                    this.SelectGridEntry(selectedGridEntry, true);
                }
                else
                {
                    this.CommonEditorHide();
                }
            }
            else
            {
                this.CommonEditorHide();
            }
            base.Invalidate();
            return true;
        }

        private void SelectEdit(bool caretAtEnd)
        {
            if (this.edit != null)
            {
                this.Edit.SelectAll();
            }
        }

        internal void SelectGridEntry(GridEntry gridEntry, bool fPageIn)
        {
            if (gridEntry != null)
            {
                int rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
                if ((rowFromGridEntry + this.GetScrollOffset()) >= 0)
                {
                    int num2 = (int) Math.Ceiling((double) (((double) this.GetOurSize().Height) / ((double) (1 + this.RowHeight))));
                    if (!fPageIn || ((rowFromGridEntry >= 0) && (rowFromGridEntry < (num2 - 1))))
                    {
                        this.SelectRow(rowFromGridEntry);
                    }
                    else
                    {
                        this.selectedRow = -1;
                        int scrollOffset = this.GetScrollOffset();
                        if (rowFromGridEntry < 0)
                        {
                            this.SetScrollOffset(rowFromGridEntry + scrollOffset);
                            base.Invalidate();
                            this.SelectRow(0);
                        }
                        else
                        {
                            int cOffset = (rowFromGridEntry + scrollOffset) - (num2 - 2);
                            if ((cOffset >= this.ScrollBar.Minimum) && (cOffset < this.ScrollBar.Maximum))
                            {
                                this.SetScrollOffset(cOffset);
                            }
                            base.Invalidate();
                            this.SelectGridEntry(gridEntry, false);
                        }
                    }
                }
            }
        }

        private void SelectRow(int row)
        {
            if (!this.GetFlag(2))
            {
                if (this.FocusInside)
                {
                    if ((this.errorState != 0) || ((row != this.selectedRow) && !this.Commit()))
                    {
                        return;
                    }
                }
                else
                {
                    this.FocusInternal();
                }
            }
            GridEntry gridEntryFromRow = this.GetGridEntryFromRow(row);
            if (row != this.selectedRow)
            {
                this.UpdateResetCommand(gridEntryFromRow);
            }
            if (this.GetFlag(2) && (this.GetGridEntryFromRow(this.selectedRow) == null))
            {
                this.CommonEditorHide();
            }
            this.UpdateHelpAttributes(this.selectedGridEntry, gridEntryFromRow);
            if (this.selectedGridEntry != null)
            {
                this.selectedGridEntry.Focus = false;
            }
            if ((row < 0) || (row >= this.visibleRows))
            {
                this.CommonEditorHide();
                this.selectedRow = row;
                this.selectedGridEntry = gridEntryFromRow;
                this.Refresh();
            }
            else if (gridEntryFromRow != null)
            {
                bool flag = false;
                int selectedRow = this.selectedRow;
                if ((this.selectedRow != row) || !gridEntryFromRow.Equals(this.selectedGridEntry))
                {
                    this.CommonEditorHide();
                    flag = true;
                }
                if (!flag)
                {
                    this.CloseDropDown();
                }
                Rectangle rectTarget = this.GetRectangle(row, 2);
                string propertyTextValue = gridEntryFromRow.GetPropertyTextValue();
                bool flag2 = gridEntryFromRow.NeedsDropDownButton | gridEntryFromRow.Enumerable;
                bool needsCustomEditorButton = gridEntryFromRow.NeedsCustomEditorButton;
                bool isTextEditable = gridEntryFromRow.IsTextEditable;
                bool isCustomPaint = gridEntryFromRow.IsCustomPaint;
                rectTarget.X++;
                rectTarget.Width--;
                if ((needsCustomEditorButton || flag2) && (!gridEntryFromRow.ShouldRenderReadOnly && this.FocusInside))
                {
                    Control ctl = flag2 ? this.DropDownButton : this.DialogButton;
                    Size size = new Size(SystemInformation.VerticalScrollBarArrowHeight, this.RowHeight);
                    Rectangle rectangle2 = new Rectangle((rectTarget.X + rectTarget.Width) - size.Width, rectTarget.Y, size.Width, rectTarget.Height);
                    this.CommonEditorUse(ctl, rectangle2);
                    size = ctl.Size;
                    rectTarget.Width -= size.Width;
                    ctl.Invalidate();
                }
                if (isCustomPaint)
                {
                    rectTarget.X += 0x1b;
                    rectTarget.Width -= 0x1b;
                }
                else
                {
                    rectTarget.X++;
                    rectTarget.Width--;
                }
                if ((this.GetFlag(2) || !this.Edit.Focused) && ((propertyTextValue != null) && !propertyTextValue.Equals(this.Edit.Text)))
                {
                    this.Edit.Text = propertyTextValue;
                    this.originalTextValue = propertyTextValue;
                    this.Edit.SelectionStart = 0;
                    this.Edit.SelectionLength = 0;
                }
                this.Edit.AccessibleName = gridEntryFromRow.Label;
                switch (inheritRenderMode)
                {
                    case 2:
                        if (gridEntryFromRow.ShouldSerializePropertyValue())
                        {
                            rectTarget.X += 8;
                            rectTarget.Width -= 8;
                        }
                        break;

                    case 3:
                        if (!gridEntryFromRow.ShouldSerializePropertyValue())
                        {
                            this.Edit.Font = this.Font;
                            break;
                        }
                        this.Edit.Font = this.GetBoldFont();
                        break;
                }
                if ((this.GetFlag(4) || !gridEntryFromRow.HasValue) || !this.FocusInside)
                {
                    this.Edit.Visible = false;
                }
                else
                {
                    rectTarget.Offset(1, 1);
                    rectTarget.Height--;
                    rectTarget.Width--;
                    this.CommonEditorUse(this.Edit, rectTarget);
                    bool shouldRenderReadOnly = gridEntryFromRow.ShouldRenderReadOnly;
                    this.Edit.ForeColor = shouldRenderReadOnly ? this.GrayTextColor : this.ForeColor;
                    this.Edit.BackColor = this.BackColor;
                    this.Edit.ReadOnly = shouldRenderReadOnly || !gridEntryFromRow.IsTextEditable;
                    this.Edit.UseSystemPasswordChar = gridEntryFromRow.ShouldRenderPassword;
                }
                GridEntry selectedGridEntry = this.selectedGridEntry;
                this.selectedRow = row;
                this.selectedGridEntry = gridEntryFromRow;
                this.ownerGrid.SetStatusBox(gridEntryFromRow.PropertyLabel, gridEntryFromRow.PropertyDescription);
                if (this.selectedGridEntry != null)
                {
                    this.selectedGridEntry.Focus = this.FocusInside;
                }
                if (!this.GetFlag(2))
                {
                    this.FocusInternal();
                }
                this.InvalidateRow(selectedRow);
                this.InvalidateRow(row);
                if (this.FocusInside)
                {
                    this.SetFlag(2, false);
                }
                try
                {
                    if (this.selectedGridEntry != selectedGridEntry)
                    {
                        this.ownerGrid.OnSelectedGridItemChanged(selectedGridEntry, this.selectedGridEntry);
                    }
                }
                catch
                {
                }
            }
        }

        private void SetCommitError(short error)
        {
            this.SetCommitError(error, error == 1);
        }

        private void SetCommitError(short error, bool capture)
        {
            this.errorState = error;
            if (error != 0)
            {
                this.CancelSplitterMove();
            }
            this.Edit.HookMouseDown = capture;
        }

        public virtual void SetConstants()
        {
            Size ourSize = this.GetOurSize();
            this.visibleRows = (int) Math.Ceiling((double) (((double) ourSize.Height) / ((double) (1 + this.RowHeight))));
            ourSize = this.GetOurSize();
            if (ourSize.Width >= 0)
            {
                this.labelRatio = Math.Max(Math.Min(this.labelRatio, 9.0), 1.1);
                this.labelWidth = this.ptOurLocation.X + ((int) (((double) ourSize.Width) / this.labelRatio));
            }
            int labelWidth = this.labelWidth;
            bool flag = this.SetScrollbarLength();
            GridEntryCollection allGridEntries = this.GetAllGridEntries();
            if (allGridEntries != null)
            {
                int scrollOffset = this.GetScrollOffset();
                if ((scrollOffset + this.visibleRows) >= allGridEntries.Count)
                {
                    this.visibleRows = allGridEntries.Count - scrollOffset;
                }
            }
            if (flag && (ourSize.Width >= 0))
            {
                this.labelRatio = ((double) this.GetOurSize().Width) / ((double) (labelWidth - this.ptOurLocation.X));
            }
        }

        internal void SetExpand(GridEntry gridEntry, bool value)
        {
            if ((gridEntry != null) && gridEntry.Expandable)
            {
                int rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
                int selectedRow = this.selectedRow;
                if (((this.selectedRow != -1) && (rowFromGridEntry < this.selectedRow)) && this.Edit.Visible)
                {
                    this.FocusInternal();
                }
                int scrollOffset = this.GetScrollOffset();
                int totalProps = this.totalProps;
                gridEntry.InternalExpanded = value;
                this.RecalculateProps();
                GridEntry selectedGridEntry = this.selectedGridEntry;
                if (!value)
                {
                    for (GridEntry entry2 = selectedGridEntry; entry2 != null; entry2 = entry2.ParentGridEntry)
                    {
                        if (entry2.Equals(gridEntry))
                        {
                            selectedGridEntry = gridEntry;
                        }
                    }
                }
                rowFromGridEntry = this.GetRowFromGridEntry(gridEntry);
                this.SetConstants();
                int num5 = this.totalProps - totalProps;
                if (((value && (num5 > 0)) && ((num5 < this.visibleRows) && ((rowFromGridEntry + num5) >= this.visibleRows))) && (num5 < selectedRow))
                {
                    this.SetScrollOffset((this.totalProps - totalProps) + scrollOffset);
                }
                base.Invalidate();
                this.SelectGridEntry(selectedGridEntry, false);
                int cOffset = this.GetScrollOffset();
                this.SetScrollOffset(0);
                this.SetConstants();
                this.SetScrollOffset(cOffset);
            }
        }

        private void SetFlag(short flag, bool value)
        {
            if (value)
            {
                this.flags = (short) (((ushort) this.flags) | ((ushort) flag));
            }
            else
            {
                this.flags = (short) (this.flags & ~flag);
            }
        }

        private bool SetScrollbarLength()
        {
            bool flag = false;
            if (this.totalProps != -1)
            {
                if (this.totalProps < this.visibleRows)
                {
                    this.SetScrollOffset(0);
                }
                else if (this.GetScrollOffset() > this.totalProps)
                {
                    this.SetScrollOffset((this.totalProps + 1) - this.visibleRows);
                }
                bool flag2 = !this.ScrollBar.Visible;
                if (this.visibleRows > 0)
                {
                    this.ScrollBar.LargeChange = this.visibleRows - 1;
                }
                this.ScrollBar.Maximum = Math.Max(0, this.totalProps - 1);
                if (flag2 == (this.totalProps < this.visibleRows))
                {
                    return flag;
                }
                flag = true;
                this.ScrollBar.Visible = flag2;
                Size ourSize = this.GetOurSize();
                if ((this.labelWidth != -1) && (ourSize.Width > 0))
                {
                    if (this.labelWidth > (this.ptOurLocation.X + ourSize.Width))
                    {
                        this.labelWidth = this.ptOurLocation.X + ((int) (((double) ourSize.Width) / this.labelRatio));
                    }
                    else
                    {
                        this.labelRatio = ((double) this.GetOurSize().Width) / ((double) (this.labelWidth - this.ptOurLocation.X));
                    }
                }
                base.Invalidate();
            }
            return flag;
        }

        public virtual void SetScrollOffset(int cOffset)
        {
            int newValue = Math.Max(0, Math.Min((this.totalProps - this.visibleRows) + 1, cOffset));
            int num2 = this.ScrollBar.Value;
            if (((newValue != num2) && this.IsScrollValueValid(newValue)) && (this.visibleRows > 0))
            {
                this.ScrollBar.Value = newValue;
                base.Invalidate();
                this.selectedRow = this.GetRowFromGridEntry(this.selectedGridEntry);
            }
        }

        public DialogResult ShowDialog(Form dialog)
        {
            DialogResult result;
            if (dialog.StartPosition == FormStartPosition.CenterScreen)
            {
                Control parentInternal = this;
                if (parentInternal != null)
                {
                    while (parentInternal.ParentInternal != null)
                    {
                        parentInternal = parentInternal.ParentInternal;
                    }
                    if (parentInternal.Size.Equals(dialog.Size))
                    {
                        dialog.StartPosition = FormStartPosition.Manual;
                        Point location = parentInternal.Location;
                        location.Offset(0x19, 0x19);
                        dialog.Location = location;
                    }
                }
            }
            IntPtr focus = System.Windows.Forms.UnsafeNativeMethods.GetFocus();
            IUIService service = (IUIService) this.GetService(typeof(IUIService));
            if (service != null)
            {
                result = service.ShowDialog(dialog);
            }
            else
            {
                result = dialog.ShowDialog(this);
            }
            if (focus != IntPtr.Zero)
            {
                System.Windows.Forms.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
            }
            return result;
        }

        private void ShowFormatExceptionMessage(string propName, object value, Exception ex)
        {
            if (value == null)
            {
                value = "(null)";
            }
            if (propName == null)
            {
                propName = "(unknown)";
            }
            bool hookMouseDown = this.Edit.HookMouseDown;
            this.Edit.DisableMouseHook = true;
            this.SetCommitError(2, false);
            System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
            while (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0x200, 0x20a, 1))
            {
            }
            if (ex is TargetInvocationException)
            {
                ex = ex.InnerException;
            }
            string message = ex.Message;
            bool flag2 = false;
            while ((message == null) || (message.Length == 0))
            {
                ex = ex.InnerException;
                if (ex == null)
                {
                    break;
                }
                message = ex.Message;
            }
            IUIService service = (IUIService) this.GetService(typeof(IUIService));
            this.ErrorDialog.Message = System.Windows.Forms.SR.GetString("PBRSFormatExceptionMessage");
            this.ErrorDialog.Text = System.Windows.Forms.SR.GetString("PBRSErrorTitle");
            this.ErrorDialog.Details = message;
            if (service != null)
            {
                flag2 = DialogResult.Cancel == service.ShowDialog(this.ErrorDialog);
            }
            else
            {
                flag2 = DialogResult.Cancel == this.ShowDialog(this.ErrorDialog);
            }
            this.Edit.DisableMouseHook = false;
            if (hookMouseDown)
            {
                this.SelectGridEntry(this.selectedGridEntry, true);
            }
            this.SetCommitError(1, hookMouseDown);
            if (flag2)
            {
                this.OnEscape(this.Edit);
            }
        }

        private void ShowInvalidMessage(string propName, object value, Exception ex)
        {
            if (value == null)
            {
                value = "(null)";
            }
            if (propName == null)
            {
                propName = "(unknown)";
            }
            bool hookMouseDown = this.Edit.HookMouseDown;
            this.Edit.DisableMouseHook = true;
            this.SetCommitError(2, false);
            System.Windows.Forms.NativeMethods.MSG msg = new System.Windows.Forms.NativeMethods.MSG();
            while (System.Windows.Forms.UnsafeNativeMethods.PeekMessage(ref msg, System.Windows.Forms.NativeMethods.NullHandleRef, 0x200, 0x20a, 1))
            {
            }
            if (ex is TargetInvocationException)
            {
                ex = ex.InnerException;
            }
            string message = ex.Message;
            bool flag2 = false;
            while ((message == null) || (message.Length == 0))
            {
                ex = ex.InnerException;
                if (ex == null)
                {
                    break;
                }
                message = ex.Message;
            }
            IUIService service = (IUIService) this.GetService(typeof(IUIService));
            this.ErrorDialog.Message = System.Windows.Forms.SR.GetString("PBRSErrorInvalidPropertyValue");
            this.ErrorDialog.Text = System.Windows.Forms.SR.GetString("PBRSErrorTitle");
            this.ErrorDialog.Details = message;
            if (service != null)
            {
                flag2 = DialogResult.Cancel == service.ShowDialog(this.ErrorDialog);
            }
            else
            {
                flag2 = DialogResult.Cancel == this.ShowDialog(this.ErrorDialog);
            }
            this.Edit.DisableMouseHook = false;
            if (hookMouseDown)
            {
                this.SelectGridEntry(this.selectedGridEntry, true);
            }
            this.SetCommitError(1, hookMouseDown);
            if (flag2)
            {
                this.OnEscape(this.Edit);
            }
        }

        private bool SplitterInside(int x, int y)
        {
            return (Math.Abs((int) (x - this.InternalLabelWidth)) < 4);
        }

        private void TabSelection()
        {
            if (this.GetGridEntryFromRow(this.selectedRow) != null)
            {
                if (this.Edit.Visible)
                {
                    this.Edit.FocusInternal();
                    this.SelectEdit(false);
                }
                else if ((this.dropDownHolder != null) && this.dropDownHolder.Visible)
                {
                    this.dropDownHolder.FocusComponent();
                }
                else if (this.currentEditor != null)
                {
                    this.currentEditor.FocusInternal();
                }
            }
        }

        private bool UnfocusSelection()
        {
            if (this.GetGridEntryFromRow(this.selectedRow) == null)
            {
                return true;
            }
            bool flag = this.Commit();
            if (flag && this.FocusInside)
            {
                this.FocusInternal();
            }
            return flag;
        }

        private void UpdateHelpAttributes(GridEntry oldEntry, GridEntry newEntry)
        {
            IHelpService helpService = this.GetHelpService();
            if ((helpService != null) && (oldEntry != newEntry))
            {
                GridEntry parentGridEntry = oldEntry;
                if ((oldEntry != null) && !oldEntry.Disposed)
                {
                    while (parentGridEntry != null)
                    {
                        helpService.RemoveContextAttribute("Keyword", parentGridEntry.HelpKeyword);
                        parentGridEntry = parentGridEntry.ParentGridEntry;
                    }
                }
                if (newEntry != null)
                {
                    parentGridEntry = newEntry;
                    this.UpdateHelpAttributes(helpService, parentGridEntry, true);
                }
            }
        }

        private void UpdateHelpAttributes(IHelpService helpSvc, GridEntry entry, bool addAsF1)
        {
            if (entry != null)
            {
                this.UpdateHelpAttributes(helpSvc, entry.ParentGridEntry, false);
                string helpKeyword = entry.HelpKeyword;
                if (helpKeyword != null)
                {
                    helpSvc.AddContextAttribute("Keyword", helpKeyword, addAsF1 ? HelpKeywordType.F1Keyword : HelpKeywordType.GeneralKeyword);
                }
            }
        }

        private void UpdateResetCommand(GridEntry gridEntry)
        {
            if (this.totalProps > 0)
            {
                IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                if (service != null)
                {
                    MenuCommand command = service.FindCommand(PropertyGridCommands.Reset);
                    if (command != null)
                    {
                        command.Enabled = (gridEntry != null) && gridEntry.CanResetPropertyValue();
                    }
                }
            }
        }

        private void UpdateUIBasedOnFont(bool layoutRequired)
        {
            if (base.IsHandleCreated && this.GetFlag(0x80))
            {
                try
                {
                    if (this.listBox != null)
                    {
                        this.DropDownListBox.ItemHeight = this.RowHeight + 2;
                    }
                    if (this.btnDropDown != null)
                    {
                        this.btnDropDown.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, this.RowHeight);
                        if (this.btnDialog != null)
                        {
                            this.DialogButton.Size = this.DropDownButton.Size;
                        }
                    }
                    if (layoutRequired)
                    {
                        this.LayoutWindow(true);
                    }
                }
                finally
                {
                    this.SetFlag(0x80, false);
                }
            }
        }

        internal bool WantsTab(bool forward)
        {
            if (forward)
            {
                if (this.Focused)
                {
                    if ((this.DropDownButton.Visible || this.DialogButton.Visible) || this.Edit.Visible)
                    {
                        return true;
                    }
                }
                else if (this.Edit.Focused && (this.DropDownButton.Visible || this.DialogButton.Visible))
                {
                    return true;
                }
                return this.ownerGrid.WantsTab(forward);
            }
            if ((!this.Edit.Focused && !this.DropDownButton.Focused) && !this.DialogButton.Focused)
            {
                return this.ownerGrid.WantsTab(forward);
            }
            return true;
        }

        public virtual bool WillFilterKeyPress(char charPressed)
        {
            if (!this.Edit.Visible)
            {
                return false;
            }
            if ((Control.ModifierKeys & ~Keys.Shift) != Keys.None)
            {
                return false;
            }
            if (this.selectedGridEntry != null)
            {
                switch (charPressed)
                {
                    case '*':
                    case '+':
                    case '-':
                        return !this.selectedGridEntry.Expandable;

                    case '\t':
                        return false;
                }
            }
            return true;
        }

        private unsafe bool WmNotify(ref Message m)
        {
            if (m.LParam != IntPtr.Zero)
            {
                System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
                if (lParam->hwndFrom == this.ToolTip.Handle)
                {
                    switch (lParam->code)
                    {
                        case -521:
                        {
                            Point position = Cursor.Position;
                            position = base.PointToClientInternal(position);
                            position = this.FindPosition(position.X, position.Y);
                            if (position != InvalidPosition)
                            {
                                GridEntry gridEntryFromRow = this.GetGridEntryFromRow(position.Y);
                                if (gridEntryFromRow == null)
                                {
                                    break;
                                }
                                Rectangle itemRect = this.GetRectangle(position.Y, position.X);
                                Point empty = Point.Empty;
                                if (position.X != 1)
                                {
                                    if (position.X != 2)
                                    {
                                        break;
                                    }
                                    empty = gridEntryFromRow.ValueToolTipLocation;
                                }
                                else
                                {
                                    empty = gridEntryFromRow.GetLabelToolTipLocation(position.X - itemRect.X, position.Y - itemRect.Y);
                                }
                                if (empty != InvalidPoint)
                                {
                                    itemRect.Offset(empty);
                                    PositionTooltip(this, this.ToolTip, itemRect);
                                    m.Result = (IntPtr) 1;
                                    return true;
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 7:
                    if ((this.GetInPropertySet() || !this.Edit.Visible) || ((this.errorState == 0) && this.Commit()))
                    {
                        break;
                    }
                    base.WndProc(ref m);
                    this.Edit.FocusInternal();
                    return;

                case 0x15:
                    base.Invalidate();
                    break;

                case 0x4e:
                    if (this.WmNotify(ref m))
                    {
                        return;
                    }
                    break;

                case 0x456:
                    m.Result = (IntPtr) Math.Min(this.visibleRows, this.totalProps);
                    return;

                case 0x457:
                    m.Result = (IntPtr) this.GetRowFromGridEntry(this.selectedGridEntry);
                    return;

                case 0x200:
                    if (((int) ((long) m.LParam)) == this.lastMouseMove)
                    {
                        return;
                    }
                    this.lastMouseMove = (int) ((long) m.LParam);
                    break;

                case 0x10d:
                    this.Edit.FocusInternal();
                    this.Edit.Clear();
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this.Edit, this.Edit.Handle), 0x10d, 0, 0);
                    return;

                case 0x10f:
                    this.Edit.FocusInternal();
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this.Edit, this.Edit.Handle), 0x10f, m.WParam, m.LParam);
                    return;

                case 0x87:
                {
                    int num = 0x81;
                    if (((this.selectedGridEntry != null) && ((Control.ModifierKeys & Keys.Shift) == Keys.None)) && this.edit.Visible)
                    {
                        num |= 2;
                    }
                    m.Result = (IntPtr) num;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        public override System.Drawing.Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                this.backgroundBrush = new SolidBrush(value);
                base.BackColor = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool CanCopy
        {
            get
            {
                if (this.selectedGridEntry == null)
                {
                    return false;
                }
                if (this.Edit.Focused)
                {
                    return true;
                }
                string propertyTextValue = this.selectedGridEntry.GetPropertyTextValue();
                return ((propertyTextValue != null) && (propertyTextValue.Length > 0));
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool CanCut
        {
            get
            {
                return (this.CanCopy && this.selectedGridEntry.IsTextEditable);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), Browsable(false)]
        public bool CanPaste
        {
            get
            {
                return ((this.selectedGridEntry != null) && this.selectedGridEntry.IsTextEditable);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool CanUndo
        {
            get
            {
                return ((this.Edit.Visible && this.Edit.Focused) && (0 != ((int) this.Edit.SendMessage(0xc6, 0, 0))));
            }
        }

        public Point ContextMenuDefaultLocation
        {
            get
            {
                Rectangle rectangle = this.GetRectangle(this.selectedRow, 1);
                Point point = base.PointToScreen(new Point(rectangle.X, rectangle.Y));
                return new Point(point.X + (rectangle.Width / 2), point.Y + (rectangle.Height / 2));
            }
        }

        private System.Windows.Forms.Button DialogButton
        {
            get
            {
                if (this.btnDialog == null)
                {
                    this.btnDialog = new System.Windows.Forms.PropertyGridInternal.DropDownButton();
                    this.btnDialog.BackColor = SystemColors.Control;
                    this.btnDialog.ForeColor = SystemColors.ControlText;
                    this.btnDialog.TabIndex = 3;
                    this.btnDialog.Image = new Bitmap(typeof(PropertyGrid), "dotdotdot.png");
                    this.btnDialog.Click += new EventHandler(this.OnBtnClick);
                    this.btnDialog.KeyDown += new KeyEventHandler(this.OnBtnKeyDown);
                    this.btnDialog.LostFocus += new EventHandler(this.OnChildLostFocus);
                    this.btnDialog.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, this.RowHeight);
                    this.CommonEditorSetup(this.btnDialog);
                }
                return this.btnDialog;
            }
        }

        internal bool DrawValuesRightToLeft
        {
            get
            {
                if ((this.edit != null) && this.edit.IsHandleCreated)
                {
                    int windowLong = (int) ((long) System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(this.edit, this.edit.Handle), -20));
                    return ((windowLong & 0x2000) != 0);
                }
                return false;
            }
        }

        private System.Windows.Forms.PropertyGridInternal.DropDownButton DropDownButton
        {
            get
            {
                if (this.btnDropDown == null)
                {
                    this.btnDropDown = new System.Windows.Forms.PropertyGridInternal.DropDownButton();
                    this.btnDropDown.UseComboBoxTheme = true;
                    Bitmap bitmap = this.CreateDownArrow();
                    this.btnDropDown.Image = bitmap;
                    this.btnDropDown.BackColor = SystemColors.Control;
                    this.btnDropDown.ForeColor = SystemColors.ControlText;
                    this.btnDropDown.Click += new EventHandler(this.OnBtnClick);
                    this.btnDropDown.LostFocus += new EventHandler(this.OnChildLostFocus);
                    this.btnDropDown.TabIndex = 2;
                    this.CommonEditorSetup(this.btnDropDown);
                    this.btnDropDown.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, this.RowHeight);
                }
                return this.btnDropDown;
            }
        }

        private GridViewListBox DropDownListBox
        {
            get
            {
                if (this.listBox == null)
                {
                    this.listBox = new GridViewListBox(this);
                    this.listBox.DrawMode = DrawMode.OwnerDrawFixed;
                    this.listBox.MouseUp += new MouseEventHandler(this.OnListMouseUp);
                    this.listBox.DrawItem += new DrawItemEventHandler(this.OnListDrawItem);
                    this.listBox.SelectedIndexChanged += new EventHandler(this.OnListChange);
                    this.listBox.KeyDown += new KeyEventHandler(this.OnListKeyDown);
                    this.listBox.LostFocus += new EventHandler(this.OnChildLostFocus);
                    this.listBox.Visible = true;
                    this.listBox.ItemHeight = this.RowHeight;
                }
                return this.listBox;
            }
        }

        private GridViewEdit Edit
        {
            get
            {
                if (this.edit == null)
                {
                    this.edit = new GridViewEdit(this);
                    this.edit.BorderStyle = BorderStyle.None;
                    this.edit.AutoSize = false;
                    this.edit.TabStop = false;
                    this.edit.AcceptsReturn = true;
                    this.edit.BackColor = this.BackColor;
                    this.edit.ForeColor = this.ForeColor;
                    this.edit.KeyDown += new KeyEventHandler(this.OnEditKeyDown);
                    this.edit.KeyPress += new KeyPressEventHandler(this.OnEditKeyPress);
                    this.edit.GotFocus += new EventHandler(this.OnEditGotFocus);
                    this.edit.LostFocus += new EventHandler(this.OnEditLostFocus);
                    this.edit.MouseDown += new MouseEventHandler(this.OnEditMouseDown);
                    this.edit.TextChanged += new EventHandler(this.OnEditChange);
                    this.edit.TabIndex = 1;
                    this.CommonEditorSetup(this.edit);
                }
                return this.edit;
            }
        }

        private GridErrorDlg ErrorDialog
        {
            get
            {
                if (this.errorDlg == null)
                {
                    this.errorDlg = new GridErrorDlg(this.ownerGrid);
                }
                return this.errorDlg;
            }
        }

        public bool FocusInside
        {
            get
            {
                return (base.ContainsFocus || ((this.dropDownHolder != null) && this.dropDownHolder.ContainsFocus));
            }
        }

        internal System.Drawing.Color GrayTextColor
        {
            get
            {
                if (this.ForeColor.ToArgb() == SystemColors.WindowText.ToArgb())
                {
                    return SystemColors.GrayText;
                }
                int argb = this.ForeColor.ToArgb();
                int num2 = (argb >> 0x18) & 0xff;
                if (num2 != 0)
                {
                    num2 /= 2;
                    argb &= 0xffffff;
                    argb |= (num2 << 0x18) & ((int) 0xff000000L);
                }
                else
                {
                    argb /= 2;
                }
                return System.Drawing.Color.FromArgb(argb);
            }
        }

        private bool HasEntries
        {
            get
            {
                return ((this.topLevelGridEntries != null) && (this.topLevelGridEntries.Count > 0));
            }
        }

        protected int InternalLabelWidth
        {
            get
            {
                if (this.GetFlag(0x80))
                {
                    this.UpdateUIBasedOnFont(true);
                }
                if (this.labelWidth == -1)
                {
                    this.SetConstants();
                }
                return this.labelWidth;
            }
        }

        internal bool IsExplorerTreeSupported
        {
            get
            {
                return (System.Windows.Forms.UnsafeNativeMethods.IsVista && VisualStyleRenderer.IsSupported);
            }
        }

        internal int LabelPaintMargin
        {
            set
            {
                this.requiredLabelPaintMargin = (short) Math.Max(Math.Max(value, this.requiredLabelPaintMargin), 2);
            }
        }

        protected bool NeedsCommit
        {
            get
            {
                if ((this.edit == null) || !this.Edit.Visible)
                {
                    return false;
                }
                string text = this.Edit.Text;
                return ((((text != null) && (text.Length != 0)) || ((this.originalTextValue != null) && (this.originalTextValue.Length != 0))) && (((text == null) || (this.originalTextValue == null)) || !text.Equals(this.originalTextValue)));
            }
        }

        public PropertyGrid OwnerGrid
        {
            get
            {
                return this.ownerGrid;
            }
        }

        protected int RowHeight
        {
            get
            {
                if (this.cachedRowHeight == -1)
                {
                    this.cachedRowHeight = this.Font.Height + 2;
                }
                return this.cachedRowHeight;
            }
        }

        private System.Windows.Forms.ScrollBar ScrollBar
        {
            get
            {
                if (this.scrollBar == null)
                {
                    this.scrollBar = new VScrollBar();
                    this.scrollBar.Scroll += new ScrollEventHandler(this.OnScroll);
                    base.Controls.Add(this.scrollBar);
                }
                return this.scrollBar;
            }
        }

        internal GridEntry SelectedGridEntry
        {
            get
            {
                return this.selectedGridEntry;
            }
            set
            {
                if (this.allGridEntries != null)
                {
                    foreach (GridEntry entry in this.allGridEntries)
                    {
                        if (entry == value)
                        {
                            this.SelectGridEntry(value, true);
                            return;
                        }
                    }
                }
                GridEntry gridEntry = this.FindEquivalentGridEntry(new GridEntryCollection(null, new GridEntry[] { value }));
                if (gridEntry == null)
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("PropertyGridInvalidGridEntry"));
                }
                this.SelectGridEntry(gridEntry, true);
            }
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return this.serviceProvider;
            }
            set
            {
                if (value != this.serviceProvider)
                {
                    this.serviceProvider = value;
                    this.topHelpService = null;
                    if ((this.helpService != null) && (this.helpService is IDisposable))
                    {
                        ((IDisposable) this.helpService).Dispose();
                    }
                    this.helpService = null;
                }
            }
        }

        private int TipColumn
        {
            get
            {
                return ((this.tipInfo & -65536) >> 0x10);
            }
            set
            {
                this.tipInfo &= 0xffff;
                this.tipInfo |= (value & 0xffff) << 0x10;
            }
        }

        private int TipRow
        {
            get
            {
                return (this.tipInfo & 0xffff);
            }
            set
            {
                this.tipInfo &= -65536;
                this.tipInfo |= value & 0xffff;
            }
        }

        private GridToolTip ToolTip
        {
            get
            {
                if (this.toolTip == null)
                {
                    this.toolTip = new GridToolTip(new Control[] { this, this.Edit });
                    this.toolTip.ToolTip = "";
                    this.toolTip.Font = this.Font;
                }
                return this.toolTip;
            }
        }

        private class DropDownHolder : Form, PropertyGridView.IMouseHookClient
        {
            private LinkLabel createNewLink;
            private Control currentControl;
            private int currentMoveType;
            private Rectangle dragBaseRect = Rectangle.Empty;
            private Point dragStart = Point.Empty;
            private const int DropDownHolderBorder = 1;
            private PropertyGridView gridView;
            private static readonly Size MinDropDownSize = new Size(SystemInformation.VerticalScrollBarWidth * 4, SystemInformation.HorizontalScrollBarHeight * 4);
            private PropertyGridView.MouseHook mouseHook;
            private const int MoveTypeBottom = 1;
            private const int MoveTypeLeft = 2;
            private const int MoveTypeNone = 0;
            private const int MoveTypeTop = 4;
            private bool resizable = true;
            private static readonly int ResizeBarSize = (ResizeGripSize + 1);
            private static readonly int ResizeBorderSize = (ResizeBarSize / 2);
            private static readonly int ResizeGripSize = SystemInformation.HorizontalScrollBarHeight;
            private bool resizeUp;
            private bool resizing;
            private Bitmap sizeGripGlyph;

            internal DropDownHolder(PropertyGridView psheet)
            {
                base.ShowInTaskbar = false;
                base.ControlBox = false;
                base.MinimizeBox = false;
                base.MaximizeBox = false;
                this.Text = "";
                base.FormBorderStyle = FormBorderStyle.None;
                base.AutoScaleMode = AutoScaleMode.None;
                this.mouseHook = new PropertyGridView.MouseHook(this, this, psheet);
                base.Visible = false;
                this.gridView = psheet;
                this.BackColor = this.gridView.BackColor;
            }

            protected override void DestroyHandle()
            {
                this.mouseHook.HookMouseDown = false;
                base.DestroyHandle();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.createNewLink != null))
                {
                    this.createNewLink.Dispose();
                    this.createNewLink = null;
                }
                base.Dispose(disposing);
            }

            public void DoModalLoop()
            {
                while (base.Visible)
                {
                    Application.DoEventsModal();
                    System.Windows.Forms.UnsafeNativeMethods.MsgWaitForMultipleObjectsEx(0, IntPtr.Zero, 250, 0xff, 4);
                }
            }

            public virtual void FocusComponent()
            {
                if ((this.currentControl != null) && base.Visible)
                {
                    this.currentControl.FocusInternal();
                }
            }

            private InstanceCreationEditor GetInstanceCreationEditor(PropertyDescriptorGridEntry entry)
            {
                if (entry == null)
                {
                    return null;
                }
                InstanceCreationEditor editor = null;
                PropertyDescriptor propertyDescriptor = entry.PropertyDescriptor;
                if (propertyDescriptor != null)
                {
                    editor = propertyDescriptor.GetEditor(typeof(InstanceCreationEditor)) as InstanceCreationEditor;
                }
                if (editor == null)
                {
                    UITypeEditor uITypeEditor = entry.UITypeEditor;
                    if ((uITypeEditor != null) && (uITypeEditor.GetEditStyle() == UITypeEditorEditStyle.DropDown))
                    {
                        editor = (InstanceCreationEditor) TypeDescriptor.GetEditor(uITypeEditor, typeof(InstanceCreationEditor));
                    }
                }
                return editor;
            }

            private Bitmap GetSizeGripGlyph(Graphics g)
            {
                if (this.sizeGripGlyph == null)
                {
                    this.sizeGripGlyph = new Bitmap(ResizeGripSize, ResizeGripSize, g);
                    using (Graphics graphics = Graphics.FromImage(this.sizeGripGlyph))
                    {
                        Matrix matrix = new Matrix();
                        matrix.Translate((float) (ResizeGripSize + 1), this.resizeUp ? ((float) (ResizeGripSize + 1)) : ((float) 0));
                        matrix.Scale(-1f, this.resizeUp ? ((float) (-1)) : ((float) 1));
                        graphics.Transform = matrix;
                        ControlPaint.DrawSizeGrip(graphics, this.BackColor, 0, 0, ResizeGripSize, ResizeGripSize);
                        graphics.ResetTransform();
                    }
                    this.sizeGripGlyph.MakeTransparent(this.BackColor);
                }
                return this.sizeGripGlyph;
            }

            public virtual bool GetUsed()
            {
                return (this.currentControl != null);
            }

            private int MoveTypeFromPoint(int x, int y)
            {
                Rectangle rectangle = new Rectangle(0, base.Height - ResizeGripSize, ResizeGripSize, ResizeGripSize);
                Rectangle rectangle2 = new Rectangle(0, 0, ResizeGripSize, ResizeGripSize);
                if (!this.resizeUp && rectangle.Contains(x, y))
                {
                    return 3;
                }
                if (this.resizeUp && rectangle2.Contains(x, y))
                {
                    return 6;
                }
                if (!this.resizeUp && (Math.Abs((int) (base.Height - y)) < ResizeBorderSize))
                {
                    return 1;
                }
                if (this.resizeUp && (Math.Abs(y) < ResizeBorderSize))
                {
                    return 4;
                }
                return 0;
            }

            public bool OnClickHooked()
            {
                this.gridView.CloseDropDownInternal(false);
                return false;
            }

            private void OnCurrentControlResize(object o, EventArgs e)
            {
                if ((this.currentControl != null) && !this.resizing)
                {
                    int width = base.Width;
                    Size size = new Size(2 + this.currentControl.Width, 2 + this.currentControl.Height);
                    if (this.resizable)
                    {
                        size.Height += ResizeBarSize;
                    }
                    try
                    {
                        this.resizing = true;
                        base.SuspendLayout();
                        base.Size = size;
                    }
                    finally
                    {
                        this.resizing = false;
                        base.ResumeLayout(false);
                    }
                    base.Left -= base.Width - width;
                }
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                try
                {
                    this.resizing = true;
                    base.OnLayout(levent);
                }
                finally
                {
                    this.resizing = false;
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.currentMoveType = this.MoveTypeFromPoint(e.X, e.Y);
                    if (this.currentMoveType != 0)
                    {
                        this.dragStart = base.PointToScreen(new Point(e.X, e.Y));
                        this.dragBaseRect = base.Bounds;
                        base.Capture = true;
                    }
                    else
                    {
                        this.gridView.CloseDropDown();
                    }
                }
                base.OnMouseDown(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                this.Cursor = null;
                base.OnMouseLeave(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (this.currentMoveType != 0)
                {
                    Point point = base.PointToScreen(new Point(e.X, e.Y));
                    Rectangle bounds = base.Bounds;
                    if ((this.currentMoveType & 1) == 1)
                    {
                        bounds.Height = Math.Max(MinDropDownSize.Height, this.dragBaseRect.Height + (point.Y - this.dragStart.Y));
                    }
                    if ((this.currentMoveType & 4) == 4)
                    {
                        int num2 = point.Y - this.dragStart.Y;
                        if ((this.dragBaseRect.Height - num2) > MinDropDownSize.Height)
                        {
                            bounds.Y = this.dragBaseRect.Top + num2;
                            bounds.Height = this.dragBaseRect.Height - num2;
                        }
                    }
                    if ((this.currentMoveType & 2) == 2)
                    {
                        int num3 = point.X - this.dragStart.X;
                        if ((this.dragBaseRect.Width - num3) > MinDropDownSize.Width)
                        {
                            bounds.X = this.dragBaseRect.Left + num3;
                            bounds.Width = this.dragBaseRect.Width - num3;
                        }
                    }
                    if (bounds != base.Bounds)
                    {
                        try
                        {
                            this.resizing = true;
                            base.Bounds = bounds;
                        }
                        finally
                        {
                            this.resizing = false;
                        }
                    }
                    base.Invalidate();
                }
                else
                {
                    switch (this.MoveTypeFromPoint(e.X, e.Y))
                    {
                        case 1:
                        case 4:
                            this.Cursor = Cursors.SizeNS;
                            goto Label_01D8;

                        case 3:
                            this.Cursor = Cursors.SizeNESW;
                            goto Label_01D8;

                        case 6:
                            this.Cursor = Cursors.SizeNWSE;
                            goto Label_01D8;
                    }
                    this.Cursor = null;
                }
            Label_01D8:
                base.OnMouseMove(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (e.Button == MouseButtons.Left)
                {
                    this.currentMoveType = 0;
                    this.dragStart = Point.Empty;
                    this.dragBaseRect = Rectangle.Empty;
                    base.Capture = false;
                }
            }

            private void OnNewLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
            {
                InstanceCreationEditor linkData = e.Link.LinkData as InstanceCreationEditor;
                if (linkData != null)
                {
                    System.Type propertyType = this.gridView.SelectedGridEntry.PropertyType;
                    if (propertyType != null)
                    {
                        this.gridView.CloseDropDown();
                        object o = linkData.CreateInstance(this.gridView.SelectedGridEntry, propertyType);
                        if (o != null)
                        {
                            if (!propertyType.IsInstanceOfType(o))
                            {
                                throw new InvalidCastException(System.Windows.Forms.SR.GetString("PropertyGridViewEditorCreatedInvalidObject", new object[] { propertyType }));
                            }
                            this.gridView.CommitValue(o);
                        }
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs pe)
            {
                base.OnPaint(pe);
                if (this.resizable)
                {
                    Rectangle rect = new Rectangle(0, this.resizeUp ? 0 : (base.Height - ResizeGripSize), ResizeGripSize, ResizeGripSize);
                    pe.Graphics.DrawImage(this.GetSizeGripGlyph(pe.Graphics), rect);
                    int num = this.resizeUp ? (ResizeBarSize - 1) : (base.Height - ResizeBarSize);
                    Pen pen = new Pen(SystemColors.ControlDark, 1f) {
                        DashStyle = DashStyle.Solid
                    };
                    pe.Graphics.DrawLine(pen, 0, num, base.Width, num);
                    pen.Dispose();
                }
            }

            private bool OwnsWindow(IntPtr hWnd)
            {
                while (hWnd != IntPtr.Zero)
                {
                    hWnd = System.Windows.Forms.UnsafeNativeMethods.GetWindowLong(new HandleRef(null, hWnd), -8);
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
                if ((keyData & (Keys.Alt | Keys.Control | Keys.Shift)) == Keys.None)
                {
                    switch ((keyData & Keys.KeyCode))
                    {
                        case Keys.Enter:
                            if (this.gridView.UnfocusSelection() && (this.gridView.SelectedGridEntry != null))
                            {
                                this.gridView.SelectedGridEntry.OnValueReturnKey();
                            }
                            return true;

                        case Keys.Escape:
                            this.gridView.OnEscape(this);
                            return true;

                        case Keys.F4:
                            this.gridView.F4Selection(true);
                            return true;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            public void SetComponent(Control ctl, bool resizable)
            {
                this.resizable = resizable;
                this.Font = this.gridView.Font;
                InstanceCreationEditor linkData = (ctl == null) ? null : this.GetInstanceCreationEditor(this.gridView.SelectedGridEntry as PropertyDescriptorGridEntry);
                if (this.currentControl != null)
                {
                    this.currentControl.Resize -= new EventHandler(this.OnCurrentControlResize);
                    base.Controls.Remove(this.currentControl);
                    this.currentControl = null;
                }
                if ((this.createNewLink != null) && (this.createNewLink.Parent == this))
                {
                    base.Controls.Remove(this.createNewLink);
                }
                if (ctl != null)
                {
                    this.currentControl = ctl;
                    base.DockPadding.All = 0;
                    if (this.currentControl is PropertyGridView.GridViewListBox)
                    {
                        ListBox currentControl = (ListBox) this.currentControl;
                        if (currentControl.Items.Count == 0)
                        {
                            currentControl.Height = Math.Max(currentControl.Height, currentControl.ItemHeight);
                        }
                    }
                    try
                    {
                        base.SuspendLayout();
                        base.Controls.Add(ctl);
                        Size size = new Size(2 + ctl.Width, 2 + ctl.Height);
                        if (linkData != null)
                        {
                            this.CreateNewLink.Text = linkData.Text;
                            this.CreateNewLink.Links.Clear();
                            this.CreateNewLink.Links.Add(0, linkData.Text.Length, linkData);
                            int height = this.CreateNewLink.Height;
                            using (Graphics graphics = this.gridView.CreateGraphics())
                            {
                                height = (int) PropertyGrid.MeasureTextHelper.MeasureText(this.gridView.ownerGrid, graphics, linkData.Text, this.gridView.GetBaseFont()).Height;
                            }
                            this.CreateNewLink.Height = height + 1;
                            size.Height += height + 2;
                        }
                        if (resizable)
                        {
                            size.Height += ResizeBarSize;
                            if (this.resizeUp)
                            {
                                base.DockPadding.Top = ResizeBarSize;
                            }
                            else
                            {
                                base.DockPadding.Bottom = ResizeBarSize;
                            }
                        }
                        base.Size = size;
                        ctl.Dock = DockStyle.Fill;
                        ctl.Visible = true;
                        if (linkData != null)
                        {
                            this.CreateNewLink.Dock = DockStyle.Bottom;
                            base.Controls.Add(this.CreateNewLink);
                        }
                    }
                    finally
                    {
                        base.ResumeLayout(true);
                    }
                    this.currentControl.Resize += new EventHandler(this.OnCurrentControlResize);
                }
                base.Enabled = this.currentControl != null;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 6)
                {
                    base.SetState(0x20, true);
                    IntPtr lParam = m.LParam;
                    if ((base.Visible && (System.Windows.Forms.NativeMethods.Util.LOWORD(m.WParam) == 0)) && !this.OwnsWindow(lParam))
                    {
                        this.gridView.CloseDropDownInternal(false);
                        return;
                    }
                }
                else if (m.Msg == 0x10)
                {
                    if (base.Visible)
                    {
                        this.gridView.CloseDropDown();
                    }
                    return;
                }
                base.WndProc(ref m);
            }

            public virtual Control Component
            {
                get
                {
                    return this.currentControl;
                }
            }

            private LinkLabel CreateNewLink
            {
                get
                {
                    if (this.createNewLink == null)
                    {
                        this.createNewLink = new LinkLabel();
                        this.createNewLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnNewLinkClicked);
                    }
                    return this.createNewLink;
                }
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    createParams.ExStyle |= 0x80;
                    createParams.Style |= -2139095040;
                    if (OSFeature.IsPresent(SystemParameter.DropShadow))
                    {
                        createParams.ClassStyle |= 0x20000;
                    }
                    if (this.gridView != null)
                    {
                        createParams.Parent = this.gridView.ParentInternal.Handle;
                    }
                    return createParams;
                }
            }

            public virtual bool HookMouseDown
            {
                get
                {
                    return this.mouseHook.HookMouseDown;
                }
                set
                {
                    this.mouseHook.HookMouseDown = value;
                }
            }

            public bool ResizeUp
            {
                set
                {
                    if (this.resizeUp != value)
                    {
                        this.sizeGripGlyph = null;
                        this.resizeUp = value;
                        if (this.resizable)
                        {
                            base.DockPadding.Bottom = 0;
                            base.DockPadding.Top = 0;
                            if (value)
                            {
                                base.DockPadding.Top = ResizeBarSize;
                            }
                            else
                            {
                                base.DockPadding.Bottom = ResizeBarSize;
                            }
                        }
                    }
                }
            }
        }

        internal class GridPositionData
        {
            private ArrayList expandedState;
            private int itemCount;
            private int itemRow;
            private GridEntryCollection selectedItemTree;

            public GridPositionData(PropertyGridView gridView)
            {
                this.selectedItemTree = gridView.GetGridEntryHierarchy(gridView.selectedGridEntry);
                this.expandedState = gridView.SaveHierarchyState(gridView.topLevelGridEntries);
                this.itemRow = gridView.selectedRow;
                this.itemCount = gridView.totalProps;
            }

            public GridEntry Restore(PropertyGridView gridView)
            {
                gridView.RestoreHierarchyState(this.expandedState);
                GridEntry gridEntry = gridView.FindEquivalentGridEntry(this.selectedItemTree);
                if (gridEntry != null)
                {
                    gridView.SelectGridEntry(gridEntry, true);
                    int cOffset = gridView.selectedRow - this.itemRow;
                    if (((cOffset == 0) || !gridView.ScrollBar.Visible) || (this.itemRow >= gridView.visibleRows))
                    {
                        return gridEntry;
                    }
                    cOffset += gridView.GetScrollOffset();
                    if (cOffset < 0)
                    {
                        cOffset = 0;
                    }
                    else if (cOffset > gridView.ScrollBar.Maximum)
                    {
                        cOffset = gridView.ScrollBar.Maximum - 1;
                    }
                    gridView.SetScrollOffset(cOffset);
                }
                return gridEntry;
            }
        }

        private class GridViewEdit : System.Windows.Forms.TextBox, PropertyGridView.IMouseHookClient
        {
            private bool dontFocusMe;
            internal bool filter;
            internal bool fInSetText;
            private int lastMove;
            private PropertyGridView.MouseHook mouseHook;
            internal PropertyGridView psheet;

            public GridViewEdit(PropertyGridView psheet)
            {
                this.psheet = psheet;
                this.mouseHook = new PropertyGridView.MouseHook(this, this, psheet);
            }

            protected override void DestroyHandle()
            {
                this.mouseHook.HookMouseDown = false;
                base.DestroyHandle();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.mouseHook.Dispose();
                }
                base.Dispose(disposing);
            }

            public void FilterKeyPress(char keyChar)
            {
                if (this.IsInputChar(keyChar))
                {
                    this.FocusInternal();
                    base.SelectAll();
                    System.Windows.Forms.UnsafeNativeMethods.PostMessage(new HandleRef(this, base.Handle), 0x102, (IntPtr) keyChar, IntPtr.Zero);
                }
            }

            public virtual bool InSetText()
            {
                return this.fInSetText;
            }

            protected override bool IsInputChar(char keyChar)
            {
                Keys keys = (Keys) keyChar;
                return (((keys != Keys.Tab) && (keys != Keys.Enter)) && base.IsInputChar(keyChar));
            }

            protected override bool IsInputKey(Keys keyData)
            {
                Keys keys = keyData & Keys.KeyCode;
                if (keys <= Keys.Enter)
                {
                    switch (keys)
                    {
                        case Keys.Tab:
                        case Keys.Enter:
                            goto Label_0028;
                    }
                    goto Label_002A;
                }
                if (((keys != Keys.Escape) && (keys != Keys.F1)) && (keys != Keys.F4))
                {
                    goto Label_002A;
                }
            Label_0028:
                return false;
            Label_002A:
                if (this.psheet.NeedsCommit)
                {
                    return false;
                }
                return base.IsInputKey(keyData);
            }

            public bool OnClickHooked()
            {
                return !this.psheet._Commit();
            }

            protected override void OnKeyDown(KeyEventArgs ke)
            {
                if (this.ProcessDialogKey(ke.KeyData))
                {
                    ke.Handled = true;
                }
                else
                {
                    base.OnKeyDown(ke);
                }
            }

            protected override void OnKeyPress(KeyPressEventArgs ke)
            {
                if (!this.IsInputChar(ke.KeyChar))
                {
                    ke.Handled = true;
                }
                else
                {
                    base.OnKeyPress(ke);
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                if (!this.Focused)
                {
                    Graphics g = base.CreateGraphics();
                    if ((this.psheet.SelectedGridEntry != null) && (base.ClientRectangle.Width <= this.psheet.SelectedGridEntry.GetValueTextWidth(this.Text, g, this.Font)))
                    {
                        this.psheet.ToolTip.ToolTip = this.PasswordProtect ? "" : this.Text;
                    }
                    g.Dispose();
                }
            }

            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                switch ((keyData & Keys.KeyCode))
                {
                    case Keys.Insert:
                        if (((keyData & Keys.Alt) != Keys.None) || !(((keyData & Keys.Control) != Keys.None) ^ ((keyData & Keys.Shift) == Keys.None)))
                        {
                            break;
                        }
                        return false;

                    case Keys.Delete:
                        if ((((keyData & Keys.Control) != Keys.None) || ((keyData & Keys.Shift) == Keys.None)) || ((keyData & Keys.Alt) != Keys.None))
                        {
                            if (((((keyData & Keys.Control) == Keys.None) && ((keyData & Keys.Shift) == Keys.None)) && (((keyData & Keys.Alt) == Keys.None) && (this.psheet.SelectedGridEntry != null))) && ((!this.psheet.SelectedGridEntry.Enumerable && !this.psheet.SelectedGridEntry.IsTextEditable) && this.psheet.SelectedGridEntry.CanResetPropertyValue()))
                            {
                                object propertyValue = this.psheet.SelectedGridEntry.PropertyValue;
                                this.psheet.SelectedGridEntry.ResetPropertyValue();
                                this.psheet.UnfocusSelection();
                                this.psheet.ownerGrid.OnPropertyValueSet(this.psheet.SelectedGridEntry, propertyValue);
                            }
                            break;
                        }
                        return false;

                    case Keys.A:
                        if ((((keyData & Keys.Control) == Keys.None) || ((keyData & Keys.Shift) != Keys.None)) || ((keyData & Keys.Alt) != Keys.None))
                        {
                            break;
                        }
                        base.SelectAll();
                        return true;

                    case Keys.C:
                    case Keys.V:
                    case Keys.X:
                    case Keys.Z:
                        if ((((keyData & Keys.Control) == Keys.None) || ((keyData & Keys.Shift) != Keys.None)) || ((keyData & Keys.Alt) != Keys.None))
                        {
                            break;
                        }
                        return false;
                }
                return base.ProcessCmdKey(ref msg, keyData);
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if ((keyData & (Keys.Alt | Keys.Control | Keys.Shift)) == Keys.None)
                {
                    switch ((keyData & Keys.KeyCode))
                    {
                        case Keys.Enter:
                        {
                            bool flag = !this.psheet.NeedsCommit;
                            if (this.psheet.UnfocusSelection() && flag)
                            {
                                this.psheet.SelectedGridEntry.OnValueReturnKey();
                            }
                            return true;
                        }
                        case Keys.Escape:
                            this.psheet.OnEscape(this);
                            return true;

                        case Keys.F4:
                            this.psheet.F4Selection(true);
                            return true;
                    }
                }
                if (((keyData & Keys.KeyCode) == Keys.Tab) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                {
                    return !this.psheet._Commit();
                }
                return base.ProcessDialogKey(keyData);
            }

            protected override void SetVisibleCore(bool value)
            {
                if (!value && this.HookMouseDown)
                {
                    this.mouseHook.HookMouseDown = false;
                }
                base.SetVisibleCore(value);
            }

            internal bool WantsTab(bool forward)
            {
                return this.psheet.WantsTab(forward);
            }

            private unsafe bool WmNotify(ref Message m)
            {
                if (m.LParam != IntPtr.Zero)
                {
                    System.Windows.Forms.NativeMethods.NMHDR* lParam = (System.Windows.Forms.NativeMethods.NMHDR*) m.LParam;
                    if (lParam->hwndFrom == this.psheet.ToolTip.Handle)
                    {
                        if (lParam->code == -521)
                        {
                            PropertyGridView.PositionTooltip(this, this.psheet.ToolTip, base.ClientRectangle);
                            m.Result = (IntPtr) 1;
                            return true;
                        }
                        this.psheet.WndProc(ref m);
                    }
                }
                return false;
            }

            protected override void WndProc(ref Message m)
            {
                if (!this.filter || !this.psheet.FilterEditWndProc(ref m))
                {
                    switch (m.Msg)
                    {
                        case 2:
                            this.mouseHook.HookMouseDown = false;
                            break;

                        case 0x18:
                            if (IntPtr.Zero == m.WParam)
                            {
                                this.mouseHook.HookMouseDown = false;
                            }
                            break;

                        case 0x4e:
                            if (this.WmNotify(ref m))
                            {
                                return;
                            }
                            break;

                        case 0x200:
                            if (((int) ((long) m.LParam)) == this.lastMove)
                            {
                                return;
                            }
                            this.lastMove = (int) ((long) m.LParam);
                            break;

                        case 770:
                            if (!base.ReadOnly)
                            {
                                break;
                            }
                            return;

                        case 0x7d:
                            if ((((int) ((long) m.WParam)) & -20) != 0)
                            {
                                this.psheet.Invalidate();
                            }
                            break;

                        case 0x87:
                            m.Result = (IntPtr) ((((long) m.Result) | 1L) | 0x80L);
                            if (this.psheet.NeedsCommit || this.WantsTab((Control.ModifierKeys & Keys.Shift) == Keys.None))
                            {
                                m.Result = (IntPtr) ((((long) m.Result) | 4L) | 2L);
                            }
                            return;
                    }
                    base.WndProc(ref m);
                }
            }

            public bool DisableMouseHook
            {
                set
                {
                    this.mouseHook.DisableMouseHook = value;
                }
            }

            public bool DontFocus
            {
                set
                {
                    this.dontFocusMe = value;
                }
            }

            public virtual bool Filter
            {
                get
                {
                    return this.filter;
                }
                set
                {
                    this.filter = value;
                }
            }

            public override bool Focused
            {
                get
                {
                    if (this.dontFocusMe)
                    {
                        return false;
                    }
                    return base.Focused;
                }
            }

            public virtual bool HookMouseDown
            {
                get
                {
                    return this.mouseHook.HookMouseDown;
                }
                set
                {
                    this.mouseHook.HookMouseDown = value;
                    if (value)
                    {
                        this.FocusInternal();
                    }
                }
            }

            public override string Text
            {
                get
                {
                    return base.Text;
                }
                set
                {
                    this.fInSetText = true;
                    base.Text = value;
                    this.fInSetText = false;
                }
            }
        }

        private class GridViewListBox : ListBox
        {
            internal bool fInSetSelectedIndex;

            public GridViewListBox(PropertyGridView gridView)
            {
                base.IntegralHeight = false;
            }

            public virtual bool InSetSelectedIndex()
            {
                return this.fInSetSelectedIndex;
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                this.fInSetSelectedIndex = true;
                base.OnSelectedIndexChanged(e);
                this.fInSetSelectedIndex = false;
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    createParams.Style &= -8388609;
                    createParams.ExStyle &= -513;
                    return createParams;
                }
            }
        }

        internal interface IMouseHookClient
        {
            bool OnClickHooked();
        }

        internal class MouseHook
        {
            private PropertyGridView.IMouseHookClient client;
            private Control control;
            private PropertyGridView gridView;
            private bool hookDisable;
            private IntPtr mouseHookHandle = IntPtr.Zero;
            private GCHandle mouseHookRoot;
            private bool processing;
            internal int thisProcessID;

            public MouseHook(Control control, PropertyGridView.IMouseHookClient client, PropertyGridView gridView)
            {
                this.control = control;
                this.gridView = gridView;
                this.client = client;
            }

            public void Dispose()
            {
                this.UnhookMouse();
            }

            private void HookMouse()
            {
                GC.KeepAlive(this);
                lock (this)
                {
                    if (this.mouseHookHandle == IntPtr.Zero)
                    {
                        if (this.thisProcessID == 0)
                        {
                            System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this.control, this.control.Handle), out this.thisProcessID);
                        }
                        MouseHookObject obj1 = new MouseHookObject(this);
                        System.Windows.Forms.NativeMethods.HookProc proc = new System.Windows.Forms.NativeMethods.HookProc(obj1.Callback);
                        this.mouseHookRoot = GCHandle.Alloc(proc);
                        this.mouseHookHandle = System.Windows.Forms.UnsafeNativeMethods.SetWindowsHookEx(7, proc, System.Windows.Forms.NativeMethods.NullHandleRef, System.Windows.Forms.SafeNativeMethods.GetCurrentThreadId());
                    }
                }
            }

            private IntPtr MouseHookProc(int nCode, IntPtr wparam, IntPtr lparam)
            {
                GC.KeepAlive(this);
                if (nCode == 0)
                {
                    System.Windows.Forms.NativeMethods.MOUSEHOOKSTRUCT mousehookstruct = (System.Windows.Forms.NativeMethods.MOUSEHOOKSTRUCT) System.Windows.Forms.UnsafeNativeMethods.PtrToStructure(lparam, typeof(System.Windows.Forms.NativeMethods.MOUSEHOOKSTRUCT));
                    if (mousehookstruct != null)
                    {
                        switch (((int) ((long) wparam)))
                        {
                            case 0x21:
                            case 0xa1:
                            case 0xa4:
                            case 0x204:
                            case 0x207:
                            case 0xa7:
                            case 0x201:
                                if (this.ProcessMouseDown(mousehookstruct.hWnd, mousehookstruct.pt_x, mousehookstruct.pt_y))
                                {
                                    return (IntPtr) 1;
                                }
                                break;
                        }
                    }
                }
                return System.Windows.Forms.UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, this.mouseHookHandle), nCode, wparam, lparam);
            }

            private bool ProcessMouseDown(IntPtr hWnd, int x, int y)
            {
                if (!this.processing)
                {
                    IntPtr handle = hWnd;
                    IntPtr ptr2 = this.control.Handle;
                    Control ctl = Control.FromHandleInternal(handle);
                    if ((handle != ptr2) && !this.control.Contains(ctl))
                    {
                        int num;
                        System.Windows.Forms.SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, handle), out num);
                        if (num != this.thisProcessID)
                        {
                            this.HookMouseDown = false;
                            return false;
                        }
                        bool flag = false;
                        flag = (ctl == null) || !this.gridView.IsSiblingControl(this.control, ctl);
                        try
                        {
                            this.processing = true;
                            if (flag && this.client.OnClickHooked())
                            {
                                return true;
                            }
                        }
                        finally
                        {
                            this.processing = false;
                        }
                        this.HookMouseDown = false;
                    }
                }
                return false;
            }

            private void UnhookMouse()
            {
                GC.KeepAlive(this);
                lock (this)
                {
                    if (this.mouseHookHandle != IntPtr.Zero)
                    {
                        System.Windows.Forms.UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, this.mouseHookHandle));
                        this.mouseHookRoot.Free();
                        this.mouseHookHandle = IntPtr.Zero;
                    }
                }
            }

            public bool DisableMouseHook
            {
                set
                {
                    this.hookDisable = value;
                    if (value)
                    {
                        this.UnhookMouse();
                    }
                }
            }

            public virtual bool HookMouseDown
            {
                get
                {
                    GC.KeepAlive(this);
                    return (this.mouseHookHandle != IntPtr.Zero);
                }
                set
                {
                    if (value && !this.hookDisable)
                    {
                        this.HookMouse();
                    }
                    else
                    {
                        this.UnhookMouse();
                    }
                }
            }

            private class MouseHookObject
            {
                internal WeakReference reference;

                public MouseHookObject(PropertyGridView.MouseHook parent)
                {
                    this.reference = new WeakReference(parent, false);
                }

                public virtual IntPtr Callback(int nCode, IntPtr wparam, IntPtr lparam)
                {
                    IntPtr zero = IntPtr.Zero;
                    try
                    {
                        PropertyGridView.MouseHook target = (PropertyGridView.MouseHook) this.reference.Target;
                        if (target != null)
                        {
                            zero = target.MouseHookProc(nCode, wparam, lparam);
                        }
                    }
                    catch
                    {
                    }
                    return zero;
                }
            }
        }

        [ComVisible(true)]
        internal class PropertyGridViewAccessibleObject : Control.ControlAccessibleObject
        {
            public PropertyGridViewAccessibleObject(PropertyGridView owner) : base(owner)
            {
            }

            public override AccessibleObject GetChild(int index)
            {
                GridEntryCollection entrys = ((PropertyGridView) base.Owner).AccessibilityGetGridEntries();
                if (((entrys != null) && (index >= 0)) && (index < entrys.Count))
                {
                    return entrys.GetEntry(index).AccessibilityObject;
                }
                return null;
            }

            public override int GetChildCount()
            {
                GridEntryCollection entrys = ((PropertyGridView) base.Owner).AccessibilityGetGridEntries();
                if (entrys != null)
                {
                    return entrys.Count;
                }
                return 0;
            }

            public override AccessibleObject GetFocused()
            {
                GridEntry selectedGridEntry = ((PropertyGridView) base.Owner).SelectedGridEntry;
                if ((selectedGridEntry != null) && selectedGridEntry.Focus)
                {
                    return selectedGridEntry.AccessibilityObject;
                }
                return null;
            }

            public override AccessibleObject GetSelected()
            {
                GridEntry selectedGridEntry = ((PropertyGridView) base.Owner).SelectedGridEntry;
                if (selectedGridEntry != null)
                {
                    return selectedGridEntry.AccessibilityObject;
                }
                return null;
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                System.Windows.Forms.NativeMethods.POINT pt = new System.Windows.Forms.NativeMethods.POINT(x, y);
                System.Windows.Forms.UnsafeNativeMethods.ScreenToClient(new HandleRef(base.Owner, base.Owner.Handle), pt);
                Point point2 = ((PropertyGridView) base.Owner).FindPosition(pt.x, pt.y);
                if (point2 != PropertyGridView.InvalidPosition)
                {
                    GridEntry gridEntryFromRow = ((PropertyGridView) base.Owner).GetGridEntryFromRow(point2.Y);
                    if (gridEntryFromRow != null)
                    {
                        return gridEntryFromRow.AccessibilityObject;
                    }
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (this.GetChildCount() > 0)
                {
                    switch (navdir)
                    {
                        case AccessibleNavigation.FirstChild:
                            return this.GetChild(0);

                        case AccessibleNavigation.LastChild:
                            return this.GetChild(this.GetChildCount() - 1);
                    }
                }
                return null;
            }

            public AccessibleObject Next(GridEntry current)
            {
                int rowFromGridEntry = ((PropertyGridView) base.Owner).GetRowFromGridEntry(current);
                GridEntry gridEntryFromRow = ((PropertyGridView) base.Owner).GetGridEntryFromRow(++rowFromGridEntry);
                if (gridEntryFromRow != null)
                {
                    return gridEntryFromRow.AccessibilityObject;
                }
                return null;
            }

            public AccessibleObject Previous(GridEntry current)
            {
                int rowFromGridEntry = ((PropertyGridView) base.Owner).GetRowFromGridEntry(current);
                GridEntry gridEntryFromRow = ((PropertyGridView) base.Owner).GetGridEntryFromRow(--rowFromGridEntry);
                if (gridEntryFromRow != null)
                {
                    return gridEntryFromRow.AccessibilityObject;
                }
                return null;
            }

            public override string Name
            {
                get
                {
                    string accessibleName = base.Owner.AccessibleName;
                    if (accessibleName != null)
                    {
                        return accessibleName;
                    }
                    return System.Windows.Forms.SR.GetString("PropertyGridDefaultAccessibleName");
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    AccessibleRole accessibleRole = base.Owner.AccessibleRole;
                    if (accessibleRole != AccessibleRole.Default)
                    {
                        return accessibleRole;
                    }
                    return AccessibleRole.Table;
                }
            }
        }
    }
}

