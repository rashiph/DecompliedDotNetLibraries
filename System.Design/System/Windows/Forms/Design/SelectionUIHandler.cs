namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal abstract class SelectionUIHandler
    {
        private Control[] dragControls;
        private Rectangle dragOffset = Rectangle.Empty;
        private const int MinControlHeight = 3;
        private const int MinControlWidth = 3;
        private BoundsInfo[] originalCoords;
        private SelectionRules rules;

        protected SelectionUIHandler()
        {
        }

        public virtual bool BeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
        {
            this.dragOffset = new Rectangle();
            this.originalCoords = null;
            this.rules = rules;
            this.dragControls = new Control[components.Length];
            for (int i = 0; i < components.Length; i++)
            {
                this.dragControls[i] = this.GetControl((IComponent) components[i]);
            }
            bool flag = false;
            IComponent component = this.GetComponent();
            for (int j = 0; j < components.Length; j++)
            {
                if (components[j] == component)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Control control = this.GetControl();
                Size currentSnapSize = this.GetCurrentSnapSize();
                Rectangle rectangle = control.RectangleToScreen(control.ClientRectangle);
                rectangle.Inflate(currentSnapSize.Width, currentSnapSize.Height);
                ScrollableControl control2 = this.GetControl() as ScrollableControl;
                if ((control2 != null) && control2.AutoScroll)
                {
                    Rectangle virtualScreen = SystemInformation.VirtualScreen;
                    rectangle.Width = virtualScreen.Width;
                    rectangle.Height = virtualScreen.Height;
                }
            }
            return true;
        }

        private void CancelControlMove(Control[] controls, BoundsInfo[] bounds)
        {
            Rectangle rectangle = new Rectangle();
            for (int i = 0; i < controls.Length; i++)
            {
                Control parent = controls[i].Parent;
                if (parent != null)
                {
                    parent.SuspendLayout();
                }
                rectangle.X = bounds[i].X;
                rectangle.Y = bounds[i].Y;
                rectangle.Width = bounds[i].Width;
                rectangle.Height = bounds[i].Height;
                controls[i].Bounds = rectangle;
            }
            for (int j = 0; j < controls.Length; j++)
            {
                Control control2 = controls[j].Parent;
                if (control2 != null)
                {
                    control2.ResumeLayout();
                }
            }
        }

        public virtual void DragMoved(object[] components, Rectangle offset)
        {
            this.dragOffset = offset;
            this.MoveControls(components, false, false);
        }

        public virtual void EndDrag(object[] components, bool cancel)
        {
            try
            {
                this.MoveControls(components, cancel, true);
            }
            catch (CheckoutException exception)
            {
                if (exception != CheckoutException.Canceled)
                {
                    throw exception;
                }
                this.MoveControls(components, true, false);
            }
        }

        protected abstract IComponent GetComponent();
        protected abstract Control GetControl();
        protected abstract Control GetControl(IComponent component);
        protected abstract Size GetCurrentSnapSize();
        protected abstract object GetService(System.Type serviceType);
        protected abstract bool GetShouldSnapToGrid();
        public abstract Rectangle GetUpdatedRect(Rectangle orignalRect, Rectangle dragRect, bool updateSize);
        private void MoveControls(object[] components, bool cancel, bool finalMove)
        {
            Control[] dragControls = this.dragControls;
            Rectangle dragOffset = this.dragOffset;
            BoundsInfo[] originalCoords = this.originalCoords;
            Point point = new Point();
            if (finalMove)
            {
                Cursor.Clip = Rectangle.Empty;
                this.dragOffset = Rectangle.Empty;
                this.dragControls = null;
                this.originalCoords = null;
            }
            if (!dragOffset.IsEmpty && (((!finalMove || (dragOffset.X != 0)) || ((dragOffset.Y != 0) || (dragOffset.Width != 0))) || (dragOffset.Height != 0)))
            {
                if (cancel)
                {
                    this.CancelControlMove(dragControls, originalCoords);
                }
                else
                {
                    if ((this.originalCoords == null) && !finalMove)
                    {
                        this.originalCoords = new BoundsInfo[dragControls.Length];
                        for (int k = 0; k < dragControls.Length; k++)
                        {
                            this.originalCoords[k] = new BoundsInfo(dragControls[k]);
                        }
                        originalCoords = this.originalCoords;
                    }
                    for (int i = 0; i < dragControls.Length; i++)
                    {
                        Control parent = dragControls[i].Parent;
                        if (parent != null)
                        {
                            parent.SuspendLayout();
                        }
                        BoundsInfo info = originalCoords[i];
                        point.X = info.lastRequestedX;
                        point.Y = info.lastRequestedY;
                        if (!finalMove)
                        {
                            info.lastRequestedX += dragOffset.X;
                            info.lastRequestedY += dragOffset.Y;
                            info.lastRequestedWidth += dragOffset.Width;
                            info.lastRequestedHeight += dragOffset.Height;
                        }
                        int lastRequestedX = info.lastRequestedX;
                        int lastRequestedY = info.lastRequestedY;
                        int lastRequestedWidth = info.lastRequestedWidth;
                        int lastRequestedHeight = info.lastRequestedHeight;
                        Rectangle bounds = dragControls[i].Bounds;
                        if ((this.rules & SelectionRules.Moveable) == SelectionRules.None)
                        {
                            Size currentSnapSize;
                            if (this.GetShouldSnapToGrid())
                            {
                                currentSnapSize = this.GetCurrentSnapSize();
                            }
                            else
                            {
                                currentSnapSize = new Size(1, 1);
                            }
                            if (lastRequestedWidth < currentSnapSize.Width)
                            {
                                lastRequestedWidth = currentSnapSize.Width;
                                lastRequestedX = bounds.X;
                            }
                            if (lastRequestedHeight < currentSnapSize.Height)
                            {
                                lastRequestedHeight = currentSnapSize.Height;
                                lastRequestedY = bounds.Y;
                            }
                        }
                        IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                        if (dragControls[i] == service.RootComponent)
                        {
                            lastRequestedX = 0;
                            lastRequestedY = 0;
                        }
                        Rectangle rectangle3 = this.GetUpdatedRect(bounds, new Rectangle(lastRequestedX, lastRequestedY, lastRequestedWidth, lastRequestedHeight), true);
                        Rectangle rectangle4 = bounds;
                        if ((this.rules & SelectionRules.Moveable) != SelectionRules.None)
                        {
                            rectangle4.X = rectangle3.X;
                            rectangle4.Y = rectangle3.Y;
                        }
                        else
                        {
                            if ((this.rules & SelectionRules.TopSizeable) != SelectionRules.None)
                            {
                                rectangle4.Y = rectangle3.Y;
                                rectangle4.Height = rectangle3.Height;
                            }
                            if ((this.rules & SelectionRules.BottomSizeable) != SelectionRules.None)
                            {
                                rectangle4.Height = rectangle3.Height;
                            }
                            if ((this.rules & SelectionRules.LeftSizeable) != SelectionRules.None)
                            {
                                rectangle4.X = rectangle3.X;
                                rectangle4.Width = rectangle3.Width;
                            }
                            if ((this.rules & SelectionRules.RightSizeable) != SelectionRules.None)
                            {
                                rectangle4.Width = rectangle3.Width;
                            }
                        }
                        bool flag = (dragOffset.X != 0) || (dragOffset.Y != 0);
                        bool flag2 = (dragOffset.Width != 0) || (dragOffset.Height != 0);
                        if (flag && flag2)
                        {
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(components[i])["Bounds"];
                            if ((descriptor != null) && !descriptor.IsReadOnly)
                            {
                                if (finalMove)
                                {
                                    object component = components[i];
                                    descriptor.SetValue(component, rectangle4);
                                }
                                else
                                {
                                    dragControls[i].Bounds = rectangle4;
                                }
                                flag = flag2 = false;
                            }
                        }
                        if (flag)
                        {
                            point.X = rectangle4.X;
                            point.Y = rectangle4.Y;
                            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(components[i])["TrayLocation"];
                            if ((descriptor2 != null) && !descriptor2.IsReadOnly)
                            {
                                descriptor2.SetValue(components[i], point);
                            }
                            else
                            {
                                PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(components[i])["Left"];
                                PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(components[i])["Top"];
                                if ((descriptor4 != null) && !descriptor4.IsReadOnly)
                                {
                                    if (finalMove)
                                    {
                                        object obj3 = components[i];
                                        descriptor4.SetValue(obj3, point.Y);
                                    }
                                    else
                                    {
                                        dragControls[i].Top = point.Y;
                                    }
                                }
                                if ((descriptor3 != null) && !descriptor3.IsReadOnly)
                                {
                                    if (finalMove)
                                    {
                                        object obj4 = components[i];
                                        descriptor3.SetValue(obj4, point.X);
                                    }
                                    else
                                    {
                                        dragControls[i].Left = point.X;
                                    }
                                }
                                if ((descriptor3 == null) || (descriptor4 == null))
                                {
                                    PropertyDescriptor descriptor5 = TypeDescriptor.GetProperties(components[i])["Location"];
                                    if ((descriptor5 != null) && !descriptor5.IsReadOnly)
                                    {
                                        descriptor5.SetValue(components[i], point);
                                    }
                                }
                            }
                        }
                        if (flag2)
                        {
                            Size size2 = new Size(Math.Max(3, rectangle4.Width), Math.Max(3, rectangle4.Height));
                            PropertyDescriptor descriptor6 = TypeDescriptor.GetProperties(components[i])["Width"];
                            PropertyDescriptor descriptor7 = TypeDescriptor.GetProperties(components[i])["Height"];
                            if (((descriptor6 != null) && !descriptor6.IsReadOnly) && (size2.Width != ((int) descriptor6.GetValue(components[i]))))
                            {
                                if (finalMove)
                                {
                                    object obj5 = components[i];
                                    descriptor6.SetValue(obj5, size2);
                                }
                                else
                                {
                                    dragControls[i].Width = size2.Width;
                                }
                            }
                            if (((descriptor7 != null) && !descriptor7.IsReadOnly) && (size2.Height != ((int) descriptor7.GetValue(components[i]))))
                            {
                                if (finalMove)
                                {
                                    object obj6 = components[i];
                                    descriptor7.SetValue(obj6, size2);
                                }
                                else
                                {
                                    dragControls[i].Height = size2.Height;
                                }
                            }
                        }
                    }
                    for (int j = 0; j < dragControls.Length; j++)
                    {
                        Control control2 = dragControls[j].Parent;
                        if (control2 != null)
                        {
                            control2.ResumeLayout();
                            control2.Update();
                        }
                        dragControls[j].Update();
                    }
                }
            }
        }

        public virtual void OleDragDrop(DragEventArgs de)
        {
        }

        public virtual void OleDragEnter(DragEventArgs de)
        {
        }

        public virtual void OleDragLeave()
        {
        }

        public virtual void OleDragOver(DragEventArgs de)
        {
        }

        public bool QueryBeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                try
                {
                    if ((components != null) && (components.Length > 0))
                    {
                        foreach (object obj2 in components)
                        {
                            service.OnComponentChanging(obj2, TypeDescriptor.GetProperties(obj2)["Location"]);
                            PropertyDescriptor member = TypeDescriptor.GetProperties(obj2)["Size"];
                            if ((member != null) && member.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden))
                            {
                                member = TypeDescriptor.GetProperties(obj2)["ClientSize"];
                            }
                            service.OnComponentChanging(obj2, member);
                        }
                    }
                    else
                    {
                        service.OnComponentChanging(this.GetComponent(), null);
                    }
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                    return false;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            return ((components != null) && (components.Length > 0));
        }

        public abstract void SetCursor();

        private class BoundsInfo
        {
            public int Height;
            public int lastRequestedHeight;
            public int lastRequestedWidth;
            public int lastRequestedX;
            public int lastRequestedY;
            public int Width;
            public int X;
            public int Y;

            public BoundsInfo(Control control)
            {
                Size size;
                Point location;
                this.lastRequestedX = -1;
                this.lastRequestedY = -1;
                this.lastRequestedWidth = -1;
                this.lastRequestedHeight = -1;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control)["Size"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control)["Location"];
                if (descriptor != null)
                {
                    size = (Size) descriptor.GetValue(control);
                }
                else
                {
                    size = control.Size;
                }
                if (descriptor2 != null)
                {
                    location = (Point) descriptor2.GetValue(control);
                }
                else
                {
                    location = control.Location;
                }
                this.X = location.X;
                this.Y = location.Y;
                this.Width = size.Width;
                this.Height = size.Height;
                this.lastRequestedX = this.X;
                this.lastRequestedY = this.Y;
                this.lastRequestedWidth = this.Width;
                this.lastRequestedHeight = this.Height;
            }

            public override string ToString()
            {
                return ("{X=" + this.X.ToString(CultureInfo.CurrentCulture) + ", Y=" + this.Y.ToString(CultureInfo.CurrentCulture) + ", Width=" + this.Width.ToString(CultureInfo.CurrentCulture) + ", Height=" + this.Height.ToString(CultureInfo.CurrentCulture) + "}");
            }
        }
    }
}

