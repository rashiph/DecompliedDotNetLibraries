namespace System.Windows.Forms
{
    using System;

    public class PreviewKeyDownEventArgs : EventArgs
    {
        private bool _isInputKey;
        private readonly Keys _keyData;

        public PreviewKeyDownEventArgs(Keys keyData)
        {
            this._keyData = keyData;
        }

        public bool Alt
        {
            get
            {
                return ((this._keyData & Keys.Alt) == Keys.Alt);
            }
        }

        public bool Control
        {
            get
            {
                return ((this._keyData & Keys.Control) == Keys.Control);
            }
        }

        public bool IsInputKey
        {
            get
            {
                return this._isInputKey;
            }
            set
            {
                this._isInputKey = value;
            }
        }

        public Keys KeyCode
        {
            get
            {
                Keys keys = this._keyData & Keys.KeyCode;
                if (!Enum.IsDefined(typeof(Keys), (int) keys))
                {
                    return Keys.None;
                }
                return keys;
            }
        }

        public Keys KeyData
        {
            get
            {
                return this._keyData;
            }
        }

        public int KeyValue
        {
            get
            {
                return (((int) this._keyData) & 0xffff);
            }
        }

        public Keys Modifiers
        {
            get
            {
                return (this._keyData & ~Keys.KeyCode);
            }
        }

        public bool Shift
        {
            get
            {
                return ((this._keyData & Keys.Shift) == Keys.Shift);
            }
        }
    }
}

