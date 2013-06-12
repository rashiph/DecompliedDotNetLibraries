namespace System.Net.Sockets
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class TransmitFileOverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        private System.Net.TransmitFileBuffers m_buffers;
        private FileStream m_fileStream;
        private TransmitFileOptions m_flags;

        internal TransmitFileOverlappedAsyncResult(Socket socket) : base(socket)
        {
        }

        internal TransmitFileOverlappedAsyncResult(Socket socket, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
        }

        protected override void ForceReleaseUnmanagedStructures()
        {
            if (this.m_fileStream != null)
            {
                this.m_fileStream.Close();
                this.m_fileStream = null;
            }
            base.ForceReleaseUnmanagedStructures();
        }

        internal void SetUnmanagedStructures(byte[] preBuffer, byte[] postBuffer, FileStream fileStream, TransmitFileOptions flags, bool sync)
        {
            this.m_fileStream = fileStream;
            this.m_flags = flags;
            this.m_buffers = null;
            int num = 0;
            if ((preBuffer != null) && (preBuffer.Length > 0))
            {
                num++;
            }
            if ((postBuffer != null) && (postBuffer.Length > 0))
            {
                num++;
            }
            object[] objectsToPin = null;
            if (num != 0)
            {
                num++;
                objectsToPin = new object[num];
                this.m_buffers = new System.Net.TransmitFileBuffers();
                objectsToPin[--num] = this.m_buffers;
                if ((preBuffer != null) && (preBuffer.Length > 0))
                {
                    this.m_buffers.preBufferLength = preBuffer.Length;
                    objectsToPin[--num] = preBuffer;
                }
                if ((postBuffer != null) && (postBuffer.Length > 0))
                {
                    this.m_buffers.postBufferLength = postBuffer.Length;
                    objectsToPin[--num] = postBuffer;
                }
                if (sync)
                {
                    base.PinUnmanagedObjects(objectsToPin);
                }
                else
                {
                    base.SetUnmanagedStructures(objectsToPin);
                }
                if ((preBuffer != null) && (preBuffer.Length > 0))
                {
                    this.m_buffers.preBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(preBuffer, 0);
                }
                if ((postBuffer != null) && (postBuffer.Length > 0))
                {
                    this.m_buffers.postBuffer = Marshal.UnsafeAddrOfPinnedArrayElement(postBuffer, 0);
                }
            }
            else if (!sync)
            {
                base.SetUnmanagedStructures(null);
            }
        }

        internal void SetUnmanagedStructures(byte[] preBuffer, byte[] postBuffer, FileStream fileStream, TransmitFileOptions flags, ref OverlappedCache overlappedCache)
        {
            base.SetupCache(ref overlappedCache);
            this.SetUnmanagedStructures(preBuffer, postBuffer, fileStream, flags, false);
        }

        internal void SyncReleaseUnmanagedStructures()
        {
            this.ForceReleaseUnmanagedStructures();
        }

        internal TransmitFileOptions Flags
        {
            get
            {
                return this.m_flags;
            }
        }

        internal System.Net.TransmitFileBuffers TransmitFileBuffers
        {
            get
            {
                return this.m_buffers;
            }
        }
    }
}

