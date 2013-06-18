namespace System.Drawing
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable, StructLayout(LayoutKind.Sequential), TypeConverter(typeof(ColorConverter)), DebuggerDisplay("{NameAndARGBValue}"), Editor("System.Drawing.Design.ColorEditor, System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public struct Color
    {
        private const int ARGBAlphaShift = 0x18;
        private const int ARGBRedShift = 0x10;
        private const int ARGBGreenShift = 8;
        private const int ARGBBlueShift = 0;
        public static readonly Color Empty;
        private static short StateKnownColorValid;
        private static short StateARGBValueValid;
        private static short StateValueMask;
        private static short StateNameValid;
        private static long NotDefinedValue;
        private readonly string name;
        private readonly long value;
        private readonly short knownColor;
        private readonly short state;
        public static Color Transparent
        {
            get
            {
                return new Color(KnownColor.Transparent);
            }
        }
        public static Color AliceBlue
        {
            get
            {
                return new Color(KnownColor.AliceBlue);
            }
        }
        public static Color AntiqueWhite
        {
            get
            {
                return new Color(KnownColor.AntiqueWhite);
            }
        }
        public static Color Aqua
        {
            get
            {
                return new Color(KnownColor.Aqua);
            }
        }
        public static Color Aquamarine
        {
            get
            {
                return new Color(KnownColor.Aquamarine);
            }
        }
        public static Color Azure
        {
            get
            {
                return new Color(KnownColor.Azure);
            }
        }
        public static Color Beige
        {
            get
            {
                return new Color(KnownColor.Beige);
            }
        }
        public static Color Bisque
        {
            get
            {
                return new Color(KnownColor.Bisque);
            }
        }
        public static Color Black
        {
            get
            {
                return new Color(KnownColor.Black);
            }
        }
        public static Color BlanchedAlmond
        {
            get
            {
                return new Color(KnownColor.BlanchedAlmond);
            }
        }
        public static Color Blue
        {
            get
            {
                return new Color(KnownColor.Blue);
            }
        }
        public static Color BlueViolet
        {
            get
            {
                return new Color(KnownColor.BlueViolet);
            }
        }
        public static Color Brown
        {
            get
            {
                return new Color(KnownColor.Brown);
            }
        }
        public static Color BurlyWood
        {
            get
            {
                return new Color(KnownColor.BurlyWood);
            }
        }
        public static Color CadetBlue
        {
            get
            {
                return new Color(KnownColor.CadetBlue);
            }
        }
        public static Color Chartreuse
        {
            get
            {
                return new Color(KnownColor.Chartreuse);
            }
        }
        public static Color Chocolate
        {
            get
            {
                return new Color(KnownColor.Chocolate);
            }
        }
        public static Color Coral
        {
            get
            {
                return new Color(KnownColor.Coral);
            }
        }
        public static Color CornflowerBlue
        {
            get
            {
                return new Color(KnownColor.CornflowerBlue);
            }
        }
        public static Color Cornsilk
        {
            get
            {
                return new Color(KnownColor.Cornsilk);
            }
        }
        public static Color Crimson
        {
            get
            {
                return new Color(KnownColor.Crimson);
            }
        }
        public static Color Cyan
        {
            get
            {
                return new Color(KnownColor.Cyan);
            }
        }
        public static Color DarkBlue
        {
            get
            {
                return new Color(KnownColor.DarkBlue);
            }
        }
        public static Color DarkCyan
        {
            get
            {
                return new Color(KnownColor.DarkCyan);
            }
        }
        public static Color DarkGoldenrod
        {
            get
            {
                return new Color(KnownColor.DarkGoldenrod);
            }
        }
        public static Color DarkGray
        {
            get
            {
                return new Color(KnownColor.DarkGray);
            }
        }
        public static Color DarkGreen
        {
            get
            {
                return new Color(KnownColor.DarkGreen);
            }
        }
        public static Color DarkKhaki
        {
            get
            {
                return new Color(KnownColor.DarkKhaki);
            }
        }
        public static Color DarkMagenta
        {
            get
            {
                return new Color(KnownColor.DarkMagenta);
            }
        }
        public static Color DarkOliveGreen
        {
            get
            {
                return new Color(KnownColor.DarkOliveGreen);
            }
        }
        public static Color DarkOrange
        {
            get
            {
                return new Color(KnownColor.DarkOrange);
            }
        }
        public static Color DarkOrchid
        {
            get
            {
                return new Color(KnownColor.DarkOrchid);
            }
        }
        public static Color DarkRed
        {
            get
            {
                return new Color(KnownColor.DarkRed);
            }
        }
        public static Color DarkSalmon
        {
            get
            {
                return new Color(KnownColor.DarkSalmon);
            }
        }
        public static Color DarkSeaGreen
        {
            get
            {
                return new Color(KnownColor.DarkSeaGreen);
            }
        }
        public static Color DarkSlateBlue
        {
            get
            {
                return new Color(KnownColor.DarkSlateBlue);
            }
        }
        public static Color DarkSlateGray
        {
            get
            {
                return new Color(KnownColor.DarkSlateGray);
            }
        }
        public static Color DarkTurquoise
        {
            get
            {
                return new Color(KnownColor.DarkTurquoise);
            }
        }
        public static Color DarkViolet
        {
            get
            {
                return new Color(KnownColor.DarkViolet);
            }
        }
        public static Color DeepPink
        {
            get
            {
                return new Color(KnownColor.DeepPink);
            }
        }
        public static Color DeepSkyBlue
        {
            get
            {
                return new Color(KnownColor.DeepSkyBlue);
            }
        }
        public static Color DimGray
        {
            get
            {
                return new Color(KnownColor.DimGray);
            }
        }
        public static Color DodgerBlue
        {
            get
            {
                return new Color(KnownColor.DodgerBlue);
            }
        }
        public static Color Firebrick
        {
            get
            {
                return new Color(KnownColor.Firebrick);
            }
        }
        public static Color FloralWhite
        {
            get
            {
                return new Color(KnownColor.FloralWhite);
            }
        }
        public static Color ForestGreen
        {
            get
            {
                return new Color(KnownColor.ForestGreen);
            }
        }
        public static Color Fuchsia
        {
            get
            {
                return new Color(KnownColor.Fuchsia);
            }
        }
        public static Color Gainsboro
        {
            get
            {
                return new Color(KnownColor.Gainsboro);
            }
        }
        public static Color GhostWhite
        {
            get
            {
                return new Color(KnownColor.GhostWhite);
            }
        }
        public static Color Gold
        {
            get
            {
                return new Color(KnownColor.Gold);
            }
        }
        public static Color Goldenrod
        {
            get
            {
                return new Color(KnownColor.Goldenrod);
            }
        }
        public static Color Gray
        {
            get
            {
                return new Color(KnownColor.Gray);
            }
        }
        public static Color Green
        {
            get
            {
                return new Color(KnownColor.Green);
            }
        }
        public static Color GreenYellow
        {
            get
            {
                return new Color(KnownColor.GreenYellow);
            }
        }
        public static Color Honeydew
        {
            get
            {
                return new Color(KnownColor.Honeydew);
            }
        }
        public static Color HotPink
        {
            get
            {
                return new Color(KnownColor.HotPink);
            }
        }
        public static Color IndianRed
        {
            get
            {
                return new Color(KnownColor.IndianRed);
            }
        }
        public static Color Indigo
        {
            get
            {
                return new Color(KnownColor.Indigo);
            }
        }
        public static Color Ivory
        {
            get
            {
                return new Color(KnownColor.Ivory);
            }
        }
        public static Color Khaki
        {
            get
            {
                return new Color(KnownColor.Khaki);
            }
        }
        public static Color Lavender
        {
            get
            {
                return new Color(KnownColor.Lavender);
            }
        }
        public static Color LavenderBlush
        {
            get
            {
                return new Color(KnownColor.LavenderBlush);
            }
        }
        public static Color LawnGreen
        {
            get
            {
                return new Color(KnownColor.LawnGreen);
            }
        }
        public static Color LemonChiffon
        {
            get
            {
                return new Color(KnownColor.LemonChiffon);
            }
        }
        public static Color LightBlue
        {
            get
            {
                return new Color(KnownColor.LightBlue);
            }
        }
        public static Color LightCoral
        {
            get
            {
                return new Color(KnownColor.LightCoral);
            }
        }
        public static Color LightCyan
        {
            get
            {
                return new Color(KnownColor.LightCyan);
            }
        }
        public static Color LightGoldenrodYellow
        {
            get
            {
                return new Color(KnownColor.LightGoldenrodYellow);
            }
        }
        public static Color LightGreen
        {
            get
            {
                return new Color(KnownColor.LightGreen);
            }
        }
        public static Color LightGray
        {
            get
            {
                return new Color(KnownColor.LightGray);
            }
        }
        public static Color LightPink
        {
            get
            {
                return new Color(KnownColor.LightPink);
            }
        }
        public static Color LightSalmon
        {
            get
            {
                return new Color(KnownColor.LightSalmon);
            }
        }
        public static Color LightSeaGreen
        {
            get
            {
                return new Color(KnownColor.LightSeaGreen);
            }
        }
        public static Color LightSkyBlue
        {
            get
            {
                return new Color(KnownColor.LightSkyBlue);
            }
        }
        public static Color LightSlateGray
        {
            get
            {
                return new Color(KnownColor.LightSlateGray);
            }
        }
        public static Color LightSteelBlue
        {
            get
            {
                return new Color(KnownColor.LightSteelBlue);
            }
        }
        public static Color LightYellow
        {
            get
            {
                return new Color(KnownColor.LightYellow);
            }
        }
        public static Color Lime
        {
            get
            {
                return new Color(KnownColor.Lime);
            }
        }
        public static Color LimeGreen
        {
            get
            {
                return new Color(KnownColor.LimeGreen);
            }
        }
        public static Color Linen
        {
            get
            {
                return new Color(KnownColor.Linen);
            }
        }
        public static Color Magenta
        {
            get
            {
                return new Color(KnownColor.Magenta);
            }
        }
        public static Color Maroon
        {
            get
            {
                return new Color(KnownColor.Maroon);
            }
        }
        public static Color MediumAquamarine
        {
            get
            {
                return new Color(KnownColor.MediumAquamarine);
            }
        }
        public static Color MediumBlue
        {
            get
            {
                return new Color(KnownColor.MediumBlue);
            }
        }
        public static Color MediumOrchid
        {
            get
            {
                return new Color(KnownColor.MediumOrchid);
            }
        }
        public static Color MediumPurple
        {
            get
            {
                return new Color(KnownColor.MediumPurple);
            }
        }
        public static Color MediumSeaGreen
        {
            get
            {
                return new Color(KnownColor.MediumSeaGreen);
            }
        }
        public static Color MediumSlateBlue
        {
            get
            {
                return new Color(KnownColor.MediumSlateBlue);
            }
        }
        public static Color MediumSpringGreen
        {
            get
            {
                return new Color(KnownColor.MediumSpringGreen);
            }
        }
        public static Color MediumTurquoise
        {
            get
            {
                return new Color(KnownColor.MediumTurquoise);
            }
        }
        public static Color MediumVioletRed
        {
            get
            {
                return new Color(KnownColor.MediumVioletRed);
            }
        }
        public static Color MidnightBlue
        {
            get
            {
                return new Color(KnownColor.MidnightBlue);
            }
        }
        public static Color MintCream
        {
            get
            {
                return new Color(KnownColor.MintCream);
            }
        }
        public static Color MistyRose
        {
            get
            {
                return new Color(KnownColor.MistyRose);
            }
        }
        public static Color Moccasin
        {
            get
            {
                return new Color(KnownColor.Moccasin);
            }
        }
        public static Color NavajoWhite
        {
            get
            {
                return new Color(KnownColor.NavajoWhite);
            }
        }
        public static Color Navy
        {
            get
            {
                return new Color(KnownColor.Navy);
            }
        }
        public static Color OldLace
        {
            get
            {
                return new Color(KnownColor.OldLace);
            }
        }
        public static Color Olive
        {
            get
            {
                return new Color(KnownColor.Olive);
            }
        }
        public static Color OliveDrab
        {
            get
            {
                return new Color(KnownColor.OliveDrab);
            }
        }
        public static Color Orange
        {
            get
            {
                return new Color(KnownColor.Orange);
            }
        }
        public static Color OrangeRed
        {
            get
            {
                return new Color(KnownColor.OrangeRed);
            }
        }
        public static Color Orchid
        {
            get
            {
                return new Color(KnownColor.Orchid);
            }
        }
        public static Color PaleGoldenrod
        {
            get
            {
                return new Color(KnownColor.PaleGoldenrod);
            }
        }
        public static Color PaleGreen
        {
            get
            {
                return new Color(KnownColor.PaleGreen);
            }
        }
        public static Color PaleTurquoise
        {
            get
            {
                return new Color(KnownColor.PaleTurquoise);
            }
        }
        public static Color PaleVioletRed
        {
            get
            {
                return new Color(KnownColor.PaleVioletRed);
            }
        }
        public static Color PapayaWhip
        {
            get
            {
                return new Color(KnownColor.PapayaWhip);
            }
        }
        public static Color PeachPuff
        {
            get
            {
                return new Color(KnownColor.PeachPuff);
            }
        }
        public static Color Peru
        {
            get
            {
                return new Color(KnownColor.Peru);
            }
        }
        public static Color Pink
        {
            get
            {
                return new Color(KnownColor.Pink);
            }
        }
        public static Color Plum
        {
            get
            {
                return new Color(KnownColor.Plum);
            }
        }
        public static Color PowderBlue
        {
            get
            {
                return new Color(KnownColor.PowderBlue);
            }
        }
        public static Color Purple
        {
            get
            {
                return new Color(KnownColor.Purple);
            }
        }
        public static Color Red
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new Color(KnownColor.Red);
            }
        }
        public static Color RosyBrown
        {
            get
            {
                return new Color(KnownColor.RosyBrown);
            }
        }
        public static Color RoyalBlue
        {
            get
            {
                return new Color(KnownColor.RoyalBlue);
            }
        }
        public static Color SaddleBrown
        {
            get
            {
                return new Color(KnownColor.SaddleBrown);
            }
        }
        public static Color Salmon
        {
            get
            {
                return new Color(KnownColor.Salmon);
            }
        }
        public static Color SandyBrown
        {
            get
            {
                return new Color(KnownColor.SandyBrown);
            }
        }
        public static Color SeaGreen
        {
            get
            {
                return new Color(KnownColor.SeaGreen);
            }
        }
        public static Color SeaShell
        {
            get
            {
                return new Color(KnownColor.SeaShell);
            }
        }
        public static Color Sienna
        {
            get
            {
                return new Color(KnownColor.Sienna);
            }
        }
        public static Color Silver
        {
            get
            {
                return new Color(KnownColor.Silver);
            }
        }
        public static Color SkyBlue
        {
            get
            {
                return new Color(KnownColor.SkyBlue);
            }
        }
        public static Color SlateBlue
        {
            get
            {
                return new Color(KnownColor.SlateBlue);
            }
        }
        public static Color SlateGray
        {
            get
            {
                return new Color(KnownColor.SlateGray);
            }
        }
        public static Color Snow
        {
            get
            {
                return new Color(KnownColor.Snow);
            }
        }
        public static Color SpringGreen
        {
            get
            {
                return new Color(KnownColor.SpringGreen);
            }
        }
        public static Color SteelBlue
        {
            get
            {
                return new Color(KnownColor.SteelBlue);
            }
        }
        public static Color Tan
        {
            get
            {
                return new Color(KnownColor.Tan);
            }
        }
        public static Color Teal
        {
            get
            {
                return new Color(KnownColor.Teal);
            }
        }
        public static Color Thistle
        {
            get
            {
                return new Color(KnownColor.Thistle);
            }
        }
        public static Color Tomato
        {
            get
            {
                return new Color(KnownColor.Tomato);
            }
        }
        public static Color Turquoise
        {
            get
            {
                return new Color(KnownColor.Turquoise);
            }
        }
        public static Color Violet
        {
            get
            {
                return new Color(KnownColor.Violet);
            }
        }
        public static Color Wheat
        {
            get
            {
                return new Color(KnownColor.Wheat);
            }
        }
        public static Color White
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return new Color(KnownColor.White);
            }
        }
        public static Color WhiteSmoke
        {
            get
            {
                return new Color(KnownColor.WhiteSmoke);
            }
        }
        public static Color Yellow
        {
            get
            {
                return new Color(KnownColor.Yellow);
            }
        }
        public static Color YellowGreen
        {
            get
            {
                return new Color(KnownColor.YellowGreen);
            }
        }
        internal Color(KnownColor knownColor)
        {
            this.value = 0L;
            this.state = StateKnownColorValid;
            this.name = null;
            this.knownColor = (short) knownColor;
        }

        private Color(long value, short state, string name, KnownColor knownColor)
        {
            this.value = value;
            this.state = state;
            this.name = name;
            this.knownColor = (short) knownColor;
        }

        public byte R
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (byte) ((this.Value >> 0x10) & 0xffL);
            }
        }
        public byte G
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (byte) ((this.Value >> 8) & 0xffL);
            }
        }
        public byte B
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (byte) (this.Value & 0xffL);
            }
        }
        public byte A
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (byte) ((this.Value >> 0x18) & 0xffL);
            }
        }
        public bool IsKnownColor
        {
            get
            {
                return ((this.state & StateKnownColorValid) != 0);
            }
        }
        public bool IsEmpty
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                return (this.state == 0);
            }
        }
        public bool IsNamedColor
        {
            get
            {
                if ((this.state & StateNameValid) == 0)
                {
                    return this.IsKnownColor;
                }
                return true;
            }
        }
        public bool IsSystemColor
        {
            get
            {
                if (!this.IsKnownColor)
                {
                    return false;
                }
                if (this.knownColor > 0x1a)
                {
                    return (this.knownColor > 0xa7);
                }
                return true;
            }
        }
        private string NameAndARGBValue
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "{{Name={0}, ARGB=({1}, {2}, {3}, {4})}}", new object[] { this.Name, this.A, this.R, this.G, this.B });
            }
        }
        public string Name
        {
            get
            {
                if ((this.state & StateNameValid) != 0)
                {
                    return this.name;
                }
                if (!this.IsKnownColor)
                {
                    return Convert.ToString(this.value, 0x10);
                }
                string str = KnownColorTable.KnownColorToName((KnownColor) this.knownColor);
                if (str != null)
                {
                    return str;
                }
                return ((KnownColor) this.knownColor).ToString();
            }
        }
        private long Value
        {
            get
            {
                if ((this.state & StateValueMask) != 0)
                {
                    return this.value;
                }
                if (this.IsKnownColor)
                {
                    return (long) KnownColorTable.KnownColorToArgb((KnownColor) this.knownColor);
                }
                return NotDefinedValue;
            }
        }
        private static void CheckByte(int value, string name)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw new ArgumentException(System.Drawing.SR.GetString("InvalidEx2BoundArgument", new object[] { name, value, 0, 0xff }));
            }
        }

        private static long MakeArgb(byte alpha, byte red, byte green, byte blue)
        {
            return (long) (((ulong) ((((red << 0x10) | (green << 8)) | blue) | (alpha << 0x18))) & 0xffffffffL);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static Color FromArgb(int argb)
        {
            return new Color(argb & ((long) 0xffffffffL), StateARGBValueValid, null, (KnownColor) 0);
        }

        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            CheckByte(alpha, "alpha");
            CheckByte(red, "red");
            CheckByte(green, "green");
            CheckByte(blue, "blue");
            return new Color(MakeArgb((byte) alpha, (byte) red, (byte) green, (byte) blue), StateARGBValueValid, null, (KnownColor) 0);
        }

        public static Color FromArgb(int alpha, Color baseColor)
        {
            CheckByte(alpha, "alpha");
            return new Color(MakeArgb((byte) alpha, baseColor.R, baseColor.G, baseColor.B), StateARGBValueValid, null, (KnownColor) 0);
        }

        public static Color FromArgb(int red, int green, int blue)
        {
            return FromArgb(0xff, red, green, blue);
        }

        public static Color FromKnownColor(KnownColor color)
        {
            if (!System.Drawing.ClientUtils.IsEnumValid(color, (int) color, 1, 0xae))
            {
                return FromName(color.ToString());
            }
            return new Color(color);
        }

        public static Color FromName(string name)
        {
            object namedColor = ColorConverter.GetNamedColor(name);
            if (namedColor != null)
            {
                return (Color) namedColor;
            }
            return new Color(NotDefinedValue, StateNameValid, name, (KnownColor) 0);
        }

        public float GetBrightness()
        {
            float num = ((float) this.R) / 255f;
            float num2 = ((float) this.G) / 255f;
            float num3 = ((float) this.B) / 255f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            return ((num4 + num5) / 2f);
        }

        public float GetHue()
        {
            if ((this.R == this.G) && (this.G == this.B))
            {
                return 0f;
            }
            float num = ((float) this.R) / 255f;
            float num2 = ((float) this.G) / 255f;
            float num3 = ((float) this.B) / 255f;
            float num7 = 0f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            float num6 = num4 - num5;
            if (num == num4)
            {
                num7 = (num2 - num3) / num6;
            }
            else if (num2 == num4)
            {
                num7 = 2f + ((num3 - num) / num6);
            }
            else if (num3 == num4)
            {
                num7 = 4f + ((num - num2) / num6);
            }
            num7 *= 60f;
            if (num7 < 0f)
            {
                num7 += 360f;
            }
            return num7;
        }

        public float GetSaturation()
        {
            float num = ((float) this.R) / 255f;
            float num2 = ((float) this.G) / 255f;
            float num3 = ((float) this.B) / 255f;
            float num7 = 0f;
            float num4 = num;
            float num5 = num;
            if (num2 > num4)
            {
                num4 = num2;
            }
            if (num3 > num4)
            {
                num4 = num3;
            }
            if (num2 < num5)
            {
                num5 = num2;
            }
            if (num3 < num5)
            {
                num5 = num3;
            }
            if (num4 == num5)
            {
                return num7;
            }
            float num6 = (num4 + num5) / 2f;
            if (num6 <= 0.5)
            {
                return ((num4 - num5) / (num4 + num5));
            }
            return ((num4 - num5) / ((2f - num4) - num5));
        }

        public int ToArgb()
        {
            return (int) this.Value;
        }

        public KnownColor ToKnownColor()
        {
            return (KnownColor) this.knownColor;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x20);
            builder.Append(base.GetType().Name);
            builder.Append(" [");
            if ((this.state & StateNameValid) != 0)
            {
                builder.Append(this.Name);
            }
            else if ((this.state & StateKnownColorValid) != 0)
            {
                builder.Append(this.Name);
            }
            else if ((this.state & StateValueMask) != 0)
            {
                builder.Append("A=");
                builder.Append(this.A);
                builder.Append(", R=");
                builder.Append(this.R);
                builder.Append(", G=");
                builder.Append(this.G);
                builder.Append(", B=");
                builder.Append(this.B);
            }
            else
            {
                builder.Append("Empty");
            }
            builder.Append("]");
            return builder.ToString();
        }

        public static bool operator ==(Color left, Color right)
        {
            if (((left.value != right.value) || (left.state != right.state)) || (left.knownColor != right.knownColor))
            {
                return false;
            }
            return ((left.name == right.name) || (((left.name != null) && (right.name != null)) && left.name.Equals(right.name)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                Color color = (Color) obj;
                if (((this.value == color.value) && (this.state == color.state)) && (this.knownColor == color.knownColor))
                {
                    return ((this.name == color.name) || (((this.name != null) && (color.name != null)) && this.name.Equals(this.name)));
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ((this.value.GetHashCode() ^ this.state.GetHashCode()) ^ this.knownColor.GetHashCode());
        }

        static Color()
        {
            Empty = new Color();
            StateKnownColorValid = 1;
            StateARGBValueValid = 2;
            StateValueMask = StateARGBValueValid;
            StateNameValid = 8;
            NotDefinedValue = 0L;
        }
    }
}

