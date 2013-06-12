namespace System.Web.UI
{
    using System;

    public sealed class StateItem
    {
        private bool isDirty;
        private object value;

        internal StateItem(object initialValue)
        {
            this.value = initialValue;
            this.isDirty = false;
        }

        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }
            set
            {
                this.isDirty = value;
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

