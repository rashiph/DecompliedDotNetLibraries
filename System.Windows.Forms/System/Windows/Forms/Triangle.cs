namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    internal static class Triangle
    {
        private const double TRI_HEIGHT_RATIO = 2.5;
        private const double TRI_WIDTH_RATIO = 0.8;

        private static Point[] BuildTrianglePoints(TriangleDirection dir, Rectangle bounds)
        {
            Point[] points = new Point[3];
            int x = (int) (bounds.Width * 0.8);
            if ((x % 2) == 1)
            {
                x++;
            }
            int y = (int) Math.Ceiling((double) ((x / 2) * 2.5));
            int num3 = (int) (bounds.Height * 0.8);
            if ((num3 % 2) == 0)
            {
                num3++;
            }
            int num4 = (int) Math.Ceiling((double) ((num3 / 2) * 2.5));
            switch (dir)
            {
                case TriangleDirection.Up:
                    points[0] = new Point(0, y);
                    points[1] = new Point(x, y);
                    points[2] = new Point(x / 2, 0);
                    break;

                case TriangleDirection.Down:
                    points[0] = new Point(0, 0);
                    points[1] = new Point(x, 0);
                    points[2] = new Point(x / 2, y);
                    break;

                case TriangleDirection.Left:
                    points[0] = new Point(num3, 0);
                    points[1] = new Point(num3, num4);
                    points[2] = new Point(0, num4 / 2);
                    break;

                case TriangleDirection.Right:
                    points[0] = new Point(0, 0);
                    points[1] = new Point(0, num4);
                    points[2] = new Point(num3, num4 / 2);
                    break;
            }
            switch (dir)
            {
                case TriangleDirection.Up:
                case TriangleDirection.Down:
                    OffsetPoints(points, bounds.X + ((bounds.Width - y) / 2), bounds.Y + ((bounds.Height - x) / 2));
                    return points;

                case TriangleDirection.Left:
                case TriangleDirection.Right:
                    OffsetPoints(points, bounds.X + ((bounds.Width - num3) / 2), bounds.Y + ((bounds.Height - num4) / 2));
                    return points;
            }
            return points;
        }

        private static void OffsetPoints(Point[] points, int xOffset, int yOffset)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X += xOffset;
                points[i].Y += yOffset;
            }
        }

        public static void Paint(Graphics g, Rectangle bounds, TriangleDirection dir, Brush backBr, Pen backPen1, Pen backPen2, Pen backPen3, bool opaque)
        {
            Point[] points = BuildTrianglePoints(dir, bounds);
            if (opaque)
            {
                g.FillPolygon(backBr, points);
            }
            g.DrawLine(backPen1, points[0], points[1]);
            g.DrawLine(backPen2, points[1], points[2]);
            g.DrawLine(backPen3, points[2], points[0]);
        }
    }
}

