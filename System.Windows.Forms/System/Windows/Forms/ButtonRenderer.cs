namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class ButtonRenderer
    {
        private static readonly VisualStyleElement ButtonElement = VisualStyleElement.Button.PushButton.Normal;
        private static bool renderMatchingApplicationState = true;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private ButtonRenderer()
        {
        }

        internal static ButtonState ConvertToButtonState(PushButtonState state)
        {
            switch (state)
            {
                case PushButtonState.Pressed:
                    return ButtonState.Pushed;

                case PushButtonState.Disabled:
                    return ButtonState.Inactive;
            }
            return ButtonState.Normal;
        }

        public static void DrawButton(Graphics g, Rectangle bounds, PushButtonState state)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
            }
            else
            {
                ControlPaint.DrawButton(g, bounds, ConvertToButtonState(state));
            }
        }

        public static void DrawButton(Graphics g, Rectangle bounds, bool focused, PushButtonState state)
        {
            Rectangle backgroundContentRectangle;
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
                backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
            }
            else
            {
                ControlPaint.DrawButton(g, bounds, ConvertToButtonState(state));
                backgroundContentRectangle = Rectangle.Inflate(bounds, -3, -3);
            }
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, backgroundContentRectangle);
            }
        }

        public static void DrawButton(Graphics g, Rectangle bounds, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
        {
            Rectangle backgroundContentRectangle;
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
                visualStyleRenderer.DrawImage(g, imageBounds, image);
                backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
            }
            else
            {
                ControlPaint.DrawButton(g, bounds, ConvertToButtonState(state));
                g.DrawImage(image, imageBounds);
                backgroundContentRectangle = Rectangle.Inflate(bounds, -3, -3);
            }
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, backgroundContentRectangle);
            }
        }

        public static void DrawButton(Graphics g, Rectangle bounds, string buttonText, Font font, bool focused, PushButtonState state)
        {
            DrawButton(g, bounds, buttonText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, focused, state);
        }

        public static void DrawButton(Graphics g, Rectangle bounds, string buttonText, Font font, TextFormatFlags flags, bool focused, PushButtonState state)
        {
            Rectangle backgroundContentRectangle;
            Color controlText;
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
                backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
                controlText = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            }
            else
            {
                ControlPaint.DrawButton(g, bounds, ConvertToButtonState(state));
                backgroundContentRectangle = Rectangle.Inflate(bounds, -3, -3);
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, buttonText, font, backgroundContentRectangle, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, backgroundContentRectangle);
            }
        }

        public static void DrawButton(Graphics g, Rectangle bounds, string buttonText, Font font, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
        {
            DrawButton(g, bounds, buttonText, font, TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter, image, imageBounds, focused, state);
        }

        public static void DrawButton(Graphics g, Rectangle bounds, string buttonText, Font font, TextFormatFlags flags, Image image, Rectangle imageBounds, bool focused, PushButtonState state)
        {
            Rectangle backgroundContentRectangle;
            Color controlText;
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                visualStyleRenderer.DrawBackground(g, bounds);
                visualStyleRenderer.DrawImage(g, imageBounds, image);
                backgroundContentRectangle = visualStyleRenderer.GetBackgroundContentRectangle(g, bounds);
                controlText = visualStyleRenderer.GetColor(ColorProperty.TextColor);
            }
            else
            {
                ControlPaint.DrawButton(g, bounds, ConvertToButtonState(state));
                g.DrawImage(image, imageBounds);
                backgroundContentRectangle = Rectangle.Inflate(bounds, -3, -3);
                controlText = SystemColors.ControlText;
            }
            TextRenderer.DrawText(g, buttonText, font, backgroundContentRectangle, controlText, flags);
            if (focused)
            {
                ControlPaint.DrawFocusRectangle(g, backgroundContentRectangle);
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

        private static void InitializeRenderer(int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(ButtonElement.ClassName, ButtonElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(ButtonElement.ClassName, ButtonElement.Part, state);
            }
        }

        public static bool IsBackgroundPartiallyTransparent(PushButtonState state)
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

