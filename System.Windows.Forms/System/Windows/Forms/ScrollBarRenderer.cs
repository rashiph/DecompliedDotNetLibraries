namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class ScrollBarRenderer
    {
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer;

        private ScrollBarRenderer()
        {
        }

        public static void DrawArrowButton(Graphics g, Rectangle bounds, ScrollBarArrowButtonState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.ArrowButton.LeftNormal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawHorizontalThumb(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.ThumbButtonHorizontal.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawHorizontalThumbGrip(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.GripperHorizontal.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawLeftHorizontalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.LeftTrackHorizontal.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawLowerVerticalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.LowerTrackVertical.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawRightHorizontalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.RightTrackHorizontal.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawSizeBox(Graphics g, Rectangle bounds, ScrollBarSizeBoxState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.SizeBox.LeftAlign, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawUpperVerticalTrack(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.UpperTrackVertical.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalThumb(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.ThumbButtonVertical.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawVerticalThumbGrip(Graphics g, Rectangle bounds, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.GripperVertical.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static Size GetSizeBoxSize(Graphics g, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.SizeBox.LeftAlign, (int) state);
            return visualStyleRenderer.GetPartSize(g, ThemeSizeType.True);
        }

        public static Size GetThumbGripSize(Graphics g, ScrollBarState state)
        {
            InitializeRenderer(VisualStyleElement.ScrollBar.GripperHorizontal.Normal, (int) state);
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

