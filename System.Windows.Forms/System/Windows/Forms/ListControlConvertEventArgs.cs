namespace System.Windows.Forms
{
    using System;

    public class ListControlConvertEventArgs : ConvertEventArgs
    {
        private object listItem;

        public ListControlConvertEventArgs(object value, System.Type desiredType, object listItem) : base(value, desiredType)
        {
            this.listItem = listItem;
        }

        public object ListItem
        {
            get
            {
                return this.listItem;
            }
        }
    }
}

