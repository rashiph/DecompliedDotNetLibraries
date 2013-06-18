namespace System.Windows.Forms.Internal
{
    using System;

    internal static class MeasurementDCInfo
    {
        [ThreadStatic]
        private static CachedInfo cachedMeasurementDCInfo;

        internal static IntNativeMethods.DRAWTEXTPARAMS GetTextMargins(WindowsGraphics wg, WindowsFont font)
        {
            CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
            if (((cachedMeasurementDCInfo == null) || (cachedMeasurementDCInfo.LeftTextMargin <= 0)) || ((cachedMeasurementDCInfo.RightTextMargin <= 0) || (font != cachedMeasurementDCInfo.LastUsedFont)))
            {
                if (cachedMeasurementDCInfo == null)
                {
                    cachedMeasurementDCInfo = new CachedInfo();
                    MeasurementDCInfo.cachedMeasurementDCInfo = cachedMeasurementDCInfo;
                }
                IntNativeMethods.DRAWTEXTPARAMS textMargins = wg.GetTextMargins(font);
                cachedMeasurementDCInfo.LeftTextMargin = textMargins.iLeftMargin;
                cachedMeasurementDCInfo.RightTextMargin = textMargins.iRightMargin;
            }
            return new IntNativeMethods.DRAWTEXTPARAMS(cachedMeasurementDCInfo.LeftTextMargin, cachedMeasurementDCInfo.RightTextMargin);
        }

        internal static bool IsMeasurementDC(DeviceContext dc)
        {
            WindowsGraphics currentMeasurementGraphics = WindowsGraphicsCacheManager.GetCurrentMeasurementGraphics();
            return (((currentMeasurementGraphics != null) && (currentMeasurementGraphics.DeviceContext != null)) && (currentMeasurementGraphics.DeviceContext.Hdc == dc.Hdc));
        }

        internal static void Reset()
        {
            CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
            if (cachedMeasurementDCInfo != null)
            {
                cachedMeasurementDCInfo.UpdateFont(null);
            }
        }

        internal static void ResetIfIsMeasurementDC(IntPtr hdc)
        {
            WindowsGraphics currentMeasurementGraphics = WindowsGraphicsCacheManager.GetCurrentMeasurementGraphics();
            if (((currentMeasurementGraphics != null) && (currentMeasurementGraphics.DeviceContext != null)) && (currentMeasurementGraphics.DeviceContext.Hdc == hdc))
            {
                CachedInfo cachedMeasurementDCInfo = MeasurementDCInfo.cachedMeasurementDCInfo;
                if (cachedMeasurementDCInfo != null)
                {
                    cachedMeasurementDCInfo.UpdateFont(null);
                }
            }
        }

        internal static WindowsFont LastUsedFont
        {
            get
            {
                if (cachedMeasurementDCInfo != null)
                {
                    return cachedMeasurementDCInfo.LastUsedFont;
                }
                return null;
            }
            set
            {
                if (cachedMeasurementDCInfo == null)
                {
                    cachedMeasurementDCInfo = new CachedInfo();
                }
                cachedMeasurementDCInfo.UpdateFont(value);
            }
        }

        private sealed class CachedInfo
        {
            public WindowsFont LastUsedFont;
            public int LeftTextMargin;
            public int RightTextMargin;

            internal void UpdateFont(WindowsFont font)
            {
                if (this.LastUsedFont != font)
                {
                    this.LastUsedFont = font;
                    this.LeftTextMargin = -1;
                    this.RightTextMargin = -1;
                }
            }
        }
    }
}

