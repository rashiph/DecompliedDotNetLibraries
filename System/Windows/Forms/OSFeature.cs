namespace System.Windows.Forms
{
    using System;

    public class OSFeature : FeatureSupport
    {
        private static OSFeature feature = null;
        public static readonly object LayeredWindows = new object();
        public static readonly object Themes = new object();
        private static bool themeSupport = false;
        private static bool themeSupportTested = false;

        protected OSFeature()
        {
        }

        public override Version GetVersionPresent(object feature)
        {
            Version version = null;
            if (feature == LayeredWindows)
            {
                if ((Environment.OSVersion.Platform == PlatformID.Win32NT) && (Environment.OSVersion.Version.CompareTo(new Version(5, 0, 0, 0)) >= 0))
                {
                    version = new Version(0, 0, 0, 0);
                }
                return version;
            }
            if (feature == Themes)
            {
                if (!themeSupportTested)
                {
                    try
                    {
                        SafeNativeMethods.IsAppThemed();
                        themeSupport = true;
                    }
                    catch
                    {
                        themeSupport = false;
                    }
                    themeSupportTested = true;
                }
                if (themeSupport)
                {
                    version = new Version(0, 0, 0, 0);
                }
            }
            return version;
        }

        public static bool IsPresent(SystemParameter enumVal)
        {
            switch (enumVal)
            {
                case SystemParameter.DropShadow:
                    return Feature.OnXp;

                case SystemParameter.FlatMenu:
                    return Feature.OnXp;

                case SystemParameter.FontSmoothingContrastMetric:
                    return Feature.OnXp;

                case SystemParameter.FontSmoothingTypeMetric:
                    return Feature.OnXp;

                case SystemParameter.MenuFadeEnabled:
                    return Feature.OnWin2k;

                case SystemParameter.SelectionFade:
                    return Feature.OnWin2k;

                case SystemParameter.ToolTipAnimationMetric:
                    return Feature.OnWin2k;

                case SystemParameter.UIEffects:
                    return Feature.OnWin2k;

                case SystemParameter.CaretWidthMetric:
                    return Feature.OnWin2k;

                case SystemParameter.VerticalFocusThicknessMetric:
                    return Feature.OnXp;

                case SystemParameter.HorizontalFocusThicknessMetric:
                    return Feature.OnXp;
            }
            return false;
        }

        public static OSFeature Feature
        {
            get
            {
                if (feature == null)
                {
                    feature = new OSFeature();
                }
                return feature;
            }
        }

        internal bool OnWin2k
        {
            get
            {
                bool flag = false;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    flag = Environment.OSVersion.Version.CompareTo(new Version(5, 0, 0, 0)) >= 0;
                }
                return flag;
            }
        }

        internal bool OnXp
        {
            get
            {
                bool flag = false;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    flag = Environment.OSVersion.Version.CompareTo(new Version(5, 1, 0, 0)) >= 0;
                }
                return flag;
            }
        }
    }
}

