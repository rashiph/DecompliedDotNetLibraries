namespace System
{
    [Serializable]
    public sealed class ConsoleCancelEventArgs : EventArgs
    {
        private bool _cancel;
        private ConsoleSpecialKey _type;

        internal ConsoleCancelEventArgs(ConsoleSpecialKey type)
        {
            this._type = type;
            this._cancel = false;
        }

        public bool Cancel
        {
            get
            {
                return this._cancel;
            }
            set
            {
                if ((this.SpecialKey == ConsoleSpecialKey.ControlBreak) && value)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_CantCancelCtrlBreak"));
                }
                this._cancel = value;
            }
        }

        public ConsoleSpecialKey SpecialKey
        {
            get
            {
                return this._type;
            }
        }
    }
}

