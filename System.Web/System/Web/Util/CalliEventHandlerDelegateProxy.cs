namespace System.Web.Util
{
    using System;

    internal class CalliEventHandlerDelegateProxy
    {
        private bool _argless;
        private IntPtr _functionPointer;
        private object _target;

        internal CalliEventHandlerDelegateProxy(object target, IntPtr functionPointer, bool argless)
        {
            this._argless = argless;
            this._target = target;
            this._functionPointer = functionPointer;
        }

        internal void Callback(object sender, EventArgs e)
        {
            if (this._argless)
            {
                CalliHelper.ArglessFunctionCaller(this._functionPointer, this._target);
            }
            else
            {
                CalliHelper.EventArgFunctionCaller(this._functionPointer, this._target, sender, e);
            }
        }

        internal EventHandler Handler
        {
            get
            {
                return new EventHandler(this.Callback);
            }
        }
    }
}

