namespace System.Windows.Forms
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public static class ImeContext
    {
        private static IntPtr originalImeContext;

        public static void Disable(IntPtr handle)
        {
            if (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable)
            {
                if (IsOpen(handle))
                {
                    SetOpenStatus(false, handle);
                }
                IntPtr ptr = UnsafeNativeMethods.ImmAssociateContext(new HandleRef(null, handle), System.Windows.Forms.NativeMethods.NullHandleRef);
                if (ptr != IntPtr.Zero)
                {
                    originalImeContext = ptr;
                }
            }
        }

        public static void Enable(IntPtr handle)
        {
            if (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable)
            {
                IntPtr ptr = UnsafeNativeMethods.ImmGetContext(new HandleRef(null, handle));
                if (ptr == IntPtr.Zero)
                {
                    if (originalImeContext == IntPtr.Zero)
                    {
                        ptr = UnsafeNativeMethods.ImmCreateContext();
                        if (ptr != IntPtr.Zero)
                        {
                            UnsafeNativeMethods.ImmAssociateContext(new HandleRef(null, handle), new HandleRef(null, ptr));
                        }
                    }
                    else
                    {
                        UnsafeNativeMethods.ImmAssociateContext(new HandleRef(null, handle), new HandleRef(null, originalImeContext));
                    }
                }
                else
                {
                    UnsafeNativeMethods.ImmReleaseContext(new HandleRef(null, handle), new HandleRef(null, ptr));
                }
                if (!IsOpen(handle))
                {
                    SetOpenStatus(true, handle);
                }
            }
        }

        public static ImeMode GetImeMode(IntPtr handle)
        {
            IntPtr zero = IntPtr.Zero;
            ImeMode noControl = ImeMode.NoControl;
            ImeMode[] inputLanguageTable = ImeModeConversion.InputLanguageTable;
            if (inputLanguageTable == ImeModeConversion.UnsupportedTable)
            {
                noControl = ImeMode.Inherit;
            }
            else
            {
                zero = UnsafeNativeMethods.ImmGetContext(new HandleRef(null, handle));
                if (zero == IntPtr.Zero)
                {
                    noControl = ImeMode.Disable;
                }
                else if (!IsOpen(handle))
                {
                    noControl = inputLanguageTable[3];
                }
                else
                {
                    int conversion = 0;
                    int sentence = 0;
                    UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(null, zero), ref conversion, ref sentence);
                    if ((conversion & 1) != 0)
                    {
                        if ((conversion & 2) != 0)
                        {
                            noControl = ((conversion & 8) != 0) ? inputLanguageTable[6] : inputLanguageTable[7];
                        }
                        else
                        {
                            noControl = ((conversion & 8) != 0) ? inputLanguageTable[4] : inputLanguageTable[5];
                        }
                    }
                    else
                    {
                        noControl = ((conversion & 8) != 0) ? inputLanguageTable[8] : inputLanguageTable[9];
                    }
                }
            }
            if (zero != IntPtr.Zero)
            {
                UnsafeNativeMethods.ImmReleaseContext(new HandleRef(null, handle), new HandleRef(null, zero));
            }
            return noControl;
        }

        public static bool IsOpen(IntPtr handle)
        {
            IntPtr ptr = UnsafeNativeMethods.ImmGetContext(new HandleRef(null, handle));
            bool flag = false;
            if (ptr != IntPtr.Zero)
            {
                flag = UnsafeNativeMethods.ImmGetOpenStatus(new HandleRef(null, ptr));
                UnsafeNativeMethods.ImmReleaseContext(new HandleRef(null, handle), new HandleRef(null, ptr));
            }
            return flag;
        }

        public static void SetImeStatus(ImeMode imeMode, IntPtr handle)
        {
            if (imeMode == ImeMode.Inherit)
            {
                return;
            }
            if (imeMode == ImeMode.NoControl)
            {
                return;
            }
            ImeMode[] inputLanguageTable = ImeModeConversion.InputLanguageTable;
            if (inputLanguageTable == ImeModeConversion.UnsupportedTable)
            {
                return;
            }
            int num = 0;
            int sentence = 0;
            if (imeMode == ImeMode.Disable)
            {
                Disable(handle);
            }
            else
            {
                Enable(handle);
            }
            switch (imeMode)
            {
                case ImeMode.NoControl:
                case ImeMode.Disable:
                    return;

                case ImeMode.On:
                    imeMode = ImeMode.Hiragana;
                    goto Label_0079;

                case ImeMode.Off:
                    if (inputLanguageTable == ImeModeConversion.JapaneseTable)
                    {
                        break;
                    }
                    imeMode = ImeMode.Alpha;
                    goto Label_0079;

                case ImeMode.Close:
                    break;

                default:
                    goto Label_0079;
            }
            if (inputLanguageTable == ImeModeConversion.KoreanTable)
            {
                imeMode = ImeMode.Alpha;
            }
            else
            {
                SetOpenStatus(false, handle);
                return;
            }
        Label_0079:
            if (ImeModeConversion.ImeModeConversionBits.ContainsKey(imeMode))
            {
                ImeModeConversion conversion = ImeModeConversion.ImeModeConversionBits[imeMode];
                IntPtr ptr = UnsafeNativeMethods.ImmGetContext(new HandleRef(null, handle));
                UnsafeNativeMethods.ImmGetConversionStatus(new HandleRef(null, ptr), ref num, ref sentence);
                num |= conversion.setBits;
                num &= ~conversion.clearBits;
                UnsafeNativeMethods.ImmSetConversionStatus(new HandleRef(null, ptr), num, sentence);
                UnsafeNativeMethods.ImmReleaseContext(new HandleRef(null, handle), new HandleRef(null, ptr));
            }
        }

        public static void SetOpenStatus(bool open, IntPtr handle)
        {
            if (ImeModeConversion.InputLanguageTable != ImeModeConversion.UnsupportedTable)
            {
                IntPtr ptr = UnsafeNativeMethods.ImmGetContext(new HandleRef(null, handle));
                if ((ptr != IntPtr.Zero) && UnsafeNativeMethods.ImmSetOpenStatus(new HandleRef(null, ptr), open))
                {
                    bool flag = UnsafeNativeMethods.ImmReleaseContext(new HandleRef(null, handle), new HandleRef(null, ptr));
                }
            }
        }

        [Conditional("DEBUG")]
        private static void TraceImeStatus(IntPtr handle)
        {
        }

        [Conditional("DEBUG")]
        internal static void TraceImeStatus(Control ctl)
        {
        }
    }
}

