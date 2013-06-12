namespace System.Drawing
{
    using System;

    public sealed class Brushes
    {
        private static readonly object AliceBlueKey = new object();
        private static readonly object AntiqueWhiteKey = new object();
        private static readonly object AquaKey = new object();
        private static readonly object AquamarineKey = new object();
        private static readonly object AzureKey = new object();
        private static readonly object BeigeKey = new object();
        private static readonly object BisqueKey = new object();
        private static readonly object BlackKey = new object();
        private static readonly object BlanchedAlmondKey = new object();
        private static readonly object BlueKey = new object();
        private static readonly object BlueVioletKey = new object();
        private static readonly object BrownKey = new object();
        private static readonly object BurlyWoodKey = new object();
        private static readonly object CadetBlueKey = new object();
        private static readonly object ChartreuseKey = new object();
        private static readonly object ChocolateKey = new object();
        private static readonly object ChoralKey = new object();
        private static readonly object CornflowerBlueKey = new object();
        private static readonly object CornsilkKey = new object();
        private static readonly object CrimsonKey = new object();
        private static readonly object CyanKey = new object();
        private static readonly object DarkBlueKey = new object();
        private static readonly object DarkCyanKey = new object();
        private static readonly object DarkGoldenrodKey = new object();
        private static readonly object DarkGrayKey = new object();
        private static readonly object DarkGreenKey = new object();
        private static readonly object DarkKhakiKey = new object();
        private static readonly object DarkMagentaKey = new object();
        private static readonly object DarkOliveGreenKey = new object();
        private static readonly object DarkOrangeKey = new object();
        private static readonly object DarkOrchidKey = new object();
        private static readonly object DarkRedKey = new object();
        private static readonly object DarkSalmonKey = new object();
        private static readonly object DarkSeaGreenKey = new object();
        private static readonly object DarkSlateBlueKey = new object();
        private static readonly object DarkSlateGrayKey = new object();
        private static readonly object DarkTurquoiseKey = new object();
        private static readonly object DarkVioletKey = new object();
        private static readonly object DeepPinkKey = new object();
        private static readonly object DeepSkyBlueKey = new object();
        private static readonly object DimGrayKey = new object();
        private static readonly object DodgerBlueKey = new object();
        private static readonly object FirebrickKey = new object();
        private static readonly object FloralWhiteKey = new object();
        private static readonly object ForestGreenKey = new object();
        private static readonly object FuchiaKey = new object();
        private static readonly object GainsboroKey = new object();
        private static readonly object GhostWhiteKey = new object();
        private static readonly object GoldenrodKey = new object();
        private static readonly object GoldKey = new object();
        private static readonly object GrayKey = new object();
        private static readonly object GreenKey = new object();
        private static readonly object GreenYellowKey = new object();
        private static readonly object HoneydewKey = new object();
        private static readonly object HotPinkKey = new object();
        private static readonly object IndianRedKey = new object();
        private static readonly object IndigoKey = new object();
        private static readonly object IvoryKey = new object();
        private static readonly object KhakiKey = new object();
        private static readonly object LavenderBlushKey = new object();
        private static readonly object LavenderKey = new object();
        private static readonly object LawnGreenKey = new object();
        private static readonly object LemonChiffonKey = new object();
        private static readonly object LightBlueKey = new object();
        private static readonly object LightCoralKey = new object();
        private static readonly object LightCyanKey = new object();
        private static readonly object LightGoldenrodYellowKey = new object();
        private static readonly object LightGrayKey = new object();
        private static readonly object LightGreenKey = new object();
        private static readonly object LightPinkKey = new object();
        private static readonly object LightSalmonKey = new object();
        private static readonly object LightSeaGreenKey = new object();
        private static readonly object LightSkyBlueKey = new object();
        private static readonly object LightSlateGrayKey = new object();
        private static readonly object LightSteelBlueKey = new object();
        private static readonly object LightYellowKey = new object();
        private static readonly object LimeGreenKey = new object();
        private static readonly object LimeKey = new object();
        private static readonly object LinenKey = new object();
        private static readonly object MagentaKey = new object();
        private static readonly object MaroonKey = new object();
        private static readonly object MediumAquamarineKey = new object();
        private static readonly object MediumBlueKey = new object();
        private static readonly object MediumOrchidKey = new object();
        private static readonly object MediumPurpleKey = new object();
        private static readonly object MediumSeaGreenKey = new object();
        private static readonly object MediumSlateBlueKey = new object();
        private static readonly object MediumSpringGreenKey = new object();
        private static readonly object MediumTurquoiseKey = new object();
        private static readonly object MediumVioletRedKey = new object();
        private static readonly object MidnightBlueKey = new object();
        private static readonly object MintCreamKey = new object();
        private static readonly object MistyRoseKey = new object();
        private static readonly object MoccasinKey = new object();
        private static readonly object NavajoWhiteKey = new object();
        private static readonly object NavyKey = new object();
        private static readonly object OldLaceKey = new object();
        private static readonly object OliveDrabKey = new object();
        private static readonly object OliveKey = new object();
        private static readonly object OrangeKey = new object();
        private static readonly object OrangeRedKey = new object();
        private static readonly object OrchidKey = new object();
        private static readonly object PaleGoldenrodKey = new object();
        private static readonly object PaleGreenKey = new object();
        private static readonly object PaleTurquoiseKey = new object();
        private static readonly object PaleVioletRedKey = new object();
        private static readonly object PapayaWhipKey = new object();
        private static readonly object PeachPuffKey = new object();
        private static readonly object PeruKey = new object();
        private static readonly object PinkKey = new object();
        private static readonly object PlumKey = new object();
        private static readonly object PowderBlueKey = new object();
        private static readonly object PurpleKey = new object();
        private static readonly object RedKey = new object();
        private static readonly object RosyBrownKey = new object();
        private static readonly object RoyalBlueKey = new object();
        private static readonly object SaddleBrownKey = new object();
        private static readonly object SalmonKey = new object();
        private static readonly object SandyBrownKey = new object();
        private static readonly object SeaGreenKey = new object();
        private static readonly object SeaShellKey = new object();
        private static readonly object SiennaKey = new object();
        private static readonly object SilverKey = new object();
        private static readonly object SkyBlueKey = new object();
        private static readonly object SlateBlueKey = new object();
        private static readonly object SlateGrayKey = new object();
        private static readonly object SnowKey = new object();
        private static readonly object SpringGreenKey = new object();
        private static readonly object SteelBlueKey = new object();
        private static readonly object TanKey = new object();
        private static readonly object TealKey = new object();
        private static readonly object ThistleKey = new object();
        private static readonly object TomatoKey = new object();
        private static readonly object TransparentKey = new object();
        private static readonly object TurquoiseKey = new object();
        private static readonly object VioletKey = new object();
        private static readonly object WheatKey = new object();
        private static readonly object WhiteKey = new object();
        private static readonly object WhiteSmokeKey = new object();
        private static readonly object YellowGreenKey = new object();
        private static readonly object YellowKey = new object();

        private Brushes()
        {
        }

        public static Brush AliceBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[AliceBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.AliceBlue);
                    SafeNativeMethods.Gdip.ThreadData[AliceBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush AntiqueWhite
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[AntiqueWhiteKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.AntiqueWhite);
                    SafeNativeMethods.Gdip.ThreadData[AntiqueWhiteKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Aqua
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[AquaKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Aqua);
                    SafeNativeMethods.Gdip.ThreadData[AquaKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Aquamarine
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[AquamarineKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Aquamarine);
                    SafeNativeMethods.Gdip.ThreadData[AquamarineKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Azure
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[AzureKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Azure);
                    SafeNativeMethods.Gdip.ThreadData[AzureKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Beige
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BeigeKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Beige);
                    SafeNativeMethods.Gdip.ThreadData[BeigeKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Bisque
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BisqueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Bisque);
                    SafeNativeMethods.Gdip.ThreadData[BisqueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Black
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BlackKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Black);
                    SafeNativeMethods.Gdip.ThreadData[BlackKey] = brush;
                }
                return brush;
            }
        }

        public static Brush BlanchedAlmond
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BlanchedAlmondKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.BlanchedAlmond);
                    SafeNativeMethods.Gdip.ThreadData[BlanchedAlmondKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Blue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Blue);
                    SafeNativeMethods.Gdip.ThreadData[BlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush BlueViolet
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BlueVioletKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.BlueViolet);
                    SafeNativeMethods.Gdip.ThreadData[BlueVioletKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Brown
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BrownKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Brown);
                    SafeNativeMethods.Gdip.ThreadData[BrownKey] = brush;
                }
                return brush;
            }
        }

        public static Brush BurlyWood
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[BurlyWoodKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.BurlyWood);
                    SafeNativeMethods.Gdip.ThreadData[BurlyWoodKey] = brush;
                }
                return brush;
            }
        }

        public static Brush CadetBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[CadetBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.CadetBlue);
                    SafeNativeMethods.Gdip.ThreadData[CadetBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Chartreuse
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[ChartreuseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Chartreuse);
                    SafeNativeMethods.Gdip.ThreadData[ChartreuseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Chocolate
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[ChocolateKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Chocolate);
                    SafeNativeMethods.Gdip.ThreadData[ChocolateKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Coral
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[ChoralKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Coral);
                    SafeNativeMethods.Gdip.ThreadData[ChoralKey] = brush;
                }
                return brush;
            }
        }

        public static Brush CornflowerBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[CornflowerBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.CornflowerBlue);
                    SafeNativeMethods.Gdip.ThreadData[CornflowerBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Cornsilk
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[CornsilkKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Cornsilk);
                    SafeNativeMethods.Gdip.ThreadData[CornsilkKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Crimson
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[CrimsonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Crimson);
                    SafeNativeMethods.Gdip.ThreadData[CrimsonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Cyan
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[CyanKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Cyan);
                    SafeNativeMethods.Gdip.ThreadData[CyanKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkBlue);
                    SafeNativeMethods.Gdip.ThreadData[DarkBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkCyan
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkCyanKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkCyan);
                    SafeNativeMethods.Gdip.ThreadData[DarkCyanKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkGoldenrod
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkGoldenrodKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkGoldenrod);
                    SafeNativeMethods.Gdip.ThreadData[DarkGoldenrodKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkGray);
                    SafeNativeMethods.Gdip.ThreadData[DarkGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkGreen);
                    SafeNativeMethods.Gdip.ThreadData[DarkGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkKhaki
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkKhakiKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkKhaki);
                    SafeNativeMethods.Gdip.ThreadData[DarkKhakiKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkMagenta
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkMagentaKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkMagenta);
                    SafeNativeMethods.Gdip.ThreadData[DarkMagentaKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkOliveGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkOliveGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkOliveGreen);
                    SafeNativeMethods.Gdip.ThreadData[DarkOliveGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkOrange
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkOrangeKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkOrange);
                    SafeNativeMethods.Gdip.ThreadData[DarkOrangeKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkOrchid
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkOrchidKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkOrchid);
                    SafeNativeMethods.Gdip.ThreadData[DarkOrchidKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkRed
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkRedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkRed);
                    SafeNativeMethods.Gdip.ThreadData[DarkRedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkSalmon
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkSalmonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkSalmon);
                    SafeNativeMethods.Gdip.ThreadData[DarkSalmonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkSeaGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkSeaGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkSeaGreen);
                    SafeNativeMethods.Gdip.ThreadData[DarkSeaGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkSlateBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkSlateBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkSlateBlue);
                    SafeNativeMethods.Gdip.ThreadData[DarkSlateBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkSlateGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkSlateGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkSlateGray);
                    SafeNativeMethods.Gdip.ThreadData[DarkSlateGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkTurquoise
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkTurquoiseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkTurquoise);
                    SafeNativeMethods.Gdip.ThreadData[DarkTurquoiseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DarkViolet
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DarkVioletKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DarkViolet);
                    SafeNativeMethods.Gdip.ThreadData[DarkVioletKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DeepPink
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DeepPinkKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DeepPink);
                    SafeNativeMethods.Gdip.ThreadData[DeepPinkKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DeepSkyBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DeepSkyBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DeepSkyBlue);
                    SafeNativeMethods.Gdip.ThreadData[DeepSkyBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DimGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DimGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DimGray);
                    SafeNativeMethods.Gdip.ThreadData[DimGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush DodgerBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[DodgerBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.DodgerBlue);
                    SafeNativeMethods.Gdip.ThreadData[DodgerBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Firebrick
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[FirebrickKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Firebrick);
                    SafeNativeMethods.Gdip.ThreadData[FirebrickKey] = brush;
                }
                return brush;
            }
        }

        public static Brush FloralWhite
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[FloralWhiteKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.FloralWhite);
                    SafeNativeMethods.Gdip.ThreadData[FloralWhiteKey] = brush;
                }
                return brush;
            }
        }

        public static Brush ForestGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[ForestGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.ForestGreen);
                    SafeNativeMethods.Gdip.ThreadData[ForestGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Fuchsia
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[FuchiaKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Fuchsia);
                    SafeNativeMethods.Gdip.ThreadData[FuchiaKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Gainsboro
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GainsboroKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Gainsboro);
                    SafeNativeMethods.Gdip.ThreadData[GainsboroKey] = brush;
                }
                return brush;
            }
        }

        public static Brush GhostWhite
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GhostWhiteKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.GhostWhite);
                    SafeNativeMethods.Gdip.ThreadData[GhostWhiteKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Gold
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GoldKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Gold);
                    SafeNativeMethods.Gdip.ThreadData[GoldKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Goldenrod
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GoldenrodKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Goldenrod);
                    SafeNativeMethods.Gdip.ThreadData[GoldenrodKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Gray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Gray);
                    SafeNativeMethods.Gdip.ThreadData[GrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Green
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Green);
                    SafeNativeMethods.Gdip.ThreadData[GreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush GreenYellow
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[GreenYellowKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.GreenYellow);
                    SafeNativeMethods.Gdip.ThreadData[GreenYellowKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Honeydew
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[HoneydewKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Honeydew);
                    SafeNativeMethods.Gdip.ThreadData[HoneydewKey] = brush;
                }
                return brush;
            }
        }

        public static Brush HotPink
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[HotPinkKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.HotPink);
                    SafeNativeMethods.Gdip.ThreadData[HotPinkKey] = brush;
                }
                return brush;
            }
        }

        public static Brush IndianRed
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[IndianRedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.IndianRed);
                    SafeNativeMethods.Gdip.ThreadData[IndianRedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Indigo
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[IndigoKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Indigo);
                    SafeNativeMethods.Gdip.ThreadData[IndigoKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Ivory
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[IvoryKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Ivory);
                    SafeNativeMethods.Gdip.ThreadData[IvoryKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Khaki
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[KhakiKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Khaki);
                    SafeNativeMethods.Gdip.ThreadData[KhakiKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Lavender
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LavenderKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Lavender);
                    SafeNativeMethods.Gdip.ThreadData[LavenderKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LavenderBlush
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LavenderBlushKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LavenderBlush);
                    SafeNativeMethods.Gdip.ThreadData[LavenderBlushKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LawnGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LawnGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LawnGreen);
                    SafeNativeMethods.Gdip.ThreadData[LawnGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LemonChiffon
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LemonChiffonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LemonChiffon);
                    SafeNativeMethods.Gdip.ThreadData[LemonChiffonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightBlue);
                    SafeNativeMethods.Gdip.ThreadData[LightBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightCoral
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightCoralKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightCoral);
                    SafeNativeMethods.Gdip.ThreadData[LightCoralKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightCyan
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightCyanKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightCyan);
                    SafeNativeMethods.Gdip.ThreadData[LightCyanKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightGoldenrodYellow
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightGoldenrodYellowKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightGoldenrodYellow);
                    SafeNativeMethods.Gdip.ThreadData[LightGoldenrodYellowKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightGray);
                    SafeNativeMethods.Gdip.ThreadData[LightGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightGreen);
                    SafeNativeMethods.Gdip.ThreadData[LightGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightPink
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightPinkKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightPink);
                    SafeNativeMethods.Gdip.ThreadData[LightPinkKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightSalmon
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightSalmonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightSalmon);
                    SafeNativeMethods.Gdip.ThreadData[LightSalmonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightSeaGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightSeaGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightSeaGreen);
                    SafeNativeMethods.Gdip.ThreadData[LightSeaGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightSkyBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightSkyBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightSkyBlue);
                    SafeNativeMethods.Gdip.ThreadData[LightSkyBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightSlateGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightSlateGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightSlateGray);
                    SafeNativeMethods.Gdip.ThreadData[LightSlateGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightSteelBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightSteelBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightSteelBlue);
                    SafeNativeMethods.Gdip.ThreadData[LightSteelBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LightYellow
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LightYellowKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LightYellow);
                    SafeNativeMethods.Gdip.ThreadData[LightYellowKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Lime
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LimeKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Lime);
                    SafeNativeMethods.Gdip.ThreadData[LimeKey] = brush;
                }
                return brush;
            }
        }

        public static Brush LimeGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LimeGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.LimeGreen);
                    SafeNativeMethods.Gdip.ThreadData[LimeGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Linen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[LinenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Linen);
                    SafeNativeMethods.Gdip.ThreadData[LinenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Magenta
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MagentaKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Magenta);
                    SafeNativeMethods.Gdip.ThreadData[MagentaKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Maroon
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MaroonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Maroon);
                    SafeNativeMethods.Gdip.ThreadData[MaroonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumAquamarine
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumAquamarineKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumAquamarine);
                    SafeNativeMethods.Gdip.ThreadData[MediumAquamarineKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumBlue);
                    SafeNativeMethods.Gdip.ThreadData[MediumBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumOrchid
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumOrchidKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumOrchid);
                    SafeNativeMethods.Gdip.ThreadData[MediumOrchidKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumPurple
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumPurpleKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumPurple);
                    SafeNativeMethods.Gdip.ThreadData[MediumPurpleKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumSeaGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumSeaGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumSeaGreen);
                    SafeNativeMethods.Gdip.ThreadData[MediumSeaGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumSlateBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumSlateBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumSlateBlue);
                    SafeNativeMethods.Gdip.ThreadData[MediumSlateBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumSpringGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumSpringGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumSpringGreen);
                    SafeNativeMethods.Gdip.ThreadData[MediumSpringGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumTurquoise
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumTurquoiseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumTurquoise);
                    SafeNativeMethods.Gdip.ThreadData[MediumTurquoiseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MediumVioletRed
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MediumVioletRedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MediumVioletRed);
                    SafeNativeMethods.Gdip.ThreadData[MediumVioletRedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MidnightBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MidnightBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MidnightBlue);
                    SafeNativeMethods.Gdip.ThreadData[MidnightBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MintCream
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MintCreamKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MintCream);
                    SafeNativeMethods.Gdip.ThreadData[MintCreamKey] = brush;
                }
                return brush;
            }
        }

        public static Brush MistyRose
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MistyRoseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.MistyRose);
                    SafeNativeMethods.Gdip.ThreadData[MistyRoseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Moccasin
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[MoccasinKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Moccasin);
                    SafeNativeMethods.Gdip.ThreadData[MoccasinKey] = brush;
                }
                return brush;
            }
        }

        public static Brush NavajoWhite
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[NavajoWhiteKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.NavajoWhite);
                    SafeNativeMethods.Gdip.ThreadData[NavajoWhiteKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Navy
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[NavyKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Navy);
                    SafeNativeMethods.Gdip.ThreadData[NavyKey] = brush;
                }
                return brush;
            }
        }

        public static Brush OldLace
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OldLaceKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.OldLace);
                    SafeNativeMethods.Gdip.ThreadData[OldLaceKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Olive
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OliveKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Olive);
                    SafeNativeMethods.Gdip.ThreadData[OliveKey] = brush;
                }
                return brush;
            }
        }

        public static Brush OliveDrab
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OliveDrabKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.OliveDrab);
                    SafeNativeMethods.Gdip.ThreadData[OliveDrabKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Orange
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OrangeKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Orange);
                    SafeNativeMethods.Gdip.ThreadData[OrangeKey] = brush;
                }
                return brush;
            }
        }

        public static Brush OrangeRed
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OrangeRedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.OrangeRed);
                    SafeNativeMethods.Gdip.ThreadData[OrangeRedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Orchid
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[OrchidKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Orchid);
                    SafeNativeMethods.Gdip.ThreadData[OrchidKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PaleGoldenrod
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PaleGoldenrodKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PaleGoldenrod);
                    SafeNativeMethods.Gdip.ThreadData[PaleGoldenrodKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PaleGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PaleGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PaleGreen);
                    SafeNativeMethods.Gdip.ThreadData[PaleGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PaleTurquoise
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PaleTurquoiseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PaleTurquoise);
                    SafeNativeMethods.Gdip.ThreadData[PaleTurquoiseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PaleVioletRed
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PaleVioletRedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PaleVioletRed);
                    SafeNativeMethods.Gdip.ThreadData[PaleVioletRedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PapayaWhip
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PapayaWhipKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PapayaWhip);
                    SafeNativeMethods.Gdip.ThreadData[PapayaWhipKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PeachPuff
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PeachPuffKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PeachPuff);
                    SafeNativeMethods.Gdip.ThreadData[PeachPuffKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Peru
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PeruKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Peru);
                    SafeNativeMethods.Gdip.ThreadData[PeruKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Pink
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PinkKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Pink);
                    SafeNativeMethods.Gdip.ThreadData[PinkKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Plum
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PlumKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Plum);
                    SafeNativeMethods.Gdip.ThreadData[PlumKey] = brush;
                }
                return brush;
            }
        }

        public static Brush PowderBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PowderBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.PowderBlue);
                    SafeNativeMethods.Gdip.ThreadData[PowderBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Purple
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[PurpleKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Purple);
                    SafeNativeMethods.Gdip.ThreadData[PurpleKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Red
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[RedKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Red);
                    SafeNativeMethods.Gdip.ThreadData[RedKey] = brush;
                }
                return brush;
            }
        }

        public static Brush RosyBrown
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[RosyBrownKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.RosyBrown);
                    SafeNativeMethods.Gdip.ThreadData[RosyBrownKey] = brush;
                }
                return brush;
            }
        }

        public static Brush RoyalBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[RoyalBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.RoyalBlue);
                    SafeNativeMethods.Gdip.ThreadData[RoyalBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SaddleBrown
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SaddleBrownKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SaddleBrown);
                    SafeNativeMethods.Gdip.ThreadData[SaddleBrownKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Salmon
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SalmonKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Salmon);
                    SafeNativeMethods.Gdip.ThreadData[SalmonKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SandyBrown
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SandyBrownKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SandyBrown);
                    SafeNativeMethods.Gdip.ThreadData[SandyBrownKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SeaGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SeaGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SeaGreen);
                    SafeNativeMethods.Gdip.ThreadData[SeaGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SeaShell
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SeaShellKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SeaShell);
                    SafeNativeMethods.Gdip.ThreadData[SeaShellKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Sienna
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SiennaKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Sienna);
                    SafeNativeMethods.Gdip.ThreadData[SiennaKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Silver
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SilverKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Silver);
                    SafeNativeMethods.Gdip.ThreadData[SilverKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SkyBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SkyBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SkyBlue);
                    SafeNativeMethods.Gdip.ThreadData[SkyBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SlateBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SlateBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SlateBlue);
                    SafeNativeMethods.Gdip.ThreadData[SlateBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SlateGray
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SlateGrayKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SlateGray);
                    SafeNativeMethods.Gdip.ThreadData[SlateGrayKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Snow
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SnowKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Snow);
                    SafeNativeMethods.Gdip.ThreadData[SnowKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SpringGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SpringGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SpringGreen);
                    SafeNativeMethods.Gdip.ThreadData[SpringGreenKey] = brush;
                }
                return brush;
            }
        }

        public static Brush SteelBlue
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[SteelBlueKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.SteelBlue);
                    SafeNativeMethods.Gdip.ThreadData[SteelBlueKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Tan
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[TanKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Tan);
                    SafeNativeMethods.Gdip.ThreadData[TanKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Teal
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[TealKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Teal);
                    SafeNativeMethods.Gdip.ThreadData[TealKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Thistle
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[ThistleKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Thistle);
                    SafeNativeMethods.Gdip.ThreadData[ThistleKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Tomato
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[TomatoKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Tomato);
                    SafeNativeMethods.Gdip.ThreadData[TomatoKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Transparent
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[TransparentKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Transparent);
                    SafeNativeMethods.Gdip.ThreadData[TransparentKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Turquoise
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[TurquoiseKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Turquoise);
                    SafeNativeMethods.Gdip.ThreadData[TurquoiseKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Violet
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[VioletKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Violet);
                    SafeNativeMethods.Gdip.ThreadData[VioletKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Wheat
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[WheatKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Wheat);
                    SafeNativeMethods.Gdip.ThreadData[WheatKey] = brush;
                }
                return brush;
            }
        }

        public static Brush White
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[WhiteKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.White);
                    SafeNativeMethods.Gdip.ThreadData[WhiteKey] = brush;
                }
                return brush;
            }
        }

        public static Brush WhiteSmoke
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[WhiteSmokeKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.WhiteSmoke);
                    SafeNativeMethods.Gdip.ThreadData[WhiteSmokeKey] = brush;
                }
                return brush;
            }
        }

        public static Brush Yellow
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[YellowKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.Yellow);
                    SafeNativeMethods.Gdip.ThreadData[YellowKey] = brush;
                }
                return brush;
            }
        }

        public static Brush YellowGreen
        {
            get
            {
                Brush brush = (Brush) SafeNativeMethods.Gdip.ThreadData[YellowGreenKey];
                if (brush == null)
                {
                    brush = new SolidBrush(Color.YellowGreen);
                    SafeNativeMethods.Gdip.ThreadData[YellowGreenKey] = brush;
                }
                return brush;
            }
        }
    }
}

