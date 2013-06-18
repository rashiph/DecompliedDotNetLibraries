namespace System.Drawing
{
    using System;

    public sealed class SystemBrushes
    {
        private static readonly object SystemBrushesKey = new object();

        private SystemBrushes()
        {
        }

        public static Brush FromSystemColor(Color c)
        {
            if (!c.IsSystemColor)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("ColorNotSystemColor", new object[] { c.ToString() }));
            }
            Brush[] brushArray = (Brush[]) SafeNativeMethods.Gdip.ThreadData[SystemBrushesKey];
            if (brushArray == null)
            {
                brushArray = new Brush[0x21];
                SafeNativeMethods.Gdip.ThreadData[SystemBrushesKey] = brushArray;
            }
            int index = (int) c.ToKnownColor();
            if (index > 0xa7)
            {
                index -= 0x8d;
            }
            index--;
            if (brushArray[index] == null)
            {
                brushArray[index] = new SolidBrush(c, true);
            }
            return brushArray[index];
        }

        public static Brush ActiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveBorder);
            }
        }

        public static Brush ActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaption);
            }
        }

        public static Brush ActiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaptionText);
            }
        }

        public static Brush AppWorkspace
        {
            get
            {
                return FromSystemColor(SystemColors.AppWorkspace);
            }
        }

        public static Brush ButtonFace
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonFace);
            }
        }

        public static Brush ButtonHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonHighlight);
            }
        }

        public static Brush ButtonShadow
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonShadow);
            }
        }

        public static Brush Control
        {
            get
            {
                return FromSystemColor(SystemColors.Control);
            }
        }

        public static Brush ControlDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDark);
            }
        }

        public static Brush ControlDarkDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDarkDark);
            }
        }

        public static Brush ControlLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLight);
            }
        }

        public static Brush ControlLightLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLightLight);
            }
        }

        public static Brush ControlText
        {
            get
            {
                return FromSystemColor(SystemColors.ControlText);
            }
        }

        public static Brush Desktop
        {
            get
            {
                return FromSystemColor(SystemColors.Desktop);
            }
        }

        public static Brush GradientActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientActiveCaption);
            }
        }

        public static Brush GradientInactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientInactiveCaption);
            }
        }

        public static Brush GrayText
        {
            get
            {
                return FromSystemColor(SystemColors.GrayText);
            }
        }

        public static Brush Highlight
        {
            get
            {
                return FromSystemColor(SystemColors.Highlight);
            }
        }

        public static Brush HighlightText
        {
            get
            {
                return FromSystemColor(SystemColors.HighlightText);
            }
        }

        public static Brush HotTrack
        {
            get
            {
                return FromSystemColor(SystemColors.HotTrack);
            }
        }

        public static Brush InactiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveBorder);
            }
        }

        public static Brush InactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaption);
            }
        }

        public static Brush InactiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaptionText);
            }
        }

        public static Brush Info
        {
            get
            {
                return FromSystemColor(SystemColors.Info);
            }
        }

        public static Brush InfoText
        {
            get
            {
                return FromSystemColor(SystemColors.InfoText);
            }
        }

        public static Brush Menu
        {
            get
            {
                return FromSystemColor(SystemColors.Menu);
            }
        }

        public static Brush MenuBar
        {
            get
            {
                return FromSystemColor(SystemColors.MenuBar);
            }
        }

        public static Brush MenuHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.MenuHighlight);
            }
        }

        public static Brush MenuText
        {
            get
            {
                return FromSystemColor(SystemColors.MenuText);
            }
        }

        public static Brush ScrollBar
        {
            get
            {
                return FromSystemColor(SystemColors.ScrollBar);
            }
        }

        public static Brush Window
        {
            get
            {
                return FromSystemColor(SystemColors.Window);
            }
        }

        public static Brush WindowFrame
        {
            get
            {
                return FromSystemColor(SystemColors.WindowFrame);
            }
        }

        public static Brush WindowText
        {
            get
            {
                return FromSystemColor(SystemColors.WindowText);
            }
        }
    }
}

