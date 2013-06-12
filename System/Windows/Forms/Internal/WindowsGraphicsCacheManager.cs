namespace System.Windows.Forms.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    internal class WindowsGraphicsCacheManager
    {
        private const int CacheSize = 10;
        [ThreadStatic]
        private static int currentIndex;
        [ThreadStatic]
        private static WindowsGraphics measurementGraphics;
        [ThreadStatic]
        private static List<KeyValuePair<Font, WindowsFont>> windowsFontCache;

        private WindowsGraphicsCacheManager()
        {
        }

        internal static WindowsGraphics GetCurrentMeasurementGraphics()
        {
            return measurementGraphics;
        }

        public static WindowsFont GetWindowsFont(Font font)
        {
            return GetWindowsFont(font, WindowsFontQuality.Default);
        }

        public static WindowsFont GetWindowsFont(Font font, WindowsFontQuality fontQuality)
        {
            if (font == null)
            {
                return null;
            }
            int num = 0;
            int currentIndex = WindowsGraphicsCacheManager.currentIndex;
            while (num < WindowsFontCache.Count)
            {
                KeyValuePair<Font, WindowsFont> pair2 = WindowsFontCache[currentIndex];
                if (pair2.Key.Equals(font))
                {
                    KeyValuePair<Font, WindowsFont> pair3 = WindowsFontCache[currentIndex];
                    WindowsFont font2 = pair3.Value;
                    if (font2.Quality == fontQuality)
                    {
                        return font2;
                    }
                }
                currentIndex--;
                num++;
                if (currentIndex < 0)
                {
                    currentIndex = 9;
                }
            }
            WindowsFont font3 = WindowsFont.FromFont(font, fontQuality);
            KeyValuePair<Font, WindowsFont> item = new KeyValuePair<Font, WindowsFont>(font, font3);
            WindowsGraphicsCacheManager.currentIndex++;
            if (WindowsGraphicsCacheManager.currentIndex == 10)
            {
                WindowsGraphicsCacheManager.currentIndex = 0;
            }
            if (WindowsFontCache.Count != 10)
            {
                font3.OwnedByCacheManager = true;
                WindowsFontCache.Add(item);
                return font3;
            }
            WindowsFont wf = null;
            bool flag = false;
            int num3 = WindowsGraphicsCacheManager.currentIndex;
            int num4 = num3 + 1;
            while (!flag)
            {
                if (num4 >= 10)
                {
                    num4 = 0;
                }
                if (num4 == num3)
                {
                    flag = true;
                }
                KeyValuePair<Font, WindowsFont> pair4 = WindowsFontCache[num4];
                wf = pair4.Value;
                if (!DeviceContexts.IsFontInUse(wf))
                {
                    WindowsGraphicsCacheManager.currentIndex = num4;
                    flag = true;
                    break;
                }
                num4++;
                wf = null;
            }
            if (wf != null)
            {
                WindowsFontCache[WindowsGraphicsCacheManager.currentIndex] = item;
                font3.OwnedByCacheManager = true;
                wf.OwnedByCacheManager = false;
                wf.Dispose();
                return font3;
            }
            font3.OwnedByCacheManager = false;
            return font3;
        }

        public static WindowsGraphics MeasurementGraphics
        {
            get
            {
                if ((measurementGraphics == null) || (measurementGraphics.DeviceContext == null))
                {
                    measurementGraphics = WindowsGraphics.CreateMeasurementWindowsGraphics();
                }
                return measurementGraphics;
            }
        }

        private static List<KeyValuePair<Font, WindowsFont>> WindowsFontCache
        {
            get
            {
                if (windowsFontCache == null)
                {
                    currentIndex = -1;
                    windowsFontCache = new List<KeyValuePair<Font, WindowsFont>>(10);
                }
                return windowsFontCache;
            }
        }
    }
}

