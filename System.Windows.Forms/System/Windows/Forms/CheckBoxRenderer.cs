namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class CheckBoxRenderer
    {
        private static readonly VisualStyleElement CheckBoxElement = VisualStyleElement.Button.CheckBox.UncheckedNormal;
        private static bool renderMatchingApplicationState = true;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private CheckBoxRenderer()
        {
        }

        internal static CheckBoxState ConvertFromButtonState(ButtonState state, bool isMixed, bool isHot)
        {
            if (isMixed)
            {
                if ((state & ButtonState.Pushed) == ButtonState.Pushed)
                {
                    return CheckBoxState.MixedPressed;
                }
                if ((state & ButtonState.Inactive) == ButtonState.Inactive)
                {
                    return CheckBoxState.MixedDisabled;
                }
                if (isHot)
                {
                    return CheckBoxState.MixedHot;
                }
                return CheckBoxState.MixedNormal;
            }
            if ((state & ButtonState.Checked) == ButtonState.Checked)
            {
                if ((state & ButtonState.Pushed) == ButtonState.Pushed)
                {
                    return CheckBoxState.CheckedPressed;
                }
                if ((state & ButtonState.Inactive) == ButtonState.Inactive)
                {
                    return CheckBoxState.CheckedDisabled;
                }
                if (isHot)
                {
                    return CheckBoxState.CheckedHot;
                }
                return CheckBoxState.CheckedNormal;
            }
            if ((state & ButtonState.Pushed) == ButtonState.Pushed)
            {
                return CheckBoxState.UncheckedPressed;
            }
            if ((state & ButtonState.Inactive) == ButtonState.Inactive)
            {
                return CheckBoxState.UncheckedDisabled;
            }
            if (isHot)
            {
                return CheckBoxState.UncheckedHot;
            }
            return CheckBoxState.UncheckedNormal;
        }

        internal static ButtonState ConvertToButtonState(CheckBoxState state)
        {
            switch (state)
            {
                case CheckBoxState.UncheckedPressed:
                    return ButtonState.Pushed;

                case CheckBoxState.UncheckedDisabled:
                    return ButtonState.Inactive;

                case CheckBoxState.CheckedNormal:
                case CheckBoxState.CheckedHot:
                    return ButtonState.Checked;

                case CheckBoxState.CheckedPressed:
                    return (ButtonState.Checked | ButtonState.Pushed);

                case CheckBoxState.CheckedDisabled:
                    return (ButtonState.Checked | ButtonState.Inactive);

                case CheckBoxState.MixedNormal:
                case CheckBoxState.MixedHot:
                    return ButtonState.Checked;

                case CheckBoxState.MixedPressed:
                    return (ButtonState.Checked | ButtonState.Pushed);

                case CheckBoxState.MixedDisabled:
                    return (ButtonState.Checked | ButtonState.Inactive);
            }
            return ButtonState.Normal;
        }

        public static void DrawCheckBox(Graphics g, Point glyphLocation, CheckBoxState state)
        {
            Rectangle bounds = new Rectangle(glyphLocation, GetGlyphSize(g, state));
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
            }
            else if (IsMixed(state))
            {
                ControlPaint.DrawMixedCheckBox(g, bounds, ConvertToButtonState(state));
            }
            else
            {
                ControlPaint.DrawCheckBox(g, bounds, ConvertToButtonState(state));
            }
        }

        public static void DrawCheckBox(Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, bool focused, CheckBoxState state)
        {
            DrawCheckBox(g, glyphLocation, textBounds, checkBoxText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, focused, state);
        }

        public static void DrawCheckBox(Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, TextFormatFlags flags, bool focused, CheckBoxState state)
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
                if (IsMixed(state))
                {
                    ControlPaint.DrawMixedCheckBox(g, bounds, ConvertToButtonState(state));
                }
                else
                {
                    ControlPaint.DrawCheckBox(g, bounds, ConvertToButtonState(state));
                }
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, checkBoxText, font, textBounds, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, textBounds);
            }
        }

        public static void DrawCheckBox(Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, Image image, Rectangle imageBounds, bool focused, CheckBoxState state)
        {
            DrawCheckBox(g, glyphLocation, textBounds, checkBoxText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
        }

        public static void DrawCheckBox(Graphics g, Point glyphLocation, Rectangle textBounds, string checkBoxText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, CheckBoxState state)
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
                if (IsMixed(state))
                {
                    ControlPaint.DrawMixedCheckBox(g, bounds, ConvertToButtonState(state));
                }
                else
                {
                    ControlPaint.DrawCheckBox(g, bounds, ConvertToButtonState(state));
                }
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, checkBoxText, font, textBounds, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, textBounds);
            }
        }

        public static void DrawParentBackground(Graphics g, Rectangle bounds, Control childControl)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer(0);
                visualStyleRenderer.DrawParentBackground(g, bounds, childControl);
            }
        }

        public static Size GetGlyphSize(Graphics g, CheckBoxState state)
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
                visualStyleRenderer = new VisualStyleRenderer(CheckBoxElement.ClassName, CheckBoxElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(CheckBoxElement.ClassName, CheckBoxElement.Part, state);
            }
        }

        public static bool IsBackgroundPartiallyTransparent(CheckBoxState state)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                return visualStyleRenderer.IsBackgroundPartiallyTransparent();
            }
            return false;
        }

        private static bool IsMixed(CheckBoxState state)
        {
            switch (state)
            {
                case CheckBoxState.MixedNormal:
                case CheckBoxState.MixedHot:
                case CheckBoxState.MixedPressed:
                case CheckBoxState.MixedDisabled:
                    return true;
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

