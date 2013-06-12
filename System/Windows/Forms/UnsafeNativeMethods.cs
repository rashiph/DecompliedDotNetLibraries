namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Internal;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public const int LAYOUT_BITMAPORIENTATIONPRESERVED = 8;
        public const int LAYOUT_RTL = 1;
        public const int MB_PRECOMPOSED = 1;
        public const int SMTO_ABORTIFHUNG = 2;
        private static readonly Version VistaOSVersion = new Version(6, 0);

        [DllImport("user32.dll", EntryPoint="ChildWindowFromPointEx", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr _ChildWindowFromPointEx(HandleRef hwndParent, POINTSTRUCT pt, int uFlags);
        [DllImport("user32.dll", EntryPoint="WindowFromPoint", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr _WindowFromPoint(POINTSTRUCT pt);
        public static IntPtr BeginPaint(HandleRef hWnd, [In, Out, MarshalAs(UnmanagedType.LPStruct)] ref System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint)
        {
            return System.Internal.HandleCollector.Add(IntBeginPaint(hWnd, ref lpPaint), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool BlockInput([In, MarshalAs(UnmanagedType.Bool)] bool fBlockIt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr CallNextHookEx(HandleRef hhook, int code, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr CallWindowProc(IntPtr wndProc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public static IntPtr ChildWindowFromPointEx(HandleRef hwndParent, int x, int y, int uFlags)
        {
            POINTSTRUCT pt = new POINTSTRUCT(x, y);
            return _ChildWindowFromPointEx(hwndParent, pt, uFlags);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int ClientToScreen(HandleRef hWnd, [In, Out] System.Windows.Forms.NativeMethods.POINT pt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ClipCursor(ref System.Windows.Forms.NativeMethods.RECT rcClip);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool ClipCursor(System.Windows.Forms.NativeMethods.COMRECT rcClip);
        public static bool CloseHandle(HandleRef handle)
        {
            System.Internal.HandleCollector.Remove((IntPtr) handle, System.Windows.Forms.NativeMethods.CommonHandles.Kernel);
            return IntCloseHandle(handle);
        }

        [return: MarshalAs(UnmanagedType.Interface)]
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern object CoCreateInstance([In] ref Guid clsid, [MarshalAs(UnmanagedType.Interface)] object punkOuter, int context, [In] ref Guid iid);
        [DllImport("ole32.dll", ExactSpelling=true, PreserveSig=false)]
        public static extern IClassFactory2 CoGetClassObject([In] ref Guid clsid, int dwContext, int serverInfo, [In] ref Guid refiid);
        [DllImport("ole32.dll")]
        public static extern int CoGetMalloc(int dwReserved, out IMalloc pMalloc);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", ExactSpelling=true)]
        public static extern void CopyMemory(IntPtr pdst, byte[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern void CopyMemory(HandleRef destData, HandleRef srcData, int size);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern void CopyMemoryA(IntPtr pdst, string psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern void CopyMemoryA(IntPtr pdst, char[] psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern void CopyMemoryW(IntPtr pdst, string psrc, int cb);
        [DllImport("kernel32.dll", EntryPoint="RtlMoveMemory", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern void CopyMemoryW(IntPtr pdst, char[] psrc, int cb);
        [DllImport("ole32.dll", ExactSpelling=true)]
        public static extern int CoRegisterMessageFilter(HandleRef newFilter, ref IntPtr oldMsgFilter);
        [DllImport("clr.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        internal static extern void CorLaunchApplication(uint hostType, string applicationFullName, int manifestPathsCount, string[] manifestPaths, int activationDataCount, string[] activationData, PROCESS_INFORMATION processInformation);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        internal static extern void CoTaskMemFree(IntPtr pv);
        public static IntPtr CreateAcceleratorTable(HandleRef pentries, int cCount)
        {
            return System.Internal.HandleCollector.Add(IntCreateAcceleratorTable(pentries, cCount), System.Windows.Forms.NativeMethods.CommonHandles.Accelerator);
        }

        public static IntPtr CreateCompatibleDC(HandleRef hDC)
        {
            return System.Internal.HandleCollector.Add(IntCreateCompatibleDC(hDC), System.Windows.Forms.NativeMethods.CommonHandles.CompatibleHDC);
        }

        public static IntPtr CreateDC(string lpszDriver)
        {
            return System.Internal.HandleCollector.Add(IntCreateDC(lpszDriver, null, null, System.Windows.Forms.NativeMethods.NullHandleRef), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        public static IntPtr CreateDC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData)
        {
            return System.Internal.HandleCollector.Add(IntCreateDC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        public static IntPtr CreateIC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData)
        {
            return System.Internal.HandleCollector.Add(IntCreateIC(lpszDriverName, lpszDeviceName, lpszOutput, lpInitData), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern ILockBytes CreateILockBytesOnHGlobal(HandleRef hGlobal, bool fDeleteOnRelease);
        public static IntPtr CreateMenu()
        {
            return System.Internal.HandleCollector.Add(IntCreateMenu(), System.Windows.Forms.NativeMethods.CommonHandles.Menu);
        }

        public static IntPtr CreatePopupMenu()
        {
            return System.Internal.HandleCollector.Add(IntCreatePopupMenu(), System.Windows.Forms.NativeMethods.CommonHandles.Menu);
        }

        [DllImport("oleacc.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int CreateStdAccessibleObject(HandleRef hWnd, int objID, ref Guid refiid, [In, Out, MarshalAs(UnmanagedType.Interface)] ref object pAcc);
        public static IntPtr CreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width, int height, HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam)
        {
            return IntCreateWindowEx(dwExStyle, lpszClassName, lpszWindowName, style, x, y, width, height, hWndParent, hMenu, hInst, pvParam);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DefFrameProc(IntPtr hWnd, IntPtr hWndClient, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DefMDIChildProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public static bool DeleteCompatibleDC(HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, System.Windows.Forms.NativeMethods.CommonHandles.CompatibleHDC);
            return IntDeleteDC(hDC);
        }

        public static bool DeleteDC(HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, System.Windows.Forms.NativeMethods.CommonHandles.HDC);
            return IntDeleteDC(hDC);
        }

        public static bool DestroyAcceleratorTable(HandleRef hAccel)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hAccel, System.Windows.Forms.NativeMethods.CommonHandles.Accelerator);
            return IntDestroyAcceleratorTable(hAccel);
        }

        public static bool DestroyCursor(HandleRef hCurs)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hCurs, System.Windows.Forms.NativeMethods.CommonHandles.Cursor);
            return IntDestroyCursor(hCurs);
        }

        public static bool DestroyMenu(HandleRef hMenu)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hMenu, System.Windows.Forms.NativeMethods.CommonHandles.Menu);
            return IntDestroyMenu(hMenu);
        }

        public static bool DestroyWindow(HandleRef hWnd)
        {
            return IntDestroyWindow(hWnd);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr DispatchMessage([In] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern IntPtr DispatchMessageA([In] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern IntPtr DispatchMessageW([In] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("shell32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern void DragAcceptFiles(HandleRef hWnd, bool fAccept);
        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
        public static extern int DragQueryFile(HandleRef hDrop, int iFile, StringBuilder lpszFile, int cch);
        public static IntPtr DuplicateHandle(HandleRef processSource, HandleRef handleSource, HandleRef processTarget, ref IntPtr handleTarget, int desiredAccess, bool inheritHandle, int options)
        {
            IntPtr ptr = IntDuplicateHandle(processSource, handleSource, processTarget, ref handleTarget, desiredAccess, inheritHandle, options);
            System.Internal.HandleCollector.Add(handleTarget, System.Windows.Forms.NativeMethods.CommonHandles.Kernel);
            return ptr;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool EnableMenuItem(HandleRef hMenu, int UIDEnabledItem, int uEnable);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool EnableScrollBar(HandleRef hWnd, int nBar, int value);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern bool EndDialog(HandleRef hWnd, IntPtr result);
        public static bool EndPaint(HandleRef hWnd, [In, MarshalAs(UnmanagedType.LPStruct)] ref System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint)
        {
            System.Internal.HandleCollector.Remove(lpPaint.hdc, System.Windows.Forms.NativeMethods.CommonHandles.HDC);
            return IntEndPaint(hWnd, ref lpPaint);
        }

        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern bool EnumChildWindows(HandleRef hwndParent, System.Windows.Forms.NativeMethods.EnumChildrenCallback lpEnumFunc, HandleRef lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool EnumThreadWindows(int dwThreadId, System.Windows.Forms.NativeMethods.EnumThreadWindowsCallback lpfn, HandleRef lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern bool FreeLibrary(HandleRef hModule);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetAncestor(HandleRef hWnd, int flags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern short GetAsyncKeyState(int vkey);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetCapture();
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetClassInfo(HandleRef hInst, string lpszClass, IntPtr h);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetClassInfo(HandleRef hInst, string lpszClass, [In, Out] System.Windows.Forms.NativeMethods.WNDCLASS_I wc);
        [DllImport("user32.dll")]
        public static extern int GetClassName(HandleRef hwnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(HandleRef hWnd, [In, Out] ref System.Windows.Forms.NativeMethods.RECT rect);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetClientRect(HandleRef hWnd, IntPtr rect);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, int[] nSize);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetCursorPos([In, Out] System.Windows.Forms.NativeMethods.POINT pt);
        public static IntPtr GetDC(HandleRef hWnd)
        {
            return System.Internal.HandleCollector.Add(IntGetDC(hWnd), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        public static IntPtr GetDCEx(HandleRef hWnd, HandleRef hrgnClip, int flags)
        {
            return System.Internal.HandleCollector.Add(IntGetDCEx(hWnd, hrgnClip, flags), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetDeviceCaps(HandleRef hDC, int nIndex);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetDlgItem(HandleRef hWnd, int nIDDlgItem);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void GetErrorInfo(int reserved, [In, Out] ref IErrorInfo errorInfo);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern IntPtr GetHGlobalFromILockBytes(ILockBytes pLkbyt);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetKeyboardState(byte[] keystate);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern short GetKeyState(int keyCode);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int GetLocaleInfo(int Locale, int LCType, StringBuilder lpLCData, int cchData);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetMenu(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetMenuItemCount(HandleRef hMenu);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetMenuItemID(HandleRef hMenu, int nPos);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetMenuItemInfo(HandleRef hMenu, int uItem, bool fByPosition, [In, Out] System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetMenuItemInfo(HandleRef hMenu, int uItem, bool fByPosition, [In, Out] System.Windows.Forms.NativeMethods.MENUITEMINFO_T_RW lpmii);
        [DllImport("user32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern bool GetMessageA([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg, HandleRef hWnd, int uMsgFilterMin, int uMsgFilterMax);
        [DllImport("user32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern bool GetMessageW([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg, HandleRef hWnd, int uMsgFilterMin, int uMsgFilterMax);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string modName);
        public static int GetObject(HandleRef hObject, System.Windows.Forms.NativeMethods.LOGBRUSH lb)
        {
            return GetObject(hObject, Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LOGBRUSH)), lb);
        }

        public static int GetObject(HandleRef hObject, System.Windows.Forms.NativeMethods.LOGFONT lp)
        {
            return GetObject(hObject, Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LOGFONT)), lp);
        }

        public static int GetObject(HandleRef hObject, System.Windows.Forms.NativeMethods.LOGPEN lp)
        {
            return GetObject(hObject, Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.LOGPEN)), lp);
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] System.Windows.Forms.NativeMethods.BITMAP bm);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] System.Windows.Forms.NativeMethods.LOGBRUSH lb);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] System.Windows.Forms.NativeMethods.LOGFONT lf);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, [In, Out] System.Windows.Forms.NativeMethods.LOGPEN lp);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, ref int nEntries);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int GetObject(HandleRef hObject, int nSize, int[] nEntries);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetObjectType(HandleRef hObject);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetOpenFileName([In, Out] System.Windows.Forms.NativeMethods.OPENFILENAME_I ofn);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetParent(HandleRef hWnd);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
        public static extern IntPtr GetProcAddress(HandleRef hModule, string lpProcName);
        [DllImport("user32.dll", ExactSpelling=true)]
        public static extern IntPtr GetProcessWindowStation();
        public static unsafe System.Windows.Forms.NativeMethods.RECT[] GetRectsFromRegion(IntPtr hRgn)
        {
            System.Windows.Forms.NativeMethods.RECT[] rectArray = null;
            IntPtr zero = IntPtr.Zero;
            try
            {
                int cb = GetRegionData(new HandleRef(null, hRgn), 0, IntPtr.Zero);
                if (cb == 0)
                {
                    return rectArray;
                }
                zero = Marshal.AllocCoTaskMem(cb);
                if (GetRegionData(new HandleRef(null, hRgn), cb, zero) != cb)
                {
                    return rectArray;
                }
                System.Windows.Forms.NativeMethods.RGNDATAHEADER* rgndataheaderPtr = (System.Windows.Forms.NativeMethods.RGNDATAHEADER*) zero;
                if (rgndataheaderPtr->iType != 1)
                {
                    return rectArray;
                }
                rectArray = new System.Windows.Forms.NativeMethods.RECT[rgndataheaderPtr->nCount];
                int cbSizeOfStruct = rgndataheaderPtr->cbSizeOfStruct;
                for (int i = 0; i < rgndataheaderPtr->nCount; i++)
                {
                    rectArray[i] = (System.Windows.Forms.NativeMethods.RECT) (((void*) zero) + cbSizeOfStruct)[Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.RECT)) * i];
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            return rectArray;
        }

        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int GetRegionData(HandleRef hRgn, int size, IntPtr lpRgnData);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool GetSaveFileName([In, Out] System.Windows.Forms.NativeMethods.OPENFILENAME_I ofn);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetScrollInfo(HandleRef hWnd, int fnBar, System.Windows.Forms.NativeMethods.SCROLLINFO si);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern void GetStartupInfo([In, Out] System.Windows.Forms.NativeMethods.STARTUPINFO_I startupinfo_i);
        [DllImport("gdi32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr GetStockObject(int nIndex);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetSubMenu(HandleRef hwnd, int index);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetSystemMenu(HandleRef hWnd, bool bRevert);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetSystemMetrics(int nIndex);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetSystemPowerStatus([In, Out] ref System.Windows.Forms.NativeMethods.SYSTEM_POWER_STATUS systemPowerStatus);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern void GetTempFileName(string tempDirName, string prefixName, int unique, StringBuilder sb);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetUserName(StringBuilder lpBuffer, int[] nSize);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool GetUserObjectInformation(HandleRef hObj, int nIndex, [MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.USEROBJECTFLAGS pvBuffer, int nLength, ref int lpnLengthNeeded);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);
        public static IntPtr GetWindowDC(HandleRef hWnd)
        {
            return System.Internal.HandleCollector.Add(IntGetWindowDC(hWnd), System.Windows.Forms.NativeMethods.CommonHandles.HDC);
        }

        public static IntPtr GetWindowLong(HandleRef hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint="GetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(HandleRef hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint="GetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(HandleRef hWnd, int nIndex);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GetWindowPlacement(HandleRef hWnd, ref System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref System.Windows.Forms.NativeMethods.RECT rect);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern short GlobalAddAtom(string atomName);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern short GlobalDeleteAtom(short atom);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GlobalFree(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GlobalLock(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr GlobalReAlloc(HandleRef handle, int bytes, int flags);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int GlobalSize(HandleRef handle);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool GlobalUnlock(HandleRef handle);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr ImmAssociateContext(HandleRef hWnd, HandleRef hIMC);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr ImmCreateContext();
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmDestroyContext(HandleRef hIMC);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr ImmGetContext(HandleRef hWnd);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmGetConversionStatus(HandleRef hIMC, ref int conversion, ref int sentence);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmGetOpenStatus(HandleRef hIMC);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmNotifyIME(HandleRef hIMC, int dwAction, int dwIndex, int dwValue);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmReleaseContext(HandleRef hWnd, HandleRef hIMC);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmSetConversionStatus(HandleRef hIMC, int conversion, int sentence);
        [DllImport("imm32.dll", CharSet=CharSet.Auto)]
        public static extern bool ImmSetOpenStatus(HandleRef hIMC, bool open);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool InsertMenuItem(HandleRef hMenu, int uItem, bool fByPosition, System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii);
        [DllImport("user32.dll", EntryPoint="BeginPaint", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntBeginPaint(HandleRef hWnd, [In, Out] ref System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint);
        [DllImport("kernel32.dll", EntryPoint="CloseHandle", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool IntCloseHandle(HandleRef handle);
        [DllImport("user32.dll", EntryPoint="CreateAcceleratorTable", CharSet=CharSet.Auto)]
        private static extern IntPtr IntCreateAcceleratorTable(HandleRef pentries, int cCount);
        [DllImport("gdi32.dll", EntryPoint="CreateCompatibleDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntCreateCompatibleDC(HandleRef hDC);
        [DllImport("gdi32.dll", EntryPoint="CreateDC", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr IntCreateDC(string lpszDriver, string lpszDeviceName, string lpszOutput, HandleRef devMode);
        [DllImport("gdi32.dll", EntryPoint="CreateIC", CharSet=CharSet.Auto, SetLastError=true)]
        private static extern IntPtr IntCreateIC(string lpszDriverName, string lpszDeviceName, string lpszOutput, HandleRef lpInitData);
        [DllImport("user32.dll", EntryPoint="CreateMenu", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntCreateMenu();
        [DllImport("user32.dll", EntryPoint="CreatePopupMenu", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntCreatePopupMenu();
        [DllImport("user32.dll", EntryPoint="CreateWindowEx", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr IntCreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width, int height, HandleRef hWndParent, HandleRef hMenu, HandleRef hInst, [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        [DllImport("gdi32.dll", EntryPoint="DeleteDC", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool IntDeleteDC(HandleRef hDC);
        [DllImport("user32.dll", EntryPoint="DestroyAcceleratorTable", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern bool IntDestroyAcceleratorTable(HandleRef hAccel);
        [DllImport("user32.dll", EntryPoint="DestroyCursor", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern bool IntDestroyCursor(HandleRef hCurs);
        [DllImport("user32.dll", EntryPoint="DestroyMenu", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern bool IntDestroyMenu(HandleRef hMenu);
        [DllImport("user32.dll", EntryPoint="DestroyWindow", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IntDestroyWindow(HandleRef hWnd);
        [DllImport("kernel32.dll", EntryPoint="DuplicateHandle", SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntDuplicateHandle(HandleRef processSource, HandleRef handleSource, HandleRef processTarget, ref IntPtr handleTarget, int desiredAccess, bool inheritHandle, int options);
        [DllImport("user32.dll", EntryPoint="EndPaint", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern bool IntEndPaint(HandleRef hWnd, ref System.Windows.Forms.NativeMethods.PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntGetDC(HandleRef hWnd);
        [DllImport("user32.dll", EntryPoint="GetDCEx", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntGetDCEx(HandleRef hWnd, HandleRef hrgnClip, int flags);
        [DllImport("user32.dll", EntryPoint="GetWindowDC", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern IntPtr IntGetWindowDC(HandleRef hWnd);
        [DllImport("kernel32.dll", EntryPoint="MapViewOfFile", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern IntPtr IntMapViewOfFile(HandleRef hFileMapping, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, int dwNumberOfBytesToMap);
        [DllImport("ole32.dll", EntryPoint="OleInitialize", SetLastError=true, ExactSpelling=true)]
        private static extern int IntOleInitialize(int val);
        [DllImport("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern int IntReleaseDC(HandleRef hWnd, HandleRef hDC);
        [DllImport("user32.dll", EntryPoint="SetWindowRgn", CharSet=CharSet.Auto, ExactSpelling=true)]
        private static extern int IntSetWindowRgn(HandleRef hwnd, HandleRef hrgn, bool fRedraw);
        [DllImport("kernel32.dll", EntryPoint="UnmapViewOfFile", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        private static extern bool IntUnmapViewOfFile(HandleRef pvBaseAddress);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsChild(HandleRef hWndParent, HandleRef hwnd);
        internal static bool IsComObject(object o)
        {
            return Marshal.IsComObject(o);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsDialogMessage(HandleRef hWndDlg, [In, Out] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool IsZoomed(HandleRef hWnd);
        [DllImport("user32.dll", EntryPoint="keybd_event", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern void Keybd_event(byte vk, byte scan, int flags, int extrainfo);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr LoadLibrary(string libname);
        [DllImport("oleacc.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr LresultFromObject(ref Guid refiid, IntPtr wParam, HandleRef pAcc);
        public static IntPtr MapViewOfFile(HandleRef hFileMapping, int dwDesiredAccess, int dwFileOffsetHigh, int dwFileOffsetLow, int dwNumberOfBytesToMap)
        {
            return System.Internal.HandleCollector.Add(IntMapViewOfFile(hFileMapping, dwDesiredAccess, dwFileOffsetHigh, dwFileOffsetLow, dwNumberOfBytesToMap), System.Windows.Forms.NativeMethods.CommonHandles.Kernel);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, [In, Out] ref System.Windows.Forms.NativeMethods.RECT rect, int cPoints);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, [In, Out] System.Windows.Forms.NativeMethods.POINT pt, int cPoints);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int MsgWaitForMultipleObjectsEx(int nCount, IntPtr pHandles, int dwMilliseconds, int dwWakeMask, int dwFlags);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true, ExactSpelling=true)]
        public static extern int MultiByteToWideChar(int CodePage, int dwFlags, byte[] lpMultiByteStr, int cchMultiByte, char[] lpWideCharStr, int cchWideChar);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern void NotifyWinEvent(int winEvent, HandleRef hwnd, int objType, int objID);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern IFont OleCreateFontIndirect(System.Windows.Forms.NativeMethods.tagFONTDESC fontdesc, [In] ref Guid refiid);
        [DllImport("oleaut32.dll", EntryPoint="OleCreateFontIndirect", ExactSpelling=true, PreserveSig=false)]
        public static extern IFont OleCreateIFontIndirect(System.Windows.Forms.NativeMethods.FONTDESC fd, ref Guid iid);
        [DllImport("oleaut32.dll", EntryPoint="OleCreatePictureIndirect", ExactSpelling=true, PreserveSig=false)]
        public static extern IPictureDisp OleCreateIPictureDispIndirect([MarshalAs(UnmanagedType.AsAny)] object pictdesc, ref Guid iid, bool fOwn);
        [DllImport("oleaut32.dll", EntryPoint="OleCreatePictureIndirect", ExactSpelling=true, PreserveSig=false)]
        public static extern IPicture OleCreateIPictureIndirect([MarshalAs(UnmanagedType.AsAny)] object pictdesc, ref Guid iid, bool fOwn);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern IPicture OleCreatePictureIndirect(System.Windows.Forms.NativeMethods.PICTDESC pictdesc, [In] ref Guid refiid, bool fOwn);
        [DllImport("oleaut32.dll", ExactSpelling=true)]
        public static extern void OleCreatePropertyFrameIndirect(System.Windows.Forms.NativeMethods.OCPFIPARAMS p);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int OleFlushClipboard();
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int OleGetClipboard(ref System.Runtime.InteropServices.ComTypes.IDataObject data);
        public static int OleInitialize()
        {
            return IntOleInitialize(0);
        }

        [DllImport("ole32.dll")]
        public static extern int OleLoadFromStream(IStream pStorage, ref Guid iid, out IOleObject pObject);
        [DllImport("ole32.dll")]
        public static extern int OleSaveToStream(IPersistStream pPersistStream, IStream pStream);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int OleSetClipboard(System.Runtime.InteropServices.ComTypes.IDataObject pDataObj);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int OleUninitialize();
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool PageSetupDlg([In, Out] System.Windows.Forms.NativeMethods.PAGESETUPDLG lppsd);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PeekMessage([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);
        [DllImport("user32.dll", CharSet=CharSet.Ansi)]
        public static extern bool PeekMessageA([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);
        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        public static extern bool PeekMessageW([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr PostMessage(HandleRef hwnd, int msg, int wparam, int lparam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr PostMessage(HandleRef hwnd, int msg, int wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PostMessage(HandleRef hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int PostThreadMessage(int id, int msg, IntPtr wparam, IntPtr lparam);
        public static bool PrintDlg([In, Out] System.Windows.Forms.NativeMethods.PRINTDLG lppd)
        {
            if (IntPtr.Size == 4)
            {
                System.Windows.Forms.NativeMethods.PRINTDLG_32 printdlg_ = lppd as System.Windows.Forms.NativeMethods.PRINTDLG_32;
                if (printdlg_ == null)
                {
                    throw new NullReferenceException("PRINTDLG data is null");
                }
                return PrintDlg_32(printdlg_);
            }
            System.Windows.Forms.NativeMethods.PRINTDLG_64 printdlg_2 = lppd as System.Windows.Forms.NativeMethods.PRINTDLG_64;
            if (printdlg_2 == null)
            {
                throw new NullReferenceException("PRINTDLG data is null");
            }
            return PrintDlg_64(printdlg_2);
        }

        [DllImport("comdlg32.dll", EntryPoint="PrintDlg", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool PrintDlg_32([In, Out] System.Windows.Forms.NativeMethods.PRINTDLG_32 lppd);
        [DllImport("comdlg32.dll", EntryPoint="PrintDlg", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool PrintDlg_64([In, Out] System.Windows.Forms.NativeMethods.PRINTDLG_64 lppd);
        [DllImport("comdlg32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int PrintDlgEx([In, Out] System.Windows.Forms.NativeMethods.PRINTDLGEX lppdex);
        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        public static void PtrToStructure(IntPtr lparam, object data)
        {
            Marshal.PtrToStructure(lparam, data);
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted=true)]
        public static object PtrToStructure(IntPtr lparam, System.Type cls)
        {
            return Marshal.PtrToStructure(lparam, cls);
        }

        [DllImport("ole32.dll")]
        public static extern int ReadClassStg(HandleRef pStg, [In, Out] ref Guid pclsid);
        [DllImport("ole32.dll")]
        public static extern int ReadClassStg(IStorage pStorage, out Guid clsid);
        [DllImport("ole32.dll")]
        public static extern int ReadClassStm(IStream pStream, out Guid clsid);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern short RegisterClass(System.Windows.Forms.NativeMethods.WNDCLASS_D wc);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int RegisterDragDrop(HandleRef hwnd, IOleDropTarget target);
        internal static int ReleaseComObject(object objToRelease)
        {
            return Marshal.ReleaseComObject(objToRelease);
        }

        public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
        {
            System.Internal.HandleCollector.Remove((IntPtr) hDC, System.Windows.Forms.NativeMethods.CommonHandles.HDC);
            return IntReleaseDC(hWnd, hDC);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool RemoveMenu(HandleRef hMenu, int uPosition, int uFlags);
        [DllImport("ole32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int RevokeDragDrop(HandleRef hwnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int ScreenToClient(HandleRef hWnd, [In, Out] System.Windows.Forms.NativeMethods.POINT pt);
        [DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
        public static extern IntPtr SendCallbackMessage(HandleRef hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendDlgItemMessage(HandleRef hDlg, int nIDDlgItem, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern uint SendInput(uint nInputs, System.Windows.Forms.NativeMethods.INPUT[] pInputs, int cbSize);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, bool wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, ref int wParam, ref int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.IUnknown)] out object editOle);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int[] lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int[] wParam, int[] lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.HDLAYOUT hdlayout);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.TBBUTTON lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.TBBUTTONINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.TV_INSERTSTRUCT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref System.Windows.Forms.NativeMethods.TV_ITEM lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, [In, Out, MarshalAs(UnmanagedType.Bool)] ref bool wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, HandleRef lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.CHARFORMAT2A lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.CHARFORMATA lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.CHARFORMATW lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.CHARRANGE lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.EDITSTREAM lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.EDITSTREAM64 lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.FINDTEXT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVBKIMAGE lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out] ref System.Windows.Forms.NativeMethods.LVFINDINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVCOLUMN_T lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out] ref System.Windows.Forms.NativeMethods.LVITEM lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out] System.Windows.Forms.NativeMethods.LOGFONT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVCOLUMN lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVGROUP lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVHITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.LVINSERTMARK lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out] System.Windows.Forms.NativeMethods.LVTILEVIEWINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.MCHITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.MSG lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out, MarshalAs(UnmanagedType.LPStruct)] System.Windows.Forms.NativeMethods.PARAFORMAT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.POINT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.REPASTESPECIAL lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In, Out] System.Windows.Forms.NativeMethods.SIZE lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.SYSTEMTIME lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.SYSTEMTIMEARRAY lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.TCITEM_T lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.TEXTRANGE lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.TOOLINFO_T lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.TOOLINFO_TOOLTIP lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, System.Windows.Forms.NativeMethods.TV_HITTESTINFO lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, int wParam, [In, Out] ref Rectangle lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, ref short wParam, ref short lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, int wParam, [In, Out] ref System.Windows.Forms.NativeMethods.RECT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, string lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, [In, Out] ref System.Windows.Forms.NativeMethods.RECT lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int Msg, IntPtr wParam, System.Windows.Forms.NativeMethods.ListViewCompareCallback pfnCompare);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, HandleRef wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, System.Windows.Forms.NativeMethods.GETTEXTLENGTHEX wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, System.Windows.Forms.NativeMethods.POINT wParam, int lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, System.Windows.Forms.NativeMethods.POINT wParam, [In, Out] System.Windows.Forms.NativeMethods.LVINSERTMARK lParam);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam, int flags, int timeout, out IntPtr pdwResult);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetActiveWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetCapture(HandleRef hwnd);
        public static IntPtr SetClassLong(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetClassLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetClassLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint="SetClassLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetClassLongPtr32(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetClassLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetClassLongPtr64(HandleRef hwnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetCursor(HandleRef hcursor);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetFocus(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetForegroundWindow(HandleRef hWnd);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int SetKeyboardState(byte[] keystate);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool SetLayeredWindowAttributes(HandleRef hwnd, int crKey, byte bAlpha, int dwFlags);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetMenu(HandleRef hWnd, HandleRef hMenu);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetMenuDefaultItem(HandleRef hwnd, int nIndex, bool pos);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SetMenuItemInfo(HandleRef hMenu, int uItem, bool fByPosition, System.Windows.Forms.NativeMethods.MENUITEMINFO_T lpmii);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern IntPtr SetParent(HandleRef hWnd, HandleRef hWndParent);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int SetScrollInfo(HandleRef hWnd, int fnBar, System.Windows.Forms.NativeMethods.SCROLLINFO si, bool redraw);
        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern int SetScrollPos(HandleRef hWnd, int nBar, int nPos, bool bRedraw);
        [DllImport("Powrprof.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, HandleRef dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, System.Windows.Forms.NativeMethods.WndProc wndproc)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, wndproc);
            }
            return SetWindowLongPtr64(hWnd, nIndex, wndproc);
        }

        [DllImport("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, System.Windows.Forms.NativeMethods.WndProc wndproc);
        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, HandleRef dwNewLong);
        [DllImport("user32.dll", EntryPoint="SetWindowLongPtr", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, System.Windows.Forms.NativeMethods.WndProc wndproc);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool SetWindowPlacement(HandleRef hWnd, [In] ref System.Windows.Forms.NativeMethods.WINDOWPLACEMENT placement);
        public static int SetWindowRgn(HandleRef hwnd, HandleRef hrgn, bool fRedraw)
        {
            int num = IntSetWindowRgn(hwnd, hrgn, fRedraw);
            if (num != 0)
            {
                System.Internal.HandleCollector.Remove((IntPtr) hrgn, System.Windows.Forms.NativeMethods.CommonHandles.GDI);
            }
            return num;
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr SetWindowsHookEx(int hookid, System.Windows.Forms.NativeMethods.HookProc pfnhook, HandleRef hinst, int threadid);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SetWindowText(HandleRef hWnd, string text);
        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
        public static extern int Shell_NotifyIcon(int message, System.Windows.Forms.NativeMethods.NOTIFYICONDATA pnid);
        [DllImport("shell32.dll", CharSet=CharSet.Auto)]
        public static extern IntPtr ShellExecute(HandleRef hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
        [DllImport("shell32.dll", EntryPoint="ShellExecute", CharSet=CharSet.Auto)]
        public static extern IntPtr ShellExecute_NoBFM(HandleRef hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);
        [DllImport("shlwapi.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        internal static extern uint SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, uint cchOutBuf, IntPtr ppvReserved);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern int ShowCursor(bool bShow);
        internal static int SizeOf(System.Type t)
        {
            return Marshal.SizeOf(t);
        }

        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern IStorage StgCreateDocfileOnILockBytes(ILockBytes iLockBytes, int grfMode, int reserved);
        [DllImport("ole32.dll", PreserveSig=false)]
        public static extern IStorage StgOpenStorageOnILockBytes(ILockBytes iLockBytes, IStorage pStgPriority, int grfMode, int sndExcluded, int reserved);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, bool[] flag, bool nUpdate);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref bool value, int ignore);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref int value, int ignore);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref System.Windows.Forms.NativeMethods.HIGHCONTRAST_I rc, int nUpdate);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, ref System.Windows.Forms.NativeMethods.RECT rc, int nUpdate);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] System.Windows.Forms.NativeMethods.LOGFONT font, int nUpdate);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] System.Windows.Forms.NativeMethods.NONCLIENTMETRICS metrics, int nUpdate);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool SystemParametersInfo(int nAction, int nParam, [In, Out] IntPtr[] rc, int nUpdate);
        internal static void ThrowExceptionForHR(int errorCode)
        {
            Marshal.ThrowExceptionForHR(errorCode);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool TranslateMDISysAccel(IntPtr hWndClient, [In, Out] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool TranslateMessage([In, Out] ref System.Windows.Forms.NativeMethods.MSG msg);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern bool UnhookWindowsHookEx(HandleRef hhook);
        public static bool UnmapViewOfFile(HandleRef pvBaseAddress)
        {
            System.Internal.HandleCollector.Remove((IntPtr) pvBaseAddress, System.Windows.Forms.NativeMethods.CommonHandles.Kernel);
            return IntUnmapViewOfFile(pvBaseAddress);
        }

        [DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool UnregisterClass(string className, HandleRef hInstance);
        [DllImport("oleaut32.dll", ExactSpelling=true)]
        public static extern int VarFormat(ref object pvarIn, HandleRef pstrFormat, int iFirstDay, int iFirstWeek, uint dwFlags, [In, Out] ref IntPtr pbstr);
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern short VkKeyScan(char key);
        [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
        public static extern void WaitMessage();
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
        public static extern int WideCharToMultiByte(int codePage, int flags, [MarshalAs(UnmanagedType.LPWStr)] string wideStr, int chars, [In, Out] byte[] pOutBytes, int bufferBytes, IntPtr defaultChar, IntPtr pDefaultUsed);
        [DllImport("user32.dll", SetLastError=true, ExactSpelling=true)]
        public static extern IntPtr WindowFromDC(HandleRef hDC);
        public static IntPtr WindowFromPoint(int x, int y)
        {
            POINTSTRUCT pt = new POINTSTRUCT(x, y);
            return _WindowFromPoint(pt);
        }

        [DllImport("ole32.dll")]
        public static extern int WriteClassStm(IStream pStream, ref Guid clsid);

        internal static bool IsVista
        {
            get
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                if (oSVersion == null)
                {
                    return false;
                }
                return ((oSVersion.Platform == PlatformID.Win32NT) && (oSVersion.Version.CompareTo(VistaOSVersion) >= 0));
            }
        }

        public class AnsiCharBuffer : System.Windows.Forms.UnsafeNativeMethods.CharBuffer
        {
            internal byte[] buffer;
            internal int offset;

            public AnsiCharBuffer(int size)
            {
                this.buffer = new byte[size];
            }

            public override IntPtr AllocCoTaskMem()
            {
                IntPtr destination = Marshal.AllocCoTaskMem(this.buffer.Length);
                Marshal.Copy(this.buffer, 0, destination, this.buffer.Length);
                return destination;
            }

            public override string GetString()
            {
                int offset = this.offset;
                while ((offset < this.buffer.Length) && (this.buffer[offset] != 0))
                {
                    offset++;
                }
                string str = Encoding.Default.GetString(this.buffer, this.offset, offset - this.offset);
                if (offset < this.buffer.Length)
                {
                    offset++;
                }
                this.offset = offset;
                return str;
            }

            public override void PutCoTaskMem(IntPtr ptr)
            {
                Marshal.Copy(ptr, this.buffer, 0, this.buffer.Length);
                this.offset = 0;
            }

            public override void PutString(string s)
            {
                byte[] bytes = Encoding.Default.GetBytes(s);
                int length = Math.Min(bytes.Length, this.buffer.Length - this.offset);
                Array.Copy(bytes, 0, this.buffer, this.offset, length);
                this.offset += length;
                if (this.offset < this.buffer.Length)
                {
                    this.buffer[this.offset++] = 0;
                }
            }
        }

        public delegate int BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        public class BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public int ulFlags;
            public System.Windows.Forms.UnsafeNativeMethods.BrowseCallbackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        [Flags]
        public enum BrowseInfos
        {
            HideNewFolderButton = 0x200,
            NewDialogStyle = 0x40
        }

        public abstract class CharBuffer
        {
            protected CharBuffer()
            {
            }

            public abstract IntPtr AllocCoTaskMem();
            public static System.Windows.Forms.UnsafeNativeMethods.CharBuffer CreateBuffer(int size)
            {
                if (Marshal.SystemDefaultCharSize == 1)
                {
                    return new System.Windows.Forms.UnsafeNativeMethods.AnsiCharBuffer(size);
                }
                return new System.Windows.Forms.UnsafeNativeMethods.UnicodeCharBuffer(size);
            }

            public abstract string GetString();
            public abstract void PutCoTaskMem(IntPtr ptr);
            public abstract void PutString(string s);
        }

        public class ComStreamFromDataStream : System.Windows.Forms.UnsafeNativeMethods.IStream
        {
            protected Stream dataStream;
            private long virtualPosition = -1L;

            public ComStreamFromDataStream(Stream dataStream)
            {
                if (dataStream == null)
                {
                    throw new ArgumentNullException("dataStream");
                }
                this.dataStream = dataStream;
            }

            private void ActualizeVirtualPosition()
            {
                if (this.virtualPosition != -1L)
                {
                    if (this.virtualPosition > this.dataStream.Length)
                    {
                        this.dataStream.SetLength(this.virtualPosition);
                    }
                    this.dataStream.Position = this.virtualPosition;
                    this.virtualPosition = -1L;
                }
            }

            public System.Windows.Forms.UnsafeNativeMethods.IStream Clone()
            {
                NotImplemented();
                return null;
            }

            public void Commit(int grfCommitFlags)
            {
                this.dataStream.Flush();
                this.ActualizeVirtualPosition();
            }

            public long CopyTo(System.Windows.Forms.UnsafeNativeMethods.IStream pstm, long cb, long[] pcbRead)
            {
                int num = 0x1000;
                IntPtr buf = Marshal.AllocHGlobal(num);
                if (buf == IntPtr.Zero)
                {
                    throw new OutOfMemoryException();
                }
                long num2 = 0L;
                try
                {
                    while (num2 < cb)
                    {
                        int length = num;
                        if ((num2 + length) > cb)
                        {
                            length = (int) (cb - num2);
                        }
                        int len = this.Read(buf, length);
                        if (len == 0)
                        {
                            goto Label_006C;
                        }
                        if (pstm.Write(buf, len) != len)
                        {
                            throw EFail("Wrote an incorrect number of bytes");
                        }
                        num2 += len;
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buf);
                }
            Label_006C:
                if ((pcbRead != null) && (pcbRead.Length > 0))
                {
                    pcbRead[0] = num2;
                }
                return num2;
            }

            protected static ExternalException EFail(string msg)
            {
                ExternalException exception = new ExternalException(msg, -2147467259);
                throw exception;
            }

            public Stream GetDataStream()
            {
                return this.dataStream;
            }

            public void LockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            protected static void NotImplemented()
            {
                ExternalException exception = new ExternalException(System.Windows.Forms.SR.GetString("UnsafeNativeMethodsNotImplemented"), -2147467263);
                throw exception;
            }

            public int Read(IntPtr buf, int length)
            {
                byte[] buffer = new byte[length];
                int num = this.Read(buffer, length);
                Marshal.Copy(buffer, 0, buf, num);
                return num;
            }

            public int Read(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                return this.dataStream.Read(buffer, 0, length);
            }

            public void Revert()
            {
                NotImplemented();
            }

            public long Seek(long offset, int origin)
            {
                long virtualPosition = this.virtualPosition;
                if (this.virtualPosition == -1L)
                {
                    virtualPosition = this.dataStream.Position;
                }
                long length = this.dataStream.Length;
                switch (origin)
                {
                    case 0:
                        if (offset > length)
                        {
                            this.virtualPosition = offset;
                            break;
                        }
                        this.dataStream.Position = offset;
                        this.virtualPosition = -1L;
                        break;

                    case 1:
                        if ((offset + virtualPosition) > length)
                        {
                            this.virtualPosition = offset + virtualPosition;
                            break;
                        }
                        this.dataStream.Position = virtualPosition + offset;
                        this.virtualPosition = -1L;
                        break;

                    case 2:
                        if (offset > 0L)
                        {
                            this.virtualPosition = length + offset;
                            break;
                        }
                        this.dataStream.Position = length + offset;
                        this.virtualPosition = -1L;
                        break;
                }
                if (this.virtualPosition != -1L)
                {
                    return this.virtualPosition;
                }
                return this.dataStream.Position;
            }

            public void SetSize(long value)
            {
                this.dataStream.SetLength(value);
            }

            public void Stat(System.Windows.Forms.NativeMethods.STATSTG pstatstg, int grfStatFlag)
            {
                pstatstg.type = 2;
                pstatstg.cbSize = this.dataStream.Length;
                pstatstg.grfLocksSupported = 2;
            }

            public void UnlockRegion(long libOffset, long cb, int dwLockType)
            {
            }

            public int Write(IntPtr buf, int length)
            {
                byte[] destination = new byte[length];
                Marshal.Copy(buf, destination, 0, length);
                return this.Write(destination, length);
            }

            public int Write(byte[] buffer, int length)
            {
                this.ActualizeVirtualPosition();
                this.dataStream.Write(buffer, 0, length);
                return length;
            }
        }

        [ComImport, Guid("CB2F6723-AB3A-11d2-9C40-00C04FA30A3E")]
        internal class CorRuntimeHost
        {
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f610-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLAnchorEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, Guid("3050f611-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLAreaEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f617-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLButtonElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f612-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLControlElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f613-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLDocumentEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x402)]
            bool onstop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x403)]
            void onbeforeeditfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40d)]
            void onselectionchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f60f-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f614-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLFormElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ef)]
            bool onsubmit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f7)]
            bool onreset(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f7ff-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLFrameSiteEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f616-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLImgEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3e8)]
            void onabort(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, Guid("3050f61a-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLInputFileElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412082)]
            bool onchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412102)]
            void onselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3e8)]
            void onabort(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f61b-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLInputImageEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412080)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412083)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412084)]
            void onabort(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f618-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface DHTMLInputTextElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3e9)]
            bool onchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ee)]
            void onselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3e9)]
            void onabort(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f61c-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface DHTMLLabelEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f61d-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLLinkElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412080)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412083)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f61e-98b5-11cf-bb82-00aa00bdce0b"), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLMapEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, Guid("3050f61f-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLMarqueeElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412092)]
            void onbounce(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412086)]
            void onfinish(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412085)]
            void onstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, Guid("3050f619-98b5-11cf-bb82-00aa00bdce0b"), TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface DHTMLOptionButtonElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412082)]
            bool onchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f621-98b5-11cf-bb82-00aa00bdce0b"), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLScriptEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f622-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface DHTMLSelectElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147412082)]
            void onchange_void(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f615-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLStyleElementEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            void onerror(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), Guid("3050f623-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        public interface DHTMLTableEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f624-98b5-11cf-bb82-00aa00bdce0b"), TypeLibType(TypeLibTypeFlags.FHidden)]
        public interface DHTMLTextContainerEvents2
        {
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-600)]
            bool onclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-601)]
            bool ondblclick(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-603)]
            bool onkeypress(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-602)]
            void onkeydown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-604)]
            void onkeyup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418103)]
            void onmouseout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418104)]
            void onmouseover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-606)]
            void onmousemove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-605)]
            void onmousedown(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-607)]
            void onmouseup(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418100)]
            bool onselectstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418095)]
            void onfilterchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418101)]
            bool ondragstart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418108)]
            bool onbeforeupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418107)]
            void onafterupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418099)]
            bool onerrorupdate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418106)]
            bool onrowexit(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418105)]
            void onrowenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418098)]
            void ondatasetchanged(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418097)]
            void ondataavailable(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418096)]
            void ondatasetcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418094)]
            void onlosecapture(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418093)]
            void onpropertychange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418092)]
            bool ondrag(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418091)]
            void ondragend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418090)]
            bool ondragenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418089)]
            bool ondragover(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418088)]
            void ondragleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418087)]
            bool ondrop(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418083)]
            bool onbeforecut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418086)]
            bool oncut(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418082)]
            bool onbeforecopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418085)]
            bool oncopy(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418081)]
            bool onbeforepaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418084)]
            bool onpaste(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ff)]
            bool oncontextmenu(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418080)]
            void onrowsdelete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418079)]
            void onrowsinserted(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418078)]
            void oncellchange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-609)]
            void onreadystatechange(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x406)]
            void onlayoutcomplete(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x407)]
            void onpage(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x412)]
            void onmouseenter(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x413)]
            void onmouseleave(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x414)]
            void onactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x415)]
            void ondeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40a)]
            bool onbeforedeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x417)]
            bool onbeforeactivate(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x418)]
            void onfocusin(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x419)]
            void onfocusout(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40b)]
            void onmove(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40c)]
            bool oncontrolselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40e)]
            bool onmovestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x40f)]
            void onmoveend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x410)]
            bool onresizestart(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x411)]
            void onresizeend(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x409)]
            bool onmousewheel(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3e9)]
            void onchange_void(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ee)]
            void onselect(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050f625-98b5-11cf-bb82-00aa00bdce0b")]
        public interface DHTMLWindowEvents2
        {
            [DispId(0x3eb)]
            void onload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f0)]
            void onunload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418102)]
            bool onhelp(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418111)]
            void onfocus(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(-2147418112)]
            void onblur(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3ea)]
            bool onerror(string description, string url, int line);
            [DispId(0x3f8)]
            void onresize(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f6)]
            void onscroll(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x3f9)]
            void onbeforeunload(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x400)]
            void onbeforeprint(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
            [DispId(0x401)]
            void onafterprint(System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj evtObj);
        }

        [ComImport, TypeLibType(TypeLibTypeFlags.FHidden), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("34A715A0-6587-11D0-924A-0020AFC7AC4D")]
        public interface DWebBrowserEvents2
        {
            [DispId(0x66)]
            void StatusTextChange([In] string text);
            [DispId(0x6c)]
            void ProgressChange([In] int progress, [In] int progressMax);
            [DispId(0x69)]
            void CommandStateChange([In] long command, [In] bool enable);
            [DispId(0x6a)]
            void DownloadBegin();
            [DispId(0x68)]
            void DownloadComplete();
            [DispId(0x71)]
            void TitleChange([In] string text);
            [DispId(0x70)]
            void PropertyChange([In] string szProperty);
            [DispId(250)]
            void BeforeNavigate2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers, [In, Out] ref bool cancel);
            [DispId(0xfb)]
            void NewWindow2([In, Out, MarshalAs(UnmanagedType.IDispatch)] ref object pDisp, [In, Out] ref bool cancel);
            [DispId(0xfc)]
            void NavigateComplete2([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL);
            [DispId(0x103)]
            void DocumentComplete([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL);
            [DispId(0xfd)]
            void OnQuit();
            [DispId(0xfe)]
            void OnVisible([In] bool visible);
            [DispId(0xff)]
            void OnToolBar([In] bool toolBar);
            [DispId(0x100)]
            void OnMenuBar([In] bool menuBar);
            [DispId(0x101)]
            void OnStatusBar([In] bool statusBar);
            [DispId(0x102)]
            void OnFullScreen([In] bool fullScreen);
            [DispId(260)]
            void OnTheaterMode([In] bool theaterMode);
            [DispId(0x106)]
            void WindowSetResizable([In] bool resizable);
            [DispId(0x108)]
            void WindowSetLeft([In] int left);
            [DispId(0x109)]
            void WindowSetTop([In] int top);
            [DispId(0x10a)]
            void WindowSetWidth([In] int width);
            [DispId(0x10b)]
            void WindowSetHeight([In] int height);
            [DispId(0x107)]
            void WindowClosing([In] bool isChildWindow, [In, Out] ref bool cancel);
            [DispId(0x10c)]
            void ClientToHostWindow([In, Out] ref long cx, [In, Out] ref long cy);
            [DispId(0x10d)]
            void SetSecureLockIcon([In] int secureLockIcon);
            [DispId(270)]
            void FileDownload([In, Out] ref bool cancel);
            [DispId(0x10f)]
            void NavigateError([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object URL, [In] ref object frame, [In] ref object statusCode, [In, Out] ref bool cancel);
            [DispId(0xe1)]
            void PrintTemplateInstantiation([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);
            [DispId(0xe2)]
            void PrintTemplateTeardown([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp);
            [DispId(0xe3)]
            void UpdatePageStatus([In, MarshalAs(UnmanagedType.IDispatch)] object pDisp, [In] ref object nPage, [In] ref object fDone);
            [DispId(0x110)]
            void PrivacyImpactedStateChange([In] bool bImpacted);
        }

        [ComImport, Guid("618736E0-3C3D-11CF-810C-00AA00389B71"), TypeLibType((short) 0x1050)]
        public interface IAccessibleInternal
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [TypeLibFunc((short) 0x40), DispId(-5000)]
            object get_accParent();
            [TypeLibFunc((short) 0x40), DispId(-5001)]
            int get_accChildCount();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [TypeLibFunc((short) 0x40), DispId(-5002)]
            object get_accChild([In, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(-5003), TypeLibFunc((short) 0x40)]
            string get_accName([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(-5004), TypeLibFunc((short) 0x40)]
            string get_accValue([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(-5005), TypeLibFunc((short) 0x40)]
            string get_accDescription([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.Struct)]
            [TypeLibFunc((short) 0x40), DispId(-5006)]
            object get_accRole([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.Struct)]
            [DispId(-5007), TypeLibFunc((short) 0x40)]
            object get_accState([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            [TypeLibFunc((short) 0x40), DispId(-5008)]
            string get_accHelp([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [DispId(-5009), TypeLibFunc((short) 0x40)]
            int get_accHelpTopic([MarshalAs(UnmanagedType.BStr)] out string pszHelpFile, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.BStr)]
            [DispId(-5010), TypeLibFunc((short) 0x40)]
            string get_accKeyboardShortcut([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.Struct)]
            [DispId(-5011), TypeLibFunc((short) 0x40)]
            object get_accFocus();
            [return: MarshalAs(UnmanagedType.Struct)]
            [TypeLibFunc((short) 0x40), DispId(-5012)]
            object get_accSelection();
            [return: MarshalAs(UnmanagedType.BStr)]
            [TypeLibFunc((short) 0x40), DispId(-5013)]
            string get_accDefaultAction([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [DispId(-5014), TypeLibFunc((short) 0x40)]
            void accSelect([In] int flagsSelect, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [DispId(-5015), TypeLibFunc((short) 0x40)]
            void accLocation(out int pxLeft, out int pyTop, out int pcxWidth, out int pcyHeight, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [return: MarshalAs(UnmanagedType.Struct)]
            [TypeLibFunc((short) 0x40), DispId(-5016)]
            object accNavigate([In] int navDir, [In, Optional, MarshalAs(UnmanagedType.Struct)] object varStart);
            [return: MarshalAs(UnmanagedType.Struct)]
            [TypeLibFunc((short) 0x40), DispId(-5017)]
            object accHitTest([In] int xLeft, [In] int yTop);
            [TypeLibFunc((short) 0x40), DispId(-5018)]
            void accDoDefaultAction([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild);
            [DispId(-5003), TypeLibFunc((short) 0x40)]
            void set_accName([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild, [In, MarshalAs(UnmanagedType.BStr)] string pszName);
            [DispId(-5004), TypeLibFunc((short) 0x40)]
            void set_accValue([In, Optional, MarshalAs(UnmanagedType.Struct)] object varChild, [In, MarshalAs(UnmanagedType.BStr)] string pszValue);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("00bb2762-6a77-11d0-a535-00c04fd7d062"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAutoComplete
        {
            int Init([In] HandleRef hwndEdit, [In] IEnumString punkACL, [In] string pwszRegKeyPath, [In] string pwszQuickComplete);
            void Enable([In] bool fEnable);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, Guid("EAC04BC0-3791-11d2-BB95-0060977B464C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAutoComplete2
        {
            int Init([In] HandleRef hwndEdit, [In] IEnumString punkACL, [In] string pwszRegKeyPath, [In] string pwszQuickComplete);
            void Enable([In] bool fEnable);
            int SetOptions([In] int dwFlag);
            void GetOptions([Out] IntPtr pdwFlag);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
        public interface IClassFactory2
        {
            void CreateInstance([In, MarshalAs(UnmanagedType.Interface)] object unused, [In] ref Guid refiid, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppunk);
            void LockServer(int fLock);
            void GetLicInfo([Out] System.Windows.Forms.NativeMethods.tagLICINFO licInfo);
            void RequestLicKey([In, MarshalAs(UnmanagedType.U4)] int dwReserved, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrKey);
            void CreateInstanceLic([In, MarshalAs(UnmanagedType.Interface)] object pUnkOuter, [In, MarshalAs(UnmanagedType.Interface)] object pUnkReserved, [In] ref Guid riid, [In, MarshalAs(UnmanagedType.BStr)] string bstrKey, [MarshalAs(UnmanagedType.Interface)] out object ppVal);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B196B286-BAB4-101A-B69C-00AA00341D07"), SuppressUnmanagedCodeSecurity]
        public interface IConnectionPoint
        {
            [PreserveSig]
            int GetConnectionInterface(out Guid iid);
            [PreserveSig]
            int GetConnectionPointContainer([MarshalAs(UnmanagedType.Interface)] ref System.Windows.Forms.UnsafeNativeMethods.IConnectionPointContainer pContainer);
            [PreserveSig]
            int Advise([In, MarshalAs(UnmanagedType.Interface)] object pUnkSink, ref int cookie);
            [PreserveSig]
            int Unadvise(int cookie);
            [PreserveSig]
            int EnumConnections(out object pEnum);
        }

        [ComImport, Guid("B196B284-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        public interface IConnectionPointContainer
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object EnumConnectionPoints();
            [PreserveSig]
            int FindConnectionPoint([In] ref Guid guid, [MarshalAs(UnmanagedType.Interface)] out System.Windows.Forms.UnsafeNativeMethods.IConnectionPoint ppCP);
        }

        [ComImport, Guid("CB2F6722-AB3A-11d2-9C40-00C04FA30A3E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICorRuntimeHost
        {
            [PreserveSig]
            int CreateLogicalThreadState();
            [PreserveSig]
            int DeleteLogicalThreadState();
            [PreserveSig]
            int SwitchInLogicalThreadState([In] ref uint pFiberCookie);
            [PreserveSig]
            int SwitchOutLogicalThreadState(out uint FiberCookie);
            [PreserveSig]
            int LocksHeldByLogicalThread(out uint pCount);
            [PreserveSig]
            int MapFile(IntPtr hFile, out IntPtr hMapAddress);
            [PreserveSig]
            int GetConfiguration([MarshalAs(UnmanagedType.IUnknown)] out object pConfiguration);
            [PreserveSig]
            int Start();
            [PreserveSig]
            int Stop();
            [PreserveSig]
            int CreateDomain(string pwzFriendlyName, [MarshalAs(UnmanagedType.IUnknown)] object pIdentityArray, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int EnumDomains(out IntPtr hEnum);
            [PreserveSig]
            int NextDomain(IntPtr hEnum, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int CloseEnum(IntPtr hEnum);
            [PreserveSig]
            int CreateDomainEx(string pwzFriendlyName, [MarshalAs(UnmanagedType.IUnknown)] object pSetup, [MarshalAs(UnmanagedType.IUnknown)] object pEvidence, [MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
            [PreserveSig]
            int CreateDomainSetup([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomainSetup);
            [PreserveSig]
            int CreateEvidence([MarshalAs(UnmanagedType.IUnknown)] out object pEvidence);
            [PreserveSig]
            int UnloadDomain([MarshalAs(UnmanagedType.IUnknown)] object pAppDomain);
            [PreserveSig]
            int CurrentDomain([MarshalAs(UnmanagedType.IUnknown)] out object pAppDomain);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020400-0000-0000-C000-000000000046")]
        public interface IDispatch
        {
            int GetTypeInfoCount();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITypeInfo GetTypeInfo([In, MarshalAs(UnmanagedType.U4)] int iTInfo, [In, MarshalAs(UnmanagedType.U4)] int lcid);
            [PreserveSig]
            int GetIDsOfNames([In] ref Guid riid, [In, MarshalAs(UnmanagedType.LPArray)] string[] rgszNames, [In, MarshalAs(UnmanagedType.U4)] int cNames, [In, MarshalAs(UnmanagedType.U4)] int lcid, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);
            [PreserveSig]
            int Invoke(int dispIdMember, [In] ref Guid riid, [In, MarshalAs(UnmanagedType.U4)] int lcid, [In, MarshalAs(UnmanagedType.U4)] int dwFlags, [In, Out] System.Windows.Forms.NativeMethods.tagDISPPARAMS pDispParams, [Out, MarshalAs(UnmanagedType.LPArray)] object[] pVarResult, [In, Out] System.Windows.Forms.NativeMethods.tagEXCEPINFO pExcepInfo, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pArgErr);
        }

        [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true)]
        public interface IDocHostUIHandler
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowContextMenu([In, MarshalAs(UnmanagedType.U4)] int dwID, [In] System.Windows.Forms.NativeMethods.POINT pt, [In, MarshalAs(UnmanagedType.Interface)] object pcmdtReserved, [In, MarshalAs(UnmanagedType.Interface)] object pdispReserved);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetHostInfo([In, Out] System.Windows.Forms.NativeMethods.DOCHOSTUIINFO info);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ShowUI([In, MarshalAs(UnmanagedType.I4)] int dwID, [In] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject activeObject, [In] System.Windows.Forms.NativeMethods.IOleCommandTarget commandTarget, [In] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame frame, [In] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow doc);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int HideUI();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int UpdateUI();
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int EnableModeless([In, MarshalAs(UnmanagedType.Bool)] bool fEnable);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnDocWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int OnFrameWindowActivate([In, MarshalAs(UnmanagedType.Bool)] bool fActivate);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ResizeBorder([In] System.Windows.Forms.NativeMethods.COMRECT rect, [In] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow doc, bool fFrameWindow);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Windows.Forms.NativeMethods.MSG msg, [In] ref Guid group, [In, MarshalAs(UnmanagedType.I4)] int nCmdID);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, [In, MarshalAs(UnmanagedType.U4)] int dw);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleDropTarget pDropTarget, [MarshalAs(UnmanagedType.Interface)] out System.Windows.Forms.UnsafeNativeMethods.IOleDropTarget ppDropTarget);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int GetExternal([MarshalAs(UnmanagedType.Interface)] out object ppDispatch);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int TranslateUrl([In, MarshalAs(UnmanagedType.U4)] int dwTranslate, [In, MarshalAs(UnmanagedType.LPWStr)] string strURLIn, [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B196B285-BAB4-101A-B69C-00AA00341D07")]
        public interface IEnumConnectionPoints
        {
            [PreserveSig]
            int Next(int cConnections, out System.Windows.Forms.UnsafeNativeMethods.IConnectionPoint pCp, out int pcFetched);
            [PreserveSig]
            int Skip(int cSkip);
            void Reset();
            System.Windows.Forms.UnsafeNativeMethods.IEnumConnectionPoints Clone();
        }

        [ComImport, Guid("00000104-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumOLEVERB
        {
            [PreserveSig]
            int Next([MarshalAs(UnmanagedType.U4)] int celt, [Out] System.Windows.Forms.NativeMethods.tagOLEVERB rgelt, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000100-0000-0000-C000-000000000046")]
        public interface IEnumUnknown
        {
            [PreserveSig]
            int Next([In, MarshalAs(UnmanagedType.U4)] int celt, [Out] IntPtr rgelt, IntPtr pceltFetched);
            [PreserveSig]
            int Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone(out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020404-0000-0000-C000-000000000046")]
        public interface IEnumVariant
        {
            [PreserveSig]
            int Next([In, MarshalAs(UnmanagedType.U4)] int celt, [In, Out] IntPtr rgvar, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            void Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            void Reset();
            void Clone([Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.IEnumVariant[] ppenum);
        }

        [ComImport, Guid("1CF2B120-547D-101B-8E65-08002B2BD119"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IErrorInfo
        {
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetGUID(out Guid pguid);
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetSource([In, Out, MarshalAs(UnmanagedType.BStr)] ref string pBstrSource);
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetDescription([In, Out, MarshalAs(UnmanagedType.BStr)] ref string pBstrDescription);
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetHelpFile([In, Out, MarshalAs(UnmanagedType.BStr)] ref string pBstrHelpFile);
            [PreserveSig, SuppressUnmanagedCodeSecurity]
            int GetHelpContext([In, Out, MarshalAs(UnmanagedType.U4)] ref int pdwHelpContext);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3127CA40-446E-11CE-8135-00AA004BB851")]
        public interface IErrorLog
        {
            void AddError([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName_p0, [In, MarshalAs(UnmanagedType.Struct)] System.Windows.Forms.NativeMethods.tagEXCEPINFO pExcepInfo_p1);
        }

        [ComImport, Guid("39088D7E-B71E-11D1-8F39-00C04FD946D0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IExtender
        {
            int Align { get; set; }
            bool Enabled { get; set; }
            int Height { get; set; }
            int Left { get; set; }
            bool TabStop { get; set; }
            int Top { get; set; }
            bool Visible { get; set; }
            int Width { get; set; }
            string Name { [return: MarshalAs(UnmanagedType.BStr)] get; }
            object Parent { [return: MarshalAs(UnmanagedType.Interface)] get; }
            IntPtr Hwnd { get; }
            object Container { [return: MarshalAs(UnmanagedType.Interface)] get; }
            void Move([In, MarshalAs(UnmanagedType.Interface)] object left, [In, MarshalAs(UnmanagedType.Interface)] object top, [In, MarshalAs(UnmanagedType.Interface)] object width, [In, MarshalAs(UnmanagedType.Interface)] object height);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("BEF6E002-A874-101A-8BBA-00AA00300CAB")]
        public interface IFont
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            string GetName();
            void SetName([In, MarshalAs(UnmanagedType.BStr)] string pname);
            [return: MarshalAs(UnmanagedType.U8)]
            long GetSize();
            void SetSize([In, MarshalAs(UnmanagedType.U8)] long psize);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetBold();
            void SetBold([In, MarshalAs(UnmanagedType.Bool)] bool pbold);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetItalic();
            void SetItalic([In, MarshalAs(UnmanagedType.Bool)] bool pitalic);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetUnderline();
            void SetUnderline([In, MarshalAs(UnmanagedType.Bool)] bool punderline);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetStrikethrough();
            void SetStrikethrough([In, MarshalAs(UnmanagedType.Bool)] bool pstrikethrough);
            [return: MarshalAs(UnmanagedType.I2)]
            short GetWeight();
            void SetWeight([In, MarshalAs(UnmanagedType.I2)] short pweight);
            [return: MarshalAs(UnmanagedType.I2)]
            short GetCharset();
            void SetCharset([In, MarshalAs(UnmanagedType.I2)] short pcharset);
            IntPtr GetHFont();
            void Clone(out System.Windows.Forms.UnsafeNativeMethods.IFont ppfont);
            [PreserveSig]
            int IsEqual([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IFont pfontOther);
            void SetRatio(int cyLogical, int cyHimetric);
            void QueryTextMetrics(out IntPtr ptm);
            void AddRefHfont(IntPtr hFont);
            void ReleaseHfont(IntPtr hFont);
            void SetHdc(IntPtr hdc);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("8A701DA0-4FEB-101B-A82E-08002B2B2337")]
        public interface IGetOleObject
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetOleObject(ref Guid riid);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("91733A60-3F4C-101B-A3F6-00AA0034E4E9")]
        public interface IGetVBAObject
        {
            [PreserveSig]
            int GetObject([In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.IVBFormat[] rval, int dwReserved);
        }

        [Guid("626FC520-A41E-11cf-A731-00A0C9082637"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
        internal interface IHTMLDocument
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetScript();
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("332C4425-26CB-11D0-B483-00C04FD90119"), SuppressUnmanagedCodeSecurity, ComVisible(true)]
        internal interface IHTMLDocument2
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetScript();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetAll();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetBody();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetActiveElement();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetImages();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetApplets();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetLinks();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetForms();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetAnchors();
            void SetTitle(string p);
            string GetTitle();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetScripts();
            void SetDesignMode(string p);
            string GetDesignMode();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetSelection();
            string GetReadyState();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetFrames();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetEmbeds();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetPlugins();
            void SetAlinkColor(object c);
            object GetAlinkColor();
            void SetBgColor(object c);
            object GetBgColor();
            void SetFgColor(object c);
            object GetFgColor();
            void SetLinkColor(object c);
            object GetLinkColor();
            void SetVlinkColor(object c);
            object GetVlinkColor();
            string GetReferrer();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLLocation GetLocation();
            string GetLastModified();
            void SetUrl(string p);
            string GetUrl();
            void SetDomain(string p);
            string GetDomain();
            void SetCookie(string p);
            string GetCookie();
            void SetExpando(bool p);
            bool GetExpando();
            void SetCharset(string p);
            string GetCharset();
            void SetDefaultCharset(string p);
            string GetDefaultCharset();
            string GetMimeType();
            string GetFileSize();
            string GetFileCreatedDate();
            string GetFileModifiedDate();
            string GetFileUpdatedDate();
            string GetSecurity();
            string GetProtocol();
            string GetNameProp();
            int Write([In, MarshalAs(UnmanagedType.SafeArray)] object[] psarray);
            int WriteLine([In, MarshalAs(UnmanagedType.SafeArray)] object[] psarray);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Open(string mimeExtension, object name, object features, object replace);
            void Close();
            void Clear();
            bool QueryCommandSupported(string cmdID);
            bool QueryCommandEnabled(string cmdID);
            bool QueryCommandState(string cmdID);
            bool QueryCommandIndeterm(string cmdID);
            string QueryCommandText(string cmdID);
            object QueryCommandValue(string cmdID);
            bool ExecCommand(string cmdID, bool showUI, object value);
            bool ExecCommandShowHelp(string cmdID);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement CreateElement(string eTag);
            void SetOnhelp(object p);
            object GetOnhelp();
            void SetOnclick(object p);
            object GetOnclick();
            void SetOndblclick(object p);
            object GetOndblclick();
            void SetOnkeyup(object p);
            object GetOnkeyup();
            void SetOnkeydown(object p);
            object GetOnkeydown();
            void SetOnkeypress(object p);
            object GetOnkeypress();
            void SetOnmouseup(object p);
            object GetOnmouseup();
            void SetOnmousedown(object p);
            object GetOnmousedown();
            void SetOnmousemove(object p);
            object GetOnmousemove();
            void SetOnmouseout(object p);
            object GetOnmouseout();
            void SetOnmouseover(object p);
            object GetOnmouseover();
            void SetOnreadystatechange(object p);
            object GetOnreadystatechange();
            void SetOnafterupdate(object p);
            object GetOnafterupdate();
            void SetOnrowexit(object p);
            object GetOnrowexit();
            void SetOnrowenter(object p);
            object GetOnrowenter();
            void SetOndragstart(object p);
            object GetOndragstart();
            void SetOnselectstart(object p);
            object GetOnselectstart();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement ElementFromPoint(int x, int y);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 GetParentWindow();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetStyleSheets();
            void SetOnbeforeupdate(object p);
            object GetOnbeforeupdate();
            void SetOnerrorupdate(object p);
            object GetOnerrorupdate();
            string toString();
            [return: MarshalAs(UnmanagedType.Interface)]
            object CreateStyleSheet(string bstrHref, int lIndex);
        }

        [Guid("3050F485-98B5-11CF-BB82-00AA00BDCE0B"), ComVisible(true), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLDocument3
        {
            void ReleaseCapture();
            void Recalc([In] bool fForce);
            object CreateTextNode([In] string text);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetDocumentElement();
            string GetUniqueID();
            bool AttachEvent([In] string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            void DetachEvent([In] string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            void SetOnrowsdelete([In] object p);
            object GetOnrowsdelete();
            void SetOnrowsinserted([In] object p);
            object GetOnrowsinserted();
            void SetOncellchange([In] object p);
            object GetOncellchange();
            void SetOndatasetchanged([In] object p);
            object GetOndatasetchanged();
            void SetOndataavailable([In] object p);
            object GetOndataavailable();
            void SetOndatasetcomplete([In] object p);
            object GetOndatasetcomplete();
            void SetOnpropertychange([In] object p);
            object GetOnpropertychange();
            void SetDir([In] string p);
            string GetDir();
            void SetOncontextmenu([In] object p);
            object GetOncontextmenu();
            void SetOnstop([In] object p);
            object GetOnstop();
            object CreateDocumentFragment();
            object GetParentDocument();
            void SetEnableDownload([In] bool p);
            bool GetEnableDownload();
            void SetBaseUrl([In] string p);
            string GetBaseUrl();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetChildNodes();
            void SetInheritStyleSheets([In] bool p);
            bool GetInheritStyleSheets();
            void SetOnbeforeeditfocus([In] object p);
            object GetOnbeforeeditfocus();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetElementsByName([In] string v);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetElementById([In] string v);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetElementsByTagName([In] string v);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity, ComVisible(true), Guid("3050F69A-98B5-11CF-BB82-00AA00BDCE0B")]
        internal interface IHTMLDocument4
        {
            void Focus();
            bool HasFocus();
            void SetOnselectionchange(object p);
            object GetOnselectionchange();
            object GetNamespaces();
            object createDocumentFromUrl(string bstrUrl, string bstrOptions);
            void SetMedia(string bstrMedia);
            string GetMedia();
            object CreateEventObject([In, Optional] ref object eventObject);
            bool FireEvent(string eventName);
            object CreateRenderStyle(string bstr);
            void SetOncontrolselect(object p);
            object GetOncontrolselect();
            string GetURLUnencoded();
        }

        [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050f5da-98b5-11cf-bb82-00aa00bdce0b"), SuppressUnmanagedCodeSecurity]
        public interface IHTMLDOMNode
        {
            long GetNodeType();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode GetParentNode();
            bool HasChildNodes();
            object GetChildNodes();
            object GetAttributes();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode InsertBefore(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode newChild, object refChild);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode RemoveChild(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode oldChild);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode ReplaceChild(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode newChild, System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode oldChild);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode CloneNode(bool fDeep);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode RemoveNode(bool fDeep);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode SwapNode(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode otherNode);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode ReplaceNode(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode replacement);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode AppendChild(System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode newChild);
            string NodeName();
            void SetNodeValue(object v);
            object GetNodeValue();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode FirstChild();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode LastChild();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode PreviousSibling();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDOMNode NextSibling();
        }

        [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity, Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B")]
        internal interface IHTMLElement
        {
            void SetAttribute(string attributeName, object attributeValue, int lFlags);
            object GetAttribute(string attributeName, int lFlags);
            bool RemoveAttribute(string strAttributeName, int lFlags);
            void SetClassName(string p);
            string GetClassName();
            void SetId(string p);
            string GetId();
            string GetTagName();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetParentElement();
            System.Windows.Forms.UnsafeNativeMethods.IHTMLStyle GetStyle();
            void SetOnhelp(object p);
            object GetOnhelp();
            void SetOnclick(object p);
            object GetOnclick();
            void SetOndblclick(object p);
            object GetOndblclick();
            void SetOnkeydown(object p);
            object GetOnkeydown();
            void SetOnkeyup(object p);
            object GetOnkeyup();
            void SetOnkeypress(object p);
            object GetOnkeypress();
            void SetOnmouseout(object p);
            object GetOnmouseout();
            void SetOnmouseover(object p);
            object GetOnmouseover();
            void SetOnmousemove(object p);
            object GetOnmousemove();
            void SetOnmousedown(object p);
            object GetOnmousedown();
            void SetOnmouseup(object p);
            object GetOnmouseup();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 GetDocument();
            void SetTitle(string p);
            string GetTitle();
            void SetLanguage(string p);
            string GetLanguage();
            void SetOnselectstart(object p);
            object GetOnselectstart();
            void ScrollIntoView(object varargStart);
            bool Contains(System.Windows.Forms.UnsafeNativeMethods.IHTMLElement pChild);
            int GetSourceIndex();
            object GetRecordNumber();
            void SetLang(string p);
            string GetLang();
            int GetOffsetLeft();
            int GetOffsetTop();
            int GetOffsetWidth();
            int GetOffsetHeight();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetOffsetParent();
            void SetInnerHTML(string p);
            string GetInnerHTML();
            void SetInnerText(string p);
            string GetInnerText();
            void SetOuterHTML(string p);
            string GetOuterHTML();
            void SetOuterText(string p);
            string GetOuterText();
            void InsertAdjacentHTML(string where, string html);
            void InsertAdjacentText(string where, string text);
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetParentTextEdit();
            bool GetIsTextEdit();
            void Click();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetFilters();
            void SetOndragstart(object p);
            object GetOndragstart();
            string toString();
            void SetOnbeforeupdate(object p);
            object GetOnbeforeupdate();
            void SetOnafterupdate(object p);
            object GetOnafterupdate();
            void SetOnerrorupdate(object p);
            object GetOnerrorupdate();
            void SetOnrowexit(object p);
            object GetOnrowexit();
            void SetOnrowenter(object p);
            object GetOnrowenter();
            void SetOndatasetchanged(object p);
            object GetOndatasetchanged();
            void SetOndataavailable(object p);
            object GetOndataavailable();
            void SetOndatasetcomplete(object p);
            object GetOndatasetcomplete();
            void SetOnfilterchange(object p);
            object GetOnfilterchange();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetChildren();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetAll();
        }

        [ComVisible(true), Guid("3050f434-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity]
        internal interface IHTMLElement2
        {
            string ScopeName();
            void SetCapture(bool containerCapture);
            void ReleaseCapture();
            void SetOnLoseCapture(object v);
            object GetOnLoseCapture();
            string GetComponentFromPoint(int x, int y);
            void DoScroll(object component);
            void SetOnScroll(object v);
            object GetOnScroll();
            void SetOnDrag(object v);
            object GetOnDrag();
            void SetOnDragEnd(object v);
            object GetOnDragEnd();
            void SetOnDragEnter(object v);
            object GetOnDragEnter();
            void SetOnDragOver(object v);
            object GetOnDragOver();
            void SetOnDragleave(object v);
            object GetOnDragLeave();
            void SetOnDrop(object v);
            object GetOnDrop();
            void SetOnBeforeCut(object v);
            object GetOnBeforeCut();
            void SetOnCut(object v);
            object GetOnCut();
            void SetOnBeforeCopy(object v);
            object GetOnBeforeCopy();
            void SetOnCopy(object v);
            object GetOnCopy(object p);
            void SetOnBeforePaste(object v);
            object GetOnBeforePaste(object p);
            void SetOnPaste(object v);
            object GetOnPaste(object p);
            object GetCurrentStyle();
            void SetOnPropertyChange(object v);
            object GetOnPropertyChange(object p);
            object GetClientRects();
            object GetBoundingClientRect();
            void SetExpression(string propName, string expression, string language);
            object GetExpression(string propName);
            bool RemoveExpression(string propName);
            void SetTabIndex(int v);
            short GetTabIndex();
            void Focus();
            void SetAccessKey(string v);
            string GetAccessKey();
            void SetOnBlur(object v);
            object GetOnBlur();
            void SetOnFocus(object v);
            object GetOnFocus();
            void SetOnResize(object v);
            object GetOnResize();
            void Blur();
            void AddFilter(object pUnk);
            void RemoveFilter(object pUnk);
            int ClientHeight();
            int ClientWidth();
            int ClientTop();
            int ClientLeft();
            bool AttachEvent(string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            void DetachEvent(string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            object ReadyState();
            void SetOnReadyStateChange(object v);
            object GetOnReadyStateChange();
            void SetOnRowsDelete(object v);
            object GetOnRowsDelete();
            void SetOnRowsInserted(object v);
            object GetOnRowsInserted();
            void SetOnCellChange(object v);
            object GetOnCellChange();
            void SetDir(string v);
            string GetDir();
            object CreateControlRange();
            int GetScrollHeight();
            int GetScrollWidth();
            void SetScrollTop(int v);
            int GetScrollTop();
            void SetScrollLeft(int v);
            int GetScrollLeft();
            void ClearAttributes();
            void MergeAttributes(object mergeThis);
            void SetOnContextMenu(object v);
            object GetOnContextMenu();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement InsertAdjacentElement(string where, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IHTMLElement insertedElement);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement applyElement([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IHTMLElement apply, string where);
            string GetAdjacentText(string where);
            string ReplaceAdjacentText(string where, string newText);
            bool CanHaveChildren();
            int AddBehavior(string url, ref object oFactory);
            bool RemoveBehavior(int cookie);
            object GetRuntimeStyle();
            object GetBehaviorUrns();
            void SetTagUrn(string v);
            string GetTagUrn();
            void SetOnBeforeEditFocus(object v);
            object GetOnBeforeEditFocus();
            int GetReadyStateValue();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElementCollection GetElementsByTagName(string v);
        }

        [ComVisible(true), Guid("3050f673-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity]
        internal interface IHTMLElement3
        {
            void MergeAttributes(object mergeThis, object pvarFlags);
            bool IsMultiLine();
            bool CanHaveHTML();
            void SetOnLayoutComplete(object v);
            object GetOnLayoutComplete();
            void SetOnPage(object v);
            object GetOnPage();
            void SetInflateBlock(bool v);
            bool GetInflateBlock();
            void SetOnBeforeDeactivate(object v);
            object GetOnBeforeDeactivate();
            void SetActive();
            void SetContentEditable(string v);
            string GetContentEditable();
            bool IsContentEditable();
            void SetHideFocus(bool v);
            bool GetHideFocus();
            void SetDisabled(bool v);
            bool GetDisabled();
            bool IsDisabled();
            void SetOnMove(object v);
            object GetOnMove();
            void SetOnControlSelect(object v);
            object GetOnControlSelect();
            bool FireEvent(string bstrEventName, object pvarEventObject);
            void SetOnResizeStart(object v);
            object GetOnResizeStart();
            void SetOnResizeEnd(object v);
            object GetOnResizeEnd();
            void SetOnMoveStart(object v);
            object GetOnMoveStart();
            void SetOnMoveEnd(object v);
            object GetOnMoveEnd();
            void SetOnMouseEnter(object v);
            object GetOnMouseEnter();
            void SetOnMouseLeave(object v);
            object GetOnMouseLeave();
            void SetOnActivate(object v);
            object GetOnActivate();
            void SetOnDeactivate(object v);
            object GetOnDeactivate();
            bool DragDrop();
            int GlyphMode();
        }

        [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"), SuppressUnmanagedCodeSecurity]
        internal interface IHTMLElementCollection
        {
            string toString();
            void SetLength(int p);
            int GetLength();
            [return: MarshalAs(UnmanagedType.Interface)]
            object Get_newEnum();
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object Item(object idOrName, object index);
            [return: MarshalAs(UnmanagedType.Interface)]
            object Tags(object tagName);
        }

        [ComVisible(true), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("3050F32D-98B5-11CF-BB82-00AA00BDCE0B")]
        internal interface IHTMLEventObj
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetSrcElement();
            bool GetAltKey();
            bool GetCtrlKey();
            bool GetShiftKey();
            void SetReturnValue(object p);
            object GetReturnValue();
            void SetCancelBubble(bool p);
            bool GetCancelBubble();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetFromElement();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLElement GetToElement();
            void SetKeyCode([In] int p);
            int GetKeyCode();
            int GetButton();
            string GetEventType();
            string GetQualifier();
            int GetReason();
            int GetX();
            int GetY();
            int GetClientX();
            int GetClientY();
            int GetOffsetX();
            int GetOffsetY();
            int GetScreenX();
            int GetScreenY();
            object GetSrcFilter();
        }

        [ComVisible(true), Guid("3050f48B-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity]
        internal interface IHTMLEventObj2
        {
            void SetAttribute(string attributeName, object attributeValue, int lFlags);
            object GetAttribute(string attributeName, int lFlags);
            bool RemoveAttribute(string attributeName, int lFlags);
            void SetPropertyName(string name);
            string GetPropertyName();
            void SetBookmarks(ref object bm);
            object GetBookmarks();
            void SetRecordset(ref object rs);
            object GetRecordset();
            void SetDataFld(string df);
            string GetDataFld();
            void SetBoundElements(ref object be);
            object GetBoundElements();
            void SetRepeat(bool r);
            bool GetRepeat();
            void SetSrcUrn(string urn);
            string GetSrcUrn();
            void SetSrcElement(ref object se);
            object GetSrcElement();
            void SetAltKey(bool alt);
            bool GetAltKey();
            void SetCtrlKey(bool ctrl);
            bool GetCtrlKey();
            void SetShiftKey(bool shift);
            bool GetShiftKey();
            void SetFromElement(ref object element);
            object GetFromElement();
            void SetToElement(ref object element);
            object GetToElement();
            void SetButton(int b);
            int GetButton();
            void SetType(string type);
            string GetType();
            void SetQualifier(string q);
            string GetQualifier();
            void SetReason(int r);
            int GetReason();
            void SetX(int x);
            int GetX();
            void SetY(int y);
            int GetY();
            void SetClientX(int x);
            int GetClientX();
            void SetClientY(int y);
            int GetClientY();
            void SetOffsetX(int x);
            int GetOffsetX();
            void SetOffsetY(int y);
            int GetOffsetY();
            void SetScreenX(int x);
            int GetScreenX();
            void SetScreenY(int y);
            int GetScreenY();
            void SetSrcFilter(ref object filter);
            object GetSrcFilter();
            object GetDataTransfer();
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), SuppressUnmanagedCodeSecurity, Guid("3050f814-98b5-11cf-bb82-00aa00bdce0b")]
        internal interface IHTMLEventObj4
        {
            int GetWheelDelta();
        }

        [ComVisible(true), SuppressUnmanagedCodeSecurity, Guid("332C4426-26CB-11D0-B483-00C04FD90119"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
        internal interface IHTMLFramesCollection2
        {
            object Item(ref object idOrName);
            int GetLength();
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), SuppressUnmanagedCodeSecurity, Guid("163BB1E0-6E00-11CF-837A-48DC04C10000")]
        internal interface IHTMLLocation
        {
            void SetHref([In] string p);
            string GetHref();
            void SetProtocol([In] string p);
            string GetProtocol();
            void SetHost([In] string p);
            string GetHost();
            void SetHostname([In] string p);
            string GetHostname();
            void SetPort([In] string p);
            string GetPort();
            void SetPathname([In] string p);
            string GetPathname();
            void SetSearch([In] string p);
            string GetSearch();
            void SetHash([In] string p);
            string GetHash();
            void Reload([In] bool flag);
            void Replace([In] string bstr);
            void Assign([In] string bstr);
        }

        [Guid("3050f666-98b5-11cf-bb82-00aa00bdce0b"), ComVisible(true), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual)]
        public interface IHTMLPopup
        {
            void show(int x, int y, int w, int h, ref object element);
            void hide();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument GetDocument();
            bool IsOpen();
        }

        [SuppressUnmanagedCodeSecurity, Guid("3050f35c-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
        public interface IHTMLScreen
        {
            int GetColorDepth();
            void SetBufferDepth(int d);
            int GetBufferDepth();
            int GetWidth();
            int GetHeight();
            void SetUpdateInterval(int i);
            int GetUpdateInterval();
            int GetAvailHeight();
            int GetAvailWidth();
            bool GetFontSmoothingEnabled();
        }

        [Guid("3050F25E-98B5-11CF-BB82-00AA00BDCE0B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity]
        internal interface IHTMLStyle
        {
            void SetFontFamily(string p);
            string GetFontFamily();
            void SetFontStyle(string p);
            string GetFontStyle();
            void SetFontObject(string p);
            string GetFontObject();
            void SetFontWeight(string p);
            string GetFontWeight();
            void SetFontSize(object p);
            object GetFontSize();
            void SetFont(string p);
            string GetFont();
            void SetColor(object p);
            object GetColor();
            void SetBackground(string p);
            string GetBackground();
            void SetBackgroundColor(object p);
            object GetBackgroundColor();
            void SetBackgroundImage(string p);
            string GetBackgroundImage();
            void SetBackgroundRepeat(string p);
            string GetBackgroundRepeat();
            void SetBackgroundAttachment(string p);
            string GetBackgroundAttachment();
            void SetBackgroundPosition(string p);
            string GetBackgroundPosition();
            void SetBackgroundPositionX(object p);
            object GetBackgroundPositionX();
            void SetBackgroundPositionY(object p);
            object GetBackgroundPositionY();
            void SetWordSpacing(object p);
            object GetWordSpacing();
            void SetLetterSpacing(object p);
            object GetLetterSpacing();
            void SetTextDecoration(string p);
            string GetTextDecoration();
            void SetTextDecorationNone(bool p);
            bool GetTextDecorationNone();
            void SetTextDecorationUnderline(bool p);
            bool GetTextDecorationUnderline();
            void SetTextDecorationOverline(bool p);
            bool GetTextDecorationOverline();
            void SetTextDecorationLineThrough(bool p);
            bool GetTextDecorationLineThrough();
            void SetTextDecorationBlink(bool p);
            bool GetTextDecorationBlink();
            void SetVerticalAlign(object p);
            object GetVerticalAlign();
            void SetTextTransform(string p);
            string GetTextTransform();
            void SetTextAlign(string p);
            string GetTextAlign();
            void SetTextIndent(object p);
            object GetTextIndent();
            void SetLineHeight(object p);
            object GetLineHeight();
            void SetMarginTop(object p);
            object GetMarginTop();
            void SetMarginRight(object p);
            object GetMarginRight();
            void SetMarginBottom(object p);
            object GetMarginBottom();
            void SetMarginLeft(object p);
            object GetMarginLeft();
            void SetMargin(string p);
            string GetMargin();
            void SetPaddingTop(object p);
            object GetPaddingTop();
            void SetPaddingRight(object p);
            object GetPaddingRight();
            void SetPaddingBottom(object p);
            object GetPaddingBottom();
            void SetPaddingLeft(object p);
            object GetPaddingLeft();
            void SetPadding(string p);
            string GetPadding();
            void SetBorder(string p);
            string GetBorder();
            void SetBorderTop(string p);
            string GetBorderTop();
            void SetBorderRight(string p);
            string GetBorderRight();
            void SetBorderBottom(string p);
            string GetBorderBottom();
            void SetBorderLeft(string p);
            string GetBorderLeft();
            void SetBorderColor(string p);
            string GetBorderColor();
            void SetBorderTopColor(object p);
            object GetBorderTopColor();
            void SetBorderRightColor(object p);
            object GetBorderRightColor();
            void SetBorderBottomColor(object p);
            object GetBorderBottomColor();
            void SetBorderLeftColor(object p);
            object GetBorderLeftColor();
            void SetBorderWidth(string p);
            string GetBorderWidth();
            void SetBorderTopWidth(object p);
            object GetBorderTopWidth();
            void SetBorderRightWidth(object p);
            object GetBorderRightWidth();
            void SetBorderBottomWidth(object p);
            object GetBorderBottomWidth();
            void SetBorderLeftWidth(object p);
            object GetBorderLeftWidth();
            void SetBorderStyle(string p);
            string GetBorderStyle();
            void SetBorderTopStyle(string p);
            string GetBorderTopStyle();
            void SetBorderRightStyle(string p);
            string GetBorderRightStyle();
            void SetBorderBottomStyle(string p);
            string GetBorderBottomStyle();
            void SetBorderLeftStyle(string p);
            string GetBorderLeftStyle();
            void SetWidth(object p);
            object GetWidth();
            void SetHeight(object p);
            object GetHeight();
            void SetStyleFloat(string p);
            string GetStyleFloat();
            void SetClear(string p);
            string GetClear();
            void SetDisplay(string p);
            string GetDisplay();
            void SetVisibility(string p);
            string GetVisibility();
            void SetListStyleType(string p);
            string GetListStyleType();
            void SetListStylePosition(string p);
            string GetListStylePosition();
            void SetListStyleImage(string p);
            string GetListStyleImage();
            void SetListStyle(string p);
            string GetListStyle();
            void SetWhiteSpace(string p);
            string GetWhiteSpace();
            void SetTop(object p);
            object GetTop();
            void SetLeft(object p);
            object GetLeft();
            string GetPosition();
            void SetZIndex(object p);
            object GetZIndex();
            void SetOverflow(string p);
            string GetOverflow();
            void SetPageBreakBefore(string p);
            string GetPageBreakBefore();
            void SetPageBreakAfter(string p);
            string GetPageBreakAfter();
            void SetCssText(string p);
            string GetCssText();
            void SetPixelTop(int p);
            int GetPixelTop();
            void SetPixelLeft(int p);
            int GetPixelLeft();
            void SetPixelWidth(int p);
            int GetPixelWidth();
            void SetPixelHeight(int p);
            int GetPixelHeight();
            void SetPosTop(float p);
            float GetPosTop();
            void SetPosLeft(float p);
            float GetPosLeft();
            void SetPosWidth(float p);
            float GetPosWidth();
            void SetPosHeight(float p);
            float GetPosHeight();
            void SetCursor(string p);
            string GetCursor();
            void SetClip(string p);
            string GetClip();
            void SetFilter(string p);
            string GetFilter();
            void SetAttribute(string strAttributeName, object AttributeValue, int lFlags);
            object GetAttribute(string strAttributeName, int lFlags);
            bool RemoveAttribute(string strAttributeName, int lFlags);
        }

        [SuppressUnmanagedCodeSecurity, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("332C4427-26CB-11D0-B483-00C04FD90119")]
        public interface IHTMLWindow2
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object Item([In] ref object pvarIndex);
            int GetLength();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLFramesCollection2 GetFrames();
            void SetDefaultStatus([In] string p);
            string GetDefaultStatus();
            void SetStatus([In] string p);
            string GetStatus();
            int SetTimeout([In] string expression, [In] int msec, [In] ref object language);
            void ClearTimeout([In] int timerID);
            void Alert([In] string message);
            bool Confirm([In] string message);
            [return: MarshalAs(UnmanagedType.Struct)]
            object Prompt([In] string message, [In] string defstr);
            object GetImage();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLLocation GetLocation();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IOmHistory GetHistory();
            void Close();
            void SetOpener([In] object p);
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetOpener();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IOmNavigator GetNavigator();
            void SetName([In] string p);
            string GetName();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 GetParent();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLWindow2 Open([In] string URL, [In] string name, [In] string features, [In] bool replace);
            object GetSelf();
            object GetTop();
            object GetWindow();
            void Navigate([In] string URL);
            void SetOnfocus([In] object p);
            object GetOnfocus();
            void SetOnblur([In] object p);
            object GetOnblur();
            void SetOnload([In] object p);
            object GetOnload();
            void SetOnbeforeunload(object p);
            object GetOnbeforeunload();
            void SetOnunload([In] object p);
            object GetOnunload();
            void SetOnhelp(object p);
            object GetOnhelp();
            void SetOnerror([In] object p);
            object GetOnerror();
            void SetOnresize([In] object p);
            object GetOnresize();
            void SetOnscroll([In] object p);
            object GetOnscroll();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLDocument2 GetDocument();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLEventObj GetEvent();
            object Get_newEnum();
            object ShowModalDialog([In] string dialog, [In] ref object varArgIn, [In] ref object varOptions);
            void ShowHelp([In] string helpURL, [In] object helpArg, [In] string features);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IHTMLScreen GetScreen();
            object GetOption();
            void Focus();
            bool GetClosed();
            void Blur();
            void Scroll([In] int x, [In] int y);
            object GetClientInformation();
            int SetInterval([In] string expression, [In] int msec, [In] ref object language);
            void ClearInterval([In] int timerID);
            void SetOffscreenBuffering([In] object p);
            object GetOffscreenBuffering();
            [return: MarshalAs(UnmanagedType.Struct)]
            object ExecScript([In] string code, [In] string language);
            string toString();
            void ScrollBy([In] int x, [In] int y);
            void ScrollTo([In] int x, [In] int y);
            void MoveTo([In] int x, [In] int y);
            void MoveBy([In] int x, [In] int y);
            void ResizeTo([In] int x, [In] int y);
            void ResizeBy([In] int x, [In] int y);
            object GetExternal();
        }

        [Guid("3050f4ae-98b5-11cf-bb82-00aa00bdce0b"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), SuppressUnmanagedCodeSecurity]
        public interface IHTMLWindow3
        {
            int GetScreenLeft();
            int GetScreenTop();
            bool AttachEvent(string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            void DetachEvent(string ev, [In, MarshalAs(UnmanagedType.IDispatch)] object pdisp);
            int SetTimeout([In] ref object expression, int msec, [In] ref object language);
            int SetInterval([In] ref object expression, int msec, [In] ref object language);
            void Print();
            void SetBeforePrint(object o);
            object GetBeforePrint();
            void SetAfterPrint(object o);
            object GetAfterPrint();
            object GetClipboardData();
            object ShowModelessDialog(string url, object varArgIn, object options);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), Guid("3050f6cf-98b5-11cf-bb82-00aa00bdce0b"), SuppressUnmanagedCodeSecurity]
        public interface IHTMLWindow4
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            object CreatePopup([In] ref object reserved);
            [return: MarshalAs(UnmanagedType.Interface)]
            object frameElement();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000A-0000-0000-C000-000000000046")]
        public interface ILockBytes
        {
            void ReadAt([In, MarshalAs(UnmanagedType.U8)] long ulOffset, [Out] IntPtr pv, [In, MarshalAs(UnmanagedType.U4)] int cb, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pcbRead);
            void WriteAt([In, MarshalAs(UnmanagedType.U8)] long ulOffset, IntPtr pv, [In, MarshalAs(UnmanagedType.U4)] int cb, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pcbWritten);
            void Flush();
            void SetSize([In, MarshalAs(UnmanagedType.U8)] long cb);
            void LockRegion([In, MarshalAs(UnmanagedType.U8)] long libOffset, [In, MarshalAs(UnmanagedType.U8)] long cb, [In, MarshalAs(UnmanagedType.U4)] int dwLockType);
            void UnlockRegion([In, MarshalAs(UnmanagedType.U8)] long libOffset, [In, MarshalAs(UnmanagedType.U8)] long cb, [In, MarshalAs(UnmanagedType.U4)] int dwLockType);
            void Stat([Out] System.Windows.Forms.NativeMethods.STATSTG pstatstg, [In, MarshalAs(UnmanagedType.U4)] int grfStatFlag);
        }

        [ComImport, Guid("00000002-0000-0000-c000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc(int cb);
            [PreserveSig]
            IntPtr Realloc(IntPtr pv, int cb);
            [PreserveSig]
            void Free(IntPtr pv);
            [PreserveSig]
            int GetSize(IntPtr pv);
            [PreserveSig]
            int DidAlloc(IntPtr pv);
            [PreserveSig]
            void HeapMinimize();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000C0600-0000-0000-C000-000000000046")]
        public interface IMsoComponent
        {
            [PreserveSig]
            bool FDebugMessage(IntPtr hInst, int msg, IntPtr wParam, IntPtr lParam);
            [PreserveSig]
            bool FPreTranslateMessage(ref System.Windows.Forms.NativeMethods.MSG msg);
            [PreserveSig]
            void OnEnterState(int uStateID, bool fEnter);
            [PreserveSig]
            void OnAppActivate(bool fActive, int dwOtherThreadID);
            [PreserveSig]
            void OnLoseActivation();
            [PreserveSig]
            void OnActivationChange(System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component, bool fSameComponent, int pcrinfo, bool fHostIsActivating, int pchostinfo, int dwReserved);
            [PreserveSig]
            bool FDoIdle(int grfidlef);
            [PreserveSig]
            bool FContinueMessageLoop(int uReason, int pvLoopData, [MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.NativeMethods.MSG[] pMsgPeeked);
            [PreserveSig]
            bool FQueryTerminate(bool fPromptUser);
            [PreserveSig]
            void Terminate();
            [PreserveSig]
            IntPtr HwndGetWindow(int dwWhich, int dwReserved);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000C0601-0000-0000-C000-000000000046")]
        public interface IMsoComponentManager
        {
            [PreserveSig]
            int QueryService(ref Guid guidService, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object ppvObj);
            [PreserveSig]
            bool FDebugMessage(IntPtr hInst, int msg, IntPtr wParam, IntPtr lParam);
            [PreserveSig]
            bool FRegisterComponent(System.Windows.Forms.UnsafeNativeMethods.IMsoComponent component, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT pcrinfo, out IntPtr dwComponentID);
            [PreserveSig]
            bool FRevokeComponent(IntPtr dwComponentID);
            [PreserveSig]
            bool FUpdateComponentRegistration(IntPtr dwComponentID, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT pcrinfo);
            [PreserveSig]
            bool FOnComponentActivate(IntPtr dwComponentID);
            [PreserveSig]
            bool FSetTrackingComponent(IntPtr dwComponentID, [In, MarshalAs(UnmanagedType.Bool)] bool fTrack);
            [PreserveSig]
            void OnComponentEnterState(IntPtr dwComponentID, int uStateID, int uContext, int cpicmExclude, int rgpicmExclude, int dwReserved);
            [PreserveSig]
            bool FOnComponentExitState(IntPtr dwComponentID, int uStateID, int uContext, int cpicmExclude, int rgpicmExclude);
            [PreserveSig]
            bool FInState(int uStateID, IntPtr pvoid);
            [PreserveSig]
            bool FContinueIdle();
            [PreserveSig]
            bool FPushMessageLoop(IntPtr dwComponentID, int uReason, int pvLoopData);
            [PreserveSig]
            bool FCreateSubComponentManager([MarshalAs(UnmanagedType.Interface)] object punkOuter, [MarshalAs(UnmanagedType.Interface)] object punkServProv, ref Guid riid, out IntPtr ppvObj);
            [PreserveSig]
            bool FGetParentComponentManager(out System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager ppicm);
            [PreserveSig]
            bool FGetActiveComponent(int dwgac, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.IMsoComponent[] ppic, System.Windows.Forms.NativeMethods.MSOCRINFOSTRUCT pcrinfo, int dwReserved);
        }

        [ComImport, Guid("0000011e-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleCache
        {
            int Cache(ref FORMATETC pformatetc, int advf);
            void Uncache(int dwConnection);
            object EnumCache();
            void InitCache(System.Runtime.InteropServices.ComTypes.IDataObject pDataObject);
            void SetData(ref FORMATETC pformatetc, ref STGMEDIUM pmedium, bool fRelease);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000118-0000-0000-C000-000000000046")]
        public interface IOleClientSite
        {
            [PreserveSig]
            int SaveObject();
            [PreserveSig]
            int GetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwAssign, [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [MarshalAs(UnmanagedType.Interface)] out object moniker);
            [PreserveSig]
            int GetContainer(out System.Windows.Forms.UnsafeNativeMethods.IOleContainer container);
            [PreserveSig]
            int ShowObject();
            [PreserveSig]
            int OnShowWindow(int fShow);
            [PreserveSig]
            int RequestNewObjectLayout();
        }

        [ComImport, Guid("0000011B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleContainer
        {
            [PreserveSig]
            int ParseDisplayName([In, MarshalAs(UnmanagedType.Interface)] object pbc, [In, MarshalAs(UnmanagedType.BStr)] string pszDisplayName, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pchEaten, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppmkOut);
            [PreserveSig]
            int EnumObjects([In, MarshalAs(UnmanagedType.U4)] int grfFlags, out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum);
            [PreserveSig]
            int LockContainer(bool fLock);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B196B288-BAB4-101A-B69C-00AA00341D07")]
        public interface IOleControl
        {
            [PreserveSig]
            int GetControlInfo([Out] System.Windows.Forms.NativeMethods.tagCONTROLINFO pCI);
            [PreserveSig]
            int OnMnemonic([In] ref System.Windows.Forms.NativeMethods.MSG pMsg);
            [PreserveSig]
            int OnAmbientPropertyChange(int dispID);
            [PreserveSig]
            int FreezeEvents(int bFreeze);
        }

        [ComImport, Guid("B196B289-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleControlSite
        {
            [PreserveSig]
            int OnControlInfoChanged();
            [PreserveSig]
            int LockInPlaceActive(int fLock);
            [PreserveSig]
            int GetExtendedControl([MarshalAs(UnmanagedType.IDispatch)] out object ppDisp);
            [PreserveSig]
            int TransformCoords([In, Out] System.Windows.Forms.NativeMethods._POINTL pPtlHimetric, [In, Out] System.Windows.Forms.NativeMethods.tagPOINTF pPtfContainer, [In, MarshalAs(UnmanagedType.U4)] int dwFlags);
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Windows.Forms.NativeMethods.MSG pMsg, [In, MarshalAs(UnmanagedType.U4)] int grfModifiers);
            [PreserveSig]
            int OnFocus(int fGotFocus);
            [PreserveSig]
            int ShowPropertyFrame();
        }

        [ComImport, Guid("b722bcc5-4e68-101b-a2bc-00aa00404770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDocument
        {
            [PreserveSig]
            int CreateView(System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite pIPSite, System.Windows.Forms.UnsafeNativeMethods.IStream pstm, int dwReserved, out System.Windows.Forms.UnsafeNativeMethods.IOleDocumentView ppView);
            [PreserveSig]
            int GetDocMiscStatus(out int pdwStatus);
            int EnumViews(out object ppEnum, out System.Windows.Forms.UnsafeNativeMethods.IOleDocumentView ppView);
        }

        [ComImport, ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("B722BCC7-4E68-101B-A2BC-00AA00404770")]
        public interface IOleDocumentSite
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int ActivateMe([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleDocumentView pViewToActivate);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("B722BCC6-4E68-101B-A2BC-00AA00404770")]
        public interface IOleDocumentView
        {
            void SetInPlaceSite([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite pIPSite);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite GetInPlaceSite();
            [return: MarshalAs(UnmanagedType.Interface)]
            object GetDocument();
            void SetRect([In] ref System.Windows.Forms.NativeMethods.RECT prcView);
            void GetRect([In, Out] ref System.Windows.Forms.NativeMethods.RECT prcView);
            void SetRectComplex([In] System.Windows.Forms.NativeMethods.RECT prcView, [In] System.Windows.Forms.NativeMethods.RECT prcHScroll, [In] System.Windows.Forms.NativeMethods.RECT prcVScroll, [In] System.Windows.Forms.NativeMethods.RECT prcSizeBox);
            void Show(bool fShow);
            [PreserveSig]
            int UIActivate(bool fUIActivate);
            void Open();
            [PreserveSig]
            int Close([In, MarshalAs(UnmanagedType.U4)] int dwReserved);
            void SaveViewState([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
            void ApplyViewState([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
            void Clone([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceSite pIPSiteNew, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.IOleDocumentView[] ppViewNew);
        }

        [ComImport, Guid("00000121-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropSource
        {
            [PreserveSig]
            int OleQueryContinueDrag(int fEscapePressed, [In, MarshalAs(UnmanagedType.U4)] int grfKeyState);
            [PreserveSig]
            int OleGiveFeedback([In, MarshalAs(UnmanagedType.U4)] int dwEffect);
        }

        [ComImport, Guid("00000122-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleDropTarget
        {
            [PreserveSig]
            int OleDragEnter([In, MarshalAs(UnmanagedType.Interface)] object pDataObj, [In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out] ref int pdwEffect);
            [PreserveSig]
            int OleDragOver([In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out] ref int pdwEffect);
            [PreserveSig]
            int OleDragLeave();
            [PreserveSig]
            int OleDrop([In, MarshalAs(UnmanagedType.Interface)] object pDataObj, [In, MarshalAs(UnmanagedType.U4)] int grfKeyState, [In, MarshalAs(UnmanagedType.U8)] long pt, [In, Out] ref int pdwEffect);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("00000117-0000-0000-C000-000000000046")]
        public interface IOleInPlaceActiveObject
        {
            [PreserveSig]
            int GetWindow(out IntPtr hwnd);
            void ContextSensitiveHelp(int fEnterMode);
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Windows.Forms.NativeMethods.MSG lpmsg);
            void OnFrameWindowActivate(bool fActivate);
            void OnDocWindowActivate(int fActivate);
            void ResizeBorder([In] System.Windows.Forms.NativeMethods.COMRECT prcBorder, [In] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow pUIWindow, bool fFrameWindow);
            void EnableModeless(int fEnable);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000116-0000-0000-C000-000000000046")]
        public interface IOleInPlaceFrame
        {
            IntPtr GetWindow();
            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);
            [PreserveSig]
            int GetBorder([Out] System.Windows.Forms.NativeMethods.COMRECT lprectBorder);
            [PreserveSig]
            int RequestBorderSpace([In] System.Windows.Forms.NativeMethods.COMRECT pborderwidths);
            [PreserveSig]
            int SetBorderSpace([In] System.Windows.Forms.NativeMethods.COMRECT pborderwidths);
            [PreserveSig]
            int SetActiveObject([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
            [PreserveSig]
            int InsertMenus([In] IntPtr hmenuShared, [In, Out] System.Windows.Forms.NativeMethods.tagOleMenuGroupWidths lpMenuWidths);
            [PreserveSig]
            int SetMenu([In] IntPtr hmenuShared, [In] IntPtr holemenu, [In] IntPtr hwndActiveObject);
            [PreserveSig]
            int RemoveMenus([In] IntPtr hmenuShared);
            [PreserveSig]
            int SetStatusText([In, MarshalAs(UnmanagedType.LPWStr)] string pszStatusText);
            [PreserveSig]
            int EnableModeless(bool fEnable);
            [PreserveSig]
            int TranslateAccelerator([In] ref System.Windows.Forms.NativeMethods.MSG lpmsg, [In, MarshalAs(UnmanagedType.U2)] short wID);
        }

        [ComImport, Guid("00000113-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        public interface IOleInPlaceObject
        {
            [PreserveSig]
            int GetWindow(out IntPtr hwnd);
            void ContextSensitiveHelp(int fEnterMode);
            void InPlaceDeactivate();
            [PreserveSig]
            int UIDeactivate();
            void SetObjectRects([In] System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, [In] System.Windows.Forms.NativeMethods.COMRECT lprcClipRect);
            void ReactivateAndUndo();
        }

        [ComImport, Guid("1C2056CC-5EF4-101B-8BC8-00AA003E3B29"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceObjectWindowless
        {
            [PreserveSig]
            int SetClientSite([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pClientSite);
            [PreserveSig]
            int GetClientSite(out System.Windows.Forms.UnsafeNativeMethods.IOleClientSite site);
            [PreserveSig]
            int SetHostNames([In, MarshalAs(UnmanagedType.LPWStr)] string szContainerApp, [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerObj);
            [PreserveSig]
            int Close(int dwSaveOption);
            [PreserveSig]
            int SetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [In, MarshalAs(UnmanagedType.Interface)] object pmk);
            [PreserveSig]
            int GetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwAssign, [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [MarshalAs(UnmanagedType.Interface)] out object moniker);
            [PreserveSig]
            int InitFromData([In, MarshalAs(UnmanagedType.Interface)] System.Runtime.InteropServices.ComTypes.IDataObject pDataObject, int fCreation, [In, MarshalAs(UnmanagedType.U4)] int dwReserved);
            [PreserveSig]
            int GetClipboardData([In, MarshalAs(UnmanagedType.U4)] int dwReserved, out System.Runtime.InteropServices.ComTypes.IDataObject data);
            [PreserveSig]
            int DoVerb(int iVerb, [In] IntPtr lpmsg, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pActiveSite, int lindex, IntPtr hwndParent, [In] System.Windows.Forms.NativeMethods.COMRECT lprcPosRect);
            [PreserveSig]
            int EnumVerbs(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB e);
            [PreserveSig]
            int OleUpdate();
            [PreserveSig]
            int IsUpToDate();
            [PreserveSig]
            int GetUserClassID([In, Out] ref Guid pClsid);
            [PreserveSig]
            int GetUserType([In, MarshalAs(UnmanagedType.U4)] int dwFormOfType, [MarshalAs(UnmanagedType.LPWStr)] out string userType);
            [PreserveSig]
            int SetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [In] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int GetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [Out] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int Advise([In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink, out int cookie);
            [PreserveSig]
            int Unadvise([In, MarshalAs(UnmanagedType.U4)] int dwConnection);
            [PreserveSig]
            int EnumAdvise(out IEnumSTATDATA e);
            [PreserveSig]
            int GetMiscStatus([In, MarshalAs(UnmanagedType.U4)] int dwAspect, out int misc);
            [PreserveSig]
            int SetColorScheme([In] System.Windows.Forms.NativeMethods.tagLOGPALETTE pLogpal);
            [PreserveSig]
            int OnWindowMessage([In, MarshalAs(UnmanagedType.U4)] int msg, [In, MarshalAs(UnmanagedType.U4)] int wParam, [In, MarshalAs(UnmanagedType.U4)] int lParam, [Out, MarshalAs(UnmanagedType.U4)] int plResult);
            [PreserveSig]
            int GetDropTarget([Out, MarshalAs(UnmanagedType.Interface)] object ppDropTarget);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000119-0000-0000-C000-000000000046")]
        public interface IOleInPlaceSite
        {
            IntPtr GetWindow();
            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);
            [PreserveSig]
            int CanInPlaceActivate();
            [PreserveSig]
            int OnInPlaceActivate();
            [PreserveSig]
            int OnUIActivate();
            [PreserveSig]
            int GetWindowContext([MarshalAs(UnmanagedType.Interface)] out System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceFrame ppFrame, [MarshalAs(UnmanagedType.Interface)] out System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceUIWindow ppDoc, [Out] System.Windows.Forms.NativeMethods.COMRECT lprcPosRect, [Out] System.Windows.Forms.NativeMethods.COMRECT lprcClipRect, [In, Out] System.Windows.Forms.NativeMethods.tagOIFI lpFrameInfo);
            [PreserveSig]
            int Scroll(System.Windows.Forms.NativeMethods.tagSIZE scrollExtant);
            [PreserveSig]
            int OnUIDeactivate(int fUndoable);
            [PreserveSig]
            int OnInPlaceDeactivate();
            [PreserveSig]
            int DiscardUndoState();
            [PreserveSig]
            int DeactivateAndUndo();
            [PreserveSig]
            int OnPosRectChange([In] System.Windows.Forms.NativeMethods.COMRECT lprcPosRect);
        }

        [ComImport, Guid("00000115-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleInPlaceUIWindow
        {
            IntPtr GetWindow();
            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);
            [PreserveSig]
            int GetBorder([Out] System.Windows.Forms.NativeMethods.COMRECT lprectBorder);
            [PreserveSig]
            int RequestBorderSpace([In] System.Windows.Forms.NativeMethods.COMRECT pborderwidths);
            [PreserveSig]
            int SetBorderSpace([In] System.Windows.Forms.NativeMethods.COMRECT pborderwidths);
            void SetActiveObject([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleInPlaceActiveObject pActiveObject, [In, MarshalAs(UnmanagedType.LPWStr)] string pszObjName);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000016-0000-0000-C000-000000000046")]
        public interface IOleMessageFilter
        {
            [PreserveSig]
            int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);
            [PreserveSig]
            int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);
            [PreserveSig]
            int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
        }

        [ComImport, Guid("00000112-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        public interface IOleObject
        {
            [PreserveSig]
            int SetClientSite([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pClientSite);
            System.Windows.Forms.UnsafeNativeMethods.IOleClientSite GetClientSite();
            [PreserveSig]
            int SetHostNames([In, MarshalAs(UnmanagedType.LPWStr)] string szContainerApp, [In, MarshalAs(UnmanagedType.LPWStr)] string szContainerObj);
            [PreserveSig]
            int Close(int dwSaveOption);
            [PreserveSig]
            int SetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [In, MarshalAs(UnmanagedType.Interface)] object pmk);
            [PreserveSig]
            int GetMoniker([In, MarshalAs(UnmanagedType.U4)] int dwAssign, [In, MarshalAs(UnmanagedType.U4)] int dwWhichMoniker, [MarshalAs(UnmanagedType.Interface)] out object moniker);
            [PreserveSig]
            int InitFromData([In, MarshalAs(UnmanagedType.Interface)] System.Runtime.InteropServices.ComTypes.IDataObject pDataObject, int fCreation, [In, MarshalAs(UnmanagedType.U4)] int dwReserved);
            [PreserveSig]
            int GetClipboardData([In, MarshalAs(UnmanagedType.U4)] int dwReserved, out System.Runtime.InteropServices.ComTypes.IDataObject data);
            [PreserveSig]
            int DoVerb(int iVerb, [In] IntPtr lpmsg, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pActiveSite, int lindex, IntPtr hwndParent, [In] System.Windows.Forms.NativeMethods.COMRECT lprcPosRect);
            [PreserveSig]
            int EnumVerbs(out System.Windows.Forms.UnsafeNativeMethods.IEnumOLEVERB e);
            [PreserveSig]
            int OleUpdate();
            [PreserveSig]
            int IsUpToDate();
            [PreserveSig]
            int GetUserClassID([In, Out] ref Guid pClsid);
            [PreserveSig]
            int GetUserType([In, MarshalAs(UnmanagedType.U4)] int dwFormOfType, [MarshalAs(UnmanagedType.LPWStr)] out string userType);
            [PreserveSig]
            int SetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [In] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int GetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, [Out] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
            [PreserveSig]
            int Advise(IAdviseSink pAdvSink, out int cookie);
            [PreserveSig]
            int Unadvise([In, MarshalAs(UnmanagedType.U4)] int dwConnection);
            [PreserveSig]
            int EnumAdvise(out IEnumSTATDATA e);
            [PreserveSig]
            int GetMiscStatus([In, MarshalAs(UnmanagedType.U4)] int dwAspect, out int misc);
            [PreserveSig]
            int SetColorScheme([In] System.Windows.Forms.NativeMethods.tagLOGPALETTE pLogpal);
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, out IntPtr ppvObject);
        }

        [ComImport, Guid("00000114-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IOleWindow
        {
            [PreserveSig]
            int GetWindow(out IntPtr hwnd);
            void ContextSensitiveHelp(int fEnterMode);
        }

        [Guid("FECEAAA2-8405-11CF-8BA1-00AA00476DA6"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), SuppressUnmanagedCodeSecurity]
        internal interface IOmHistory
        {
            short GetLength();
            void Back();
            void Forward();
            void Go([In] ref object pvargdistance);
        }

        [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("FECEAAA5-8405-11CF-8BA1-00AA00476DA6"), SuppressUnmanagedCodeSecurity]
        internal interface IOmNavigator
        {
            string GetAppCodeName();
            string GetAppName();
            string GetAppVersion();
            string GetUserAgent();
            bool JavaEnabled();
            bool TaintEnabled();
            object GetMimeTypes();
            object GetPlugins();
            bool GetCookieEnabled();
            object GetOpsProfile();
            string GetCpuClass();
            string GetSystemLanguage();
            string GetBrowserLanguage();
            string GetUserLanguage();
            string GetPlatform();
            string GetAppMinorVersion();
            int GetConnectionSpeed();
            bool GetOnLine();
            object GetUserProfile();
        }

        [ComImport, Guid("0000010C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersist
        {
            [SuppressUnmanagedCodeSecurity]
            void GetClassID(out Guid pClassID);
        }

        [ComImport, Guid("37D84F60-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistPropertyBag
        {
            void GetClassID(out Guid pClassID);
            void InitNew();
            void Load([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog);
            void Save([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IPropertyBag pPropBag, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty, [In, MarshalAs(UnmanagedType.Bool)] bool fSaveAllProperties);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010A-0000-0000-C000-000000000046")]
        public interface IPersistStorage
        {
            void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();
            void InitNew(System.Windows.Forms.UnsafeNativeMethods.IStorage pstg);
            [PreserveSig]
            int Load(System.Windows.Forms.UnsafeNativeMethods.IStorage pstg);
            void Save(System.Windows.Forms.UnsafeNativeMethods.IStorage pStgSave, bool fSameAsLoad);
            void SaveCompleted(System.Windows.Forms.UnsafeNativeMethods.IStorage pStgNew);
            void HandsOffStorage();
        }

        [ComImport, Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistStream
        {
            void GetClassID(out Guid pClassId);
            [PreserveSig]
            int IsDirty();
            void Load([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
            void Save([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            long GetSizeMax();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity, Guid("7FD52380-4E07-101B-AE2D-08002B2EC713")]
        public interface IPersistStreamInit
        {
            void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();
            void Load([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm);
            void Save([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
            void GetSizeMax([Out, MarshalAs(UnmanagedType.LPArray)] long pcbSize);
            void InitNew();
        }

        [ComImport, Guid("7BF80980-BF32-101A-8BBB-00AA00300CAB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPicture
        {
            IntPtr GetHandle();
            IntPtr GetHPal();
            [return: MarshalAs(UnmanagedType.I2)]
            short GetPictureType();
            int GetWidth();
            int GetHeight();
            void Render(IntPtr hDC, int x, int y, int cx, int cy, int xSrc, int ySrc, int cxSrc, int cySrc, IntPtr rcBounds);
            void SetHPal(IntPtr phpal);
            IntPtr GetCurDC();
            void SelectPicture(IntPtr hdcIn, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] phdcOut, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] phbmpOut);
            [return: MarshalAs(UnmanagedType.Bool)]
            bool GetKeepOriginalFormat();
            void SetKeepOriginalFormat([In, MarshalAs(UnmanagedType.Bool)] bool pfkeep);
            void PictureChanged();
            [PreserveSig]
            int SaveAsFile([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm, int fSaveMemCopy, out int pcbSize);
            int GetAttributes();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("7BF80981-BF32-101A-8BBB-00AA00300CAB")]
        public interface IPictureDisp
        {
            IntPtr Handle { get; }
            IntPtr HPal { get; }
            short PictureType { get; }
            int Width { get; }
            int Height { get; }
            void Render(IntPtr hdc, int x, int y, int cx, int cy, int xSrc, int ySrc, int cxSrc, int cySrc);
        }

        [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyBag
        {
            [PreserveSig]
            int Read([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, Out] ref object pVar, [In] System.Windows.Forms.UnsafeNativeMethods.IErrorLog pErrorLog);
            [PreserveSig]
            int Write([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In] ref object pVar);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9BFBBC02-EFF1-101A-84ED-00AA00341D07")]
        public interface IPropertyNotifySink
        {
            void OnChanged(int dispID);
            [PreserveSig]
            int OnRequestEdit(int dispID);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CF51ED10-62FE-11CF-BF86-00A0C9034836")]
        public interface IQuickActivate
        {
            void QuickActivate([In] System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER pQaContainer, [Out] System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL pQaControl);
            void SetContentExtent([In] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
            void GetContentExtent([Out] System.Windows.Forms.NativeMethods.tagSIZEL pSizel);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020D03-0000-0000-C000-000000000046")]
        public interface IRichEditOleCallback
        {
            [PreserveSig]
            int GetNewStorage(out System.Windows.Forms.UnsafeNativeMethods.IStorage ret);
            [PreserveSig]
            int GetInPlaceContext(IntPtr lplpFrame, IntPtr lplpDoc, IntPtr lpFrameInfo);
            [PreserveSig]
            int ShowContainerUI(int fShow);
            [PreserveSig]
            int QueryInsertObject(ref Guid lpclsid, IntPtr lpstg, int cp);
            [PreserveSig]
            int DeleteObject(IntPtr lpoleobj);
            [PreserveSig]
            int QueryAcceptData(System.Runtime.InteropServices.ComTypes.IDataObject lpdataobj, IntPtr lpcfFormat, int reco, int fReally, IntPtr hMetaPict);
            [PreserveSig]
            int ContextSensitiveHelp(int fEnterMode);
            [PreserveSig]
            int GetClipboardData(System.Windows.Forms.NativeMethods.CHARRANGE lpchrg, int reco, IntPtr lplpdataobj);
            [PreserveSig]
            int GetDragDropEffect(bool fDrag, int grfKeyState, ref int pdwEffect);
            [PreserveSig]
            int GetContextMenu(short seltype, IntPtr lpoleobj, System.Windows.Forms.NativeMethods.CHARRANGE lpchrg, out IntPtr hmenu);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000126-0000-0000-C000-000000000046")]
        public interface IRunnableObject
        {
            void GetRunningClass(out Guid guid);
            [PreserveSig]
            int Run(IntPtr lpBindContext);
            bool IsRunning();
            void LockRunning(bool fLock, bool fLastUnlockCloses);
            void SetContainedObject(bool fContained);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("742B0E01-14E6-101B-914E-00AA00300CAB")]
        public interface ISimpleFrameSite
        {
            [PreserveSig]
            int PreMessageFilter(IntPtr hwnd, [In, MarshalAs(UnmanagedType.U4)] int msg, IntPtr wp, IntPtr lp, [In, Out] ref IntPtr plResult, [In, Out, MarshalAs(UnmanagedType.U4)] ref int pdwCookie);
            [PreserveSig]
            int PostMessageFilter(IntPtr hwnd, [In, MarshalAs(UnmanagedType.U4)] int msg, IntPtr wp, IntPtr lp, [In, Out] ref IntPtr plResult, [In, MarshalAs(UnmanagedType.U4)] int dwCookie);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000000B-0000-0000-C000-000000000046")]
        public interface IStorage
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IStream CreateStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IStream OpenStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IStorage CreateStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IStorage OpenStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr pstgPriority, [In, MarshalAs(UnmanagedType.U4)] int grfMode, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.U4)] int reserved);
            void CopyTo(int ciidExclude, [In, MarshalAs(UnmanagedType.LPArray)] Guid[] pIIDExclude, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStorage stgDest);
            void MoveElementTo([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStorage stgDest, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName, [In, MarshalAs(UnmanagedType.U4)] int grfFlags);
            void Commit(int grfCommitFlags);
            void Revert();
            void EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [MarshalAs(UnmanagedType.Interface)] out object ppVal);
            void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);
            void RenameElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName);
            void SetElementTimes([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] System.Windows.Forms.NativeMethods.FILETIME pctime, [In] System.Windows.Forms.NativeMethods.FILETIME patime, [In] System.Windows.Forms.NativeMethods.FILETIME pmtime);
            void SetClass([In] ref Guid clsid);
            void SetStateBits(int grfStateBits, int grfMask);
            void Stat([Out] System.Windows.Forms.NativeMethods.STATSTG pStatStg, int grfStatFlag);
        }

        [ComImport, Guid("0000000C-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        public interface IStream
        {
            int Read(IntPtr buf, int len);
            int Write(IntPtr buf, int len);
            [return: MarshalAs(UnmanagedType.I8)]
            long Seek([In, MarshalAs(UnmanagedType.I8)] long dlibMove, int dwOrigin);
            void SetSize([In, MarshalAs(UnmanagedType.I8)] long libNewSize);
            [return: MarshalAs(UnmanagedType.I8)]
            long CopyTo([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.IStream pstm, [In, MarshalAs(UnmanagedType.I8)] long cb, [Out, MarshalAs(UnmanagedType.LPArray)] long[] pcbRead);
            void Commit(int grfCommitFlags);
            void Revert();
            void LockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, int dwLockType);
            void UnlockRegion([In, MarshalAs(UnmanagedType.I8)] long libOffset, [In, MarshalAs(UnmanagedType.I8)] long cb, int dwLockType);
            void Stat([Out] System.Windows.Forms.NativeMethods.STATSTG pStatstg, int grfStatFlag);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.IStream Clone();
        }

        [ComImport, Guid("DF0B3D60-548F-101B-8E65-08002B2BD119"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ISupportErrorInfo
        {
            int InterfaceSupportsErrorInfo([In] ref Guid riid);
        }

        [Guid("8CC497C0-A1DF-11ce-8098-00AA0047BE5D"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
        public interface ITextDocument
        {
            string GetName();
            object GetSelection();
            int GetStoryCount();
            object GetStoryRanges();
            int GetSaved();
            void SetSaved(int value);
            object GetDefaultTabStop();
            void SetDefaultTabStop(object value);
            void New();
            void Open(object pVar, int flags, int codePage);
            void Save(object pVar, int flags, int codePage);
            int Freeze();
            int Unfreeze();
            void BeginEditCollection();
            void EndEditCollection();
            int Undo(int count);
            int Redo(int count);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITextRange Range(int cp1, int cp2);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITextRange RangeFromPoint(int x, int y);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), SuppressUnmanagedCodeSecurity, Guid("8CC497C2-A1DF-11ce-8098-00AA0047BE5D")]
        public interface ITextRange
        {
            string GetText();
            void SetText(string text);
            object GetChar();
            void SetChar(object ch);
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITextRange GetDuplicate();
            [return: MarshalAs(UnmanagedType.Interface)]
            System.Windows.Forms.UnsafeNativeMethods.ITextRange GetFormattedText();
            void SetFormattedText([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.ITextRange range);
            int GetStart();
            void SetStart(int cpFirst);
            int GetEnd();
            void SetEnd(int cpLim);
            object GetFont();
            void SetFont(object font);
            object GetPara();
            void SetPara(object para);
            int GetStoryLength();
            int GetStoryType();
            void Collapse(int start);
            int Expand(int unit);
            int GetIndex(int unit);
            void SetIndex(int unit, int index, int extend);
            void SetRange(int cpActive, int cpOther);
            int InRange([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.ITextRange range);
            int InStory([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.ITextRange range);
            int IsEqual([In, MarshalAs(UnmanagedType.Interface)] System.Windows.Forms.UnsafeNativeMethods.ITextRange range);
            void Select();
            int StartOf(int unit, int extend);
            int EndOf(int unit, int extend);
            int Move(int unit, int count);
            int MoveStart(int unit, int count);
            int MoveEnd(int unit, int count);
            int MoveWhile(object cset, int count);
            int MoveStartWhile(object cset, int count);
            int MoveEndWhile(object cset, int count);
            int MoveUntil(object cset, int count);
            int MoveStartUntil(object cset, int count);
            int MoveEndUntil(object cset, int count);
            int FindText(string text, int cch, int flags);
            int FindTextStart(string text, int cch, int flags);
            int FindTextEnd(string text, int cch, int flags);
            int Delete(int unit, int count);
            void Cut(out object pVar);
            void Copy(out object pVar);
            void Paste(object pVar, int format);
            int CanPaste(object pVar, int format);
            int CanEdit();
            void ChangeCase(int type);
            void GetPoint(int type, out int x, out int y);
            void SetPoint(int x, int y, int type, int extend);
            void ScrollIntoView(int value);
            object GetEmbeddedObject();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020403-0000-0000-C000-000000000046")]
        public interface ITypeComp
        {
            void RemoteBind([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [In, MarshalAs(UnmanagedType.U2)] short wFlags, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.NativeMethods.tagDESCKIND[] pDescKind, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.NativeMethods.tagFUNCDESC[] ppFuncDesc, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.NativeMethods.tagVARDESC[] ppVarDesc, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeComp[] ppTypeComp, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pDummy);
            void RemoteBindType([In, MarshalAs(UnmanagedType.LPWStr)] string szName, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeInfo[] ppTInfo);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020401-0000-0000-C000-000000000046")]
        public interface ITypeInfo
        {
            [PreserveSig]
            int GetTypeAttr(ref IntPtr pTypeAttr);
            [PreserveSig]
            int GetTypeComp([Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeComp[] ppTComp);
            [PreserveSig]
            int GetFuncDesc([In, MarshalAs(UnmanagedType.U4)] int index, ref IntPtr pFuncDesc);
            [PreserveSig]
            int GetVarDesc([In, MarshalAs(UnmanagedType.U4)] int index, ref IntPtr pVarDesc);
            [PreserveSig]
            int GetNames(int memid, [Out, MarshalAs(UnmanagedType.LPArray)] string[] rgBstrNames, [In, MarshalAs(UnmanagedType.U4)] int cMaxNames, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pcNames);
            [PreserveSig]
            int GetRefTypeOfImplType([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pRefType);
            [PreserveSig]
            int GetImplTypeFlags([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pImplTypeFlags);
            [PreserveSig]
            int GetIDsOfNames(IntPtr rgszNames, int cNames, IntPtr pMemId);
            [PreserveSig]
            int Invoke();
            [PreserveSig]
            int GetDocumentation(int memid, ref string pBstrName, ref string pBstrDocString, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pdwHelpContext, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrHelpFile);
            [PreserveSig]
            int GetDllEntry(int memid, System.Windows.Forms.NativeMethods.tagINVOKEKIND invkind, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrDllName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrName, [Out, MarshalAs(UnmanagedType.LPArray)] short[] pwOrdinal);
            [PreserveSig]
            int GetRefTypeInfo(IntPtr hreftype, ref System.Windows.Forms.UnsafeNativeMethods.ITypeInfo pTypeInfo);
            [PreserveSig]
            int AddressOfMember();
            [PreserveSig]
            int CreateInstance([In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppvObj);
            [PreserveSig]
            int GetMops(int memid, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrMops);
            [PreserveSig]
            int GetContainingTypeLib([Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeLib[] ppTLib, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pIndex);
            [PreserveSig]
            void ReleaseTypeAttr(IntPtr typeAttr);
            [PreserveSig]
            void ReleaseFuncDesc(IntPtr funcDesc);
            [PreserveSig]
            void ReleaseVarDesc(IntPtr varDesc);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020402-0000-0000-C000-000000000046")]
        public interface ITypeLib
        {
            void RemoteGetTypeInfoCount([Out, MarshalAs(UnmanagedType.LPArray)] int[] pcTInfo);
            void GetTypeInfo([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeInfo[] ppTInfo);
            void GetTypeInfoType([In, MarshalAs(UnmanagedType.U4)] int index, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.NativeMethods.tagTYPEKIND[] pTKind);
            void GetTypeInfoOfGuid([In] ref Guid guid, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeInfo[] ppTInfo);
            void RemoteGetLibAttr(IntPtr ppTLibAttr, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pDummy);
            void GetTypeComp([Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeComp[] ppTComp);
            void RemoteGetDocumentation(int index, [In, MarshalAs(UnmanagedType.U4)] int refPtrFlags, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrDocString, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pdwHelpContext, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrHelpFile);
            void RemoteIsName([In, MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] pfName, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrLibName);
            void RemoteFindName([In, MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, [In, MarshalAs(UnmanagedType.U4)] int lHashVal, [Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.ITypeInfo[] ppTInfo, [Out, MarshalAs(UnmanagedType.LPArray)] int[] rgMemId, [In, Out, MarshalAs(UnmanagedType.LPArray)] short[] pcFound, [Out, MarshalAs(UnmanagedType.LPArray)] string[] pBstrLibName);
            void LocalReleaseTLibAttr();
        }

        [ComImport, Guid("9849FD60-3768-101B-8D72-AE6164FFE3CF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVBFormat
        {
            [PreserveSig]
            int Format([In] ref object var, IntPtr pszFormat, IntPtr lpBuffer, short cpBuffer, int lcid, short firstD, short firstW, [Out, MarshalAs(UnmanagedType.LPArray)] short[] result);
        }

        [ComImport, Guid("40A050A0-3C31-101B-A82E-08002B2B2337"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IVBGetControl
        {
            [PreserveSig]
            int EnumControls(int dwOleContF, int dwWhich, out System.Windows.Forms.UnsafeNativeMethods.IEnumUnknown ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010d-0000-0000-C000-000000000046")]
        public interface IViewObject
        {
            [PreserveSig]
            int Draw([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [In] System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, [In] System.Windows.Forms.NativeMethods.COMRECT lprcBounds, [In] System.Windows.Forms.NativeMethods.COMRECT lprcWBounds, IntPtr pfnContinue, [In] int dwContinue);
            [PreserveSig]
            int GetColorSet([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [In] System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hicTargetDev, [Out] System.Windows.Forms.NativeMethods.tagLOGPALETTE ppColorSet);
            [PreserveSig]
            int Freeze([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [Out] IntPtr pdwFreeze);
            [PreserveSig]
            int Unfreeze([In, MarshalAs(UnmanagedType.U4)] int dwFreeze);
            void SetAdvise([In, MarshalAs(UnmanagedType.U4)] int aspects, [In, MarshalAs(UnmanagedType.U4)] int advf, [In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink);
            void GetAdvise([In, Out, MarshalAs(UnmanagedType.LPArray)] int[] paspects, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] advf, [In, Out, MarshalAs(UnmanagedType.LPArray)] IAdviseSink[] pAdvSink);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000127-0000-0000-C000-000000000046")]
        public interface IViewObject2
        {
            void Draw([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [In] System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hdcTargetDev, IntPtr hdcDraw, [In] System.Windows.Forms.NativeMethods.COMRECT lprcBounds, [In] System.Windows.Forms.NativeMethods.COMRECT lprcWBounds, IntPtr pfnContinue, [In] int dwContinue);
            [PreserveSig]
            int GetColorSet([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [In] System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, IntPtr hicTargetDev, [Out] System.Windows.Forms.NativeMethods.tagLOGPALETTE ppColorSet);
            [PreserveSig]
            int Freeze([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, IntPtr pvAspect, [Out] IntPtr pdwFreeze);
            [PreserveSig]
            int Unfreeze([In, MarshalAs(UnmanagedType.U4)] int dwFreeze);
            void SetAdvise([In, MarshalAs(UnmanagedType.U4)] int aspects, [In, MarshalAs(UnmanagedType.U4)] int advf, [In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink);
            void GetAdvise([In, Out, MarshalAs(UnmanagedType.LPArray)] int[] paspects, [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] advf, [In, Out, MarshalAs(UnmanagedType.LPArray)] IAdviseSink[] pAdvSink);
            void GetExtent([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect, int lindex, [In] System.Windows.Forms.NativeMethods.tagDVTARGETDEVICE ptd, [Out] System.Windows.Forms.NativeMethods.tagSIZEL lpsizel);
        }

        [ComImport, SuppressUnmanagedCodeSecurity, TypeLibType(TypeLibTypeFlags.FOleAutomation | TypeLibTypeFlags.FDual | TypeLibTypeFlags.FHidden), Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E")]
        public interface IWebBrowser2
        {
            [DispId(100)]
            void GoBack();
            [DispId(0x65)]
            void GoForward();
            [DispId(0x66)]
            void GoHome();
            [DispId(0x67)]
            void GoSearch();
            [DispId(0x68)]
            void Navigate([In] string Url, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
            [DispId(-550)]
            void Refresh();
            [DispId(0x69)]
            void Refresh2([In] ref object level);
            [DispId(0x6a)]
            void Stop();
            [DispId(200)]
            object Application { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xc9)]
            object Parent { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xca)]
            object Container { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xcb)]
            object Document { [return: MarshalAs(UnmanagedType.IDispatch)] get; }
            [DispId(0xcc)]
            bool TopLevelContainer { get; }
            [DispId(0xcd)]
            string Type { get; }
            [DispId(0xce)]
            int Left { get; set; }
            [DispId(0xcf)]
            int Top { get; set; }
            [DispId(0xd0)]
            int Width { get; set; }
            [DispId(0xd1)]
            int Height { get; set; }
            [DispId(210)]
            string LocationName { get; }
            [DispId(0xd3)]
            string LocationURL { get; }
            [DispId(0xd4)]
            bool Busy { get; }
            [DispId(300)]
            void Quit();
            [DispId(0x12d)]
            void ClientToWindow(out int pcx, out int pcy);
            [DispId(0x12e)]
            void PutProperty([In] string property, [In] object vtValue);
            [DispId(0x12f)]
            object GetProperty([In] string property);
            [DispId(0)]
            string Name { get; }
            [DispId(-515)]
            int HWND { get; }
            [DispId(400)]
            string FullName { get; }
            [DispId(0x191)]
            string Path { get; }
            [DispId(0x192)]
            bool Visible { get; set; }
            [DispId(0x193)]
            bool StatusBar { get; set; }
            [DispId(0x194)]
            string StatusText { get; set; }
            [DispId(0x195)]
            int ToolBar { get; set; }
            [DispId(0x196)]
            bool MenuBar { get; set; }
            [DispId(0x197)]
            bool FullScreen { get; set; }
            [DispId(500)]
            void Navigate2([In] ref object URL, [In] ref object flags, [In] ref object targetFrameName, [In] ref object postData, [In] ref object headers);
            [DispId(0x1f5)]
            System.Windows.Forms.NativeMethods.OLECMDF QueryStatusWB([In] System.Windows.Forms.NativeMethods.OLECMDID cmdID);
            [DispId(0x1f6)]
            void ExecWB([In] System.Windows.Forms.NativeMethods.OLECMDID cmdID, [In] System.Windows.Forms.NativeMethods.OLECMDEXECOPT cmdexecopt, ref object pvaIn, IntPtr pvaOut);
            [DispId(0x1f7)]
            void ShowBrowserBar([In] ref object pvaClsid, [In] ref object pvarShow, [In] ref object pvarSize);
            [DispId(-525)]
            WebBrowserReadyState ReadyState { get; }
            [DispId(550)]
            bool Offline { get; set; }
            [DispId(0x227)]
            bool Silent { get; set; }
            [DispId(0x228)]
            bool RegisterAsBrowser { get; set; }
            [DispId(0x229)]
            bool RegisterAsDropTarget { get; set; }
            [DispId(0x22a)]
            bool TheaterMode { get; set; }
            [DispId(0x22b)]
            bool AddressBar { get; set; }
            [DispId(0x22c)]
            bool Resizable { get; set; }
        }

        [StructLayout(LayoutKind.Sequential), SuppressUnmanagedCodeSecurity]
        public class OFNOTIFY
        {
            public IntPtr hdr_hwndFrom = IntPtr.Zero;
            public IntPtr hdr_idFrom = IntPtr.Zero;
            public int hdr_code;
            public IntPtr lpOFN = IntPtr.Zero;
            public IntPtr pszFile = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTSTRUCT
        {
            public int x;
            public int y;
            public POINTSTRUCT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential), SuppressUnmanagedCodeSecurity]
        internal class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
            private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            ~PROCESS_INFORMATION()
            {
                this.Close();
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void Close()
            {
                if ((this.hProcess != IntPtr.Zero) && (this.hProcess != INVALID_HANDLE_VALUE))
                {
                    CloseHandle(new HandleRef(this, this.hProcess));
                    this.hProcess = INVALID_HANDLE_VALUE;
                }
                if ((this.hThread != IntPtr.Zero) && (this.hThread != INVALID_HANDLE_VALUE))
                {
                    CloseHandle(new HandleRef(this, this.hThread));
                    this.hThread = INVALID_HANDLE_VALUE;
                }
            }

            [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
            private static extern bool CloseHandle(HandleRef handle);
        }

        [SuppressUnmanagedCodeSecurity]
        internal class Shell32
        {
            [DllImport("shell32.dll", CharSet=CharSet.Auto)]
            public static extern IntPtr SHBrowseForFolder([In] System.Windows.Forms.UnsafeNativeMethods.BROWSEINFO lpbi);
            [DllImport("shell32.dll")]
            public static extern int SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr pidl, out FileDialogNative.IShellItem ppsi);
            public static int SHGetFolderPathEx(ref Guid rfid, uint dwFlags, IntPtr hToken, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath, uint cchPath)
            {
                if (!System.Windows.Forms.UnsafeNativeMethods.IsVista)
                {
                    throw new NotSupportedException();
                }
                return SHGetFolderPathExPrivate(ref rfid, dwFlags, hToken, pszPath, cchPath);
            }

            [DllImport("shell32.dll", EntryPoint="SHGetFolderPathEx")]
            private static extern int SHGetFolderPathExPrivate(ref Guid rfid, uint dwFlags, IntPtr hToken, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath, uint cchPath);
            [DllImport("shell32.dll")]
            public static extern int SHGetMalloc([Out, MarshalAs(UnmanagedType.LPArray)] System.Windows.Forms.UnsafeNativeMethods.IMalloc[] ppMalloc);
            [DllImport("shell32.dll", CharSet=CharSet.Auto)]
            public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);
            [DllImport("shell32.dll")]
            public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
            [DllImport("shell32.dll")]
            public static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgflnOut);
        }

        [ComImport, Guid("000C060B-0000-0000-C000-000000000046")]
        public class SMsoComponentManager
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagQACONTAINER
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.UnsafeNativeMethods.tagQACONTAINER));
            public System.Windows.Forms.UnsafeNativeMethods.IOleClientSite pClientSite;
            [MarshalAs(UnmanagedType.Interface)]
            public object pAdviseSink;
            public System.Windows.Forms.UnsafeNativeMethods.IPropertyNotifySink pPropertyNotifySink;
            [MarshalAs(UnmanagedType.Interface)]
            public object pUnkEventSink;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAmbientFlags;
            [MarshalAs(UnmanagedType.U4)]
            public uint colorFore;
            [MarshalAs(UnmanagedType.U4)]
            public uint colorBack;
            [MarshalAs(UnmanagedType.Interface)]
            public object pFont;
            [MarshalAs(UnmanagedType.Interface)]
            public object pUndoMgr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAppearance;
            public int lcid;
            public IntPtr hpal = IntPtr.Zero;
            [MarshalAs(UnmanagedType.Interface)]
            public object pBindHost;
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class tagQACONTROL
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.UnsafeNativeMethods.tagQACONTROL));
            [MarshalAs(UnmanagedType.U4)]
            public int dwMiscStatus;
            [MarshalAs(UnmanagedType.U4)]
            public int dwViewStatus;
            [MarshalAs(UnmanagedType.U4)]
            public int dwEventCookie;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPropNotifyCookie;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPointerActivationPolicy;
        }

        [SuppressUnmanagedCodeSecurity]
        internal class ThemingScope
        {
            private const int ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 4;
            private const int ACTCTX_FLAG_RESOURCE_NAME_VALID = 8;
            private static bool contextCreationSucceeded;
            private static ACTCTX enableThemingActivationContext;
            private static IntPtr hActCtx;

            public static IntPtr Activate()
            {
                IntPtr zero = IntPtr.Zero;
                if (((Application.UseVisualStyles && contextCreationSucceeded) && (OSFeature.Feature.IsPresent(OSFeature.Themes) && !IsContextActive())) && !ActivateActCtx(hActCtx, out zero))
                {
                    zero = IntPtr.Zero;
                }
                return zero;
            }

            [DllImport("kernel32.dll")]
            private static extern bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);
            [DllImport("kernel32.dll")]
            private static extern IntPtr CreateActCtx(ref ACTCTX actctx);
            public static bool CreateActivationContext(string dllPath, int nativeResourceManifestID)
            {
                lock (typeof(System.Windows.Forms.UnsafeNativeMethods.ThemingScope))
                {
                    if (!contextCreationSucceeded && OSFeature.Feature.IsPresent(OSFeature.Themes))
                    {
                        enableThemingActivationContext = new ACTCTX();
                        enableThemingActivationContext.cbSize = Marshal.SizeOf(typeof(ACTCTX));
                        enableThemingActivationContext.lpSource = dllPath;
                        enableThemingActivationContext.lpResourceName = (IntPtr) nativeResourceManifestID;
                        enableThemingActivationContext.dwFlags = 8;
                        hActCtx = CreateActCtx(ref enableThemingActivationContext);
                        contextCreationSucceeded = hActCtx != new IntPtr(-1);
                    }
                    return contextCreationSucceeded;
                }
            }

            public static IntPtr Deactivate(IntPtr userCookie)
            {
                if (((userCookie != IntPtr.Zero) && OSFeature.Feature.IsPresent(OSFeature.Themes)) && DeactivateActCtx(0, userCookie))
                {
                    userCookie = IntPtr.Zero;
                }
                return userCookie;
            }

            [DllImport("kernel32.dll")]
            private static extern bool DeactivateActCtx(int dwFlags, IntPtr lpCookie);
            [DllImport("kernel32.dll")]
            private static extern bool GetCurrentActCtx(out IntPtr handle);
            private static bool IsContextActive()
            {
                IntPtr zero = IntPtr.Zero;
                return ((contextCreationSucceeded && GetCurrentActCtx(out zero)) && (zero == hActCtx));
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct ACTCTX
            {
                public int cbSize;
                public uint dwFlags;
                public string lpSource;
                public ushort wProcessorArchitecture;
                public ushort wLangId;
                public string lpAssemblyDirectory;
                public IntPtr lpResourceName;
                public string lpApplicationName;
            }
        }

        public class UnicodeCharBuffer : System.Windows.Forms.UnsafeNativeMethods.CharBuffer
        {
            internal char[] buffer;
            internal int offset;

            public UnicodeCharBuffer(int size)
            {
                this.buffer = new char[size];
            }

            public override IntPtr AllocCoTaskMem()
            {
                IntPtr destination = Marshal.AllocCoTaskMem(this.buffer.Length * 2);
                Marshal.Copy(this.buffer, 0, destination, this.buffer.Length);
                return destination;
            }

            public override string GetString()
            {
                int offset = this.offset;
                while ((offset < this.buffer.Length) && (this.buffer[offset] != '\0'))
                {
                    offset++;
                }
                string str = new string(this.buffer, this.offset, offset - this.offset);
                if (offset < this.buffer.Length)
                {
                    offset++;
                }
                this.offset = offset;
                return str;
            }

            public override void PutCoTaskMem(IntPtr ptr)
            {
                Marshal.Copy(ptr, this.buffer, 0, this.buffer.Length);
                this.offset = 0;
            }

            public override void PutString(string s)
            {
                int count = Math.Min(s.Length, this.buffer.Length - this.offset);
                s.CopyTo(0, this.buffer, this.offset, count);
                this.offset += count;
                if (this.offset < this.buffer.Length)
                {
                    this.buffer[this.offset++] = '\0';
                }
            }
        }
    }
}

