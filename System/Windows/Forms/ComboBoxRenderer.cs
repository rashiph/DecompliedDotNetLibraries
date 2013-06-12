namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class ComboBoxRenderer
    {
        private static readonly VisualStyleElement ComboBoxElement = VisualStyleElement.ComboBox.DropDownButton.Normal;
        private static readonly VisualStyleElement TextBoxElement = VisualStyleElement.TextBox.TextEdit.Normal;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private ComboBoxRenderer()
        {
        }

        private static void DrawBackground(Graphics g, Rectangle bounds, ComboBoxState state)
        {
            visualStyleRenderer.DrawBackground(g, bounds);
            if ((state != ComboBoxState.Disabled) && (visualStyleRenderer.GetColor(ColorProperty.FillColor) != SystemColors.Window))
            {
                Rectangle backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
                backgroundContentRectangle.Inflate(-2, -2);
                g.FillRectangle(SystemBrushes.Window, backgroundContentRectangle);
            }
        }

        public static void DrawDropDownButton(Graphics g, Rectangle bounds, ComboBoxState state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(ComboBoxElement.ClassName, ComboBoxElement.Part, (int) state);
            }
            else
            {
                visualStyleRenderer.SetParameters(ComboBoxElement.ClassName, ComboBoxElement.Part, (int) state);
            }
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, ComboBoxState state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            else
            {
                visualStyleRenderer.SetParameters(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            DrawBackground(g, bounds, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string comboBoxText, Font font, ComboBoxState state)
        {
            DrawTextBox(g, bounds, comboBoxText, font, TextFormatFlags.TextBoxControl, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string comboBoxText, Font font, Rectangle textBounds, ComboBoxState state)
        {
            DrawTextBox(g, bounds, comboBoxText, font, textBounds, TextFormatFlags.TextBoxControl, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string comboBoxText, Font font, TextFormatFlags flags, ComboBoxState state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            else
            {
                visualStyleRenderer.SetParameters(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            Rectangle backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
            backgroundContentRectangle.Inflate(-2, -2);
            DrawTextBox(g, bounds, comboBoxText, font, backgroundContentRectangle, flags, state);
        }

        public static void DrawTextBox(Graphics g, Rectangle bounds, string comboBoxText, Font font, Rectangle textBounds, TextFormatFlags flags, ComboBoxState state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            else
            {
                visualStyleRenderer.SetParameters(TextBoxElement.ClassName, TextBoxElement.Part, (int) state);
            }
            DrawBackground(g, bounds, state);
            Color foreColor = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            TextRenderer.DrawText(g, comboBoxText, font, textBounds, foreColor, flags);
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

