namespace System.Net.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class OverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        internal WSABuffer m_SingleBuffer;
        private System.Net.SocketAddress m_SocketAddress;
        private System.Net.SocketAddress m_SocketAddressOriginal;
        internal WSABuffer[] m_WSABuffers;

        internal OverlappedAsyncResult(Socket socket, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
        }

        internal IntPtr GetSocketAddressPtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, 0);
        }

        internal IntPtr GetSocketAddressSizePtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, this.m_SocketAddress.GetAddressSizeOffset());
        }

        private void LogBuffer(int size)
        {
            if (size > -1)
            {
                if (this.m_WSABuffers != null)
                {
                    foreach (WSABuffer buffer in this.m_WSABuffers)
                    {
                        Logging.Dump(Logging.Sockets, base.AsyncObject, "PostCompletion", buffer.Pointer, Math.Min(buffer.Length, size));
                        if ((size -= buffer.Length) <= 0)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Logging.Dump(Logging.Sockets, base.AsyncObject, "PostCompletion", this.m_SingleBuffer.Pointer, Math.Min(this.m_SingleBuffer.Length, size));
                }
            }
        }

        internal override object PostCompletion(int numBytes)
        {
            if ((base.ErrorCode == 0) && Logging.On)
            {
                this.LogBuffer(numBytes);
            }
            return numBytes;
        }

        internal void SetUnmanagedStructures(BufferOffsetSize[] buffers)
        {
            this.m_WSABuffers = new WSABuffer[buffers.Length];
            object[] objectsToPin = new object[buffers.Length];
            for (int i = 0; i < buffers.Length; i++)
            {
                objectsToPin[i] = buffers[i].Buffer;
            }
            base.SetUnmanagedStructures(objectsToPin);
            for (int j = 0; j < buffers.Length; j++)
            {
                this.m_WSABuffers[j].Length = buffers[j].Size;
                this.m_WSABuffers[j].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffers[j].Buffer, buffers[j].Offset);
            }
        }

        internal void SetUnmanagedStructures(IList<ArraySegment<byte>> buffers)
        {
            int count = buffers.Count;
            ArraySegment<byte>[] segmentArray = new ArraySegment<byte>[count];
            for (int i = 0; i < count; i++)
            {
                segmentArray[i] = buffers[i];
                ValidationHelper.ValidateSegment(segmentArray[i]);
            }
            this.m_WSABuffers = new WSABuffer[count];
            object[] objectsToPin = new object[count];
            for (int j = 0; j < count; j++)
            {
                objectsToPin[j] = segmentArray[j].Array;
            }
            base.SetUnmanagedStructures(objectsToPin);
            for (int k = 0; k < count; k++)
            {
                this.m_WSABuffers[k].Length = segmentArray[k].Count;
                this.m_WSABuffers[k].Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(segmentArray[k].Array, segmentArray[k].Offset);
            }
        }

        internal void SetUnmanagedStructures(BufferOffsetSize[] buffers, ref OverlappedCache overlappedCache)
        {
            base.SetupCache(ref overlappedCache);
            this.SetUnmanagedStructures(buffers);
        }

        internal void SetUnmanagedStructures(IList<ArraySegment<byte>> buffers, ref OverlappedCache overlappedCache)
        {
            base.SetupCache(ref overlappedCache);
            this.SetUnmanagedStructures(buffers);
        }

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, System.Net.SocketAddress socketAddress, bool pinSocketAddress)
        {
            this.m_SocketAddress = socketAddress;
            if (pinSocketAddress && (this.m_SocketAddress != null))
            {
                object[] objectsToPin = null;
                objectsToPin = new object[2];
                objectsToPin[0] = buffer;
                this.m_SocketAddress.CopyAddressSizeIntoBuffer();
                objectsToPin[1] = this.m_SocketAddress.m_Buffer;
                base.SetUnmanagedStructures(objectsToPin);
            }
            else
            {
                base.SetUnmanagedStructures(buffer);
            }
            this.m_SingleBuffer.Length = size;
            this.m_SingleBuffer.Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
        }

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, System.Net.SocketAddress socketAddress, bool pinSocketAddress, ref OverlappedCache overlappedCache)
        {
            base.SetupCache(ref overlappedCache);
            this.SetUnmanagedStructures(buffer, offset, size, socketAddress, pinSocketAddress);
        }

        internal System.Net.SocketAddress SocketAddress
        {
            get
            {
                return this.m_SocketAddress;
            }
        }

        internal System.Net.SocketAddress SocketAddressOriginal
        {
            get
            {
                return this.m_SocketAddressOriginal;
            }
            set
            {
                this.m_SocketAddressOriginal = value;
            }
        }
    }
}

