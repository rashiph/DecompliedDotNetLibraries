namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Drawing.Text;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.Interop;

    public static class ActivityDesignerPaint
    {
        private static Color[,] XPColorSchemes;

        static ActivityDesignerPaint()
        {
            Color[,] colorArray = new Color[3, 5];
            *(colorArray[0, 0]) = Color.FromArgb(0, 60, 0xa5);
            *(colorArray[0, 1]) = Color.FromArgb(0xff, 0xff, 0xff);
            *(colorArray[0, 2]) = Color.FromArgb(0xb5, 0xba, 0xd6);
            *(colorArray[0, 3]) = Color.FromArgb(0x42, 0x8e, 0xff);
            *(colorArray[0, 4]) = Color.FromArgb(0xb5, 0xc3, 0xe7);
            *(colorArray[1, 0]) = Color.FromArgb(0x31, 0x44, 0x73);
            *(colorArray[1, 1]) = Color.FromArgb(0xff, 0xff, 0xff);
            *(colorArray[1, 2]) = Color.FromArgb(0xba, 0xbb, 0xc9);
            *(colorArray[1, 3]) = Color.FromArgb(0x7e, 0x7c, 0x7c);
            *(colorArray[1, 4]) = Color.FromArgb(0xce, 0xcf, 0xd8);
            *(colorArray[2, 0]) = Color.FromArgb(0x56, 0x66, 0x2d);
            *(colorArray[2, 1]) = Color.FromArgb(0xff, 0xff, 0xff);
            *(colorArray[2, 2]) = Color.FromArgb(210, 0xdb, 0xc5);
            *(colorArray[2, 3]) = Color.FromArgb(0x72, 0x92, 0x1d);
            *(colorArray[2, 4]) = Color.FromArgb(0xd4, 220, 190);
            XPColorSchemes = colorArray;
        }

        public static void Draw3DButton(Graphics graphics, Image image, Rectangle bounds, float transparency, ButtonState buttonState)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            int alpha = Math.Max(0, Convert.ToInt32((float) (transparency * 255f)));
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, SystemColors.Control)))
            {
                using (Pen pen = new Pen(Color.FromArgb(alpha, SystemColors.ControlLightLight)))
                {
                    using (Pen pen2 = new Pen(Color.FromArgb(alpha, SystemColors.ControlDark)))
                    {
                        using (Pen pen3 = new Pen(Color.FromArgb(alpha, SystemColors.ControlDarkDark)))
                        {
                            graphics.FillRectangle(brush, bounds);
                            if ((buttonState == ButtonState.Normal) || (buttonState == ButtonState.Inactive))
                            {
                                graphics.DrawLine(pen, (int) (bounds.Left + 1), (int) (bounds.Bottom - 1), (int) (bounds.Left + 1), (int) (bounds.Top + 1));
                                graphics.DrawLine(pen, (int) (bounds.Left + 1), (int) (bounds.Top + 1), (int) (bounds.Right - 1), (int) (bounds.Top + 1));
                                graphics.DrawLine(pen2, (int) (bounds.Left + 1), (int) (bounds.Bottom - 1), (int) (bounds.Right - 1), (int) (bounds.Bottom - 1));
                                graphics.DrawLine(pen2, (int) (bounds.Right - 1), (int) (bounds.Bottom - 1), (int) (bounds.Right - 1), (int) (bounds.Top + 1));
                                graphics.DrawLine(pen3, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
                                graphics.DrawLine(pen3, bounds.Right, bounds.Bottom, bounds.Right, bounds.Top);
                            }
                            else if (buttonState == ButtonState.Pushed)
                            {
                                graphics.DrawRectangle(pen2, bounds);
                                bounds.Offset(1, 1);
                            }
                            if (image != null)
                            {
                                bounds.Inflate(-2, -2);
                                DrawImage(graphics, image, bounds, new Rectangle(Point.Empty, image.Size), DesignerContentAlignment.Fill, transparency, buttonState == ButtonState.Inactive);
                            }
                        }
                    }
                }
            }
        }

        internal static void DrawConnectors(Graphics graphics, Pen pen, Point[] points, Size connectorCapSize, Size maxCapSize, LineAnchor startConnectorCap, LineAnchor endConnectorCap)
        {
            if (points.GetLength(0) >= 2)
            {
                GraphicsPath path = null;
                float capinset = 0f;
                if (startConnectorCap != LineAnchor.None)
                {
                    Point[] pointArray = new Point[] { points[0], points[1] };
                    int num2 = (pointArray[0].Y == pointArray[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                    num2 += num2 % 2;
                    num2 = Math.Min(Math.Min(num2, maxCapSize.Width), maxCapSize.Height);
                    path = GetLineCap(startConnectorCap, num2, out capinset);
                    if (((path != null) && ((startConnectorCap % LineAnchor.ArrowAnchor) == LineAnchor.None)) && ((pointArray[0].X == pointArray[1].X) || (pointArray[0].Y == pointArray[1].Y)))
                    {
                        Matrix transform = graphics.Transform;
                        graphics.TranslateTransform((float) pointArray[0].X, (float) pointArray[0].Y);
                        if (pointArray[0].Y == pointArray[1].Y)
                        {
                            graphics.RotateTransform((pointArray[0].X < pointArray[1].X) ? 90f : 270f);
                        }
                        else
                        {
                            graphics.RotateTransform((pointArray[0].Y < pointArray[1].Y) ? 180f : 0f);
                        }
                        using (Brush brush = new SolidBrush(pen.Color))
                        {
                            graphics.FillPath(brush, path);
                            graphics.DrawPath(pen, path);
                        }
                        graphics.Transform = (transform != null) ? transform : new Matrix();
                    }
                }
                GraphicsPath path2 = null;
                float num3 = 0f;
                if (endConnectorCap != LineAnchor.None)
                {
                    Point[] pointArray2 = new Point[] { points[points.GetLength(0) - 2], points[points.GetLength(0) - 1] };
                    int num4 = (pointArray2[0].Y == pointArray2[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                    num4 += num4 % 2;
                    num4 = Math.Min(Math.Min(num4, maxCapSize.Width), maxCapSize.Height);
                    path2 = GetLineCap(endConnectorCap, num4, out num3);
                    if (((path2 != null) && ((endConnectorCap % LineAnchor.ArrowAnchor) == LineAnchor.None)) && ((pointArray2[0].X == pointArray2[1].X) || (pointArray2[0].Y == pointArray2[1].Y)))
                    {
                        Matrix matrix2 = graphics.Transform;
                        graphics.TranslateTransform((float) pointArray2[1].X, (float) pointArray2[1].Y);
                        if (pointArray2[0].Y == pointArray2[1].Y)
                        {
                            graphics.RotateTransform((pointArray2[0].X < pointArray2[1].X) ? 270f : 90f);
                        }
                        else
                        {
                            graphics.RotateTransform((pointArray2[0].Y < pointArray2[1].Y) ? 0f : 180f);
                        }
                        using (Brush brush2 = new SolidBrush(pen.Color))
                        {
                            graphics.FillPath(brush2, path2);
                            graphics.DrawPath(pen, path2);
                        }
                        graphics.Transform = (matrix2 != null) ? matrix2 : new Matrix();
                    }
                }
                if (path != null)
                {
                    CustomLineCap cap = new CustomLineCap(null, path) {
                        WidthScale = 1f / pen.Width,
                        BaseInset = capinset
                    };
                    pen.CustomStartCap = cap;
                }
                if (path2 != null)
                {
                    CustomLineCap cap2 = new CustomLineCap(null, path2) {
                        WidthScale = 1f / pen.Width,
                        BaseInset = num3
                    };
                    pen.CustomEndCap = cap2;
                }
                graphics.DrawLines(pen, points);
                if (path != null)
                {
                    CustomLineCap customStartCap = pen.CustomStartCap;
                    pen.StartCap = LineCap.Flat;
                    customStartCap.Dispose();
                }
                if (path2 != null)
                {
                    CustomLineCap customEndCap = pen.CustomEndCap;
                    pen.EndCap = LineCap.Flat;
                    customEndCap.Dispose();
                }
            }
        }

        internal static void DrawDesignerBackground(Graphics graphics, ActivityDesigner designer)
        {
            ActivityDesignerTheme designerTheme = designer.DesignerTheme;
            CompositeDesignerTheme theme2 = designerTheme as CompositeDesignerTheme;
            Rectangle bounds = designer.Bounds;
            Point location = bounds.Location;
            bounds.Location = Point.Empty;
            Matrix transform = graphics.Transform;
            graphics.TranslateTransform((float) location.X, (float) location.Y);
            GraphicsPath path = GetDesignerPath(designer, new Point(-location.X, -location.Y), Size.Empty, DesignerEdges.None);
            RectangleF ef = path.GetBounds();
            int width = Convert.ToInt32(Math.Ceiling((double) ef.Width));
            Rectangle rectangle = new Rectangle(0, 0, width, Convert.ToInt32(Math.Ceiling((double) ef.Height)));
            graphics.FillPath(designerTheme.GetBackgroundBrush(rectangle), path);
            bool flag = (designer is CompositeActivityDesigner) ? ((CompositeActivityDesigner) designer).Expanded : false;
            if (((theme2 != null) && flag) && (theme2.WatermarkImage != null))
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                rectangle.Inflate(-margin.Width, -margin.Height);
                DrawImage(graphics, theme2.WatermarkImage, rectangle, new Rectangle(Point.Empty, theme2.WatermarkImage.Size), theme2.WatermarkAlignment, 0.25f, false);
            }
            if (WorkflowTheme.CurrentTheme.AmbientTheme.ShowDesignerBorder)
            {
                graphics.DrawPath(designerTheme.BorderPen, path);
            }
            path.Dispose();
            graphics.Transform = transform;
        }

        internal static void DrawDropShadow(Graphics graphics, Rectangle shadowSourceRectangle, Color baseColor, int shadowDepth, LightSourcePosition lightSourcePosition, float lightSourceIntensity, bool roundEdges)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if ((shadowSourceRectangle.IsEmpty || (shadowSourceRectangle.Width < 0)) || (shadowSourceRectangle.Height < 0))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidShadowRectangle"), "shadowRectangle");
            }
            if ((shadowDepth < 1) || (shadowDepth > 12))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidShadowDepth"), "shadowDepth");
            }
            if ((lightSourceIntensity <= 0f) || (lightSourceIntensity > 1f))
            {
                throw new ArgumentException(SR.GetString("Error_InvalidLightSource"), "lightSourceIntensity");
            }
            Rectangle rectangle = shadowSourceRectangle;
            Size empty = Size.Empty;
            if ((lightSourcePosition & LightSourcePosition.Center) > 0)
            {
                rectangle.Inflate(shadowDepth, shadowDepth);
            }
            if ((lightSourcePosition & LightSourcePosition.Left) > 0)
            {
                empty.Width += shadowDepth + 1;
            }
            else if ((lightSourcePosition & LightSourcePosition.Right) > 0)
            {
                empty.Width -= shadowDepth + 1;
            }
            if ((lightSourcePosition & LightSourcePosition.Top) > 0)
            {
                empty.Height += shadowDepth + 1;
            }
            else if ((lightSourcePosition & LightSourcePosition.Bottom) > 0)
            {
                empty.Height -= shadowDepth + 1;
            }
            rectangle.Offset(empty.Width, empty.Height);
            GraphicsContainer container = graphics.BeginContainer();
            GraphicsPath path = new GraphicsPath();
            if (roundEdges)
            {
                path.AddPath(GetRoundedRectanglePath(shadowSourceRectangle, 8), true);
            }
            else
            {
                path.AddRectangle(shadowSourceRectangle);
            }
            try
            {
                using (Region region = new Region(path))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.ExcludeClip(region);
                    Color color = Color.FromArgb(Convert.ToInt32((float) (40f * lightSourceIntensity)), baseColor);
                    int num = Math.Max(40 / shadowDepth, 2);
                    for (int i = 0; i < shadowDepth; i++)
                    {
                        rectangle.Inflate(-1, -1);
                        using (Brush brush = new SolidBrush(color))
                        {
                            using (GraphicsPath path2 = new GraphicsPath())
                            {
                                if (roundEdges)
                                {
                                    path2.AddPath(GetRoundedRectanglePath(rectangle, 8), true);
                                }
                                else
                                {
                                    path2.AddRectangle(rectangle);
                                }
                                graphics.FillPath(brush, path2);
                            }
                        }
                        color = Color.FromArgb(color.A + num, color.R, color.G, color.B);
                    }
                }
            }
            finally
            {
                graphics.EndContainer(container);
            }
        }

        public static void DrawExpandButton(Graphics graphics, Rectangle boundingRect, bool drawExpanded, CompositeDesignerTheme compositeDesignerTheme)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (compositeDesignerTheme == null)
            {
                throw new ArgumentNullException("compositeDesignerTheme");
            }
            if (!boundingRect.IsEmpty)
            {
                graphics.FillRectangle(compositeDesignerTheme.GetExpandButtonBackgroundBrush(boundingRect), boundingRect);
                graphics.DrawRectangle(CompositeDesignerTheme.ExpandButtonBorderPen, boundingRect);
                graphics.DrawLine(CompositeDesignerTheme.ExpandButtonForegoundPen, (int) (boundingRect.Left + 2), (int) (boundingRect.Top + (boundingRect.Height / 2)), (int) (boundingRect.Right - 2), (int) (boundingRect.Top + (boundingRect.Height / 2)));
                if (drawExpanded)
                {
                    graphics.DrawLine(CompositeDesignerTheme.ExpandButtonForegoundPen, (int) (boundingRect.Left + (boundingRect.Width / 2)), (int) (boundingRect.Top + 2), (int) (boundingRect.Left + (boundingRect.Width / 2)), (int) (boundingRect.Bottom - 2));
                }
            }
        }

        internal static void DrawGrabHandles(Graphics graphics, Rectangle[] grabHandles, bool isPrimary)
        {
            foreach (Rectangle rectangle in grabHandles)
            {
                if (isPrimary)
                {
                    graphics.FillRectangle(Brushes.White, rectangle);
                    graphics.DrawRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionForegroundPen, rectangle);
                }
                else
                {
                    Pen selectionPatternPen = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionPatternPen;
                    DashStyle dashStyle = selectionPatternPen.DashStyle;
                    selectionPatternPen.DashStyle = DashStyle.Solid;
                    graphics.FillRectangle(Brushes.White, rectangle);
                    graphics.DrawRectangle(selectionPatternPen, rectangle);
                    selectionPatternPen.DashStyle = dashStyle;
                }
            }
        }

        internal static void DrawGrid(Graphics graphics, Rectangle viewableRectangle)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            if (ambientTheme.GridStyle == DashStyle.Dot)
            {
                Point empty = Point.Empty;
                empty.X = viewableRectangle.X - (viewableRectangle.X % ambientTheme.GridSize.Width);
                empty.Y = viewableRectangle.Y - (viewableRectangle.Y % ambientTheme.GridSize.Height);
                for (int i = empty.X; i <= viewableRectangle.Right; i += Math.Max(ambientTheme.GridSize.Width, 1))
                {
                    for (int j = empty.Y; j <= viewableRectangle.Bottom; j += Math.Max(ambientTheme.GridSize.Height, 1))
                    {
                        graphics.FillRectangle(ambientTheme.MajorGridBrush, new Rectangle(new Point(i, j), new Size(1, 1)));
                        if ((((i + (ambientTheme.GridSize.Width / 2)) >= viewableRectangle.Left) && ((i + (ambientTheme.GridSize.Width / 2)) <= viewableRectangle.Right)) && (((j + (ambientTheme.GridSize.Height / 2)) >= viewableRectangle.Top) && ((j + (ambientTheme.GridSize.Height / 2)) <= viewableRectangle.Bottom)))
                        {
                            graphics.FillRectangle(ambientTheme.MinorGridBrush, new Rectangle(new Point(i + (ambientTheme.GridSize.Width / 2), j + (ambientTheme.GridSize.Height / 2)), new Size(1, 1)));
                        }
                    }
                }
            }
            else
            {
                using (Hdc hdc = new Hdc(graphics))
                {
                    using (HPen pen = new HPen(ambientTheme.MajorGridPen))
                    {
                        using (HPen pen2 = new HPen(ambientTheme.MinorGridPen))
                        {
                            hdc.DrawGrid(pen, pen2, viewableRectangle, ambientTheme.GridSize, true);
                        }
                    }
                }
            }
        }

        internal static void DrawImage(Graphics graphics, Image image, Rectangle destination, float transparency)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            DrawImage(graphics, image, destination, new Rectangle(Point.Empty, image.Size), DesignerContentAlignment.Center, transparency, false);
        }

        public static void DrawImage(Graphics graphics, Image image, Rectangle destination, DesignerContentAlignment alignment)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            DrawImage(graphics, image, destination, new Rectangle(Point.Empty, image.Size), alignment, 1f, false);
        }

        public static void DrawImage(Graphics graphics, Image image, Rectangle destination, Rectangle source, DesignerContentAlignment alignment, float transparency, bool grayscale)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (image == null)
            {
                throw new ArgumentNullException("image");
            }
            if (destination.IsEmpty)
            {
                throw new ArgumentNullException("destination");
            }
            if (source.IsEmpty)
            {
                throw new ArgumentNullException("source");
            }
            if ((transparency < 0f) || (transparency > 1f))
            {
                throw new ArgumentNullException("transparency");
            }
            Rectangle destRect = GetRectangleFromAlignment(alignment, destination, source.Size);
            if ((image != null) && !destRect.IsEmpty)
            {
                ColorMatrix newColorMatrix = new ColorMatrix();
                if (grayscale)
                {
                    newColorMatrix.Matrix00 = 0.3333333f;
                    newColorMatrix.Matrix01 = 0.3333333f;
                    newColorMatrix.Matrix02 = 0.3333333f;
                    newColorMatrix.Matrix10 = 0.3333333f;
                    newColorMatrix.Matrix11 = 0.3333333f;
                    newColorMatrix.Matrix12 = 0.3333333f;
                    newColorMatrix.Matrix20 = 0.3333333f;
                    newColorMatrix.Matrix21 = 0.3333333f;
                    newColorMatrix.Matrix22 = 0.3333333f;
                }
                newColorMatrix.Matrix33 = transparency;
                ImageAttributes imageAttr = new ImageAttributes();
                imageAttr.SetColorMatrix(newColorMatrix);
                graphics.DrawImage(image, destRect, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, imageAttr);
            }
        }

        internal static void DrawInvalidDesignerIndicator(Graphics graphics, ActivityDesigner activityDesigner)
        {
            Rectangle bounds = activityDesigner.Bounds;
            graphics.DrawRectangle(Pens.Red, bounds);
            graphics.DrawLine(Pens.Red, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            graphics.DrawLine(Pens.Red, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
        }

        public static void DrawRoundedRectangle(Graphics graphics, Pen drawingPen, Rectangle rectangle, int radius)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (drawingPen == null)
            {
                throw new ArgumentNullException("drawingPen");
            }
            GraphicsPath roundedRectanglePath = null;
            roundedRectanglePath = GetRoundedRectanglePath(rectangle, radius * 2);
            graphics.DrawPath(drawingPen, roundedRectanglePath);
            roundedRectanglePath.Dispose();
        }

        internal static void DrawSelection(Graphics graphics, Rectangle boundingRect, bool isPrimary, Size selectionSize, Rectangle[] grabHandles)
        {
            InterpolationMode interpolationMode = graphics.InterpolationMode;
            SmoothingMode smoothingMode = graphics.SmoothingMode;
            graphics.InterpolationMode = InterpolationMode.High;
            graphics.SmoothingMode = SmoothingMode.None;
            Rectangle rect = boundingRect;
            rect.Inflate(selectionSize.Width, selectionSize.Height);
            rect.Inflate(-selectionSize.Width / 2, -selectionSize.Height / 2);
            graphics.DrawRectangle(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionPatternPen, rect);
            rect.Inflate(selectionSize.Width / 2, selectionSize.Height / 2);
            DrawGrabHandles(graphics, grabHandles, isPrimary);
            graphics.InterpolationMode = interpolationMode;
            graphics.SmoothingMode = smoothingMode;
        }

        public static void DrawText(Graphics graphics, Font font, string text, Rectangle boundingRect, StringAlignment alignment, TextQuality textQuality, Brush textBrush)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (textBrush == null)
            {
                throw new ArgumentNullException("textBrush");
            }
            if (!boundingRect.IsEmpty)
            {
                StringFormat format = new StringFormat {
                    Alignment = alignment,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.FitBlackBox
                };
                TextRenderingHint textRenderingHint = graphics.TextRenderingHint;
                graphics.TextRenderingHint = (textQuality == TextQuality.AntiAliased) ? TextRenderingHint.AntiAlias : TextRenderingHint.SystemDefault;
                graphics.DrawString(text, font, textBrush, boundingRect, format);
                graphics.TextRenderingHint = textRenderingHint;
            }
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, bool enableRoundedCorners)
        {
            return GetDesignerPath(designer, Point.Empty, Size.Empty, DesignerEdges.None, enableRoundedCorners);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Point offset, Size inflate, DesignerEdges edgeToInflate)
        {
            return GetDesignerPath(designer, offset, inflate, edgeToInflate, true);
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Point offset, Size inflate, DesignerEdges edgeToInflate, bool enableRoundedCorners)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle bounds = designer.Bounds;
            bounds.Offset(offset);
            if ((edgeToInflate & DesignerEdges.Left) > DesignerEdges.None)
            {
                bounds.X -= inflate.Width;
                bounds.Width += inflate.Width;
            }
            if ((edgeToInflate & DesignerEdges.Right) > DesignerEdges.None)
            {
                bounds.Width += inflate.Width;
            }
            if ((edgeToInflate & DesignerEdges.Top) > DesignerEdges.None)
            {
                bounds.Y -= inflate.Height;
                bounds.Height += inflate.Height;
            }
            if ((edgeToInflate & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                bounds.Height += inflate.Height;
            }
            if ((designer == ActivityDesigner.GetSafeRootDesigner(designer.Activity.Site)) && (((IWorkflowRootDesigner) designer).InvokingDesigner == null))
            {
                path.AddRectangle(bounds);
                return path;
            }
            ActivityDesignerTheme designerTheme = designer.DesignerTheme;
            if ((enableRoundedCorners && (designerTheme != null)) && (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle))
            {
                path.AddPath(GetRoundedRectanglePath(bounds, 8), true);
                return path;
            }
            path.AddRectangle(bounds);
            return path;
        }

        internal static GraphicsPath GetLineCap(LineAnchor lineCap, int capsize, out float capinset)
        {
            int num;
            capinset = 0f;
            capinset = capsize;
            Size size = new Size(capsize, capsize);
            GraphicsPath path = new GraphicsPath();
            switch (lineCap)
            {
                case LineAnchor.Arrow:
                case LineAnchor.ArrowAnchor:
                    num = size.Height / 3;
                    path.AddLine(size.Width / 2, -size.Height, 0, 0);
                    path.AddLine(0, 0, -size.Width / 2, -size.Height);
                    path.AddLine(-size.Width / 2, -size.Height, 0, -size.Height + num);
                    path.AddLine(0, -size.Height + num, size.Width / 2, -size.Height);
                    capinset = size.Height - num;
                    break;

                case LineAnchor.Diamond:
                case LineAnchor.DiamondAnchor:
                    path.AddLine(0, -size.Height, size.Width / 2, -size.Height / 2);
                    path.AddLine(size.Width / 2, -size.Height / 2, 0, 0);
                    path.AddLine(0, 0, -size.Width / 2, -size.Height / 2);
                    path.AddLine(-size.Width / 2, -size.Height / 2, 0, -size.Height);
                    break;

                case LineAnchor.Round:
                case LineAnchor.RoundAnchor:
                    path.AddEllipse(new Rectangle(-size.Width / 2, -size.Height, size.Width, size.Height));
                    break;

                case LineAnchor.Rectangle:
                case LineAnchor.RectangleAnchor:
                    path.AddRectangle(new Rectangle(-size.Width / 2, -size.Height, size.Width, size.Height));
                    break;

                case LineAnchor.RoundedRectangle:
                case LineAnchor.RoundedRectangleAnchor:
                    num = size.Height / 4;
                    path.AddPath(GetRoundedRectanglePath(new Rectangle(-size.Width / 2, -size.Height, size.Width, size.Height), num), true);
                    break;
            }
            path.CloseFigure();
            return path;
        }

        internal static GraphicsPath[] GetPagePaths(Rectangle pageBounds, int pageFoldSize, DesignerContentAlignment foldAlignment)
        {
            GraphicsPath[] pathArray = new GraphicsPath[2];
            if (foldAlignment == DesignerContentAlignment.TopLeft)
            {
                pathArray[0] = new GraphicsPath();
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize);
                pathArray[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top);
                pathArray[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top + pageFoldSize);
                pathArray[1] = new GraphicsPath();
                pathArray[1].AddLine(pageBounds.Left, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize);
                pathArray[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Top);
                pathArray[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Top, pageBounds.Left, pageBounds.Top + pageFoldSize);
                return pathArray;
            }
            if (foldAlignment == DesignerContentAlignment.BottomLeft)
            {
                pathArray[0] = new GraphicsPath();
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pathArray[0].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Bottom - pageFoldSize);
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Top);
                pathArray[1] = new GraphicsPath();
                pathArray[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom, pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pathArray[1].AddLine(pageBounds.Left + pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Left, pageBounds.Bottom - pageFoldSize);
                pathArray[1].AddLine(pageBounds.Left, pageBounds.Bottom - pageFoldSize, pageBounds.Left + pageFoldSize, pageBounds.Bottom);
                return pathArray;
            }
            if (foldAlignment == DesignerContentAlignment.TopRight)
            {
                pathArray[0] = new GraphicsPath();
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top);
                pathArray[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize);
                pathArray[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Top + pageFoldSize);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top);
                pathArray[1] = new GraphicsPath();
                pathArray[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top, pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize);
                pathArray[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Top + pageFoldSize, pageBounds.Right, pageBounds.Top + pageFoldSize);
                pathArray[1].AddLine(pageBounds.Right, pageBounds.Top + pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Top);
                return pathArray;
            }
            if (foldAlignment == DesignerContentAlignment.BottomRight)
            {
                pathArray[0] = new GraphicsPath();
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Top, pageBounds.Right, pageBounds.Top);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Top, pageBounds.Right, pageBounds.Bottom - pageFoldSize);
                pathArray[0].AddLine(pageBounds.Right, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pathArray[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom, pageBounds.Left, pageBounds.Bottom);
                pathArray[0].AddLine(pageBounds.Left, pageBounds.Bottom, pageBounds.Left, pageBounds.Top);
                pathArray[1] = new GraphicsPath();
                pathArray[1].AddLine(pageBounds.Right, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize);
                pathArray[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom - pageFoldSize, pageBounds.Right - pageFoldSize, pageBounds.Bottom);
                pathArray[1].AddLine(pageBounds.Right - pageFoldSize, pageBounds.Bottom, pageBounds.Right, pageBounds.Bottom - pageFoldSize);
            }
            return pathArray;
        }

        internal static Rectangle GetRectangleFromAlignment(DesignerContentAlignment alignment, Rectangle destination, Size size)
        {
            if (size.IsEmpty || destination.IsEmpty)
            {
                return Rectangle.Empty;
            }
            Rectangle empty = Rectangle.Empty;
            empty.Width = Math.Min(size.Width, destination.Width);
            empty.Height = Math.Min(size.Height, destination.Height);
            if ((alignment & DesignerContentAlignment.Fill) > ((DesignerContentAlignment) 0))
            {
                return destination;
            }
            if ((alignment & DesignerContentAlignment.Left) > ((DesignerContentAlignment) 0))
            {
                empty.X = destination.Left;
            }
            else if ((alignment & DesignerContentAlignment.Right) > ((DesignerContentAlignment) 0))
            {
                empty.X = destination.Right - empty.Width;
            }
            else
            {
                empty.X = (destination.Left + (destination.Width / 2)) - (empty.Width / 2);
            }
            if ((alignment & DesignerContentAlignment.Top) > ((DesignerContentAlignment) 0))
            {
                empty.Y = destination.Top;
                return empty;
            }
            if ((alignment & DesignerContentAlignment.Bottom) > ((DesignerContentAlignment) 0))
            {
                empty.Y = destination.Bottom - empty.Height;
                return empty;
            }
            empty.Y = (destination.Top + (destination.Height / 2)) - (empty.Height / 2);
            return empty;
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle rectangle, int radius)
        {
            if (rectangle.IsEmpty)
            {
                throw new ArgumentException(SR.GetString("Error_EmptyRectangleValue"), "rectangle");
            }
            if (radius <= 0)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidRadiusValue"), "radius");
            }
            int width = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddLine(rectangle.Left, rectangle.Bottom - radius, rectangle.Left, rectangle.Top + radius);
            path.AddArc(rectangle.Left, rectangle.Top, width, width, 180f, 90f);
            path.AddLine(rectangle.Left + radius, rectangle.Top, rectangle.Right - radius, rectangle.Top);
            path.AddArc(rectangle.Right - width, rectangle.Top, width, width, 270f, 90f);
            path.AddLine(rectangle.Right, rectangle.Top + radius, rectangle.Right, rectangle.Bottom - radius);
            path.AddArc(rectangle.Right - width, rectangle.Bottom - width, width, width, 0f, 90f);
            path.AddLine(rectangle.Right - radius, rectangle.Bottom, rectangle.Left + radius, rectangle.Bottom);
            path.AddArc(rectangle.Left, rectangle.Bottom - width, width, width, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        internal static GraphicsPath GetScrollIndicatorPath(Rectangle bounds, ScrollButton button)
        {
            GraphicsPath path = new GraphicsPath();
            if (!bounds.IsEmpty)
            {
                if ((button == ScrollButton.Left) || (button == ScrollButton.Right))
                {
                    int height = bounds.Height + (bounds.Height % 2);
                    int num2 = height / 2;
                    Size size = new Size(height / 2, height);
                    if (button == ScrollButton.Right)
                    {
                        path.AddLine(bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Top, bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Top + size.Height);
                        path.AddLine((int) (bounds.Left + ((bounds.Width - size.Width) / 2)), (int) (bounds.Top + size.Height), (int) ((bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width), (int) (bounds.Top + num2));
                        path.AddLine((bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width, bounds.Top + num2, bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Top);
                    }
                    else
                    {
                        path.AddLine((int) (bounds.Left + ((bounds.Width - size.Width) / 2)), (int) (bounds.Top + num2), (int) ((bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width), (int) (bounds.Top + size.Height));
                        path.AddLine((bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width, bounds.Top + size.Height, (bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width, bounds.Top);
                        path.AddLine((bounds.Left + ((bounds.Width - size.Width) / 2)) + size.Width, bounds.Top, bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Top + num2);
                    }
                }
                else if ((button == ScrollButton.Up) || (button == ScrollButton.Down))
                {
                    int width = bounds.Width + (bounds.Width % 2);
                    int num4 = width / 2;
                    Size size2 = new Size(width, width / 2);
                    if (button == ScrollButton.Down)
                    {
                        path.AddLine(bounds.Left, bounds.Top + ((bounds.Height - size2.Height) / 2), bounds.Left + size2.Width, bounds.Top + ((bounds.Height - size2.Height) / 2));
                        path.AddLine((int) (bounds.Left + size2.Width), (int) (bounds.Top + ((bounds.Height - size2.Height) / 2)), (int) (bounds.Left + num4), (int) ((bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height));
                        path.AddLine(bounds.Left + num4, (bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height, bounds.Left, bounds.Top + ((bounds.Height - size2.Height) / 2));
                    }
                    else
                    {
                        path.AddLine((int) (bounds.Left + num4), (int) (bounds.Top + ((bounds.Height - size2.Height) / 2)), (int) (bounds.Left + size2.Width), (int) ((bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height));
                        path.AddLine(bounds.Left + size2.Width, (bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height, bounds.Left, (bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height);
                        path.AddLine(bounds.Left, (bounds.Top + ((bounds.Height - size2.Height) / 2)) + size2.Height, bounds.Left + num4, bounds.Top + ((bounds.Height - size2.Height) / 2));
                    }
                }
            }
            path.CloseFigure();
            return path;
        }

        internal static Size MeasureString(Graphics graphics, Font font, string text, StringAlignment alignment, Size maxSize)
        {
            SizeF empty = SizeF.Empty;
            if (maxSize.IsEmpty)
            {
                empty = graphics.MeasureString(text, font);
            }
            else
            {
                StringFormat stringFormat = new StringFormat {
                    Alignment = alignment,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoClip
                };
                empty = graphics.MeasureString(text, font, new SizeF((float) maxSize.Width, (float) maxSize.Height), stringFormat);
            }
            int width = Convert.ToInt32(Math.Ceiling((double) empty.Width));
            return new Size(width, Convert.ToInt32(Math.Ceiling((double) empty.Height)));
        }

        private sealed class Hdc : IDisposable
        {
            private Graphics graphics;
            private HandleRef hdc;
            private HandleRef oldBrush;
            private int oldGraphicsMode;
            private HandleRef oldPen;
            private HandleRef oldPenEx;

            internal Hdc(Graphics graphics)
            {
                this.graphics = graphics;
                System.Workflow.Interop.NativeMethods.XFORM xform = new System.Workflow.Interop.NativeMethods.XFORM(this.graphics.Transform);
                this.hdc = new HandleRef(this, this.graphics.GetHdc());
                this.oldGraphicsMode = System.Workflow.Interop.NativeMethods.SetGraphicsMode(this.hdc, 2);
                if (this.oldGraphicsMode == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (System.Workflow.Interop.NativeMethods.SetWorldTransform(this.hdc, xform) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                IntPtr currentObject = System.Workflow.Interop.NativeMethods.GetCurrentObject(this.hdc, 1);
                if (currentObject == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                this.oldPen = new HandleRef(this, currentObject);
                currentObject = System.Workflow.Interop.NativeMethods.GetCurrentObject(this.hdc, 11);
                if (currentObject == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                this.oldPenEx = new HandleRef(this, currentObject);
                currentObject = System.Workflow.Interop.NativeMethods.GetCurrentObject(this.hdc, 2);
                if (currentObject == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                this.oldBrush = new HandleRef(this, currentObject);
            }

            internal void DrawGrid(ActivityDesignerPaint.HPen majorGridPen, ActivityDesignerPaint.HPen minorGridPen, Rectangle viewableRectangle, Size gridUnit, bool showMinorGrid)
            {
                try
                {
                    Point empty = Point.Empty;
                    empty.X = viewableRectangle.X - (viewableRectangle.X % gridUnit.Width);
                    empty.Y = viewableRectangle.Y - (viewableRectangle.Y % gridUnit.Height);
                    if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, majorGridPen.Handle) == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    for (int i = empty.X; i <= viewableRectangle.Right; i += Math.Max(gridUnit.Width, 1))
                    {
                        if (i >= viewableRectangle.Left)
                        {
                            if (!System.Workflow.Interop.NativeMethods.MoveToEx(this.hdc, i, viewableRectangle.Top + 1, null))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.LineTo(this.hdc, i, viewableRectangle.Bottom - 1))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }
                        if ((showMinorGrid && ((i + (gridUnit.Width / 2)) >= viewableRectangle.Left)) && ((i + (gridUnit.Width / 2)) <= viewableRectangle.Right))
                        {
                            if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, minorGridPen.Handle) == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.MoveToEx(this.hdc, i + (gridUnit.Width / 2), viewableRectangle.Top + 1, null))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.LineTo(this.hdc, i + (gridUnit.Width / 2), viewableRectangle.Bottom - 1))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, majorGridPen.Handle) == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }
                    }
                    for (int j = empty.Y; j <= viewableRectangle.Bottom; j += Math.Max(gridUnit.Height, 1))
                    {
                        if (j >= viewableRectangle.Top)
                        {
                            if (!System.Workflow.Interop.NativeMethods.MoveToEx(this.hdc, viewableRectangle.Left + 1, j, null))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.LineTo(this.hdc, viewableRectangle.Right - 1, j))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }
                        if ((showMinorGrid && ((j + (gridUnit.Height / 2)) >= viewableRectangle.Top)) && ((j + (gridUnit.Height / 2)) <= viewableRectangle.Bottom))
                        {
                            if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, minorGridPen.Handle) == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.MoveToEx(this.hdc, viewableRectangle.Left + 1, j + (gridUnit.Height / 2), null))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (!System.Workflow.Interop.NativeMethods.LineTo(this.hdc, viewableRectangle.Right - 1, j + (gridUnit.Height / 2)))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                            if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, majorGridPen.Handle) == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }
                    }
                }
                finally
                {
                    if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, this.oldPen) == IntPtr.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }

            void IDisposable.Dispose()
            {
                if (this.graphics != null)
                {
                    if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, this.oldPen) == IntPtr.Zero)
                    {
                        Win32Exception exception = new Win32Exception();
                        string message = exception.Message;
                    }
                    if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, this.oldPenEx) == IntPtr.Zero)
                    {
                        Win32Exception exception2 = new Win32Exception();
                        string text2 = exception2.Message;
                    }
                    if (System.Workflow.Interop.NativeMethods.SelectObject(this.hdc, this.oldBrush) == IntPtr.Zero)
                    {
                        Win32Exception exception3 = new Win32Exception();
                        string text3 = exception3.Message;
                    }
                    if (System.Workflow.Interop.NativeMethods.SetWorldTransform(this.hdc, new System.Workflow.Interop.NativeMethods.XFORM()) == 0)
                    {
                        Win32Exception exception4 = new Win32Exception();
                        string text4 = exception4.Message;
                    }
                    if (System.Workflow.Interop.NativeMethods.SetGraphicsMode(this.hdc, this.oldGraphicsMode) == 0)
                    {
                        Win32Exception exception5 = new Win32Exception();
                        string text5 = exception5.Message;
                    }
                    this.graphics.ReleaseHdc();
                    this.graphics = null;
                }
            }
        }

        private sealed class HPen : IDisposable
        {
            private HandleRef hpen;
            private Pen pen;

            internal HPen(Pen pen)
            {
                this.pen = pen;
                int num = (pen.DashStyle < DashStyle.DashDotDot) ? ((int) pen.DashStyle) : 0;
                IntPtr handle = System.Workflow.Interop.NativeMethods.ExtCreatePen(7 | num, 1, new System.Workflow.Interop.NativeMethods.LOGBRUSH(0, ColorTranslator.ToWin32(pen.Color), 0), 2, new int[] { 1, 1 });
                if (handle == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                this.hpen = new HandleRef(this, handle);
            }

            void IDisposable.Dispose()
            {
                if (this.pen != null)
                {
                    if (System.Workflow.Interop.NativeMethods.DeleteObject(this.hpen) == 0)
                    {
                        Win32Exception exception = new Win32Exception();
                        string message = exception.Message;
                    }
                    this.pen = null;
                }
            }

            internal HandleRef Handle
            {
                get
                {
                    return this.hpen;
                }
            }
        }

        private enum XpSchemeColorIndex
        {
            FgGnd,
            BkGnd,
            Border,
            Highlight,
            Shadow
        }

        internal enum XpThemeColorStyles
        {
            Blue,
            Silver,
            Green
        }
    }
}

