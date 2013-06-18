namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms.Internal;

    internal sealed class WindowsGraphicsWrapper : IDisposable
    {
        private IDeviceContext idc;
        private System.Windows.Forms.Internal.WindowsGraphics wg;

        public WindowsGraphicsWrapper(IDeviceContext idc, TextFormatFlags flags)
        {
            if (idc is Graphics)
            {
                ApplyGraphicsProperties none = ApplyGraphicsProperties.None;
                if ((flags & TextFormatFlags.PreserveGraphicsClipping) != TextFormatFlags.Default)
                {
                    none |= ApplyGraphicsProperties.Clipping;
                }
                if ((flags & TextFormatFlags.PreserveGraphicsTranslateTransform) != TextFormatFlags.Default)
                {
                    none |= ApplyGraphicsProperties.TranslateTransform;
                }
                if (none != ApplyGraphicsProperties.None)
                {
                    this.wg = System.Windows.Forms.Internal.WindowsGraphics.FromGraphics(idc as Graphics, none);
                }
            }
            else
            {
                this.wg = idc as System.Windows.Forms.Internal.WindowsGraphics;
                if (this.wg != null)
                {
                    this.idc = idc;
                }
            }
            if (this.wg == null)
            {
                this.idc = idc;
                this.wg = System.Windows.Forms.Internal.WindowsGraphics.FromHdc(idc.GetHdc());
            }
            if ((flags & TextFormatFlags.LeftAndRightPadding) != TextFormatFlags.Default)
            {
                this.wg.TextPadding = TextPaddingOptions.LeftAndRightPadding;
            }
            else if ((flags & TextFormatFlags.NoPadding) != TextFormatFlags.Default)
            {
                this.wg.TextPadding = TextPaddingOptions.NoPadding;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (this.wg != null)
            {
                if (this.wg != this.idc)
                {
                    this.wg.Dispose();
                    if (this.idc != null)
                    {
                        this.idc.ReleaseHdc();
                    }
                }
                this.idc = null;
                this.wg = null;
            }
        }

        ~WindowsGraphicsWrapper()
        {
            this.Dispose(false);
        }

        public System.Windows.Forms.Internal.WindowsGraphics WindowsGraphics
        {
            get
            {
                return this.wg;
            }
        }
    }
}

