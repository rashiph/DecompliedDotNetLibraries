namespace System.Diagnostics
{
    using System;

    public class DataReceivedEventArgs : EventArgs
    {
        internal string _data;

        internal DataReceivedEventArgs(string data)
        {
            this._data = data;
        }

        public string Data
        {
            get
            {
                return this._data;
            }
        }
    }
}

