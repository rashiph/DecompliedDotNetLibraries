namespace System.Web.UI.WebControls
{
    using System;

    public class ServerValidateEventArgs : EventArgs
    {
        private bool isValid;
        private string value;

        public ServerValidateEventArgs(string value, bool isValid)
        {
            this.isValid = isValid;
            this.value = value;
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            set
            {
                this.isValid = value;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

