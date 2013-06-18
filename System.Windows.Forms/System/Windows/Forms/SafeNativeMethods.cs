namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Internal;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Text;
    using System.Windows.Forms.VisualStyles;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [CLSCompliant(false), DllImport("comctl32.dll", ExactSpelling=true)]
        private static extern bool _TrackMouseEvent(System.Windows.Forms.NativeMethods.TRACKMOUSEEVENT tme);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr ActivateKeyboardLayout(HandleRef hkl, int uFlags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool AdjustWindowRectEx(ref System.Windows.Forms.NativeMethods.RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool BitBlt(HandleRef hDC, int x, int y, int nWidth, int nHeight, HandleRef hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool ChooseColor([In, Out] System.Windows.Forms.NativeMethods.CHOOSECOLOR cc);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool ChooseFont([In, Out] System.Windows.Forms.NativeMethods.CHOOSEFONT cf);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int CloseThemeData(HandleRef hTheme);
        public static Color ColorFromCOLORREF(int colorref)
        {
            int red = colorref & 0xff;
            int green = (colorref >> 8) & 0xff;
            int blue = (colorref >> 0x10) & 0xff;
            return Color.FromArgb(red, green, blue);
        }

        public static int ColorToCOLORREF(Color color)
        {
            return ((color.R | (color.G << 8)) | (color.B << 0x10));
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int CombineRgn(HandleRef hRgn, HandleRef hRgn1, HandleRef hRgn2, int nCombineMode);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int CommDlgExtendedError();
        public static IntPtr CopyImage(HandleRef hImage, int uType, int cxDesired, int cyDesired, int fuFlags)
        {
            return System.Internal.HandleCollector.Add(IntCopyImage(hImage, uType, cxDesired, cyDesired, fuFlags), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CopyImageAsCursor(HandleRef hImage, int uType, int cxDesired, int cyDesired, int fuFlags)
        {
            return System.Internal.HandleCollector.Add(IntCopyImage(hImage, uType, cxDesired, cyDesired, fuFlags), System.Windows.Forms.NativeMethods.CommonHandles.Cursor);
        }

        public static IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, IntPtr lpvBits)
        {
            return System.Internal.HandleCollector.Add(IntCreateBitmap(nWidth, nHeight, nPlanes, nBitsPerPixel, lpvBits), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, byte[] lpvBits)
        {
            return System.Internal.HandleCollector.Add(IntCreateBitmapByte(nWidth, nHeight, nPlanes, nBitsPerPixel, lpvBits), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits)
        {
            return System.Internal.HandleCollector.Add(IntCreateBitmapShort(nWidth, nHeight, nPlanes, nBitsPerPixel, lpvBits), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateBrushIndirect(System.Windows.Forms.NativeMethods.LOGBRUSH lb)
        {
            return System.Internal.HandleCollector.Add(IntCreateBrushIndirect(lb), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateCompatibleBitmap(HandleRef hDC, int width, int height)
        {
            return System.Internal.HandleCollector.Add(IntCreateCompatibleBitmap(hDC, width, height), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateDIBSection(HandleRef hdc, HandleRef pbmi, int iUsage, byte[] ppvBits, IntPtr hSection, int dwOffset)
        {
            return System.Internal.HandleCollector.Add(IntCreateDIBSection(hdc, pbmi, iUsage, ppvBits, hSection, dwOffset), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateHalftonePalette(HandleRef hdc)
        {
            return System.Internal.HandleCollector.Add(IntCreateHalftonePalette(hdc), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreatePatternBrush(HandleRef hbmp)
        {
            return System.Internal.HandleCollector.Add(IntCreatePatternBrush(hbmp), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreatePen(int nStyle, int nWidth, int crColor)
        {
            return System.Internal.HandleCollector.Add(IntCreatePen(nStyle, nWidth, crColor), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateRectRgn(int x1, int y1, int x2, int y2)
        {
            return System.Internal.HandleCollector.Add(IntCreateRectRgn(x1, y1, x2, y2), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static IntPtr CreateSolidBrush(int crColor)
        {
            return System.Internal.HandleCollector.Add(IntCreateSolidBrush(crColor), System.Windows.Forms.NativeMethods.CommonHandles.GDI);
        }

        public static bool DeleteObject(HandleRef hObject)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hObject, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
            return IntDeleteObject(hObject);
        }

        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int DoDragDrop(System.Runtime.InteropServices.ComTypes.IDataObject dataObject, System.Windows.Forms.UnsafeNativeMethods.IOleDropSource dropSource, int allowedEffects, int[] finalEffect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool DrawEdge(HandleRef hDC, ref System.Windows.Forms.NativeMethods.RECT rect, int edge, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool DrawFrameControl(HandleRef hDC, ref System.Windows.Forms.NativeMethods.RECT rect, int type, int state);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool DrawIcon(HandleRef hDC, int x, int y, HandleRef hIcon);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool DrawIconEx(HandleRef hDC, int x, int y, HandleRef hIcon, int width, int height, int iStepIfAniCursor, HandleRef hBrushFlickerFree, int diFlags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool DrawMenuBar(HandleRef hWnd);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int DrawThemeBackground(HandleRef hTheme, HandleRef hdc, int partId, int stateId, [In] System.Windows.Forms.NativeMethods.COMRECT pRect, [In] System.Windows.Forms.NativeMethods.COMRECT pClipRect);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int DrawThemeEdge(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] System.Windows.Forms.NativeMethods.COMRECT pDestRect, int uEdge, int uFlags, [Out] System.Windows.Forms.NativeMethods.COMRECT pContentRect);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int DrawThemeParentBackground(HandleRef hwnd, HandleRef hdc, [In] System.Windows.Forms.NativeMethods.COMRECT prc);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int DrawThemeText(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlags, int dwTextFlags2, [In] System.Windows.Forms.NativeMethods.COMRECT pRect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool EnableWindow(HandleRef hWnd, bool enable);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern bool EnumDisplayMonitors(HandleRef hdc, System.Windows.Forms.NativeMethods.COMRECT rcClip, System.Windows.Forms.NativeMethods.MonitorEnumProc lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref System.Windows.Forms.NativeMethods.DEVMODE lpDevMode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool ExternalDeleteObject(HandleRef hObject);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int FillRect(HandleRef hdc, [In] ref System.Windows.Forms.NativeMethods.RECT rect, HandleRef hbrush);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, HandleRef arguments);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetBitmapBits(HandleRef hbmp, int cbBuffer, byte[] lpvBits);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetBkColor(HandleRef hDC);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern uint GetCaretBlinkTime();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(HandleRef hWnd, [In, Out] ref System.Windows.Forms.NativeMethods.RECT rect);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetClipboardFormatName(int format, StringBuilder lpString, int cchMax);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClipCursor([In, Out] ref System.Windows.Forms.NativeMethods.RECT lpRect);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetCurrentProcessId();
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetCurrentThreadId();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetCursor();
        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(HandleRef hdc, HandleRef hbm, int uStartScan, int cScanLines, byte[] lpvBits, ref System.Windows.Forms.NativeMethods.BITMAPINFO_FLAT bmi, int uUsage);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetDoubleClickTime();
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetExitCodeThread(HandleRef hWnd, out int lpdwExitCode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetIconInfo(HandleRef hIcon, [In, Out] System.Windows.Forms.NativeMethods.ICONINFO info);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetKeyboardLayout(int dwLayout);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetKeyboardLayoutList(int size, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] hkls);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetMessagePos();
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] System.Windows.Forms.NativeMethods.MONITORINFOEX info);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetPaletteEntries(HandleRef hpal, int iStartIndex, int nEntries, int[] lppe);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetRgnBox(HandleRef hRegion, ref System.Windows.Forms.NativeMethods.RECT clipRect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetScrollInfo(HandleRef hWnd, int fnBar, [In, Out] System.Windows.Forms.NativeMethods.SCROLLINFO si);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetSysColorBrush(int nIndex);
        [DllImport("gdi32.dll")]
        public static extern int GetSystemPaletteEntries(HandleRef hdc, int iStartIndex, int nEntries, byte[] lppe);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetTextColor(HandleRef hDC);
        public static int GetTextMetrics(HandleRef hDC, ref System.Windows.Forms.NativeMethods.TEXTMETRIC lptm)
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                System.Windows.Forms.NativeMethods.TEXTMETRICA textmetrica = new System.Windows.Forms.NativeMethods.TEXTMETRICA();
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
        public static extern int GetTextMetricsA(HandleRef hDC, [In, Out] ref System.Windows.Forms.NativeMethods.TEXTMETRICA lptm);
        [DllImport("gdi32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        public static extern int GetTextMetricsW(HandleRef hDC, [In, Out] ref System.Windows.Forms.NativeMethods.TEXTMETRIC lptm);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeAppProperties();
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeBackgroundContentRect(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] System.Windows.Forms.NativeMethods.COMRECT pBoundingRect, [Out] System.Windows.Forms.NativeMethods.COMRECT pContentRect);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeBackgroundExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] System.Windows.Forms.NativeMethods.COMRECT pContentRect, [Out] System.Windows.Forms.NativeMethods.COMRECT pExtentRect);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeBackgroundRegion(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] System.Windows.Forms.NativeMethods.COMRECT pRect, ref IntPtr pRegion);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeBool(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref bool pfVal);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeColor(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int pColor);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeDocumentationProperty([MarshalAs(UnmanagedType.LPWStr)] string pszThemeName, [MarshalAs(UnmanagedType.LPWStr)] string pszPropertyName, StringBuilder pszValueBuff, int cchMaxValChars);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeEnumValue(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeFilename(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszThemeFilename, int cchMaxBuffChars);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeFont(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int iPropId, System.Windows.Forms.NativeMethods.LOGFONT pFont);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeInt(HandleRef hTheme, int iPartId, int iStateId, int iPropId, ref int piVal);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeMargins(HandleRef hTheme, HandleRef hDC, int iPartId, int iStateId, int iPropId, ref System.Windows.Forms.NativeMethods.MARGINS margins);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemePartSize(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [In] System.Windows.Forms.NativeMethods.COMRECT prc, ThemeSizeType eSize, [Out] System.Windows.Forms.NativeMethods.SIZE psz);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemePosition(HandleRef hTheme, int iPartId, int iStateId, int iPropId, [Out] System.Windows.Forms.NativeMethods.POINT pPoint);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeString(HandleRef hTheme, int iPartId, int iStateId, int iPropId, StringBuilder pszBuff, int cchMaxBuffChars);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern bool GetThemeSysBool(HandleRef hTheme, int iBoolId);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeSysInt(HandleRef hTheme, int iIntId, ref int piValue);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeTextExtent(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, [MarshalAs(UnmanagedType.LPWStr)] string pszText, int iCharCount, int dwTextFlags, [In] System.Windows.Forms.NativeMethods.COMRECT pBoundingRect, [Out] System.Windows.Forms.NativeMethods.COMRECT pExtentRect);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int GetThemeTextMetrics(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, ref TextMetrics ptm);
        [DllImport("kernel32.dll", EntryPoint="GetThreadLocale", CharSet=CharSet.Auto)]
        public static extern int GetThreadLCID();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetThreadLocale();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetTickCount();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetUpdateRgn(HandleRef hwnd, HandleRef hrgn, bool fErase);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool GetViewportOrgEx(HandleRef hDC, [In, Out] System.Windows.Forms.NativeMethods.POINT point);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool HideCaret(HandleRef hWnd);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern int HitTestThemeBackground(HandleRef hTheme, HandleRef hdc, int iPartId, int iStateId, int dwOptions, [In] System.Windows.Forms.NativeMethods.COMRECT pRect, HandleRef hrgn, [In] System.Windows.Forms.NativeMethods.POINTSTRUCT ptTest, ref int pwHitTestCode);
        [DllImport("hhctrl.ocx", CharSet=CharSet.Auto)]
        public static extern int HtmlHelp(HandleRef hwndCaller, [MarshalAs(UnmanagedType.LPTStr)] string pszFile, int uCommand, int dwData);
        [DllImport("hhctrl.ocx", CharSet=CharSet.Auto)]
        public static extern int HtmlHelp(HandleRef hwndCaller, [MarshalAs(UnmanagedType.LPTStr)] string pszFile, int uCommand, string dwData);
        [DllImport("hhctrl.ocx", CharSet=CharSet.Auto)]
        public static extern int HtmlHelp(HandleRef hwndCaller, [MarshalAs(UnmanagedType.LPTStr)] string pszFile, int uCommand, [MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.HH_AKLINK dwData);
        [DllImport("hhctrl.ocx", CharSet=CharSet.Auto)]
        public static extern int HtmlHelp(HandleRef hwndCaller, [MarshalAs(UnmanagedType.LPTStr)] string pszFile, int uCommand, [MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.HH_FTS_QUERY dwData);
        [DllImport("hhctrl.ocx", CharSet=CharSet.Auto)]
        public static extern int HtmlHelp(HandleRef hwndCaller, [MarshalAs(UnmanagedType.LPTStr)] string pszFile, int uCommand, [MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.HH_POPUP dwData);
        [DllImport("comctl32.dll")]
        public static extern int ImageList_Add(HandleRef himl, HandleRef hbmImage, HandleRef hbmMask);
        [DllImport("comctl32.dll")]
        public static extern IntPtr ImageList_Create(int cx, int cy, int flags, int cInitial, int cGrow);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Destroy(HandleRef himl);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Draw(HandleRef himl, int i, HandleRef hdcDst, int x, int y, int fStyle);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_DrawEx(HandleRef himl, int i, HandleRef hdcDst, int x, int y, int dx, int dy, int rgbBk, int rgbFg, int fStyle);
        [DllImport("comctl32.dll")]
        public static extern IntPtr ImageList_Duplicate(HandleRef himl);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_GetIconSize(HandleRef himl, out int x, out int y);
        [DllImport("comctl32.dll")]
        public static extern int ImageList_GetImageCount(HandleRef himl);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_GetImageInfo(HandleRef himl, int i, System.Windows.Forms.NativeMethods.IMAGEINFO pImageInfo);
        [DllImport("comctl32.dll")]
        public static extern IntPtr ImageList_Read(System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Remove(HandleRef himl, int i);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Replace(HandleRef himl, int i, HandleRef hbmImage, HandleRef hbmMask);
        [DllImport("comctl32.dll")]
        public static extern int ImageList_ReplaceIcon(HandleRef himl, int index, HandleRef hicon);
        [DllImport("comctl32.dll")]
        public static extern int ImageList_SetBkColor(HandleRef himl, int clrBk);
        [DllImport("comctl32.dll")]
        public static extern bool ImageList_Write(HandleRef himl, System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
        [DllImport("comctl32.dll")]
        public static extern int ImageList_WriteEx(HandleRef himl, int dwFlags, System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
        [DllImport("comctl32.dll")]
        public static extern void InitCommonControls();
        [DllImport("comctl32.dll")]
        public static extern bool InitCommonControlsEx(System.Windows.Forms.NativeMethods.INITCOMMONCONTROLSEX icc);
        [DllImport("user32.dll", EntryPoint="CopyImage", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntCopyImage(HandleRef hImage, int uType, int cxDesired, int cyDesired, int fuFlags);
        [DllImport("gdi32.dll", EntryPoint="CreateBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, IntPtr lpvBits);
        [DllImport("gdi32.dll", EntryPoint="CreateBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateBitmapByte(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, byte[] lpvBits);
        [DllImport("gdi32.dll", EntryPoint="CreateBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateBitmapShort(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits);
        [DllImport("gdi32.dll", EntryPoint="CreateBrushIndirect", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateBrushIndirect(System.Windows.Forms.NativeMethods.LOGBRUSH lb);
        [DllImport("gdi32.dll", EntryPoint="CreateCompatibleBitmap", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr IntCreateCompatibleBitmap(HandleRef hDC, int width, int height);
        [DllImport("gdi32.dll", EntryPoint="CreateDIBSection", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateDIBSection(HandleRef hdc, HandleRef pbmi, int iUsage, byte[] ppvBits, IntPtr hSection, int dwOffset);
        [DllImport("gdi32.dll", EntryPoint="CreateHalftonePalette", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateHalftonePalette(HandleRef hdc);
        [DllImport("gdi32.dll", EntryPoint="CreatePatternBrush", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreatePatternBrush(HandleRef hbmp);
        [DllImport("gdi32.dll", EntryPoint="CreatePen", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreatePen(int nStyle, int nWidth, int crColor);
        [DllImport("gdi32.dll", EntryPoint="CreateRectRgn", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateRectRgn(int x1, int y1, int x2, int y2);
        [DllImport("gdi32.dll", EntryPoint="CreateSolidBrush", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateSolidBrush(int crColor);
        [DllImport("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern bool IntDeleteObject(HandleRef hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int IntersectClipRect(HandleRef hDC, int x1, int y1, int x2, int y2);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool InvalidateRect(HandleRef hWnd, ref System.Windows.Forms.NativeMethods.RECT rect, bool erase);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool InvalidateRect(HandleRef hWnd, System.Windows.Forms.NativeMethods.COMRECT rect, bool erase);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool InvalidateRgn(HandleRef hWnd, HandleRef hrgn, bool erase);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsAccelerator(HandleRef hAccel, int cAccelEntries, [In] ref System.Windows.Forms.NativeMethods.MSG lpMsg, short[] lpwCmd);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern bool IsAppThemed();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsChild(HandleRef parent, HandleRef child);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern bool IsThemeBackgroundPartiallyTransparent(HandleRef hTheme, int iPartId, int iStateId);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern bool IsThemePartDefined(HandleRef hTheme, int iPartId, int iStateId);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindowEnabled(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindowUnicode(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindowVisible(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool KillTimer(HandleRef hwnd, int idEvent);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool LineTo(HandleRef hdc, int x, int y);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr LoadCursor(HandleRef hInst, int iconId);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool LPtoDP(HandleRef hDC, [In, Out] ref System.Windows.Forms.NativeMethods.RECT lpRect, int nCount);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool MessageBeep(int type);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int MessageBox(HandleRef hWnd, string text, string caption, int type);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern IntPtr MonitorFromPoint(System.Windows.Forms.NativeMethods.POINTSTRUCT pt, int flags);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern IntPtr MonitorFromRect(ref System.Windows.Forms.NativeMethods.RECT rect, int flags);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern IntPtr MonitorFromWindow(HandleRef handle, int flags);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool MoveToEx(HandleRef hdc, int x, int y, System.Windows.Forms.NativeMethods.POINT pt);
        [DllImport("user32.dll")]
        public static extern int OemKeyScan(short wAsciiVal);
        [DllImport("oleaut32.dll", EntryPoint="OleCreateFontIndirect", ExactSpelling=true, PreserveSig=false)]
        public static extern IFontDisp OleCreateIFontDispIndirect(System.Windows.Forms.NativeMethods.FONTDESC fd, ref Guid iid);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void OleCreatePropertyFrame(HandleRef hwndOwner, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string caption, int objects, [MarshalAs(UnmanagedType.Interface)] ref object pobjs, int pages, HandleRef pClsid, int locale, int reserved1, IntPtr reserved2);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void OleCreatePropertyFrame(HandleRef hwndOwner, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string caption, int objects, [MarshalAs(UnmanagedType.Interface)] ref object pobjs, int pages, Guid[] pClsid, int locale, int reserved1, IntPtr reserved2);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void OleCreatePropertyFrame(HandleRef hwndOwner, int x, int y, [MarshalAs(UnmanagedType.LPWStr)] string caption, int objects, HandleRef lplpobjs, int pages, HandleRef pClsid, int locale, int reserved1, IntPtr reserved2);
        [DllImport("user32.dll")]
        public static extern IntPtr OpenInputDesktop(int dwFlags, [MarshalAs(UnmanagedType.Bool)] bool fInherit, int dwDesiredAccess);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr OpenThemeData(HandleRef hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszClassList);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool PatBlt(HandleRef hdc, int left, int top, int width, int height, int rop);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int RealizePalette(HandleRef hDC);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool Rectangle(HandleRef hdc, int left, int top, int right, int bottom);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool RedrawWindow(HandleRef hwnd, ref System.Windows.Forms.NativeMethods.RECT rcUpdate, HandleRef hrgnUpdate, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool RedrawWindow(HandleRef hwnd, System.Windows.Forms.NativeMethods.COMRECT rcUpdate, HandleRef hrgnUpdate, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int RegisterClipboardFormat(string format);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int RegisterWindowMessage(string msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ReleaseCapture();
        public static int RGBToCOLORREF(int rgbValue)
        {
            int num = (rgbValue & 0xff) << 0x10;
            rgbValue &= 0xffff00;
            rgbValue |= (rgbValue >> 0x10) & 0xff;
            rgbValue &= 0xffff;
            rgbValue |= num;
            return rgbValue;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ScrollWindow(HandleRef hWnd, int nXAmount, int nYAmount, ref System.Windows.Forms.NativeMethods.RECT rectScrollRegion, ref System.Windows.Forms.NativeMethods.RECT rectClip);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int ScrollWindowEx(HandleRef hWnd, int nXAmount, int nYAmount, System.Windows.Forms.NativeMethods.COMRECT rectScrollRegion, ref System.Windows.Forms.NativeMethods.RECT rectClip, HandleRef hrgnUpdate, ref System.Windows.Forms.NativeMethods.RECT prcUpdate, int flags);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SelectClipRgn(HandleRef hDC, HandleRef hRgn);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr SelectPalette(HandleRef hdc, HandleRef hpal, int bForceBackground);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetBkColor(HandleRef hDC, int clr);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetBkMode(HandleRef hDC, int nBkMode);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetMapMode(HandleRef hDC, int nMapMode);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetROP2(HandleRef hDC, int nDrawMode);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetTextColor(HandleRef hDC, int crColor);
        [DllImport("uxtheme.dll", CharSet=CharSet.Auto)]
        public static extern void SetThemeAppProperties(int Flags);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetThreadLocale(int Locale);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetTimer(HandleRef hWnd, int nIDEvent, int uElapse, IntPtr lpTimerFunc);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool SetViewportExtEx(HandleRef hDC, int x, int y, System.Windows.Forms.NativeMethods.SIZE size);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool SetViewportOrgEx(HandleRef hDC, int x, int y, [In, Out] System.Windows.Forms.NativeMethods.POINT point);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool SetWindowExtEx(HandleRef hDC, int x, int y, [In, Out] System.Windows.Forms.NativeMethods.SIZE size);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool SetWindowOrgEx(HandleRef hDC, int x, int y, [In, Out] System.Windows.Forms.NativeMethods.POINT point);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);
        [DllImport("shlwapi.dll")]
        public static extern int SHAutoComplete(HandleRef hwndEdit, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ShowCaret(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ShowWindow(HandleRef hWnd, int nCmdShow);
        [DllImport("gdi32.dll")]
        public static extern int StretchDIBits(HandleRef hdc, int XDest, int YDest, int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth, int nSrcHeight, byte[] lpBits, ref System.Windows.Forms.NativeMethods.BITMAPINFO_FLAT lpBitsInfo, int iUsage, int dwRop);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern void SysFreeString(HandleRef bstr);
        public static bool TrackMouseEvent(System.Windows.Forms.NativeMethods.TRACKMOUSEEVENT tme)
        {
            return _TrackMouseEvent(tme);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool TrackPopupMenuEx(HandleRef hmenu, int fuFlags, int x, int y, HandleRef hwnd, System.Windows.Forms.NativeMethods.TPMPARAMS tpm);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool UpdateWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ValidateRect(HandleRef hWnd, [In, Out] ref System.Windows.Forms.NativeMethods.RECT rect);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void VariantClear(HandleRef pObject);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void VariantInit(HandleRef pObject);

        internal delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("BEF6E003-A874-101A-8BBA-00AA00300CAB")]
        public interface IFontDisp
        {
            string Name { get; set; }
            long Size { get; set; }
            bool Bold { get; set; }
            bool Italic { get; set; }
            bool Underline { get; set; }
            bool Strikethrough { get; set; }
            short Weight { get; set; }
            short Charset { get; set; }
        }
    }
}

