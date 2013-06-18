namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class TableLayoutPanelBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private BehaviorService behaviorService;
        private PropertyDescriptor changedProp;
        private bool currentColumnStyles;
        private TableLayoutPanelDesigner designer;
        private Point lastMouseLoc;
        private StyleHelper leftStyle;
        private bool pushedBehavior;
        private PropertyDescriptor resizeProp;
        private DesignerTransaction resizeTransaction;
        private StyleHelper rightStyle;
        private IServiceProvider serviceProvider;
        private ArrayList styles;
        private TableLayoutPanel table;
        private TableLayoutPanelResizeGlyph tableGlyph;
        private static readonly TraceSwitch tlpResizeSwitch;

        internal TableLayoutPanelBehavior(TableLayoutPanel panel, TableLayoutPanelDesigner designer, IServiceProvider serviceProvider)
        {
            this.table = panel;
            this.designer = designer;
            this.serviceProvider = serviceProvider;
            this.behaviorService = serviceProvider.GetService(typeof(BehaviorService)) as BehaviorService;
            if (this.behaviorService != null)
            {
                this.pushedBehavior = false;
                this.lastMouseLoc = Point.Empty;
            }
        }

        private bool CanResizeStyle(int[] widths)
        {
            bool flag = false;
            bool flag2 = false;
            int index = this.styles.IndexOf(this.tableGlyph.Style);
            if ((index > -1) && (index != this.styles.Count))
            {
                flag = this.IndexOfNextStealableStyle(true, index, widths) != -1;
                flag2 = this.IndexOfNextStealableStyle(false, index, widths) != -1;
            }
            else
            {
                return false;
            }
            return (flag && flag2);
        }

        private void FinishResize()
        {
            this.pushedBehavior = false;
            this.behaviorService.PopBehavior(this);
            this.lastMouseLoc = Point.Empty;
            this.styles = null;
            IComponentChangeService service = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if ((service != null) && (this.changedProp != null))
            {
                service.OnComponentChanged(this.table, this.changedProp, null, null);
                this.changedProp = null;
            }
            SelectionManager manager = this.serviceProvider.GetService(typeof(SelectionManager)) as SelectionManager;
            if (manager != null)
            {
                manager.Refresh();
            }
        }

        private void GetActiveStyleCollection(bool isColumn)
        {
            if (((this.styles == null) || (isColumn != this.currentColumnStyles)) && (this.table != null))
            {
                this.styles = new ArrayList(this.changedProp.GetValue(this.table) as TableLayoutStyleCollection);
                this.currentColumnStyles = isColumn;
            }
        }

        private int IndexOfNextStealableStyle(bool forward, int startIndex, int[] widths)
        {
            int num = -1;
            if (this.styles != null)
            {
                if (forward)
                {
                    for (int j = startIndex + 1; j < this.styles.Count; j++)
                    {
                        if ((((TableLayoutStyle) this.styles[j]).SizeType != SizeType.AutoSize) && (widths[j] >= DesignerUtils.MINUMUMSTYLESIZEDRAG))
                        {
                            return j;
                        }
                    }
                    return num;
                }
                for (int i = startIndex; i >= 0; i--)
                {
                    if ((((TableLayoutStyle) this.styles[i]).SizeType != SizeType.AutoSize) && (widths[i] >= DesignerUtils.MINUMUMSTYLESIZEDRAG))
                    {
                        return i;
                    }
                }
            }
            return num;
        }

        public override void OnLoseCapture(Glyph g, EventArgs e)
        {
            if (this.pushedBehavior)
            {
                this.FinishResize();
                if (this.resizeTransaction != null)
                {
                    DesignerTransaction resizeTransaction = this.resizeTransaction;
                    this.resizeTransaction = null;
                    using (resizeTransaction)
                    {
                        resizeTransaction.Cancel();
                    }
                }
            }
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if ((button != MouseButtons.Left) || !(g is TableLayoutPanelResizeGlyph))
            {
                goto Label_02E0;
            }
            this.tableGlyph = g as TableLayoutPanelResizeGlyph;
            ISelectionService service = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                service.SetSelectedComponents(new object[] { this.designer.Component }, SelectionTypes.Click);
            }
            bool isColumn = this.tableGlyph.Type == TableLayoutPanelResizeGlyph.TableLayoutResizeType.Column;
            this.lastMouseLoc = mouseLoc;
            this.resizeProp = TypeDescriptor.GetProperties(this.tableGlyph.Style)[isColumn ? "Width" : "Height"];
            IComponentChangeService service2 = this.serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            if (service2 != null)
            {
                this.changedProp = TypeDescriptor.GetProperties(this.table)[isColumn ? "ColumnStyles" : "RowStyles"];
                int[] widths = isColumn ? this.table.GetColumnWidths() : this.table.GetRowHeights();
                if (this.changedProp != null)
                {
                    this.GetActiveStyleCollection(isColumn);
                    if ((this.styles != null) && this.CanResizeStyle(widths))
                    {
                        IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (host != null)
                        {
                            this.resizeTransaction = host.CreateTransaction(System.Design.SR.GetString("TableLayoutPanelRowColResize", new object[] { isColumn ? "Column" : "Row", this.designer.Control.Site.Name }));
                        }
                        try
                        {
                            int index = this.styles.IndexOf(this.tableGlyph.Style);
                            this.rightStyle.index = this.IndexOfNextStealableStyle(true, index, widths);
                            this.rightStyle.style = (TableLayoutStyle) this.styles[this.rightStyle.index];
                            this.rightStyle.styleProp = TypeDescriptor.GetProperties(this.rightStyle.style)[isColumn ? "Width" : "Height"];
                            this.leftStyle.index = this.IndexOfNextStealableStyle(false, index, widths);
                            this.leftStyle.style = (TableLayoutStyle) this.styles[this.leftStyle.index];
                            this.leftStyle.styleProp = TypeDescriptor.GetProperties(this.leftStyle.style)[isColumn ? "Width" : "Height"];
                            service2.OnComponentChanging(this.table, this.changedProp);
                            goto Label_02CD;
                        }
                        catch (CheckoutException exception)
                        {
                            if ((CheckoutException.Canceled.Equals(exception) && (this.resizeTransaction != null)) && !this.resizeTransaction.Canceled)
                            {
                                this.resizeTransaction.Cancel();
                            }
                            throw;
                        }
                    }
                    return false;
                }
            }
        Label_02CD:
            this.behaviorService.PushCaptureBehavior(this);
            this.pushedBehavior = true;
        Label_02E0:
            return false;
        }

        public override bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (this.pushedBehavior)
            {
                bool columnResize = this.ColumnResize;
                this.GetActiveStyleCollection(columnResize);
                if (this.styles != null)
                {
                    int index = this.rightStyle.index;
                    int num2 = this.leftStyle.index;
                    int num3 = columnResize ? (mouseLoc.X - this.lastMouseLoc.X) : (mouseLoc.Y - this.lastMouseLoc.Y);
                    if (columnResize && (this.table.RightToLeft == RightToLeft.Yes))
                    {
                        num3 *= -1;
                    }
                    if (num3 == 0)
                    {
                        return false;
                    }
                    int[] numArray = columnResize ? this.table.GetColumnWidths() : this.table.GetRowHeights();
                    int[] numArray2 = numArray.Clone() as int[];
                    numArray2[index] -= num3;
                    numArray2[num2] += num3;
                    if ((numArray2[index] < DesignerUtils.MINUMUMSTYLESIZEDRAG) || (numArray2[num2] < DesignerUtils.MINUMUMSTYLESIZEDRAG))
                    {
                        return false;
                    }
                    this.table.SuspendLayout();
                    int num4 = 0;
                    if ((((TableLayoutStyle) this.styles[index]).SizeType == SizeType.Absolute) && (((TableLayoutStyle) this.styles[num2]).SizeType == SizeType.Absolute))
                    {
                        float num5 = numArray2[index];
                        float num6 = (float) this.rightStyle.styleProp.GetValue(this.rightStyle.style);
                        if (num6 != numArray[index])
                        {
                            num5 = Math.Max(num6 - num3, (float) DesignerUtils.MINUMUMSTYLESIZEDRAG);
                        }
                        float num7 = numArray2[num2];
                        float num8 = (float) this.leftStyle.styleProp.GetValue(this.leftStyle.style);
                        if (num8 != numArray[num2])
                        {
                            num7 = Math.Max(num8 + num3, (float) DesignerUtils.MINUMUMSTYLESIZEDRAG);
                        }
                        this.rightStyle.styleProp.SetValue(this.rightStyle.style, num5);
                        this.leftStyle.styleProp.SetValue(this.leftStyle.style, num7);
                    }
                    else if ((((TableLayoutStyle) this.styles[index]).SizeType == SizeType.Percent) && (((TableLayoutStyle) this.styles[num2]).SizeType == SizeType.Percent))
                    {
                        for (int j = 0; j < this.styles.Count; j++)
                        {
                            if (((TableLayoutStyle) this.styles[j]).SizeType == SizeType.Percent)
                            {
                                num4 += numArray[j];
                            }
                        }
                        for (int k = 0; k < 2; k++)
                        {
                            int num11 = (k == 0) ? index : num2;
                            float num12 = (numArray2[num11] * 100f) / ((float) num4);
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.styles[num11])[columnResize ? "Width" : "Height"];
                            if (descriptor != null)
                            {
                                descriptor.SetValue(this.styles[num11], num12);
                            }
                        }
                    }
                    else
                    {
                        int num13 = (((TableLayoutStyle) this.styles[index]).SizeType == SizeType.Absolute) ? index : num2;
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.styles[num13])[columnResize ? "Width" : "Height"];
                        if (descriptor2 != null)
                        {
                            float num14 = numArray2[num13];
                            float num15 = (float) descriptor2.GetValue(this.styles[num13]);
                            if (num15 != numArray[num13])
                            {
                                num14 = Math.Max((num13 == index) ? (num15 - num3) : (num15 + num3), (float) DesignerUtils.MINUMUMSTYLESIZEDRAG);
                            }
                            descriptor2.SetValue(this.styles[num13], num14);
                        }
                    }
                    this.table.ResumeLayout(true);
                    bool flag2 = true;
                    int[] numArray3 = columnResize ? this.table.GetColumnWidths() : this.table.GetRowHeights();
                    for (int i = 0; i < numArray3.Length; i++)
                    {
                        if ((numArray3[i] == numArray[i]) && (numArray2[i] != numArray[i]))
                        {
                            flag2 = false;
                        }
                    }
                    if (flag2)
                    {
                        this.lastMouseLoc = mouseLoc;
                    }
                }
                else
                {
                    this.lastMouseLoc = mouseLoc;
                }
            }
            return false;
        }

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            if (this.pushedBehavior)
            {
                this.FinishResize();
                if (this.resizeTransaction != null)
                {
                    DesignerTransaction resizeTransaction = this.resizeTransaction;
                    this.resizeTransaction = null;
                    using (resizeTransaction)
                    {
                        resizeTransaction.Commit();
                    }
                    this.resizeProp = null;
                }
            }
            return false;
        }

        private bool ColumnResize
        {
            get
            {
                bool flag = false;
                if (this.tableGlyph != null)
                {
                    flag = this.tableGlyph.Type == TableLayoutPanelResizeGlyph.TableLayoutResizeType.Column;
                }
                return flag;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct StyleHelper
        {
            public int index;
            public PropertyDescriptor styleProp;
            public TableLayoutStyle style;
        }
    }
}

