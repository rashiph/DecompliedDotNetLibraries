namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class RadioButtonRenderer
    {
        private static readonly VisualStyleElement RadioElement = VisualStyleElement.Button.RadioButton.UncheckedNormal;
        private static bool renderMatchingApplicationState = true;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private RadioButtonRenderer()
        {
        }

        internal static RadioButtonState ConvertFromButtonState(ButtonState state, bool isHot)
        {
            if ((state & ButtonState.Checked) == ButtonState.Checked)
            {
                if ((state & ButtonState.Pushed) == ButtonState.Pushed)
                {
                    return RadioButtonState.CheckedPressed;
                }
                if ((state & ButtonState.Inactive) == ButtonState.Inactive)
                {
                    return RadioButtonState.CheckedDisabled;
                }
                if (isHot)
                {
                    return RadioButtonState.CheckedHot;
                }
                return RadioButtonState.CheckedNormal;
            }
            if ((state & ButtonState.Pushed) == ButtonState.Pushed)
            {
                return RadioButtonState.UncheckedPressed;
            }
            if ((state & ButtonState.Inactive) == ButtonState.Inactive)
            {
                return RadioButtonState.UncheckedDisabled;
            }
            if (isHot)
            {
                return RadioButtonState.UncheckedHot;
            }
            return RadioButtonState.UncheckedNormal;
        }

        internal static ButtonState ConvertToButtonState(RadioButtonState state)
        {
            switch (state)
            {
                case RadioButtonState.UncheckedPressed:
                    return ButtonState.Pushed;

                case RadioButtonState.UncheckedDisabled:
                    return ButtonState.Inactive;

                case RadioButtonState.CheckedNormal:
                case RadioButtonState.CheckedHot:
                    return ButtonState.Checked;

                case RadioButtonState.CheckedPressed:
                    return (ButtonState.Checked | ButtonState.Pushed);

                case RadioButtonState.CheckedDisabled:
                    return (ButtonState.Checked | ButtonState.Inactive);
            }
            return ButtonState.Normal;
        }

        public static void DrawParentBackground(Graphics g, Rectangle bounds, Control childControl)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer(0);
                visualStyleRenderer.DrawParentBackground(g, bounds, childControl);
            }
        }

        public static void DrawRadioButton(Graphics g, Point glyphLocation, RadioButtonState state)
        {
            Rectangle bounds = new Rectangle(glyphLocation, GetGlyphSize(g, state));
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
            }
            else
            {
                ControlPaint.DrawRadioButton(g, bounds, ConvertToButtonState(state));
            }
        }

        public static void DrawRadioButton(Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, bool focused, RadioButtonState state)
        {
            DrawRadioButton(g, glyphLocation, textBounds, radioButtonText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, focused, state);
        }

        public static void DrawRadioButton(Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, TextFormatFlags flags, bool focused, RadioButtonState state)
        {
            Color controlText;
            Rectangle bounds = new Rectangle(glyphLocation, GetGlyphSize(g, state));
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
                controlText = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            }
            else
            {
                ControlPaint.DrawRadioButton(g, bounds, ConvertToButtonState(state));
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, radioButtonText, font, textBounds, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, textBounds);
            }
        }

        public static void DrawRadioButton(Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, Image image, Rectangle imageBounds, bool focused, RadioButtonState state)
        {
            DrawRadioButton(g, glyphLocation, textBounds, radioButtonText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
        }

        public static void DrawRadioButton(Graphics g, Point glyphLocation, Rectangle textBounds, string radioButtonText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, RadioButtonState state)
        {
            Color controlText;
            Rectangle bounds = new Rectangle(glyphLocation, GetGlyphSize(g, state));
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawImage(g, imageBounds, image);
                visualStyleRenderer.DrawBackground(g, bounds);
                controlText = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            }
            else
            {
                g.DrawImage(image, imageBounds);
                ControlPaint.DrawRadioButton(g, bounds, ConvertToButtonState(state));
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, radioButtonText, font, textBounds, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, textBounds);
            }
        }

        public static Size GetGlyphSize(Graphics g, RadioButtonState state)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                return visualStyleRenderer.GetPartSize(g, ThemeSizeType.Draw);
            }
            return new Size(13, 13);
        }

        private static void InitializeRenderer(int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(RadioElement.ClassName, RadioElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(RadioElement.ClassName, RadioElement.Part, state);
            }
        }

        public static bool IsBackgroundPartiallyTransparent(RadioButtonState state)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                return visualStyleRenderer.IsBackgroundPartiallyTransparent();
            }
            return false;
        }

        public static bool RenderMatchingApplicationState
        {
            get
            {
                return renderMatchingApplicationState;
            }
            set
            {
                renderMatchingApplicationState = value;
            }
        }

        private static bool RenderWithVisualStyles
        {
            get
            {
                if (renderMatchingApplicationState)
                {
                    return Application.RenderWithVisualStyles;
                }
                return true;
            }
        }
    }
}

