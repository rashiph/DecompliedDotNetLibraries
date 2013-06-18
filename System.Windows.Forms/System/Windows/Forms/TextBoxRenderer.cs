namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class TextBoxRenderer
    {
        private static readonly VisualStyleElement TextBoxElement = VisualStyleElement.TextBox.TextEdit.Normal;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private TextBoxRenderer()
        {
        }

        private static void DrawBackground(Graphics g, Rectangle bounds, TextBoxState state)
        {
            visualStyleRenderer.DrawBackground(g, bounds);
            if ((state != TextBoxState.Disabled) && (visualStyleRenderer.GetColor(ColorProperty.FillColor) != SystemColors.Window))
            {
                Rectangle backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
                using (SolidBrush brush = new SolidBrush(SystemColors.Window))
                {
                    g.FillRectangle(brush, backgroundContentRectangle);
                }
            }
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, TextBoxState state)
        {
            InitializeRenderer((int) state);
            DrawBackground(g, bounds, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string textBoxText, Font font, TextBoxState state)
        {
            DrawTextBox(g, bounds, textBoxText, font, TextFormatFlags.TextBoxControl, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string textBoxText, Font font, Rectangle textBounds, TextBoxState state)
        {
            DrawTextBox(g, bounds, textBoxText, font, textBounds, TextFormatFlags.TextBoxControl, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string textBoxText, Font font, TextFormatFlags flags, TextBoxState state)
        {
            InitializeRenderer((int) state);
            Rectangle backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
            backgroundContentRectangle.Inflate(-2, -2);
            DrawTextBox(g, bounds, textBoxText, font, backgroundContentRectangle, flags, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string textBoxText, Font font, Rectangle textBounds, TextFormatFlags flags, TextBoxState state)
        {
            InitializeRenderer((int) state);
            DrawBackground(g, bounds, state);
            Color foreColor = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            TextRenderer.DrawText(g, textBoxText, font, textBounds, foreColor, flags);
        }

        private static void InitializeRenderer(int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(TextBoxElement.ClassName, TextBoxElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(TextBoxElement.ClassName, TextBoxElement.Part, state);
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

