namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal static class ActivityDesignerConnectorRouter
    {
        private static bool AreAllSegmentsVerticalOrHorizontal(Point[] segments)
        {
            if ((segments == null) || (segments.Length == 0))
            {
                return false;
            }
            for (int i = 1; i < segments.Length; i++)
            {
                if ((segments[i - 1].X != segments[i].X) && (segments[i - 1].Y != segments[i].Y))
                {
                    return false;
                }
            }
            return true;
        }

        private static IList<Point> GetDesignerEscapeCover(ActivityDesigner designer, ICollection<object> escapeLocations)
        {
            Rectangle bounds = designer.Bounds;
            Dictionary<DesignerEdges, List<Point>> dictionary = new Dictionary<DesignerEdges, List<Point>>();
            foreach (object obj2 in escapeLocations)
            {
                DesignerEdges none = DesignerEdges.None;
                Point empty = Point.Empty;
                if (obj2 is ConnectionPoint)
                {
                    none = ((ConnectionPoint) obj2).ConnectionEdge;
                    empty = ((ConnectionPoint) obj2).Location;
                }
                else if (obj2 is Point)
                {
                    empty = (Point) obj2;
                    none = DesignerGeometryHelper.ClosestEdgeToPoint((Point) obj2, bounds, DesignerEdges.All);
                }
                if (none != DesignerEdges.None)
                {
                    List<Point> list = null;
                    if (!dictionary.ContainsKey(none))
                    {
                        list = new List<Point>();
                        dictionary.Add(none, list);
                    }
                    else
                    {
                        list = dictionary[none];
                    }
                    list.Add(empty);
                }
            }
            Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
            bounds.Inflate(margin);
            Dictionary<DesignerEdges, Point[]> dictionary2 = new Dictionary<DesignerEdges, Point[]>();
            Point[] pointArray2 = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Left, bounds.Bottom) };
            dictionary2.Add(DesignerEdges.Left, pointArray2);
            Point[] pointArray3 = new Point[] { new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Top) };
            dictionary2.Add(DesignerEdges.Top, pointArray3);
            Point[] pointArray4 = new Point[] { new Point(bounds.Right, bounds.Top), new Point(bounds.Right, bounds.Bottom) };
            dictionary2.Add(DesignerEdges.Right, pointArray4);
            Point[] pointArray5 = new Point[] { new Point(bounds.Left, bounds.Bottom), new Point(bounds.Right, bounds.Bottom) };
            dictionary2.Add(DesignerEdges.Bottom, pointArray5);
            List<Point> list2 = new List<Point>();
            foreach (DesignerEdges edges2 in dictionary2.Keys)
            {
                if (dictionary.ContainsKey(edges2))
                {
                    Point[] pointArray = dictionary2[edges2];
                    List<Point> list3 = dictionary[edges2];
                    List<Point> list4 = new List<Point>();
                    switch (edges2)
                    {
                        case DesignerEdges.Left:
                            list4.Add(new Point(pointArray[0].X, pointArray[0].Y));
                            for (int j = 0; j < list3.Count; j++)
                            {
                                Point point2 = list3[j];
                                if (((point2.X > pointArray[0].X) && (point2.Y > pointArray[0].Y)) && (point2.Y < pointArray[1].Y))
                                {
                                    list4.Add(new Point(pointArray[0].X, point2.Y - 1));
                                    list4.Add(new Point(point2.X + 1, point2.Y - 1));
                                    list4.Add(new Point(point2.X + 1, point2.Y + 1));
                                    list4.Add(new Point(pointArray[0].X, point2.Y + 1));
                                }
                            }
                            list4.Add(new Point(pointArray[0].X, pointArray[1].Y));
                            break;

                        case DesignerEdges.Right:
                            list4.Add(new Point(pointArray[0].X, pointArray[0].Y));
                            for (int k = 0; k < list3.Count; k++)
                            {
                                Point point3 = list3[k];
                                if (((point3.X < pointArray[0].X) && (point3.Y > pointArray[0].Y)) && (point3.Y < pointArray[1].Y))
                                {
                                    list4.Add(new Point(pointArray[0].X, point3.Y - 1));
                                    list4.Add(new Point(point3.X - 1, point3.Y - 1));
                                    list4.Add(new Point(point3.X - 1, point3.Y + 1));
                                    list4.Add(new Point(pointArray[0].X, point3.Y + 1));
                                }
                            }
                            list4.Add(new Point(pointArray[0].X, pointArray[1].Y));
                            break;

                        case DesignerEdges.Top:
                            list4.Add(new Point(pointArray[0].X, pointArray[0].Y));
                            for (int m = 0; m < list3.Count; m++)
                            {
                                Point point4 = list3[m];
                                if (((point4.Y > pointArray[0].Y) && (point4.X > pointArray[0].X)) && (point4.X < pointArray[1].X))
                                {
                                    list4.Add(new Point(point4.X - 1, pointArray[0].Y));
                                    list4.Add(new Point(point4.X - 1, point4.Y + 1));
                                    list4.Add(new Point(point4.X + 1, point4.Y + 1));
                                    list4.Add(new Point(point4.X + 1, pointArray[0].Y));
                                }
                            }
                            list4.Add(new Point(pointArray[1].X, pointArray[0].Y));
                            break;

                        case DesignerEdges.Bottom:
                            list4.Add(new Point(pointArray[0].X, pointArray[0].Y));
                            for (int n = 0; n < list3.Count; n++)
                            {
                                Point point5 = list3[n];
                                if (((point5.Y < pointArray[0].Y) && (point5.X > pointArray[0].X)) && (point5.X < pointArray[1].X))
                                {
                                    list4.Add(new Point(point5.X - 1, pointArray[0].Y));
                                    list4.Add(new Point(point5.X - 1, point5.Y - 1));
                                    list4.Add(new Point(point5.X + 1, point5.Y - 1));
                                    list4.Add(new Point(point5.X + 1, pointArray[0].Y));
                                }
                            }
                            list4.Add(new Point(pointArray[1].X, pointArray[0].Y));
                            break;
                    }
                    for (int i = 1; i < list4.Count; i++)
                    {
                        list2.Add(list4[i - 1]);
                        list2.Add(list4[i]);
                    }
                    continue;
                }
                list2.AddRange(dictionary2[edges2]);
            }
            return list2.AsReadOnly();
        }

        public static void GetRoutingObstacles(IServiceProvider serviceProvider, object source, object target, out List<Rectangle> rectanglesToExclude, out List<Point> linesToExclude, out List<Point> pointsToExclude)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            ActivityDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(serviceProvider);
            ConnectionPoint point = source as ConnectionPoint;
            Point point2 = (point != null) ? point.Location : ((Point) source);
            ActivityDesigner designer = (point != null) ? point.AssociatedDesigner : safeRootDesigner.HitTest(point2).AssociatedDesigner;
            ConnectionPoint point3 = target as ConnectionPoint;
            Point point4 = (point3 != null) ? point3.Location : ((Point) target);
            ActivityDesigner designer3 = (point3 != null) ? point3.AssociatedDesigner : safeRootDesigner.HitTest(point4).AssociatedDesigner;
            Dictionary<int, ActivityDesigner> dictionary = new Dictionary<int, ActivityDesigner>();
            if (designer != null)
            {
                for (CompositeActivityDesigner designer4 = designer.ParentDesigner; designer4 != null; designer4 = designer4.ParentDesigner)
                {
                    if (dictionary.ContainsKey(designer4.GetHashCode()))
                    {
                        break;
                    }
                    dictionary.Add(designer4.GetHashCode(), designer4);
                }
            }
            if (designer3 != null)
            {
                for (CompositeActivityDesigner designer5 = designer3.ParentDesigner; designer5 != null; designer5 = designer5.ParentDesigner)
                {
                    if (dictionary.ContainsKey(designer5.GetHashCode()))
                    {
                        break;
                    }
                    dictionary.Add(designer5.GetHashCode(), designer5);
                }
            }
            rectanglesToExclude = new List<Rectangle>();
            pointsToExclude = new List<Point>();
            foreach (CompositeActivityDesigner designer6 in dictionary.Values)
            {
                ReadOnlyCollection<ActivityDesigner> containedDesigners = designer6.ContainedDesigners;
                for (int i = 0; i < containedDesigners.Count; i++)
                {
                    ActivityDesigner designer7 = containedDesigners[i];
                    if ((designer7.IsVisible && !dictionary.ContainsKey(designer7.GetHashCode())) && ((designer7 != designer) && (designer7 != designer3)))
                    {
                        Rectangle bounds = designer7.Bounds;
                        bounds.Inflate(ambientTheme.Margin);
                        rectanglesToExclude.Add(bounds);
                    }
                }
            }
            linesToExclude = new List<Point>();
            if (((designer != null) && (designer == designer3)) && !designer.IsRootDesigner)
            {
                linesToExclude.AddRange(GetDesignerEscapeCover(designer, new object[] { source, target }));
            }
            else
            {
                if ((designer != null) && !designer.IsRootDesigner)
                {
                    linesToExclude.AddRange(GetDesignerEscapeCover(designer, new object[] { source }));
                }
                if ((designer3 != null) && !designer3.IsRootDesigner)
                {
                    bool flag = true;
                    for (CompositeActivityDesigner designer8 = (designer != null) ? designer.ParentDesigner : null; designer8 != null; designer8 = (designer != null) ? designer8.ParentDesigner : null)
                    {
                        if (designer3 == designer8)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        linesToExclude.AddRange(GetDesignerEscapeCover(designer3, new object[] { target }));
                    }
                }
            }
        }

        public static Point[] Route(IServiceProvider serviceProvider, object source, object target, ICollection<Rectangle> userDefinedRoutingObstacles)
        {
            List<Rectangle> list;
            List<Point> list2;
            List<Point> list3;
            GetRoutingObstacles(serviceProvider, source, target, out list, out list2, out list3);
            if (userDefinedRoutingObstacles != null)
            {
                list.AddRange(userDefinedRoutingObstacles);
            }
            ActivityDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(serviceProvider);
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Point begin = (source is ConnectionPoint) ? ((ConnectionPoint) source).Location : ((Point) source);
            Point end = (target is ConnectionPoint) ? ((ConnectionPoint) target).Location : ((Point) target);
            Point[] segments = ConnectorRouter.Route(begin, end, new Size(2 * ambientTheme.Margin.Width, 2 * ambientTheme.Margin.Height), safeRootDesigner.Bounds, list.ToArray(), list2.ToArray(), list3.ToArray());
            if (!AreAllSegmentsVerticalOrHorizontal(segments))
            {
                segments = ConnectorRouter.Route(begin, end, ambientTheme.Margin, safeRootDesigner.Bounds, new Rectangle[0], list2.ToArray(), new Point[0]);
            }
            if (!AreAllSegmentsVerticalOrHorizontal(segments))
            {
                Point point3 = (DesignerGeometryHelper.SlopeOfLineSegment(begin, end) < 1f) ? new Point(end.X, begin.Y) : new Point(begin.X, end.Y);
                segments = new Point[] { begin, point3, end };
            }
            return segments;
        }
    }
}

