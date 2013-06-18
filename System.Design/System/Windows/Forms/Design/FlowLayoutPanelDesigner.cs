namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class FlowLayoutPanelDesigner : FlowPanelDesigner
    {
        private ChildInfo[] childInfo;
        private ArrayList commonSizes = new ArrayList();
        private ArrayList dragControls;
        private const int iBarHalfSize = 2;
        private const int iBarHatHeight = 3;
        private const int iBarHatWidth = 5;
        private const int iBarLineOffset = 5;
        private const int iBarSpace = 2;
        private int insertIndex;
        private static readonly int InvalidIndex = -1;
        private Point lastMouseLoc;
        private int maxIBarWidth = Math.Max(2, 2);
        private const int minIBar = 10;
        private Point oldP1;
        private Point oldP2;
        private System.Windows.Forms.Control primaryDragControl;

        public FlowLayoutPanelDesigner()
        {
            this.oldP1 = this.oldP2 = Point.Empty;
            this.insertIndex = InvalidIndex;
        }

        private void CreateMarginBoundsList()
        {
            this.commonSizes.Clear();
            if (this.Control.Controls.Count == 0)
            {
                this.childInfo = new ChildInfo[0];
            }
            else
            {
                this.childInfo = new ChildInfo[this.Control.Controls.Count];
                Point point = this.Control.PointToScreen(Point.Empty);
                System.Windows.Forms.FlowDirection direction = this.RTLTranslateFlowDirection(this.Control.FlowDirection);
                bool horizontalFlow = this.HorizontalFlow;
                int x = 0x7fffffff;
                int y = -1;
                int num3 = -1;
                if ((horizontalFlow && (direction == System.Windows.Forms.FlowDirection.RightToLeft)) || (!horizontalFlow && (direction == System.Windows.Forms.FlowDirection.BottomUp)))
                {
                    num3 = 0x7fffffff;
                }
                int index = 0;
                bool flag2 = this.Control.RightToLeft == RightToLeft.Yes;
                index = 0;
                while (index < this.Control.Controls.Count)
                {
                    System.Windows.Forms.Control control = this.Control.Controls[index];
                    Rectangle marginBounds = this.GetMarginBounds(control);
                    Rectangle bounds = control.Bounds;
                    if (horizontalFlow)
                    {
                        bounds.X -= !flag2 ? control.Margin.Left : control.Margin.Right;
                        bounds.Width += control.Margin.Horizontal;
                        bounds.Height--;
                    }
                    else
                    {
                        bounds.Y -= control.Margin.Top;
                        bounds.Height += control.Margin.Vertical;
                        bounds.Width--;
                    }
                    marginBounds.Offset(point.X, point.Y);
                    bounds.Offset(point.X, point.Y);
                    this.childInfo[index].marginBounds = marginBounds;
                    this.childInfo[index].controlBounds = bounds;
                    this.childInfo[index].inSelectionColl = false;
                    if ((this.dragControls != null) && this.dragControls.Contains(control))
                    {
                        this.childInfo[index].inSelectionColl = true;
                    }
                    if (horizontalFlow)
                    {
                        if ((((direction == System.Windows.Forms.FlowDirection.LeftToRight) ? (marginBounds.X < num3) : (marginBounds.X > num3)) && (x > 0)) && (y > 0))
                        {
                            this.commonSizes.Add(new Rectangle(x, y, y - x, index));
                            x = 0x7fffffff;
                            y = -1;
                        }
                        num3 = marginBounds.X;
                        if (marginBounds.Top < x)
                        {
                            x = marginBounds.Top;
                        }
                        if (marginBounds.Bottom > y)
                        {
                            y = marginBounds.Bottom;
                        }
                    }
                    else
                    {
                        if ((((direction == System.Windows.Forms.FlowDirection.TopDown) ? (marginBounds.Y < num3) : (marginBounds.Y > num3)) && (x > 0)) && (y > 0))
                        {
                            this.commonSizes.Add(new Rectangle(x, y, y - x, index));
                            x = 0x7fffffff;
                            y = -1;
                        }
                        num3 = marginBounds.Y;
                        if (marginBounds.Left < x)
                        {
                            x = marginBounds.Left;
                        }
                        if (marginBounds.Right > y)
                        {
                            y = marginBounds.Right;
                        }
                    }
                    index++;
                }
                if ((x > 0) && (y > 0))
                {
                    this.commonSizes.Add(new Rectangle(x, y, y - x, index));
                }
                int num5 = 0;
                for (index = 0; index < this.commonSizes.Count; index++)
                {
                    Rectangle rectangle7;
                Label_043B:
                    rectangle7 = (Rectangle) this.commonSizes[index];
                    if (num5 < rectangle7.Height)
                    {
                        if (horizontalFlow)
                        {
                            Rectangle rectangle3 = (Rectangle) this.commonSizes[index];
                            this.childInfo[num5].marginBounds.Y = rectangle3.X;
                            Rectangle rectangle4 = (Rectangle) this.commonSizes[index];
                            this.childInfo[num5].marginBounds.Height = rectangle4.Width;
                        }
                        else
                        {
                            Rectangle rectangle5 = (Rectangle) this.commonSizes[index];
                            this.childInfo[num5].marginBounds.X = rectangle5.X;
                            Rectangle rectangle6 = (Rectangle) this.commonSizes[index];
                            this.childInfo[num5].marginBounds.Width = rectangle6.Width;
                        }
                        num5++;
                        goto Label_043B;
                    }
                }
            }
        }

        private void EraseIBar()
        {
            this.ReDrawIBar(Point.Empty, Point.Empty);
        }

        private Rectangle GetMarginBounds(System.Windows.Forms.Control control)
        {
            return new Rectangle(control.Bounds.Left - ((this.Control.RightToLeft == RightToLeft.No) ? control.Margin.Left : control.Margin.Right), control.Bounds.Top - control.Margin.Top, control.Bounds.Width + control.Margin.Horizontal, control.Bounds.Height + control.Margin.Vertical);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            if (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                for (int i = 0; i < this.Control.Controls.Count; i++)
                {
                    TypeDescriptor.AddAttributes(this.Control.Controls[i], new Attribute[] { System.ComponentModel.InheritanceAttribute.InheritedReadOnly });
                }
            }
        }

        private void OnChildControlAdded(object sender, ControlEventArgs e)
        {
            if ((this.insertIndex != InvalidIndex) && (this.GetService(typeof(IDesignerHost)) is IDesignerHost))
            {
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(this.Control)["Controls"];
                if ((service != null) && (member != null))
                {
                    service.OnComponentChanging(this.Control, member);
                    this.Control.Controls.SetChildIndex(e.Control, this.insertIndex);
                    this.insertIndex++;
                    service.OnComponentChanged(this.Control, member, null, null);
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            bool flag = false;
            if (((this.dragControls != null) && (this.primaryDragControl != null)) && this.Control.Controls.Contains(this.primaryDragControl))
            {
                flag = true;
            }
            if (!flag)
            {
                if (this.Control != null)
                {
                    this.Control.ControlAdded += new ControlEventHandler(this.OnChildControlAdded);
                }
                try
                {
                    base.OnDragDrop(de);
                }
                finally
                {
                    if (this.Control != null)
                    {
                        this.Control.ControlAdded -= new ControlEventHandler(this.OnChildControlAdded);
                    }
                }
            }
            else
            {
                IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (host != null)
                {
                    string str;
                    DesignerTransaction transaction = null;
                    bool flag2 = de.Effect == DragDropEffects.Copy;
                    ArrayList list = null;
                    ISelectionService service = null;
                    if (this.dragControls.Count == 1)
                    {
                        string componentName = TypeDescriptor.GetComponentName(this.dragControls[0]);
                        if ((componentName == null) || (componentName.Length == 0))
                        {
                            componentName = this.dragControls[0].GetType().Name;
                        }
                        str = System.Design.SR.GetString(flag2 ? "BehaviorServiceCopyControl" : "BehaviorServiceMoveControl", new object[] { componentName });
                    }
                    else
                    {
                        str = System.Design.SR.GetString(flag2 ? "BehaviorServiceCopyControls" : "BehaviorServiceMoveControls", new object[] { this.dragControls.Count });
                    }
                    transaction = host.CreateTransaction(str);
                    try
                    {
                        while ((this.insertIndex < (this.childInfo.Length - 1)) && this.childInfo[this.insertIndex].inSelectionColl)
                        {
                            this.insertIndex++;
                        }
                        IComponentChangeService service2 = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                        PropertyDescriptor member = TypeDescriptor.GetProperties(this.Control)["Controls"];
                        System.Windows.Forms.Control child = null;
                        if (this.insertIndex != this.childInfo.Length)
                        {
                            child = this.Control.Controls[this.insertIndex];
                        }
                        else
                        {
                            this.insertIndex = -1;
                        }
                        if ((service2 != null) && (member != null))
                        {
                            service2.OnComponentChanging(this.Control, member);
                        }
                        if (!flag2)
                        {
                            for (int j = 0; j < this.dragControls.Count; j++)
                            {
                                this.Control.Controls.Remove(this.dragControls[j] as System.Windows.Forms.Control);
                            }
                            if (child != null)
                            {
                                this.insertIndex = this.Control.Controls.GetChildIndex(child, false);
                            }
                        }
                        else
                        {
                            ArrayList objects = new ArrayList();
                            for (int k = 0; k < this.dragControls.Count; k++)
                            {
                                objects.Add(this.dragControls[k]);
                            }
                            objects = DesignerUtils.CopyDragObjects(objects, base.Component.Site) as ArrayList;
                            if (objects == null)
                            {
                                return;
                            }
                            list = new ArrayList();
                            for (int m = 0; m < objects.Count; m++)
                            {
                                list.Add(this.dragControls[m]);
                                if (this.primaryDragControl.Equals(this.dragControls[m] as System.Windows.Forms.Control))
                                {
                                    this.primaryDragControl = objects[m] as System.Windows.Forms.Control;
                                }
                                this.dragControls[m] = objects[m];
                            }
                            service = (ISelectionService) this.GetService(typeof(ISelectionService));
                        }
                        if (this.insertIndex == -1)
                        {
                            this.insertIndex = this.Control.Controls.Count;
                        }
                        this.Control.Controls.Add(this.primaryDragControl);
                        this.Control.Controls.SetChildIndex(this.primaryDragControl, this.insertIndex);
                        this.insertIndex++;
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { this.primaryDragControl }, SelectionTypes.Click | SelectionTypes.Replace);
                        }
                        for (int i = this.dragControls.Count - 1; i >= 0; i--)
                        {
                            if (!this.primaryDragControl.Equals(this.dragControls[i] as System.Windows.Forms.Control))
                            {
                                this.Control.Controls.Add(this.dragControls[i] as System.Windows.Forms.Control);
                                this.Control.Controls.SetChildIndex(this.dragControls[i] as System.Windows.Forms.Control, this.insertIndex);
                                this.insertIndex++;
                                if (service != null)
                                {
                                    service.SetSelectedComponents(new object[] { this.dragControls[i] }, SelectionTypes.Add);
                                }
                            }
                        }
                        if ((service2 != null) && (member != null))
                        {
                            service2.OnComponentChanged(this.Control, member, null, null);
                        }
                        if (list != null)
                        {
                            for (int n = 0; n < list.Count; n++)
                            {
                                this.dragControls[n] = list[n];
                            }
                        }
                        base.OnDragComplete(de);
                        if (transaction != null)
                        {
                            transaction.Commit();
                            transaction = null;
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
            }
            this.insertIndex = InvalidIndex;
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            base.OnDragEnter(de);
            this.insertIndex = InvalidIndex;
            this.lastMouseLoc = Point.Empty;
            this.primaryDragControl = null;
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                int primaryControlIndex = -1;
                this.dragControls = data.GetSortedDragControls(ref primaryControlIndex);
                this.primaryDragControl = this.dragControls[primaryControlIndex] as System.Windows.Forms.Control;
            }
            this.CreateMarginBoundsList();
        }

        protected override void OnDragLeave(EventArgs e)
        {
            this.EraseIBar();
            this.insertIndex = InvalidIndex;
            this.primaryDragControl = null;
            if (this.dragControls != null)
            {
                this.dragControls.Clear();
            }
            base.OnDragLeave(e);
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            base.OnDragOver(de);
            Point mousePosition = System.Windows.Forms.Control.MousePosition;
            if ((!mousePosition.Equals(this.lastMouseLoc) && (this.childInfo != null)) && ((this.childInfo.Length != 0) && (this.commonSizes.Count != 0)))
            {
                Rectangle empty = Rectangle.Empty;
                this.lastMouseLoc = mousePosition;
                Point point2 = this.Control.PointToScreen(new Point(0, 0));
                if (this.Control.RightToLeft == RightToLeft.Yes)
                {
                    point2.X += this.Control.Width;
                }
                this.insertIndex = InvalidIndex;
                int index = 0;
                index = 0;
                while (index < this.childInfo.Length)
                {
                    if (this.childInfo[index].marginBounds.Contains(mousePosition))
                    {
                        empty = this.childInfo[index].controlBounds;
                        break;
                    }
                    index++;
                }
                if (!empty.IsEmpty)
                {
                    this.insertIndex = index;
                    if (this.childInfo[index].inSelectionColl)
                    {
                        this.EraseIBar();
                    }
                    else
                    {
                        switch (this.RTLTranslateFlowDirection(this.Control.FlowDirection))
                        {
                            case System.Windows.Forms.FlowDirection.LeftToRight:
                                this.ReDrawIBar(new Point(empty.Left, empty.Top), new Point(empty.Left, empty.Bottom));
                                break;

                            case System.Windows.Forms.FlowDirection.RightToLeft:
                                this.ReDrawIBar(new Point(empty.Right, empty.Top), new Point(empty.Right, empty.Bottom));
                                break;

                            case System.Windows.Forms.FlowDirection.TopDown:
                                this.ReDrawIBar(new Point(empty.Left, empty.Top), new Point(empty.Right, empty.Top));
                                break;

                            case System.Windows.Forms.FlowDirection.BottomUp:
                                this.ReDrawIBar(new Point(empty.Left, empty.Bottom), new Point(empty.Right, empty.Bottom));
                                break;
                        }
                    }
                }
                else
                {
                    int num2 = this.HorizontalFlow ? point2.Y : point2.X;
                    bool flag = this.Control.RightToLeft == RightToLeft.Yes;
                    for (index = 0; index < this.commonSizes.Count; index++)
                    {
                        if (flag)
                        {
                            Rectangle rectangle2 = (Rectangle) this.commonSizes[index];
                            num2 -= rectangle2.Width;
                        }
                        else
                        {
                            Rectangle rectangle3 = (Rectangle) this.commonSizes[index];
                            num2 += rectangle3.Width;
                        }
                        bool flag2 = false;
                        if (!flag)
                        {
                            flag2 = (this.HorizontalFlow ? mousePosition.Y : mousePosition.X) <= num2;
                        }
                        else
                        {
                            flag2 = (this.HorizontalFlow && (mousePosition.Y <= num2)) || (!this.HorizontalFlow && (mousePosition.X >= num2));
                        }
                        if (flag2)
                        {
                            Rectangle rectangle4 = (Rectangle) this.commonSizes[index];
                            this.insertIndex = rectangle4.Height;
                            empty = this.childInfo[this.insertIndex - 1].controlBounds;
                            if (this.childInfo[this.insertIndex - 1].inSelectionColl)
                            {
                                this.EraseIBar();
                            }
                            else
                            {
                                switch (this.RTLTranslateFlowDirection(this.Control.FlowDirection))
                                {
                                    case System.Windows.Forms.FlowDirection.LeftToRight:
                                        this.ReDrawIBar(new Point(empty.Right, empty.Top), new Point(empty.Right, empty.Bottom));
                                        break;

                                    case System.Windows.Forms.FlowDirection.RightToLeft:
                                        this.ReDrawIBar(new Point(empty.Left, empty.Top), new Point(empty.Left, empty.Bottom));
                                        break;

                                    case System.Windows.Forms.FlowDirection.TopDown:
                                        this.ReDrawIBar(new Point(empty.Left, empty.Bottom), new Point(empty.Right, empty.Bottom));
                                        break;

                                    case System.Windows.Forms.FlowDirection.BottomUp:
                                        this.ReDrawIBar(new Point(empty.Left, empty.Top), new Point(empty.Right, empty.Top));
                                        break;
                                }
                            }
                            break;
                        }
                    }
                }
                if (this.insertIndex == InvalidIndex)
                {
                    this.insertIndex = this.Control.Controls.Count;
                    this.EraseIBar();
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "FlowDirection" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(FlowLayoutPanelDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private void ReDrawIBar(Point p1, Point p2)
        {
            Point point = base.BehaviorService.AdornerWindowToScreen();
            Pen controlText = SystemPens.ControlText;
            if ((this.Control.BackColor != Color.Empty) && (this.Control.BackColor.GetBrightness() < 0.5))
            {
                controlText = SystemPens.ControlLight;
            }
            if (p1 != Point.Empty)
            {
                p1.Offset(-point.X, -point.Y);
                p2.Offset(-point.X, -point.Y);
            }
            if (((p1 != this.oldP1) && (p2 != this.oldP2)) && (this.oldP1 != Point.Empty))
            {
                Rectangle rect = new Rectangle(this.oldP1.X, this.oldP1.Y, (this.oldP2.X - this.oldP1.X) + 1, (this.oldP2.Y - this.oldP1.Y) + 1);
                rect.Inflate(this.maxIBarWidth, this.maxIBarWidth);
                base.BehaviorService.Invalidate(rect);
            }
            this.oldP1 = p1;
            this.oldP2 = p2;
            if (p1 != Point.Empty)
            {
                using (Graphics graphics = base.BehaviorService.AdornerWindowGraphics)
                {
                    if (this.HorizontalFlow)
                    {
                        if (Math.Abs((int) (p1.Y - p2.Y)) <= 10)
                        {
                            graphics.DrawLine(controlText, p1, p2);
                            graphics.DrawLine(controlText, p1.X - 2, p1.Y, p1.X + 2, p1.Y);
                            graphics.DrawLine(controlText, p2.X - 2, p2.Y, p2.X + 2, p2.Y);
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                graphics.DrawLine(controlText, (int) (p1.X - ((4 - (i * 2)) / 2)), (int) (p1.Y + i), (int) (p1.X + ((4 - (i * 2)) / 2)), (int) (p1.Y + i));
                                graphics.DrawLine(controlText, (int) (p2.X - ((4 - (i * 2)) / 2)), (int) (p2.Y - i), (int) (p2.X + ((4 - (i * 2)) / 2)), (int) (p2.Y - i));
                            }
                            graphics.DrawLine(controlText, p1.X, p1.Y, p1.X, (p1.Y + 3) - 1);
                            graphics.DrawLine(controlText, p2.X, p2.Y, p2.X, (p2.Y - 3) + 1);
                            graphics.DrawLine(controlText, p1.X, p1.Y + 5, p2.X, p2.Y - 5);
                        }
                    }
                    else if (Math.Abs((int) (p1.X - p2.X)) <= 10)
                    {
                        graphics.DrawLine(controlText, p1, p2);
                        graphics.DrawLine(controlText, p1.X, p1.Y - 2, p1.X, p1.Y + 2);
                        graphics.DrawLine(controlText, p2.X, p2.Y - 2, p2.X, p2.Y + 2);
                    }
                    else
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            graphics.DrawLine(controlText, (int) (p1.X + j), (int) (p1.Y - ((4 - (j * 2)) / 2)), (int) (p1.X + j), (int) (p1.Y + ((4 - (j * 2)) / 2)));
                            graphics.DrawLine(controlText, (int) (p2.X - j), (int) (p2.Y - ((4 - (j * 2)) / 2)), (int) (p2.X - j), (int) (p2.Y + ((4 - (j * 2)) / 2)));
                        }
                        graphics.DrawLine(controlText, p1.X, p1.Y, (p1.X + 3) - 1, p1.Y);
                        graphics.DrawLine(controlText, p2.X, p2.Y, (p2.X - 3) + 1, p2.Y);
                        graphics.DrawLine(controlText, p1.X + 5, p1.Y, p2.X - 5, p2.Y);
                    }
                }
            }
        }

        private System.Windows.Forms.FlowDirection RTLTranslateFlowDirection(System.Windows.Forms.FlowDirection direction)
        {
            if (this.Control.RightToLeft != RightToLeft.No)
            {
                switch (direction)
                {
                    case System.Windows.Forms.FlowDirection.LeftToRight:
                        return System.Windows.Forms.FlowDirection.RightToLeft;

                    case System.Windows.Forms.FlowDirection.TopDown:
                    case System.Windows.Forms.FlowDirection.BottomUp:
                        return direction;

                    case System.Windows.Forms.FlowDirection.RightToLeft:
                        return System.Windows.Forms.FlowDirection.LeftToRight;
                }
            }
            return direction;
        }

        protected override bool AllowGenericDragBox
        {
            get
            {
                return false;
            }
        }

        protected internal override bool AllowSetChildIndexOnDrop
        {
            get
            {
                return false;
            }
        }

        private FlowLayoutPanel Control
        {
            get
            {
                return (base.Control as FlowLayoutPanel);
            }
        }

        private System.Windows.Forms.FlowDirection FlowDirection
        {
            get
            {
                return this.Control.FlowDirection;
            }
            set
            {
                if (value != this.Control.FlowDirection)
                {
                    base.BehaviorService.Invalidate(base.BehaviorService.ControlRectInAdornerWindow(this.Control));
                    this.Control.FlowDirection = value;
                }
            }
        }

        private bool HorizontalFlow
        {
            get
            {
                if (this.Control.FlowDirection != System.Windows.Forms.FlowDirection.RightToLeft)
                {
                    return (this.Control.FlowDirection == System.Windows.Forms.FlowDirection.LeftToRight);
                }
                return true;
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

        [StructLayout(LayoutKind.Sequential)]
        private struct ChildInfo
        {
            public Rectangle marginBounds;
            public Rectangle controlBounds;
            public bool inSelectionColl;
        }
    }
}

