namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal static class ConnectorRouter
    {
        private static readonly Size DefaultSize = new Size(4, 4);

        private static void AddBoundPoint(ref List<DistanceFromPoint> extremitiesList, Point p, ConnectorSegment segment, Point Z)
        {
            if (((p.X != -2147483648) && (p.X != 0x7fffffff)) && ((p.Y != -2147483648) && (p.Y != 0x7fffffff)))
            {
                extremitiesList.Add(new DistanceFromPoint(segment, Z, p));
            }
        }

        private static Point? EscapeAlgorithm(CoverSet coverSet, Point Z, Point targetPoint, ref List<Point> LeA, ref List<ConnectorSegment> LhA, ref List<ConnectorSegment> LvA, ref List<ConnectorSegment> LhB, ref List<ConnectorSegment> LvB, ref Orientation orientationA, out ConnectorSegment intersectionSegmentA, out ConnectorSegment intersectionSegmentB, Size margin, ref bool noEscapeA)
        {
            bool flag2;
            bool flag3;
            bool flag4;
            bool flag5;
            Point? nullable = null;
            intersectionSegmentA = null;
            intersectionSegmentB = null;
            ConnectorSegment cover = coverSet.GetCover(Z, DesignerEdges.Left);
            ConnectorSegment segment2 = coverSet.GetCover(Z, DesignerEdges.Right);
            ConnectorSegment segment3 = coverSet.GetCover(Z, DesignerEdges.Bottom);
            ConnectorSegment segment4 = coverSet.GetCover(Z, DesignerEdges.Top);
            ConnectorSegment item = ConnectorSegment.SegmentFromLeftToRightCover(coverSet, Z);
            LhA.Add(item);
            ConnectorSegment segment6 = ConnectorSegment.SegmentFromBottomToTopCover(coverSet, Z);
            LvA.Add(segment6);
            for (int i = 0; i < LvB.Count; i++)
            {
                ConnectorSegment segment = LvB[i];
                nullable = item.Intersect(segment);
                if (nullable.HasValue)
                {
                    intersectionSegmentA = item;
                    intersectionSegmentB = segment;
                    return nullable;
                }
            }
            for (int j = 0; j < LhB.Count; j++)
            {
                ConnectorSegment segment8 = LhB[j];
                nullable = segment6.Intersect(segment8);
                if (nullable.HasValue)
                {
                    intersectionSegmentA = segment6;
                    intersectionSegmentB = segment8;
                    return nullable;
                }
            }
            Point? nullable2 = EscapeProcessI(coverSet, Z, segment6, Orientation.Horizontal, margin);
            if (nullable2.HasValue)
            {
                orientationA = Orientation.Vertical;
                LeA.Add(nullable2.Value);
                return null;
            }
            nullable2 = EscapeProcessI(coverSet, Z, item, Orientation.Vertical, margin);
            if (nullable2.HasValue)
            {
                orientationA = Orientation.Horizontal;
                LeA.Add(nullable2.Value);
                return null;
            }
            bool intersectionFlag = false;
            Point empty = Point.Empty;
            Point r = Point.Empty;
            Point point3 = Point.Empty;
            Point point4 = Point.Empty;
            if (segment4 != null)
            {
                empty = new Point(Z.X, segment4.A.Y);
            }
            if (segment2 != null)
            {
                r = new Point(segment2.A.X, Z.Y);
            }
            if (segment3 != null)
            {
                point3 = new Point(Z.X, segment3.A.Y);
            }
            if (cover != null)
            {
                point4 = new Point(cover.A.X, Z.Y);
            }
            do
            {
                flag2 = flag3 = flag4 = flag5 = false;
                if (segment4 != null)
                {
                    empty.Y -= margin.Height;
                    if (empty.Y > Z.Y)
                    {
                        flag2 = true;
                        Point? nullable3 = EscapeProcessII(coverSet, Orientation.Vertical, ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, empty, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (nullable3.HasValue)
                        {
                            LvA.Add(segment6);
                            if (intersectionFlag)
                            {
                                return nullable3;
                            }
                            orientationA = Orientation.Horizontal;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, empty));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(empty, nullable3.Value));
                            LeA.Add(nullable3.Value);
                            return null;
                        }
                    }
                }
                if (segment2 != null)
                {
                    r.X -= margin.Width;
                    if (r.X > Z.X)
                    {
                        flag3 = true;
                        Point? nullable4 = EscapeProcessII(coverSet, Orientation.Horizontal, ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, r, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (nullable4.HasValue)
                        {
                            LhA.Add(item);
                            if (intersectionFlag)
                            {
                                return nullable4;
                            }
                            orientationA = Orientation.Vertical;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, r));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(r, nullable4.Value));
                            LeA.Add(nullable4.Value);
                            return null;
                        }
                    }
                }
                if (segment3 != null)
                {
                    point3.Y += margin.Height;
                    if (point3.Y < Z.Y)
                    {
                        flag4 = true;
                        Point? nullable5 = EscapeProcessII(coverSet, Orientation.Vertical, ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, point3, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (nullable5.HasValue)
                        {
                            LvA.Add(segment6);
                            if (intersectionFlag)
                            {
                                return nullable5;
                            }
                            orientationA = Orientation.Horizontal;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, point3));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(point3, nullable5.Value));
                            LeA.Add(nullable5.Value);
                            return null;
                        }
                    }
                }
                if (cover != null)
                {
                    point4.X += margin.Width;
                    if (point4.X < Z.X)
                    {
                        flag5 = true;
                        Point? nullable6 = EscapeProcessII(coverSet, Orientation.Horizontal, ref LeA, ref LhA, ref LvA, ref LhB, ref LvB, Z, point4, margin, out intersectionFlag, out intersectionSegmentA, out intersectionSegmentB);
                        if (nullable6.HasValue)
                        {
                            LhA.Add(item);
                            if (intersectionFlag)
                            {
                                return nullable6;
                            }
                            orientationA = Orientation.Vertical;
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(Z, point4));
                            coverSet.AddUsedEscapeLine(new ConnectorSegment(point4, nullable6.Value));
                            LeA.Add(nullable6.Value);
                            return null;
                        }
                    }
                }
            }
            while ((flag2 || flag3) || (flag4 || flag5));
            noEscapeA = true;
            return null;
        }

        private static Point? EscapeProcessI(CoverSet coverSet, Point Z, ConnectorSegment escapeLine, Orientation orientation, Size margin)
        {
            List<DistanceFromPoint> extremitiesList = new List<DistanceFromPoint>(4);
            ConnectorSegment cover = coverSet.GetCover(Z, (orientation == Orientation.Horizontal) ? DesignerEdges.Left : DesignerEdges.Bottom);
            if (cover != null)
            {
                AddBoundPoint(ref extremitiesList, cover.A, cover, Z);
                AddBoundPoint(ref extremitiesList, cover.B, cover, Z);
            }
            ConnectorSegment segment = coverSet.GetCover(Z, (orientation == Orientation.Horizontal) ? DesignerEdges.Right : DesignerEdges.Top);
            if (segment != null)
            {
                AddBoundPoint(ref extremitiesList, segment.A, segment, Z);
                AddBoundPoint(ref extremitiesList, segment.B, segment, Z);
            }
            if (extremitiesList.Count != 0)
            {
                DistanceSorter.Sort(ref extremitiesList);
                for (int i = 0; i < extremitiesList.Count; i++)
                {
                    DesignerEdges edges;
                    Point point3;
                    Point p = extremitiesList[i].P;
                    int x = Math.Sign((int) (p.X - Z.X));
                    Point point2 = new Point(x, Math.Sign((int) (p.Y - Z.Y)));
                    if (((orientation == Orientation.Vertical) ? point2.X : point2.Y) == 0)
                    {
                        p = extremitiesList[i].ConnectorSegment.ExtendPointOutwards(p);
                        int introduced13 = Math.Sign((int) (p.X - Z.X));
                        point2 = new Point(introduced13, Math.Sign((int) (p.Y - Z.Y)));
                        p = extremitiesList[i].P;
                    }
                    if (orientation == Orientation.Vertical)
                    {
                        edges = (point2.Y < 0) ? DesignerEdges.Bottom : DesignerEdges.Top;
                    }
                    else
                    {
                        edges = (point2.X < 0) ? DesignerEdges.Left : DesignerEdges.Right;
                    }
                    if (orientation == Orientation.Vertical)
                    {
                        point3 = new Point(p.X + (point2.X * margin.Width), Z.Y);
                    }
                    else
                    {
                        point3 = new Point(Z.X, p.Y + (point2.Y * margin.Height));
                    }
                    ConnectorSegment segment4 = new ConnectorSegment(Z, point3);
                    if (((!coverSet.EscapeLineHasBeenUsed(segment4, point3) && escapeLine.IsPointOnSegment(point3)) && ((escapeLine.A != point3) && (escapeLine.B != point3))) && coverSet.IsEscapePoint(Z, point3, edges))
                    {
                        coverSet.AddUsedEscapeLine(segment4);
                        return new Point?(point3);
                    }
                }
            }
            return null;
        }

        private static Point? EscapeProcessII(CoverSet coverSet, Orientation orientation, ref List<Point> LeA, ref List<ConnectorSegment> LhA, ref List<ConnectorSegment> LvA, ref List<ConnectorSegment> LhB, ref List<ConnectorSegment> LvB, Point Z, Point R, Size margin, out bool intersectionFlag, out ConnectorSegment intersectionSegmentA, out ConnectorSegment intersectionSegmentB)
        {
            intersectionFlag = false;
            intersectionSegmentA = null;
            intersectionSegmentB = null;
            ConnectorSegment escapeLine = ConnectorSegment.SegmentFromLeftToRightCover(coverSet, R);
            ConnectorSegment segment2 = ConnectorSegment.SegmentFromBottomToTopCover(coverSet, R);
            for (int i = 0; i < LvB.Count; i++)
            {
                ConnectorSegment segment = LvB[i];
                Point? nullable = escapeLine.Intersect(segment);
                if (nullable.HasValue)
                {
                    intersectionFlag = true;
                    intersectionSegmentA = escapeLine;
                    intersectionSegmentB = segment;
                    LeA.Add(R);
                    return nullable;
                }
            }
            for (int j = 0; j < LhB.Count; j++)
            {
                ConnectorSegment segment4 = LhB[j];
                Point? nullable2 = segment2.Intersect(segment4);
                if (nullable2.HasValue)
                {
                    intersectionFlag = true;
                    intersectionSegmentA = segment2;
                    intersectionSegmentB = segment4;
                    LeA.Add(R);
                    return nullable2;
                }
            }
            Point? nullable3 = null;
            if (orientation == Orientation.Horizontal)
            {
                nullable3 = EscapeProcessI(coverSet, R, segment2, Orientation.Horizontal, margin);
                if (nullable3.HasValue)
                {
                    LvA.Add(segment2);
                    LeA.Add(R);
                    return nullable3;
                }
                nullable3 = EscapeProcessI(coverSet, R, escapeLine, Orientation.Vertical, margin);
                if (nullable3.HasValue)
                {
                    LhA.Add(escapeLine);
                    LeA.Add(R);
                    return nullable3;
                }
            }
            else
            {
                nullable3 = EscapeProcessI(coverSet, R, escapeLine, Orientation.Vertical, margin);
                if (nullable3.HasValue)
                {
                    LhA.Add(escapeLine);
                    LeA.Add(R);
                    return nullable3;
                }
                nullable3 = EscapeProcessI(coverSet, R, segment2, Orientation.Horizontal, margin);
                if (nullable3.HasValue)
                {
                    LvA.Add(segment2);
                    LeA.Add(R);
                    return nullable3;
                }
            }
            return null;
        }

        private static List<Point> FirstRefinementAlgorithm(List<Point> Le, Point intersection, ConnectorSegment intersectionSegment)
        {
            Point point;
            List<Point> list = new List<Point>();
            for (ConnectorSegment segment = intersectionSegment; Le.Count > 0; segment = segment.PeprendecularThroughPoint(point))
            {
                int index = Le.Count - 1;
                while (!segment.PointLiesOnThisLine(Le[index]) && (index > 0))
                {
                    index--;
                }
                while ((index > 0) && segment.PointLiesOnThisLine(Le[index - 1]))
                {
                    index--;
                }
                point = Le[index];
                list.Add(point);
                while (Le.Count > index)
                {
                    Le.RemoveAt(index);
                }
            }
            return list;
        }

        private static Point[] GetRoutedLineSegments(Point begin, Point end, Size margin, Rectangle[] rectanglesToExclude, Point[] linesToExclude)
        {
            if (rectanglesToExclude == null)
            {
                throw new ArgumentNullException("rectanglesToExclude");
            }
            if (linesToExclude == null)
            {
                throw new ArgumentNullException("linesToExclude");
            }
            if ((linesToExclude.Length % 2) > 0)
            {
                throw new ArgumentException(DR.GetString("Error_Connector2", new object[0]));
            }
            CoverSet coverSet = new CoverSet(rectanglesToExclude, linesToExclude);
            coverSet.ClearUsedLines();
            Point point = begin;
            Point point2 = end;
            List<Point> leA = new List<Point>();
            List<Point> le = new List<Point>();
            List<ConnectorSegment> lhA = new List<ConnectorSegment>();
            List<ConnectorSegment> lvA = new List<ConnectorSegment>();
            List<ConnectorSegment> lhB = new List<ConnectorSegment>();
            List<ConnectorSegment> lvB = new List<ConnectorSegment>();
            Orientation horizontal = Orientation.Horizontal;
            Orientation orientation2 = Orientation.Horizontal;
            leA.Add(begin);
            le.Add(end);
            bool noEscapeA = false;
            bool flag2 = false;
            Point? nullable = null;
            ConnectorSegment intersectionSegmentA = null;
            ConnectorSegment intersectionSegmentB = null;
            try
            {
            Label_00A1:
                while (noEscapeA)
                {
                    if (flag2)
                    {
                        goto Label_0172;
                    }
                    List<Point> list7 = leA;
                    leA = le;
                    le = list7;
                    Point point3 = point;
                    point = point2;
                    point2 = point3;
                    bool flag3 = noEscapeA;
                    noEscapeA = flag2;
                    flag2 = flag3;
                    Orientation orientation3 = horizontal;
                    horizontal = orientation2;
                    orientation2 = orientation3;
                    List<ConnectorSegment> list8 = lhA;
                    lhA = lhB;
                    lhB = list8;
                    list8 = lvA;
                    lvA = lvB;
                    lvB = list8;
                }
                Point z = leA[leA.Count - 1];
                Point targetPoint = point2;
                nullable = EscapeAlgorithm(coverSet, z, targetPoint, ref leA, ref lhA, ref lvA, ref lhB, ref lvB, ref horizontal, out intersectionSegmentA, out intersectionSegmentB, margin, ref noEscapeA);
                if (!nullable.HasValue)
                {
                    List<Point> list9 = leA;
                    leA = le;
                    le = list9;
                    Point point6 = point;
                    point = point2;
                    point2 = point6;
                    bool flag4 = noEscapeA;
                    noEscapeA = flag2;
                    flag2 = flag4;
                    Orientation orientation4 = horizontal;
                    horizontal = orientation2;
                    orientation2 = orientation4;
                    List<ConnectorSegment> list10 = lhA;
                    lhA = lhB;
                    lhB = list10;
                    list10 = lvA;
                    lvA = lvB;
                    lvB = list10;
                    goto Label_00A1;
                }
            Label_0172:
                if (!nullable.HasValue)
                {
                    return null;
                }
                List<Point> refinedPath = new List<Point>();
                leA = FirstRefinementAlgorithm(leA, nullable.Value, intersectionSegmentA);
                le = FirstRefinementAlgorithm(le, nullable.Value, intersectionSegmentB);
                for (int i = leA.Count - 1; i >= 0; i--)
                {
                    refinedPath.Add(leA[i]);
                }
                refinedPath.Add(nullable.Value);
                for (int j = 0; j < le.Count; j++)
                {
                    refinedPath.Add(le[j]);
                }
                SecondRefinementAlgorithm(coverSet, ref refinedPath, margin);
                if ((refinedPath.Count > 1) && (refinedPath[refinedPath.Count - 1] == begin))
                {
                    refinedPath.Reverse();
                }
                return refinedPath.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public static Point[] Route(Point begin, Point end, Size margin, Rectangle enclosingRectangle, Rectangle[] rectanglesToExclude, Point[] linesToExclude, Point[] pointsToExclude)
        {
            List<Rectangle> list = new List<Rectangle>(rectanglesToExclude);
            if (!enclosingRectangle.IsEmpty)
            {
                begin.X = Math.Min(Math.Max(begin.X, enclosingRectangle.Left + 1), enclosingRectangle.Right - 1);
                begin.Y = Math.Min(Math.Max(begin.Y, enclosingRectangle.Top + 1), enclosingRectangle.Bottom - 1);
                list.Insert(0, enclosingRectangle);
            }
            List<Point> list2 = new List<Point>(linesToExclude);
            int num = Math.Max(margin.Width / 2, 1);
            int num2 = Math.Max(margin.Height / 2, 1);
            foreach (Point point in pointsToExclude)
            {
                list2.Add(new Point(point.X - num, point.Y));
                list2.Add(new Point(point.X + num, point.Y));
                list2.Add(new Point(point.X, point.Y - num2));
                list2.Add(new Point(point.X, point.Y + num2));
            }
            return GetRoutedLineSegments(begin, end, margin, list.ToArray(), list2.ToArray());
        }

        private static void SecondRefinementAlgorithm(CoverSet coverSet, ref List<Point> refinedPath, Size margin)
        {
            int num;
            List<Point> list = new List<Point>();
            for (num = 0; num < (refinedPath.Count - 1); num++)
            {
                Point a = refinedPath[num];
                Point b = refinedPath[num + 1];
                ConnectorSegment segment = ConnectorSegment.ConstructBoundSegment(coverSet, a, b);
                int num2 = num + 2;
                while (num2 < (refinedPath.Count - 1))
                {
                    Point point3 = refinedPath[num2];
                    Point point4 = refinedPath[num2 + 1];
                    ConnectorSegment segment2 = ConnectorSegment.ConstructBoundSegment(coverSet, point3, point4);
                    Point? nullable = segment.Intersect(segment2);
                    if (nullable.HasValue)
                    {
                        list.Clear();
                        for (int i = 0; i <= num; i++)
                        {
                            list.Add(refinedPath[i]);
                        }
                        list.Add(nullable.Value);
                        for (int j = num2 + 1; j < refinedPath.Count; j++)
                        {
                            list.Add(refinedPath[j]);
                        }
                        List<Point> list2 = refinedPath;
                        refinedPath = list;
                        list = list2;
                        list.Clear();
                        num2 = num + 2;
                    }
                    else
                    {
                        num2++;
                    }
                }
            }
            num = 0;
            while (num < (refinedPath.Count - 1))
            {
                Point point5 = refinedPath[num];
                Point point6 = refinedPath[num + 1];
                bool flag = false;
                ConnectorSegment segment3 = ConnectorSegment.ConstructBoundSegment(coverSet, point5, point6);
                if (segment3 != null)
                {
                    Point point7 = new Point(point6.X - point5.X, point6.Y - point5.Y);
                    int introduced30 = Math.Abs((int) (point7.X / margin.Width));
                    int num5 = Math.Max(introduced30, Math.Abs((int) (point7.Y / margin.Height)));
                    point7.X = Math.Sign(point7.X);
                    point7.Y = Math.Sign(point7.Y);
                    for (int k = 1; k <= num5; k++)
                    {
                        Point point8 = new Point(point5.X + ((k * margin.Width) * point7.X), point5.Y + ((k * margin.Height) * point7.Y));
                        if (point8 == point6)
                        {
                            break;
                        }
                        ConnectorSegment segment4 = ConnectorSegment.ConstructBoundSegment(coverSet, point8, (segment3.Orientation == Orientation.Horizontal) ? Orientation.Vertical : Orientation.Horizontal);
                        for (int m = num + 2; (m < (refinedPath.Count - 1)) && !flag; m++)
                        {
                            Point point9 = refinedPath[m];
                            Point point10 = refinedPath[m + 1];
                            ConnectorSegment segment5 = new ConnectorSegment(point9, point10);
                            Point? nullable2 = segment4.Intersect(segment5);
                            if (nullable2.HasValue && segment5.IsPointOnSegment(nullable2.Value))
                            {
                                flag = true;
                                list.Clear();
                                for (int n = 0; n <= num; n++)
                                {
                                    list.Add(refinedPath[n]);
                                }
                                list.Add(point8);
                                list.Add(nullable2.Value);
                                for (int num9 = m + 1; num9 < refinedPath.Count; num9++)
                                {
                                    list.Add(refinedPath[num9]);
                                }
                                List<Point> list3 = refinedPath;
                                refinedPath = list;
                                list3.Clear();
                                break;
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    num++;
                }
            }
        }

        [DebuggerDisplay("Segment ( {A.X}, {A.Y} ) - ( {B.X},{B.Y} ), {Orientation}")]
        private sealed class ConnectorSegment
        {
            private System.Windows.Forms.Orientation orientation;
            private Point point1;
            private Point point2;

            public ConnectorSegment(Point point1, Point point2)
            {
                if ((point1.X != point2.X) && (point1.Y != point2.Y))
                {
                    throw new InvalidOperationException(SR.GetString("Error_InvalidConnectorSegment"));
                }
                this.point1 = point1;
                this.point2 = point2;
                this.orientation = (this.point1.X == this.point2.X) ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal;
            }

            public static ConnectorRouter.ConnectorSegment ConstructBoundSegment(ConnectorRouter.CoverSet coverSet, Point a, Point b)
            {
                if ((a.X != b.X) && (a.Y != b.Y))
                {
                    return null;
                }
                return ConstructBoundSegment(coverSet, a, (a.X == b.X) ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal);
            }

            public static ConnectorRouter.ConnectorSegment ConstructBoundSegment(ConnectorRouter.CoverSet coverSet, Point a, System.Windows.Forms.Orientation orientation)
            {
                if (orientation != System.Windows.Forms.Orientation.Horizontal)
                {
                    return SegmentFromBottomToTopCover(coverSet, a);
                }
                return SegmentFromLeftToRightCover(coverSet, a);
            }

            public bool Covers(Point p)
            {
                if (this.orientation != System.Windows.Forms.Orientation.Horizontal)
                {
                    return ((p.Y >= Math.Min(this.point1.Y, this.point2.Y)) && (p.Y <= Math.Max(this.point1.Y, this.point2.Y)));
                }
                return ((p.X >= Math.Min(this.point1.X, this.point2.X)) && (p.X <= Math.Max(this.point1.X, this.point2.X)));
            }

            public static double DistanceBetweenPoints(Point p, Point q)
            {
                return Math.Sqrt(((p.X - q.X) * (p.X - q.X)) + ((p.Y - q.Y) * (p.Y - q.Y)));
            }

            public override bool Equals(object obj)
            {
                ConnectorRouter.ConnectorSegment segment = obj as ConnectorRouter.ConnectorSegment;
                if (segment == null)
                {
                    return false;
                }
                return (((this.point1 == segment.A) && (this.point2 == segment.B)) && (this.Orientation == segment.Orientation));
            }

            public Point ExtendPointOutwards(Point p)
            {
                if ((p != this.point1) && (p != this.point2))
                {
                    return p;
                }
                int num = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? p.X : p.Y;
                int num2 = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? this.point1.X : this.point1.Y;
                int num3 = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? this.point2.X : this.point2.Y;
                if (num == Math.Min(num2, num3))
                {
                    num--;
                }
                else
                {
                    num++;
                }
                return new Point((this.orientation == System.Windows.Forms.Orientation.Horizontal) ? num : p.X, (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? p.Y : num);
            }

            public override int GetHashCode()
            {
                return ((this.point1.GetHashCode() ^ this.point2.GetHashCode()) ^ this.Orientation.GetHashCode());
            }

            public Point? Intersect(ConnectorRouter.ConnectorSegment segment)
            {
                if (this.orientation != segment.Orientation)
                {
                    ConnectorRouter.ConnectorSegment segment2 = (this.orientation == System.Windows.Forms.Orientation.Vertical) ? this : segment;
                    ConnectorRouter.ConnectorSegment segment3 = (this.orientation == System.Windows.Forms.Orientation.Vertical) ? segment : this;
                    if ((segment2.A.X < Math.Min(segment3.A.X, segment3.B.X)) || (segment2.A.X > Math.Max(segment3.A.X, segment3.B.X)))
                    {
                        return null;
                    }
                    if ((segment3.A.Y >= Math.Min(segment2.A.Y, segment2.B.Y)) && (segment3.A.Y <= Math.Max(segment2.A.Y, segment2.B.Y)))
                    {
                        return new Point(segment2.A.X, segment3.A.Y);
                    }
                }
                return null;
            }

            public bool IsPointOnSegment(Point p)
            {
                if (((this.orientation == System.Windows.Forms.Orientation.Horizontal) && (p.Y != this.point1.Y)) || ((this.orientation == System.Windows.Forms.Orientation.Vertical) && (p.X != this.point1.X)))
                {
                    return false;
                }
                int num = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? p.X : p.Y;
                int num2 = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? this.point1.X : this.point1.Y;
                int num3 = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? this.point2.X : this.point2.Y;
                return ((num >= Math.Min(num2, num3)) && (num <= Math.Max(num2, num3)));
            }

            public ConnectorRouter.ConnectorSegment PeprendecularThroughPoint(Point p)
            {
                System.Windows.Forms.Orientation orientation = (this.orientation == System.Windows.Forms.Orientation.Horizontal) ? System.Windows.Forms.Orientation.Vertical : System.Windows.Forms.Orientation.Horizontal;
                Point point = new Point(p.X, p.Y);
                if (orientation == System.Windows.Forms.Orientation.Horizontal)
                {
                    point.X = 0x7fffffff;
                }
                else
                {
                    point.Y = 0x7fffffff;
                }
                return new ConnectorRouter.ConnectorSegment(p, point);
            }

            public bool PointLiesOnThisLine(Point p)
            {
                if (this.orientation != System.Windows.Forms.Orientation.Horizontal)
                {
                    return (p.X == this.point1.X);
                }
                return (p.Y == this.point1.Y);
            }

            public static ConnectorRouter.ConnectorSegment SegmentFromBottomToTopCover(ConnectorRouter.CoverSet coverSet, Point p)
            {
                ConnectorRouter.ConnectorSegment cover = coverSet.GetCover(p, DesignerEdges.Bottom);
                ConnectorRouter.ConnectorSegment segment2 = coverSet.GetCover(p, DesignerEdges.Top);
                Point point = new Point(p.X, (cover != null) ? cover.A.Y : -2147483648);
                return new ConnectorRouter.ConnectorSegment(point, new Point(p.X, (segment2 != null) ? segment2.A.Y : 0x7fffffff));
            }

            public static ConnectorRouter.ConnectorSegment SegmentFromLeftToRightCover(ConnectorRouter.CoverSet coverSet, Point p)
            {
                ConnectorRouter.ConnectorSegment cover = coverSet.GetCover(p, DesignerEdges.Left);
                ConnectorRouter.ConnectorSegment segment2 = coverSet.GetCover(p, DesignerEdges.Right);
                Point point = new Point((cover != null) ? cover.A.X : -2147483648, p.Y);
                return new ConnectorRouter.ConnectorSegment(point, new Point((segment2 != null) ? segment2.A.X : 0x7fffffff, p.Y));
            }

            public Point A
            {
                get
                {
                    return this.point1;
                }
            }

            public Point B
            {
                get
                {
                    return this.point2;
                }
            }

            public System.Windows.Forms.Orientation Orientation
            {
                get
                {
                    return this.orientation;
                }
            }
        }

        private sealed class CoverSet
        {
            private List<ConnectorRouter.ConnectorSegment> horizontalCovers = new List<ConnectorRouter.ConnectorSegment>();
            private List<ConnectorRouter.ConnectorSegment> usedEscapeLine = new List<ConnectorRouter.ConnectorSegment>();
            private List<ConnectorRouter.ConnectorSegment> verticalCovers = new List<ConnectorRouter.ConnectorSegment>();

            public CoverSet(Rectangle[] rectanglesToExclude, Point[] linesToExclude)
            {
                foreach (Rectangle rectangle in rectanglesToExclude)
                {
                    this.AddCover(new ConnectorRouter.ConnectorSegment(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Left, rectangle.Bottom)));
                    this.AddCover(new ConnectorRouter.ConnectorSegment(new Point(rectangle.Right, rectangle.Top), new Point(rectangle.Right, rectangle.Bottom)));
                    this.AddCover(new ConnectorRouter.ConnectorSegment(new Point(rectangle.Left, rectangle.Top), new Point(rectangle.Right, rectangle.Top)));
                    this.AddCover(new ConnectorRouter.ConnectorSegment(new Point(rectangle.Left, rectangle.Bottom), new Point(rectangle.Right, rectangle.Bottom)));
                }
                for (int i = 0; i < (linesToExclude.Length / 2); i++)
                {
                    this.AddCover(new ConnectorRouter.ConnectorSegment(linesToExclude[i * 2], linesToExclude[(i * 2) + 1]));
                }
            }

            public void AddCover(ConnectorRouter.ConnectorSegment cover)
            {
                List<ConnectorRouter.ConnectorSegment> list = (cover.Orientation == Orientation.Vertical) ? this.verticalCovers : this.horizontalCovers;
                for (int i = 0; i < list.Count; i++)
                {
                    ConnectorRouter.ConnectorSegment segment = list[i];
                    if (cover.IsPointOnSegment(segment.A) && cover.IsPointOnSegment(segment.B))
                    {
                        list.RemoveAt(i);
                        break;
                    }
                    if (segment.IsPointOnSegment(cover.A) && segment.IsPointOnSegment(cover.B))
                    {
                        return;
                    }
                }
                list.Add(cover);
            }

            public void AddUsedEscapeLine(ConnectorRouter.ConnectorSegment segment)
            {
                this.usedEscapeLine.Add(segment);
            }

            public void ClearUsedLines()
            {
                this.usedEscapeLine.Clear();
            }

            public bool EscapeLineHasBeenUsed(ConnectorRouter.ConnectorSegment segment, Point escapePoint)
            {
                for (int i = 0; i < this.usedEscapeLine.Count; i++)
                {
                    ConnectorRouter.ConnectorSegment segment2 = this.usedEscapeLine[i];
                    if (segment2.IsPointOnSegment(escapePoint))
                    {
                        return true;
                    }
                }
                return false;
            }

            public ConnectorRouter.ConnectorSegment GetCover(Point p, DesignerEdges side)
            {
                ConnectorRouter.ConnectorSegment segment = null;
                int num = 0;
                if ((side == DesignerEdges.Left) || (side == DesignerEdges.Right))
                {
                    for (int j = 0; j < this.verticalCovers.Count; j++)
                    {
                        ConnectorRouter.ConnectorSegment segment2 = this.verticalCovers[j];
                        int num3 = (side == DesignerEdges.Left) ? (p.X - segment2.A.X) : (segment2.A.X - p.X);
                        if (((num3 > 0) && segment2.Covers(p)) && ((segment == null) || (num > num3)))
                        {
                            segment = segment2;
                            num = num3;
                        }
                    }
                    return segment;
                }
                for (int i = 0; i < this.horizontalCovers.Count; i++)
                {
                    ConnectorRouter.ConnectorSegment segment3 = this.horizontalCovers[i];
                    int num5 = (side == DesignerEdges.Bottom) ? (p.Y - segment3.A.Y) : (segment3.A.Y - p.Y);
                    if (((num5 > 0) && segment3.Covers(p)) && ((segment == null) || (num > num5)))
                    {
                        segment = segment3;
                        num = num5;
                    }
                }
                return segment;
            }

            public List<ConnectorRouter.ConnectorSegment> GetCovers(Point p, DesignerEdges side)
            {
                List<ConnectorRouter.ConnectorSegment> list = new List<ConnectorRouter.ConnectorSegment>();
                if ((side == DesignerEdges.Left) || (side == DesignerEdges.Right))
                {
                    for (int j = 0; j < this.verticalCovers.Count; j++)
                    {
                        ConnectorRouter.ConnectorSegment item = this.verticalCovers[j];
                        int num2 = (side == DesignerEdges.Left) ? (p.X - item.A.X) : (item.A.X - p.X);
                        if ((num2 > 0) && item.Covers(p))
                        {
                            list.Add(item);
                        }
                    }
                    return list;
                }
                for (int i = 0; i < this.horizontalCovers.Count; i++)
                {
                    ConnectorRouter.ConnectorSegment segment2 = this.horizontalCovers[i];
                    int num4 = (side == DesignerEdges.Bottom) ? (p.Y - segment2.A.Y) : (segment2.A.Y - p.Y);
                    if ((num4 > 0) && segment2.Covers(p))
                    {
                        list.Add(segment2);
                    }
                }
                return list;
            }

            public bool IsEscapePoint(Point origin, Point escape, DesignerEdges side)
            {
                int num;
                ConnectorRouter.ConnectorSegment cover = this.GetCover(origin, side);
                if ((side == DesignerEdges.Left) || (side == DesignerEdges.Right))
                {
                    num = cover.A.X - escape.X;
                }
                else
                {
                    num = cover.A.Y - escape.Y;
                }
                if (cover.Covers(escape))
                {
                    return false;
                }
                List<ConnectorRouter.ConnectorSegment> covers = this.GetCovers(escape, side);
                for (int i = 0; i < covers.Count; i++)
                {
                    int num3;
                    ConnectorRouter.ConnectorSegment segment2 = covers[i];
                    if (segment2 == cover)
                    {
                        return false;
                    }
                    if ((side == DesignerEdges.Left) || (side == DesignerEdges.Right))
                    {
                        num3 = Math.Abs((int) (segment2.A.X - escape.X));
                    }
                    else
                    {
                        num3 = Math.Abs((int) (segment2.A.Y - escape.Y));
                    }
                    if ((Math.Sign(num3) == Math.Sign(num)) && (Math.Abs(num3) < Math.Abs(num)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DistanceFromPoint
        {
            public System.Workflow.ComponentModel.Design.ConnectorRouter.ConnectorSegment ConnectorSegment;
            public Point P;
            public double Distance;
            public DistanceFromPoint(System.Workflow.ComponentModel.Design.ConnectorRouter.ConnectorSegment segment, Point z, Point p)
            {
                this.ConnectorSegment = segment;
                this.P = p;
                this.Distance = System.Workflow.ComponentModel.Design.ConnectorRouter.ConnectorSegment.DistanceBetweenPoints(z, p);
            }
        }

        private sealed class DistanceSorter : IComparer<ConnectorRouter.DistanceFromPoint>
        {
            private DistanceSorter()
            {
            }

            public static void Sort(ref List<ConnectorRouter.DistanceFromPoint> distances)
            {
                ConnectorRouter.DistanceSorter comparer = new ConnectorRouter.DistanceSorter();
                distances.Sort(comparer);
            }

            int IComparer<ConnectorRouter.DistanceFromPoint>.Compare(ConnectorRouter.DistanceFromPoint lhs, ConnectorRouter.DistanceFromPoint rhs)
            {
                if (lhs.Distance == rhs.Distance)
                {
                    return 0;
                }
                if (lhs.Distance > rhs.Distance)
                {
                    return 1;
                }
                return -1;
            }
        }
    }
}

