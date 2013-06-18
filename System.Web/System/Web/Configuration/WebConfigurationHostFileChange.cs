namespace System.Web.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Web;

    internal sealed class WebConfigurationHostFileChange
    {
        private StreamChangeCallback _callback;

        internal WebConfigurationHostFileChange(StreamChangeCallback callback)
        {
            this._callback = callback;
        }

        internal void OnFileChanged(object sender, FileChangeEvent e)
        {
            this._callback(e.FileName);
        }

        internal StreamChangeCallback Callback
        {
            get
            {
                return this._callback;
            }
        }
    }
}

