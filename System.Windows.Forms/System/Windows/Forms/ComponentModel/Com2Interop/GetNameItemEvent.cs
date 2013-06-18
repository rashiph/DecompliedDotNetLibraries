namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;

    internal class GetNameItemEvent : EventArgs
    {
        private object nameItem;

        public GetNameItemEvent(object defName)
        {
            this.nameItem = defName;
        }

        public object Name
        {
            get
            {
                return this.nameItem;
            }
            set
            {
                this.nameItem = value;
            }
        }

        public string NameString
        {
            get
            {
                if (this.nameItem != null)
                {
                    return this.nameItem.ToString();
                }
                return "";
            }
        }
    }
}

