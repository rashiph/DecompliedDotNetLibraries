namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ShellExecuteHelper
    {
        private int _errorCode;
        private Microsoft.Win32.NativeMethods.ShellExecuteInfo _executeInfo;
        private bool _succeeded;

        public ShellExecuteHelper(Microsoft.Win32.NativeMethods.ShellExecuteInfo executeInfo)
        {
            this._executeInfo = executeInfo;
        }

        public void ShellExecuteFunction()
        {
            if (!(this._succeeded = Microsoft.Win32.NativeMethods.ShellExecuteEx(this._executeInfo)))
            {
                this._errorCode = Marshal.GetLastWin32Error();
            }
        }

        public bool ShellExecuteOnSTAThread()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                ThreadStart start = new ThreadStart(this.ShellExecuteFunction);
                Thread thread = new Thread(start);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            else
            {
                this.ShellExecuteFunction();
            }
            return this._succeeded;
        }

        public int ErrorCode
        {
            get
            {
                return this._errorCode;
            }
        }
    }
}

