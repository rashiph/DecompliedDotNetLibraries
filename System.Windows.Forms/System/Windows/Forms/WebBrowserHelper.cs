namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal static class WebBrowserHelper
    {
        internal static readonly int addedSelectionHandler = BitVector32.CreateMask(setClientSiteFirst);
        internal static Guid comctlImageCombo_Clsid = new Guid("{a98a24c0-b06f-3684-8c12-c52ae341e0bc}");
        private const int HMperInch = 0x9ec;
        private static Guid ifont_Guid = typeof(System.Windows.Forms.UnsafeNativeMethods.IFont).GUID;
        internal static readonly int inTransition = BitVector32.CreateMask(siteProcessedInputKey);
        internal static readonly int isMaskEdit = BitVector32.CreateMask(processingKeyUp);
        private static int logPixelsX = -1;
        private static int logPixelsY = -1;
        internal static readonly int manualUpdate = BitVector32.CreateMask(sinkAttached);
        internal static Guid maskEdit_Clsid = new Guid("{c932ba85-4374-101b-a56c-00aa003668dc}");
        internal static readonly int processingKeyUp = BitVector32.CreateMask(inTransition);
        internal static readonly int recomputeContainingControl = BitVector32.CreateMask(isMaskEdit);
        internal static readonly int REGMSG_MSG = System.Windows.Forms.SafeNativeMethods.RegisterWindowMessage(Application.WindowMessagesVersion + "_subclassCheck");
        internal const int REGMSG_RETVAL = 0x7b;
        internal static readonly int setClientSiteFirst = BitVector32.CreateMask(manualUpdate);
        internal static readonly int sinkAttached = BitVector32.CreateMask();
        internal static readonly int siteProcessedInputKey = BitVector32.CreateMask(addedSelectionHandler);
        internal static Guid windowsMediaPlayer_Clsid = new Guid("{22d6f312-b0f6-11d0-94ab-0080c74c7e95}");

        internal static System.Windows.Forms.NativeMethods.COMRECT GetClipRect()
        {
            return new System.Windows.Forms.NativeMethods.COMRECT(new Rectangle(0, 0, 0x7d00, 0x7d00));
        }

        internal static ISelectionService GetSelectionService(Control ctl)
        {
            ISite site = ctl.Site;
            if (site != null)
            {
                object service = site.GetService(typeof(ISelectionService));
                if (service is ISelectionService)
                {
                    return (ISelectionService) service;
                }
            }
            return null;
        }

        internal static int HM2Pix(int hm, int logP)
        {
            return (((logP * hm) + 0x4f6) / 0x9ec);
        }

        internal static int Pix2HM(int pix, int logP)
        {
            return (((0x9ec * pix) + (logP >> 1)) / logP);
        }

        internal static void ResetLogPixelsX()
        {
            logPixelsX = -1;
        }

        internal static void ResetLogPixelsY()
        {
            logPixelsY = -1;
        }

        internal static int LogPixelsX
        {
            get
            {
                if (logPixelsX == -1)
                {
                    IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
                    if (dC != IntPtr.Zero)
                    {
                        logPixelsX = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 0x58);
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
                    }
                }
                return logPixelsX;
            }
        }

        internal static int LogPixelsY
        {
            get
            {
                if (logPixelsY == -1)
                {
                    IntPtr dC = System.Windows.Forms.UnsafeNativeMethods.GetDC(System.Windows.Forms.NativeMethods.NullHandleRef);
                    if (dC != IntPtr.Zero)
                    {
                        logPixelsY = System.Windows.Forms.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
                        System.Windows.Forms.UnsafeNativeMethods.ReleaseDC(System.Windows.Forms.NativeMethods.NullHandleRef, new HandleRef(null, dC));
                    }
                }
                return logPixelsY;
            }
        }

        internal enum AXEditMode
        {
            None,
            Object,
            Host
        }

        internal enum AXState
        {
            InPlaceActive = 4,
            Loaded = 1,
            Passive = 0,
            Running = 2,
            UIActive = 8
        }

        internal enum SelectionStyle
        {
            NotSelected,
            Selected,
            Active
        }
    }
}

