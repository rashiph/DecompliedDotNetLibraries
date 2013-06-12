namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    internal static class AssertWrapper
    {
        public static void ShowAssert(string stackTrace, StackFrame frame, string message, string detailMessage)
        {
            ShowMessageBoxAssert(stackTrace, message, detailMessage);
        }

        [SecuritySafeCritical]
        private static void ShowMessageBoxAssert(string stackTrace, string message, string detailMessage)
        {
            string text = TruncateMessageToFitScreen(message + Environment.NewLine + detailMessage + Environment.NewLine + stackTrace);
            int type = 0x40212;
            if (!Environment.UserInteractive)
            {
                type |= 0x200000;
            }
            if (IsRTLResources)
            {
                type = (type | 0x80000) | 0x100000;
            }
            switch (Microsoft.Win32.SafeNativeMethods.MessageBox(IntPtr.Zero, text, SR.GetString("DebugAssertTitle"), type))
            {
                case 3:
                    Environment.Exit(1);
                    return;

                case 4:
                    if (!Debugger.IsAttached)
                    {
                        Debugger.Launch();
                    }
                    Debugger.Break();
                    return;
            }
        }

        [SecuritySafeCritical]
        private static string TruncateMessageToFitScreen(string message)
        {
            IntPtr stockObject = Microsoft.Win32.SafeNativeMethods.GetStockObject(0x11);
            IntPtr dC = Microsoft.Win32.UnsafeNativeMethods.GetDC(IntPtr.Zero);
            NativeMethods.TEXTMETRIC tm = new NativeMethods.TEXTMETRIC();
            stockObject = Microsoft.Win32.UnsafeNativeMethods.SelectObject(dC, stockObject);
            Microsoft.Win32.SafeNativeMethods.GetTextMetrics(dC, tm);
            Microsoft.Win32.UnsafeNativeMethods.SelectObject(dC, stockObject);
            Microsoft.Win32.UnsafeNativeMethods.ReleaseDC(IntPtr.Zero, dC);
            dC = IntPtr.Zero;
            int num2 = (Microsoft.Win32.UnsafeNativeMethods.GetSystemMetrics(1) / tm.tmHeight) - 15;
            int num3 = 0;
            int num4 = 0;
            int length = 0;
            while ((num3 < num2) && (length < (message.Length - 1)))
            {
                char ch = message[length];
                num4++;
                if (((ch == '\n') || (ch == '\r')) || (num4 > 80))
                {
                    num3++;
                    num4 = 0;
                }
                if ((ch == '\r') && (message[length + 1] == '\n'))
                {
                    length += 2;
                }
                else
                {
                    if ((ch == '\n') && (message[length + 1] == '\r'))
                    {
                        length += 2;
                        continue;
                    }
                    length++;
                }
            }
            if (length < (message.Length - 1))
            {
                message = SR.GetString("DebugMessageTruncated", new object[] { message.Substring(0, length) });
            }
            return message;
        }

        private static bool IsRTLResources
        {
            get
            {
                return (SR.GetString("RTL") != "RTL_False");
            }
        }
    }
}

