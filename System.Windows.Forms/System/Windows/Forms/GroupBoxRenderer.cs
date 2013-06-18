namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.VisualStyles;

    public sealed class GroupBoxRenderer
    {
        private const int boxHeaderWidth = 7;
        private static readonly VisualStyleElement GroupBoxElement = VisualStyleElement.Button.GroupBox.Normal;
        private static bool renderMatchingApplicationState = true;
        private const int textOffset = 8;
        [ThreadStatic]
        private static VisualStyleRenderer visualStyleRenderer = null;

        private GroupBoxRenderer()
        {
        }

        private static Color DefaultTextColor(GroupBoxState state)
        {
            if (RenderWithVisualStyles)
            {
                InitializeRenderer((int) state);
                return visualStyleRenderer.GetColor(ColorProperty.TextColor);
            }
            return SystemColors.ControlText;
        }

        public static void DrawGroupBox(Graphics g, Rectangle bounds, GroupBoxState state)
        {
            if (RenderWithVisualStyles)
            {
                DrawThemedGroupBoxNoText(g, bounds, state);
            }
            else
            {
                DrawUnthemedGroupBoxNoText(g, bounds, state);
            }
        }

        public static void DrawGroupBox(Graphics g, Rectangle bounds, string groupBoxText, Font font, GroupBoxState state)
        {
            DrawGroupBox(g, bounds, groupBoxText, font, TextFormatFlags.Default, state);
        }

        public static void DrawGroupBox(Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, GroupBoxState state)
        {
            DrawGroupBox(g, bounds, groupBoxText, font, textColor, TextFormatFlags.Default, state);
        }

        public static void DrawGroupBox(Graphics g, Rectangle bounds, string groupBoxText, Font font, TextFormatFlags flags, GroupBoxState state)
        {
            if (RenderWithVisualStyles)
            {
                DrawThemedGroupBoxWithText(g, bounds, groupBoxText, font, DefaultTextColor(state), flags, state);
            }
            else
            {
                DrawUnthemedGroupBoxWithText(g, bounds, groupBoxText, font, DefaultTextColor(state), flags, state);
            }
        }

        public static void DrawGroupBox(Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, TextFormatFlags flags, GroupBoxState state)
        {
            if (RenderWithVisualStyles)
            {
                DrawThemedGroupBoxWithText(g, bounds, groupBoxText, font, textColor, flags, state);
            }
            else
            {
                DrawUnthemedGroupBoxWithText(g, bounds, groupBoxText, font, textColor, flags, state);
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

        private static void DrawThemedGroupBoxNoText(Graphics g, Rectangle bounds, GroupBoxState state)
        {
            InitializeRenderer((int) state);
            visualStyleRenderer.DrawBackground(g, bounds);
        }

        private static void DrawThemedGroupBoxWithText(Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, TextFormatFlags flags, GroupBoxState state)
        {
            InitializeRenderer((int) state);
            Rectangle rectangle = bounds;
            rectangle.Width -= 14;
            Size size = TextRenderer.MeasureText(g, groupBoxText, font, new Size(rectangle.Width, rectangle.Height), flags);
            rectangle.Width = size.Width;
            rectangle.Height = size.Height;
            if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                rectangle.X = ((bounds.Right - rectangle.Width) - 7) + 1;
            }
            else
            {
                rectangle.X += 6;
            }
            TextRenderer.DrawText(g, groupBoxText, font, rectangle, textColor, flags);
            Rectangle rectangle2 = bounds;
            rectangle2.Y += font.Height / 2;
            rectangle2.Height -= font.Height / 2;
            Rectangle clipRectangle = rectangle2;
            Rectangle rectangle4 = rectangle2;
            Rectangle rectangle5 = rectangle2;
            clipRectangle.Width = 7;
            rectangle4.Width = Math.Max(0, rectangle.Width - 3);
            if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                clipRectangle.X = rectangle2.Right - 7;
                rectangle4.X = clipRectangle.Left - rectangle4.Width;
                rectangle5.Width = rectangle4.X - rectangle2.X;
            }
            else
            {
                rectangle4.X = clipRectangle.Right;
                rectangle5.X = rectangle4.Right;
                rectangle5.Width = rectangle2.Right - rectangle5.X;
            }
            rectangle4.Y = rectangle.Bottom;
            rectangle4.Height -= rectangle.Bottom - rectangle2.Top;
            visualStyleRenderer.DrawBackground(g, rectangle2, clipRectangle);
            visualStyleRenderer.DrawBackground(g, rectangle2, rectangle4);
            visualStyleRenderer.DrawBackground(g, rectangle2, rectangle5);
        }

        private static void DrawUnthemedGroupBoxNoText(Graphics g, Rectangle bounds, GroupBoxState state)
        {
            Color control = SystemColors.Control;
            Pen pen = new Pen(ControlPaint.Light(control, 1f));
            Pen pen2 = new Pen(ControlPaint.Dark(control, 0f));
            try
            {
                g.DrawLine(pen, (int) (bounds.Left + 1), (int) (bounds.Top + 1), (int) (bounds.Left + 1), (int) (bounds.Height - 1));
                g.DrawLine(pen2, bounds.Left, bounds.Top + 1, bounds.Left, bounds.Height - 2);
                g.DrawLine(pen, bounds.Left, bounds.Height - 1, bounds.Width - 1, bounds.Height - 1);
                g.DrawLine(pen2, bounds.Left, bounds.Height - 2, bounds.Width - 1, bounds.Height - 2);
                g.DrawLine(pen, (int) (bounds.Left + 1), (int) (bounds.Top + 1), (int) (bounds.Width - 1), (int) (bounds.Top + 1));
                g.DrawLine(pen2, bounds.Left, bounds.Top, bounds.Width - 2, bounds.Top);
                g.DrawLine(pen, bounds.Width - 1, bounds.Top, bounds.Width - 1, bounds.Height - 1);
                g.DrawLine(pen2, bounds.Width - 2, bounds.Top, bounds.Width - 2, bounds.Height - 2);
            }
            finally
            {
                if (pen != null)
                {
                    pen.Dispose();
                }
                if (pen2 != null)
                {
                    pen2.Dispose();
                }
            }
        }

        private static void DrawUnthemedGroupBoxWithText(Graphics g, Rectangle bounds, string groupBoxText, Font font, Color textColor, TextFormatFlags flags, GroupBoxState state)
        {
            Rectangle rectangle = bounds;
            rectangle.Width -= 8;
            Size size = TextRenderer.MeasureText(g, groupBoxText, font, new Size(rectangle.Width, rectangle.Height), flags);
            rectangle.Width = size.Width;
            rectangle.Height = size.Height;
            if ((flags & TextFormatFlags.Right) == TextFormatFlags.Right)
            {
                rectangle.X = (bounds.Right - rectangle.Width) - 8;
            }
            else
            {
                rectangle.X += 8;
            }
            TextRenderer.DrawText(g, groupBoxText, font, rectangle, textColor, flags);
            if (rectangle.Width > 0)
            {
                rectangle.Inflate(2, 0);
            }
            Pen pen = new Pen(SystemColors.ControlLight);
            Pen pen2 = new Pen(SystemColors.ControlDark);
            int num = bounds.Top + (font.Height / 2);
            g.DrawLine(pen, bounds.Left + 1, num, bounds.Left + 1, bounds.Height - 1);
            g.DrawLine(pen2, bounds.Left, num - 1, bounds.Left, bounds.Height - 2);
            g.DrawLine(pen, bounds.Left, bounds.Height - 1, bounds.Width, bounds.Height - 1);
            g.DrawLine(pen2, bounds.Left, bounds.Height - 2, bounds.Width - 1, bounds.Height - 2);
            g.DrawLine(pen, bounds.Left + 1, num, rectangle.X - 2, num);
            g.DrawLine(pen2, bounds.Left, num - 1, rectangle.X - 3, num - 1);
            g.DrawLine(pen, (rectangle.X + rectangle.Width) + 1, num, bounds.Width - 1, num);
            g.DrawLine(pen2, (int) ((rectangle.X + rectangle.Width) + 2), (int) (num - 1), (int) (bounds.Width - 2), (int) (num - 1));
            g.DrawLine(pen, bounds.Width - 1, num, bounds.Width - 1, bounds.Height - 1);
            g.DrawLine(pen2, (int) (bounds.Width - 2), (int) (num - 1), (int) (bounds.Width - 2), (int) (bounds.Height - 2));
            pen.Dispose();
            pen2.Dispose();
        }

        private static void InitializeRenderer(int state)
        {
            if (visualStyleRenderer == null)
            {
                visualStyleRenderer = new VisualStyleRenderer(GroupBoxElement.ClassName, GroupBoxElement.Part, state);
            }
            else
            {
                visualStyleRenderer.SetParameters(GroupBoxElement.ClassName, GroupBoxElement.Part, state);
            }
        }

        public static bool IsBackgroundPartiallyTransparent(GroupBoxState state)
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

