namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    internal static class DesignerGeometryHelper
    {
        internal static DesignerEdges ClosestEdgeToPoint(Point point, Rectangle rect, DesignerEdges edgesToConsider)
        {
            List<double> list = new List<double>();
            List<DesignerEdges> list2 = new List<DesignerEdges>();
            if ((edgesToConsider & DesignerEdges.Left) > DesignerEdges.None)
            {
                Point[] line = new Point[] { new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom) };
                list.Add(DistanceFromPointToLineSegment(point, line));
                list2.Add(DesignerEdges.Left);
            }
            if ((edgesToConsider & DesignerEdges.Top) > DesignerEdges.None)
            {
                Point[] pointArray2 = new Point[] { new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top) };
                list.Add(DistanceFromPointToLineSegment(point, pointArray2));
                list2.Add(DesignerEdges.Top);
            }
            if ((edgesToConsider & DesignerEdges.Right) > DesignerEdges.None)
            {
                Point[] pointArray3 = new Point[] { new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom) };
                list.Add(DistanceFromPointToLineSegment(point, pointArray3));
                list2.Add(DesignerEdges.Right);
            }
            if ((edgesToConsider & DesignerEdges.Bottom) > DesignerEdges.None)
            {
                Point[] pointArray4 = new Point[] { new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom) };
                list.Add(DistanceFromPointToLineSegment(point, pointArray4));
                list2.Add(DesignerEdges.Bottom);
            }
            if (list.Count <= 0)
            {
                return DesignerEdges.None;
            }
            double num = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                num = Math.Min(num, list[i]);
            }
            return list2[list.IndexOf(num)];
        }

        internal static double DistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow((double) (point2.X - point1.X), 2.0) + Math.Pow((double) (point2.Y - point1.Y), 2.0));
        }

        internal static double DistanceFromPointToLineSegment(Point point, Point[] line)
        {
            return Math.Sqrt(Math.Pow((double) Math.Abs((int) (((point.Y - line[0].Y) * (line[1].X - line[0].X)) - ((point.X - line[0].X) * (line[1].Y - line[0].Y)))), 2.0) / (Math.Pow((double) (line[1].X - line[0].X), 2.0) + Math.Pow((double) (line[1].Y - line[0].Y), 2.0)));
        }

        internal static double DistanceFromPointToRectangle(Point point, Rectangle rect)
        {
            List<double> list = new List<double> {
                DistanceBetweenPoints(point, new Point(rect.Left, rect.Top)),
                DistanceBetweenPoints(point, new Point(rect.Left + (rect.Width / 2), rect.Top)),
                DistanceBetweenPoints(point, new Point(rect.Right, rect.Top)),
                DistanceBetweenPoints(point, new Point(rect.Right, rect.Top + (rect.Height / 2))),
                DistanceBetweenPoints(point, new Point(rect.Right, rect.Bottom)),
                DistanceBetweenPoints(point, new Point(rect.Right + (rect.Width / 2), rect.Bottom)),
                DistanceBetweenPoints(point, new Point(rect.Left, rect.Bottom)),
                DistanceBetweenPoints(point, new Point(rect.Left, rect.Bottom - (rect.Height / 2)))
            };
            double num = list[0];
            for (int i = 1; i < list.Count; i++)
            {
                num = Math.Min(num, list[i]);
            }
            return num;
        }

        internal static double DistanceOfLineSegments(Point[] segments)
        {
            double num = 0.0;
            for (int i = 1; i < segments.Length; i++)
            {
                num += DistanceBetweenPoints(segments[i - 1], segments[i]);
            }
            return num;
        }

        internal static Point MidPointOfLineSegment(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2);
        }

        internal static bool PointOnLineSegment(Point point, Point[] line, Size hitAreaSize)
        {
            Rectangle rectangle = RectangleFromLineSegments(line);
            rectangle.Inflate(hitAreaSize);
            if (rectangle.Contains(point))
            {
                double num = DistanceFromPointToLineSegment(point, line);
                if ((num < hitAreaSize.Width) && (num < hitAreaSize.Height))
                {
                    return true;
                }
            }
            return false;
        }

        internal static Rectangle RectangleFromLineSegments(Point[] segments)
        {
            if (segments.Length == 0)
            {
                return Rectangle.Empty;
            }
            Point location = segments[0];
            Point point2 = segments[0];
            foreach (Point point3 in segments)
            {
                location.X = Math.Min(location.X, point3.X);
                location.Y = Math.Min(location.Y, point3.Y);
                point2.X = Math.Max(point2.X, point3.X);
                point2.Y = Math.Max(point2.Y, point3.Y);
            }
            Rectangle rectangle = new Rectangle(location, new Size(point2.X - location.X, point2.Y - location.Y));
            rectangle.Inflate(4, 4);
            return rectangle;
        }

        internal static float SlopeOfLineSegment(Point start, Point end)
        {
            if (start.X == end.X)
            {
                return float.MaxValue;
            }
            if (start.Y == end.Y)
            {
                return 0f;
            }
            return (((float) (end.Y - start.Y)) / ((float) (end.X - start.X)));
        }
    }
}

