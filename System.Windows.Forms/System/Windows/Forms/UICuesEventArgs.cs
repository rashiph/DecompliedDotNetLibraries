namespace System.Windows.Forms
{
    using System;

    public class UICuesEventArgs : EventArgs
    {
        private readonly UICues uicues;

        public UICuesEventArgs(UICues uicues)
        {
            this.uicues = uicues;
        }

        public UICues Changed
        {
            get
            {
                return (this.uicues & UICues.Changed);
            }
        }

        public bool ChangeFocus
        {
            get
            {
                return ((this.uicues & UICues.ChangeFocus) != UICues.None);
            }
        }

        public bool ChangeKeyboard
        {
            get
            {
                return ((this.uicues & UICues.ChangeKeyboard) != UICues.None);
            }
        }

        public bool ShowFocus
        {
            get
            {
                return ((this.uicues & UICues.ShowFocus) != UICues.None);
            }
        }

        public bool ShowKeyboard
        {
            get
            {
                return ((this.uicues & UICues.ShowKeyboard) != UICues.None);
            }
        }
    }
}

