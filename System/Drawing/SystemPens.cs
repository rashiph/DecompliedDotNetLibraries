namespace System.Drawing
{
    using System;

    public sealed class SystemPens
    {
        private static readonly object SystemPensKey = new object();

        private SystemPens()
        {
        }

        public static Pen FromSystemColor(Color c)
        {
            if (!c.IsSystemColor)
            {
                throw new ArgumentException(System.Drawing.SR.GetString("ColorNotSystemColor", new object[] { c.ToString() }));
            }
            Pen[] penArray = (Pen[]) SafeNativeMethods.Gdip.ThreadData[SystemPensKey];
            if (penArray == null)
            {
                penArray = new Pen[0x21];
                SafeNativeMethods.Gdip.ThreadData[SystemPensKey] = penArray;
            }
            int index = (int) c.ToKnownColor();
            if (index > 0xa7)
            {
                index -= 0x8d;
            }
            index--;
            if (penArray[index] == null)
            {
                penArray[index] = new Pen(c, true);
            }
            return penArray[index];
        }

        public static Pen ActiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveBorder);
            }
        }

        public static Pen ActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaption);
            }
        }

        public static Pen ActiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.ActiveCaptionText);
            }
        }

        public static Pen AppWorkspace
        {
            get
            {
                return FromSystemColor(SystemColors.AppWorkspace);
            }
        }

        public static Pen ButtonFace
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonFace);
            }
        }

        public static Pen ButtonHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonHighlight);
            }
        }

        public static Pen ButtonShadow
        {
            get
            {
                return FromSystemColor(SystemColors.ButtonShadow);
            }
        }

        public static Pen Control
        {
            get
            {
                return FromSystemColor(SystemColors.Control);
            }
        }

        public static Pen ControlDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDark);
            }
        }

        public static Pen ControlDarkDark
        {
            get
            {
                return FromSystemColor(SystemColors.ControlDarkDark);
            }
        }

        public static Pen ControlLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLight);
            }
        }

        public static Pen ControlLightLight
        {
            get
            {
                return FromSystemColor(SystemColors.ControlLightLight);
            }
        }

        public static Pen ControlText
        {
            get
            {
                return FromSystemColor(SystemColors.ControlText);
            }
        }

        public static Pen Desktop
        {
            get
            {
                return FromSystemColor(SystemColors.Desktop);
            }
        }

        public static Pen GradientActiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientActiveCaption);
            }
        }

        public static Pen GradientInactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.GradientInactiveCaption);
            }
        }

        public static Pen GrayText
        {
            get
            {
                return FromSystemColor(SystemColors.GrayText);
            }
        }

        public static Pen Highlight
        {
            get
            {
                return FromSystemColor(SystemColors.Highlight);
            }
        }

        public static Pen HighlightText
        {
            get
            {
                return FromSystemColor(SystemColors.HighlightText);
            }
        }

        public static Pen HotTrack
        {
            get
            {
                return FromSystemColor(SystemColors.HotTrack);
            }
        }

        public static Pen InactiveBorder
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveBorder);
            }
        }

        public static Pen InactiveCaption
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaption);
            }
        }

        public static Pen InactiveCaptionText
        {
            get
            {
                return FromSystemColor(SystemColors.InactiveCaptionText);
            }
        }

        public static Pen Info
        {
            get
            {
                return FromSystemColor(SystemColors.Info);
            }
        }

        public static Pen InfoText
        {
            get
            {
                return FromSystemColor(SystemColors.InfoText);
            }
        }

        public static Pen Menu
        {
            get
            {
                return FromSystemColor(SystemColors.Menu);
            }
        }

        public static Pen MenuBar
        {
            get
            {
                return FromSystemColor(SystemColors.MenuBar);
            }
        }

        public static Pen MenuHighlight
        {
            get
            {
                return FromSystemColor(SystemColors.MenuHighlight);
            }
        }

        public static Pen MenuText
        {
            get
            {
                return FromSystemColor(SystemColors.MenuText);
            }
        }

        public static Pen ScrollBar
        {
            get
            {
                return FromSystemColor(SystemColors.ScrollBar);
            }
        }

        public static Pen Window
        {
            get
            {
                return FromSystemColor(SystemColors.Window);
            }
        }

        public static Pen WindowFrame
        {
            get
            {
                return FromSystemColor(SystemColors.WindowFrame);
            }
        }

        public static Pen WindowText
        {
            get
            {
                return FromSystemColor(SystemColors.WindowText);
            }
        }
    }
}

