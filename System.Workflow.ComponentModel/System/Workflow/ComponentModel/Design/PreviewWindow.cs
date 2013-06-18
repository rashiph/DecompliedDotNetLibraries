namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    internal sealed class PreviewWindow
    {
        private PreviewWindowAccessibleObject accessibilityObject;
        private Rectangle bounds = Rectangle.Empty;
        private Rectangle canvasBounds = Rectangle.Empty;
        private ActivityPreviewDesigner parentDesigner;
        private System.Drawing.Size previewDescTextSize = System.Drawing.Size.Empty;
        private Activity previewedActivity;
        private Image previewedActivityImage;
        private bool previewMode = true;
        private Rectangle previewModeButtonRectangle = Rectangle.Empty;
        private Rectangle previewModeDescRectangle = Rectangle.Empty;
        private IServiceProvider serviceProvider;

        public event EventHandler PreviewModeChanged;

        public PreviewWindow(ActivityPreviewDesigner parent)
        {
            this.parentDesigner = parent;
            this.serviceProvider = this.parentDesigner.Activity.Site;
        }

        public void Draw(Graphics graphics, Rectangle viewPort)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme != null)
            {
                System.Drawing.Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, this.PreviewModeDescription, this.previewModeDescRectangle, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, designerTheme.ForegroundBrush);
                graphics.DrawRectangle(Pens.Black, (int) (this.previewModeButtonRectangle.Left - 1), (int) (this.previewModeButtonRectangle.Top - 1), (int) (this.previewModeButtonRectangle.Width + 1), (int) (this.previewModeButtonRectangle.Height + 1));
                ActivityDesignerPaint.Draw3DButton(graphics, null, this.previewModeButtonRectangle, 1f, !this.PreviewMode ? ButtonState.Pushed : ButtonState.Normal);
                Image image = this.PreviewMode ? ActivityPreviewDesignerTheme.PreviewButtonImage : ActivityPreviewDesignerTheme.EditButtonImage;
                ActivityDesignerPaint.DrawImage(graphics, image, new Rectangle(this.previewModeButtonRectangle.Left + 2, this.previewModeButtonRectangle.Top + 2, this.previewModeButtonRectangle.Width - 4, this.previewModeButtonRectangle.Height - 4), DesignerContentAlignment.Center);
                graphics.FillRectangle(designerTheme.PreviewBackgroundBrush, this.canvasBounds);
                if (this.PreviewMode)
                {
                    graphics.DrawRectangle(designerTheme.PreviewBorderPen, this.canvasBounds);
                }
                else
                {
                    Rectangle canvasBounds = this.canvasBounds;
                    canvasBounds.Inflate(2, 2);
                    graphics.DrawRectangle(SystemPens.ControlDark, canvasBounds);
                    canvasBounds.Inflate(-1, -1);
                    graphics.DrawLine(SystemPens.ControlDarkDark, canvasBounds.Left, canvasBounds.Top, canvasBounds.Left, canvasBounds.Bottom);
                    graphics.DrawLine(SystemPens.ControlDarkDark, canvasBounds.Left, canvasBounds.Top, canvasBounds.Right, canvasBounds.Top);
                    graphics.DrawLine(SystemPens.ControlLight, canvasBounds.Right, canvasBounds.Top, canvasBounds.Right, canvasBounds.Bottom);
                    graphics.DrawLine(SystemPens.ControlLight, canvasBounds.Left, canvasBounds.Bottom, canvasBounds.Right, canvasBounds.Bottom);
                    canvasBounds.Inflate(-1, -1);
                    graphics.DrawLine(SystemPens.ControlLight, canvasBounds.Left, canvasBounds.Top, canvasBounds.Left, canvasBounds.Bottom);
                    graphics.DrawLine(SystemPens.ControlLight, canvasBounds.Left, canvasBounds.Top, canvasBounds.Right, canvasBounds.Top);
                    graphics.FillRectangle(designerTheme.PreviewBackgroundBrush, canvasBounds);
                }
                if (this.PreviewDesigner == null)
                {
                    Rectangle boundingRect = this.canvasBounds;
                    boundingRect.Inflate(-margin.Width, -margin.Height);
                    string text = DR.GetString("SelectActivityDesc", new object[0]);
                    ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, text, boundingRect, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, designerTheme.ForegroundBrush);
                }
                if (this.PreviewMode)
                {
                    Image image2 = this.GeneratePreview(graphics);
                    if (image2 != null)
                    {
                        Rectangle empty = Rectangle.Empty;
                        System.Drawing.Size size2 = new System.Drawing.Size(this.canvasBounds.Width - (2 * margin.Width), this.canvasBounds.Height - (2 * margin.Height));
                        double num = ((double) image2.Width) / ((double) size2.Width);
                        num = Math.Max(Math.Max(num, ((double) image2.Height) / ((double) size2.Height)), 1.2999999523162842);
                        empty.Width = Convert.ToInt32(Math.Ceiling((double) (((double) image2.Width) / num)));
                        empty.Height = Convert.ToInt32(Math.Ceiling((double) (((double) image2.Height) / num)));
                        empty.X = (this.canvasBounds.Left + (this.canvasBounds.Width / 2)) - (empty.Width / 2);
                        empty.Y = (this.canvasBounds.Top + (this.canvasBounds.Height / 2)) - (empty.Height / 2);
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(image2, empty, new Rectangle(Point.Empty, image2.Size), GraphicsUnit.Pixel);
                    }
                    Rectangle destination = this.canvasBounds;
                    destination.Inflate(-margin.Width, -margin.Height);
                    ActivityDesignerPaint.DrawImage(graphics, ActivityPreviewDesignerTheme.PreviewImage, destination, DesignerContentAlignment.TopLeft);
                }
                else if (this.PreviewDesigner != null)
                {
                    Rectangle bounds = this.PreviewDesigner.Bounds;
                    bounds.Inflate(margin.Width, margin.Height);
                    using (PaintEventArgs args = new PaintEventArgs(graphics, bounds))
                    {
                        ((IWorkflowDesignerMessageSink) this.PreviewDesigner).OnPaint(args, bounds);
                    }
                }
            }
        }

        private void EnsureValidDesignerPreview(ActivityDesigner designer)
        {
            CompositeActivityDesigner designer2 = designer as CompositeActivityDesigner;
            if ((designer2 != null) && designer2.Expanded)
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                if (designerTheme != null)
                {
                    System.Drawing.Size previewWindowSize = designerTheme.PreviewWindowSize;
                    System.Drawing.Size size = designer2.Size;
                    float num = ((float) previewWindowSize.Width) / ((float) size.Width);
                    if (Math.Min(num, ((float) previewWindowSize.Height) / ((float) size.Height)) < 0.1f)
                    {
                        if (!designer2.CanExpandCollapse && (designer2.ContainedDesigners.Count > 0))
                        {
                            designer2 = designer2.ContainedDesigners[0] as CompositeActivityDesigner;
                        }
                        if (designer2 != null)
                        {
                            designer2.Expanded = false;
                        }
                    }
                }
            }
        }

        private Image GeneratePreview(Graphics graphics)
        {
            if (this.previewedActivityImage == null)
            {
                ActivityDesigner previewDesigner = this.PreviewDesigner;
                if ((previewDesigner != null) && (this.parentDesigner != null))
                {
                    this.previewedActivityImage = previewDesigner.GetPreviewImage(graphics);
                }
            }
            return this.previewedActivityImage;
        }

        public void OnLayoutSize(Graphics graphics, int minWidth)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme != null)
            {
                System.Drawing.Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                if (!this.PreviewMode && (this.PreviewDesigner != null))
                {
                    this.canvasBounds.Size = this.PreviewDesigner.Bounds.Size;
                    this.canvasBounds.Inflate(margin.Width * 2, margin.Height * 2);
                    int introduced12 = Math.Max(this.canvasBounds.Width, designerTheme.PreviewWindowSize.Width);
                    this.canvasBounds.Size = new System.Drawing.Size(introduced12, Math.Max(this.canvasBounds.Height, designerTheme.PreviewWindowSize.Height));
                }
                else
                {
                    this.canvasBounds.Size = designerTheme.PreviewWindowSize;
                }
                this.canvasBounds.Width = Math.Max(this.canvasBounds.Width, minWidth);
                SizeF ef = graphics.MeasureString(this.PreviewModeDescription, designerTheme.Font);
                int width = Convert.ToInt32(Math.Ceiling((double) ef.Width));
                this.previewDescTextSize = new System.Drawing.Size(width, Convert.ToInt32(Math.Ceiling((double) ef.Height)));
                this.previewDescTextSize.Width = Math.Min((this.canvasBounds.Size.Width - margin.Width) - this.previewModeButtonRectangle.Size.Width, this.previewDescTextSize.Width);
                this.previewModeDescRectangle.Size = this.previewDescTextSize;
                this.previewModeButtonRectangle.Height = Math.Min(designerTheme.PreviewButtonSize.Height, this.previewDescTextSize.Height);
                this.previewModeButtonRectangle.Width = this.previewModeButtonRectangle.Size.Height;
                System.Drawing.Size empty = System.Drawing.Size.Empty;
                empty.Width = this.canvasBounds.Width + (2 * margin.Width);
                empty.Height = Math.Max(this.previewModeButtonRectangle.Size.Height, this.previewDescTextSize.Height);
                empty.Height += margin.Height;
                empty.Height += this.canvasBounds.Height;
                this.bounds.Size = empty;
            }
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (this.PreviewModeButtonRectangle.Contains(new Point(e.X, e.Y)))
            {
                this.PreviewMode = !this.PreviewMode;
            }
        }

        public void Refresh()
        {
            if (this.previewedActivityImage != null)
            {
                this.previewedActivityImage.Dispose();
                this.previewedActivityImage = null;
            }
            if (this.serviceProvider != null)
            {
                WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (service != null)
                {
                    service.InvalidateLogicalRectangle(this.bounds);
                }
            }
        }

        public AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new PreviewWindowAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public Rectangle Bounds
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.bounds;
            }
        }

        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }
            set
            {
                System.Drawing.Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                this.bounds.Location = value;
                int num = Math.Max(this.previewModeDescRectangle.Height, this.previewModeButtonRectangle.Height);
                Point empty = Point.Empty;
                empty.X = (((this.bounds.Left + (this.bounds.Width / 2)) - (this.previewModeDescRectangle.Width / 2)) + this.previewModeButtonRectangle.Width) + margin.Width;
                empty.Y = (this.bounds.Top + (num / 2)) - (this.previewModeDescRectangle.Height / 2);
                this.previewModeDescRectangle.Location = empty;
                Point point2 = Point.Empty;
                point2.X = empty.X - (this.previewModeButtonRectangle.Width + margin.Width);
                point2.Y = (this.bounds.Top + (num / 2)) - (this.previewModeButtonRectangle.Height / 2);
                this.previewModeButtonRectangle.Location = point2;
                this.canvasBounds.Location = new Point((value.X + (this.bounds.Width / 2)) - (this.canvasBounds.Width / 2), this.previewModeDescRectangle.Bottom + margin.Height);
                if (this.PreviewDesigner != null)
                {
                    Point point3 = Point.Empty;
                    point3.X = (this.canvasBounds.Left + (this.canvasBounds.Width / 2)) - (this.PreviewDesigner.Size.Width / 2);
                    point3.Y = (this.canvasBounds.Top + (this.canvasBounds.Height / 2)) - (this.PreviewDesigner.Size.Height / 2);
                    this.PreviewDesigner.Location = point3;
                }
            }
        }

        private ActivityDesigner PreviewDesigner
        {
            get
            {
                return ActivityDesigner.GetDesigner(this.previewedActivity);
            }
        }

        public Activity PreviewedActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewedActivity;
            }
            set
            {
                if (this.previewedActivity != value)
                {
                    this.previewedActivity = value;
                    if (this.previewedActivityImage != null)
                    {
                        this.previewedActivityImage.Dispose();
                        this.previewedActivityImage = null;
                    }
                    if (this.serviceProvider != null)
                    {
                        WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                        if (service != null)
                        {
                            service.PerformLayout(false);
                        }
                    }
                }
            }
        }

        public bool PreviewMode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.previewMode;
            }
            set
            {
                if (this.previewMode != value)
                {
                    this.previewMode = value;
                    if (this.previewMode)
                    {
                        this.EnsureValidDesignerPreview(this.PreviewDesigner);
                        if (this.previewedActivityImage != null)
                        {
                            this.previewedActivityImage.Dispose();
                            this.previewedActivityImage = null;
                        }
                    }
                    if (this.PreviewModeChanged != null)
                    {
                        this.PreviewModeChanged(this, EventArgs.Empty);
                    }
                    if (this.serviceProvider != null)
                    {
                        WorkflowView service = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                        if (service != null)
                        {
                            service.PerformLayout(false);
                        }
                    }
                }
            }
        }

        private Rectangle PreviewModeButtonRectangle
        {
            get
            {
                return this.previewModeButtonRectangle;
            }
        }

        private string PreviewModeDescription
        {
            get
            {
                string str = this.PreviewMode ? DR.GetString("PreviewMode", new object[0]) : DR.GetString("EditMode", new object[0]);
                CompositeActivity activity = (this.parentDesigner != null) ? (this.parentDesigner.Activity as CompositeActivity) : null;
                if (activity == null)
                {
                    return str;
                }
                IComponent component = (this.PreviewDesigner != null) ? this.PreviewDesigner.Activity : null;
                if (component == null)
                {
                    return str;
                }
                List<Activity> list = new List<Activity>();
                foreach (Activity activity2 in activity.Activities)
                {
                    if (!Helpers.IsAlternateFlowActivity(activity2))
                    {
                        list.Add(activity2);
                    }
                }
                int num = list.IndexOf(component as Activity) + 1;
                string str2 = str;
                return (str2 + " [" + num.ToString(CultureInfo.CurrentCulture) + "/" + list.Count.ToString(CultureInfo.CurrentCulture) + "]");
            }
        }

        public System.Drawing.Size Size
        {
            get
            {
                return this.bounds.Size;
            }
        }

        private sealed class PreviewWindowAccessibleObject : AccessibleObject
        {
            private PreviewWindow previewWindow;

            internal PreviewWindowAccessibleObject(PreviewWindow previewWindow)
            {
                this.previewWindow = previewWindow;
            }

            public override void DoDefaultAction()
            {
                this.previewWindow.PreviewMode = !this.previewWindow.PreviewMode;
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (navdir == AccessibleNavigation.Previous)
                {
                    int childCount = this.previewWindow.parentDesigner.AccessibilityObject.GetChildCount();
                    if ((childCount - 3) >= 0)
                    {
                        return this.previewWindow.parentDesigner.AccessibilityObject.GetChild(childCount - 3);
                    }
                }
                else if (navdir == AccessibleNavigation.Next)
                {
                    if (this.previewWindow.PreviewMode)
                    {
                        return this.previewWindow.parentDesigner.AccessibilityObject.Navigate(navdir);
                    }
                    int num2 = this.previewWindow.parentDesigner.AccessibilityObject.GetChildCount();
                    if ((num2 - 1) >= 0)
                    {
                        return this.previewWindow.parentDesigner.AccessibilityObject.GetChild(num2 - 1);
                    }
                }
                return base.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                base.Select(flags);
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle previewModeButtonRectangle = this.previewWindow.PreviewModeButtonRectangle;
                    WorkflowView service = this.previewWindow.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                    if (service != null)
                    {
                        previewModeButtonRectangle = new Rectangle(service.LogicalPointToScreen(previewModeButtonRectangle.Location), service.LogicalSizeToClient(previewModeButtonRectangle.Size));
                    }
                    return previewModeButtonRectangle;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString("AccessibleAction", new object[0]);
                }
            }

            public override string Description
            {
                get
                {
                    return DR.GetString("PreviewButtonAccessibleDescription", new object[0]);
                }
            }

            public override string Help
            {
                get
                {
                    return DR.GetString("PreviewButtonAccessibleHelp", new object[0]);
                }
            }

            public override string Name
            {
                get
                {
                    return DR.GetString("PreviewButtonName", new object[0]);
                }
                set
                {
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.previewWindow.parentDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return base.State;
                }
            }
        }
    }
}

