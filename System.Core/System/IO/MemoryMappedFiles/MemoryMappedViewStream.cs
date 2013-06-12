namespace System.IO.MemoryMappedFiles
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    public sealed class MemoryMappedViewStream : UnmanagedMemoryStream
    {
        private MemoryMappedView m_view;

        [SecurityCritical]
        internal MemoryMappedViewStream(MemoryMappedView view)
        {
            this.m_view = view;
            base.Initialize(this.m_view.ViewHandle, this.m_view.PointerOffset, this.m_view.Size, MemoryMappedFile.GetFileAccess(this.m_view.Access));
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && (this.m_view != null)) && !this.m_view.IsClosed)
                {
                    this.Flush();
                }
            }
            finally
            {
                try
                {
                    if (this.m_view != null)
                    {
                        this.m_view.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        [SecurityCritical]
        public override void Flush()
        {
            if (!this.CanSeek)
            {
                System.IO.__Error.StreamIsClosed();
            }
            if (this.m_view != null)
            {
                this.m_view.Flush((IntPtr) base.Capacity);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(System.SR.GetString("NotSupported_MMViewStreamsFixedLength"));
        }

        public Microsoft.Win32.SafeHandles.SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                if (this.m_view == null)
                {
                    return null;
                }
                return this.m_view.ViewHandle;
            }
        }
    }
}

