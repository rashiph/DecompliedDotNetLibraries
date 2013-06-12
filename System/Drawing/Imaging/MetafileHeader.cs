namespace System.Drawing.Imaging
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public sealed class MetafileHeader
    {
        internal MetafileHeaderWmf wmf;
        internal MetafileHeaderEmf emf;
        internal MetafileHeader()
        {
        }

        public MetafileType Type
        {
            get
            {
                if (!this.IsWmf())
                {
                    return this.emf.type;
                }
                return this.wmf.type;
            }
        }
        public int MetafileSize
        {
            get
            {
                if (!this.IsWmf())
                {
                    return this.emf.size;
                }
                return this.wmf.size;
            }
        }
        public int Version
        {
            get
            {
                if (!this.IsWmf())
                {
                    return this.emf.version;
                }
                return this.wmf.version;
            }
        }
        public float DpiX
        {
            get
            {
                if (!this.IsWmf())
                {
                    return this.emf.dpiX;
                }
                return this.wmf.dpiX;
            }
        }
        public float DpiY
        {
            get
            {
                if (!this.IsWmf())
                {
                    return this.emf.dpiY;
                }
                return this.wmf.dpiY;
            }
        }
        public Rectangle Bounds
        {
            get
            {
                if (!this.IsWmf())
                {
                    return new Rectangle(this.emf.X, this.emf.Y, this.emf.Width, this.emf.Height);
                }
                return new Rectangle(this.wmf.X, this.wmf.Y, this.wmf.Width, this.wmf.Height);
            }
        }
        public bool IsWmf()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            if ((this.wmf == null) || ((this.wmf.type != MetafileType.Wmf) && (this.wmf.type != MetafileType.WmfPlaceable)))
            {
                return false;
            }
            return true;
        }

        public bool IsWmfPlaceable()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.wmf != null) && (this.wmf.type == MetafileType.WmfPlaceable));
        }

        public bool IsEmf()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.emf != null) && (this.emf.type == MetafileType.Emf));
        }

        public bool IsEmfOrEmfPlus()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.emf != null) && (this.emf.type >= MetafileType.Emf));
        }

        public bool IsEmfPlus()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.emf != null) && (this.emf.type >= MetafileType.EmfPlusOnly));
        }

        public bool IsEmfPlusDual()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.emf != null) && (this.emf.type == MetafileType.EmfPlusDual));
        }

        public bool IsEmfPlusOnly()
        {
            if ((this.wmf == null) && (this.emf == null))
            {
                throw SafeNativeMethods.Gdip.StatusException(2);
            }
            return ((this.emf != null) && (this.emf.type == MetafileType.EmfPlusOnly));
        }

        public bool IsDisplay()
        {
            return (this.IsEmfPlus() && ((this.emf.emfPlusFlags & EmfPlusFlags.Display) != ((EmfPlusFlags) 0)));
        }

        public MetaHeader WmfHeader
        {
            get
            {
                if (this.wmf == null)
                {
                    throw SafeNativeMethods.Gdip.StatusException(2);
                }
                return this.wmf.WmfHeader;
            }
        }
        public int EmfPlusHeaderSize
        {
            get
            {
                if ((this.wmf == null) && (this.emf == null))
                {
                    throw SafeNativeMethods.Gdip.StatusException(2);
                }
                if (!this.IsWmf())
                {
                    return this.emf.EmfPlusHeaderSize;
                }
                return this.wmf.EmfPlusHeaderSize;
            }
        }
        public int LogicalDpiX
        {
            get
            {
                if ((this.wmf == null) && (this.emf == null))
                {
                    throw SafeNativeMethods.Gdip.StatusException(2);
                }
                if (!this.IsWmf())
                {
                    return this.emf.LogicalDpiX;
                }
                return this.wmf.LogicalDpiX;
            }
        }
        public int LogicalDpiY
        {
            get
            {
                if ((this.wmf == null) && (this.emf == null))
                {
                    throw SafeNativeMethods.Gdip.StatusException(2);
                }
                if (!this.IsWmf())
                {
                    return this.emf.LogicalDpiX;
                }
                return this.wmf.LogicalDpiY;
            }
        }
    }
}

