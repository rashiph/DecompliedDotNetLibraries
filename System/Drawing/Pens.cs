namespace System.Drawing
{
    using System;

    public sealed class Pens
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

        private Pens()
        {
        }

        public static Pen AliceBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[AliceBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.AliceBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[AliceBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen AntiqueWhite
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[AntiqueWhiteKey];
                if (pen == null)
                {
                    pen = new Pen(Color.AntiqueWhite, true);
                    SafeNativeMethods.Gdip.ThreadData[AntiqueWhiteKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Aqua
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[AquaKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Aqua, true);
                    SafeNativeMethods.Gdip.ThreadData[AquaKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Aquamarine
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[AquamarineKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Aquamarine, true);
                    SafeNativeMethods.Gdip.ThreadData[AquamarineKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Azure
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[AzureKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Azure, true);
                    SafeNativeMethods.Gdip.ThreadData[AzureKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Beige
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BeigeKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Beige, true);
                    SafeNativeMethods.Gdip.ThreadData[BeigeKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Bisque
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BisqueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Bisque, true);
                    SafeNativeMethods.Gdip.ThreadData[BisqueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Black
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BlackKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Black, true);
                    SafeNativeMethods.Gdip.ThreadData[BlackKey] = pen;
                }
                return pen;
            }
        }

        public static Pen BlanchedAlmond
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BlanchedAlmondKey];
                if (pen == null)
                {
                    pen = new Pen(Color.BlanchedAlmond, true);
                    SafeNativeMethods.Gdip.ThreadData[BlanchedAlmondKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Blue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Blue, true);
                    SafeNativeMethods.Gdip.ThreadData[BlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen BlueViolet
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BlueVioletKey];
                if (pen == null)
                {
                    pen = new Pen(Color.BlueViolet, true);
                    SafeNativeMethods.Gdip.ThreadData[BlueVioletKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Brown
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BrownKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Brown, true);
                    SafeNativeMethods.Gdip.ThreadData[BrownKey] = pen;
                }
                return pen;
            }
        }

        public static Pen BurlyWood
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[BurlyWoodKey];
                if (pen == null)
                {
                    pen = new Pen(Color.BurlyWood, true);
                    SafeNativeMethods.Gdip.ThreadData[BurlyWoodKey] = pen;
                }
                return pen;
            }
        }

        public static Pen CadetBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[CadetBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.CadetBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[CadetBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Chartreuse
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[ChartreuseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Chartreuse, true);
                    SafeNativeMethods.Gdip.ThreadData[ChartreuseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Chocolate
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[ChocolateKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Chocolate, true);
                    SafeNativeMethods.Gdip.ThreadData[ChocolateKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Coral
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[ChoralKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Coral, true);
                    SafeNativeMethods.Gdip.ThreadData[ChoralKey] = pen;
                }
                return pen;
            }
        }

        public static Pen CornflowerBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[CornflowerBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.CornflowerBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[CornflowerBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Cornsilk
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[CornsilkKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Cornsilk, true);
                    SafeNativeMethods.Gdip.ThreadData[CornsilkKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Crimson
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[CrimsonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Crimson, true);
                    SafeNativeMethods.Gdip.ThreadData[CrimsonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Cyan
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[CyanKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Cyan, true);
                    SafeNativeMethods.Gdip.ThreadData[CyanKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkCyan
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkCyanKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkCyan, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkCyanKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkGoldenrod
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkGoldenrodKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkGoldenrod, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkGoldenrodKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkGray, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkKhaki
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkKhakiKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkKhaki, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkKhakiKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkMagenta
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkMagentaKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkMagenta, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkMagentaKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkOliveGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkOliveGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkOliveGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkOliveGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkOrange
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkOrangeKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkOrange, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkOrangeKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkOrchid
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkOrchidKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkOrchid, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkOrchidKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkRed
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkRedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkRed, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkRedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkSalmon
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkSalmonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkSalmon, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkSalmonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkSeaGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkSeaGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkSeaGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkSeaGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkSlateBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkSlateBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkSlateBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkSlateBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkSlateGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkSlateGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkSlateGray, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkSlateGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkTurquoise
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkTurquoiseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkTurquoise, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkTurquoiseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DarkViolet
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DarkVioletKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DarkViolet, true);
                    SafeNativeMethods.Gdip.ThreadData[DarkVioletKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DeepPink
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DeepPinkKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DeepPink, true);
                    SafeNativeMethods.Gdip.ThreadData[DeepPinkKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DeepSkyBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DeepSkyBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DeepSkyBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[DeepSkyBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DimGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DimGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DimGray, true);
                    SafeNativeMethods.Gdip.ThreadData[DimGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen DodgerBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[DodgerBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.DodgerBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[DodgerBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Firebrick
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[FirebrickKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Firebrick, true);
                    SafeNativeMethods.Gdip.ThreadData[FirebrickKey] = pen;
                }
                return pen;
            }
        }

        public static Pen FloralWhite
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[FloralWhiteKey];
                if (pen == null)
                {
                    pen = new Pen(Color.FloralWhite, true);
                    SafeNativeMethods.Gdip.ThreadData[FloralWhiteKey] = pen;
                }
                return pen;
            }
        }

        public static Pen ForestGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[ForestGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.ForestGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[ForestGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Fuchsia
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[FuchiaKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Fuchsia, true);
                    SafeNativeMethods.Gdip.ThreadData[FuchiaKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Gainsboro
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GainsboroKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Gainsboro, true);
                    SafeNativeMethods.Gdip.ThreadData[GainsboroKey] = pen;
                }
                return pen;
            }
        }

        public static Pen GhostWhite
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GhostWhiteKey];
                if (pen == null)
                {
                    pen = new Pen(Color.GhostWhite, true);
                    SafeNativeMethods.Gdip.ThreadData[GhostWhiteKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Gold
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GoldKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Gold, true);
                    SafeNativeMethods.Gdip.ThreadData[GoldKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Goldenrod
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GoldenrodKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Goldenrod, true);
                    SafeNativeMethods.Gdip.ThreadData[GoldenrodKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Gray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Gray, true);
                    SafeNativeMethods.Gdip.ThreadData[GrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Green
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Green, true);
                    SafeNativeMethods.Gdip.ThreadData[GreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen GreenYellow
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[GreenYellowKey];
                if (pen == null)
                {
                    pen = new Pen(Color.GreenYellow, true);
                    SafeNativeMethods.Gdip.ThreadData[GreenYellowKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Honeydew
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[HoneydewKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Honeydew, true);
                    SafeNativeMethods.Gdip.ThreadData[HoneydewKey] = pen;
                }
                return pen;
            }
        }

        public static Pen HotPink
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[HotPinkKey];
                if (pen == null)
                {
                    pen = new Pen(Color.HotPink, true);
                    SafeNativeMethods.Gdip.ThreadData[HotPinkKey] = pen;
                }
                return pen;
            }
        }

        public static Pen IndianRed
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[IndianRedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.IndianRed, true);
                    SafeNativeMethods.Gdip.ThreadData[IndianRedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Indigo
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[IndigoKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Indigo, true);
                    SafeNativeMethods.Gdip.ThreadData[IndigoKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Ivory
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[IvoryKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Ivory, true);
                    SafeNativeMethods.Gdip.ThreadData[IvoryKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Khaki
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[KhakiKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Khaki, true);
                    SafeNativeMethods.Gdip.ThreadData[KhakiKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Lavender
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LavenderKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Lavender, true);
                    SafeNativeMethods.Gdip.ThreadData[LavenderKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LavenderBlush
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LavenderBlushKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LavenderBlush, true);
                    SafeNativeMethods.Gdip.ThreadData[LavenderBlushKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LawnGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LawnGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LawnGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[LawnGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LemonChiffon
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LemonChiffonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LemonChiffon, true);
                    SafeNativeMethods.Gdip.ThreadData[LemonChiffonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[LightBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightCoral
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightCoralKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightCoral, true);
                    SafeNativeMethods.Gdip.ThreadData[LightCoralKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightCyan
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightCyanKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightCyan, true);
                    SafeNativeMethods.Gdip.ThreadData[LightCyanKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightGoldenrodYellow
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightGoldenrodYellowKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightGoldenrodYellow, true);
                    SafeNativeMethods.Gdip.ThreadData[LightGoldenrodYellowKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightGray, true);
                    SafeNativeMethods.Gdip.ThreadData[LightGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[LightGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightPink
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightPinkKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightPink, true);
                    SafeNativeMethods.Gdip.ThreadData[LightPinkKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightSalmon
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightSalmonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightSalmon, true);
                    SafeNativeMethods.Gdip.ThreadData[LightSalmonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightSeaGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightSeaGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightSeaGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[LightSeaGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightSkyBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightSkyBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightSkyBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[LightSkyBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightSlateGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightSlateGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightSlateGray, true);
                    SafeNativeMethods.Gdip.ThreadData[LightSlateGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightSteelBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightSteelBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightSteelBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[LightSteelBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LightYellow
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LightYellowKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LightYellow, true);
                    SafeNativeMethods.Gdip.ThreadData[LightYellowKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Lime
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LimeKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Lime, true);
                    SafeNativeMethods.Gdip.ThreadData[LimeKey] = pen;
                }
                return pen;
            }
        }

        public static Pen LimeGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LimeGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.LimeGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[LimeGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Linen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[LinenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Linen, true);
                    SafeNativeMethods.Gdip.ThreadData[LinenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Magenta
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MagentaKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Magenta, true);
                    SafeNativeMethods.Gdip.ThreadData[MagentaKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Maroon
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MaroonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Maroon, true);
                    SafeNativeMethods.Gdip.ThreadData[MaroonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumAquamarine
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumAquamarineKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumAquamarine, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumAquamarineKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumOrchid
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumOrchidKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumOrchid, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumOrchidKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumPurple
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumPurpleKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumPurple, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumPurpleKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumSeaGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumSeaGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumSeaGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumSeaGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumSlateBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumSlateBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumSlateBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumSlateBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumSpringGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumSpringGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumSpringGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumSpringGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumTurquoise
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumTurquoiseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumTurquoise, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumTurquoiseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MediumVioletRed
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MediumVioletRedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MediumVioletRed, true);
                    SafeNativeMethods.Gdip.ThreadData[MediumVioletRedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MidnightBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MidnightBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MidnightBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[MidnightBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MintCream
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MintCreamKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MintCream, true);
                    SafeNativeMethods.Gdip.ThreadData[MintCreamKey] = pen;
                }
                return pen;
            }
        }

        public static Pen MistyRose
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MistyRoseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.MistyRose, true);
                    SafeNativeMethods.Gdip.ThreadData[MistyRoseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Moccasin
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[MoccasinKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Moccasin, true);
                    SafeNativeMethods.Gdip.ThreadData[MoccasinKey] = pen;
                }
                return pen;
            }
        }

        public static Pen NavajoWhite
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[NavajoWhiteKey];
                if (pen == null)
                {
                    pen = new Pen(Color.NavajoWhite, true);
                    SafeNativeMethods.Gdip.ThreadData[NavajoWhiteKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Navy
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[NavyKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Navy, true);
                    SafeNativeMethods.Gdip.ThreadData[NavyKey] = pen;
                }
                return pen;
            }
        }

        public static Pen OldLace
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OldLaceKey];
                if (pen == null)
                {
                    pen = new Pen(Color.OldLace, true);
                    SafeNativeMethods.Gdip.ThreadData[OldLaceKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Olive
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OliveKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Olive, true);
                    SafeNativeMethods.Gdip.ThreadData[OliveKey] = pen;
                }
                return pen;
            }
        }

        public static Pen OliveDrab
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OliveDrabKey];
                if (pen == null)
                {
                    pen = new Pen(Color.OliveDrab, true);
                    SafeNativeMethods.Gdip.ThreadData[OliveDrabKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Orange
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OrangeKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Orange, true);
                    SafeNativeMethods.Gdip.ThreadData[OrangeKey] = pen;
                }
                return pen;
            }
        }

        public static Pen OrangeRed
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OrangeRedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.OrangeRed, true);
                    SafeNativeMethods.Gdip.ThreadData[OrangeRedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Orchid
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[OrchidKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Orchid, true);
                    SafeNativeMethods.Gdip.ThreadData[OrchidKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PaleGoldenrod
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PaleGoldenrodKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PaleGoldenrod, true);
                    SafeNativeMethods.Gdip.ThreadData[PaleGoldenrodKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PaleGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PaleGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PaleGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[PaleGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PaleTurquoise
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PaleTurquoiseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PaleTurquoise, true);
                    SafeNativeMethods.Gdip.ThreadData[PaleTurquoiseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PaleVioletRed
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PaleVioletRedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PaleVioletRed, true);
                    SafeNativeMethods.Gdip.ThreadData[PaleVioletRedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PapayaWhip
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PapayaWhipKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PapayaWhip, true);
                    SafeNativeMethods.Gdip.ThreadData[PapayaWhipKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PeachPuff
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PeachPuffKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PeachPuff, true);
                    SafeNativeMethods.Gdip.ThreadData[PeachPuffKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Peru
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PeruKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Peru, true);
                    SafeNativeMethods.Gdip.ThreadData[PeruKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Pink
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PinkKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Pink, true);
                    SafeNativeMethods.Gdip.ThreadData[PinkKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Plum
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PlumKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Plum, true);
                    SafeNativeMethods.Gdip.ThreadData[PlumKey] = pen;
                }
                return pen;
            }
        }

        public static Pen PowderBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PowderBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.PowderBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[PowderBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Purple
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[PurpleKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Purple, true);
                    SafeNativeMethods.Gdip.ThreadData[PurpleKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Red
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[RedKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Red, true);
                    SafeNativeMethods.Gdip.ThreadData[RedKey] = pen;
                }
                return pen;
            }
        }

        public static Pen RosyBrown
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[RosyBrownKey];
                if (pen == null)
                {
                    pen = new Pen(Color.RosyBrown, true);
                    SafeNativeMethods.Gdip.ThreadData[RosyBrownKey] = pen;
                }
                return pen;
            }
        }

        public static Pen RoyalBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[RoyalBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.RoyalBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[RoyalBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SaddleBrown
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SaddleBrownKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SaddleBrown, true);
                    SafeNativeMethods.Gdip.ThreadData[SaddleBrownKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Salmon
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SalmonKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Salmon, true);
                    SafeNativeMethods.Gdip.ThreadData[SalmonKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SandyBrown
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SandyBrownKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SandyBrown, true);
                    SafeNativeMethods.Gdip.ThreadData[SandyBrownKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SeaGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SeaGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SeaGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[SeaGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SeaShell
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SeaShellKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SeaShell, true);
                    SafeNativeMethods.Gdip.ThreadData[SeaShellKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Sienna
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SiennaKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Sienna, true);
                    SafeNativeMethods.Gdip.ThreadData[SiennaKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Silver
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SilverKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Silver, true);
                    SafeNativeMethods.Gdip.ThreadData[SilverKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SkyBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SkyBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SkyBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[SkyBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SlateBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SlateBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SlateBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[SlateBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SlateGray
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SlateGrayKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SlateGray, true);
                    SafeNativeMethods.Gdip.ThreadData[SlateGrayKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Snow
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SnowKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Snow, true);
                    SafeNativeMethods.Gdip.ThreadData[SnowKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SpringGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SpringGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SpringGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[SpringGreenKey] = pen;
                }
                return pen;
            }
        }

        public static Pen SteelBlue
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[SteelBlueKey];
                if (pen == null)
                {
                    pen = new Pen(Color.SteelBlue, true);
                    SafeNativeMethods.Gdip.ThreadData[SteelBlueKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Tan
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[TanKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Tan, true);
                    SafeNativeMethods.Gdip.ThreadData[TanKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Teal
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[TealKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Teal, true);
                    SafeNativeMethods.Gdip.ThreadData[TealKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Thistle
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[ThistleKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Thistle, true);
                    SafeNativeMethods.Gdip.ThreadData[ThistleKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Tomato
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[TomatoKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Tomato, true);
                    SafeNativeMethods.Gdip.ThreadData[TomatoKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Transparent
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[TransparentKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Transparent, true);
                    SafeNativeMethods.Gdip.ThreadData[TransparentKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Turquoise
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[TurquoiseKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Turquoise, true);
                    SafeNativeMethods.Gdip.ThreadData[TurquoiseKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Violet
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[VioletKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Violet, true);
                    SafeNativeMethods.Gdip.ThreadData[VioletKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Wheat
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[WheatKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Wheat, true);
                    SafeNativeMethods.Gdip.ThreadData[WheatKey] = pen;
                }
                return pen;
            }
        }

        public static Pen White
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[WhiteKey];
                if (pen == null)
                {
                    pen = new Pen(Color.White, true);
                    SafeNativeMethods.Gdip.ThreadData[WhiteKey] = pen;
                }
                return pen;
            }
        }

        public static Pen WhiteSmoke
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[WhiteSmokeKey];
                if (pen == null)
                {
                    pen = new Pen(Color.WhiteSmoke, true);
                    SafeNativeMethods.Gdip.ThreadData[WhiteSmokeKey] = pen;
                }
                return pen;
            }
        }

        public static Pen Yellow
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[YellowKey];
                if (pen == null)
                {
                    pen = new Pen(Color.Yellow, true);
                    SafeNativeMethods.Gdip.ThreadData[YellowKey] = pen;
                }
                return pen;
            }
        }

        public static Pen YellowGreen
        {
            get
            {
                Pen pen = (Pen) SafeNativeMethods.Gdip.ThreadData[YellowGreenKey];
                if (pen == null)
                {
                    pen = new Pen(Color.YellowGreen, true);
                    SafeNativeMethods.Gdip.ThreadData[YellowGreenKey] = pen;
                }
                return pen;
            }
        }
    }
}

