namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel.Design;

    internal static class StateMachineDesignerPaint
    {
        internal static readonly Brush FadeBrush = new SolidBrush(Color.FromArgb(120, 0xff, 0xff, 0xff));

        private static void AddRoundedCorner(GraphicsPath path, int diameter, Point midPoint, ArrowDirection direction1, ArrowDirection direction2)
        {
            switch (direction1)
            {
                case ArrowDirection.Left:
                    if (direction2 != ArrowDirection.Down)
                    {
                        path.AddArc(midPoint.X, midPoint.Y - diameter, diameter, diameter, 90f, 90f);
                        return;
                    }
                    path.AddArc(midPoint.X, midPoint.Y, diameter, diameter, 270f, -90f);
                    return;

                case ArrowDirection.Up:
                    if (direction2 != ArrowDirection.Left)
                    {
                        path.AddArc(midPoint.X, midPoint.Y, diameter, diameter, 180f, 90f);
                        return;
                    }
                    path.AddArc(midPoint.X - diameter, midPoint.Y, diameter, diameter, 0f, -90f);
                    return;

                case ArrowDirection.Right:
                    if (direction2 == ArrowDirection.Down)
                    {
                        path.AddArc(midPoint.X - diameter, midPoint.Y, diameter, diameter, 270f, 90f);
                        return;
                    }
                    path.AddArc(midPoint.X - diameter, midPoint.Y - diameter, diameter, diameter, 90f, -90f);
                    return;
            }
            if (direction2 == ArrowDirection.Left)
            {
                path.AddArc(midPoint.X - diameter, midPoint.Y - diameter, diameter, diameter, 0f, 90f);
            }
            else
            {
                path.AddArc(midPoint.X, midPoint.Y - diameter, diameter, diameter, 180f, -90f);
            }
        }

        private static void AddSegment(GraphicsPath path, int radius, Point p1, Point p2, bool roundP1, bool roundP2, ArrowDirection direction)
        {
            if (roundP1)
            {
                switch (direction)
                {
                    case ArrowDirection.Left:
                        p1.X -= radius;
                        goto Label_005C;

                    case ArrowDirection.Up:
                        p1.Y -= radius;
                        goto Label_005C;

                    case ArrowDirection.Down:
                        p1.Y += radius;
                        goto Label_005C;
                }
                p1.X += radius;
            }
        Label_005C:
            if (roundP2)
            {
                switch (direction)
                {
                    case ArrowDirection.Left:
                        p2.X += radius;
                        goto Label_00B8;

                    case ArrowDirection.Up:
                        p2.Y += radius;
                        goto Label_00B8;

                    case ArrowDirection.Down:
                        p2.Y -= radius;
                        goto Label_00B8;
                }
                p2.X -= radius;
            }
        Label_00B8:
            path.AddLine(p1, p2);
        }

        internal static void DrawConnector(Graphics graphics, Pen pen, Point[] points, Size connectorCapSize, Size maxCapSize, LineAnchor startConnectorCap, LineAnchor endConnectorCap)
        {
            if (points.GetLength(0) >= 2)
            {
                points = OptimizeConnectorPoints(points);
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
                using (GraphicsPath path3 = GetRoundedPath(points, 5))
                {
                    graphics.DrawPath(pen, path3);
                }
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

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Rectangle bounds, ActivityDesignerTheme designerTheme)
        {
            GraphicsPath path = new GraphicsPath();
            if ((designer == GetSafeRootDesigner(designer.Activity.Site)) && (((IWorkflowRootDesigner) designer).InvokingDesigner == null))
            {
                path.AddRectangle(bounds);
                return path;
            }
            int radius = 8;
            if ((designerTheme != null) && (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle))
            {
                path.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(bounds, radius), true);
                return path;
            }
            path.AddRectangle(bounds);
            return path;
        }

        private static ArrowDirection GetDirection(Point start, Point end)
        {
            if (start.X == end.X)
            {
                if (start.Y < end.Y)
                {
                    return ArrowDirection.Down;
                }
                return ArrowDirection.Up;
            }
            if (start.X < end.X)
            {
                return ArrowDirection.Right;
            }
            return ArrowDirection.Left;
        }

        private static int GetDistance(Point p1, Point p2)
        {
            if (p1.X == p2.X)
            {
                return Math.Abs((int) (p1.Y - p2.Y));
            }
            return Math.Abs((int) (p1.X - p2.X));
        }

        internal static GraphicsPath GetLineCap(LineAnchor lineCap, int capsize, out float capinset)
        {
            int num;
            capinset = 0f;
            capinset = ((float) capsize) / 2f;
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
                    path.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(new Rectangle(-size.Width / 2, -size.Height, size.Width, size.Height), num), true);
                    break;
            }
            path.CloseFigure();
            return path;
        }

        private static GraphicsPath GetRoundedPath(Point[] points, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (points.Length == 2)
            {
                path.AddLine(points[0], points[1]);
                return path;
            }
            int diameter = radius * 2;
            Point point = points[0];
            Point point2 = points[1];
            Point point3 = points[2];
            int distance = GetDistance(point, point2);
            int num4 = GetDistance(point2, point3);
            ArrowDirection direction = GetDirection(point, point2);
            ArrowDirection direction2 = GetDirection(point2, point3);
            if ((distance < diameter) || (num4 < diameter))
            {
                AddSegment(path, radius, point, point2, false, false, direction);
            }
            else
            {
                AddSegment(path, radius, point, point2, false, true, direction);
                AddRoundedCorner(path, diameter, point2, direction, direction2);
            }
            for (int i = 2; i < (points.Length - 1); i++)
            {
                int num2 = distance;
                distance = num4;
                direction = direction2;
                point = point2;
                point2 = point3;
                point3 = points[i + 1];
                direction2 = GetDirection(point2, point3);
                num4 = GetDistance(point2, point3);
                if ((distance >= diameter) && (num4 >= diameter))
                {
                    AddSegment(path, radius, point, point2, num2 >= diameter, true, direction);
                    AddRoundedCorner(path, diameter, point2, direction, direction2);
                }
                else
                {
                    AddSegment(path, radius, point, point2, num2 >= diameter, false, direction);
                }
            }
            AddSegment(path, radius, point2, point3, (distance >= diameter) && (num4 >= diameter), false, direction2);
            return path;
        }

        internal static ActivityDesigner GetSafeRootDesigner(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                return null;
            }
            return ActivityDesigner.GetRootDesigner(serviceProvider);
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
                    FormatFlags = StringFormatFlags.LineLimit
                };
                empty = graphics.MeasureString(text, font, new SizeF((float) maxSize.Width, (float) maxSize.Height), stringFormat);
            }
            int width = Convert.ToInt32(Math.Ceiling((double) empty.Width));
            return new Size(width, Convert.ToInt32(Math.Ceiling((double) empty.Height)));
        }

        private static Point[] OptimizeConnectorPoints(Point[] points)
        {
            List<Point> list = new List<Point> {
                points[0]
            };
            Point item = points[0];
            Point point3 = points[1];
            if ((item.X != point3.X) && (item.Y != point3.Y))
            {
                list.Add(new Point(point3.X, item.Y));
            }
            for (int i = 2; i < points.Length; i++)
            {
                Point point = item;
                item = point3;
                point3 = points[i];
                if (((point.X != item.X) || (item.X != point3.X)) && ((point.Y != item.Y) || (item.Y != point3.Y)))
                {
                    list.Add(item);
                    if ((item.X != point3.X) && (item.Y != point3.Y))
                    {
                        list.Add(new Point(point3.X, item.Y));
                    }
                }
            }
            list.Add(points[points.Length - 1]);
            return list.ToArray();
        }

        internal static Rectangle TrimRectangle(Rectangle rectangle, Rectangle bounds)
        {
            int left = rectangle.Left;
            int top = rectangle.Top;
            int width = rectangle.Width;
            int height = rectangle.Height;
            if (left < bounds.Left)
            {
                left = bounds.Left;
            }
            if (top < bounds.Top)
            {
                top = bounds.Top;
            }
            if ((left + width) > bounds.Right)
            {
                width -= rectangle.Right - bounds.Right;
            }
            if ((top + height) > bounds.Bottom)
            {
                height -= rectangle.Bottom - bounds.Bottom;
            }
            return new Rectangle(left, top, width, height);
        }
    }
}

