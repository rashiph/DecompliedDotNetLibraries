namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class TrackBarRenderer
    {
        private const int lineWidth = 2;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer;

        private TrackBarRenderer()
        {
        }

        public static void DrawBottomPointingThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbBottom.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawHorizontalThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.Thumb.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawHorizontalTicks(Graphics g, Rectangle bounds, int numTicks, EdgeStyle edgeStyle)
        {
            if (((numTicks > 0) && (bounds.Height > 0)) && ((bounds.Width > 0) && (g != null)))
            {
                InitializeRenderer(VisualStyleElement.TrackBar.Ticks.Normal, 1);
                if (numTicks == 1)
                {
                    visualStyleRenderer.DrawEdge(g, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), Edges.Left, edgeStyle, EdgeEffects.None);
                }
                else
                {
                    float num = (bounds.Width - 2f) / (numTicks - 1f);
                    while (numTicks > 0)
                    {
                        float num2 = bounds.X + ((numTicks - 1) * num);
                        visualStyleRenderer.DrawEdge(g, new Rectangle((int) Math.Round((double) num2), bounds.Y, 2, bounds.Height), Edges.Left, edgeStyle, EdgeEffects.None);
                        numTicks--;
                    }
                }
            }
        }

        public static void DrawHorizontalTrack(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.Track.Normal, 1);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawLeftPointingThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbLeft.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawRightPointingThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbRight.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawTopPointingThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbTop.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalThumb(Graphics g, Rectangle bounds, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbVertical.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalTicks(Graphics g, Rectangle bounds, int numTicks, EdgeStyle edgeStyle)
        {
            if (((numTicks > 0) && (bounds.Height > 0)) && ((bounds.Width > 0) && (g != null)))
            {
                InitializeRenderer(VisualStyleElement.TrackBar.TicksVertical.Normal, 1);
                if (numTicks == 1)
                {
                    visualStyleRenderer.DrawEdge(g, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), Edges.Top, edgeStyle, EdgeEffects.None);
                }
                else
                {
                    float num = (bounds.Height - 2f) / (numTicks - 1f);
                    while (numTicks > 0)
                    {
                        float num2 = bounds.Y + ((numTicks - 1) * num);
                        visualStyleRenderer.DrawEdge(g, new Rectangle(bounds.X, (int) Math.Round((double) num2), bounds.Width, 2), Edges.Top, edgeStyle, EdgeEffects.None);
                        numTicks--;
                    }
                }
            }
        }

        public static void DrawVerticalTrack(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.TrackVertical.Normal, 1);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static Size GetBottomPointingThumbSize(Graphics g, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbBottom.Normal, (int) state);
            return visualStyleRenderer.GetPartSize(g, ThemeSizeType.True);
        }

        public static Size GetLeftPointingThumbSize(Graphics g, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbLeft.Normal, (int) state);
            return visualStyleRenderer.GetPartSize(g, ThemeSizeType.True);
        }

        public static Size GetRightPointingThumbSize(Graphics g, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbRight.Normal, (int) state);
            return visualStyleRenderer.GetPartSize(g, ThemeSizeType.True);
        }

        public static Size GetTopPointingThumbSize(Graphics g, TrackBarThumbState state)
        {
            InitializeRenderer(VisualStyleElement.TrackBar.ThumbTop.Normal, (int) state);
            return visualStyleRenderer.GetPartSize(g, ThemeSizeType.True);
        }

        private static void InitializeRenderer(VisualStyleElement element, int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(element.ClassName, element.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(element.ClassName, element.Part, state);
            }
        }

        public static bool IsSupported
        {
            get
            {
                return VisualStyleRenderer.IsSupported;
            }
        }
    }
}

