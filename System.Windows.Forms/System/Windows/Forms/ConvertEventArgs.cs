namespace System.Windows.Forms
{
    using System;

    public class ConvertEventArgs : EventArgs
    {
        private System.Type desiredType;
        private object value;

        public ConvertEventArgs(object value, System.Type desiredType)
        {
            this.value = value;
            this.desiredType = desiredType;
        }

        public System.Type DesiredType
        {
            get
            {
                return this.desiredType;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

