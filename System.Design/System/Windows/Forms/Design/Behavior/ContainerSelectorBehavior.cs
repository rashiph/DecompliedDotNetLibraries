namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class ContainerSelectorBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private BehaviorService behaviorService;
        private Control containerControl;
        private Point initialDragPoint;
        private bool okToMove;
        private IServiceProvider serviceProvider;
        private bool setInitialDragPoint;

        internal ContainerSelectorBehavior(Control containerControl, IServiceProvider serviceProvider)
        {
            this.Init(containerControl, serviceProvider);
            this.setInitialDragPoint = false;
        }

        internal ContainerSelectorBehavior(Control containerControl, IServiceProvider serviceProvider, bool setInitialDragPoint)
        {
            this.Init(containerControl, serviceProvider);
            this.setInitialDragPoint = setInitialDragPoint;
        }

        private Point DetermineInitialDragPoint(Point mouseLoc)
        {
            if (this.setInitialDragPoint)
            {
                Point p = this.behaviorService.ControlToAdornerWindow(this.containerControl);
                p = this.behaviorService.AdornerWindowPointToScreen(p);
                Cursor.Position = p;
                return p;
            }
            return mouseLoc;
        }

        private void Init(Control containerControl, IServiceProvider serviceProvider)
        {
            this.behaviorService = (BehaviorService) serviceProvider.GetService(typeof(BehaviorService));
            if (this.behaviorService != null)
            {
                this.containerControl = containerControl;
                this.serviceProvider = serviceProvider;
                this.initialDragPoint = Point.Empty;
                this.okToMove = false;
            }
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (button == MouseButtons.Left)
            {
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if ((service != null) && !this.containerControl.Equals(service.PrimarySelection as Control))
                {
                    service.SetSelectedComponents(new object[] { this.containerControl }, SelectionTypes.Toggle | SelectionTypes.Click);
                    ContainerSelectorGlyph glyph = g as ContainerSelectorGlyph;
                    if (glyph == null)
                    {
                        return false;
                    }
                    using (BehaviorServiceAdornerCollectionEnumerator enumerator = this.behaviorService.Adorners.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            foreach (Glyph glyph2 in enumerator.Current.Glyphs)
                            {
                                ContainerSelectorGlyph glyph3 = glyph2 as ContainerSelectorGlyph;
                                if ((glyph3 != null) && !glyph3.Equals(glyph))
                                {
                                    ContainerSelectorBehavior relatedBehavior = glyph3.RelatedBehavior as ContainerSelectorBehavior;
                                    ContainerSelectorBehavior behavior2 = glyph.RelatedBehavior as ContainerSelectorBehavior;
                                    if (((relatedBehavior != null) && (behavior2 != null)) && behavior2.ContainerControl.Equals(relatedBehavior.ContainerControl))
                                    {
                                        relatedBehavior.OkToMove = true;
                                        relatedBehavior.InitialDragPoint = this.DetermineInitialDragPoint(mouseLoc);
                                        continue;
                                    }
                                }
                            }
                        }
                        goto Label_0167;
                    }
                }
                this.InitialDragPoint = this.DetermineInitialDragPoint(mouseLoc);
                this.OkToMove = true;
            }
        Label_0167:
            return false;
        }

        public override bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if ((button == MouseButtons.Left) && this.OkToMove)
            {
                if (this.InitialDragPoint == Point.Empty)
                {
                    this.InitialDragPoint = this.DetermineInitialDragPoint(mouseLoc);
                }
                Size size = new Size(Math.Abs((int) (mouseLoc.X - this.InitialDragPoint.X)), Math.Abs((int) (mouseLoc.Y - this.InitialDragPoint.Y)));
                if ((size.Width >= (DesignerUtils.MinDragSize.Width / 2)) || (size.Height >= (DesignerUtils.MinDragSize.Height / 2)))
                {
                    Point initialMouseLocation = this.behaviorService.AdornerWindowToScreen();
                    initialMouseLocation.Offset(mouseLoc.X, mouseLoc.Y);
                    this.StartDragOperation(initialMouseLocation);
                }
            }
            return false;
        }

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            this.InitialDragPoint = Point.Empty;
            this.OkToMove = false;
            return false;
        }

        private void StartDragOperation(Point initialMouseLocation)
        {
            ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
            IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if ((service != null) && (host != null))
            {
                Control parent = this.containerControl.Parent;
                ArrayList dragComponents = new ArrayList();
                foreach (IComponent component in service.GetSelectedComponents())
                {
                    Control control2 = component as Control;
                    if ((control2 != null) && control2.Parent.Equals(parent))
                    {
                        ControlDesigner designer = host.GetDesigner(control2) as ControlDesigner;
                        if ((designer != null) && ((designer.SelectionRules & SelectionRules.Moveable) != SelectionRules.None))
                        {
                            dragComponents.Add(control2);
                        }
                    }
                }
                if (dragComponents.Count > 0)
                {
                    Point point;
                    if (this.setInitialDragPoint)
                    {
                        point = this.behaviorService.ControlToAdornerWindow(this.containerControl);
                        point = this.behaviorService.AdornerWindowPointToScreen(point);
                    }
                    else
                    {
                        point = initialMouseLocation;
                    }
                    DropSourceBehavior dropSourceBehavior = new DropSourceBehavior(dragComponents, this.containerControl.Parent, point);
                    try
                    {
                        this.behaviorService.DoDragDrop(dropSourceBehavior);
                    }
                    finally
                    {
                        this.OkToMove = false;
                        this.InitialDragPoint = Point.Empty;
                    }
                }
            }
        }

        public Control ContainerControl
        {
            get
            {
                return this.containerControl;
            }
        }

        public Point InitialDragPoint
        {
            get
            {
                return this.initialDragPoint;
            }
            set
            {
                this.initialDragPoint = value;
            }
        }

        public bool OkToMove
        {
            get
            {
                return this.okToMove;
            }
            set
            {
                this.okToMove = value;
            }
        }
    }
}

