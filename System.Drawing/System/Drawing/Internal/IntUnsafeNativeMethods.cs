namespace System.Drawing.Internal
{
    using System;
    using System.Internal;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class IntUnsafeNativeMethods
    {
        public static bool AngleArc(HandleRef hDC, int x, int y, int radius, float startAngle, float endAngle)
        {
            return IntAngleArc(hDC, x, y, radius, startAngle, endAngle);
        }

        public static bool Arc(HandleRef hDC, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nXStartArc, int nYStartArc, int nXEndArc, int nYEndArc)
        {
            return IntArc(hDC, nLeftRect, nTopRect, nRightRect, nBottomRect, nXStartArc, nYStartArc, nXEndArc, nYEndArc);
        }

        public static bool BeginPath(HandleRef hDC)
        {
            return IntBeginPath(hDC);
        }

        public static IntNativeMethods.RegionFlags CombineRgn(HandleRef hRgnDest, HandleRef hRgnSrc1, HandleRef hRgnSrc2, RegionCombineMode combineMode)
        {
            if (((hRgnDest.Wrapper != null) && (hRgnSrc1.Wrapper != null)) && (hRgnSrc2.Wrapper != null))
            {
                return IntCombineRgn(hRgnDest, hRgnSrc1, hRgnSrc2, combineMode);
            }
            return IntNativeMethods.RegionFlags.ERROR;
        }

        public static IntPtr CreateCompatibleDC(HandleRef hDC)
        {
            return System.Internal.HandleCollector.Add(IntCreateCompatibleDC(hDC), IntSafeNativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateDC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData)
        {
            return System.Internal.HandleCollector.Add(IntCreateDC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData), IntSafeNativeMethods.CommonHandles.HDC);
        }

        public static IntPtr CreateFontIndirect(object lf)
        {
            return System.Internal.HandleCollector.Add(IntCreateFontIndirect(lf), IntSafeNativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateIC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData)
        {
            return System.Internal.HandleCollector.Add(IntCreateIC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData), IntSafeNativeMethods.CommonHandles.HDC);
        }

        public static bool DeleteDC(HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, IntSafeNativeMethods.CommonHandles.GDI);
            return IntDeleteDC(hDC);
        }

        public static bool DeleteHDC(HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, IntSafeNativeMethods.CommonHandles.HDC);
            return IntDeleteDC(hDC);
        }

        public static bool DeleteObject(HandleRef hObject)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hObject, IntSafeNativeMethods.CommonHandles.GDI);
            return IntDeleteObject(hObject);
        }

        public static int DrawText(HandleRef hDC, string text, ref IntNativeMethods.RECT lpRect, int nFormat)
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                lpRect.top = Math.Min(0x7fff, lpRect.top);
                lpRect.left = Math.Min(0x7fff, lpRect.left);
                lpRect.right = Math.Min(0x7fff, lpRect.right);
                lpRect.bottom = Math.Min(0x7fff, lpRect.bottom);
                int num2 = WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                byte[] pOutBytes = new byte[num2];
                WideCharToMultiByte(0, 0, text, text.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                num2 = Math.Min(num2, 0x2000);
                return DrawTextA(hDC, pOutBytes, num2, ref lpRect, nFormat);
            }
            return DrawTextW(hDC, text, text.Length, ref lpRect, nFormat);
        }

        [DllImport("user32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        public static extern int DrawTextA(HandleRef hDC, byte[] lpszString, int byteCount, ref IntNativeMethods.RECT lpRect, int nFormat);
        public static int DrawTextEx(HandleRef hDC, string text, ref IntNativeMethods.RECT lpRect, int nFormat, [In, Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams)
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                lpRect.top = Math.Min(0x7fff, lpRect.top);
                lpRect.left = Math.Min(0x7fff, lpRect.left);
                lpRect.right = Math.Min(0x7fff, lpRect.right);
                lpRect.bottom = Math.Min(0x7fff, lpRect.bottom);
                int num2 = WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero);
                byte[] pOutBytes = new byte[num2];
                WideCharToMultiByte(0, 0, text, text.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                num2 = Math.Min(num2, 0x2000);
                return DrawTextExA(hDC, pOutBytes, num2, ref lpRect, nFormat, lpDTParams);
            }
            return DrawTextExW(hDC, text, text.Length, ref lpRect, nFormat, lpDTParams);
        }

        [DllImport("user32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern int DrawTextExA(HandleRef hDC, byte[] lpszString, int byteCount, ref IntNativeMethods.RECT lpRect, int nFormat, [In, Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams);
        [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int DrawTextExW(HandleRef hDC, string lpszString, int nCount, ref IntNativeMethods.RECT lpRect, int nFormat, [In, Out] IntNativeMethods.DRAWTEXTPARAMS lpDTParams);
        [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        public static extern int DrawTextW(HandleRef hDC, string lpszString, int nCount, ref IntNativeMethods.RECT lpRect, int nFormat);
        public static bool Ellipse(HandleRef hDc, int x1, int y1, int x2, int y2)
        {
            return IntEllipse(hDc, x1, y1, x2, y2);
        }

        public static bool EndPath(HandleRef hDC)
        {
            return IntEndPath(hDC);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool ExtTextOut(HandleRef hdc, int x, int y, int options, ref IntNativeMethods.RECT rect, string str, int length, int[] spacing);
        public static bool FillRect(HandleRef hDC, [In] ref IntNativeMethods.RECT rect, HandleRef hbrush)
        {
            return IntFillRect(hDC, ref rect, hbrush);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetBkColor(HandleRef hDC);
        public static int GetBkMode(HandleRef hDC)
        {
            return IntGetBkMode(hDC);
        }

        public static int GetClipRgn(HandleRef hDC, HandleRef hRgn)
        {
            return IntGetClipRgn(hDC, hRgn);
        }

        public static IntPtr GetCurrentObject(HandleRef hDC, int uObjectType)
        {
            return IntGetCurrentObject(hDC, uObjectType);
        }

        public static IntPtr GetDC(HandleRef hWnd)
        {
            return System.Internal.HandleCollector.Add(IntGetDC(hWnd), IntSafeNativeMethods.CommonHandles.HDC);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetGraphicsMode(HandleRef hDC);
        public static int GetMapMode(HandleRef hDC)
        {
            return IntGetMapMode(hDC);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetNearestColor(HandleRef hDC, int color);
        public static int GetObject(HandleRef hBrush, IntNativeMethods.LOGBRUSH lb)
        {
            return IntGetObject(hBrush, Marshal.SizeOf(typeof(IntNativeMethods.LOGBRUSH)), lb);
        }

        public static int GetObject(HandleRef hFont, IntNativeMethods.LOGFONT lp)
        {
            return IntGetObject(hFont, Marshal.SizeOf(typeof(IntNativeMethods.LOGFONT)), lp);
        }

        public static IntNativeMethods.RegionFlags GetRgnBox(HandleRef hRgn, [In, Out] ref IntNativeMethods.RECT clipRect)
        {
            return IntGetRgnBox(hRgn, ref clipRect);
        }

        [DllImport("gdi32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern int GetROP2(HandleRef hdc);
        public static IntPtr GetStockObject(int nIndex)
        {
            return IntGetStockObject(nIndex);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextAlign(HandleRef hdc);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextColor(HandleRef hDC);
        public static int GetTextExtentPoint32(HandleRef hDC, string text, [In, Out] IntNativeMethods.SIZE size)
        {
            int length = text.Length;
            if (Marshal.SystemDefaultCharSize == 1)
            {
                byte[] pOutBytes = new byte[WideCharToMultiByte(0, 0, text, text.Length, null, 0, IntPtr.Zero, IntPtr.Zero)];
                WideCharToMultiByte(0, 0, text, text.Length, pOutBytes, pOutBytes.Length, IntPtr.Zero, IntPtr.Zero);
                length = Math.Min(text.Length, 0x2000);
                return GetTextExtentPoint32A(hDC, pOutBytes, length, size);
            }
            return GetTextExtentPoint32W(hDC, text, text.Length, size);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextExtentPoint32A(HandleRef hDC, byte[] lpszString, int byteCount, [In, Out] IntNativeMethods.SIZE size);
        [DllImport("gdi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextExtentPoint32W(HandleRef hDC, string text, int len, [In, Out] IntNativeMethods.SIZE size);
        public static int GetTextMetrics(HandleRef hDC, ref IntNativeMethods.TEXTMETRIC lptm)
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                IntNativeMethods.TEXTMETRICA textmetrica = new IntNativeMethods.TEXTMETRICA();
                int textMetricsA = GetTextMetricsA(hDC, ref textmetrica);
                lptm.tmHeight = textmetrica.tmHeight;
                lptm.tmAscent = textmetrica.tmAscent;
                lptm.tmDescent = textmetrica.tmDescent;
                lptm.tmInternalLeading = textmetrica.tmInternalLeading;
                lptm.tmExternalLeading = textmetrica.tmExternalLeading;
                lptm.tmAveCharWidth = textmetrica.tmAveCharWidth;
                lptm.tmMaxCharWidth = textmetrica.tmMaxCharWidth;
                lptm.tmWeight = textmetrica.tmWeight;
                lptm.tmOverhang = textmetrica.tmOverhang;
                lptm.tmDigitizedAspectX = textmetrica.tmDigitizedAspectX;
                lptm.tmDigitizedAspectY = textmetrica.tmDigitizedAspectY;
                lptm.tmFirstChar = (char) textmetrica.tmFirstChar;
                lptm.tmLastChar = (char) textmetrica.tmLastChar;
                lptm.tmDefaultChar = (char) textmetrica.tmDefaultChar;
                lptm.tmBreakChar = (char) textmetrica.tmBreakChar;
                lptm.tmItalic = textmetrica.tmItalic;
                lptm.tmUnderlined = textmetrica.tmUnderlined;
                lptm.tmStruckOut = textmetrica.tmStruckOut;
                lptm.tmPitchAndFamily = textmetrica.tmPitchAndFamily;
                lptm.tmCharSet = textmetrica.tmCharSet;
                return textMetricsA;
            }
            return GetTextMetricsW(hDC, ref lptm);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextMetricsA(HandleRef hDC, [In, Out] ref IntNativeMethods.TEXTMETRICA lptm);
        [DllImport("gdi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextMetricsW(HandleRef hDC, [In, Out] ref IntNativeMethods.TEXTMETRIC lptm);
        public static bool GetViewportExtEx(HandleRef hdc, [In, Out] IntNativeMethods.SIZE lpSize)
        {
            return IntGetViewportExtEx(hdc, lpSize);
        }

        public static bool GetViewportOrgEx(HandleRef hdc, [In, Out] IntNativeMethods.POINT lpPoint)
        {
            return IntGetViewportOrgEx(hdc, lpPoint);
        }

        [DllImport("gdi32.dll", EntryPoint="AngleArc", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntAngleArc(HandleRef hDC, int x, int y, int radius, float startAngle, float endAngle);
        [DllImport("gdi32.dll", EntryPoint="Arc", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntArc(HandleRef hDC, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nXStartArc, int nYStartArc, int nXEndArc, int nYEndArc);
        [DllImport("gdi32.dll", EntryPoint="BeginPath", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntBeginPath(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="CombineRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntNativeMethods.RegionFlags IntCombineRgn(HandleRef hRgnDest, HandleRef hRgnSrc1, HandleRef hRgnSrc2, RegionCombineMode combineMode);
        [DllImport("gdi32.dll", EntryPoint="CreateCompatibleDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="CreateDC", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr IntCreateDC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData);
        [DllImport("gdi32.dll", EntryPoint="CreateFontIndirect", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr IntCreateFontIndirect([In, Out, MarshalAs(UnmanagedType.AsAny)] object lf);
        [DllImport("gdi32.dll", EntryPoint="CreateIC", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr IntCreateIC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData);
        [DllImport("gdi32.dll", EntryPoint="DeleteDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntDeleteDC(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntDeleteObject(HandleRef hObject);
        [DllImport("gdi32.dll", EntryPoint="Ellipse", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntEllipse(HandleRef hDc, int x1, int y1, int x2, int y2);
        [DllImport("gdi32.dll", EntryPoint="EndPath", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntEndPath(HandleRef hDC);
        [DllImport("user32.dll", EntryPoint="FillRect", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntFillRect(HandleRef hdc, [In] ref IntNativeMethods.RECT rect, HandleRef hbrush);
        [DllImport("gdi32.dll", EntryPoint="GetBkMode", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntGetBkMode(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="GetClipRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntGetClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("gdi32.dll", EntryPoint="GetCurrentObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntGetCurrentObject(HandleRef hDC, int uObjectType);
        [DllImport("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntGetDC(HandleRef hWnd);
        [DllImport("gdi32.dll", EntryPoint="GetMapMode", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntGetMapMode(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="GetObject", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int IntGetObject(HandleRef hBrush, int nSize, [In, Out] IntNativeMethods.LOGBRUSH lb);
        [DllImport("gdi32.dll", EntryPoint="GetObject", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int IntGetObject(HandleRef hFont, int nSize, [In, Out] IntNativeMethods.LOGFONT lf);
        [DllImport("gdi32.dll", EntryPoint="GetRgnBox", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntNativeMethods.RegionFlags IntGetRgnBox(HandleRef hRgn, [In, Out] ref IntNativeMethods.RECT clipRect);
        [DllImport("gdi32.dll", EntryPoint="GetStockObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntGetStockObject(int nIndex);
        [DllImport("gdi32.dll", EntryPoint="GetViewportExtEx", SetLastError=true, ExactSpelling=true)]
        public static extern bool IntGetViewportExtEx(HandleRef hdc, [In, Out] IntNativeMethods.SIZE lpSize);
        [DllImport("gdi32.dll", EntryPoint="GetViewportOrgEx", SetLastError=true, ExactSpelling=true)]
        public static extern bool IntGetViewportOrgEx(HandleRef hdc, [In, Out] IntNativeMethods.POINT lpPoint);
        [DllImport("gdi32.dll", EntryPoint="LineTo", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntLineTo(HandleRef hdc, int x, int y);
        [DllImport("gdi32.dll", EntryPoint="MoveToEx", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntMoveToEx(HandleRef hdc, int x, int y, IntNativeMethods.POINT pt);
        [DllImport("gdi32.dll", EntryPoint="OffsetViewportOrgEx", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntOffsetViewportOrgEx(HandleRef hDC, int nXOffset, int nYOffset, [In, Out] IntNativeMethods.POINT point);
        [DllImport("gdi32.dll", EntryPoint="Rectangle", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntRectangle(HandleRef hdc, int left, int top, int right, int bottom);
        [DllImport("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="RestoreDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntRestoreDC(HandleRef hDC, int nSavedDC);
        [DllImport("gdi32.dll", EntryPoint="SaveDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntSaveDC(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="SelectClipRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntNativeMethods.RegionFlags IntSelectClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("gdi32.dll", EntryPoint="SelectObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntSelectObject(HandleRef hdc, HandleRef obj);
        [DllImport("gdi32.dll", EntryPoint="SetBkMode", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntSetBkMode(HandleRef hDC, int nBkMode);
        [DllImport("gdi32.dll", EntryPoint="SetGraphicsMode", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntSetGraphicsMode(HandleRef hDC, int iMode);
        [DllImport("gdi32.dll", EntryPoint="SetMapMode", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntSetMapMode(HandleRef hDC, int nMapMode);
        [DllImport("gdi32.dll", EntryPoint="SetViewportExtEx", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntSetViewportExtEx(HandleRef hDC, int x, int y, [In, Out] IntNativeMethods.SIZE size);
        [DllImport("gdi32.dll", EntryPoint="SetViewportOrgEx", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntSetViewportOrgEx(HandleRef hDC, int x, int y, [In, Out] IntNativeMethods.POINT point);
        [DllImport("gdi32.dll", EntryPoint="StrokePath", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool IntStrokePath(HandleRef hDC);
        public static bool LineTo(HandleRef hdc, int x, int y)
        {
            return IntLineTo(hdc, x, y);
        }

        public static bool MoveToEx(HandleRef hdc, int x, int y, IntNativeMethods.POINT pt)
        {
            return IntMoveToEx(hdc, x, y, pt);
        }

        public static bool OffsetViewportOrgEx(HandleRef hDC, int nXOffset, int nYOffset, [In, Out] IntNativeMethods.POINT point)
        {
            return IntOffsetViewportOrgEx(hDC, nXOffset, nYOffset, point);
        }

        public static bool Rectangle(HandleRef hdc, int left, int top, int right, int bottom)
        {
            return IntRectangle(hdc, left, top, right, bottom);
        }

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, IntSafeNativeMethods.CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }

        public static bool RestoreDC(HandleRef hDC, int nSavedDC)
        {
            return IntRestoreDC(hDC, nSavedDC);
        }

        public static int SaveDC(HandleRef hDC)
        {
            return IntSaveDC(hDC);
        }

        public static IntNativeMethods.RegionFlags SelectClipRgn(HandleRef hDC, HandleRef hRgn)
        {
            return IntSelectClipRgn(hDC, hRgn);
        }

        public static IntPtr SelectObject(HandleRef hdc, HandleRef obj)
        {
            return IntSelectObject(hdc, obj);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);
        public static int SetBkMode(HandleRef hDC, int nBkMode)
        {
            return IntSetBkMode(hDC, nBkMode);
        }

        public static int SetGraphicsMode(HandleRef hDC, int iMode)
        {
            iMode = IntSetGraphicsMode(hDC, iMode);
            return iMode;
        }

        public static int SetMapMode(HandleRef hDC, int nMapMode)
        {
            return IntSetMapMode(hDC, nMapMode);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetROP2(HandleRef hDC, int nDrawMode);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetTextAlign(HandleRef hDC, int nMode);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetTextColor(HandleRef hDC, int crColor);
        public static bool SetViewportExtEx(HandleRef hDC, int x, int y, [In, Out] IntNativeMethods.SIZE size)
        {
            return IntSetViewportExtEx(hDC, x, y, size);
        }

        public static bool SetViewportOrgEx(HandleRef hDC, int x, int y, [In, Out] IntNativeMethods.POINT point)
        {
            return IntSetViewportOrgEx(hDC, x, y, point);
        }

        public static bool StrokePath(HandleRef hDC)
        {
            return IntStrokePath(hDC);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int WideCharToMultiByte(int codePage, int flags, [MarshalAs(UnmanagedType.LPWStr)] string wideStr, int chars, [In, Out] byte[] pOutBytes, int bufferBytes, IntPtr defaultChar, IntPtr pDefaultUsed);
        [DllImport("user32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr WindowFromDC(HandleRef hDC);
    }
}

