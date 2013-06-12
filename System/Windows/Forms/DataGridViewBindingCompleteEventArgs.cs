namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;

    public class DataGridViewBindingCompleteEventArgs : EventArgs
    {
        private System.ComponentModel.ListChangedType listChangedType;

        public DataGridViewBindingCompleteEventArgs(System.ComponentModel.ListChangedType listChangedType)
        {
            this.listChangedType = listChangedType;
        }

        public System.ComponentModel.ListChangedType ListChangedType
        {
            get
            {
                return this.listChangedType;
            }
        }
    }
}

