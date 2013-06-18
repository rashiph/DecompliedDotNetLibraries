namespace System.Drawing
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;

    internal static class KnownColorTable
    {
        private const int AlphaShift = 0x18;
        private const int BlueShift = 0;
        private static string[] colorNameTable;
        private static int[] colorTable;
        private const int GreenShift = 8;
        private const int RedShift = 0x10;
        private const int Win32BlueShift = 0x10;
        private const int Win32GreenShift = 8;
        private const int Win32RedShift = 0;

        public static System.Drawing.Color ArgbToKnownColor(int targetARGB)
        {
            EnsureColorTable();
            for (int i = 0; i < colorTable.Length; i++)
            {
                int num2 = colorTable[i];
                if (num2 == targetARGB)
                {
                    System.Drawing.Color color = System.Drawing.Color.FromKnownColor((KnownColor) i);
                    if (!color.IsSystemColor)
                    {
                        return color;
                    }
                }
            }
            return System.Drawing.Color.FromArgb(targetARGB);
        }

        private static int Encode(int alpha, int red, int green, int blue)
        {
            return ((((red << 0x10) | (green << 8)) | blue) | (alpha << 0x18));
        }

        private static void EnsureColorNameTable()
        {
            if (colorNameTable == null)
            {
                InitColorNameTable();
            }
        }

        private static void EnsureColorTable()
        {
            if (colorTable == null)
            {
                InitColorTable();
            }
        }

        private static int FromWin32Value(int value)
        {
            return Encode(0xff, value & 0xff, (value >> 8) & 0xff, (value >> 0x10) & 0xff);
        }

        private static void InitColorNameTable()
        {
            string[] strArray = new string[0xaf];
            strArray[1] = "ActiveBorder";
            strArray[2] = "ActiveCaption";
            strArray[3] = "ActiveCaptionText";
            strArray[4] = "AppWorkspace";
            strArray[0xa8] = "ButtonFace";
            strArray[0xa9] = "ButtonHighlight";
            strArray[170] = "ButtonShadow";
            strArray[5] = "Control";
            strArray[6] = "ControlDark";
            strArray[7] = "ControlDarkDark";
            strArray[8] = "ControlLight";
            strArray[9] = "ControlLightLight";
            strArray[10] = "ControlText";
            strArray[11] = "Desktop";
            strArray[0xab] = "GradientActiveCaption";
            strArray[0xac] = "GradientInactiveCaption";
            strArray[12] = "GrayText";
            strArray[13] = "Highlight";
            strArray[14] = "HighlightText";
            strArray[15] = "HotTrack";
            strArray[0x10] = "InactiveBorder";
            strArray[0x11] = "InactiveCaption";
            strArray[0x12] = "InactiveCaptionText";
            strArray[0x13] = "Info";
            strArray[20] = "InfoText";
            strArray[0x15] = "Menu";
            strArray[0xad] = "MenuBar";
            strArray[0xae] = "MenuHighlight";
            strArray[0x16] = "MenuText";
            strArray[0x17] = "ScrollBar";
            strArray[0x18] = "Window";
            strArray[0x19] = "WindowFrame";
            strArray[0x1a] = "WindowText";
            strArray[0x1b] = "Transparent";
            strArray[0x1c] = "AliceBlue";
            strArray[0x1d] = "AntiqueWhite";
            strArray[30] = "Aqua";
            strArray[0x1f] = "Aquamarine";
            strArray[0x20] = "Azure";
            strArray[0x21] = "Beige";
            strArray[0x22] = "Bisque";
            strArray[0x23] = "Black";
            strArray[0x24] = "BlanchedAlmond";
            strArray[0x25] = "Blue";
            strArray[0x26] = "BlueViolet";
            strArray[0x27] = "Brown";
            strArray[40] = "BurlyWood";
            strArray[0x29] = "CadetBlue";
            strArray[0x2a] = "Chartreuse";
            strArray[0x2b] = "Chocolate";
            strArray[0x2c] = "Coral";
            strArray[0x2d] = "CornflowerBlue";
            strArray[0x2e] = "Cornsilk";
            strArray[0x2f] = "Crimson";
            strArray[0x30] = "Cyan";
            strArray[0x31] = "DarkBlue";
            strArray[50] = "DarkCyan";
            strArray[0x33] = "DarkGoldenrod";
            strArray[0x34] = "DarkGray";
            strArray[0x35] = "DarkGreen";
            strArray[0x36] = "DarkKhaki";
            strArray[0x37] = "DarkMagenta";
            strArray[0x38] = "DarkOliveGreen";
            strArray[0x39] = "DarkOrange";
            strArray[0x3a] = "DarkOrchid";
            strArray[0x3b] = "DarkRed";
            strArray[60] = "DarkSalmon";
            strArray[0x3d] = "DarkSeaGreen";
            strArray[0x3e] = "DarkSlateBlue";
            strArray[0x3f] = "DarkSlateGray";
            strArray[0x40] = "DarkTurquoise";
            strArray[0x41] = "DarkViolet";
            strArray[0x42] = "DeepPink";
            strArray[0x43] = "DeepSkyBlue";
            strArray[0x44] = "DimGray";
            strArray[0x45] = "DodgerBlue";
            strArray[70] = "Firebrick";
            strArray[0x47] = "FloralWhite";
            strArray[0x48] = "ForestGreen";
            strArray[0x49] = "Fuchsia";
            strArray[0x4a] = "Gainsboro";
            strArray[0x4b] = "GhostWhite";
            strArray[0x4c] = "Gold";
            strArray[0x4d] = "Goldenrod";
            strArray[0x4e] = "Gray";
            strArray[0x4f] = "Green";
            strArray[80] = "GreenYellow";
            strArray[0x51] = "Honeydew";
            strArray[0x52] = "HotPink";
            strArray[0x53] = "IndianRed";
            strArray[0x54] = "Indigo";
            strArray[0x55] = "Ivory";
            strArray[0x56] = "Khaki";
            strArray[0x57] = "Lavender";
            strArray[0x58] = "LavenderBlush";
            strArray[0x59] = "LawnGreen";
            strArray[90] = "LemonChiffon";
            strArray[0x5b] = "LightBlue";
            strArray[0x5c] = "LightCoral";
            strArray[0x5d] = "LightCyan";
            strArray[0x5e] = "LightGoldenrodYellow";
            strArray[0x5f] = "LightGray";
            strArray[0x60] = "LightGreen";
            strArray[0x61] = "LightPink";
            strArray[0x62] = "LightSalmon";
            strArray[0x63] = "LightSeaGreen";
            strArray[100] = "LightSkyBlue";
            strArray[0x65] = "LightSlateGray";
            strArray[0x66] = "LightSteelBlue";
            strArray[0x67] = "LightYellow";
            strArray[0x68] = "Lime";
            strArray[0x69] = "LimeGreen";
            strArray[0x6a] = "Linen";
            strArray[0x6b] = "Magenta";
            strArray[0x6c] = "Maroon";
            strArray[0x6d] = "MediumAquamarine";
            strArray[110] = "MediumBlue";
            strArray[0x6f] = "MediumOrchid";
            strArray[0x70] = "MediumPurple";
            strArray[0x71] = "MediumSeaGreen";
            strArray[0x72] = "MediumSlateBlue";
            strArray[0x73] = "MediumSpringGreen";
            strArray[0x74] = "MediumTurquoise";
            strArray[0x75] = "MediumVioletRed";
            strArray[0x76] = "MidnightBlue";
            strArray[0x77] = "MintCream";
            strArray[120] = "MistyRose";
            strArray[0x79] = "Moccasin";
            strArray[0x7a] = "NavajoWhite";
            strArray[0x7b] = "Navy";
            strArray[0x7c] = "OldLace";
            strArray[0x7d] = "Olive";
            strArray[0x7e] = "OliveDrab";
            strArray[0x7f] = "Orange";
            strArray[0x80] = "OrangeRed";
            strArray[0x81] = "Orchid";
            strArray[130] = "PaleGoldenrod";
            strArray[0x83] = "PaleGreen";
            strArray[0x84] = "PaleTurquoise";
            strArray[0x85] = "PaleVioletRed";
            strArray[0x86] = "PapayaWhip";
            strArray[0x87] = "PeachPuff";
            strArray[0x88] = "Peru";
            strArray[0x89] = "Pink";
            strArray[0x8a] = "Plum";
            strArray[0x8b] = "PowderBlue";
            strArray[140] = "Purple";
            strArray[0x8d] = "Red";
            strArray[0x8e] = "RosyBrown";
            strArray[0x8f] = "RoyalBlue";
            strArray[0x90] = "SaddleBrown";
            strArray[0x91] = "Salmon";
            strArray[0x92] = "SandyBrown";
            strArray[0x93] = "SeaGreen";
            strArray[0x94] = "SeaShell";
            strArray[0x95] = "Sienna";
            strArray[150] = "Silver";
            strArray[0x97] = "SkyBlue";
            strArray[0x98] = "SlateBlue";
            strArray[0x99] = "SlateGray";
            strArray[0x9a] = "Snow";
            strArray[0x9b] = "SpringGreen";
            strArray[0x9c] = "SteelBlue";
            strArray[0x9d] = "Tan";
            strArray[0x9e] = "Teal";
            strArray[0x9f] = "Thistle";
            strArray[160] = "Tomato";
            strArray[0xa1] = "Turquoise";
            strArray[0xa2] = "Violet";
            strArray[0xa3] = "Wheat";
            strArray[0xa4] = "White";
            strArray[0xa5] = "WhiteSmoke";
            strArray[0xa6] = "Yellow";
            strArray[0xa7] = "YellowGreen";
            colorNameTable = strArray;
        }

        private static void InitColorTable()
        {
            int[] colorTable = new int[0xaf];
            SystemEvents.UserPreferenceChanging += new UserPreferenceChangingEventHandler(KnownColorTable.OnUserPreferenceChanging);
            UpdateSystemColors(colorTable);
            colorTable[0x1b] = 0xffffff;
            colorTable[0x1c] = -984833;
            colorTable[0x1d] = -332841;
            colorTable[30] = -16711681;
            colorTable[0x1f] = -8388652;
            colorTable[0x20] = -983041;
            colorTable[0x21] = -657956;
            colorTable[0x22] = -6972;
            colorTable[0x23] = -16777216;
            colorTable[0x24] = -5171;
            colorTable[0x25] = -16776961;
            colorTable[0x26] = -7722014;
            colorTable[0x27] = -5952982;
            colorTable[40] = -2180985;
            colorTable[0x29] = -10510688;
            colorTable[0x2a] = -8388864;
            colorTable[0x2b] = -2987746;
            colorTable[0x2c] = -32944;
            colorTable[0x2d] = -10185235;
            colorTable[0x2e] = -1828;
            colorTable[0x2f] = -2354116;
            colorTable[0x30] = -16711681;
            colorTable[0x31] = -16777077;
            colorTable[50] = -16741493;
            colorTable[0x33] = -4684277;
            colorTable[0x34] = -5658199;
            colorTable[0x35] = -16751616;
            colorTable[0x36] = -4343957;
            colorTable[0x37] = -7667573;
            colorTable[0x38] = -11179217;
            colorTable[0x39] = -29696;
            colorTable[0x3a] = -6737204;
            colorTable[0x3b] = -7667712;
            colorTable[60] = -1468806;
            colorTable[0x3d] = -7357301;
            colorTable[0x3e] = -12042869;
            colorTable[0x3f] = -13676721;
            colorTable[0x40] = -16724271;
            colorTable[0x41] = -7077677;
            colorTable[0x42] = -60269;
            colorTable[0x43] = -16728065;
            colorTable[0x44] = -9868951;
            colorTable[0x45] = -14774017;
            colorTable[70] = -5103070;
            colorTable[0x47] = -1296;
            colorTable[0x48] = -14513374;
            colorTable[0x49] = -65281;
            colorTable[0x4a] = -2302756;
            colorTable[0x4b] = -460545;
            colorTable[0x4c] = -10496;
            colorTable[0x4d] = -2448096;
            colorTable[0x4e] = -8355712;
            colorTable[0x4f] = -16744448;
            colorTable[80] = -5374161;
            colorTable[0x51] = -983056;
            colorTable[0x52] = -38476;
            colorTable[0x53] = -3318692;
            colorTable[0x54] = -11861886;
            colorTable[0x55] = -16;
            colorTable[0x56] = -989556;
            colorTable[0x57] = -1644806;
            colorTable[0x58] = -3851;
            colorTable[0x59] = -8586240;
            colorTable[90] = -1331;
            colorTable[0x5b] = -5383962;
            colorTable[0x5c] = -1015680;
            colorTable[0x5d] = -2031617;
            colorTable[0x5e] = -329006;
            colorTable[0x5f] = -2894893;
            colorTable[0x60] = -7278960;
            colorTable[0x61] = -18751;
            colorTable[0x62] = -24454;
            colorTable[0x63] = -14634326;
            colorTable[100] = -7876870;
            colorTable[0x65] = -8943463;
            colorTable[0x66] = -5192482;
            colorTable[0x67] = -32;
            colorTable[0x68] = -16711936;
            colorTable[0x69] = -13447886;
            colorTable[0x6a] = -331546;
            colorTable[0x6b] = -65281;
            colorTable[0x6c] = -8388608;
            colorTable[0x6d] = -10039894;
            colorTable[110] = -16777011;
            colorTable[0x6f] = -4565549;
            colorTable[0x70] = -7114533;
            colorTable[0x71] = -12799119;
            colorTable[0x72] = -8689426;
            colorTable[0x73] = -16713062;
            colorTable[0x74] = -12004916;
            colorTable[0x75] = -3730043;
            colorTable[0x76] = -15132304;
            colorTable[0x77] = -655366;
            colorTable[120] = -6943;
            colorTable[0x79] = -6987;
            colorTable[0x7a] = -8531;
            colorTable[0x7b] = -16777088;
            colorTable[0x7c] = -133658;
            colorTable[0x7d] = -8355840;
            colorTable[0x7e] = -9728477;
            colorTable[0x7f] = -23296;
            colorTable[0x80] = -47872;
            colorTable[0x81] = -2461482;
            colorTable[130] = -1120086;
            colorTable[0x83] = -6751336;
            colorTable[0x84] = -5247250;
            colorTable[0x85] = -2396013;
            colorTable[0x86] = -4139;
            colorTable[0x87] = -9543;
            colorTable[0x88] = -3308225;
            colorTable[0x89] = -16181;
            colorTable[0x8a] = -2252579;
            colorTable[0x8b] = -5185306;
            colorTable[140] = -8388480;
            colorTable[0x8d] = -65536;
            colorTable[0x8e] = -4419697;
            colorTable[0x8f] = -12490271;
            colorTable[0x90] = -7650029;
            colorTable[0x91] = -360334;
            colorTable[0x92] = -744352;
            colorTable[0x93] = -13726889;
            colorTable[0x94] = -2578;
            colorTable[0x95] = -6270419;
            colorTable[150] = -4144960;
            colorTable[0x97] = -7876885;
            colorTable[0x98] = -9807155;
            colorTable[0x99] = -9404272;
            colorTable[0x9a] = -1286;
            colorTable[0x9b] = -16711809;
            colorTable[0x9c] = -12156236;
            colorTable[0x9d] = -2968436;
            colorTable[0x9e] = -16744320;
            colorTable[0x9f] = -2572328;
            colorTable[160] = -40121;
            colorTable[0xa1] = -12525360;
            colorTable[0xa2] = -1146130;
            colorTable[0xa3] = -663885;
            colorTable[0xa4] = -1;
            colorTable[0xa5] = -657931;
            colorTable[0xa6] = -256;
            colorTable[0xa7] = -6632142;
            KnownColorTable.colorTable = colorTable;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int KnownColorToArgb(KnownColor color)
        {
            EnsureColorTable();
            if (color <= KnownColor.MenuHighlight)
            {
                return colorTable[(int) color];
            }
            return 0;
        }

        public static string KnownColorToName(KnownColor color)
        {
            EnsureColorNameTable();
            if (color <= KnownColor.MenuHighlight)
            {
                return colorNameTable[(int) color];
            }
            return null;
        }

        private static void OnUserPreferenceChanging(object sender, UserPreferenceChangingEventArgs e)
        {
            if ((e.Category == UserPreferenceCategory.Color) && (colorTable != null))
            {
                UpdateSystemColors(colorTable);
            }
        }

        private static int SystemColorToArgb(int index)
        {
            return FromWin32Value(System.Drawing.SafeNativeMethods.GetSysColor(index));
        }

        private static void UpdateSystemColors(int[] colorTable)
        {
            colorTable[1] = SystemColorToArgb(10);
            colorTable[2] = SystemColorToArgb(2);
            colorTable[3] = SystemColorToArgb(9);
            colorTable[4] = SystemColorToArgb(12);
            colorTable[0xa8] = SystemColorToArgb(15);
            colorTable[0xa9] = SystemColorToArgb(20);
            colorTable[170] = SystemColorToArgb(0x10);
            colorTable[5] = SystemColorToArgb(15);
            colorTable[6] = SystemColorToArgb(0x10);
            colorTable[7] = SystemColorToArgb(0x15);
            colorTable[8] = SystemColorToArgb(0x16);
            colorTable[9] = SystemColorToArgb(20);
            colorTable[10] = SystemColorToArgb(0x12);
            colorTable[11] = SystemColorToArgb(1);
            colorTable[0xab] = SystemColorToArgb(0x1b);
            colorTable[0xac] = SystemColorToArgb(0x1c);
            colorTable[12] = SystemColorToArgb(0x11);
            colorTable[13] = SystemColorToArgb(13);
            colorTable[14] = SystemColorToArgb(14);
            colorTable[15] = SystemColorToArgb(0x1a);
            colorTable[0x10] = SystemColorToArgb(11);
            colorTable[0x11] = SystemColorToArgb(3);
            colorTable[0x12] = SystemColorToArgb(0x13);
            colorTable[0x13] = SystemColorToArgb(0x18);
            colorTable[20] = SystemColorToArgb(0x17);
            colorTable[0x15] = SystemColorToArgb(4);
            colorTable[0xad] = SystemColorToArgb(30);
            colorTable[0xae] = SystemColorToArgb(0x1d);
            colorTable[0x16] = SystemColorToArgb(7);
            colorTable[0x17] = SystemColorToArgb(0);
            colorTable[0x18] = SystemColorToArgb(5);
            colorTable[0x19] = SystemColorToArgb(6);
            colorTable[0x1a] = SystemColorToArgb(8);
        }
    }
}

