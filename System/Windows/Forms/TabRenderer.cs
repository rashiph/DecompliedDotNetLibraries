namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class TabRenderer
    {
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer;

        private TabRenderer()
        {
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, TabItemState state)
        {
            InitializeRenderer(VisualStyleElement.Tab.TabItem.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, bool focused, TabItemState state)
        {
            InitializeRenderer(VisualStyleElement.Tab.TabItem.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
            Rectangle rectangle = Rectangle.Inflate(bounds, -3, -3);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, string tabItemText, Font font, TabItemState state)
        {
            DrawTabItem(g, bounds, tabItemText, font, false, state);
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
        {
            InitializeRenderer(VisualStyleElement.Tab.TabItem.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
            Rectangle rectangle = Rectangle.Inflate(bounds, -3, -3);
            visualStyleRenderer.DrawImage(g, imageRectangle, image);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, string tabItemText, Font font, bool focused, TabItemState state)
        {
            DrawTabItem(g, bounds, tabItemText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, focused, state);
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, string tabItemText, Font font, TextFormatFlags flags, bool focused, TabItemState state)
        {
            InitializeRenderer(VisualStyleElement.Tab.TabItem.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
            Rectangle rectangle = Rectangle.Inflate(bounds, -3, -3);
            Color foreColor = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            TextRenderer.DrawText(g, tabItemText, font, rectangle, foreColor, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, string tabItemText, Font font, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
        {
            DrawTabItem(g, bounds, tabItemText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, image, imageRectangle, focused, state);
        }

        public static void DrawTabItem(Graphics g, Rectangle bounds, string tabItemText, Font font, TextFormatFlags flags, Image image, Rectangle imageRectangle, bool focused, TabItemState state)
        {
            InitializeRenderer(VisualStyleElement.Tab.TabItem.Normal, (int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
            Rectangle rectangle = Rectangle.Inflate(bounds, -3, -3);
            visualStyleRenderer.DrawImage(g, imageRectangle, image);
            Color foreColor = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            TextRenderer.DrawText(g, tabItemText, font, rectangle, foreColor, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, rectangle);
            }
        }

        public static void DrawTabPage(Graphics g, Rectangle bounds)
        {
            InitializeRenderer(VisualStyleElement.Tab.Pane.Normal, 0);
            visualStyleRenderer.DrawBackground(g, bounds);
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

