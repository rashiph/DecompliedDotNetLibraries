namespace System.Net
{
    using System;

    internal class BaseLoggingObject
    {
        internal BaseLoggingObject()
        {
        }

        internal virtual void Dump(byte[] buffer)
        {
        }

        internal virtual void Dump(byte[] buffer, int length)
        {
        }

        internal virtual void Dump(byte[] buffer, int offset, int length)
        {
        }

        internal virtual void Dump(IntPtr pBuffer, int offset, int length)
        {
        }

        internal virtual void DumpArray(bool shouldClose)
        {
        }

        internal virtual void DumpArrayToConsole()
        {
        }

        internal virtual void DumpArrayToFile(bool shouldClose)
        {
        }

        internal virtual void EnterFunc(string funcname)
        {
        }

        internal virtual void Flush()
        {
        }

        internal virtual void Flush(bool close)
        {
        }

        internal virtual void LeaveFunc(string funcname)
        {
        }

        internal virtual void LoggingMonitorTick()
        {
        }

        internal virtual void PrintLine(string msg)
        {
        }
    }
}

