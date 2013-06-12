namespace System.Runtime.InteropServices
{
    using System;
    using System.Collections.Generic;

    internal class GCHandleCookieTable
    {
        private const uint CookieMaskIndex = 0xffffff;
        private const uint CookieMaskSentinal = 0xff000000;
        private const int InitialHandleCount = 10;
        private byte[] m_CycleCounts = new byte[10];
        private int m_FreeIndex;
        private IntPtr[] m_HandleList = new IntPtr[10];
        private Dictionary<IntPtr, IntPtr> m_HandleToCookieMap = new Dictionary<IntPtr, IntPtr>(10);
        private object m_syncObject = new object();
        private const int MaxListSize = 0xffffff;

        internal GCHandleCookieTable()
        {
            for (int i = 0; i < 10; i++)
            {
                this.m_HandleList[i] = IntPtr.Zero;
                this.m_CycleCounts[i] = 0;
            }
        }

        internal IntPtr FindOrAddHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }
            IntPtr zero = IntPtr.Zero;
            lock (this.m_syncObject)
            {
                if (this.m_HandleToCookieMap.ContainsKey(handle))
                {
                    return this.m_HandleToCookieMap[handle];
                }
                if ((this.m_FreeIndex < this.m_HandleList.Length) && (this.m_HandleList[this.m_FreeIndex] == IntPtr.Zero))
                {
                    this.m_HandleList[this.m_FreeIndex] = handle;
                    zero = this.GetCookieFromData((uint) this.m_FreeIndex, this.m_CycleCounts[this.m_FreeIndex]);
                    this.m_FreeIndex++;
                }
                else
                {
                    this.m_FreeIndex = 0;
                    while (this.m_FreeIndex < 0xffffff)
                    {
                        if (this.m_HandleList[this.m_FreeIndex] == IntPtr.Zero)
                        {
                            this.m_HandleList[this.m_FreeIndex] = handle;
                            zero = this.GetCookieFromData((uint) this.m_FreeIndex, this.m_CycleCounts[this.m_FreeIndex]);
                            this.m_FreeIndex++;
                            break;
                        }
                        if ((this.m_FreeIndex + 1) == this.m_HandleList.Length)
                        {
                            this.GrowArrays();
                        }
                        this.m_FreeIndex++;
                    }
                }
                if (zero == IntPtr.Zero)
                {
                    throw new OutOfMemoryException(Environment.GetResourceString("OutOfMemory_GCHandleMDA"));
                }
                this.m_HandleToCookieMap.Add(handle, zero);
            }
            return zero;
        }

        private IntPtr GetCookieFromData(uint index, byte cycleCount)
        {
            byte num = (byte) (AppDomain.CurrentDomain.Id % 0xff);
            return (IntPtr) ((((cycleCount ^ num) << 0x18) + index) + ((ulong) 1L));
        }

        private void GetDataFromCookie(IntPtr cookie, out int index, out byte xorData)
        {
            uint num = (uint) ((int) cookie);
            index = (int) ((num & 0xffffff) - 1);
            xorData = (byte) ((num & -16777216) >> 0x18);
        }

        internal IntPtr GetHandle(IntPtr cookie)
        {
            if (!this.ValidateCookie(cookie))
            {
                return IntPtr.Zero;
            }
            return this.m_HandleList[this.GetIndexFromCookie(cookie)];
        }

        private int GetIndexFromCookie(IntPtr cookie)
        {
            uint num = (uint) ((int) cookie);
            return ((((int) num) & 0xffffff) - 1);
        }

        private void GrowArrays()
        {
            int length = this.m_HandleList.Length;
            IntPtr[] destinationArray = new IntPtr[length * 2];
            byte[] buffer = new byte[length * 2];
            Array.Copy(this.m_HandleList, destinationArray, length);
            Array.Copy(this.m_CycleCounts, buffer, length);
            this.m_HandleList = destinationArray;
            this.m_CycleCounts = buffer;
        }

        internal void RemoveHandleIfPresent(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                lock (this.m_syncObject)
                {
                    if (this.m_HandleToCookieMap.ContainsKey(handle))
                    {
                        IntPtr cookie = this.m_HandleToCookieMap[handle];
                        if (this.ValidateCookie(cookie))
                        {
                            int indexFromCookie = this.GetIndexFromCookie(cookie);
                            this.m_CycleCounts[indexFromCookie] = (byte) (this.m_CycleCounts[indexFromCookie] + 1);
                            this.m_HandleList[indexFromCookie] = IntPtr.Zero;
                            this.m_HandleToCookieMap.Remove(handle);
                            this.m_FreeIndex = indexFromCookie;
                        }
                    }
                }
            }
        }

        private bool ValidateCookie(IntPtr cookie)
        {
            int num;
            byte num2;
            this.GetDataFromCookie(cookie, out num, out num2);
            if (num >= 0xffffff)
            {
                return false;
            }
            if (num >= this.m_HandleList.Length)
            {
                return false;
            }
            if (this.m_HandleList[num] == IntPtr.Zero)
            {
                return false;
            }
            byte num3 = (byte) (AppDomain.CurrentDomain.Id % 0xff);
            byte num4 = (byte) (this.m_CycleCounts[num] ^ num3);
            if (num2 != num4)
            {
                return false;
            }
            return true;
        }
    }
}

